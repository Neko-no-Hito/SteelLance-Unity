namespace SteelLance.Data
{
    public enum BodyRegion
    {
        Head,
        Torso,
        ArmL,
        ArmR,
        ShoulderL,
        ShoulderR,
        Legs
    }

    public enum PartKind
    {
        BodyFrame,
        Equipment
    }

    public enum SlotType
    {
        Weapon,
        Engine,
        Thruster,
        Radar,
        ECM,
        Shield,
        Drone,
        AmmoBox
    }

    public enum EquipmentClass
    {
        ArmWeapon,
        ShoulderHeavy,
        ShoulderUtility,
        TorsoModule,
        HeadModule,
        LegModule
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum WeightClass
    {
        Light40,
        Medium60,
        Heavy90
    }

    public enum PartCondition
    {
        Intact,
        Damaged,
        Destroyed
    }

    public enum MechDefeatReason
    {
        TorsoDestroyed,
        HeadDestroyed,
        LegsDestroyed,
        AllWeaponsLost
    }

    public enum BuildValidationError
    {
        MissingFrame,
        EmptyRequiredSlot,
        SlotMismatch,
        OverWeight,
        DuplicateInstance,
        TorsoDestroyed
    }
}
