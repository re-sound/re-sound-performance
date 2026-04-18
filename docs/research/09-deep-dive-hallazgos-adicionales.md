# Deep Dive — Hallazgos Adicionales 2026

Research complementario enfocado en joyas escondidas, proyectos de nicho, research profundo, hardware nuevo, tweaks oscuros y controversias 2026. Excluye info mainstream ya cubierta en docs 01-08.

---

## 1. Comunidades y foros no mainstream

### 1.1 djdallmann/GamingPCSetup — WINKERNEL

**Finding #1 — BCDEdit `tscsyncpolicy` misinterpretado**: Eliminar entrada ≠ "Enhanced". Eliminar = `0x00000000` (no documentado), "Enhanced" = `0x00000002`, "Legacy" = `0x00000001`. Contradice guías mainstream.

**Finding #2 — Win32PrioritySeparation se actualiza en caliente**: NO requiere reinicio (contrario a creencia popular). Verificable WinDBG: `dd PsPrioritySeperation l1` + `db PspForegroundQuantum l3`. `_KPROCESS.QuantumReset` se adapta dinámicamente.

**Finding #3 — PS/2 vs USB consistencia medible**: USB DPC latency más baja (4-8μs) pero inconsistente. PS/2 mantiene 8μs con 97-99% consistencia, 32μs interrupt latency constante. Valida obsesión ultra-competitiva con PS/2 (diferencia mínima).

**Finding #4 — `powercfg /energy` 60-seg audit**: Herramienta subutilizada. Reporta "Platform Timer Resolution: Outstanding Timer Request" identificando cada proceso que manipula timer. Úsala ANTES/DESPUÉS de tweaks para validar.

Fuente: https://github.com/djdallmann/GamingPCSetup/blob/master/CONTENT/RESEARCH/WINKERNEL/README.md

### 1.2 valleyofdoom/TimerResolution + PC-Tuning

**Finding #5 — `GlobalTimerResolutionRequests` registry key**: Microsoft añadió en Win11+ / Server 2022+ para restaurar comportamiento pre-2004 (global). Path: `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel\GlobalTimerResolutionRequests = 1`. Sin esto, en 24H2 cada proceso maneja timer independiente — si minimizado u ocluido NO garantiza resolución.

**Finding #6 — `0.507ms` supera a `0.500ms`**: Contraintuitivo. Testing cross-usuario (30+) confirmó 0.507ms produce varianza sleep delta ~0.012ms vs ~0.496ms en 0.500ms. Óptimo NO siempre es máximo hardware. Usar `micro-adjust-benchmark.ps1` del repo.

**Finding #7 — `PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION` flag**: Permite requests de timer resolution persistir incluso minimizado/ocluido. Via `SetProcessInformation` API. Crítico para apps background (voice comms).

**Finding #8 — RSS queues > 2 = desperdicio gaming**: Tráfico Valorant-level (~300KB/s) usa 1-2 RSS queues max. 1Gbps confirmado solo 2 CPUs. Extra = cargo cult.

**Finding #9 — Win32PrioritySeparation > 63 se trunca silenciosamente**: Valores >`0x3F` bitmask-trunca sin error. CSV pre-calculado 0-271 en repo.

Fuentes:
- https://github.com/valleyofdoom/PC-Tuning/blob/main/docs/research.md
- https://github.com/valleyofdoom/TimerResolution

### 1.3 BoringBoredom/PC-Optimization-Hub — XHCI IMOD

**Finding #10 — XHCI Interrupt Moderation tuning**: Modifica IMOD Register USB controller via RWEverything. Pasos: Device Properties → Capability Base Address → +0x18 (Runtime Base) → +0x24 (IMOD Register) → setear `FA00` = 62.5Hz testing. Batch automatizable con `Rw.exe /Min /NoLogo /Stdout /Command="W32 0xADDRESS 0x00000000"`. Reduce jitter USB significativo. Casi nadie lo discute públicamente.

Fuente: https://github.com/BoringBoredom/PC-Optimization-Hub/blob/main/content/xhci%20imod/xhci%20imod.md

### 1.4 Blur Busters Forums — 2023 Esports Guide

**Finding #11 — External framerate caps añaden más latencia que in-game caps**: Consensus validado por flood's measurements. Usar siempre cap in-engine cuando disponible.

Mark Rejhon publicó metodología medición click-to-photon con LED modificado en Logitech G9x + cámara 1000fps — sigue siendo gold standard.

Fuente: https://forums.blurbusters.com/viewtopic.php?f=10&t=10986

### 1.5 Gearspace (audio engineering)

**Finding #12 — Setear GPU/ASIO affinity a cores DISTINTOS elimina DPC latency**: Audio engineers descubrieron esto antes que gamers. Aplica directamente a competitive gaming con voice comms.

**Finding #13 — NVIDIA Studio drivers mejor para audio low-latency que Game Ready**: Comunidad gaming ignora. Si workflow incluye OBS + voice, testear Studio.

**Finding #14 — DPCLat deprecado en Windows 11**: No reporta correctamente. Usar SOLO LatencyMon. Umbrales reales: <2000μs = suitable; 2000-4000μs = dudoso; >4000μs = unsuitable.

