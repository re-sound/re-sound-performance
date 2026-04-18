# re_sound Performance Roadmap

This document tracks the state of the project and the ordered list of pending work.

## Current state (snapshot)

- Repository live at https://github.com/re-sound/re-sound-performance
- Apache 2.0 license
- .NET 8 WPF solution with three projects (main app, Core engine, tests)
- 23 tweaks registered in the catalog
- 17 xunit tests passing on Linux build
- CI workflow (windows-latest) builds, runs tests and checks `dotnet format`
- Complete research knowledge base in `docs/research/` (13 documents, ~65000 words)

### Tweak catalog (23 tweaks)

Category | Count | Tweaks
---------|-------|-------
System | 9 | Xbox Game Bar, Game DVR + FSE, MPO, MMCSS SystemResponsiveness, Edge Startup Boost, Maps Broker, Fax/RetailDemo/WMP/Biometric, Xbox services (Medium), WER + SysMain (Medium)
Input | 2 | Enhance Pointer Precision, Sticky/Filter/Toggle Keys
Privacy | 8 | Telemetry, Activity History, Advertising ID, Copilot + Recall, Tips & Consumer Features, Bing and Web Search, Location Tracking, DiagTrack services
Network | 1 | NDU (non-paged pool memory leak fix)
Power | 2 | Ultimate Performance plan, Hibernation + Fast Startup
GPU | 1 | DirectX Shader Cache 10 GB

### Abstractions in place

Domain | Abstraction | Helper | Notes
-------|-------------|--------|-------
Registry | IRegistryAccess | RegistryTweakHelper | Multi-value Apply/Probe/Revert with JSON snapshot backup
Services | IServiceManager | ServiceTweakHelper | Backed by registry writes on HKLM SYSTEM CurrentControlSet Services
Power | IPowerCfgRunner | (inline in tweaks) | Wraps powercfg.exe via Process.Start
Backup | IBackupStore + FileSystemBackupStore | | Granular per-tweak backups under LocalAppData
System Restore | RestorePointManager | | WMI SystemRestore wrapper

---

## Next steps (ordered by priority)

### 1. Hardware and anti-cheat detection (required before more UI work)

**Goal:** provide context so the UI can filter tweaks and block dangerous combinations.

Files to create:

- `Core/Detection/HardwareInfo.cs` (record: CPU vendor, CPU family hybrid flag, GPU vendor, GPU model, OS build, RAM, primary storage type)
- `Core/Detection/IHardwareDetector.cs`
- `Core/Detection/WmiHardwareDetector.cs` (uses System.Management WMI: Win32_Processor, Win32_VideoController, Win32_PhysicalMemory, Win32_DiskDrive)
- `Core/Detection/AntiCheatStatus.cs` (enum: None, Vanguard, FaceitAc, Eac, BattleEye, Multiple)
- `Core/Detection/IAntiCheatDetector.cs`
- `Core/Detection/WindowsAntiCheatDetector.cs` (checks: services vgc/vgk presence, C:\Program Files\Riot Vanguard folder, EasyAntiCheat installations under common game paths, BattlEye services)
- Tests for both detectors with mocked dependencies

Register in DI. Populate `HomePage` with detected info.

### 2. Scheduled Tasks abstraction

**Goal:** disable telemetry and feedback scheduled tasks known to cause background CPU.

Files to create:

- `Core/Tasks/IScheduledTaskManager.cs` (Exists, GetState, Enable, Disable)
- `Core/Tasks/WindowsScheduledTaskManager.cs` (wraps `schtasks.exe` or uses `Microsoft.Win32.TaskScheduler` NuGet package)
- `Core/Tasks/ScheduledTaskChange.cs` (Path, TargetState)
- `Core/Tasks/ScheduledTaskHelper.cs` (Apply/Probe/Revert pattern like Services)
- `Tests/Tasks/InMemoryScheduledTaskManager.cs` and `ScheduledTaskHelperTests.cs`

New tweaks:

