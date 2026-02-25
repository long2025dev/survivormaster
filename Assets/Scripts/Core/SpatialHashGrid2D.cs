using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorMaster.Core
{
    // Lightweight XZ spatial partition grid for nearest/radius queries.
    public sealed class SpatialHashGrid2D<T> where T : class
    {
        private readonly Dictionary<int, List<T>> _cells;
        private readonly float _cellSize;
        private readonly float _inverseCellSize;

        public SpatialHashGrid2D(float cellSize, int initialCellCapacity = 256)
        {
            _cellSize = Mathf.Max(0.1f, cellSize);
            _inverseCellSize = 1f / _cellSize;
            _cells = new Dictionary<int, List<T>>(initialCellCapacity);
        }

        public int ToCell(float world) => Mathf.FloorToInt(world * _inverseCellSize);

        public int ToHash(int x, int y)
        {
            unchecked
            {
                return (x * 73856093) ^ (y * 19349663);
            }
        }

        public void Add(T item, int x, int y)
        {
            int key = ToHash(x, y);
            if (!_cells.TryGetValue(key, out List<T> list))
            {
                list = new List<T>(16);
                _cells.Add(key, list);
            }

            list.Add(item);
        }

        public void Remove(T item, int x, int y)
        {
            int key = ToHash(x, y);
            if (!_cells.TryGetValue(key, out List<T> list))
            {
                return;
            }

            int index = list.IndexOf(item);
            if (index >= 0)
            {
                int last = list.Count - 1;
                list[index] = list[last];
                list.RemoveAt(last);
            }

            if (list.Count == 0)
            {
                _cells.Remove(key);
            }
        }

        public void Move(T item, int oldX, int oldY, int newX, int newY)
        {
            if (oldX == newX && oldY == newY)
            {
                return;
            }

            Remove(item, oldX, oldY);
            Add(item, newX, newY);
        }

        public int Query(Vector2 center, float radius, T[] buffer, Func<T, Vector2> getPosition)
        {
            int count = 0;
            float radiusSqr = radius * radius;

            int minX = ToCell(center.x - radius);
            int maxX = ToCell(center.x + radius);
            int minY = ToCell(center.y - radius);
            int maxY = ToCell(center.y + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int key = ToHash(x, y);
                    if (!_cells.TryGetValue(key, out List<T> list))
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        T item = list[i];
                        Vector2 pos = getPosition(item);
                        if ((pos - center).sqrMagnitude > radiusSqr)
                        {
                            continue;
                        }

                        if (count >= buffer.Length)
                        {
                            return count;
                        }

                        buffer[count++] = item;
                    }
                }
            }

            return count;
        }
    }
}