**Finding #15 — C-States disable = mayor impacto DPC**: Más que cualquier BIOS tweak. Orden prioridad: C-States > Spread Spectrum > Turbo/Speedstep.

Fuente: https://gearspace.com/board/music-computers/1432294-improving-real-time-audio-performance-windows-11-a.html

### 1.6 Overclock.net — 9800X3D memory tuning

**Finding #16 — Secondaries > Primaries en DDR5 Zen5**: Tras EXPO, primarias se ven (CL30-36-36) pero secundarias/terciarias quedan default malas. Focus: tRAS=28, tRC=68, tWR=48, tRFC1=500, tRFC2=400, tRFCsb=300, tFAW=20, tRDRDSCL=4, tRDRDSC=1, tWRWRSCL=4, tWRWRSC=1.

**Finding #17 — Sub-60ns requiere "Legacy mode" AGESA, NO siempre más rápido real**: En recent AGESA no llegás <60ns AIDA sin legacy. 67.6ns vs 75ns = diferencia medible gaming; 60ns vs 65ns no.

**Finding #18 — `Nitro 1/2/0` + `AdRdPtrInitVal 0` en AMD Overclocking submenu**: Config BIOS específica Zen5 X3D poco documentada fuera Overclock.net.

Fuente: https://www.overclock.net/threads/help-tuning-ryzen-9800x3d-g-skill-ddr5-6000-cl26-hynix-a-die-for-lowest-latency-1-1.1817608/

---

## 2. Proyectos de nicho

### 2.1 AME Wizard ecosystem

Más allá Atlas/ReviOS, playbooks menos conocidos:
- **Privacy+** — enfoque privacy, complemento gaming
- **RapidOS** — agresivo, modifica core con balance estabilidad
- **ArkanoidOS Lite/+/Pro** — 3 variantes modulares; Pro se acerca a Atlas

Password desempaquetar cualquier playbook: `malte`. Todo plaintext ZIP. Permite auditoría sin confiar ciegamente.

Fuente: https://github.com/jointhearkanoid/playbook

### 2.2 Ghost-Optimizer (louzkk)

Script PowerShell menos conocido que Chris Titus WinUtil. Focus explícito reducir Copilot/AI, telemetría, latencia. Más granular que WinUtil en privacy. Repo: https://github.com/louzkk/Ghost-Optimizer

### 2.3 simeononsecurity/Windows-Optimize-Harden-Debloat

**Diferenciador:** Cumple DoD STIG/SRG además de gaming. Baseline "enterprise-grade" opcional. Tiene GUI: https://github.com/simeononsecurity/Windows-Optimize-Harden-Debloat-GUI

### 2.4 DaddyMadu/Windows10GamingFocus

Fork Chris Titus con focus gaming, script único concatenado. Más agresivo debloat. Útil como "expert mode".

### 2.5 NTLite vs MSMG Toolkit

- **NTLite 2026.04.10936**: GUI comercial ($), Win7-11, live editions o ISO prep
- **MSMG Toolkit**: Text menu, free, ISO-only
- **WinReducer** / **Win Toolkit** / **DISMTools**: alternativas menos discutidas

MSMG más "community-hackable". NTLite para premium.

### 2.6 ReservedCpuSets (valleyofdoom)

**Finding #19**: Inverse affinity tool. "Don't schedule HERE". Registry: `HKLM\System\CurrentControlSet\Control\Session Manager\kernel\ReservedCpuSets`. En Intel hybrid E+P, reservas E-cores para interrupts/background y P-cores quedan para el juego. En AMD sin E-cores, reservás últimos 2 cores. **Mucho más potente que ProcessLasso affinity** — afecta scheduler del kernel.

Fuente: https://github.com/valleyofdoom/ReservedCpuSets

### 2.7 Xbox Full Screen Experience

Rolling out Abril 2026. Tool no oficial: **8bit2qubit/XboxFullScreenExperienceTool**. Activa en cualquier Win11 25H2. Reduce background, libera RAM, controller-first UI. Integrar botón "Enable Xbox Mode" = diferenciador.

Fuente: https://github.com/8bit2qubit/XboxFullscreenExperienceTool

---

## 3. Research profundo — DPC, latency, VRR

### 3.1 DPC latency — drivers culpables 2025-2026

**Finding #20 — Intel Killer Performance Suite v40.25.1125.1924 (Ene 2026)**: Drivers Intel Killer subyacentes OK; el **Performance Suite software overlay** causa DPC. **Instalar drivers puros Intel.com directo, NUNCA Killer Suite**.

**Finding #21 — Realtek 2.5GbE LAN (RTL8125) drivers pre-jun 2024 = DPC nightmare**: Versiones antiguas spikes >8000μs. Actualizar a 10.069+ de Station-Drivers.

**Finding #22 — ndis.sys alto = driver de red, no kernel**: Fix real es cambiar driver NIC, no tocar kernel.

### 3.2 Input latency end-to-end — herramientas 2026

**Finding #23 — Intel LMT (Latency Measurement Tool) en desarrollo**: Open-source hardware + software integrado con PresentMon. Incluirá PCB schematics, será free. Competencia LDAT.

