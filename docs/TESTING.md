# Testing guide for Windows 11

This document walks through cloning, building and running re_sound Performance on a Windows 11 machine.

## Prerequisites

- Windows 11 22H2 or newer (24H2 recommended)
- Administrator account (required to modify HKLM registry and services)
- .NET 8 Desktop Runtime and SDK
- Git
- A code editor or IDE (Visual Studio 2022, JetBrains Rider or VS Code with C# Dev Kit)

Optional but recommended:

- Windows Defender exclusion for the clone folder (avoid false-positive scanning during build)
- A fresh Windows 11 VM or a test SSD boot, so you can apply tweaks without risk to your daily setup

## Install prerequisites

Run PowerShell as administrator and use winget:

```powershell
winget install Microsoft.DotNet.SDK.8
winget install Git.Git
winget install Microsoft.VisualStudio.2022.Community
```

If you only need the SDK to build from the command line:

```powershell
winget install Microsoft.DotNet.DesktopRuntime.8
winget install Microsoft.DotNet.SDK.8
```

## Clone the repository

```powershell
cd $env:USERPROFILE\source
git clone https://github.com/re-sound/re-sound-performance.git
cd re-sound-performance
```

## Build from the command line

```powershell
cd src
dotnet restore
dotnet build --configuration Release
```

Expected output ends with `Compilacion correcta` (or `Build succeeded` on English locales) and zero warnings.

## Run the unit tests

```powershell
dotnet test --configuration Release --no-build
```

Expected output:

```
Correctas! - Con error:     0, Superado:    17, Omitido:     0, Total:    17
```

All 17 tests must pass before you launch the UI.

## Launch the app

```powershell
dotnet run --project re_sound_performance --configuration Release
```

A UAC prompt appears. Accept it. The main window opens with the NavigationView on the left and the Home page on the right.

If you open the solution from Visual Studio or Rider, right-click the `re_sound_performance` project and choose "Set as Startup Project", then press F5.

## First test scenario: apply and revert a safe tweak

The Disable Xbox Game Bar tweak is the ideal starter because it is reversible and has no anti-cheat or stability risk.

1. Launch the app with administrator rights.
2. Click **System tweaks** on the sidebar.
3. Locate the **Disable Xbox Game Bar** card.
4. Hover over the `?` icon. The tooltip should list the description, registry path, expected impact and source.
5. Flip the toggle to **On**. The card should update without errors.
6. Open Registry Editor (`regedit.exe`) and navigate to `HKEY_CURRENT_USER\Software\Microsoft\GameBar`. Confirm `UseNexusForGameBarEnabled` = `0` (DWORD).
7. Flip the toggle to **Off**. Re-read the registry value. It should either return to its original state or be deleted.
8. Verify that no other keys or files outside the expected registry path were touched (use ProcMon if you want to be thorough).

## Second test scenario: multi-value registry tweak

Try **Disable Game DVR and FSE hook**:

1. Toggle it **On**.
2. Verify all five changes in `regedit.exe`:
   - `HKCU\System\GameConfigStore\GameDVR_Enabled` = 0
   - `HKCU\System\GameConfigStore\GameDVR_FSEBehaviorMode` = 2
   - `HKCU\System\GameConfigStore\GameDVR_HonorUserFSEBehaviorMode` = 1
   - `HKCU\System\GameConfigStore\GameDVR_DXGIHonorFSEWindowsCompatible` = 1
   - `HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR\AllowGameDVR` = 0
3. Toggle **Off** and confirm every value is reverted.

## Third test scenario: service tweak

Try **Disable DiagTrack and WAP Push services**:

1. Toggle **On**.
2. Open `services.msc`. Locate Connected User Experiences and Telemetry (DiagTrack). Startup type should be Disabled.
3. The service might still be running if it started at boot. A restart will apply the new startup type.
4. Toggle **Off**. Confirm the startup type is restored to Automatic.

## Fourth test scenario: power plan

Try **Enable Ultimate Performance power plan**:

1. Toggle **On**.
2. Open Control Panel -> Power Options. Confirm a new plan called "Ultimate Performance" exists and is selected.
3. Toggle **Off**. The previously active plan should be restored (Balanced is the fallback if no backup is readable).

## Fifth test scenario: verify backups

Browse to `%LOCALAPPDATA%\re_sound_performance\backups`. For each tweak you applied, there should be a folder with the tweak id containing a timestamped `registry.json` or `services.json` payload.

Open one of the JSON files to inspect the snapshot format.

## Known limitations at this stage

- The UI currently only populates the System tweaks page. Privacy, Debloat and GPU pages are placeholders.
- Per-game tabs are not implemented yet.
- Benchmarking is not implemented yet.
- No auto-update mechanism yet.
- Windows SmartScreen may warn because the build is not code-signed. Right-click the exe and choose "Run as administrator" -> "More info" -> "Run anyway" for testing.

## Reporting issues

If you hit a bug, open an issue at https://github.com/re-sound/re-sound-performance/issues using the Bug Report template. Include:

- Windows build (`winver`)
- App commit hash (`git rev-parse HEAD`)
- GPU and CPU model
- Steps to reproduce
- `%LOCALAPPDATA%\re_sound_performance\logs\latest.log` contents (if logs are being written yet; logging infrastructure is still basic)

## Reverting all changes

If something goes wrong and you need to roll back everything:

1. Open the app, navigate to Settings -> Advanced -> Revert all applied tweaks (this UI is pending). Until that menu exists, revert each tweak individually via its toggle.
2. As a last resort, open Control Panel -> System -> System Protection and roll back to the restore point labeled `re_sound_performance_pre_<timestamp>` (restore point creation UI is pending too; manual `RestorePointManager` invocation is available via code right now).

## What to focus on during this testing session

Priority order:

1. Confirm registry and service tweaks apply and revert cleanly.
2. Confirm hover tooltips render correctly with all five fields populated.
3. Confirm NavigationView sidebar navigation works without visual glitches.
4. Confirm the app survives alt-tab and window resize without crashing.
5. Try applying 2 or 3 safe tweaks, reboot, apply 2 or 3 more, reboot again. Every tweak should survive reboots and the revert button should still work.

## Collecting feedback for the next iteration

Keep notes on:

- Which tweak descriptions are confusing
- Where the tooltip placement feels wrong
- Any MissingRegistryKey exceptions (some HKLM paths may not exist on a fresh install; the helper should handle this gracefully; if it does not, report it)
- Any UI elements that feel unresponsive after an apply operation (the Loaded handler re-probes everything which can be slow)

These notes feed into the priorities in `ROADMAP.md`.
