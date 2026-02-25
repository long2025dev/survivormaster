using System.Collections.Generic;
using SurvivorMaster.Core;
using SurvivorMaster.Enemy;
using SurvivorMaster.Player;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        [Header("Pool")]
        [SerializeField] private Enemy.Enemy enemyPrefab;
        [SerializeField] private int prewarmCount = 600;
        [SerializeField] private int maxActiveEnemies = 500;

        [Header("Ticking")]
        [SerializeField] private int ticksPerFrame = 180;
        [SerializeField] private float gridCellSize = 2.2f;

        [Header("Base Stats")]
        [SerializeField] private float baseEnemyHp = 25f;
        [SerializeField] private float baseEnemySpeedMultiplier = 1f;
        [SerializeField] private float baseEnemyDamageMultiplier = 1f;
        [SerializeField] private float baseXpReward = 1f;

        private ObjectPool<Enemy.Enemy> _pool;
        private readonly List<Enemy.Enemy> _active = new List<Enemy.Enemy>(1024);
        private SpatialHashGrid2D<Enemy.Enemy> _grid;
        private Enemy.Enemy[] _queryBuffer;
        private int _tickCursor;

        private Transform _playerTransform;
        private PlayerStats _playerStats;

        public int ActiveCount => _active.Count;
        public int PoolTotalCount => _pool != null ? _pool.TotalCount : 0;
        public int PoolInactiveCount => _pool != null ? _pool.InactiveCount : 0;

        private void Awake()
        {
            Instance = this;
            if (enemyPrefab == null)
            {
                enemyPrefab = CreateRuntimeFallbackPrefab();
                Debug.LogWarning("EnemyManager: enemyPrefab was null. Using runtime fallback enemy prefab.");
            }

            _queryBuffer = new Enemy.Enemy[256];
            _grid = new SpatialHashGrid2D<Enemy.Enemy>(gridCellSize);
            _pool = new ObjectPool<Enemy.Enemy>(prewarmCount, CreateEnemy, OnGetEnemy, OnReleaseEnemy);
            _pool.Prewarm(prewarmCount);
        }

        public void RegisterPlayer(PlayerController player)
        {
            if (player == null)
            {
                _playerTransform = null;
                _playerStats = null;
                return;
            }

            _playerTransform = player.transform;
            _playerStats = player.Stats;
        }

        private Enemy.Enemy CreateEnemy()
        {
            Enemy.Enemy instance = Instantiate(enemyPrefab, transform);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private Enemy.Enemy CreateRuntimeFallbackPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Enemy_RuntimeFallback";
            root.transform.SetParent(transform, false);
            root.transform.localScale = new Vector3(1f, 1f, 1f);

            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateFallbackEnemyMaterial();
            }

            root.AddComponent<EnemyAI>();
            root.AddComponent<Health>();
            Enemy.Enemy enemy = root.AddComponent<Enemy.Enemy>();
            root.SetActive(false);
            return enemy;
        }

        private Material CreateFallbackEnemyMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = new Color(0.92f, 0.28f, 0.28f, 1f);
            material.enableInstancing = true;
            return material;
        }

        private static void OnGetEnemy(Enemy.Enemy enemy)
        {
            enemy.gameObject.SetActive(true);
        }

        private static void OnReleaseEnemy(Enemy.Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_playerTransform == null || _playerStats == null || _active.Count == 0)
            {
                return;
            }

            int toProcess = Mathf.Min(_active.Count, Mathf.Max(1, ticksPerFrame));
            int processed = 0;
            float now = Time.time;

            while (processed < toProcess && _active.Count > 0)
            {
                if (_tickCursor >= _active.Count)
                {
                    _tickCursor = 0;
                }

                Enemy.Enemy enemy = _active[_tickCursor];
                if (!enemy.IsActive)
                {
                    RemoveActiveAt(_tickCursor);
                    continue;
                }

                Vector3 before = enemy.transform.position;
                float dt = Mathf.Max(0.001f, now - enemy.LastTickTime);
                enemy.LastTickTime = now;
                enemy.Tick(dt, _playerTransform.position, _playerStats);
                UpdateEnemyGridCell(enemy, before, enemy.transform.position);

                _tickCursor++;
                processed++;
            }
        }

        public bool SpawnEnemy(Vector3 position, float hpMultiplier = 1f, float speedMultiplier = 1f, float damageMultiplier = 1f, float xpRewardMultiplier = 1f)
        {
            if (_active.Count >= maxActiveEnemies)
            {
                return false;
            }

            Enemy.Enemy enemy = _pool.Get();
            enemy.Activate(
                this,
                position,
                baseEnemyHp * Mathf.Max(0.1f, hpMultiplier),
                baseEnemySpeedMultiplier * speedMultiplier,
                baseEnemyDamageMultiplier * damageMultiplier,
                baseXpReward * xpRewardMultiplier);

            enemy.ActiveIndex = _active.Count;
            _active.Add(enemy);

            int cellX = _grid.ToCell(position.x);
            int cellY = _grid.ToCell(position.z);
            enemy.CellX = cellX;
            enemy.CellY = cellY;
            _grid.Add(enemy, cellX, cellY);

            return true;
        }

        public void DespawnEnemy(Enemy.Enemy enemy, bool awardXp)
        {
            if (enemy == null || !enemy.IsActive)
            {
                return;
            }

            _grid.Remove(enemy, enemy.CellX, enemy.CellY);

            if (enemy.ActiveIndex >= 0 && enemy.ActiveIndex < _active.Count)
            {
                RemoveActiveAt(enemy.ActiveIndex);
            }

            Vector3 deathPosition = enemy.transform.position;
            float xpReward = enemy.XpReward;
            enemy.Deactivate();
            _pool.Release(enemy);

            if (awardXp && XPOrbManager.Instance != null)
            {
                XPOrbManager.Instance.Spawn(deathPosition, xpReward);
            }
        }

        private void RemoveActiveAt(int index)
        {
            int lastIndex = _active.Count - 1;
            Enemy.Enemy removed = _active[index];
            Enemy.Enemy last = _active[lastIndex];
            _active[index] = last;
            _active.RemoveAt(lastIndex);

            if (index < _active.Count)
            {
                last.ActiveIndex = index;
            }

            removed.ActiveIndex = -1;

            if (_tickCursor > index)
            {
                _tickCursor--;
            }

            if (_tickCursor >= _active.Count)
            {
                _tickCursor = 0;
            }
        }

        private void UpdateEnemyGridCell(Enemy.Enemy enemy, Vector3 before, Vector3 after)
        {
            int oldX = enemy.CellX;
            int oldY = enemy.CellY;
            int newX = _grid.ToCell(after.x);
            int newY = _grid.ToCell(after.z);

            if (oldX == newX && oldY == newY)
            {
                return;
            }

            _grid.Move(enemy, oldX, oldY, newX, newY);
            enemy.CellX = newX;
            enemy.CellY = newY;
        }

        public Enemy.Enemy FindNearestEnemy(Vector3 position, float range)
        {
            Vector2 center = new Vector2(position.x, position.z);
            int count = _grid.Query(center, range, _queryBuffer, GetEnemyPosition);
            float best = float.MaxValue;
            Enemy.Enemy nearest = null;

            for (int i = 0; i < count; i++)
            {
                Enemy.Enemy enemy = _queryBuffer[i];
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                Vector3 diff = enemy.transform.position - position;
                diff.y = 0f;
                float sqr = diff.sqrMagnitude;
                if (sqr < best)
                {
                    best = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        public int QueryEnemiesInRadius(Vector3 position, float radius, Enemy.Enemy[] buffer)
        {
            Vector2 center = new Vector2(position.x, position.z);
            return _grid.Query(center, radius, buffer, GetEnemyPosition);
        }

        private static Vector2 GetEnemyPosition(Enemy.Enemy enemy)
        {
            Vector3 p = enemy.transform.position;
            return new Vector2(p.x, p.z);
        }
    }
}
