using System;

namespace SteelLance.Combat
{
    /// <summary>
    /// Salvage quality tier from qualityHP ratio. See docs/design/unity/サルベージ設計.md §5.1.
    /// </summary>
    public enum SalvageGrade
    {
        None,
        Scrap,
        Low,
        Mid,
        High
    }

    /// <summary>
    /// Mission end state for salvage payout (R8). Not used in Phase2 combat.
    /// </summary>
    public enum MissionOutcome
    {
        Success,
        Retreat,
        Defeat
    }

    /// <summary>
    /// v0.2 とどめ方式 — deprecated v0.3.2. Do not use in new code.
    /// </summary>
    [Obsolete("SalvageImpact is deprecated. Use WeaponProfile.powerFactor / qualityDrainFactor.")]
    public enum SalvageImpact
    {
        Capture,
        Destroy,
        Sever
    }
}
