#if UNITY_EDITOR
using System;
using SteelLance.Combat;
using SteelLance.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelLance.EditorTools
{
    /// <summary>
    /// Batchmode regression for STEP 2B-ENG-F (U6/U7 API checks).
    /// Unity -batchmode -executeMethod SteelLance.EditorTools.Phase2BRegressionRunner.Run
    /// </summary>
    public static class Phase2BRegressionRunner
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const float LegDamagedMoveMultiplier = 0.5f;
        private const float DamagePerPress = 20f;

        public static void Run()
        {
            try
            {
                RunChecks();
                Debug.Log("[SteelLance] Phase2B regression: ALL PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteelLance] Phase2B regression FAILED: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        private static void RunChecks()
        {
            if (!EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single).IsValid())
            {
                throw new InvalidOperationException($"Could not open {SampleScenePath}");
            }

            var partSystem = UnityEngine.Object.FindAnyObjectByType<MechPartSystem>();
            if (partSystem == null)
            {
                throw new InvalidOperationException("MechPartSystem not found in SampleScene");
            }

            partSystem.EditorEnsureInitialized();
            Assert(partSystem.TorsoMaxHP > 0f, "Deploy: TorsoMaxHP > 0");

            ResetMech(partSystem);
            DestroyPart(partSystem, BodyRegion.Torso);
            Assert(partSystem.IsDefeated, "Torso destroyed → defeated");
            Assert(partSystem.EvaluateEndBattleFlags() == MechDefeatReason.TorsoDestroyed, "Torso end flag");

            ResetMech(partSystem);
            DestroyPart(partSystem, BodyRegion.Head);
            Assert(partSystem.IsDefeated, "Head destroyed → defeated");
            Assert(partSystem.EvaluateEndBattleFlags() == MechDefeatReason.HeadDestroyed, "Head end flag");

            ResetMech(partSystem);
            DestroyPart(partSystem, BodyRegion.Legs);
            Assert(!partSystem.IsDefeated, "Legs destroyed → NOT defeated (v0.3.3)");
            Assert(partSystem.MoveSpeedMultiplier == 0f, "Legs destroyed → move 0");

            ResetMech(partSystem);
            var legs = partSystem.GetPart(BodyRegion.Legs);
            var legsDamageToHalf = legs != null
                ? legs.MaxHP * (1f - legs.DamagedThreshold) + 1f
                : DamagePerPress * 2f;
            DamagePart(partSystem, BodyRegion.Legs, legsDamageToHalf);
            Assert(
                partSystem.GetPartCondition(BodyRegion.Legs) == PartCondition.Damaged,
                "Legs damaged state");
            Assert(
                Mathf.Approximately(partSystem.MoveSpeedMultiplier, LegDamagedMoveMultiplier) ||
                partSystem.MoveSpeedMultiplier < 1f,
                "Legs damaged → move reduced");

            ResetMech(partSystem);
            DestroyPart(partSystem, BodyRegion.ArmR);
            Assert(partSystem.GetWeaponFireRateMultiplier(BodyRegion.ArmR) == 0f, "ArmR destroyed → fire rate 0");

            ResetMech(partSystem);
            var grade = SalvageResolver.ResolveGrade(0.85f, null);
            Assert(grade == SalvageGrade.High, "ResolveGrade high tier");
            Assert(SalvageResolver.ResolveSurvivor(PartCondition.Damaged) == SalvageGrade.Scrap, "ResolveSurvivor");

            var pools = DamagePoolSplitter.SplitDamageToPools(10f, null, isVenting: true);
            Assert(Mathf.Approximately(pools.QualityDamage, 0f), "Vent qualityDrain × 0");

            Debug.Log("[SteelLance] MechBuild Deploy OK — regression runner verified");
        }

        private static void ResetMech(MechPartSystem partSystem)
        {
            partSystem.EditorEnsureInitialized();
            foreach (BodyRegion region in Enum.GetValues(typeof(BodyRegion)))
            {
                partSystem.SetPartConditionForDebug(region, PartCondition.Intact);
            }
        }

        private static void DestroyPart(MechPartSystem partSystem, BodyRegion region)
        {
            partSystem.SetPartConditionForDebug(region, PartCondition.Destroyed);
        }

        private static void DamagePart(MechPartSystem partSystem, BodyRegion region, float amount)
        {
            var part = partSystem.GetPart(region);
            if (part == null)
            {
                throw new InvalidOperationException($"No part for {region}");
            }

            var context = new DamageContext { hitRegion = region };
            part.TakeDamage(amount, in context);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }

            Debug.Log($"[SteelLance]   OK: {message}");
        }
    }
}
#endif
