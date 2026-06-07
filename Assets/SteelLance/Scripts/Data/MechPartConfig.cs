using UnityEngine;

namespace SteelLance.Data
{
    /// <summary>
    /// Mech part tuning for enemy authoring / Phase2B debug.
    /// See docs/design/unity/部位破壊設計.md §5.3.
    /// </summary>
    [CreateAssetMenu(fileName = "MechPartConfig_Default", menuName = "SteelLance/Mech Part Config")]
    public class MechPartConfig : ScriptableObject
    {
        [Tooltip("Play gate: false = head destroyed does not trigger end battle (sensor debuff only).")]
        public bool headEndBattleFlagEnabled = true;
    }
}