- `privacy.disable_telemetry_scheduled_tasks` (Microsoft\Windows\Application Experience\ProgramDataUpdater, StartupAppTask, Microsoft Compatibility Appraiser, Microsoft-Windows-DiskDiagnosticDataCollector, Microsoft\Windows\Customer Experience Improvement Program\Consolidator, UsbCeip, KernelCeipTask, Microsoft\Windows\Feedback\Siuf\DmClient, DmClientOnScenarioDownload, QueueReporting)
- `system.disable_update_orchestrator_tasks` (optional, Medium risk)

### 3. UWP apps (Appx) abstraction for the Debloat tab

**Goal:** remove stock UWP bloat (Teams consumer, Clipchamp, Bing apps, Xbox app when user does not use it).

Files to create:

- `Core/Appx/IAppxManager.cs` (GetInstalled, RemoveProvisioned, RemoveForCurrentUser)
- `Core/Appx/PowerShellAppxManager.cs` (spawns pwsh.exe with `Get-AppxPackage`, `Remove-AppxPackage`, `Get-AppxProvisionedPackage`, `Remove-AppxProvisionedPackage`)
- `Core/Appx/AppxPackage.cs`
- `Core/Appx/AppxTweakHelper.cs` (Apply/Probe/Revert via re-provision does not fully restore - backup stores original package family names)
- Tests with InMemoryAppxManager

New tweaks (one per package family grouped by purpose):

- `debloat.remove_teams_consumer` (MicrosoftTeams)
- `debloat.remove_clipchamp` (Clipchamp.Clipchamp)
- `debloat.remove_bing_apps` (Microsoft.BingNews, Microsoft.BingWeather, Microsoft.BingSearch)
- `debloat.remove_xbox_apps_consumer` (Microsoft.XboxApp, Microsoft.GamingApp, Microsoft.Xbox.TCUI, Microsoft.XboxGamingOverlay, Microsoft.XboxIdentityProvider, Microsoft.XboxSpeechToTextOverlay)
- `debloat.remove_onedrive` (Medium risk, requires `%SystemRoot%\SysWOW64\OneDriveSetup.exe /uninstall` in addition to Appx)
- `debloat.remove_copilot_app` (Microsoft.Copilot)
- `debloat.remove_recall_app` (Microsoft.Windows.AI.Copilot)
- `debloat.remove_stock_annoyances` (Microsoft.GetHelp, Microsoft.Getstarted, Microsoft.WindowsFeedbackHub, Microsoft.MixedReality.Portal)

### 4. Wire Privacy, Debloat and GPU pages

**Goal:** the Privacy, Debloat and GPU navigation items already exist. They are empty placeholders. Reuse the `SystemTweaksPage` loading pattern filtered by the appropriate `TweakCategory`.

No new tweaks required. Pattern copy:

```csharp
foreach (var tweak in _engine.AvailableTweaks)
{
    if (tweak.Metadata.Category != TweakCategory.Privacy) continue;
    ...
}
```

### 5. Preset selector on HomePage

**Goal:** one-click application of curated tweak sets.

Data structure:

- `Core/Presets/Preset.cs` (Name, Description, TweakIds list, RiskLevel)
- `Core/Presets/PresetCatalog.cs` (static list: Safe, Balanced, Competitive)
- `Core/Presets/PresetRunner.cs` (applies sequence of tweaks with progress reporting)

Preset content draft:

Preset | Included tweaks
-------|----------------
Safe | Only risk=Safe tweaks, no anti-cheat-risky changes. Example: Xbox Game Bar, Game DVR, MPO, mouse acceleration, accessibility shortcuts, telemetry, activity history, advertising ID, tips, web search, shader cache, Ndu, Edge Startup Boost, Maps Broker, Fax/RetailDemo legacy services
Balanced | Safe + Ultimate Performance, Disable Hibernation, SystemResponsiveness for gaming, DiagTrack services
Competitive | Balanced + Copilot/Recall + Xbox services (when user confirms no Game Pass use) + location tracking disable + additional per-game profile import

HomePage receives three cards with preview dialog before apply. Progress bar with cancellation.

### 6. Per-game tabs (CS2, Valorant, Apex)

**Goal:** dedicated pages with launch options editors, autoexec templates and pro config import.

Approach:

