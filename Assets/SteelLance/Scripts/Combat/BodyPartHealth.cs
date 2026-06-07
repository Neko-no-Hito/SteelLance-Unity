using System;
using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 1 Collider = 1 部位 HP。半壊/全損の状態遷移を MechPartSystem へ通知。
    /// Phase2B: dual-HP fields reserved (hasQualityPool=false on player).
    /// </summary>
    public class BodyPartHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private BodyRegion region;
        [SerializeField] private float maxHP = 50f;
        [SerializeField] private float damagedThreshold = 0.5f;
        [SerializeField] private bool hasQualityPool;
        [SerializeField] private float qualityHPMax = 50f;

        private float _currentHP;
        private float _armorHP;
        private float _destructionHP;
        private float _qualityHP;
        private PartCondition _condition = PartCondition.Intact;
        private MechPartSystem _owner;
        private Collider _collider;

        public BodyRegion Region => region;
        public float MaxHP => maxHP;
        public float CurrentHP => _currentHP;
        public float DamagedThreshold => damagedThreshold;
        public PartCondition Condition => _condition;
        public bool IsAlive => _condition != PartCondition.Destroyed;
        public bool HasQualityPool => hasQualityPool;
        public float ArmorHP => _armorHP;
        public float ArmorHPMax => maxHP;
        public float DestructionHP => _destructionHP;
        public float DestructionHPMax => maxHP;
        public float QualityHP => _qualityHP;
        public float QualityHPMax => qualityHPMax;

        public event Action<BodyPartHealth, PartCondition, PartCondition> ConditionChanged;

        public void BindOwner(MechPartSystem owner)
        {
            _owner = owner;
        }

        private void Awake()
        {
            _owner = GetComponentInParent<MechPartSystem>();
            _collider = GetComponent<Collider>();
            ResetPoolValues(maxHP);
            _condition = PartConditionUtility.Evaluate(_currentHP, maxHP, damagedThreshold);
        }

        public void Initialize(float newMaxHP, float currentHP, float newDamagedThreshold)
        {
            maxHP = Mathf.Max(1f, newMaxHP);
            damagedThreshold = Mathf.Clamp(newDamagedThreshold, 0.01f, 0.99f);
            qualityHPMax = maxHP;
            ResetPoolValues(currentHP >= 0f ? Mathf.Min(currentHP, maxHP) : maxHP);
            ApplyCondition(PartConditionUtility.Evaluate(_currentHP, maxHP, damagedThreshold), forceNotify: true);
        }

        public void TakeDamage(float amount, in DamageContext context)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            // Phase2 player path: single-HP. Dual-HP routing is 中型敵 Phase.
            if (hasQualityPool && _condition == PartCondition.Damaged)
            {
                var pools = DamagePoolSplitter.SplitDamageToPools(amount, context.weapon, context.isVenting);
                ApplyPoolDamageInternal(pools);
                return;
            }

            var previous = _condition;
            _currentHP = Mathf.Max(0f, _currentHP - amount);
            SyncPoolsFromCurrentHP();
            var next = PartConditionUtility.Evaluate(_currentHP, maxHP, damagedThreshold);
            ApplyCondition(next);

            if (previous != next)
            {
                _owner?.NotifyPartConditionChanged(this, previous, next);
            }
            else if (next != PartCondition.Destroyed)
            {
                _owner?.NotifyPartDamaged(this);
            }
        }

        internal void ApplyPoolDamageInternal(DamagePoolSplitter.PoolDamage pools)
        {
            if (_condition == PartCondition.Destroyed)
            {
                return;
            }

            var previous = _condition;
            var destApplied = Mathf.Min(pools.DestructionDamage, _destructionHP);
            _destructionHP -= destApplied;

            var qualApplied = pools.QualityDamage;
            if (hasQualityPool && _destructionHP <= 0f)
            {
                qualApplied = Mathf.Min(qualApplied, _qualityHP);
            }

            if (hasQualityPool)
            {
                _qualityHP = Mathf.Max(0f, _qualityHP - qualApplied);
            }

            _currentHP = _destructionHP;
            var next = PartConditionUtility.Evaluate(_destructionHP, maxHP, damagedThreshold);
            ApplyCondition(next);

            if (previous != next)
            {
                _owner?.NotifyPartConditionChanged(this, previous, next);
            }
            else if (next != PartCondition.Destroyed)
            {
                _owner?.NotifyPartDamaged(this);
            }
        }

        /// <summary>Phase2B debug — sets condition without combat routing.</summary>
        public void SetConditionForDebug(PartCondition target)
        {
            var hpForCondition = target switch
            {
                PartCondition.Intact => maxHP,
                PartCondition.Damaged => maxHP * damagedThreshold,
                PartCondition.Destroyed => 0f,
                _ => maxHP
            };

            ResetPoolValues(hpForCondition);
            var previous = _condition;
            ApplyCondition(target, forceNotify: true);
            _owner?.NotifyPartConditionChanged(this, previous, target);
        }

        private void ResetPoolValues(float currentHP)
        {
            _currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
            _armorHP = _currentHP;
            _destructionHP = _currentHP;
            _qualityHP = qualityHPMax;
        }

        private void SyncPoolsFromCurrentHP()
        {
            _destructionHP = _currentHP;
            _armorHP = _currentHP;
        }

        private void ApplyCondition(PartCondition next, bool forceNotify = false)
        {
            if (!forceNotify && _condition == next)
            {
                return;
            }

            var previous = _condition;
            _condition = next;

            if (_condition == PartCondition.Destroyed && _collider != null)
            {
                _collider.enabled = false;
            }

            ConditionChanged?.Invoke(this, previous, next);
        }
    }

    public static class PartConditionUtility
    {
        public static PartCondition Evaluate(float currentHP, float maxHP, float threshold)
        {
            if (currentHP <= 0f)
            {
                return PartCondition.Destroyed;
            }

            if (currentHP <= maxHP * threshold)
            {
                return PartCondition.Damaged;
            }

            return PartCondition.Intact;
        }
    }
}
