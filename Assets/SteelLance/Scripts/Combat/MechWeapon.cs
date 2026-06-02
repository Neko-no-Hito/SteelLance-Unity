using SteelLance.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SteelLance.Combat
{
    /// <summary>
    /// 左クリック射撃。Muzzle 位置から Projectile を生成。照準はカメラ水平方向。
    /// </summary>
    public class MechWeapon : MonoBehaviour
    {
        private const float DefaultFireCooldown = 0.25f;

        [SerializeField] private BodyRegion bodyRegion = BodyRegion.ArmR;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform aimReference;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float fireCooldown = DefaultFireCooldown;

        private MechPartSystem _partSystem;
        private float _nextFireTime;
        private float _damage = -1f;

        public BodyRegion BodyRegion => bodyRegion;

        public void BindPartSystem(MechPartSystem partSystem)
        {
            _partSystem = partSystem;
        }

        public void ApplyWeaponProfile(WeaponProfile profile, EquipmentStats stats)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.projectilePrefab != null)
            {
                var projectile = profile.projectilePrefab.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectilePrefab = projectile;
                }
            }

            if (profile.cooldown > 0f)
            {
                fireCooldown = profile.cooldown;
            }

            if (stats != null && stats.damage > 0f)
            {
                _damage = stats.damage;
            }
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (Time.time < _nextFireTime || projectilePrefab == null || muzzle == null)
            {
                return;
            }

            var fireRateMultiplier = _partSystem != null
                ? _partSystem.GetWeaponFireRateMultiplier(bodyRegion)
                : 1f;
            if (fireRateMultiplier <= 0f)
            {
                return;
            }

            Fire();
            var effectiveCooldown = fireCooldown / fireRateMultiplier;
            _nextFireTime = Time.time + effectiveCooldown;
        }

        private void Fire()
        {
            var direction = GetAimDirection();
            var projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
            var damageOverride = _damage >= 0f ? _damage : -1f;
            projectile.Initialize(transform.root, direction, damageOverride);
        }

        private Vector3 GetAimDirection()
        {
            if (aimReference == null)
            {
                return transform.root.forward;
            }

            var forward = aimReference.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : transform.root.forward;
        }
    }
}