**Finding #24 — OSLTT = LDAT open-source**: Mouse-to-photon hardware real, alternativa gratis al LDAT cerrado NVIDIA. Proyecto andrew-robbins.

**Finding #25 — PresentMon "All Input to Photon Latency"**: Métrica expuesta en PresentMon 2.0. FrameView NVIDIA lo incluye como "PC Latency". Correlaciona (no iguala) con hardware real. Sufficient para iteración.

### 3.3 Frame time consistency — GamersNexus Animation Error

**Finding #26 — "Runt frames" no detectan en FPS avg**: Frames parciales elevan FPS artificialmente pero crean tearing brutal. Solo visible en animation error methodology. Whitepaper Oct 2025 GamersNexus.

**Finding #27 — Simulation time error ≠ frame time**: Juego puede tener 16.6ms frametime perfecto pero animation feels "off" si simulación desincronizada. PresentMon 2.0 expone directamente. Cambia cómo pensar "stutter".

Fuente: https://gamersnexus.net/gpus-cpus-deep-dive/fps-benchmarks-are-flawed-introducing-animation-error-engineering-discussion

### 3.4 VRR technology comparison 2026

**Finding #28 — VESA AdaptiveSync es único estandarizado con tiers certificación**: NO es G-SYNC, NO es FreeSync. Open spec. Cert AdaptiveSync 144 (baseline) vs 288/360.

**Finding #29 — HDMI 2.1 VRR ≠ FreeSync sobre HDMI**: Protocolos distintos. Una TV HDMI 2.1 VRR puede NO funcionar con FreeSync y viceversa. Verificar match explícito monitor/GPU.

**Finding #30 — DisplayPort 2.1 UHBR 20 (80 Gbps) requiere cable DP80 certificado**: Muchos cables venden "DisplayPort 2.1" sin UHBR 20. Para 4K@240Hz sin DSC, cable = bottleneck.

**Finding #31 — DP 0.5ms < HDMI input lag**: Diferencia consistente pero imperceptible. Solo importa competitivo extremo.

### 3.5 ReBAR per-game — lista concreta

**Finding #32 — Watch Dogs Legion whitelisted pero PIERDE rendimiento**: -10% avg FPS @ 1080p, -16% 1% lows. Nvidia lo whitelisted por error. Deshabilitar vía Inspector.

**Finding #33 — VR games benefit > rasterized**: A veces diferencia unplayable vs playable.

**Finding #34 — Dying Light: The Beast**: +6% avg / +27% 1% low / +17% 0.1% low con ReBAR. Uno mayores gainers 2025.

**Finding #35 — Force-enable via NVIDIA Profile Inspector**: Keyword `rBAR Feature Enabled` + `rBAR Options` a `0x00000000` per-profile.

### 3.6 Reflex 2 Frame Warp (Ene 2025 — RTX 50)

**Finding #36 — 3ms PC latency VALORANT @ 800+FPS**: RTX 5090 + Reflex 2 Frame Warp. Nuevo plateau físico latencia consumer.

**Finding #37 — PureDark demo free funciona RTX 20+**: NVIDIA lanzó "first RTX 50", pero PureDark demo injected para GPUs anteriores.

**Finding #38 — Frame Warp "warps" frame renderizado justo antes de enviar al display**: No predicción, reproject. Samplea nueva cámara desde CPU, warpea frame GPU-side.

---

## 4. Windows 11 24H2 / 25H2 specific

### 4.1 Regresión gaming KB5066835 (Oct 2025)

**Finding #39 — Builds 26100.6899 y 26200.6899 afectados**: Hasta 50% drop en AC:Shadows y CS2. NO solo Nvidia — AMD e Intel también.

**Finding #40 — NVIDIA 581.94 hotfix**: Driver específico para fix. Si usuario en 24H2/25H2 con drivers <581.94, flaggear upgrade.

**Finding #41 — 25H2 no ofrece ganancias sobre 24H2**: Según Tom's Hardware. Migrar 24H2→25H2 SOLO por features, no performance.

### 4.2 WDDM 3.2 en 24H2

**Finding #42 — IOMMU DMA remapping (WDDM 3.1+)**: GPU accede memoria por logical addresses. Reduce crashes driver + mejora seguridad.

**Finding #43 — Work Graphs + generic programs (WDDM 3.2)**: Nuevas GPU-driven rendering. Requiere Shader Model 6.8. Mesh shaders + WorkGraphs = futuro inmediato Unreal 5.5+.

**Finding #44 — MPO issues en 24H2**: Algunas combinaciones driver+monitor causan flickering. Reg `OverlayTestMode=5` en `HKLM\SOFTWARE\Microsoft\Windows\Dwm` disable.

### 4.3 Auto HDR bugs 24H2

**Finding #45 — KB5051987 (11 Feb 2025) y KB5050094 arreglan Auto HDR oversaturation**: Pre-esa fecha, visualmente roto.

**Finding #46 — HDR Calibration App requiere re-run tras 25H2 upgrade**: Profile se invalida silenciosamente. Automatizar re-run post-upgrade.

