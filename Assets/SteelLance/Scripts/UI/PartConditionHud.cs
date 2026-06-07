using SteelLance.Combat;
using SteelLance.Data;
using TMPro;
using UnityEngine;

namespace SteelLance.UI
{
    /// <summary>
    /// Phase2B verification HUD — 7 region states (no numeric debuffs).
    /// See docs/design/ux/部位破壊UX設計.md §5.5.
    /// </summary>
    public class PartConditionHud : MonoBehaviour
    {
        private static readonly BodyRegion[] DisplayOrder =
        {
            BodyRegion.ShoulderL,
            BodyRegion.Head,
            BodyRegion.ShoulderR,
            BodyRegion.ArmL,
            BodyRegion.Torso,
            BodyRegion.ArmR,
            BodyRegion.Legs
        };

        private static readonly Color IntactColor = new(0.55f, 0.95f, 0.55f);
        private static readonly Color DamagedColor = new(1f, 0.85f, 0.35f);
        private static readonly Color DestroyedColor = new(0.75f, 0.35f, 0.35f);

        [SerializeField] private MechPartSystem partSystem;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private bool showDualHpStub;

        private void Start()
        {
            if (partSystem == null)
            {
                partSystem = FindAnyObjectByType<MechPartSystem>();
            }

            if (partSystem != null)
            {
                partSystem.OnPartConditionChanged += OnPartConditionChanged;
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (partSystem != null)
            {
                partSystem.OnPartConditionChanged -= OnPartConditionChanged;
            }
        }

        private void OnPartConditionChanged(BodyRegion region, PartCondition condition)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (statusText == null || partSystem == null)
            {
                return;
            }

            var lines = new System.Text.StringBuilder();
            foreach (var region in DisplayOrder)
            {
                var condition = partSystem.GetPartCondition(region);
                var color = GetColorHex(condition);
                var label = GetShortLabel(region);
                lines.AppendLine($"<color={color}>{label}: {condition}</color>");

                if (showDualHpStub)
                {
                    AppendDualHpStub(lines, region);
                }
            }

            var endFlag = partSystem.EvaluateEndBattleFlags();
            if (endFlag.HasValue)
            {
                lines.AppendLine($"<color=#FF6666>END: {endFlag.Value}</color>");
            }

            statusText.text = lines.ToString();
        }

        private void AppendDualHpStub(System.Text.StringBuilder lines, BodyRegion region)
        {
            var part = partSystem.GetPart(region);
            if (part == null || !part.HasQualityPool)
            {
                return;
            }

            lines.AppendLine(
                $"  A/D/Q {part.ArmorHP:0}/{part.DestructionHP:0}/{part.QualityHP:0}");
        }

        private static string GetShortLabel(BodyRegion region)
        {
            return region switch
            {
                BodyRegion.ShoulderL => "ShL",
                BodyRegion.ShoulderR => "ShR",
                BodyRegion.ArmL => "ArmL",
                BodyRegion.ArmR => "ArmR",
                _ => region.ToString()
            };
        }

        private static string GetColorHex(PartCondition condition)
        {
            var color = condition switch
            {
                PartCondition.Intact => IntactColor,
                PartCondition.Damaged => DamagedColor,
                _ => DestroyedColor
            };

            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }
}
