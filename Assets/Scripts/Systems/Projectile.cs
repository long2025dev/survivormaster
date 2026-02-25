using SurvivorMaster.Enemy;
using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class Projectile : MonoBehaviour
    {
        public int ActiveIndex { get; set; } = -1;
        public bool IsActive { get; private set; }
        public float Damage { get; private set; }
        public float Speed { get; private set; }
        public float Lifetime { get; set; }
        public int RemainingHits { get; set; }
        public Enemy.Enemy LastHitEnemy { get; set; }
        public Vector3 Direction { get; private set; }

        public void Activate(Vector3 position, Vector3 direction, float damage, float speed, float lifetime, int pierce)
        {
            transform.position = position;
            transform.forward = direction;
            Direction = direction;
            Damage = damage;
            Speed = speed;
            Lifetime = lifetime;
            RemainingHits = Mathf.Max(1, pierce + 1);
            LastHitEnemy = null;
            IsActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsActive = false;
            ActiveIndex = -1;
            gameObject.SetActive(false);
        }
    }
}
