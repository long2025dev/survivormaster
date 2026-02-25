using UnityEngine;

namespace SurvivorMaster.Systems
{
    public sealed class XPOrb : MonoBehaviour
    {
        [SerializeField] private float attractionRadius = 7f;
        [SerializeField] private float pickupRadius = 1f;
        [SerializeField] private float moveSpeed = 14f;

        public int ActiveIndex { get; set; } = -1;
        public bool IsActive { get; private set; }
        public float XpValue { get; private set; }

        public float AttractionRadius => attractionRadius;
        public float PickupRadius => pickupRadius;
        public float MoveSpeed => moveSpeed;

        public void Activate(Vector3 position, float xp)
        {
            transform.position = position;
            XpValue = xp;
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
