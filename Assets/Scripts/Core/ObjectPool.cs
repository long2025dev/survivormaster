using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorMaster.Core
{
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly Stack<T> _inactive;
        private readonly List<T> _all;
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        public int ActiveCount => _all.Count - _inactive.Count;
        public int InactiveCount => _inactive.Count;
        public int TotalCount => _all.Count;

        public ObjectPool(int initialCapacity, Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null)
        {
            _inactive = new Stack<T>(initialCapacity);
            _all = new List<T>(initialCapacity);
            _factory = factory;
            _onGet = onGet;
            _onRelease = onRelease;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = _factory();
                _all.Add(instance);
                _onRelease?.Invoke(instance);
                _inactive.Push(instance);
            }
        }

        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateOne();
            _onGet?.Invoke(instance);
            return instance;
        }

        public void Release(T instance)
        {
            _onRelease?.Invoke(instance);
            _inactive.Push(instance);
        }

        private T CreateOne()
        {
            T instance = _factory();
            _all.Add(instance);
            return instance;
        }
    }
}
