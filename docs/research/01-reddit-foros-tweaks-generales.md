# Windows 11 Gaming — Knowledge Base desde Reddit / Foros / Comunidades

**Fuentes:** r/optimizedgaming, r/Windows11, Guru3D, Overclock.net, Blur Busters, Windows Central, Tom's Hardware, Windows Forum, GitHub repos (NicholasBly, K3V1991, DaddyMadu, Raphire, Hellzerg, Chris Titus, KushagraSingh78), ProSettings.net, XbitLabs, NVIDIA official, Microsoft Support, Melody's Tweaks, Bitsum, Blur Busters Forums.

---

## ÍNDICE
1. Seguridad (VBS/HVCI/Spectre/Meltdown)
2. GPU & Render (HAGS/Reflex/Shader Cache)
3. Power Management
4. CPU Scheduling & Priorities
5. Timers & Kernel (HPET/BCDEdit)
6. Network
7. Memory Management
8. Services Debloat
9. Telemetry & Privacy
10. Mouse, Keyboard, Input
11. Display, HDR, Presentation
12. Antivirus & Defender
13. Tools & Utilities Ecosystem
14. Controversias y mitos
15. Ranking final

---

## 1. SEGURIDAD (el área más impactante)

### Disable Virtualization-Based Security (VBS)
- **Categoría**: Seguridad / Kernel
- **Qué hace**: Elimina la capa de virtualización tipo-1 que Windows 11 usa para aislar procesos del kernel con Hyper-V. Reduce overhead de MMU y traducciones EPT.
- **Cómo se aplica**:
  - GUI: Windows Security > Device Security > Core Isolation > Memory Integrity > Off
  - Registry: `HKLM\System\CurrentControlSet\Control\DeviceGuard` → `EnableVirtualizationBasedSecurity=0`
  - Verificar: `msinfo32` → "Virtualization-based security: Not enabled"
- **Fuente**: Tom's Hardware benchmarks (RTX 4090 testing), Microsoft Support Docs, github.com/K3V1991/How-to-disable-VBS_HVCI, r/OptimizedGaming, PCWorld
- **Consenso comunitario**: ALTO (es el tweak con mayor respaldo empírico)
- **Impacto real**: **+5% hasta +28% FPS según juego**. HZD -25%, Metro Exodus -24%, Shadow of the Tomb Raider -28% con VBS ON. Cyberpunk 5-7%, MSFS 10-15%.
- **Riesgos**: Reduce protección contra kernel rootkits y ataques de drivers sin firmar. Re-habilitar si instalas software sospechoso o haces pentest con payloads reales. No impacta Defender normal.
- **Recomendación**: **INCLUIR** (toggle grande, por default OFF pero con warning de seguridad claro)

### Disable HVCI (Memory Integrity específico)
- **Qué hace**: HVCI fuerza verificación de firmas de código en kernel vía hipervisor. El 80% del hit de VBS viene realmente de HVCI. MBEC (Mode-Based Execution Control) en Zen 2+ / Intel 7th+ mitiga parte del impacto.
- **Impacto real**: 5-15% FPS en CPU-bound scenarios
- **Consenso**: ALTO
- **Recomendación**: **INCLUIR** (mismo toggle que VBS típicamente)

### Disable Spectre/Meltdown Mitigations
- **Categoría**: Seguridad / Kernel / Controversial
- **Qué hace**: Desactiva mitigaciones de side-channel attacks en CPU (KVA Shadow, branch target injection, retpoline, etc.)
- **Cómo se aplica**: Tool InSpectre (GRC) botones GUI, o registry `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management` → `FeatureSettingsOverride=3`, `FeatureSettingsOverrideMask=3`
- **Consenso**: CONTROVERSIAL (era 10-30% en Skylake-Kabylake 2018, hoy 2-5% en Alder Lake/Zen 4+)
- **Impacto**: CPUs 2018-2020: 4-8%. CPUs 2022+: 1-3%.
- **Riesgos**: Altísimo. Exponés el sistema a Spectre v1/v2, Meltdown, Foreshadow, MDS. Cualquier JS malicioso en navegador puede explotarlo. Pentester con VMs: NO TOCAR.
- **Recomendación**: **INCLUIR CON WARNING DUAL** (checkbox bloqueado hasta 2 confirmaciones, texto rojo)

