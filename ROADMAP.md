# re_sound Performance Roadmap

This document tracks the state of the project and the ordered list of pending work. Read this first when picking the project up again.

## Current state (snapshot, 2026-04-18)

- Repository: https://github.com/re-sound/re-sound-performance (Apache 2.0)
- .NET 8 WPF solution, three projects: main app, `Core` engine, xunit tests
- **33 tweaks** registered across System, Privacy, Debloat, GPU, Input, Network, Power categories
- **30 xunit tests** passing (Linux + Windows CI)
- CI workflow builds, runs tests, runs `dotnet format`
- Release workflow publishes portable + framework-dependent ZIPs on every `v*` tag
- Complete research knowledge base in `docs/research/` (13 documents, ~65000 words)

### Releases

Tag | Date | Notes
----|------|-------
`v0.0.1-test` | 2026-04-?? | First public build, 23 tweaks, post-apply probe
`v0.0.2-test` | 2026-04-18 | Post-apply verifier pill, UI polish, +10 tweaks (schtasks + Appx), Privacy/Debloat/GPU pages wired
`v0.0.3-test` | 2026-04-18 | Single-instance guard, TweakStateCache (no more 1-by-1 reload), UsoSvc tweak, UI redesign (Synapse-style violet `#7C3AED`)

### Abstractions in place

