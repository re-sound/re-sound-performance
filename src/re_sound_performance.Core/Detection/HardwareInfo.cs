namespace re_sound_performance.Core.Detection;

public sealed record HardwareInfo(
    CpuVendor CpuVendor,
    string CpuModel,
    int LogicalCores,
    bool HybridCpu,
    GpuVendor GpuVendor,
    string GpuModel,
    int RamGb,
    string OsBuild,
    StorageKind PrimaryStorage)
{
    public static HardwareInfo Unknown { get; } = new(
        CpuVendor.Unknown,
        "Unknown",
        0,
        false,
        GpuVendor.Unknown,
        "Unknown",
        0,
        "Unknown",
        StorageKind.Unknown);
}
