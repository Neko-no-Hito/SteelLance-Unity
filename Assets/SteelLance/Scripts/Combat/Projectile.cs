using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    /// <summary>
    /// 直進弾。寿命で消滅。SphereCast + Trigger で命中判定。
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        private const float DefaultSpeed = 40f;
        private const float DefaultLifeSeconds = 3f;
        private const float DefaultDamage = 10f;
        private const float DefaultHitRadius = 0.35f;

        [SerializeField] private float speed = DefaultSpeed;
        [SerializeField] private float lifeSeconds = DefaultLifeSeconds;
        [SerializeField] private float damage = DefaultDamage;
        [SerializeField] private float hitRadius = DefaultHitRadius;

        private Transform _ownerRoot;
        private bool _initialized;

        public float Damage => damage;

        public void Initialize(Transform ownerRoot, Vector3 direction, float damageOverride = -1f)
        {
            _ownerRoot = ownerRoot;
            if (damageOverride >= 0f)
            {
                damage = damageOverride;
            }

            var flatDirection = direction;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude < 0.001f)
            {
                flatDirection = ownerRoot.forward;
            }

            transform.rotation = Quaternion.LookRotation(flatDirection.normalized);
            _initialized = true;
        }

        private void Start()
        {
            var collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = hitRadius;

            Destroy(gameObject, lifeSeconds);
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            var step = transform.forward * (speed * Time.deltaTime);
            var distance = step.magnitude;
            if (distance <= 0.0001f)
            {
                return;
            }

            if (Physics.SphereCast(
                    transform.position,
                    hitRadius,
                    step.normalized,
                    out var hit,
                    distance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Collide))
            {
                if (ProcessHit(hit.collider))
                {
                    return;
                }
            }

            transform.position += step;
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessHit(other);
        }

        private bool ProcessHit(Collider other)
        {
            if (_ownerRoot != null && other.transform.IsChildOf(_ownerRoot))
            {
                return false;
            }

            if (other.GetComponent<Projectile>() != null)
            {
                return false;
            }

            if (TryDamageBodyPart(other, out var dealt))
            {
                if (dealt)
                {
                    Destroy(gameObject);
                }

                return dealt;
            }

            if (TryDamageHealth(other))
            {
                Destroy(gameObject);
                return true;
            }

            if (BlocksProjectile(other))
            {
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        /// <summary>
        /// ダメージ対象以外の固体（Wall 等）で弾を止める。
        /// </summary>
        private static bool BlocksProjectile(Collider other)
        {
            return !other.isTrigger;
        }

        private bool TryDamageBodyPart(Collider other, out bool dealt)
        {
            dealt = false;
            var bodyPart = other.GetComponent<BodyPartHealth>();
            if (bodyPart == null || !bodyPart.IsAlive)
            {
                return false;
            }

            var context = new DamageContext
            {
                attacker = _ownerRoot,
                hitRegion = bodyPart.Region
            };
            bodyPart.TakeDamage(damage, in context);
            dealt = true;
            return true;
        }

        private bool TryDamageHealth(Collider other)
        {
            var health = other.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive)
            {
                return false;
            }

            var context = new DamageContext
            {
                attacker = _ownerRoot
            };
            health.TakeDamage(damage, in context);
            return true;
        }
    }
}
