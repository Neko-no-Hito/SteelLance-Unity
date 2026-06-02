using System.Collections.Generic;
using UnityEngine;

namespace SteelLance.Data
{
    [CreateAssetMenu(fileName = "PartCatalog", menuName = "SteelLance/Part Catalog")]
    public class PartCatalogSO : ScriptableObject
    {
        public PartSO[] allParts;

        private Dictionary<string, PartSO> _lookup;

        public PartSO GetPart(string partId)
        {
            if (string.IsNullOrEmpty(partId))
            {
                return null;
            }

            EnsureLookup();
            _lookup.TryGetValue(partId, out var part);
            return part;
        }

        public BodyFrameSO GetBodyFrame(string partId)
        {
            return GetPart(partId) as BodyFrameSO;
        }

        public EquipmentSO GetEquipment(string partId)
        {
            return GetPart(partId) as EquipmentSO;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
            {
                return;
            }

            _lookup = new Dictionary<string, PartSO>();
            if (allParts == null)
            {
                return;
            }

            foreach (var part in allParts)
            {
                if (part == null || string.IsNullOrEmpty(part.partId))
                {
                    continue;
                }

                _lookup[part.partId] = part;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
