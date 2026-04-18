# Tweaks de GPU — NVIDIA y AMD (2026)

## Nota metodológica

Fuentes priorizadas: **Blur Busters** (latency), **GamersNexus**, **Digital Foundry**, **Hardware Unboxed**, **TechPowerUp**, **Tom's Hardware**, **Guru3D forums**, **NVIDIA/AMD oficial**, **PCGamingWiki**.

Estado del ecosistema (abril 2026): **DLSS 4.5** con Multi Frame Gen 6X salió el 31 marzo 2026 (RTX 50). **FSR 4** coexiste con DLSS 4. **Anti-Lag 2** requiere integración SDK (CS2, Dota 2, Ghost of Tsushima). **HYPR-RX** exclusivo RX 7000+/RDNA 3+.

---

## 1. NVIDIA

### Power Management Mode (Prefer Maximum Performance)
- **Aplicación**: NVCP → Manage 3D Settings → Power management mode → "Prefer maximum performance"
- **Impacto**: Elimina microstutters de ramp-up; 0-3% FPS adicional; +15-40W consumo idle
- **Consenso**: ALTO (desktop), no usar en laptop con batería
- **Recomendación**: INCLUIR automático en Competitivo/Balanceado

### Low Latency Mode (On vs Ultra vs Off)
- **Off** = default (queued frames); **On** = reduce queue a 1 frame; **Ultra** = elimina queue + auto-cap bajo refresh
- **Regla crítica**: si juego tiene **Reflex nativo → LLM=Off** (conflicto)
- **Impacto**: On reduce 5-15ms; Ultra +2-5ms extra + auto-cap
- **Recomendación**: Lógica condicional — Reflex detectado → LLM=Off; sin Reflex → Ultra competitivo, On otros

### NVIDIA Reflex + Boost
- **In-game setting** (CS2, Valorant, Apex, OW2, Fortnite, ~150 títulos)
- **Impacto**: Reducción 30-50% system latency. CS2: hasta 35% lower latency
- **Recomendación**: INCLUIR como "detectar y activar automático" en Competitivo

