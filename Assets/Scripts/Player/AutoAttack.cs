using SurvivorMaster.Enemy;
using SurvivorMaster.Systems;
using UnityEngine;

namespace SurvivorMaster.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public sealed class AutoAttack : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float attackRange = 25f;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileLifetime = 2f;
        [SerializeField] private int projectilePierce = 0;

        private PlayerStats _stats;
        private float _cooldownTimer;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (EnemyManager.Instance == null || ProjectileManager.Instance == null)
            {
                return;
            }

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f)
            {
                return;
            }

            Enemy.Enemy target = EnemyManager.Instance.FindNearestEnemy(firePoint.position, attackRange);
            if (target == null)
            {
                return;
            }

            Vector3 direction = target.transform.position - firePoint.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
            }

            ProjectileManager.Instance.SpawnProjectile(
                firePoint.position + Vector3.up * 0.75f,
                direction.normalized,
                _stats.Damage,
                projectileSpeed,
                projectileLifetime,
                projectilePierce);

            _cooldownTimer = 1f / Mathf.Max(0.1f, _stats.AttackSpeed);
        }
    }
}
