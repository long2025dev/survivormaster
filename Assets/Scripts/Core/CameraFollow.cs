using UnityEngine;

namespace SurvivorMaster.Core
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 20f, -14f);
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 _velocity;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