- `Core/Games/GameProfile.cs` (GameId, Name, AntiCheat, SupportedConfigs)
- `Core/Games/IGameDetector.cs` (detects Steam path for cs2.exe, Riot path for VALORANT-Win64-Shipping.exe, Origin/EA path for r5apex.exe)
- `Core/Games/Cs2/Cs2AutoexecTemplate.cs`, `Cs2LaunchOptionsBuilder.cs`, `Cs2ProConfigs.cs`
- `Core/Games/Valorant/ValorantGameUserSettingsEditor.cs`, `ValorantEngineIniEditor.cs`, `ValorantProConfigs.cs`, `ValorantVanguardCompatChecker.cs`
- `Core/Games/Apex/ApexLaunchOptionsBuilder.cs`, `ApexVideoConfigEditor.cs`, `ApexAutoexecTemplate.cs`, `ApexProConfigs.cs`, `ApexLauncherDetector.cs` (Steam vs EA App)

Reference source: `docs/research/06-cs2-optimization.md`, `07-valorant-optimization.md`, `08-apex-optimization.md`.

Special handling:

- Valorant page must show the Vanguard compatibility matrix and block VBS/HVCI toggle access if Valorant is installed. Never apply a tweak that would trigger `VAN:RESTRICTION`.
- Apex page detects launcher (Steam vs EA App) and warns EA App users that `+` launch options do not work.
- All three tabs include pro config import with attribution (ProSettings.net linked).

### 7. Benchmarking integration

**Goal:** before/after measurement using Intel PresentMon 2.0.

Files:

- `Core/Benchmark/IPresentMonRunner.cs`
- `Core/Benchmark/PresentMonCli.cs` (downloads or locates PresentMon-2.0+ binary, parses CSV v2 schema)
- `Core/Benchmark/FrameStats.cs` (record: FpsAvg, OnePercentLow, ZeroPointOnePercentLow, PcLatencyMs, GpuBusyPercent, stddev)
- `Core/Benchmark/FrameStatsCalculator.cs` (percentile algorithms with both "average" and "integral" conventions)
- `Core/Benchmark/BenchmarkComparer.cs` (weighted verdict: 30% avg + 50% 1% low + 20% latency)
- `BenchmarkPage.xaml` with ScottPlot live graph and wizard (baseline -> apply tweaks -> post)

Reference source: `docs/research/11-benchmarking-integracion.md`.

### 8. Auto-update and signed builds

**Goal:** version 1.0 release distribution pipeline.

Tasks:

- Apply to SignPath Foundation for OSS code signing certificate (free)
- Integrate Velopack for delta auto-updates
- Add `install.ps1` one-liner installer script
- Submit to winget community repository
- Document three distribution channels (one-liner, portable, installer) on README

Reference source: `docs/research/13-ecosistema-produccion.md`.

### 9. Future enhancements (lower priority)

- PresentMon telemetry opt-in (anonymous hardware + tweak deltas)
- Community profiles (GitHub-backed JSON pro configs for top players, refreshed via CI)
- Internationalization: start with English and Spanish resource files
- Accessibility: screen reader labels on all TweakCard elements
- Theme switching (Light/Dark/System) in Settings page
- Configurable `?` tooltip delay
- Export applied tweaks as shareable JSON (with redacted hardware IDs)
- Crash reporter with Sentry (self-host or free tier)

---

## Out of scope (explicit non-goals)

- BIOS tuning and flashing. Too risky for an unattended tool. Research preserved in `docs/research/12-bios-cpu-ram-tuning.md` for future reference.
- CPU undervolt wizards (ThrottleStop, XTU, PBO2 Tuner automation). Same reason.
- RAM timing automation. Belongs in a separate expert-only tool.
- Overclocking scanner. Not the product we want.
- Kernel-mode drivers. Unsigned kernel code brings SmartScreen and anti-cheat blocklisting risk.
- Selling paid tiers. Project stays Apache 2.0, GitHub-hosted, donation-funded.

---

## Reference

- Research knowledge base: `docs/research/` (13 MDs, 65000 words)
- Testing guide: `docs/TESTING.md`
- License: `LICENSE` (Apache 2.0)
- Contributing guide: `CONTRIBUTING.md`
- Security policy: `SECURITY.md`
