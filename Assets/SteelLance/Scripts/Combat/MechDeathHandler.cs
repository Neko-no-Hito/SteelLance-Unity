using SteelLance.Data;
using SteelLance.Mech;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// プレイヤー撃破時に操作を止める。Phase2: MechPartSystem 優先。
    /// </summary>
    public class MechDeathHandler : MonoBehaviour
    {
        private MechPartSystem _partSystem;
        private Health _health;
        private MechController _controller;
        private MechWeapon[] _weapons;

        private void Awake()
        {
            _partSystem = GetComponent<MechPartSystem>();
            _health = GetComponent<Health>();
            _controller = GetComponent<MechController>();
            _weapons = GetComponentsInChildren<MechWeapon>(true);
        }

        private void OnEnable()
        {
            if (_partSystem != null)
            {
                _partSystem.OnMechDefeated += OnMechDefeated;
            }

            if (_health != null)
            {
                _health.Died += OnHealthDied;
            }
        }

        private void OnDisable()
        {
            if (_partSystem != null)
            {
                _partSystem.OnMechDefeated -= OnMechDefeated;
            }

            if (_health != null)
            {
                _health.Died -= OnHealthDied;
            }
        }

        private void OnMechDefeated(MechDefeatReason reason)
        {
            StopControl();
            Debug.Log($"[SteelLance] GAME OVER — {reason}");
        }

        private void OnHealthDied(Health _)
        {
            StopControl();
        }

        private void StopControl()
        {
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            foreach (var weapon in _weapons)
            {
                if (weapon != null)
                {
                    weapon.enabled = false;
                }
            }
        }
    }
}
