using UnityEngine;

namespace SteelLance.Data
{
    public abstract class PartSO : ScriptableObject
    {
        public string partId;
        public string displayName;
        [TextArea] public string description;
        public PartKind partKind;
        public Rarity rarity;
        public float weight;
        public int shopPrice;
        public string salvageTag;
        public Sprite icon;
    }
}