### 4.4 EAC / BattlEye / Vanguard 24H2

**Finding #47 — EAC pre-Abril 2024 = 24H2 BSOD guaranteed**: Apex, Star Wars Squadrons, Dead by Daylight históricamente. KB5063060 OOB fix.

**Finding #48 — Vanguard + HVCI/VBS con Win11 ARM64**: BattlEye ahora soporta ARM via Prism emulator 24H2. Riot no anunció aún.

### 4.5 Copilot / Recall / Widgets performance

**Finding #49 — Gaming Copilot model training = dips 70s fps (desde 85-90+)**: TechRadar test Dead As Disco. Apagar "model training" + Copilot capture features en Game Bar recupera FPS.

**Finding #50 — Recall AIXHost.exe = 1.2GB RAM con solo scroll en snapshots**: Disable Recall completo: `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI\DisableAIDataAnalysis=1`.

**Finding #51 — Copilot continuous = 500MB-1GB RAM passive**: Incluso apagado visualmente. Disable via Group Policy.

---

## 5. Hardware-specific tweaks poco discutidos

### 5.1 Intel Arc Battlemage (B580 / A770 / Alchemist)

**Finding #52 — Arc Control deprecated, Intel Graphics Software (IGS) nuevo**: Todos B-series desde Dic 2024. Si user tiene Arc Control, uninstall first.

**Finding #53 — Driver 32.0.101.8626 (Mar 2026) fix performance regression older CPUs**: 10th gen Intel, Ryzen 3000. "Intel quietly buffed GPU drivers".

**Finding #54 — XeSS 3 MFG Ene 2026**: Multi-Frame Gen 3x/4x en A770 + Battlemage. 60→180-240 FPS. Rival directo DLSS 4 MFG.

**Finding #55 — Arc config: Clock slider 1/3 range start, NEVER voltage first**: Arc GPUs voltage-sensitive causan hard crashes (no throttling graceful). Orden: Power Limit MAX → Clock target (start 1/3) → Voltage solo si estable.

Fuentes:
- https://www.switchbladegaming.com/game-settings/intel-arc-control-best-settings/
- https://downloadmirror.intel.com/915256/ReleaseNotes_101.8626.pdf

### 5.2 Zen5 X3D (9800X3D / 9950X3D / 9950X3D2)

**Finding #56 — 9950X3D CCD parking = wrong CCD en juegos (no DCS)**: Todos los juegos excepto DCS usan core parking AMD+scheduler para routear al CCD V-Cache. DCS necesita fix manual.

**Finding #57 — 9950X3D2 Dual 3D V-Cache (2026)**: AMD's fix — V-Cache en ambos CCDs. Si app detecta 9950X3D (single CCD), warning "parking confusion posible"; si 9950X3D2, skip warning.

**Finding #58 — 9800X3D single CCD = no parking issue**: Mejor gaming CPU por diseño. Detectar = skip advanced parking tweaks.

Fuentes:
- https://www.overclock.net/threads/how-i-fixed-core-parking-on-my-9950x3d-and-taichi-lite.1815819/
- https://www.newegg.com/insider/amd-ryzen-9-9950x3d2-dual-edition-inside-the-worlds-first-dual-3d-v-cache-desktop-processor

### 5.3 NVMe Gen 5 — verdad 2026

**Finding #59 — Gen 5 SSD gaming = marginal 2026**: Load times diff <2 seg. Solo 10-15% mejora initial load best-case.

**Finding #60 — Gen 5 consumen 10-12W vs Gen 4 7-8W**: Thermal + power impact. En laptop, -30 min batería.

**Finding #61 — DirectStorage aún marginal adoption 2026**: Forspoken sigue siendo benchmark. Stutter-free asset streaming es ganancia real. Gen 4 + DirectStorage > Gen 5 sin DirectStorage.

**Finding #62 — nvmedisk.sys native driver hack Win11 25H2**: Registry hack activa NVMe "native" vs SCSI legacy. +13% AS-SSD, +85% 4K random write en Crucial T705. Path: `HKLM\SYSTEM\CurrentControlSet\Services\stornvme\Parameters\Device\ForcedPhysicalSectorSizeInBytes`.

Fuente: https://www.tomshardware.com/pc-components/ssds/windows-11-rockets-ssd-performance-to-new-heights-with-hacked-native-nvme-driver-up-to-85-percent-higher-random-workload-performance-in-some-tests

### 5.4 Copilot+ / Snapdragon X Elite / ARM gaming

**Finding #63 — ARM64 gaming = non-starter competitive 2026**: Civ IV 72.3 FPS Intel vs 21.5 Snapdragon. DOTA 2 106.6 vs 52.8. Vanguard/EAC problems. Detectar ARM64 = warning "no target gaming competitive".

**Finding #64 — BattlEye funciona ARM64 Prism emulator (24H2)**: Único anti-cheat mainstream compatible. Vanguard, Valorant, LoL, PUBG, Fortnite bloqueados.

### 5.5 Arrow Lake (Core Ultra 200S)

**Finding #65 — 24H2 + build 26100.2161 (KB5044384) fix**: Arrow Lake al launch tenía scheduling raro, variance alta. Resuelto en ese build.