---

## 2. GPU & RENDER

### Hardware-Accelerated GPU Scheduling (HAGS)
- **Qué hace**: Mueve la gestión del frame queue del GPU desde CPU al procesador de comando del GPU. Reduce overhead de `dxgkrnl.sys`.
- **Cómo se aplica**: Settings > System > Display > Graphics > Default graphics settings > HAGS toggle + reboot
- **Soporte**: NVIDIA GTX 10-series+, AMD RX 7700+ (Win11), Intel Arc NO soporta
- **Consenso**: CONTROVERSIAL-MEDIO
- **Impacto real**: En RTX 40+ con DLSS3 FG **obligatorio**. En GPUs <8GB VRAM puede aumentar uso de VRAM. Sistemas balanceados: 0-3% FPS.
- **Riesgos**: Stutter en juegos viejos, +VRAM usage, problemas con OBS (docs OBS que desaconseja HAGS)
- **Recomendación**: **INCLUIR CON AUTO-DETECT** (RTX 40+ = ON obligatorio; GTX/older = mostrar comparación; Intel Arc = hide)

### NVIDIA Low Latency Mode (Ultra)
- **Qué hace**: Limita pre-rendered frames queue. Off=1-3 frames, On=1 frame, Ultra=just-in-time CPU submission
- **Consenso**: ALTO (para juegos sin Reflex nativo)
- **Impacto**: 20-40ms reducción en GPU-bound a 60-100 FPS.
- **Recomendación**: **INCLUIR**

### NVIDIA Reflex (activar in-game)
- **Qué hace**: Framecap dinámico + sync CPU/GPU nativo. Reflex 2 con Frame Warp reduce hasta 75% de PC latency.
- **Consenso**: ALTO absoluto
- **Impacto**: -50% a -75% system latency en juegos soportados (Valorant <3ms con Reflex 2)
- **Recomendación**: **INCLUIR** (recordatorio in-app)

### Shader Cache Size 10GB (NVIDIA)
- **Qué hace**: Aumenta tamaño de cache para shaders compilados, evita recompilación que causa stutter
- **Aplicación**: NVCP > Manage 3D Settings > Shader Cache Size = 10GB
- **Consenso**: ALTO
- **Impacto**: Reduce shader-comp stutter DX12/Vulkan, +5-10% frametime consistency
- **Recomendación**: **INCLUIR** (auto si hay >100GB libre)

### Power Management Mode = Prefer Maximum Performance (NVIDIA)
- **Qué hace**: Evita que el GPU baje de clock en carga ligera
- **Consenso**: ALTO
- **Impacto**: +5% FPS consistency, elimina drops random
- **Recomendación**: **INCLUIR**

### DirectStorage
- **Qué hace**: Bypass CPU decomp, GPU decodifica assets con GDeflate. Requires NVMe.
- **Impacto**: Forspoken 10s→1s load, Cyberpunk 57s→15s, Elden Ring fast travel 25s→8s
- **Recomendación**: **INCLUIR** (verificar NVMe)

---

## 3. POWER MANAGEMENT

### Ultimate Performance Power Plan
- **Aplicación**: `powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61`
- **Consenso**: ALTO
- **Impacto**: +5-10% FPS en CPU-bound, elimina latencias de wake-from-parked
- **Recomendación**: **INCLUIR** (warning en laptops: NO en batería)

