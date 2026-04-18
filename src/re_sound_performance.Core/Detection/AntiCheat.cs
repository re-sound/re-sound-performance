namespace re_sound_performance.Core.Detection;

[Flags]
public enum AntiCheat
{
    None = 0,
    Vanguard = 1 << 0,
    FaceitAc = 1 << 1,
    EasyAntiCheat = 1 << 2,
    BattlEye = 1 << 3
}
