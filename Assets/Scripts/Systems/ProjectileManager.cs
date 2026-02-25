using System.Collections.Generic;
using SurvivorMaster.Core;
using SurvivorMaster.Enemy;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class ProjectileManager : MonoBehaviour
    {
        public static ProjectileManager Instance { get; private set; }

        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private int prewarmCount = 260;
        [SerializeField] private float hitRadius = 0.5f;

        private ObjectPool<Projectile> _pool;
        private readonly List<Projectile> _active = new List<Projectile>(512);
        private Enemy.Enemy[] _hits;

        public int ActiveCount => _active.Count;
        public int PoolTotalCount => _pool != null ? _pool.TotalCount : 0;
        public int PoolInactiveCount => _pool != null ? _pool.InactiveCount : 0;

        private void Awake()
        {
            Instance = this;
            if (projectilePrefab == null)
            {
                projectilePrefab = CreateRuntimeFallbackPrefab();
                Debug.LogWarning("ProjectileManager: projectilePrefab was null. Using runtime fallback projectile prefab.");
            }

            _hits = new Enemy.Enemy[64];
            _pool = new ObjectPool<Projectile>(prewarmCount, CreateProjectile, OnGetProjectile, OnReleaseProjectile);
            _pool.Prewarm(prewarmCount);
        }

        private Projectile CreateProjectile()
        {
            Projectile p = Instantiate(projectilePrefab, transform);
            p.gameObject.SetActive(false);
            return p;
        }

        private Projectile CreateRuntimeFallbackPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "Projectile_RuntimeFallback";
            root.transform.SetParent(transform, false);
            root.transform.localScale = Vector3.one * 0.25f;

            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateFallbackProjectileMaterial();
            }

            Projectile projectile = root.AddComponent<Projectile>();
            root.SetActive(false);
            return projectile;
        }

        private Material CreateFallbackProjectileMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = new Color(1f, 0.86f, 0.2f, 1f);
            material.enableInstancing = true;
            return material;
        }

        private static void OnGetProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private static void OnReleaseProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_active.Count == 0 || EnemyManager.Instance == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Projectile p = _active[i];
                if (!p.IsActive)
                {
                    DespawnInternal(i);
                    continue;
                }

                p.Lifetime -= dt;
                if (p.Lifetime <= 0f)
                {
                    DespawnInternal(i);
                    continue;
                }

                p.transform.position += p.Direction * (p.Speed * dt);

                int hitCount = EnemyManager.Instance.QueryEnemiesInRadius(p.transform.position, hitRadius, _hits);
                Enemy.Enemy candidate = null;
                float best = float.MaxValue;

                for (int h = 0; h < hitCount; h++)
                {
                    Enemy.Enemy enemy = _hits[h];
                    if (enemy == null || !enemy.IsActive || enemy == p.LastHitEnemy)
                    {
                        continue;
                    }

                    float sqr = (enemy.transform.position - p.transform.position).sqrMagnitude;
                    if (sqr < best)
                    {
                        best = sqr;
                        candidate = enemy;
                    }
                }

                if (candidate == null)
                {
                    continue;
                }

                candidate.ApplyDamage(p.Damage);
                p.LastHitEnemy = candidate;
                p.RemainingHits--;

                if (p.RemainingHits <= 0)
                {
                    DespawnInternal(i);
                }
            }
        }

        public void SpawnProjectile(Vector3 position, Vector3 direction, float damage, float speed, float lifetime, int pierce)
        {
            Projectile projectile = _pool.Get();
            projectile.Activate(position, direction, damage, speed, lifetime, pierce);
            projectile.ActiveIndex = _active.Count;
            _active.Add(projectile);
        }

        private void DespawnInternal(int index)
        {
            int lastIndex = _active.Count - 1;
            Projectile p = _active[index];
            Projectile last = _active[lastIndex];

            _active[index] = last;
            _active.RemoveAt(lastIndex);

            if (index < _active.Count)
            {
                last.ActiveIndex = index;
            }

            p.Deactivate();
            _pool.Release(p);
        }
    }
}