### Bitsum Highest Performance (BHP)
- **Qué hace**: Como Ultimate pero también previene que CPU baje de base frequency
- **Viene con**: ParkControl o Process Lasso (Bitsum). También `.pow` file importable
- **Consenso**: ALTO
- **Recomendación**: **INCLUIR** (bundling .pow file)

### Disable CPU Core Parking
- **Qué hace**: Previene que cores entren a C6 sleep state cuando están idle
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583` → `Attributes=0`
- **Consenso**: ALTO
- **Impacto**: Mejora 1% lows, elimina micro-stutters
- **Recomendación**: **INCLUIR**

### Disable Power Throttling
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling` → `PowerThrottlingOff=1`
- **Impacto**: +2-5% FPS sostenido en laptops
- **Recomendación**: **INCLUIR** (auto-detectar laptop vs desktop)

### Disable Fast Startup
- **Consenso**: MEDIO (controversial boot time, ALTO para cleaner drivers state)
- **Recomendación**: **INCLUIR CON WARNING**

---

## 4. CPU SCHEDULING & PRIORITIES

### Win32PrioritySeparation = 0x26
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl\Win32PrioritySeparation = 38` (dec) o `0x26`
- **Valores populares**:
  - `0x02` default Win11 - NO tocar workstations
  - `0x26` hex Short/Variable/High - gaming común
  - `0x28` hex Short/Fixed/High - gaming "puro" sin background
  - `0x2A` hex alt
- **Consenso**: MEDIO
- **Recomendación**: **INCLUIR CON PRESETS** (dropdown)

### SystemResponsiveness = 10
- **Registry**: `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\SystemResponsiveness = 10`
- **Consenso**: MEDIO-ALTO
- **Impacto**: 1-3% FPS en juegos que usan MMCSS
- **Recomendación**: **INCLUIR**

### Games Task MMCSS tweaks
- **Registry**: `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games`:
  - `Affinity=0`, `Background Only=False`, `Background Priority=1`, `GPU Priority=8`, `Priority=6`, `Scheduling Category=High`, `SFIO Priority=High`, `SFIO Rate=4`
- **Consenso**: CONTROVERSIAL (probable placebo en DX12)
- **Recomendación**: **INCLUIR COMO AVANZADO**

### Intel E-cores / P-cores management
- **Aplicación**: BIOS Scroll Lock Disable E-cores, Process Lasso Efficiency Mode OFF
- **Consenso**: ALTO (issue real en W11 24H2 con Intel 12th-14th gen)
- **Impacto**: Hasta +15% FPS en juegos CPU-bound
- **Recomendación**: **INCLUIR CON AUTO-DETECT** (solo Intel hybrid)

### AMD 3D V-Cache CCD preference
- **Aplicación**: AMD Chipset Driver 5.01+ 3D V-Cache Optimizer
- **Consenso**: ALTO (oficial AMD)
- **Impacto**: Hasta +30% FPS en X3D duales
- **Recomendación**: **INCLUIR CON AUTO-DETECT**

---

## 5. TIMERS & BCDEDIT (zona minada)

### HPET / TSC / useplatformclock / useplatformtick
- **DEFAULT (recomendado)**: `bcdedit /deletevalue useplatformclock` + `bcdedit /deletevalue useplatformtick`
- **Consenso**: CONTROVERSIAL ALTO
- **Impacto real 2024-2026**: <1% en CPUs modernas. Forzar HPET on en Ryzen/Intel nuevos puede CAUSAR stutter
- **Recomendación**: **DESCARTAR FORZAR HPET ON** / **INCLUIR "Reset to Default"**

### bcdedit disabledynamictick
- **Aplicación**: `bcdedit /set disabledynamictick yes`
- **Consenso**: CONTROVERSIAL
- **Impacto**: Desktop only. Laptops: MUY MAL.
- **Recomendación**: **INCLUIR CON WARNING**

### bcdedit hypervisorlaunchtype off
- **Aplicación**: `bcdedit /set hypervisorlaunchtype off`
- **Consenso**: ALTO (par del VBS disable)
- **Riesgos**: Rompe Docker, WSL2, Sandbox. Pentester con VMs = NO.
- **Recomendación**: **INCLUIR EN BUNDLE "Disable VBS"**

### BCDEdit PELIGROSOS a EVITAR
Scripts "god-tier" meten estos ciegamente — red flags:
- `bcdedit /set nointegritychecks Yes` — desactiva verificación firma drivers
- `bcdedit /set nx AlwaysOff` — desactiva DEP (protección memoria) — CRÍTICO
- `bcdedit /set testsigning No` — combo con nointegrity = peligro
- **Recomendación**: **DESCARTAR TODOS**

---

## 6. NETWORK

### Disable Nagle's Algorithm
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{GUID}`:
  - `TcpNoDelay=1` (DWORD)
  - `TcpAckFrequency=1` (DWORD)
