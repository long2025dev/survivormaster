using SurvivorMaster.Player;
using SurvivorMaster.Systems;
using UnityEngine;

namespace SurvivorMaster.Enemy
{
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(Health))]
    public sealed class Enemy : MonoBehaviour
    {
        private EnemyAI _ai;
        private Health _health;
        private EnemyManager _owner;

        public int ActiveIndex { get; set; } = -1;
        public int CellX { get; set; }
        public int CellY { get; set; }
        public float LastTickTime { get; set; }
        public bool IsActive { get; private set; }

        public float XpReward { get; private set; }

        private void Awake()
        {
            _ai = GetComponent<EnemyAI>();
            _health = GetComponent<Health>();
            _health.Died += OnDied;
        }

        public void Activate(
            EnemyManager owner,
            Vector3 position,
            float maxHp,
            float speedMultiplier,
            float damageMultiplier,
            float xpReward)
        {
            _owner = owner;
            XpReward = Mathf.Max(1f, xpReward);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            _health.ResetHealth(maxHp);
            _ai.Configure(speedMultiplier, damageMultiplier);
            LastTickTime = Time.time;
            IsActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsActive = false;
            ActiveIndex = -1;
            gameObject.SetActive(false);
        }

        public void Tick(float dt, Vector3 playerPosition, PlayerStats playerStats)
        {
            _ai.Tick(dt, transform, playerPosition, playerStats);
        }

        public void ApplyDamage(float damage)
        {
            _health.ApplyDamage(damage);
        }

        private void OnDied(Health _)
        {
            if (IsActive)
            {
                _owner.DespawnEnemy(this, true);
            }
        }
    }
}
