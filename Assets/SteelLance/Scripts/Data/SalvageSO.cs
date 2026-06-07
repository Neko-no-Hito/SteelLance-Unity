using UnityEngine;

namespace SteelLance.Data
{
    /// <summary>
    /// Salvage tier thresholds and rates (values 🔴 Play). Stub until 中型敵 Phase.
    /// See docs/design/unity/サルベージ設計.md §4.2.
    /// </summary>
    [CreateAssetMenu(fileName = "SalvageConfig_Default", menuName = "SteelLance/Salvage Config")]
    public class SalvageSO : ScriptableObject
    {
        [Header("品質 tier 閾値（qualityRatio · 🔴 Play）")]
        public float tierHighThreshold = 0.70f;
        public float tierMidThreshold = 0.30f;

        [Header("tier別 回収率（🔴 Play — v0.2 % は deprecated）")]
        public float rateHigh;
        public float rateMid;
        public float rateLow;
        public float rateScrap;
        public float rateNone;

        [Header("H5 · 排熱")]
        public bool ventBonusEnabled = true;
    }
}
