using System.Collections.Generic;

namespace SteelLance.Data
{
    public static class MechBuildCalculator
    {
        public static float GetWeightClassLimit(WeightClass weightClass)
        {
            return weightClass switch
            {
                WeightClass.Light40 => 40f,
                WeightClass.Medium60 => 60f,
                WeightClass.Heavy90 => 90f,
                _ => 60f
            };
        }

        public static float GetTotalWeight(
            MechBuild build,
            PartCatalogSO catalog,
            IReadOnlyList<PartInstance> instances)
        {
            if (build == null || catalog == null || instances == null)
            {
                return 0f;
            }

            var instanceLookup = BuildInstanceLookup(instances);
            var total = 0f;

            foreach (var slot in build.bodySlots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.frameInstanceId))
                {
                    continue;
                }

                if (!instanceLookup.TryGetValue(slot.frameInstanceId, out var frameInstance))
                {
                    continue;
                }

                var frame = catalog.GetBodyFrame(frameInstance.partId);
                if (frame != null)
                {
                    total += frame.weight;
                }

                foreach (var slotEquipment in slot.slotEquipments)
                {
                    if (slotEquipment == null || string.IsNullOrEmpty(slotEquipment.equipmentInstanceId))
                    {
                        continue;
                    }

                    if (!instanceLookup.TryGetValue(slotEquipment.equipmentInstanceId, out var equipmentInstance))
                    {
                        continue;
                    }

                    var equipment = catalog.GetEquipment(equipmentInstance.partId);
                    if (equipment != null)
                    {
                        total += equipment.weight;
                    }
                }
            }

            return total;
        }

        public static float GetWeightFactor(
            MechBuild build,
            PartCatalogSO catalog,
            IReadOnlyList<PartInstance> instances)
        {
            if (build == null)
            {
                return 1f;
            }

            var total = GetTotalWeight(build, catalog, instances);
            var limit = GetWeightClassLimit(build.weightClass);
            return total <= limit ? 1f : 0.5f;
        }

        public static Dictionary<string, PartInstance> BuildInstanceLookup(IReadOnlyList<PartInstance> instances)
        {
            var lookup = new Dictionary<string, PartInstance>();
            if (instances == null)
            {
                return lookup;
            }

            foreach (var instance in instances)
            {
                if (instance == null || string.IsNullOrEmpty(instance.instanceId))
                {
                    continue;
                }

                lookup[instance.instanceId] = instance;
            }

            return lookup;
        }
    }
}
