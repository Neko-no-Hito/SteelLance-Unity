using SteelLance.Data;
using UnityEngine;

namespace SteelLance.Combat
{
    public struct DamageContext
    {
        public Transform attacker;
        public BodyRegion? hitRegion;
    }

    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount, in DamageContext context);
    }
}
