using SteelLance.Data;
using SteelLance.Mech;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 簡易戦車敵。プレイヤーへ接近し、射程内で定期ダメージ。NavMesh 不使用。
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyTank : MonoBehaviour
    {
        private const float DefaultMoveSpeed = 3f;
        private const float DefaultStopDistance = 4f;
        private const float DefaultAttackRange = 18f;
        private const float DefaultAttackInterval = 2f;
        private const float DefaultDamageToPlayer = 5f;

        [SerializeField] private float moveSpeed = DefaultMoveSpeed;
        [SerializeField] private float stopDistance = DefaultStopDistance;
        [SerializeField] private float attackRange = DefaultAttackRange;
        [SerializeField] private float attackInterval = DefaultAttackInterval;
        [SerializeField] private float damageToPlayer = DefaultDamageToPlayer;
        [SerializeField] private Transform playerTarget;

        private MechPartSystem _playerPartSystem;
        private Health _playerHealth;
        private float _nextAttackTime;

        private void Start()
        {
            if (playerTarget == null)
            {
                var mech = FindAnyObjectByType<MechController>();
                if (mech != null)
                {
                    playerTarget = mech.transform;
                }
            }

            if (playerTarget != null)
            {
                _playerPartSystem = playerTarget.GetComponent<MechPartSystem>();
                _playerHealth = playerTarget.GetComponent<Health>();
            }
        }
        private void Update()
        {
            if (playerTarget == null)
            {
                return;
            }

            MoveTowardPlayer();
            TryAttackPlayer();
        }

        private void MoveTowardPlayer()
        {
            var toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0f;

            var distance = toPlayer.magnitude;
            if (distance <= stopDistance)
            {
                return;
            }

            var direction = toPlayer / distance;
            transform.position += direction * (moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void TryAttackPlayer()
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            var toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.magnitude > attackRange)
            {
                return;
            }

            if (TryDamagePartSystem())
            {
                _nextAttackTime = Time.time + attackInterval;
                return;
            }

            if (_playerHealth != null && _playerHealth.IsAlive)
            {
                _playerHealth.TakeDamage(damageToPlayer);
                _nextAttackTime = Time.time + attackInterval;
            }
        }

        private bool TryDamagePartSystem()
        {
            if (_playerPartSystem == null || _playerPartSystem.IsDefeated)
            {
                return false;
            }

            var torso = _playerPartSystem.GetPart(BodyRegion.Torso);
            if (torso == null || !torso.IsAlive)
            {
                return false;
            }

            var context = new DamageContext
            {
                attacker = transform,
                hitRegion = BodyRegion.Torso
            };
            torso.TakeDamage(damageToPlayer, in context);
            return true;
        }
    }
}