### G-SYNC + V-SYNC + Reflex Stack (Blur Busters Gold Standard)
- **Stack**: G-SYNC On + V-Sync On NVCP + FPS cap 3 bajo refresh. In-game V-Sync Off. Reflex On.
- **Cap**: 144Hz→141; 165Hz→162; 240Hz→237; 360Hz→357; 480Hz→477
- **Con Reflex**: Reflex ya aplica cap (~Hz-2) automático
- **Fuente**: [Blur Busters G-SYNC 101](https://blurbusters.com/gsync/gsync101-input-lag-tests-and-settings/)
- **Recomendación**: INCLUIR automático (detectar Hz, calcular cap, aplicar via RTSS/NVCP v3)

### Preferred Refresh Rate "Highest Available"
- **Aplicación**: NVCP → "Highest available"
- **Impacto**: Corrige juegos viejos que se quedan en 60Hz
- **Consenso**: MEDIO (algunos juegos se rompen)
- **Recomendación**: INCLUIR manual (toggle per-profile)

### Shader Cache Size → Unlimited / 100 GB
- **Aplicación**: NVCP → Shader Cache Size → Unlimited (driver 496.13+)
- **Impacto**: Elimina stutters 5-10 min al primer arranque tras cambio de juego. DX12 no afecta.
- **Consenso**: ALTO
- **Recomendación**: INCLUIR automático

### Threaded Optimization
- **Aplicación**: NVCP → Threaded Optimization → **Auto** (99% casos)
- **Impacto**: 0-2%, puede romper juegos OpenGL viejos
- **Recomendación**: Auto global; per-profile Off si hay problemas título viejo

### Texture Filtering Settings
- **Competitivo**: Quality=High Performance, Anisotropic Sample Opt=On, Neg LOD Bias=Clamp, Trilinear Opt=On
- **Calidad**: High Quality, todas las opts=Off
- **Impacto**: +1-3% FPS en High Performance; visual casi imperceptible en motion

### CUDA Force P2 State
- **Tool**: NVIDIA Profile Inspector (no expuesto en NVCP)
- **Qué hace**: Por default cargas CUDA bajan a P2 (memory downclock). Force OFF permite OC de memoria siempre.
- **Impacto**: +5-8% en juegos con CUDA/PhysX; solo si hay mem OC
- **Fuente**: [Babeltech P2 analysis](https://babeltechreviews.com/nvidia-cuda-force-p2-state/)
- **Consenso**: ALTO (overclockers)
- **Recomendación**: INCLUIR automático si usuario aplica mem OC

### MSI Mode para GPU (MSI_Util v3)
- **Qué hace**: Cambia interrupts de Line-Based a Message Signaled. Reduce DPC latency
- **Aplicación**: [MSI_Util v3](https://forums.guru3d.com/threads/windows-line-based-vs-message-signaled-based-interrupts-msi-tool.378044/) → Enable MSI NVIDIA display + audio → Reboot
- **Impacto**: -100-300μs DPC latency; gana más en Pascal/Turing (Ampere+ default MSI)
- **Consenso**: ALTO comunidad audiophile/low-latency
- **Riesgo**: No tocar controllers críticos (storage, USB)
- **Recomendación**: INCLUIR manual con warning

### NVIDIA Scanner (Auto-OC)
- **Tool**: MSI Afterburner OC Scanner
- **Impacto**: OC conservador +100-165 MHz core, +200 mem. 80-90% del OC manual
- **Recomendación**: INCLUIR como "OC safe-mode automático" one-click

---

## 2. AMD

### HYPR-RX (stack oficial)
- **GPU**: AMD RX 7000+ (RDNA 3) / Ryzen 7040+ iGPU
- **Stack**: AFMF 2 + RSR + Anti-Lag + Radeon Boost + Radeon Chill
- **Aplicación**: Adrenalin → Gaming → Graphics → HYPR-RX toggle
- **Impacto**: Duplica FPS percibidos (con FG) pero +15-30ms latencia
- **Consenso**: MEDIO (single-player AAA sí; competitivo NO)
- **Recomendación**: INCLUIR toggle por preset (Visual=On, Competitivo=Off)

### Anti-Lag 2 (Anti-Lag+)
- **Requisito**: Integración SDK (CS2, Dota 2, Ghost of Tsushima confirmados abril 2026)
- **Aplicación**: Auto con toggle "Anti-Lag" en Adrenalin
- **Impacto**: 20-40ms reducción, hasta 37% vs driver-only
- **Recomendación**: INCLUIR automático + detección per-game

### Radeon Chill vs Boost
- **Chill**: FPS cap dinámico según movimiento (30-300). Power saving.
- **Boost**: Reduce dinámicamente resolución en movimiento rápido.
- **No coexisten con Anti-Lag**
- **Recomendación**: SKIP en Competitivo; alternativa power saving laptop/eco

### Frame Pacing / Enhanced Sync vs FreeSync
- **Enhanced Sync**: V-Sync por arriba refresh, V-Sync Off por abajo
- **Combo óptimo**: FreeSync + Enhanced Sync (Enhanced solo fuera rango FreeSync)
- **Impacto**: -51% latencia vs V-Sync tradicional (Halo Infinite testing)
- **Recomendación**: INCLUIR como "AMD Gold Standard" (equivalente Blur Busters NVIDIA)

### Smart Access Memory / Resizable BAR
- **Aplicación**: BIOS → "Above 4G Decoding" + "Re-Size BAR Support"
- **Impacto**: +3-7% promedio, outliers AC Valhalla +20%
- **Recomendación**: DETECTAR estado y avisar (requiere BIOS)

### FSR 4 + AFMF 2
- **FSR 4**: requiere RDNA 4; rival DLSS 4
- **AFMF 2**: RDNA 3+; frame generation universal a nivel driver
- **Impacto**: FSR 4 +40-70% FPS con calidad cercana a DLSS 4; AFMF 2 duplica FPS percibidos con 20-40ms latencia
- **Recomendación**: INCLUIR toggles diferenciados en presets

### Radeon Image Sharpening (RIS)
- **Aplicación**: Adrenalin → Image Sharpening → 50-70%
- **Impacto**: <1% FPS cost, recupera claridad perdida en upscaling
- **Recomendación**: INCLUIR con slider default 60%

### Undervolt RDNA 3/4 (Adrenalin Curve)
- **Aplicación**: Adrenalin → Performance → Tuning → Custom → Voltage offset
- **Valores verificados**:
  - RX 7900 XTX: 1075mV stable; 80-100mV undervolt → -70W, -14°C hotspot
  - RX 7900 XT: 1050mV @ 2650MHz
  - RX 9070 XT: -100mV común, +10% FPS en algunos casos
- **Consenso**: ALTO
- **Recomendación**: INCLUIR wizard guiado (-50mV conservador → stress test → incrementar)

### Morphological Anti-Aliasing (MLAA)
- **Consenso**: BAJO (FSR/TAA/DLAA lo hacen obsoleto)
- **Recomendación**: SKIP

---

## 3. Cross-Vendor

### MSI Afterburner — Voltage Curve Editor
- **GPU**: NVIDIA (RDNA 4 no soportado aún; Blackwell requiere NV-UV companion)
- **Workflow RTX 40**:
  1. Baseline: Power Limit 100%, Core/Mem 0
  2. Ctrl+F Curve Editor
  3. Voltaje objetivo (0.925-0.975V común)
  4. Shift+Click-drag para subir frequency point
  5. Flatten curve desde ese voltaje hacia la derecha
  6. Apply → test 30-60min
- **Valores verificados**:
  - RTX 4090: 2745-2800 MHz @ 0.950-0.975V + mem +1200-1600
  - RTX 4080: 2700 MHz @ 0.950V
  - RTX 5090: 2977 MHz @ 0.950V → ~450W vs 575W stock, 96-98% performance
  - RTX 5080/5070 Ti/5070: NV-UV one-click recomendado
- **Recomendación**: INCLUIR wizard presets safe/aggressive

### Fan Curves Custom
- **Zonas**:
  - Silencio: 0-55°C → 30-40% fan
  - Trabajo: 55-70°C → rampa lineal 60-70%
  - Estrés: 70°C+ → 80-100%
- **Impacto**: -5-8°C con noise aceptable
- **Recomendación**: INCLUIR presets Silent/Balanced/Aggressive

### Memory Overclocking GDDR6/6X
- **Límites seguros**:
  - **GDDR6X**: TJmax 95°C ideal, 110°C throttle NVIDIA. Común +1000-1600 MHz offset. Throughput mejora hasta 500MHz over, después ECC reduce FPS real.
  - **GDDR6**: Más frío, más headroom. +800-1500 MHz común
  - **GDDR7 (RTX 50)**: Nuevas variables, aún en caracterización
- **Recomendación**: INCLUIR slider con hard cap por arquitectura + alerta VRAM temp > 90°C

### FPS Limiting — dónde aplicarlo
**Orden preferencia (latencia)**:
1. **In-game** (Reflex/Anti-Lag 2 engine-level): mejor latencia
2. **Driver-level** (NVCP v3 / Adrenalin FRTC): segundo
3. **RTSS async**: peor latencia (~1 frametime+) **pero** mejor frametime consistency

**Regla**: GPU al 100% → in-game. GPU con headroom → RTSS superior frame pacing.

### DDU Workflow (oficial Wagnardsoft)
1. Descargar nuevo driver antes
2. Desconectar internet
3. Reiniciar en Safe Mode (Shift+Restart → Startup Settings → 4)
4. Extraer DDU a carpeta local (NO network drive)
5. Launch DDU → Select GPU + vendor
6. **Clean and restart**
7. Instalar nuevo driver
8. Reconectar internet

- **Recomendación**: INCLUIR launcher/detector de DDU; automatizar boot-to-safe-mode (bcdedit /set {current} safeboot minimal)

---

## 4. Registry / Windows Tweaks que afectan GPU

### Hardware-Accelerated GPU Scheduling (HAGS)
- **Aplicación**: Settings → Display → Graphics → HAGS On/Off
- **Estado 2026**: Performance gain ~0.3% promedio, consume hasta 1GB VRAM extra. Beneficio principal = input latency + frame pacing CPU-bound. **Frame Warp DLSS 4 depende de HAGS.**
- **Tradeoff**: GPUs 8GB VRAM pueden sufrir. OBS recomienda off.
- **Recomendación**: INCLUIR toggle + warning <12GB VRAM. Auto si detecta DLSS 4 Frame Warp.

### Multi-Plane Overlay (MPO)
- **Registry**: `HKLM\SOFTWARE\Microsoft\Windows\Dwm\OverlayTestMode = 5` (DWORD)
- **Impacto**: Fix stutters Chrome/browsers, fullscreen video, juegos
- **Fuente**: [NVIDIA oficial MPO](https://nvidia.custhelp.com/app/answers/detail/a_id/5157/)
- **Consenso**: ALTO (casi siempre beneficial)
- **Recomendación**: INCLUIR automático con backup

### Fullscreen Optimizations (FSO)
- **Aplicación per-exe**: Right-click EXE → Properties → Compatibility → "Disable fullscreen optimizations"
- **Registry global**: `HKCU\System\GameConfigStore\GameDVR_FSEBehaviorMode = 2`
- **Estado 2026**: Debate. Blur Busters: similar latency. Algunos juegos necesitan FSO off (Elden Ring, CS:GO legacy, OpenGL)
- **Recomendación**: INCLUIR toggle per-game con DB títulos conocidos

### TdrDelay (Timeout Detection Recovery)
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\TdrDelay = 60` + `TdrDdiDelay = 60`
- **Impacto**: Permite OCs agresivos sin "Display driver crashed". NO fix real — band-aid.
- **Recomendación**: INCLUIR manual con warning — perfil "content creation" (60) vs "gaming" (default=2)

### GPU P-State Force (DisableDynamicPstate)
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\DisableDynamicPstate = 1`
- **Impacto**: Similar Prefer Max Perf pero más agresivo. Idle wattage +30-80W.
- **Recomendación**: SKIP default; exponer como "extreme mode"

---

## 5. Tabla Comparativa — Low Latency Modes

| Modo | GPU | Integración | Reducción latencia | FPS cap auto | Compatible con | Mejor para |
|------|-----|-------------|-------------------|--------------|----------------|-----------|
| **NVIDIA Reflex (engine)** | NV | SDK in-game | 25-50ms (-30-50%) | Sí (~Hz-2) | G-SYNC + V-Sync On | Competitivo, juegos con Reflex |
| **NVIDIA Reflex + Boost** | NV | SDK in-game | Reflex + 2-5ms | Sí | G-SYNC + V-Sync On | Esports (power cost) |
| **NVIDIA LLM Ultra** | NV | NVCP | 10-20ms | Sí (Hz-5-7 con G-SYNC) | G-SYNC; NO con Reflex | Juegos sin Reflex |
| **NVIDIA LLM On** | NV | NVCP | 5-12ms | No | G-SYNC; NO con Reflex | Juegos viejos |
| **AMD Anti-Lag 2** | AMD | SDK in-game | 30-50ms (-37%) | Sí | FreeSync + Enhanced | Competitivo (CS2, Dota2, GoT) |
| **AMD Anti-Lag driver** | AMD | Driver | 10-25ms | No | FreeSync; no con Chill | Fallback global |
| **In-game FPS cap** | Ambas | Engine | Depende | N/A | Cualquiera | Baseline primero |
| **RTSS async cap** | Ambas | External | Negativo (~1 frametime+) | N/A | Cualquiera | Frame pacing, NO latencia |
| **Driver FPS cap** | Ambas | Driver | Neutral | N/A | Cualquiera | Cuando no hay in-game cap |

**Tier competitivo**: Reflex+Boost > Anti-Lag 2 > Reflex > LLM Ultra > Anti-Lag driver > LLM On > nothing.

---

## 6. Perfiles Preset Recomendados

### "Competitivo" (FPS máx, latencia mín)
**NVCP/Adrenalin:**
- Power Management: Prefer Maximum Performance / High
- Texture Filtering Quality: High Performance
- Low Latency Mode: Ultra (sin Reflex) / Off (con Reflex)
- Reflex/Anti-Lag 2: On in-game
- V-Sync: Off in-game, On driver
- G-SYNC/FreeSync: On
- Max Frame Rate: Refresh-3 (o Reflex auto)
- Shader Cache Size: Unlimited
- AFMF/Frame Gen: **OFF**
- DLSS/FSR: Quality si hace falta
- HAGS: On
- Anisotropic: 8x

**OC:** Core UV -50 a -75mV, Mem +800 NV / +100-200 AMD, Power 100-110%, Fan aggressive
**Windows:** MPO disabled, FSO per-exe disabled, MSI Mode GPU, Game Mode On, Game Bar DVR Off

### "Visual" (calidad máx)
- Power: Optimal / Default AMD
- Texture Filtering: High Quality
- DLSS 4.5 Quality / FSR 4 Quality
- **Frame Generation: ON** (DLSS MFG 2X-6X RTX 50, DLSS FG RTX 40, AFMF 2 High AMD)
- Reflex: On (mitiga latencia FG)
- V-Sync: On driver, Off in-game
- G-SYNC/FreeSync: On
- Max Frame Rate: Refresh-3
- Anisotropic: 16x
- DLAA si GPU tiene headroom
- HDR: On + RTX HDR en SDR
- HAGS: On
- HYPR-RX: On AMD
- RIS: 60%

### "Balanceado"
- Power: Optimal
- LLM: On (sin Reflex)
- Reflex: On in-game
- DLSS/FSR Quality si gain >20%
- FG: On solo single-player AAA
- Anisotropic: 16x
- V-Sync + G-SYNC: On
- Cap refresh-3
- HAGS: On
- UV moderado

### "Eco" (laptop / power saving)
- Power: Adaptive
- Radeon Chill: On rango custom (40-120 FPS)
- UV agresivo
- Power Limit 80%
- DLSS/FSR Performance
- Frame cap 60/90
- Fan silent

---

## 7. Tools que la app debe invocar o incluir

### Tier 1 — Core (must have)
| Tool | Uso | URL |
|------|-----|-----|
| **NVIDIA Profile Inspector** (Orbmu2k + xHybred Revamped) | Settings hidden (Force P2, shader cache unlimited, preferred refresh, RTX HDR, CustomSettingNames.xml) | [Orbmu2k/nvidiaProfileInspector](https://github.com/Orbmu2k/nvidiaProfileInspector), [xHybred Revamped](https://github.com/xHybred/NvidiaProfileInspectorRevamped) |
| **MSI Afterburner** | Undervolt curve, OC, fan | msi.com |
| **RivaTuner Statistics Server (RTSS)** | FPS limiter, Reflex injection, frametime graph | guru3d.com |
| **DDU** | Driver clean uninstall | [Wagnardsoft](https://www.wagnardsoft.com/content/How-use-Display-Driver-Uninstaller-DDU-Guide-Tutorial) |
| **MSI_Util v3** | MSI interrupts toggle | [Guru3D thread](https://forums.guru3d.com/threads/windows-line-based-vs-message-signaled-based-interrupts-msi-tool.378044/) |
| **CRU** | Custom res/refresh, FreeSync range | [customresolutionutility.net](https://customresolutionutility.net/) |

### Tier 2 — Nice to have
| Tool | Uso |
|------|-----|
| **NV-UV** | One-click undervolt RTX 50 (Blackwell) |
| **HWInfo64** | Monitoring VRAM temp, hotspot, DPC |
| **LatencyMon** | Validar DPC post-tweaks |
| **GPU-Z** | Sensor, render test, BIOS info |
| **OCAT / PresentMon** | Frametime capture |
| **AMD Adrenalin profile export** | AMD equivalente a .nip |

### Tier 3 — Avanzado
- **Process Lasso**: CPU affinity / priority (pinear games a P-cores Intel)
- **ISLC (Intelligent Standby List Cleaner)**: limpia memory standby

### Registry automations (con backup)
```reg
# MPO disable
HKLM\SOFTWARE\Microsoft\Windows\Dwm\OverlayTestMode = 5 (DWORD)

# Game DVR off
HKCU\System\GameConfigStore\GameDVR_Enabled = 0
HKCU\System\GameConfigStore\GameDVR_FSEBehaviorMode = 2

# TdrDelay (solo creator)
HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\TdrDelay = 60
HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\TdrDdiDelay = 60

# FSO per-exe
HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers\<path> = "~ DISABLEDXMAXIMIZEDWINDOWEDMODE"
```

---

## 8. Recomendaciones finales diseño de app

1. **Backup/restore automático** (registry export .reg por tweak, export NVCP/Adrenalin previos)
2. **Detección hardware primero**: vendor, arquitectura, VRAM, monitor Hz, G-SYNC/FreeSync — filtrar tweaks aplicables
3. **Presets editables**: toggle individual con indicador ALTO/MEDIO/RIESGOSO
4. **Telemetría post-tweak**: PresentMon/OCAT para validar frametime/FPS antes/después + rollback si empeoró
5. **Post-2026**: soporte **Xbox Mode Windows 11** (3-8% FPS CPU-bound), **NV-UV** Blackwell, **HYPR-RX Eco**
6. **DB perfiles community**: .nip de [NvidiaProfileStore](https://github.com/ezekiel24r/NvidiaProfileStore) para top 200 juegos
7. **Warning crítico Reflex vs LLM**: si Reflex in-game + LLM On/Ultra driver → mostrar banner y desactivar uno

---

## Fuentes clave

- [Blur Busters G-SYNC 101](https://blurbusters.com/gsync/gsync101-input-lag-tests-and-settings/)
- [Blur Busters HOWTO Low-Lag VSYNC ON](https://blurbusters.com/howto-low-lag-vsync-on/)
- [NVIDIA System Latency Optimization Guide](https://www.nvidia.com/en-us/geforce/guides/gfecnt/202010/system-latency-optimization-guide/)
- [NVIDIA DLSS 4.5 MFG 6X](https://www.nvidia.com/en-us/geforce/news/dlss-4-5-dynamic-multi-frame-generation-6x-mode-released/)
- [Microsoft DirectX HAGS](https://devblogs.microsoft.com/directx/hardware-accelerated-gpu-scheduling/)
- [AMD HYPR-RX](https://www.amd.com/en/products/software/adrenalin/hypr-rx.html)
- [AMD Anti-Lag FAQ](https://www.amd.com/en/resources/support-articles/faqs/DH3-033.html)
- [TechSpot FSR 4 vs DLSS 4](https://www.techspot.com/article/2976-amd-fsr4-4k-upscaling/)
- [GamersNexus AMD Fake Frame FSR 4](https://gamersnexus.net/gpus/amd-fake-frame-image-quality-afmf-fsr-4-vs-fsr-31-comparison)
- [Guru3D MSI Util v3](https://forums.guru3d.com/threads/windows-line-based-vs-message-signaled-based-interrupts-msi-tool.378044/)
- [Wagnardsoft DDU Guide](https://www.wagnardsoft.com/content/How-use-Display-Driver-Uninstaller-DDU-Guide-Tutorial)
- [MSI Afterburner Overclocking Guide](https://www.msi.com/blog/msi-afterburner-overclocking-undervolting-guide)
- [Tom's Hardware DLSS 4.5 CES 2026](https://www.tomshardware.com/pc-components/cpus/nvidia-introduces-dlss-4-5-and-multi-frame-generation-6x-at-ces-2026-updated-models-can-generate-higher-quality-upscaled-frames-and-more-of-them-dynamically)
- [igor's Lab GDDR6X 100°C](https://www.igorslab.de/en/gddr6x-am-limit-ueber-100-grad-bei-der-geforce-rtx-3080-fe-im-chip-gemessen-2/)
- [HWCooling RX 9070 XT tuning](https://www.hwcooling.net/en/tuning-gigabyte-radeon-rx-9070-xt-performance-in-amd-software/)
- [VideoCardz NV-UV](https://videocardz.com/newz/nv-uv-brings-one-click-undervolting-to-geforce-rtx-50-gpus)
- [Orbmu2k/nvidiaProfileInspector](https://github.com/Orbmu2k/nvidiaProfileInspector)
- [NvidiaProfileStore](https://github.com/ezekiel24r/NvidiaProfileStore)
- [customresolutionutility.net CRU](https://customresolutionutility.net/)
- [Babeltech CUDA Force P2](https://babeltechreviews.com/nvidia-cuda-force-p2-state/)
- [PCGamingWiki NPI](https://www.pcgamingwiki.com/wiki/Nvidia_Profile_Inspector)
