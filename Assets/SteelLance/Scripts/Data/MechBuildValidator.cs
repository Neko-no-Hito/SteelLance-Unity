using System.Collections.Generic;

namespace SteelLance.Data
{
    public static class MechBuildValidator
    {
        public static BuildValidationResult Validate(
            MechBuild build,
            PartCatalogSO catalog,
            IReadOnlyList<PartInstance> instances)
        {
            var result = new BuildValidationResult { IsValid = true };
            if (build == null || catalog == null || instances == null)
            {
                result.IsValid = false;
                result.Details.Add("build / catalog / partInstances のいずれかが null");
                return result;
            }

            if (build.bodySlots == null || build.bodySlots.Count == 0)
            {
                AddError(result, BuildValidationError.MissingFrame);
                result.Details.Add($"Body Slots が空です（Size を 7 に）");
                return result;
            }

            result.Details.Add($"Body Slots 数 = {build.bodySlots.Count}（7 必要）");

            var instanceLookup = MechBuildCalculator.BuildInstanceLookup(instances);
            var usedInstances = new HashSet<string>();
            var coveredRegions = new HashSet<BodyRegion>();

            foreach (var slot in build.bodySlots)
            {
                if (slot == null)
                {
                    continue;
                }

                coveredRegions.Add(slot.bodyRegion);

                if (string.IsNullOrEmpty(slot.frameInstanceId))
                {
                    AddError(result, BuildValidationError.MissingFrame);
                    result.Details.Add($"{slot.bodyRegion}: frameInstanceId が空");
                    continue;
                }

                if (!usedInstances.Add(slot.frameInstanceId))
                {
                    AddError(result, BuildValidationError.DuplicateInstance);
                    result.Details.Add($"instanceId 重複: {slot.frameInstanceId}");
                }

                if (!instanceLookup.TryGetValue(slot.frameInstanceId, out var frameInstance))
                {
                    AddError(result, BuildValidationError.MissingFrame);
                    result.Details.Add(
                        $"{slot.bodyRegion}: frameInstanceId '{slot.frameInstanceId}' が Part Instances に無い");
                    continue;
                }

                var frame = catalog.GetBodyFrame(frameInstance.partId);
                if (frame == null)
                {
                    AddError(result, BuildValidationError.MissingFrame);
                    result.Details.Add(
                        $"{slot.bodyRegion}: partId '{frameInstance.partId}' が Catalog に無い or BodyFrame ではない");
                    continue;
                }

                if (frame.bodyRegion != slot.bodyRegion)
                {
                    AddError(result, BuildValidationError.MissingFrame);
                    result.Details.Add(
                        $"{slot.bodyRegion}: SO の bodyRegion が {frame.bodyRegion}（不一致）");
                    continue;
                }

                if (slot.bodyRegion == BodyRegion.Torso)
                {
                    var torsoHp = frameInstance.currentHP >= 0f
                        ? frameInstance.currentHP
                        : frame.baseHP;
                    if (torsoHp <= 0f)
                    {
                        AddError(result, BuildValidationError.TorsoDestroyed);
                    }
                }

                ValidateSlotEquipment(slot, frame, instanceLookup, usedInstances, catalog, result);
            }

            foreach (BodyRegion region in System.Enum.GetValues(typeof(BodyRegion)))
            {
                if (!coveredRegions.Contains(region))
                {
                    AddError(result, BuildValidationError.MissingFrame);
                    result.Details.Add($"未登録の部位: {region}");
                }
            }

            var totalWeight = MechBuildCalculator.GetTotalWeight(build, catalog, instances);
            if (totalWeight > MechBuildCalculator.GetWeightClassLimit(build.weightClass))
            {
                AddError(result, BuildValidationError.OverWeight);
            }

            return result;
        }

        private static void ValidateSlotEquipment(
            BodySlotState slot,
            BodyFrameSO frame,
            Dictionary<string, PartInstance> instanceLookup,
            HashSet<string> usedInstances,
            PartCatalogSO catalog,
            BuildValidationResult result)
        {
            if (frame.internalSlots == null)
            {
                return;
            }

            foreach (var slotDefinition in frame.internalSlots)
            {
                if (slotDefinition == null)
                {
                    continue;
                }

                var equipped = FindSlotEquipment(slot, slotDefinition.slotId);
                if (equipped == null || string.IsNullOrEmpty(equipped.equipmentInstanceId))
                {
                    if (slotDefinition.required)
                    {
                        AddError(result, BuildValidationError.EmptyRequiredSlot);
                        result.Details.Add(
                            $"{slot.bodyRegion}: 必須スロット '{slotDefinition.slotId}' が空");
                    }

                    continue;
                }

                if (!usedInstances.Add(equipped.equipmentInstanceId))
                {
                    AddError(result, BuildValidationError.DuplicateInstance);
                }

                if (!instanceLookup.TryGetValue(equipped.equipmentInstanceId, out var equipmentInstance))
                {
                    AddError(result, BuildValidationError.EmptyRequiredSlot);
                    result.Details.Add(
                        $"{slot.bodyRegion}: '{equipped.equipmentInstanceId}' が Part Instances に無い（typo または未登録）");
                    continue;
                }

                var equipment = catalog.GetEquipment(equipmentInstance.partId);
                if (equipment == null)
                {
                    AddError(result, BuildValidationError.SlotMismatch);
                    continue;
                }

                if (equipment.slotType != slotDefinition.slotType)
                {
                    AddError(result, BuildValidationError.SlotMismatch);
                    continue;
                }

                if (slotDefinition.allowedClasses != null &&
                    slotDefinition.allowedClasses.Length > 0 &&
                    !System.Array.Exists(slotDefinition.allowedClasses, c => c == equipment.equipmentClass))
                {
                    AddError(result, BuildValidationError.SlotMismatch);
                }
            }
        }

        private static SlotEquipment FindSlotEquipment(BodySlotState slot, string slotId)
        {
            foreach (var slotEquipment in slot.slotEquipments)
            {
                if (slotEquipment != null && slotEquipment.slotId == slotId)
                {
                    return slotEquipment;
                }
            }

            return null;
        }

        private static void AddError(BuildValidationResult result, BuildValidationError error)
        {
            result.IsValid = false;
            if (!result.Errors.Contains(error))
            {
                result.Errors.Add(error);
            }
        }
    }
}