Domain | Abstraction | Helper | Backing
-------|-------------|--------|--------
Registry | `IRegistryAccess` | `RegistryTweakHelper` | `Microsoft.Win32`
Services | `IServiceManager` | `ServiceTweakHelper` | HKLM registry writes
Power | `IPowerCfgRunner` | (inline) | `powercfg.exe` via `Process.Start`
Scheduled tasks | `IScheduledTaskManager` (with `TrySetState`/`SetStateOutcome`) | `ScheduledTaskHelper` | `schtasks.exe`
UWP (Appx) | `IAppxManager` | — | `powershell.exe` (Get/Remove-AppxPackage, Remove-AppxProvisionedPackage)
Backup | `IBackupStore` + `FileSystemBackupStore` | | Per-tweak JSON under LocalAppData
System Restore | `RestorePointManager` | | WMI SystemRestore
**Tweak state cache (new in v0.0.3)** | `TweakStateCache` | | `ConcurrentDictionary` + `StateChanged`/`ProbingProgress` events
**Single-instance guard (new in v0.0.3)** | `SingleInstanceGuard` | | `Local\` Mutex + `FindWindow`/`SetForegroundWindow`

### Non-obvious decisions worth remembering

- **Appx revert is informational, not a true reverse.** Once `Remove-AppxProvisionedPackage` runs the package cannot be restored without the original `.appx`. Revert returns `NotApplied` with a message pointing at the Microsoft Store.
- **OneDrive debloat is deferred.** Needs a Win32 uninstaller abstraction (`%SystemRoot%\SysWOW64\OneDriveSetup.exe /uninstall`). Placeholder slot still shown in the research doc; `RemoveLegacyMediaAppsTweak` fills the 8th Appx slot in the meantime.
- **`DisableUpdateOrchestratorTasksTweak` keeps returning `Unavailable`.** Those tasks are protected by TrustedInstaller. The helper detects Access Denied and produces a friendly message ("use a service-based tweak instead") instead of red `Error`. `DisableUsoSvcTweak` is the working alternative (disables `UsoSvc` + `WaaSMedicSvc`, same practical effect, works with plain Administrator).
- **Probes run in parallel once at startup** via `TweakEngine.ProbeAllAsync` fire-and-forget from `App.OnStartup`. Cards read from `TweakStateCache` synchronously and subscribe to `StateChanged`, so tab switches are instant and never redraw 1-by-1 again.
- **Mutex uses `Local\` namespace, not `Global\`.** Single-instance is per user/session, which is what we want; `Global\` would need explicit SID access rights even for an elevated app.
- **Failed pill for probe-mismatch is intentional.** If `ApplyAsync` returns Success but the post-apply probe says `NotApplied`, we mark as `Failed` with "Post-apply verification mismatch: changes did not take effect." — catches GPO / anti-cheat overrides.
- **UI design tokens live in `Themes/Colors.xaml` and `Themes/Styles.xaml`.** No more hardcoded RGB inside controls. Category colors, risk colors, card surfaces and typography are `StaticResource` keys.

### Tweak catalog (33 tweaks)

Category | Count | Representative tweaks
---------|-------|----------------------
System | 10 | Xbox Game Bar, Game DVR + FSE, MPO, MMCSS SystemResponsiveness, Edge Startup Boost, Maps Broker, Fax/RetailDemo, Xbox services (Medium), WER + SysMain (Medium), **UsoSvc (new)**
Input | 2 | Enhance Pointer Precision, Sticky/Filter/Toggle Keys
Privacy | 10 | Telemetry, Activity History, Advertising ID, Copilot + Recall, Tips, Bing/Web Search, Location Tracking, DiagTrack services, Telemetry schtasks, Update Orchestrator schtasks (Unavailable w/o TrustedInstaller)
Network | 1 | NDU non-paged pool fix
Power | 2 | Ultimate Performance plan, Hibernation + Fast Startup
GPU | 1 | DirectX Shader Cache 10 GB
Debloat | 8 | Teams consumer, Clipchamp, Bing apps, Xbox consumer, Copilot, Recall/AI, Stock annoyances, Legacy media (Groove/Movies/Solitaire/OneNote)

---

## Next sprint — pick one when resuming

### 1. Hardware + anti-cheat detection *(unblocks presets and per-game tabs)*

Files to create:

- `Core/Detection/HardwareInfo.cs` — record: CPU vendor, hybrid flag, GPU vendor/model, OS build, RAM GB, primary storage kind (HDD/SSD/NVMe)
- `Core/Detection/IHardwareDetector.cs`, `WmiHardwareDetector.cs` — uses `System.Management` WMI (`Win32_Processor`, `Win32_VideoController`, `Win32_PhysicalMemory`, `Win32_DiskDrive`)
- `Core/Detection/AntiCheatStatus.cs` — enum `None | Vanguard | FaceitAc | Eac | BattleEye | Multiple`
- `Core/Detection/IAntiCheatDetector.cs`, `WindowsAntiCheatDetector.cs` — checks for services `vgc`/`vgk`, folder `C:\Program Files\Riot Vanguard`, EasyAntiCheat under common game install paths, BattlEye services
- Tests with mocked registry/file/service dependencies

Wire into DI, surface on `HomePage` above the stat row, and enforce `Metadata.BlockedWhenVanguardInstalled` / `BlockedWhenFaceitInstalled` in the toggle handler (right now the flags exist but nothing reads them).

### 2. Preset selector on HomePage

Files to create:

- `Core/Presets/Preset.cs`, `PresetCatalog.cs` (Safe / Balanced / Competitive), `PresetRunner.cs`
- HomePage: three preset cards under the hero with preview dialog + progress bar + cancellation

Draft preset content is in the previous roadmap entry; reuse.

### 3. Per-game tabs (CS2, Valorant, Apex)

Research ready in `docs/research/06-cs2-optimization.md`, `07-valorant-optimization.md`, `08-apex-optimization.md`. Key files:

- `Core/Games/{Cs2,Valorant,Apex}/...` autoexec / GameUserSettings / videoconfig editors and pro config import
- `Core/Games/IGameDetector.cs` (Steam path for cs2.exe, Riot path for VALORANT-Win64-Shipping.exe, Origin/EA for r5apex.exe)
- Valorant page must show Vanguard compatibility matrix and refuse VBS/HVCI toggles when Valorant is installed (prevents `VAN:RESTRICTION`).
- Apex page must detect launcher (Steam vs EA App) and warn EA App users that `+` launch options are ignored.

### 4. Benchmarking integration (Intel PresentMon 2.0)

Research: `docs/research/11-benchmarking-integracion.md`. Files:

- `Core/Benchmark/IPresentMonRunner.cs`, `PresentMonCli.cs` (CSV v2 schema)
- `FrameStats.cs`, `FrameStatsCalculator.cs` (avg + integral 1% low conventions)
- `BenchmarkComparer.cs` — weighted verdict: 30% avg + 50% 1% low + 20% latency
- `BenchmarkPage.xaml` with ScottPlot live graph and wizard (baseline → apply → post)

### 5. Auto-update + signed builds (towards v1.0)

Research: `docs/research/13-ecosistema-produccion.md`.

- Apply to SignPath Foundation for free OSS code signing
- Integrate Velopack for delta auto-updates
- `install.ps1` one-liner
- Submit to winget community repo
- Document three distribution channels on README

### 6. Future enhancements (lower priority)

- PresentMon telemetry opt-in (anonymous hardware + tweak deltas)
- Community profiles (GitHub-backed JSON pro configs, refreshed via CI)
- i18n: English + Spanish resource files
- Accessibility: screen reader labels on TweakCard
- Theme switching (Light/Dark/System) in Settings
- Configurable `?` tooltip delay
- Export applied tweaks as shareable JSON (redacted hardware IDs)
- Crash reporter (Sentry self-host or free tier)

---

## Out of scope (explicit non-goals)

- BIOS tuning and flashing. Research preserved in `docs/research/12-bios-cpu-ram-tuning.md` but we do not ship it.
- CPU undervolt automation (ThrottleStop, XTU, PBO2 Tuner).
- RAM timing automation.
- Overclocking scanner.
- Kernel-mode drivers (SmartScreen / anti-cheat blocklist risk).
- Paid tiers. Project stays Apache 2.0, GitHub-hosted, donation-funded.

---

## Reference

- Research knowledge base: `docs/research/` (13 MDs, 65000 words)
- Testing guide: `docs/TESTING.md`
- License: `LICENSE` (Apache 2.0)
- Contributing guide: `CONTRIBUTING.md`
- Security policy: `SECURITY.md`
