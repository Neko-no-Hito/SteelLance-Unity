using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// Salvage grade resolution — stub until 中型敵 Phase.
    /// See docs/design/unity/サルベージ設計.md §5.2.
    /// </summary>
    public static class SalvageResolver
    {
        // Phase: 中型敵 — GetRate / TryResolve (no % rolls in Phase2B).

        public static SalvageGrade ResolveGrade(float qualityRatio, SalvageSO cfg)
        {
            if (cfg == null)
            {
                return ResolveGradeWithDefaults(qualityRatio);
            }

            if (qualityRatio >= cfg.tierHighThreshold)
            {
                return SalvageGrade.High;
            }

            if (qualityRatio >= cfg.tierMidThreshold)
            {
                return SalvageGrade.Mid;
            }

            return SalvageGrade.Low;
        }

        public static SalvageGrade ApplyVentPrecisionBonus(
            SalvageGrade grade,
            bool targetWasVenting,
            SalvageSO cfg)
        {
            // Phase: 中型敵 — H5 bump (cfg.VentBonusBump). Phase2B: no-op.
            if (cfg != null && !cfg.ventBonusEnabled)
            {
                return grade;
            }

            return grade;
        }

        public static SalvageGrade ResolveSurvivor(PartCondition cond)
        {
            return cond == PartCondition.Damaged ? SalvageGrade.Scrap : SalvageGrade.None;
        }

        /// <summary>
        /// Phase2B: log-only hook when a part crosses to Destroyed.
        /// </summary>
        public static SalvageGrade ResolveGradeForDestroyedPart(
            BodyPartHealth part,
            SalvageSO cfg,
            bool targetWasVenting = false)
        {
            if (part == null || !part.HasQualityPool)
            {
                return SalvageGrade.None;
            }

            var qualityRatio = part.QualityHPMax > 0f ? part.QualityHP / part.QualityHPMax : 0f;
            var grade = ResolveGrade(qualityRatio, cfg);
            grade = ApplyVentPrecisionBonus(grade, targetWasVenting, cfg);
            Debug.Log(
                $"[SteelLance] Salvage grade {part.Region}: {grade} (qualityRatio={qualityRatio:0.##})");
            return grade;
        }

        private static SalvageGrade ResolveGradeWithDefaults(float qualityRatio)
        {
            const float defaultHigh = 0.70f;
            const float defaultMid = 0.30f;

            if (qualityRatio >= defaultHigh)
            {
                return SalvageGrade.High;
            }

            if (qualityRatio >= defaultMid)
            {
                return SalvageGrade.Mid;
            }

            return SalvageGrade.Low;
        }
    }
}
