using UnityEngine;

namespace SteelLance.Data
{
    [CreateAssetMenu(fileName = "EQ_New", menuName = "SteelLance/Equipment")]
    public class EquipmentSO : PartSO
    {
        public EquipmentClass equipmentClass;
        public SlotType slotType;
        public EquipmentStats stats;
        public WeaponProfile weaponProfile;
    }

    [System.Serializable]
    public class EquipmentStats
    {
        public float damage;
        public float fireRate;
        public float range;
        public float accuracyModifier;
        public float lockOnSpeedModifier;
        public float maxSpeedModifier;
        public float turnRateModifier;
        public float carryCapacityBonus;
    }

    [System.Serializable]
    public class WeaponProfile
    {
        public GameObject projectilePrefab;
        public string muzzleSocket;
        public float cooldown = 0.25f;
        public int magazineSize;

        [Header("Heat (Phase3 data · Phase4+ combat — unused in Phase2)")]
        public float heatPerShot;
        public float heatPerSecond;
        public int heatAxisScore;

        [Header("Dual-HP (中型敵 Phase combat · Phase2B data only)")]
        public float powerFactor = 1f;
        public float qualityDrainFactor = 1f;

        // Shoulder volley: sum both shoulders' heatPerShot on one input, or max — 🔴 see 決めるべきこと.md §3 Phase2B.
    }
}
