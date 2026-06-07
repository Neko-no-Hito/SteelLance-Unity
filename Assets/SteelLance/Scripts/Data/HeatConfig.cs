using UnityEngine;

namespace SteelLance.Data
{
    /// <summary>
    /// Heat tuning keys (values 🔴 Play). Not referenced by combat until Phase4+.
    /// See docs/design/unity/熱管理設計.md §5.
    /// </summary>
    [CreateAssetMenu(fileName = "HeatConfig_Default", menuName = "SteelLance/Heat Config")]
    public class HeatConfig : ScriptableObject
    {
        public float maxHeat;
        public float passiveVentRate;
        public float forcedVentThreshold;
        public float forcedVentMinHeatDrop;
        public float ventAcceleratedRate;
        public float ventMoveMultiplier = 1f;
        public float ventMoveSpeedEnemy;
        public float ventDamageMultiplier = 1f;
        public float ventSalvagePrecisionBonus;
        public float torsoDamagedHeatGainMultiplier = 1f;
        public float torsoDamagedVentEfficiencyMultiplier = 1f;
        public float torsoDamagedMaxHeatMultiplier = 1f;
    }
}