**Finding #66 — Intel APO (Application Performance Optimizer)**: Real-time thread scheduling per-juego. Case-by-case enable. Lista dinámica via Intel Driver Assistant.

**Finding #67 — E-cores en Arrow Lake = 32% IPC gain vs Raptor Lake**: E-cores competent en gaming. Algunos juegos prefieren NO ultra-full chip por scheduling issues. Desactivar E-cores solo casos específicos.

### 5.6 USB hubs vs direct — números reales

**Finding #68 — Hub USB calidad = 0.1-0.3ms extra, imperceptible**: Para 1000Hz polling irrelevante.

**Finding #69 — Hubs malos forzan fallback 1000Hz → 125Hz**: Mouse 4000Hz + hub barato → degradación dramática. Para ultra-competitive, DIRECT motherboard.

**Finding #70 — Mouse + Keyboard + Headset en MISMO hub = jitter**: Separar buses USB físicos. Mouse puerto directo, keyboard puede hub.

### 5.7 Xbox controller latency

**Finding #71 — Bluetooth = +16-32ms vs dongle 2.4GHz**: Medible, mostly imperceptible casual. SPIKES 80-120ms ocasionales Bluetooth por packet scheduling.

**Finding #72 — Dongle Xbox obligatorio para wireless headset audio**: Bluetooth bandwidth insuficiente audio + input high-fidelity simultaneous.

---

## 6. Tweaks oscuros medibles

### 6.1 Interrupt Affinity Policy Tool (Microsoft)

**Finding #73 — Win11 mejoró vs Win10 affinity GPU latency**: Minor pero medible.

**Finding #74 — spddl/GoInterruptPolicy fork moderno**: UI mejor que tool MS. Recomendable integrar en app como wrapper.

**Finding #75 — GPU affinity a NO-core-0**: Core 0 maneja kernel work. Mover GPU a cores 2+ libera contention.

Fuentes:
- https://github.com/spddl/GoInterruptPolicy
- https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/interrupt-affinity-and-priority

### 6.2 Custom .pow power plans

**Finding #76 — Ultimate Performance = High Performance en desktop**: Ambos disable core parking + min 100%. Diferencia en laptops (battery transitions). Desktop puro = misma cosa.

**Finding #77 — mintyYuki/powerplans**: Repo con .pow custom per use-case (gaming, workstation, minimal). `powercfg -import "path.pow"`.

**Finding #78 — Bitsum Highest Performance = tuned P-cores preferential en hybrid**: Superior a defaults en Intel hybrid.

### 6.3 WPR/WPA/GPUView workflow

**Finding #79 — Workflow mínimo stutter**: WPR → First Level Triage + CPU + GPU + Desktop composition + Audio glitches + Video glitches → reproducir stutter ~10seg → Stop → Merged.etl → GPUView. Busca gap en rows + spikes Latency lane.

**Finding #80 — UIforETW (randomascii)**: Bruce Dawson (Google Chrome perf team). Simplifica ETW con UI moderna, pre-configured profiles. Más accesible que xperf raw.

Fuente: https://randomascii.wordpress.com/2015/04/14/uiforetw-windows-performance-made-easier/

### 6.4 Page file + NVMe modernos

**Finding #81 — Page file en drive separado = irrelevante con NVMe único**: Incluso SSD más rápido no compensa falta de RAM causando paging constante. Solución es más RAM.

**Finding #82 — PagefilePerUser tweak**: `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PagingFiles` con "?:\pagefile.sys 0 0" (system-managed) > fixed size. Excepción: DAW/pro audio necesita determinismo.

---

## 7. Drivers deep dives

### 7.1 NVCleanstall — components

**Finding #83 — Safe remove**: GeForce Experience, Telemetry, USB-C (solo VirtualLink), NVIDIA Container Services, Shield Support, Stereo 3D, Notebook Optimizations.

**Finding #84 — KEEP obligatorio**: Display Driver (core), HD Audio (HDMI/DP output), PhysX (algunos juegos legacy).

**Finding #85 — "Disable Driver Telemetry" expert flag ROMPE anti-cheat**: EAC/BattlEye detectan driver modificado. NO aplicar si competitivo online. Flag warning en app.

### 7.2 NVIDIA DCH vs Standard, Studio vs Game Ready

**Finding #86 — Studio = más estable, menos features bleeding edge**: Para workflow mixto o audio, Studio superior. Para últimos DLSS/Reflex día-1, Game Ready.

**Finding #87 — DCH único soportado moderno Win10 2004+**: Standard drivers legacy ya no existen.

### 7.3 AMD Adrenalin vs Pro Radeon (Enterprise)

**Finding #88 — Pro Radeon = stability-optimized, missing gaming features**: Anti-Lag 2, FSR latest, Fluid Motion Frames no en Pro. Usar Adrenalin unless workstation-first.

### 7.4 Chipset drivers (AMD 2026)

**Finding #89 — AMD 8.02.18.557 (Mar 2026)**: Latest stable AM4/AM5/mobile. 63MB. Gaming + scheduler fixes. PPM benefits X3D.

