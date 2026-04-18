# re_sound Performance

Windows 11 gaming optimization app with per-game profiles, anti-cheat awareness and integrated benchmarking.

**Status:** In active development. Not ready for production use. First public beta expected in 6 weeks.

---

## What it does

re_sound Performance is a Windows 11 optimization tool focused on gaming. The core purpose is to optimize the whole PC for gaming performance, responsiveness and latency. A dedicated tab provides additional per-game tweaks for specific competitive titles.

### Primary focus: system-wide PC gaming optimization

- **Safe system-wide tweaks**: registry, services, scheduled tasks, UWP bloat, telemetry, Copilot, Recall
- **Power and kernel**: Ultimate Performance / Bitsum Highest Performance plans, core parking, timer resolution, MPO disable, HAGS configuration
- **GPU optimization**: NVIDIA Profile Inspector global settings + AMD Adrenalin tuning
- **Network and input**: NIC power saving, NDU service, mouse acceleration, pointer precision
- **Anti-cheat detection**: automatic compatibility check for Vanguard, FACEIT, EAC, BattlEye. Dangerous tweaks get blocked when a competitive anti-cheat is installed.
- **Integrated benchmarking**: before/after FPS, 1% lows, PC Latency via Intel PresentMon 2.0
- **Granular backup and revert**: every tweak can be undone individually. Automatic System Restore Point before batch operations.
- **Pro/mythological labeling**: every tweak is marked as CONFIRMED / CONTROVERSIAL / MYTH with linked sources.
- **Hover tooltips**: every tweak has a `?` icon revealing what it does, what it modifies, the risk level and the expected impact.

### Secondary feature: per-game tweaks tab

One dedicated tab inside the app ships per-game profiles for the three most popular competitive titles:

- **Counter-Strike 2**: launch options, autoexec.cfg template, per-game NVIDIA/AMD profile, FACEIT requirements checker
- **Valorant**: GameUserSettings.ini and Engine.ini tweaks, Vanguard compatibility matrix, VAN error code troubleshooter
- **Apex Legends**: launch options (Steam and EA App variants), videoconfig.txt tweaks, EAC compatibility

Each game tab includes one-click pro config import from top player settings (s1mple, ZywOo, donk, TenZ, aspas, ImperialHal and others).

## What it does NOT do (by design)

- No BIOS tuning, CPU undervolt wizards or RAM timing automation. Scope intentionally excluded to keep the tool safe and low-maintenance.
- No placebo tweaks. Every tweak shipped has a verifiable source in the research folder.
- No telemetry by default. Optional opt-in analytics only, never personal data.
- No ads, no upselling, no paywalls.

## Requirements

- Windows 11 22H2 or newer (24H2 recommended)
- Administrator rights (required to modify registry and services)
- .NET 8 Desktop Runtime
- At least 200 MB free disk space
- Internet connection for first run (auto-update check, community profiles)

Known compatible hardware:
- Any x64 CPU (Intel 10th gen+, AMD Ryzen 3000+)
- Any modern GPU (NVIDIA GTX 10 series+, AMD RX 5000+)
- Not supported: ARM64 Windows (Snapdragon X, Copilot+ PCs)

## Installation

### Option 1: Installer (recommended for most users)

