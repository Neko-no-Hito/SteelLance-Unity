using UnityEngine;
using UnityEngine.InputSystem;

namespace SteelLance.Mech
{
    /// <summary>
    /// 三人称カメラ。ターゲットの後方から追従し、マウスで視点回転。
    /// Main Camera にアタッチ。
    /// </summary>
    public class MechThirdPersonCamera : MonoBehaviour
    {
        private const float DefaultDistance = 6f;
        private const float DefaultHeight = 2.5f;
        private const float DefaultMouseSensitivity = 0.15f;
        private const float MinPitch = -20f;
        private const float MaxPitch = 50f;

        [SerializeField] private Transform target;
        [SerializeField] private float distance = DefaultDistance;
        [SerializeField] private float height = DefaultHeight;
        [SerializeField] private float mouseSensitivity = DefaultMouseSensitivity;

        private float _yaw;
        private float _pitch = 15f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            ApplyMouseLook();

            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var offset = rotation * new Vector3(0f, height, -distance);
            var focus = target.position + Vector3.up * 1.5f;

            transform.position = focus + offset;
            transform.rotation = rotation;
        }

        private void ApplyMouseLook()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var delta = mouse.delta.ReadValue() * mouseSensitivity;
            _yaw += delta.x;
            _pitch -= delta.y;
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
