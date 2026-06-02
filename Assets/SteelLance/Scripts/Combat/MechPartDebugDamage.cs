using SteelLance.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SteelLance.Combat
{
    /// <summary>
    /// Phase2 テスト用。テンキーで部位にダメージ（U6-F 確認）。
    /// テンキー配置 = 機体正面イメージ:
    ///   [7 ShoulderL] [8 Head] [9 ShoulderR]
    ///   [4 ArmL]      [5 Torso] [6 ArmR]
    ///                 [2 Legs]
    /// </summary>
    public class MechPartDebugDamage : MonoBehaviour
    {
        private const float DefaultDamagePerPress = 20f;

        [SerializeField] private MechPartSystem partSystem;
        [SerializeField] private float damagePerPress = DefaultDamagePerPress;
        [SerializeField] private bool enableDebugKeys = true;

        private void Awake()
        {
            if (partSystem == null)
            {
                partSystem = GetComponent<MechPartSystem>();
            }
        }

        private void Update()
        {
            if (!enableDebugKeys || partSystem == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.numpad8Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.Head);
            }
            else if (keyboard.numpad5Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.Torso);
            }
            else if (keyboard.numpad4Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.ArmL);
            }
            else if (keyboard.numpad6Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.ArmR);
            }
            else if (keyboard.numpad7Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.ShoulderL);
            }
            else if (keyboard.numpad9Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.ShoulderR);
            }
            else if (keyboard.numpad2Key.wasPressedThisFrame)
            {
                ApplyDebugDamage(BodyRegion.Legs);
            }
        }

        private void ApplyDebugDamage(BodyRegion region)
        {
            var part = partSystem.GetPart(region);
            if (part == null || !part.IsAlive)
            {
                Debug.LogWarning($"[SteelLance] Debug damage: no alive part for {region}");
                return;
            }

            var context = new DamageContext
            {
                attacker = transform,
                hitRegion = region
            };
            part.TakeDamage(damagePerPress, in context);
            Debug.Log(
                $"[SteelLance] Debug damage {region}: {part.CurrentHP:0}/{part.MaxHP:0} ({part.Condition})");
        }
    }
}