**Finding #90 — Downgrade path broken**: Una vez instalado 7.x+, NO se puede instalar 6.x sin uninstall first. Handle en app rollback.

### 7.5 Realtek UAD vs HDA vs Nahimic

**Finding #91 — UAD = Win10/11 64-bit only, OEM-specific, small**: HDA = Win7-10, universal, bloated. Para gaming low-latency, UAD standalone (sin SS3/Nahimic service) = ideal.

**Finding #92 — shibajee/realtek-uad-nahimic-mod**: UAD + Nahimic APO sin bloat service.

**Finding #93 — Nahimic Service alone = DPC latency ~1000-3000μs**: Medido LatencyMon. Disable via services.msc si no usás Nahimic.

---

## 8. Streaming / recording

### 8.1 OBS vs ShadowPlay vs ReLive

**Finding #94 — ShadowPlay = ~5% FPS hit, silicon dedicado NVENC**: Mínimo impacto.

**Finding #95 — AMD ReLive = paralelo, buffer a disk OR memory**: 15seg a 20min configurable. Save to memory evita SSD wear.

**Finding #96 — OBS Replay Buffer en RAM = 0 disk wear**: Activar "Keep in memory". Mejor para graba horas.

**Finding #97 — OBS CPU más agresivo que ShadowPlay**: Si single-PC streaming competitive, ShadowPlay/ReLive preferible sobre OBS x264.

### 8.2 NVENC AV1 vs H.264 (RTX 40+)

**Finding #98 — AV1 6000kbps ≈ H.264 10000kbps quality**: 40% bandwidth efficiency. Twitch aún no soporta AV1; YouTube + Discord Nitro sí.

**Finding #99 — NVENC AV1 = ~1.5-2dB PSNR higher vs H.264 mismo bitrate**: Medición Nvidia. Blind tests imperceptible hasta bitrates bajos.

**Finding #100 — Encoder silicon separado**: 0% impact FPS para NVENC/QuickSync. AMD AMF ligeramente más overhead.

### 8.3 Game Bar recording

**Finding #101 — Game Bar nuevo "Recording" = ShadowPlay/NVENC si NVIDIA**: Same silicon, worse UI. Usar ShadowPlay directo.

### 8.4 Lossless Scaling 3.0

**Finding #102 — LSFG 3.0 = -40% GPU load vs LSFG 2 (x2), -45% higher multipliers**: Major update Ene 2025.

**Finding #103 — LSFG latency penalty = no-go competitive**: Assetto Corsa Competizione prueba lag notable. Single-player only.

---

## 9. Tools poco conocidos

### 9.1 SpecialK — framework injection

**Finding #104 — SpecialK Reflex injection en juegos SIN soporte nativo**: DirectX 11/12, cualquier NVIDIA. SKIF (injection frontend) lista Epic/GOG/Steam/Xbox + custom.

**Finding #105 — Global injection mode vs Local (per-game)**: Global recomendado por compatibilidad. Kaldaien (dev) mantiene activo.

Fuente: https://wiki.special-k.info/en/SpecialK/Global

### 9.2 Nvidia Smooth Motion (nuevo, distinto de DLSS)

**Finding #106 — Driver-level frame interpolation, NO tiene motion vectors como DLSS FG**: Trade-off calidad vs universalidad. Works en todo game.

**Finding #107 — Auto-enables Low Latency Mode driver-level**: Injects Reflex en juegos que no lo soportan.

**Finding #108 — +40-50% FPS Cyberpunk test Nvidia (5 títulos)**: Cercano a DLSS FG nativo. Para juegos older/unsupported viable.

### 9.3 FrameView / PresentMon

**Finding #109 — FrameView usa PresentMon internamente**: Mismo backend, diferente UI/metrics. PresentMon 2.0 expone más (GPUBusy, animation error).

**Finding #110 — CapFrameX = frontend visual preferido community**: Mejor UI para análisis post-mortem.

### 9.4 AutoGpuAffinity (valleyofdoom)

**Finding #111 — Automated CPU affinity GPU benchmarking**: Itera todos cores, benchmarks cada uno, reporta best. Elimina guess work. Repo: github.com/valleyofdoom/AutoGpuAffinity

### 9.5 ParkControl + Process Lasso

**Finding #112 — Core parking desactivo = Win11 over-aggressive parking fix**: C6 sleep wake = 10-100ms spike. Real stutter source bursting loads.

**Finding #113 — Bitsum Highest Performance plan = tuned hybrid CPUs**: Supera Ultimate Performance en cores heterogéneos (Intel hybrid, 9950X3D).

---

## 10. Controversias 2026

### 10.1 Fullscreen vs Borderless Win11 24H2

**Finding #114 — Ambos muestran "hw composed: independent flip" en presentmon pero feel distinto**: En 24H2, user con cámara 240FPS reportó fullscreen smoother que borderless incluso con "optimizations for windowed games" ON. No hay consenso técnico del por qué.

**Finding #115 — Multimonitor = FSE sigue king**: Windowed causa 2nd monitor jitter/lag.

### 10.2 Reflex vs Anti-Lag 2 shootout