- **Consenso**: MEDIO
- **Impacto**: 5-15ms en juegos TCP (MMORPGs). **CERO en CS2/Valorant/OW2/Apex** (UDP).
- **Recomendación**: **INCLUIR** (marcar claramente "solo TCP games")

### NetworkThrottlingIndex = ffffffff
- **Registry**: `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\NetworkThrottlingIndex=ffffffff`
- **Consenso**: CONTROVERSIAL (probable placebo)
- **Recomendación**: **INCLUIR COMO AVANZADO**

### Disable NIC Power Saving / Green Ethernet / EEE
- **Consenso**: ALTO (elimina micro-desconexiones)
- **Recomendación**: **INCLUIR**

### Disable NDU (Network Data Usage)
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Services\Ndu\Start=4`
- **Recomendación**: **INCLUIR**

---

## 7. MEMORY MANAGEMENT

### DisablePagingExecutive = 1
- **Consenso**: PLACEBO
- **Recomendación**: **DESCARTAR** (era útil XP-era)

### LargeSystemCache = 1
- **Consenso**: PLACEBO PELIGROSO (server-style, mata performance workstations)
- **Recomendación**: **DESCARTAR**

### Disable Memory Compression (PowerShell)
- **Aplicación**: `Disable-MMAgent -MemoryCompression`
- **Consenso**: MEDIO (solo >=16GB RAM)
- **Recomendación**: **INCLUIR CONDICIONAL**

### Page File = System Managed
- **Consenso**: ALTO (no tocar, regla "1.5x RAM" es XP-era)
- **Recomendación**: **INCLUIR botón "Reset to System Managed"**

### Disable Hibernation
- **Aplicación**: `powercfg -h off`
- **Recomendación**: **INCLUIR** (desktop default, laptop intacto)

---

## 8. SERVICES (Tier system)

### Tier 1: Consenso ALTO (universal)
- `DiagTrack` (Connected User Experiences and Telemetry)
- `dmwappushservice` (WAP Push Service)
- `Xbox Live Auth Manager` + `XblGameSave` + `XboxNetApiSvc` (si no usas Xbox/Game Pass)
- `SysMain` (Superfetch) — Win10 sí, Win11 NVMe dejarlo
- `WSearch` — controversial
- `WerSvc` (Windows Error Reporting)
- `RetailDemo`
- `MapsBroker`
- `Fax`
- `WbioSrvc` (Biometric, si no usas Hello)

### Tier 2: Contexto-dependiente
- `Print Spooler` (si no imprimes — CVE PrintNightmare, disable = +seguridad)
- `TapiSrv` / `RasMan`
- `WlanSvc` (si Ethernet exclusivo)
- `RemoteRegistry`
- `SCardSvr`
- `SensorService` / `SensorDataService`
- `WMPNetworkSvc`

### Tier 3: PELIGROSO NO DISABLE
- `BFE` (Base Filtering Engine) — critical firewall
- `CryptSvc` — certificados
- `DcomLaunch` — critical RPC
- `Winmgmt` — WMI
- `wudfsvc` — user-mode drivers
- `NlaSvc` — Network Location Awareness
- `AudioEndpointBuilder` / `Audiosrv`

### Scheduled Tasks a deshabilitar
- `Microsoft\Windows\Application Experience\` → todas
- `Microsoft\Windows\Customer Experience Improvement Program\` → todas
- `Microsoft\Windows\Autochk\Proxy`
- `Microsoft\Windows\DiskDiagnostic\...`
- `Microsoft\Windows\Maintenance\WinSAT`
- `Microsoft\Windows\Feedback\Siuf\DmClient*`
- `Microsoft\Windows\Windows Error Reporting\QueueReporting`

---

## 9. TELEMETRY & PRIVACY

### AllowTelemetry
- **Registry**: `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection\AllowTelemetry=0`
  - Home/Pro: mínimo = 1 (Required Diagnostic Data)
  - Enterprise/Edu: 0 = Security only
- **Recomendación**: **INCLUIR** (detectar edition, máximo permitido)

### Disable Activity History / Location / Advertising ID
- **Recomendación**: **INCLUIR BUNDLE**

---

## 10. MOUSE, KEYBOARD, INPUT

### Disable Enhance Pointer Precision
- **Registry**: `HKCU\Control Panel\Mouse` → `MouseSpeed=0`, `MouseThreshold1=0`, `MouseThreshold2=0`
- **Consenso**: ALTO UNIVERSAL
- **Recomendación**: **INCLUIR OBLIGATORIO**

### Mouse Polling Rate
- **Consenso**: 1000Hz sweet spot. 4000Hz marginal. 8000Hz solo Ryzen 7000/Intel 13th+ (consume 2-3% de un core)

### Disable Sticky Keys / Filter Keys / Toggle Keys
- **Recomendación**: **INCLUIR OBLIGATORIO**

### Timer Resolution (0.5ms)
- **Win11 moderno**: muchos juegos ya lo piden (Fortnite, CS2, Valorant, OW2)
- **Recomendación**: **INCLUIR AUTO-DETECT** (Win11 22H2+ y juego lo pide = skip)

---

## 11. DISPLAY, HDR, PRESENTATION

### Optimizations for Windowed Games (Win11)
- **Qué hace**: Flip-model presentation para DX10/DX11. Permite VRR en borderless con latencia ~exclusive fullscreen
- **Consenso**: ALTO
- **Recomendación**: **INCLUIR** (ON default Win11)

### Disable Fullscreen Optimizations (por juego)
- **Consenso**: PLACEBO/NEGATIVO en Win11
- **Recomendación**: **DESCARTAR** / botón "Re-enable FSO"

### VRR / G-Sync / FreeSync setup
- **Frame cap**: 3 fps below refresh (237 para 240Hz) + V-Sync ON driver, OFF in-game
- **Fuente**: Blur Busters "G-Sync 101"
- **Recomendación**: **INCLUIR AUTO-CONFIG**

### Auto HDR
- **Consenso**: MEDIO (crashes documentados en 24H2)
- **Recomendación**: **INCLUIR CON TOGGLE** (default OFF)

### GPU Output Color / RGB Full Range
- **Aplicación**: NVCP Change Resolution > Output color format = RGB + Dynamic range = Full (0-255)
- **Recomendación**: **INCLUIR CON WARNING** (TVs necesitan Limited 16-235)

---

## 12. ANTIVIRUS & DEFENDER

### Defender Exclusions para carpetas de juego
- **Consenso**: ALTO
- **Impacto**: Elimina micro-stutters en juegos con I/O pesada
- **Recomendación**: **INCLUIR** (detectar Steam/Epic/Battle.net, ofrecer add)

### Disable Real-time Protection temporarily
- **Recomendación**: **NO INCLUIR** (demasiado peligroso automatizar)

### Schedule Defender Scans fuera de gaming hours
- **Recomendación**: **INCLUIR** ("scan at 4am" preset)

---

## 13. TOOLS ECOSYSTEM

- **ParkControl** (Bitsum, free) — Core parking + freq scaling per power plan
- **Process Lasso** (Bitsum, free/pro) — ProBalance, affinity, E-core avoidance
- **MSI_Util_v3** (Sathango GitHub) — MSI interrupts toggle
- **LatencyMon** (Resplendence, free) — DPC/ISR diagnostic
- **TimerResolution** (Lucas Hale) — Force 0.5ms
- **DDU** — Clean GPU driver install
- **MSI Afterburner + RTSS** — OC/undervolt + frame cap
- **InSpectre** — Spectre/Meltdown toggle
- **O&O ShutUp10++** — Privacy toggles GUI

---

## 14. CONTROVERSIAS Y MITOS

### MITO #1: "Disable HPET mejora FPS"
Forzar HPET puede **AUMENTAR** latency. Melody's Tweaks debunk directo.

### MITO #2: "LargeSystemCache = 1 mejora gaming"
Setting server-style. En workstation MATA performance.

### MITO #3: "DisablePagingExecutive = 1"
Placebo XP-era. Solo afecta ntoskrnl.exe.

### MITO #4: "TCP tweaks mejoran FPS"
Juegos FPS modernos usan **UDP**. TCP tweaks invisibles.

### MITO #5: "Disable Fullscreen Optimizations = más FPS"
En Win11 FSO es **superior**. Disable puede AÑADIR latency.

### MITO #6: "Custom power plans de foro X suben 30% FPS"
Muchos custom .pow rompen CPU boost. Solo Ultimate/BHP probados.

### MITO #9: "Clean Page File at Shutdown = faster"
LO CONTRARIO. Activar alenta shutdown.

### MITO #10: BCDEdit tweaks masivos
Scripts "god-tier" con `nx AlwaysOff`, `nointegritychecks` = expone a exploits.

### CONTROVERSIAL #1: Game Mode
Pro: native, prioriza threads. Contra: stutters documentados en COD Warzone, LoL.

### CONTROVERSIAL #2: HAGS
Pro: obligatorio DLSS3 FG. Contra: +VRAM, stutters viejos, OBS desaconseja.

### CONTROVERSIAL #4: Disable Memory Integrity (HVCI)
Pro: +5-28% FPS. Contra: vulnerabilidad kernel exploits.

### CONTROVERSIAL #5: Disable Spectre/Meltdown
Pro: 2-5% FPS CPUs modernos. Contra: JS puede explotar CPU side-channels.

---

## 15. RANKINGS FINALES

### TOP 20 TWEAKS CON CONSENSO ALTO (INCLUIR SIEMPRE)

1. Disable VBS/Memory Integrity — +5-28% FPS
2. Ultimate Performance Power Plan (o Bitsum BHP)
3. Disable CPU Core Parking
4. Update GPU Drivers (manufacturer app) + DDU clean
5. Enable HAGS (si RTX 40+ o FG)
6. NVIDIA Low Latency Mode = Ultra (o Reflex in-game)
7. NVIDIA Power Management = Prefer Maximum Performance
8. NVIDIA Shader Cache Size = 10GB
9. Disable Enhance Pointer Precision
10. Disable Sticky Keys / Filter Keys / Toggle Keys
11. Enable VRR/G-Sync/FreeSync + frame cap 3 below refresh
12. Enable Optimizations for Windowed Games (Win11)
13. Disable Windows Telemetry + DiagTrack service
14. Disable background startup apps
15. Defender folder exclusions para librerías juegos
16. Disable Xbox Game Bar + Game DVR
17. Set Refresh Rate = máximo monitor
18. Disable Fast Startup
19. Schedule Defender scans fuera gaming hours
20. Tier-1 services disable (DiagTrack, dmwappush, MapsBroker, RetailDemo, Xbox)

### TOP 10 CONTROVERSIALES (INCLUIR CON WARNING)

1. Disable Spectre/Meltdown mitigations
2. Disable HVCI separate from VBS
3. bcdedit hypervisorlaunchtype off
4. bcdedit disabledynamictick yes
5. MSI Mode utility GPU = High Priority
6. Win32PrioritySeparation = 0x28 Aggressive
7. Game Mode ON
8. Disable Memory Compression
9. Disable SysMain/Superfetch
10. Network Throttling Index = ffffffff

### TWEAKS QUE NUNCA INCLUIR (red flags)

- `bcdedit /set nx AlwaysOff`
- `bcdedit /set nointegritychecks Yes`
- `bcdedit /set testsigning No` combos
- `bcdedit /set useplatformclock true` forzado sin tradeoffs
- `LargeSystemCache = 1` workstations
- `DisablePagingExecutive = 1` como "magic"
- Custom power plans de foros random
- TCP hardcoded windows/scaling Win10+
- Full disable Windows Update
- Aggressive debloat (borrar AppX OneDrive, Store, Edge sin config)

### ORDEN DE APLICACIÓN EN LA APP

1. **Fase 1 — Basics Safe (por default)**: drivers, mouse accel OFF, sticky keys OFF, refresh rate max, VRR, Game Mode prompt
2. **Fase 2 — Performance Core (opt-in, low risk)**: Ultimate Performance, startup apps, Tier-1 services, telemetry off, scheduled tasks, Defender exclusions, shader cache 10GB
3. **Fase 3 — Advanced (opt-in, medium risk)**: HAGS, HVCI/VBS disable (big warning), NIC power saving, NDU, Tier-2 services, core parking
4. **Fase 4 — Expert (opt-in, high risk, uno por uno)**: MSI mode, Spectre/Meltdown, Win32PrioritySeparation, BCDEdit selected, disabledynamictick, Process Lasso
5. **Fase 5 — Rollback / Diagnostics**: LatencyMon, Reset-to-defaults per category, restore point automático

---

## Fuentes clave

- [Melody's Tweaks - HPET/TSC/PMT](https://sites.google.com/view/melodystweaks/misconceptions-about-timers-hpet-tsc-pmt)
- [ChrisTitusTech/winutil](https://github.com/christitustech/winutil)
- [hellzerg/optimizer](https://github.com/hellzerg/optimizer)
- [Raphire/Win11Debloat](https://github.com/Raphire/Win11Debloat)
- [K3V1991/How-to-disable-VBS_HVCI](https://github.com/K3V1991/How-to-disable-VBS_HVCI)
- [Tom's Hardware VBS benchmarks](https://www.tomshardware.com/news/windows-vbs-harms-performance-rtx-4090)
- [XDA optimization guides nonsense](https://www.xda-developers.com/most-windows-optimization-guides-are-nonsense-and-some-might-make-your-pc-worse/)
- [Blur Busters](https://blurbusters.com)
- [Overclock.net MSI Nvidia](https://www.overclock.net/threads/message-signaled-based-interrupt-msi-discussion-for-nvidia-gpus.1805762/)
- [Bitsum ParkControl](https://bitsum.com/parkcontrol/)
- [Bitsum Highest Performance](https://bitsum.com/bhp/)
- [Kartones services](https://blog.kartones.net/post/disabling-unneeded-windows-11-services/)
- [Microsoft Support Win11 gaming](https://support.microsoft.com/en-us/windows/options-to-optimize-gaming-performance-in-windows-11-a255f612-2949-4373-a566-ff6f3f474613)
- [NVIDIA Latency Guide](https://www.nvidia.com/en-us/geforce/guides/gfecnt/202010/system-latency-optimization-guide/)
- [Resplendence LatencyMon](https://www.resplendence.com/latencymon)
- [GRC InSpectre](https://www.grc.com/inspectre.htm)
