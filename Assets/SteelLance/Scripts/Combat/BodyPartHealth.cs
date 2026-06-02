using System;
using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 1 Collider = 1 部位 HP。半壊/全損の状態遷移を MechPartSystem へ通知。
    /// </summary>
    public class BodyPartHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private BodyRegion region;
        [SerializeField] private float maxHP = 50f;
        [SerializeField] private float damagedThreshold = 0.5f;

        private float _currentHP;
        private PartCondition _condition = PartCondition.Intact;
        private MechPartSystem _owner;
        private Collider _collider;

        public BodyRegion Region => region;
        public float MaxHP => maxHP;
        public float CurrentHP => _currentHP;
        public float DamagedThreshold => damagedThreshold;
        public PartCondition Condition => _condition;
        public bool IsAlive => _condition != PartCondition.Destroyed;

        public event Action<BodyPartHealth, PartCondition, PartCondition> ConditionChanged;

        private void Awake()
        {
            _owner = GetComponentInParent<MechPartSystem>();
            _collider = GetComponent<Collider>();
            _currentHP = maxHP;
            _condition = PartConditionUtility.Evaluate(_currentHP, maxHP, damagedThreshold);
        }

        public void Initialize(float newMaxHP, float currentHP, float newDamagedThreshold)
        {
            maxHP = Mathf.Max(1f, newMaxHP);
            damagedThreshold = Mathf.Clamp(newDamagedThreshold, 0.01f, 0.99f);
            _currentHP = currentHP >= 0f ? Mathf.Min(currentHP, maxHP) : maxHP;
            ApplyCondition(PartConditionUtility.Evaluate(_currentHP, maxHP, damagedThreshold), forceNotify: true);
        }

        public void TakeDamage(float amount, in DamageContext context)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            var previous = _condition;
            _currentHP = Mathf.Max(0f, _currentHP - amount);
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
