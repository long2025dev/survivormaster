using System.Collections.Generic;
using SurvivorMaster.Core;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class XPOrbManager : MonoBehaviour
    {
        public static XPOrbManager Instance { get; private set; }

        [SerializeField] private XPOrb xpOrbPrefab;
        [SerializeField] private int prewarmCount = 1200;

        private ObjectPool<XPOrb> _pool;
        private readonly List<XPOrb> _active = new List<XPOrb>(2048);
        private Transform _playerTransform;

        public int ActiveCount => _active.Count;
        public int PoolTotalCount => _pool != null ? _pool.TotalCount : 0;
        public int PoolInactiveCount => _pool != null ? _pool.InactiveCount : 0;

        private void Awake()
        {
            Instance = this;
            if (xpOrbPrefab == null)
            {
                xpOrbPrefab = CreateRuntimeFallbackPrefab();
                Debug.LogWarning("XPOrbManager: xpOrbPrefab was null. Using runtime fallback XP orb prefab.");
            }

            _pool = new ObjectPool<XPOrb>(prewarmCount, CreateOrb, OnGetOrb, OnReleaseOrb);
            _pool.Prewarm(prewarmCount);
        }

        public void SetPlayer(Transform player)
        {
            _playerTransform = player;
        }

        private XPOrb CreateOrb()
        {
            XPOrb orb = Instantiate(xpOrbPrefab, transform);
            orb.gameObject.SetActive(false);
            return orb;
        }

        private XPOrb CreateRuntimeFallbackPrefab()
        {
            GameObject root = new GameObject("XPOrb_RuntimeFallback");
            root.transform.SetParent(transform, false);
            XPOrb orb = root.AddComponent<XPOrb>();
            root.SetActive(false);
            return orb;
        }

        private static void OnGetOrb(XPOrb orb)
        {
            orb.gameObject.SetActive(true);
        }

        private static void OnReleaseOrb(XPOrb orb)
        {
            orb.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_playerTransform == null || _active.Count == 0 || XPManager.Instance == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            Vector3 playerPos = _playerTransform.position;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                XPOrb orb = _active[i];
                if (!orb.IsActive)
                {
                    DespawnInternal(i);
                    continue;
                }

                Vector3 delta = playerPos - orb.transform.position;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;

                if (sqr <= orb.PickupRadius * orb.PickupRadius)
                {
                    XPManager.Instance.AddXP(orb.XpValue);
                    DespawnInternal(i);
                    continue;
                }

                if (sqr <= orb.AttractionRadius * orb.AttractionRadius)
                {
                    Vector3 dir = delta.normalized;
                    orb.transform.position += dir * (orb.MoveSpeed * dt);
                }
            }
        }

        public void Spawn(Vector3 position, float xpValue)
        {
            XPOrb orb = _pool.Get();
            orb.Activate(position, xpValue);
            orb.ActiveIndex = _active.Count;
            _active.Add(orb);
        }

        private void DespawnInternal(int index)
        {
            int lastIndex = _active.Count - 1;
            XPOrb orb = _active[index];
            XPOrb last = _active[lastIndex];

            _active[index] = last;
            _active.RemoveAt(lastIndex);

            if (index < _active.Count)
            {
                last.ActiveIndex = index;
            }

            orb.Deactivate();
            _pool.Release(orb);
        }
    }
}
