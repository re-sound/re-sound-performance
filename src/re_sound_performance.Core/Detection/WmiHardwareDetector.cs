using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace re_sound_performance.Core.Detection;

[SupportedOSPlatform("windows")]
public sealed class WmiHardwareDetector : IHardwareDetector
{
    public Task<HardwareInfo> DetectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cpu = QueryCpu();
            var gpu = QueryGpu();
            var ramGb = QueryRamGb();
            var os = QueryOsBuild();
            var storage = QueryPrimaryStorage();

            return Task.FromResult(new HardwareInfo(
                cpu.Vendor,
                cpu.Model,
                cpu.LogicalCores,
                cpu.Hybrid,
                gpu.Vendor,
                gpu.Model,
                ramGb,
                os,
                storage));
        }
        catch (ManagementException)
        {
            return Task.FromResult(HardwareInfo.Unknown);
        }
        catch (COMException)
        {
            return Task.FromResult(HardwareInfo.Unknown);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(HardwareInfo.Unknown);
        }
    }

    private static (CpuVendor Vendor, string Model, int LogicalCores, bool Hybrid) QueryCpu()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, NumberOfLogicalProcessors, Architecture FROM Win32_Processor");
        foreach (var obj in searcher.Get())
        {
            var model = (obj["Name"] as string)?.Trim() ?? "Unknown";
            var manufacturer = (obj["Manufacturer"] as string) ?? string.Empty;
            var vendor = ClassifyCpuVendor(manufacturer, model);
            var logical = obj["NumberOfLogicalProcessors"] is uint lp ? (int)lp : 0;
            var hybrid = DetectHybridCpu(vendor, model);
            return (vendor, model, logical, hybrid);
        }

        return (CpuVendor.Unknown, "Unknown", 0, false);
    }

    private static (GpuVendor Vendor, string Model) QueryGpu()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterCompatibility FROM Win32_VideoController");
        GpuVendor best = GpuVendor.Unknown;
        string bestModel = "Unknown";

        foreach (var obj in searcher.Get())
        {
            var model = (obj["Name"] as string)?.Trim() ?? "Unknown";
            var vendor = ClassifyGpuVendor(obj["AdapterCompatibility"] as string, model);
            if (Priority(vendor) > Priority(best))
            {
                best = vendor;
                bestModel = model;
            }
        }

        return (best, bestModel);
    }

    private static int QueryRamGb()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
        ulong total = 0;
        foreach (var obj in searcher.Get())
        {
            if (obj["Capacity"] is ulong cap)
            {
                total += cap;
            }
        }

        return (int)Math.Round(total / 1024.0 / 1024.0 / 1024.0);
    }

    private static string QueryOsBuild()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Caption, BuildNumber FROM Win32_OperatingSystem");
        foreach (var obj in searcher.Get())
        {
            var caption = (obj["Caption"] as string)?.Trim() ?? string.Empty;
            var build = (obj["BuildNumber"] as string)?.Trim() ?? string.Empty;
            return string.IsNullOrEmpty(build) ? caption : $"{caption} (build {build})";
        }

        return "Unknown";
    }

    private static StorageKind QueryPrimaryStorage()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"\\.\root\microsoft\windows\storage", "SELECT MediaType, BusType FROM MSFT_PhysicalDisk");
            StorageKind best = StorageKind.Unknown;
            foreach (var obj in searcher.Get())
            {
                var mediaType = obj["MediaType"] is ushort mt ? mt : (ushort)0;
                var busType = obj["BusType"] is ushort bt ? bt : (ushort)0;

                var kind = (mediaType, busType) switch
                {
                    (4, 17) => StorageKind.Nvme,
                    (4, _) => StorageKind.Ssd,
                    (3, _) => StorageKind.Hdd,
                    (_, 17) => StorageKind.Nvme,
                    _ => StorageKind.Unknown
                };

                if (Priority(kind) > Priority(best))
                {
                    best = kind;
                }
            }

            return best;
        }
        catch (ManagementException)
        {
            return StorageKind.Unknown;
        }
    }

    private static CpuVendor ClassifyCpuVendor(string manufacturer, string model)
    {
        if (manufacturer.Contains("Intel", StringComparison.OrdinalIgnoreCase) ||
            model.Contains("Intel", StringComparison.OrdinalIgnoreCase))
        {
            return CpuVendor.Intel;
        }

        if (manufacturer.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
            model.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
            model.Contains("Ryzen", StringComparison.OrdinalIgnoreCase))
        {
            return CpuVendor.Amd;
        }

        return CpuVendor.Unknown;
    }

    private static bool DetectHybridCpu(CpuVendor vendor, string model)
    {
        if (vendor != CpuVendor.Intel)
        {
            return false;
        }

        var hybridFamilies = new[] { "12th Gen", "13th Gen", "14th Gen", "Core Ultra" };
        foreach (var family in hybridFamilies)
        {
            if (model.Contains(family, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static GpuVendor ClassifyGpuVendor(string? compat, string model)
    {
        var candidates = new[] { compat ?? string.Empty, model };
        foreach (var candidate in candidates)
        {
            if (candidate.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
            {
                return GpuVendor.Nvidia;
            }

            if (candidate.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
                candidate.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
            {
                return GpuVendor.Amd;
            }

            if (candidate.Contains("Intel", StringComparison.OrdinalIgnoreCase))
            {
                return GpuVendor.Intel;
            }
        }

        return GpuVendor.Unknown;
    }

    private static int Priority(GpuVendor vendor) => vendor switch
    {
        GpuVendor.Nvidia => 3,
        GpuVendor.Amd => 2,
        GpuVendor.Intel => 1,
        _ => 0
    };

    private static int Priority(StorageKind kind) => kind switch
    {
        StorageKind.Nvme => 3,
        StorageKind.Ssd => 2,
        StorageKind.Hdd => 1,
        _ => 0
    };
}

