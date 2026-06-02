using SteelLance.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SteelLance.Mech
{
    /// <summary>
    /// 三人称メカの移動。WASD はカメラ基準。CharacterController 必須。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class MechController : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 8f;
        private const float DefaultRotationSpeed = 720f;
        private const float Gravity = -20f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = DefaultMoveSpeed;
        [SerializeField] private float rotationSpeed = DefaultRotationSpeed;

        private CharacterController _controller;
        private MechPartSystem _partSystem;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _partSystem = GetComponent<MechPartSystem>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var input = ReadMoveInput(keyboard);
            var move = GetCameraRelativeMove(input);

            var turnRate = rotationSpeed * (_partSystem != null ? _partSystem.TurnRateMultiplier : 1f);
            if (move.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnRate * Time.deltaTime);
            }

            ApplyMovement(move);
        }

        private static Vector2 ReadMoveInput(Keyboard keyboard)
        {
            var input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return input;
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            var forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            var move = forward * input.y + right * input.x;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            return move;
        }

        private void ApplyMovement(Vector3 move)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += Gravity * Time.deltaTime;

            var speed = moveSpeed * (_partSystem != null ? _partSystem.MoveSpeedMultiplier : 1f);
            var velocity = move * speed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
