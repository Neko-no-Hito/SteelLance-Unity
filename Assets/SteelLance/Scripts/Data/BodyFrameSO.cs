using UnityEngine;

namespace SteelLance.Data
{
    [CreateAssetMenu(fileName = "BF_New", menuName = "SteelLance/Body Frame")]
    public class BodyFrameSO : PartSO
    {
        public BodyRegion bodyRegion;
        public float baseHP = 50f;

        [Range(0.01f, 0.99f)]
        public float damagedThreshold = 0.5f;

        public SlotDefinition[] internalSlots;
        public GameObject visualPrefab;
    }

    [System.Serializable]
    public class SlotDefinition
    {
        public string slotId;
        public SlotType slotType;
        public EquipmentClass[] allowedClasses;
        public bool required;
        public string socketName;
    }
}
