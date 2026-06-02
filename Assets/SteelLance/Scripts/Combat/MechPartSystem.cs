using System;
using System.Collections.Generic;
using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 7 部位の集約・Deploy・デバフ・撃破判定。Phase2 プレイヤー機のルートコンポーネント。
    /// </summary>
    public class MechPartSystem : MonoBehaviour
    {
        // Phase2 デバッグ用仮値（通常 moveSpeed 8 → 半壊時 約4）。最終はバランス調整
        private const float LegDamagedMoveMultiplier = 0.5f;
        private const float LegDamagedTurnMultiplier = 0.85f;
        private const float ArmDamagedFireRateMultiplier = 0.85f;

        [SerializeField] private MechBuild defaultBuild;
        [SerializeField] private PartCatalogSO partCatalog;
        [SerializeField] private PartInstance[] partInstances;
        [SerializeField] private bool deployOnStart = true;
        [SerializeField] private bool logDefeatReason = true;
        [SerializeField] private bool logDeploySummary = true;

        private readonly Dictionary<BodyRegion, BodyPartHealth> _parts = new();
        private readonly List<BodyRegion> _activeWeaponMounts = new();
        private readonly List<MechWeapon> _weapons = new();

        private float _moveSpeedMultiplier = 1f;
        private float _turnRateMultiplier = 1f;
        private bool _defeated;

        public event Action<MechDefeatReason> OnMechDefeated;
        public event Action<BodyRegion, PartCondition> OnPartConditionChanged;

        public IReadOnlyList<BodyRegion> ActiveWeaponMounts => _activeWeaponMounts;
        public float TorsoCurrentHP => GetPart(BodyRegion.Torso)?.CurrentHP ?? 0f;
        public float TorsoMaxHP => GetPart(BodyRegion.Torso)?.MaxHP ?? 0f;
        public float MoveSpeedMultiplier => _defeated ? 0f : _moveSpeedMultiplier;
        public float TurnRateMultiplier => _defeated ? 0f : _turnRateMultiplier;
        public bool IsDefeated => _defeated;

        private void Awake()
        {
            CachePartsAndWeapons();
        }

        private void Start()
        {
            if (deployOnStart)
            {
                DeployDefaultBuild();
            }
        }

        public BodyPartHealth GetPart(BodyRegion region)
        {
            _parts.TryGetValue(region, out var part);
            return part;
        }

        public PartCondition GetPartCondition(BodyRegion region)
        {
            return GetPart(region)?.Condition ?? PartCondition.Destroyed;
        }

        public float GetWeaponFireRateMultiplier(BodyRegion region)
        {
            if (_defeated)
            {
                return 0f;
            }

            var condition = GetPartCondition(region);
            if (condition == PartCondition.Destroyed)
            {
                return 0f;
            }

            if (condition == PartCondition.Damaged &&
                (region == BodyRegion.ArmL || region == BodyRegion.ArmR ||
                 region == BodyRegion.ShoulderL || region == BodyRegion.ShoulderR))
            {
                return ArmDamagedFireRateMultiplier;
            }

            return 1f;
        }

        public bool CheckAllWeaponsLost()
        {
            // Phase3+: 装着中の全武器スロットが使用不能か判定
            return false;
        }

        public void DeployDefaultBuild()
        {
            if (defaultBuild == null || partCatalog == null || partInstances == null)
            {
                RecalculateMultipliers();
                return;
            }

            var validation = MechBuildValidator.Validate(defaultBuild, partCatalog, partInstances);
            if (!validation.IsValid)
            {
                Debug.LogWarning(
                    $"[SteelLance] MechBuild validation failed. Using scene defaults.\n{validation.FormatErrors()}");
                RecalculateMultipliers();
                return;
            }

            InitializeFromBuild(defaultBuild, partCatalog, partInstances);

            if (logDeploySummary)
            {
                LogDeploySummary(defaultBuild, partCatalog, partInstances);
            }
        }

        public void InitializeFromBuild(
            MechBuild build,
            PartCatalogSO catalog,
            IReadOnlyList<PartInstance> instances)
        {
            var instanceLookup = MechBuildCalculator.BuildInstanceLookup(instances);
            _activeWeaponMounts.Clear();

            foreach (var slot in build.bodySlots)
            {
                if (slot == null || !_parts.TryGetValue(slot.bodyRegion, out var partHealth))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(slot.frameInstanceId) ||
                    !instanceLookup.TryGetValue(slot.frameInstanceId, out var frameInstance))
                {
                    continue;
                }

                var frame = catalog.GetBodyFrame(frameInstance.partId);
                if (frame == null)
                {
                    continue;
                }

                partHealth.Initialize(frame.baseHP, frameInstance.currentHP, frame.damagedThreshold);
                RegisterWeaponMounts(slot, frame, catalog, instanceLookup);
            }

            ApplyBuildWeapons(build, catalog, instanceLookup);
            RecalculateMultipliers();
        }

        public void NotifyPartDamaged(BodyPartHealth part)
        {
            RecalculateMultipliers();
        }

        public void NotifyPartConditionChanged(
            BodyPartHealth part,
            PartCondition previous,
            PartCondition next)
        {
            OnPartConditionChanged?.Invoke(part.Region, next);

            if (next == PartCondition.Destroyed)
            {
                DisableWeaponsOnRegion(part.Region);
            }

            RecalculateMultipliers();

            if (next == PartCondition.Destroyed)
            {
                TryTriggerDefeat(part.Region);
            }

            if (CheckAllWeaponsLost())
            {
                TriggerDefeat(MechDefeatReason.AllWeaponsLost);
            }
        }

        private void CachePartsAndWeapons()
        {
            _parts.Clear();
            _weapons.Clear();

            var parts = GetComponentsInChildren<BodyPartHealth>(true);
            foreach (var part in parts)
            {
                _parts[part.Region] = part;
            }

            var weapons = GetComponentsInChildren<MechWeapon>(true);
            _weapons.AddRange(weapons);
            foreach (var weapon in _weapons)
            {
                weapon.BindPartSystem(this);
            }
        }

        private void RegisterWeaponMounts(
            BodySlotState slot,
            BodyFrameSO frame,
            PartCatalogSO catalog,
            Dictionary<string, PartInstance> instanceLookup)
        {
            if (frame.internalSlots == null)
            {
                return;
            }

            foreach (var slotDefinition in frame.internalSlots)
            {
                if (slotDefinition == null || slotDefinition.slotType != SlotType.Weapon)
                {
                    continue;
                }

                var equipped = FindSlotEquipment(slot, slotDefinition.slotId);
                if (equipped == null || string.IsNullOrEmpty(equipped.equipmentInstanceId))
                {
                    continue;
                }

                if (!instanceLookup.TryGetValue(equipped.equipmentInstanceId, out var equipmentInstance))
                {
                    continue;
                }

                var equipment = catalog.GetEquipment(equipmentInstance.partId);
                if (equipment?.weaponProfile == null || equipment.weaponProfile.projectilePrefab == null)
                {
                    continue;
                }

                if (!_activeWeaponMounts.Contains(slot.bodyRegion))
                {
                    _activeWeaponMounts.Add(slot.bodyRegion);
                }
            }
        }

        private void ApplyBuildWeapons(
            MechBuild build,
            PartCatalogSO catalog,
            Dictionary<string, PartInstance> instanceLookup)
        {
            foreach (var weapon in _weapons)
            {
                var equipment = FindEquipmentForRegion(weapon.BodyRegion, build, catalog, instanceLookup);
                var hasWeaponEquipment = equipment?.weaponProfile != null &&
                                         equipment.weaponProfile.projectilePrefab != null;

                if (!hasWeaponEquipment)
                {
                    weapon.enabled = false;
                    continue;
                }

                weapon.ApplyWeaponProfile(equipment.weaponProfile, equipment.stats);
                weapon.enabled = GetPartCondition(weapon.BodyRegion) != PartCondition.Destroyed;
            }
        }

        private void LogDeploySummary(
            MechBuild build,
            PartCatalogSO catalog,
            IReadOnlyList<PartInstance> instances)
        {
            var instanceLookup = MechBuildCalculator.BuildInstanceLookup(instances);
            var totalWeight = MechBuildCalculator.GetTotalWeight(build, catalog, instances);
            var weightLimit = MechBuildCalculator.GetWeightClassLimit(build.weightClass);
            Debug.Log(
                $"[SteelLance] MechBuild Deploy OK — weight {totalWeight:0.#}t / {weightLimit:0.#}t");

            foreach (var weapon in _weapons)
            {
                var equipment = FindEquipmentForRegion(weapon.BodyRegion, build, catalog, instanceLookup);
                if (equipment?.weaponProfile == null || !weapon.enabled)
                {
                    Debug.Log($"[SteelLance]   Weapon {weapon.BodyRegion}: OFF（Build に武器装備なし or 部位全損）");
                    continue;
                }

                var damage = equipment.stats != null ? equipment.stats.damage : 0f;
                Debug.Log(
                    $"[SteelLance]   Weapon {weapon.BodyRegion}: {equipment.partId} " +
                    $"dmg={damage:0.#} cd={equipment.weaponProfile.cooldown:0.##}s");
            }
        }

        private static EquipmentSO FindEquipmentForRegion(
            BodyRegion region,
            MechBuild build,
            PartCatalogSO catalog,
            Dictionary<string, PartInstance> instanceLookup)
        {
            foreach (var slot in build.bodySlots)
            {
                if (slot == null || slot.bodyRegion != region)
                {
                    continue;
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

                    return catalog.GetEquipment(equipmentInstance.partId);
                }
            }

            return null;
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

        private void DisableWeaponsOnRegion(BodyRegion region)
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.BodyRegion == region)
                {
                    weapon.enabled = false;
                }
            }
        }

        private void RecalculateMultipliers()
        {
            var weightFactor = MechBuildCalculator.GetWeightFactor(defaultBuild, partCatalog, partInstances);
            var legCondition = GetPartCondition(BodyRegion.Legs);

            var moveMultiplier = weightFactor;
            var turnMultiplier = 1f;

            switch (legCondition)
            {
                case PartCondition.Damaged:
                    moveMultiplier *= LegDamagedMoveMultiplier;
                    turnMultiplier *= LegDamagedTurnMultiplier;
                    break;
                case PartCondition.Destroyed:
                    moveMultiplier = 0f;
                    turnMultiplier = 0f;
                    break;
            }

            _moveSpeedMultiplier = moveMultiplier;
            _turnRateMultiplier = turnMultiplier;
        }

        private void TryTriggerDefeat(BodyRegion region)
        {
            switch (region)
            {
                case BodyRegion.Torso:
                    TriggerDefeat(MechDefeatReason.TorsoDestroyed);
                    break;
                case BodyRegion.Head:
                    TriggerDefeat(MechDefeatReason.HeadDestroyed);
                    break;
                case BodyRegion.Legs:
                    TriggerDefeat(MechDefeatReason.LegsDestroyed);
                    break;
            }
        }

        private void TriggerDefeat(MechDefeatReason reason)
        {
            if (_defeated)
            {
                return;
            }

            _defeated = true;
            _moveSpeedMultiplier = 0f;
            _turnRateMultiplier = 0f;

            if (logDefeatReason)
            {
                Debug.Log($"[SteelLance] MECH DEFEATED — {reason}");
            }

            OnMechDefeated?.Invoke(reason);
        }
    }
}
