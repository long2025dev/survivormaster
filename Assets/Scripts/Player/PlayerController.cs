using SurvivorMaster.UI;
using UnityEngine;

namespace SurvivorMaster.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick virtualJoystick;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 22f;
        [SerializeField] private float gravity = -20f;

        private CharacterController _controller;
        private PlayerStats _stats;
        private Vector3 _velocity;

        public PlayerStats Stats => _stats;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            Vector2 moveInput = ReadMoveInput();
            Vector3 desiredPlanar = new Vector3(moveInput.x, 0f, moveInput.y) * _stats.MoveSpeed;

            float accel = desiredPlanar.sqrMagnitude > 0.001f ? acceleration : deceleration;
            _velocity.x = Mathf.MoveTowards(_velocity.x, desiredPlanar.x, accel * Time.deltaTime);
            _velocity.z = Mathf.MoveTowards(_velocity.z, desiredPlanar.z, accel * Time.deltaTime);
            _velocity.y += gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime);

            if (_controller.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -1f;
            }

            if (desiredPlanar.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredPlanar.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 18f * Time.deltaTime);
            }
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 joy = virtualJoystick != null ? virtualJoystick.Input : Vector2.zero;
            if (joy.sqrMagnitude > 0.01f)
            {
                return Vector2.ClampMagnitude(joy, 1f);
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 keyboard = new Vector2(x, y);
            if (keyboard.sqrMagnitude > 1f)
            {
                keyboard.Normalize();
            }

            return keyboard;
        }
    }
}
