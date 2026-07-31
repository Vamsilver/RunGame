using UnityEngine;
using UnityEngine.InputSystem;

namespace RunGame.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 7f;
        [SerializeField, Min(0f)] private float acceleration = 28f;
        [SerializeField, Min(0f)] private float rotationSpeed = 14f;

        private Rigidbody body;
        private Vector2 moveInput;

        public Vector3 HorizontalVelocity => new(body.linearVelocity.x, 0f, body.linearVelocity.z);
        public bool IsMoving => HorizontalVelocity.sqrMagnitude > 0.1f;

        private void Awake() => body = GetComponent<Rigidbody>();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = Vector2.zero;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private void FixedUpdate()
        {
            Vector3 desiredVelocity = new(moveInput.x * moveSpeed, body.linearVelocity.y, moveInput.y * moveSpeed);
            body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

            Vector3 direction = new(moveInput.x, 0f, moveInput.y);
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            }
        }
    }
}
