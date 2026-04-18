# re_sound Performance Roadmap

This document tracks the state of the project and the ordered list of pending work. Read this first when picking the project up again.

## Current state (snapshot, 2026-04-18)

- Repository: https://github.com/re-sound/re-sound-performance (Apache 2.0)
- .NET 8 WPF solution, three projects: main app, `Core` engine, xunit tests
- **34 tweaks** registered across System, Privacy, Debloat, GPU, Input, Network, Power categories
- **Hardware + anti-cheat detection** wired into the HomePage and the toggle gate
- **Preset selector** (Safe / Balanced / Competitive) with preview dialog, progress bar and cancellation
- **Per-game tabs** (CS2, Valorant, Apex) with installation detection, recommended launch options, in-game settings reference and (for CS2) an on-disk autoexec writer with backup
- **61 xunit tests** passing (Linux + Windows CI)
- CI workflow builds, runs tests, runs `dotnet format`
- Release workflow publishes portable + framework-dependent ZIPs on every `v*` tag
- Complete research knowledge base in `docs/research/` (13 documents, ~65000 words)

### Releases

Tag | Date | Notes
----|------|-------
`v0.0.1-test` | 2026-04-?? | First public build, 23 tweaks, post-apply probe
`v0.0.2-test` | 2026-04-18 | Post-apply verifier pill, UI polish, +10 tweaks (schtasks + Appx), Privacy/Debloat/GPU pages wired
`v0.0.3-test` | 2026-04-18 | Single-instance guard, TweakStateCache (no more 1-by-1 reload), UsoSvc tweak, UI redesign (Synapse-style violet `#7C3AED`)
`v0.0.4-test` | 2026-04-18 | Hardware + anti-cheat detection, preset selector (Safe/Balanced/Competitive), per-game tabs (CS2 autoexec writer, Valorant Vanguard compat, Apex launcher-aware)

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
Tweak state cache | `TweakStateCache` | | `ConcurrentDictionary` + `StateChanged`/`ProbingProgress` events
Single-instance guard | `SingleInstanceGuard` | | `Local\` Mutex + `FindWindow`/`SetForegroundWindow`
**Hardware detection (new in v0.0.4)** | `IHardwareDetector` | `WmiHardwareDetector` | `System.Management` WMI
**Anti-cheat detection (new in v0.0.4)** | `IAntiCheatDetector` | `WindowsAntiCheatDetector` | `IServiceManager` + `IFileSystemProbe`
**Detection context (new in v0.0.4)** | `DetectionContext` | | singleton + `Changed` event
**Tweak gate (new in v0.0.4)** | `TweakGate.Evaluate` | | pure function over metadata + `AntiCheatInfo`
**Presets (new in v0.0.4)** | `PresetCatalog` + `PresetRunner` | | 3 curated lists + progress-driven apply loop
**Game detection (new in v0.0.4)** | `IGameDetector` | `WindowsGameDetector` | Registry + `IFileSystemProbe` (Steam / Riot / EA App / Origin)
**Game config writer (new in v0.0.4)** | `IGameConfigWriter` | `FileSystemGameConfigWriter` | Filesystem with `.resound.bak` backup

### Non-obvious decisions worth remembering

- **Appx revert is informational, not a true reverse.** Once `Remove-AppxProvisionedPackage` runs the package cannot be restored without the original `.appx`. Revert returns `NotApplied` with a message pointing at the Microsoft Store.
- **OneDrive debloat is deferred.** Needs a Win32 uninstaller abstraction (`%SystemRoot%\SysWOW64\OneDriveSetup.exe /uninstall`). Placeholder slot still shown in the research doc; `RemoveLegacyMediaAppsTweak` fills the 8th Appx slot in the meantime.
- **`DisableUpdateOrchestratorTasksTweak` keeps returning `Unavailable`.** Those tasks are protected by TrustedInstaller. The helper detects Access Denied and produces a friendly message ("use a service-based tweak instead") instead of red `Error`. `DisableUsoSvcTweak` is the working alternative (disables `UsoSvc` + `WaaSMedicSvc`, same practical effect, works with plain Administrator).
- **Probes run in parallel once at startup** via `TweakEngine.ProbeAllAsync` fire-and-forget from `App.OnStartup`. Cards read from `TweakStateCache` synchronously and subscribe to `StateChanged`, so tab switches are instant and never redraw 1-by-1 again.
- **Mutex uses `Local\` namespace, not `Global\`.** Single-instance is per user/session, which is what we want; `Global\` would need explicit SID access rights even for an elevated app.
- **Failed pill for probe-mismatch is intentional.** If `ApplyAsync` returns Success but the post-apply probe says `NotApplied`, we mark as `Failed` with "Post-apply verification mismatch: changes did not take effect." — catches GPO / anti-cheat overrides.
- **UI design tokens live in `Themes/Colors.xaml` and `Themes/Styles.xaml`.** No more hardcoded RGB inside controls. Category colors, risk colors, card surfaces and typography are `StaticResource` keys.
- **Windows paths in detection are joined with explicit `\`** (not `Path.Combine`). Otherwise Linux CI tests end up with `C:\Foo/bar` which no `InMemoryFileSystemProbe` will match. See `WindowsGameDetector.JoinWindows`.
- **`DetectionContext` is populated once at startup** via fire-and-forget in `App.OnStartup`. UI surfaces show `Detecting...` until the `Changed` event fires.
- **`TweakGate` is pure.** Every UI path (card toggle, preset step, Valorant page warning list) flows through `TweakGate.Evaluate` so the enforcement story stays consistent.
- **Presets always skip already-applied tweaks** rather than re-applying. Probe state is the source of truth.
- **Apex EA App launcher ignores `+` launch options from shortcuts.** Surfaced as an install-time note on the Apex page; users must set them in EA App > Game settings > Advanced launch options.
- **Valorant config writes are deliberately refused.** The INI lives under `%LOCALAPPDATA%\VALORANT\Saved\Config\<hash>\Windows\` and is tied to the signed-in Riot account; re_sound Performance only displays recommended settings and the blocked-tweak list.

### Tweak catalog (34 tweaks)

Category | Count | Representative tweaks
---------|-------|----------------------
System | 10 | Xbox Game Bar, Game DVR + FSE, MPO, MMCSS SystemResponsiveness, Edge Startup Boost, Maps Broker, Fax/RetailDemo, Xbox services (Medium), WER + SysMain (Medium), UsoSvc
Input | 2 | Enhance Pointer Precision, Sticky/Filter/Toggle Keys
Privacy | 10 | Telemetry, Activity History, Advertising ID, Copilot + Recall, Tips, Bing/Web Search, Location Tracking, DiagTrack services, Telemetry schtasks, Update Orchestrator schtasks (Unavailable w/o TrustedInstaller)
Network | 1 | NDU non-paged pool fix
Power | 2 | Ultimate Performance plan, Hibernation + Fast Startup
GPU | 1 | DirectX Shader Cache 10 GB
Debloat | 8 | Teams consumer, Clipchamp, Bing apps, Xbox consumer, Copilot, Recall/AI, Stock annoyances, Legacy media (Groove/Movies/Solitaire/OneNote)

### Presets

Preset | Tweaks | Intended user
-------|--------|--------------
Safe | 12 | Any Windows 11 box. Zero services disabled, zero debloat, nothing anti-cheats will flag.
Balanced | 29 | Daily driver that games. Service hardening + Appx debloat on top of Safe.
Competitive | 31 | Dedicated gaming rig on SSD/NVMe with 16+ GB RAM. Adds hibernation off and legacy media removal.

---

## Next sprint — pick one when resuming

### 1. Benchmarking integration (Intel PresentMon 2.0)

Research: `docs/research/11-benchmarking-integracion.md`. Files:

- `Core/Benchmark/IPresentMonRunner.cs`, `PresentMonCli.cs` (CSV v2 schema)
- `FrameStats.cs`, `FrameStatsCalculator.cs` (avg + integral 1% low conventions)
- `BenchmarkComparer.cs` — weighted verdict: 30% avg + 50% 1% low + 20% latency
- `BenchmarkPage.xaml` already scaffolded; wire ScottPlot live graph and wizard (baseline → apply → post)

### 2. Auto-update + signed builds (towards v1.0)

Research: `docs/research/13-ecosistema-produccion.md`.

- Apply to SignPath Foundation for free OSS code signing
- Integrate Velopack for delta auto-updates
- `install.ps1` one-liner
- Submit to winget community repo
- Document three distribution channels on README

### 3. OneDrive debloat

Needs a Win32 uninstaller abstraction (`%SystemRoot%\SysWOW64\OneDriveSetup.exe /uninstall`). Research notes already in `docs/research/03-registry-servicios-telemetria-uwp.md`.

### 4. Future enhancements (lower priority)

- Hardware-aware preset recommendations (e.g. skip `EnableUltimatePerformancePlan` on laptops, warn on < 16 GB RAM for Competitive)
- Live reloading of presets from a community-maintained GitHub JSON (behind opt-in flag)
- PresentMon telemetry opt-in (anonymous hardware + tweak deltas)
- i18n: English + Spanish resource files
- Accessibility: screen reader labels on TweakCard
- Theme switching (Light/Dark/System) in Settings
- Configurable `?` tooltip delay
- Export applied tweaks as shareable JSON (redacted hardware IDs)
- Crash reporter (Sentry self-host or free tier)
- Valorant config writer (needs account-hash discovery under `%LOCALAPPDATA%\VALORANT\Saved\Config`)
- Apex videoconfig writer (needs read-only attribute toggling because Respawn rewrites the file on exit)

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
