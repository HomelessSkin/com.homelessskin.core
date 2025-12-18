namespace Core
{
    public enum AnimationType : int
    {
        DoublePunch = -823312553,
        TPose = 1580386860,
        Null = 0,
        Ragdoll = 1,
        MeleeWeaponAttack = 1392970538,
        Jump = -1476058720,
        Run = -1911500099,
        Punch = 1369840587,
        Walk = 2142974238,
        Idle = 575354460,
    }

    public enum EntityStateType : int
    {
        OnGround = -1674559289,
        OnAir = -44253493,
        HeadRight = -2109563161,
        HeadLeft = -57071264,
        Poisoned = 1662354341,
        Alive = -129890457,
        Dead = 1602368346,
        Run = -1729565887,
        Jump = 907782767,
    }

    public enum ParameterType : byte
    {
        Sweet = 0,
        Sour = 1,
        Leathery = 2,
        Watery = 3,

        Size = 100,
        Count = 101,
        Points = 102,
    }
}