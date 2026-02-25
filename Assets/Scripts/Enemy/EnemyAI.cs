using SurvivorMaster.Player;
using UnityEngine;

namespace SurvivorMaster.Enemy
{
    public sealed class EnemyAI : MonoBehaviour
    {
        [SerializeField] private float baseMoveSpeed = 2.5f;
        [SerializeField] private float baseContactDamage = 5f;
        [SerializeField] private float contactDamageCooldown = 0.8f;
        [SerializeField] private float contactRange = 0.9f;

        private float _moveSpeed;
        private float _contactDamage;
        private float _cooldownRemaining;

        public void Configure(float speedMultiplier, float damageMultiplier)
        {
            _moveSpeed = baseMoveSpeed * Mathf.Max(0.1f, speedMultiplier);
            _contactDamage = baseContactDamage * Mathf.Max(0.1f, damageMultiplier);
            _cooldownRemaining = 0f;
        }

        public void Tick(float dt, Transform enemyTransform, Vector3 playerPosition, PlayerStats playerStats)
        {
            if (dt <= 0f)
            {
                return;
            }

            Vector3 delta = playerPosition - enemyTransform.position;
            delta.y = 0f;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance > 0.0001f)
            {
                Vector3 dir = delta / Mathf.Sqrt(sqrDistance);
                enemyTransform.position += dir * (_moveSpeed * dt);
                enemyTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            _cooldownRemaining -= dt;
            float rangeSqr = contactRange * contactRange;
            if (_cooldownRemaining <= 0f && sqrDistance <= rangeSqr)
            {
                playerStats.ApplyDamage(_contactDamage);
                _cooldownRemaining = contactDamageCooldown;
            }
        }
    }
}