**Finding #116 — Anti-Lag 2 en CS2 = 11ms, Reflex mismo = sub-15ms range**: Paridad real cuando ambos game-integrated. Diferencia desaparece cuando Anti-Lag es driver-level vs Reflex game-level.

**Finding #117 — Anti-Lag+ original = anti-cheat ban CS2 (2023-2024)**: Anti-Lag 2 es "safe", game-integrated.

### 10.3 HAGS + Frame Generation dependency

**Finding #118 — NVIDIA DLSS FG (3 y 4) NO ACTIVA sin HAGS en Win11**: Driver-level enforce. UI puede decir "ON" pero no corre. Hard requirement.

**Finding #119 — AMD FSR 3 no requiere HAGS**: Ventaja AMD.

**Finding #120 — HAGS costo 2026 = ~0.3% FPS + ~1GB VRAM extra**: Casi-zero cost. Nvidia user = encender siempre (FG req); AMD opcional.

### 10.4 Windows Update driver delivery

**Finding #121 — `ExcludeWUDriversInQualityUpdate=1`**: Registry excluye drivers de quality updates manteniendo Windows updates. Path: `HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings`.

**Finding #122 — `SearchOrderConfig=0`**: Previene Windows Update driver search. Complementa anterior.

Combinado = Windows Update funciona, no sobrescribe drivers manuales. Solución mejor que "disable Windows Update" completo.

### 10.5 Linux gaming vs Windows 2026

**Finding #123 — CachyOS + Nobara benchmarks 2026 = dentro 5-10% Windows 11 AMD GPU titles**: Cyberpunk 2077, Space Marine 2 LIDEREN en Linux. Vulkan translation + schedulers optimizados.

**Finding #124 — Bazzite = "console-like" Fedora Atomic; CachyOS = Arch performance-first**: Diferentes filosofías.

**Finding #125 — Anti-cheat sigue bloqueando Linux**: Vanguard, EAC (parcial), BattlEye no Proton-ready en competitivo. NO recomendable Valorant/LoL/Fortnite Linux.

### 10.6 Frame Generation latency cost con MFG 6X (DLSS 4.5)

**Finding #126 — DLSS 4.5 MFG 6X (2026)**: 2nd gen Transformer Model + Dynamic MFG hasta 6x frames. Baseline 30fps → 180fps. Latency cost scales con multiplier — 2x manageable, 6x notable.

**Finding #127 — Reflex 2 Frame Warp en mismo chain**: Compensa latency cost del MFG. Net: playable incluso con 6X en RTX 5090.

---

## Notas finales — implementación app

**Prioridad findings automatizables con impacto medible:**

