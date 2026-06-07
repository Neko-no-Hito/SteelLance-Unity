using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// Dual-HP damage split helpers. Phase2B: stub signatures only.
    /// See docs/design/unity/部位破壊設計.md §5.2b.
    /// </summary>
    public static class DamagePoolSplitter
    {
        private const float StubPowerFactor = 1f;
        private const float StubQualityDrainFactor = 1f;

        public readonly struct PoolDamage
        {
            public readonly float DestructionDamage;
            public readonly float QualityDamage;

            public PoolDamage(float destructionDamage, float qualityDamage)
            {
                DestructionDamage = destructionDamage;
                QualityDamage = qualityDamage;
            }
        }

        public static PoolDamage SplitDamageToPools(float baseDamage, WeaponProfile weapon, bool isVenting)
        {
            var power = weapon != null ? weapon.powerFactor : StubPowerFactor;
            var qualityDrain = weapon != null ? weapon.qualityDrainFactor : StubQualityDrainFactor;

            if (isVenting)
            {
                qualityDrain = 0f;
            }

            return new PoolDamage(baseDamage * power, baseDamage * qualityDrain);
        }

        public static float ClampArmorBreakOverflow(float damage, float armorRemaining)
        {
            return Mathf.Min(damage, armorRemaining);
        }

        public static void ApplyVentDamage(
            BodyPartHealth part,
            float baseDamage,
            WeaponProfile weapon,
            float ventMultiplier)
        {
            if (part == null)
            {
                return;
            }

            var scaled = baseDamage * ventMultiplier;
            var pools = SplitDamageToPools(scaled, weapon, isVenting: true);
            ApplyPoolDamage(part, pools);
        }

        public static void ApplyPoolDamage(BodyPartHealth part, PoolDamage pools)
        {
            if (part == null || part.Condition == PartCondition.Destroyed)
            {
                return;
            }

            part.ApplyPoolDamageInternal(pools);
        }
    }
}
