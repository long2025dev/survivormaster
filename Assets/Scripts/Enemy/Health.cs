using System;
using UnityEngine;

namespace SurvivorMaster.Enemy
{
    public sealed class Health : MonoBehaviour
    {
        public event Action<Health> Died;

        public float MaxHp { get; private set; }
        public float CurrentHp { get; private set; }

        public void ResetHealth(float maxHp)
        {
            MaxHp = Mathf.Max(1f, maxHp);
            CurrentHp = MaxHp;
        }

        public void ApplyDamage(float value)
        {
            if (value <= 0f || CurrentHp <= 0f)
            {
                return;
            }

            CurrentHp -= value;
            if (CurrentHp <= 0f)
            {
                CurrentHp = 0f;
                Died?.Invoke(this);
            }
        }
    }
}