1. Download the latest signed installer from the [Releases page](https://github.com/re-sound/re-sound-performance/releases)
2. Run `re_sound_performance_setup.exe`
3. Accept UAC prompt
4. Follow the wizard
5. Launch from Start Menu

### Option 2: Portable

1. Download `re_sound_performance_portable.zip` from Releases
2. Extract to any folder
3. Right-click `re_sound_performance.exe` and select "Run as administrator"

### Option 3: PowerShell one-liner

Open PowerShell as administrator and run:

```powershell
irm https://raw.githubusercontent.com/re-sound/re-sound-performance/main/install.ps1 | iex
```

This downloads, verifies and installs the latest signed build.

### Option 4: winget

```powershell
winget install re-sound.Performance
```

(Available after v1.0 release)

## First run

1. Launch re_sound Performance with administrator rights
2. The app will scan your hardware and installed games
3. Review the detected profile (GPU vendor, anti-cheat presence, OS build)
4. Choose a preset:
   - **Safe**: only reversible tweaks, zero anti-cheat risk, recommended for first use
   - **Balanced**: safe tweaks plus moderate performance tuning
   - **Competitive**: maximum gaming performance, preserves anti-cheat compatibility
   - **Custom**: pick each tweak manually
5. Preview the changes that will be applied
6. Click Apply. A restore point is created automatically before any change.

## How to revert changes

Every tweak has an individual Revert button. To roll back everything:

- **Revert all**: Settings -> Revert all changes
- **System Restore Point**: Windows built-in restore point named `re_sound_performance_pre_<timestamp>`
- **Uninstall**: removes all app files and optionally reverts all applied tweaks

## Per-game optimization

The app ships with dedicated tabs for:

- **Counter-Strike 2**: launch options, autoexec.cfg template, in-game video settings, NVIDIA/AMD per-game profile, FACEIT anti-cheat requirements checker
- **Valorant**: GameUserSettings.ini and Engine.ini tweaks, Vanguard compatibility warnings, VAN error code troubleshooter
- **Apex Legends**: launch options (Steam and EA App variants), videoconfig.txt tweaks, EAC compatibility, ImperialHal/Faide/Genburten pro configs

Every setting has a `?` icon next to it. Hover over it for a tooltip explaining what it does, what it modifies, the risk level and the expected impact.

## Integrated benchmarking

Launch the benchmark wizard from the sidebar to measure the real impact of your tweaks:

1. Baseline capture: 60 to 120 seconds of gameplay recording (via Intel PresentMon 2.0)
2. Apply the tweaks you want to test
3. Post-tweak capture: same game, same scenario
4. Comparison: FPS avg, 1% lows, 0.1% lows, PC Latency, GPU Busy, frametime variance
5. Verdict: weighted score with clear winner

Supported anti-cheats for benchmarking:
- Easy Anti-Cheat: fully supported
- BattlEye: fully supported
- Vanguard: supported with CLI mode only (no Service mode)
- FACEIT AC: supported

## Project structure

```
re-sound-performance/
    src/           Application source code (C# WPF)
    docs/          User documentation
        research/  Knowledge base (13 docs, 65000 words of research)
    .github/       Issue templates, workflows, community health files
    LICENSE        Apache 2.0
    README.md      This file
    CONTRIBUTING.md
    SECURITY.md
```

## Roadmap

- **v0.1 Alpha** (week 1-2): core engine, Apply/Revert, 15 safe tweaks, System Restore Point
- **v0.3 Alpha** (week 3): CS2 tab with pro configs
- **v0.5 Beta** (week 4-5): Valorant + Apex tabs, anti-cheat detection, telemetry off bundle
- **v0.7 Beta** (week 6): integrated benchmarking (PresentMon 2.0)
- **v1.0 Stable** (week 7-8): auto-update, signed builds, docs site, public launch

## Security and trust

- 100% open source, auditable
- Reproducible builds via GitHub Actions
- Code signed with SignPath Foundation certificate (OSS-verified)
- Every tweak has a linked source in `docs/research/`
- Backup is created before every registry, service, scheduled task or file modification
- SBOM (Software Bill of Materials) published with each release

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Community tweak proposals, pro config updates and translations are welcome. Code follows a strict no-comments no-emojis style.

## Security disclosure

See [SECURITY.md](SECURITY.md). Report vulnerabilities privately via GitHub Security Advisories. Do not open public issues for security problems.

## License

Apache License 2.0. See [LICENSE](LICENSE).

## Disclaimer

Use at your own risk. This software modifies the Windows operating system. While every effort is made to ensure safety (restore points, granular reverts, backups), unexpected behavior is possible on exotic hardware configurations or OEM-customized Windows builds. The authors are not liable for data loss, system instability, warranty issues or anti-cheat actions taken against your game accounts. Not affiliated with Microsoft, Valve, Riot Games, Respawn Entertainment, NVIDIA, AMD or any other vendor referenced.
