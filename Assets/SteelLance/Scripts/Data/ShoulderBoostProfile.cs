using System;

namespace SteelLance.Data
{
    /// <summary>
    /// ShoulderL/R frame boost baseline. Values are Play-tuned (Phase4+ HeatSystem).
    /// </summary>
    [Serializable]
    public class ShoulderBoostProfile
    {
        public float thrust;
        public float boostDuration;
        public float boostCooldown;
    }
}
