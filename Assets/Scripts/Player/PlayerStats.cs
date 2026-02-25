using System;
using SurvivorMaster.Systems;
using UnityEngine;

namespace SurvivorMaster.Player
{
    public sealed class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float damage = 12f;
        [SerializeField] private float attackSpeed = 1.2f;

        public event Action<float, float> HealthChanged;
        public event Action StatsChanged;

        public float MaxHp => maxHp;
        public float CurrentHp { get; private set; }
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;
        public float AttackSpeed => attackSpeed;

        private void Awake()
        {
            CurrentHp = maxHp;
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || CurrentHp <= 0f)
            {
                return;
            }

            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            HealthChanged?.Invoke(CurrentHp, maxHp);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
            HealthChanged?.Invoke(CurrentHp, maxHp);
        }

        public void SetMaxHp(float value, bool healToFull = true)
        {
            maxHp = Mathf.Max(1f, value);
            if (healToFull)
            {
                CurrentHp = maxHp;
            }
            else
            {
                CurrentHp = Mathf.Min(CurrentHp, maxHp);
            }

            HealthChanged?.Invoke(CurrentHp, maxHp);
            StatsChanged?.Invoke();
        }

        public void ApplyUpgrade(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            switch (definition.UpgradeType)
            {
                case UpgradeType.Damage:
                    damage += definition.Value;
                    break;
                case UpgradeType.AttackSpeed:
                    attackSpeed = Mathf.Max(0.1f, attackSpeed + definition.Value);
                    break;
                case UpgradeType.MoveSpeed:
                    moveSpeed = Mathf.Max(0.1f, moveSpeed + definition.Value);
                    break;
                case UpgradeType.MaxHp:
                    SetMaxHp(maxHp + definition.Value);
                    return;
            }

            StatsChanged?.Invoke();
        }
    }
}
