using System;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 単一 HP。戦車など部位なしユニット用。Phase2 プレイヤーは MechPartSystem を使用。
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        private const float DefaultMaxHealth = 100f;

        [SerializeField] private float maxHealth = DefaultMaxHealth;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private bool logGameOverOnDeath;

        private float _currentHealth;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0f;

        public event Action<Health> Died;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount, in DamageContext context)
        {
            TakeDamage(amount);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);

            if (!IsAlive)
            {
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            Died?.Invoke(this);

            if (logGameOverOnDeath)
            {
                Debug.Log("[SteelLance] GAME OVER — Player destroyed.");
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
