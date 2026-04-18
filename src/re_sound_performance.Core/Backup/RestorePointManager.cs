using System.Management;

namespace re_sound_performance.Core.Backup;

public sealed class RestorePointManager
{
    private const int BeginSystemChange = 100;
    private const int ApplicationInstall = 0;

    public void CreateRestorePoint(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var managementPath = new ManagementPath
        {
            Server = Environment.MachineName,
            NamespacePath = "\\\\.\\root\\default",
            RelativePath = "SystemRestore"
        };

        using var systemRestoreClass = new ManagementClass(managementPath);
        using var inputParameters = systemRestoreClass.GetMethodParameters("CreateRestorePoint");
        inputParameters["Description"] = description;
        inputParameters["RestorePointType"] = ApplicationInstall;
        inputParameters["EventType"] = BeginSystemChange;

        using var result = systemRestoreClass.InvokeMethod("CreateRestorePoint", inputParameters, null);
        var returnValue = Convert.ToInt32(result?["ReturnValue"] ?? -1);
        if (returnValue != 0)
        {
            throw new InvalidOperationException($"CreateRestorePoint failed with code {returnValue}.");
        }
    }
}