1. `GlobalTimerResolutionRequests` (Finding #5) — trivial registry, impacto alto
2. `ReservedCpuSets` (Finding #19) — 5-15% 1%-low improvement
3. `PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION` (#7) — hack moderno Win11
4. `nvmedisk.sys native hack` (#62) — +13% AS-SSD
5. `Interrupt Affinity GPU a NO-core-0` (#75)
6. `ExcludeWUDriversInQualityUpdate + SearchOrderConfig` (#121-122) — balance update safety
7. `Disable Recall/Copilot aggressive` (#50-51) — 500MB-1GB RAM recovery

**Detecciones hardware importantes:**
- Build 24H2/25H2 para advisory KB5066835 regression
- EAC pre-Abril 2024 = BSOD risk advisory
- ARM64 = gaming warning
- 9950X3D single vs 9950X3D2 = parking advice diferente
- Arrow Lake build < 26100.2161 = scheduler warning
- NVIDIA driver < 581.94 en 24H2 = advisory upgrade

**Warnings integrar:**
- Nahimic Service presente = DPC latency risk
- Intel Killer Performance Suite = uninstall advice
- NVIDIA "Disable Driver Telemetry" via NVCleanstall + competitive = block
- Reflex 2 Frame Warp requires RTX 50 (o PureDark demo for older)

---

## Fuentes clave

- [djdallmann/GamingPCSetup WINKERNEL](https://github.com/djdallmann/GamingPCSetup/blob/master/CONTENT/RESEARCH/WINKERNEL/README.md)
- [valleyofdoom/TimerResolution](https://github.com/valleyofdoom/TimerResolution)
- [valleyofdoom/PC-Tuning](https://github.com/valleyofdoom/PC-Tuning)
- [valleyofdoom/ReservedCpuSets](https://github.com/valleyofdoom/ReservedCpuSets)
- [BoringBoredom/PC-Optimization-Hub](https://github.com/BoringBoredom/PC-Optimization-Hub)
- [XHCI IMOD guide](https://github.com/BoringBoredom/PC-Optimization-Hub/blob/main/content/xhci%20imod/xhci%20imod.md)
- [Blur Busters Latency Reduction](https://forums.blurbusters.com/viewtopic.php?f=10&t=10986)
- [Blur Busters Windows 11 Borderless](https://forums.blurbusters.com/viewtopic.php?t=13776)
- [Tom's Hardware Win11 25H2 vs 24H2](https://www.tomshardware.com/software/windows/early-windows-11-25h2-benchmarks-confirm-the-update-provides-no-performance-improvements-over-24h2)
- [Windows Latest NVIDIA hotfix 581.94](https://www.windowslatest.com/2025/11/20/nvidia-confirms-windows-11-25h2-24h2-update-is-hurting-gaming-performance-releases-a-new-driver/)
- [Guru3D KB5066835 regression](https://www.guru3d.com/story/windows-11-kb5066835-update-triggers-major-gaming-performance-regression/)
- [Overclock.net 9950X3D core parking](https://www.overclock.net/threads/how-i-fixed-core-parking-on-my-9950x3d-and-taichi-lite.1815819/)
- [Overclock.net 9800X3D RAM tuning](https://www.overclock.net/threads/help-tuning-ryzen-9800x3d-g-skill-ddr5-6000-cl26-hynix-a-die-for-lowest-latency-1-1.1817608/)
- [spddl/GoInterruptPolicy](https://github.com/spddl/GoInterruptPolicy)
- [Gearspace Windows 11 audio](https://gearspace.com/board/music-computers/1432294-improving-real-time-audio-performance-windows-11-a.html)
- [NVIDIA Reflex 2 Frame Warp](https://www.nvidia.com/en-us/geforce/news/reflex-2-even-lower-latency-gameplay-with-frame-warp/)
- [NVIDIA DLSS 4.5 MFG](https://www.nvidia.com/en-us/geforce/news/dlss-4-5-dynamic-multi-frame-gen-6x-2nd-gen-transformer-super-res/)
- [AMD Anti-Lag 2](https://www.tomshardware.com/pc-components/gpu-drivers/amd-introduces-radeon-anti-lag-2-with-full-integration-for-counter-strike-2-the-nvidia-reflex-alternative-shouldnt-trigger-cheating-bans-this-time)
- [GamersNexus Animation Error](https://gamersnexus.net/gpus-cpus-deep-dive/fps-benchmarks-are-flawed-introducing-animation-error-engineering-discussion)
- [Tom's Hardware Native NVMe hack](https://www.tomshardware.com/pc-components/ssds/windows-11-rockets-ssd-performance-to-new-heights-with-hacked-native-nvme-driver-up-to-85-percent-higher-random-workload-performance-in-some-tests)
- [Tom's Hardware Lossless Scaling 3](https://www.tomshardware.com/video-games/pc-gaming/lossless-scaling-3-update-touts-greatly-improved-latency-and-performance-universal-frame-gen-tool-boasts-24-percent-reduced-latency)
- [SpecialK Wiki](https://wiki.special-k.info/en/SpecialK/Global)
- [VideoCardz PureDark Reflex 2](https://videocardz.com/newz/puredark-releases-free-demo-of-nvidia-reflex-2-frame-warp-works-on-rtx-20-gpus)
- [AMD Chipset 8.02.18.557](https://www.amd.com/en/resources/support-articles/release-notes/RN-RYZEN-CHIPSET-8-02-18-557.html)
- [Intel Core Ultra 200S perf update](https://community.intel.com/t5/Blogs/Tech-Innovation/Client/Field-Update-1-of-2-Intel-Core-Ultra-200S-Series-Performance/post/1650490)
- [Intel Arc Driver 32.0.101.8626](https://downloadmirror.intel.com/915256/ReleaseNotes_101.8626.pdf)
- [NVCleanstall TechPowerUp](https://www.techpowerup.com/nvcleanstall/)
- [Bitsum ParkControl](https://bitsum.com/parkcontrol/)
- [mintyYuki/powerplans](https://github.com/mintyYuki/powerplans)
- [UIforETW — Bruce Dawson](https://randomascii.wordpress.com/2015/04/14/uiforetw-windows-performance-made-easier/)
- [louzkk/Ghost-Optimizer](https://github.com/louzkk/Ghost-Optimizer)
- [simeononsecurity/Windows-Optimize-Harden-Debloat](https://github.com/simeononsecurity/Windows-Optimize-Harden-Debloat)
- [jointhearkanoid/playbook AME](https://github.com/jointhearkanoid/playbook)
- [8bit2qubit/XboxFullscreenExperienceTool](https://github.com/8bit2qubit/XboxFullscreenExperienceTool)
- [Notebookcheck CachyOS vs Windows 11](https://www.notebookcheck.net/CachyOS-vs-Windows-11-gaming-test-shows-Linux-leading-in-Cyberpunk-2077-Space-Marine-2-and-more.1262946.0.html)
- [WindowsForum 25H2 NVMe hack](https://windowsforum.com/threads/native-nvme-in-windows-11-25h2-performance-boost-via-nvmedisk-sys-registry-hack.394722/)
- [Hardware Times Smooth Motion](https://hardwaretimes.com/nvidia-smooth-motion-almost-doubles-fps-on-rtx-40-gpus-frame-generation-benchmark-tests/)
- [NVIDIA OBS AV1](https://blogs.nvidia.com/blog/av1-obs29-youtube/)
- [Windows Latest Xbox Mode](https://www.windowslatest.com/2026/03/11/microsoft-confirms-xbox-mode-for-windows-11-pcs-in-2026-and-i-tested-the-new-console-style-gaming-interface/)
