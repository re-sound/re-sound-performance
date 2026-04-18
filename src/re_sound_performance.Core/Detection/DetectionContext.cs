namespace re_sound_performance.Core.Detection;

public sealed class DetectionContext
{
    private HardwareInfo? _hardware;
    private AntiCheatInfo? _antiCheat;
    private readonly object _lock = new();

    public event EventHandler? Changed;

    public HardwareInfo? Hardware
    {
        get { lock (_lock) { return _hardware; } }
    }

    public AntiCheatInfo? AntiCheat
    {
        get { lock (_lock) { return _antiCheat; } }
    }

    public void SetHardware(HardwareInfo info)
    {
        lock (_lock) { _hardware = info; }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetAntiCheat(AntiCheatInfo info)
    {
        lock (_lock) { _antiCheat = info; }
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
