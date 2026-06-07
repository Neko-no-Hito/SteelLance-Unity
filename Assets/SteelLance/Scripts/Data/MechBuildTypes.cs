using System;
using System.Collections.Generic;

namespace SteelLance.Data
{
    [Serializable]
    public class PartInstance
    {
        public string instanceId;
        public string partId;
        public float currentHP = -1f;
        public bool equipped;
    }

    [Serializable]
    public class SlotEquipment
    {
        public string slotId;
        public string equipmentInstanceId;
    }

    /// <summary>
    /// Arm primary/reserve loadout. SwapReserve() combat logic is 🔴 stub (R1 · Phase3+).
    /// Salvage とどめは equipmentInstanceId 単位（R2 · サルベージ設計.md）。
    /// </summary>
    [Serializable]
    public class ArmLoadout
    {
        public string primaryEquipmentInstanceId;
        public string reserveEquipmentInstanceId;
    }

    [Serializable]
    public class BodySlotState
    {
        public BodyRegion bodyRegion;
        public string frameInstanceId;
        public List<SlotEquipment> slotEquipments = new();

        // ArmL/ArmR: reserve loadout (Phase2 combat unused)
        public ArmLoadout armLoadout;
    }

    [Serializable]
    public class MechBuild
    {
        public string buildName = "Default";
        public WeightClass weightClass = WeightClass.Medium60;
        public List<BodySlotState> bodySlots = new();
        public string pilotId;
        public string aiProfileId;
    }

    [Serializable]
    public class BuildValidationResult
    {
        public bool IsValid;
        public List<BuildValidationError> Errors = new();
        public List<string> Details = new();

        public string FormatErrors()
        {
            if ((Errors == null || Errors.Count == 0) &&
                (Details == null || Details.Count == 0))
            {
                return "  (no error details)";
            }

            var lines = new System.Text.StringBuilder();
            if (Errors != null)
            {
                foreach (var error in Errors)
                {
                    lines.AppendLine($"  - {error}: {DescribeError(error)}");
                }
            }

            if (Details != null)
            {
                foreach (var detail in Details)
                {
                    lines.AppendLine($"  · {detail}");
                }
            }

            return lines.ToString().TrimEnd();
        }

        private static string DescribeError(BuildValidationError error)
        {
            return error switch
            {
                BuildValidationError.MissingFrame =>
                    "7部位すべてに frameInstanceId が必要（Body Slots = 7要素）",
                BuildValidationError.EmptyRequiredSlot =>
                    "必須スロットが空、または equipmentInstanceId が Part Instances に無い（下の · 行を確認）",
                BuildValidationError.SlotMismatch =>
                    "装備とスロット定義が不一致（slotId / slotType / equipmentClass）",
                BuildValidationError.OverWeight =>
                    "重量超過（TotalWeight > weightClass 上限）",
                BuildValidationError.DuplicateInstance =>
                    "同一 instanceId の二重使用",
                BuildValidationError.TorsoDestroyed =>
                    "胴体 frame の currentHP <= 0",
                BuildValidationError.ShoulderPairMismatch =>
                    "肩 L/R の shoulderSetId が不一致、または片方のみ設定",
                _ => error.ToString()
            };
        }
    }
}
