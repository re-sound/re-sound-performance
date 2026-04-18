# Apex Legends Optimization — Tab Apex

**Fecha:** 2026-04
**Contexto:** Source engine modificado (Respawn Titan), EAC via EOS (EasyAntiCheat Epic Online Services), servidor 20Hz, DX12 default desde 2025 (DX11 removed). Season 28 "Breach" (Feb 2026) actual.

**EA App bloquea launch options con `+`**: `+exec`, `+fps_max`, `+m_rawinput` dan error. Steam los acepta. Workaround: shortcut directo a `r5apex.exe` con argumentos en Target, o Steam exclusivo.

---

## 1. Launch Options 2026 (Steam)

| Flag | Función | Impacto | Consenso | EAC |
|------|---------|---------|----------|-----|
| `+fps_max 0` | Sin cap interno | Permite 500+ | ALTO | OK |
| `+fps_max unlimited` | Sinónimo moderno | Idéntico | ALTO | OK |
| `+fps_max_unfocused 60` | **NO existe oficialmente**. Apex no lo soporta. Usar NVCP "Background Application Max Frame Rate" | — | FALSO | — |
| `-novid` | Skip intros | -3/-5s loading | ALTO | OK |
| `-dev` | Skip intro + habilita CVAR load via autoexec. NO abre dev console (disabled public build) | Loading + CVAR unlock | ALTO | OK |
| `-high` | Prioridad High proceso | +1/3 FPS CPU-bound, inestabilidad ocasional. EAC se salta a veces — registry trick más confiable | MEDIO | OK |
| `-threads N` | Fuerza N threads Source | Apex usa 1-2 cores; >6-8 no ayuda. Puede causar stutter | BAJO (no recomendado) | OK |
| `-preload` / `cl_forcepreload 1` | Precarga texturas | +loading, reduce stutter drop ship en algunos. En BR puede ser contraproducente | MEDIO | OK |
| `-forcenovsync` | Fuerza off VSync launch | Redundante con in-game | ALTO | OK |
| `+m_rawinput 1` | Raw input | Mandatorio pros | ALTO | OK |
| `-fullscreen` | Exclusive Fullscreen | Menor input lag vs borderless | ALTO | OK |
| `-windowed` / `-noborder` | Ventana / Borderless | — | — | OK |
| `+exec autoexec` | Carga `cfg/autoexec.cfg` | Fundamental | ALTO | OK |
| `-softparticlesdefaultoff` | Soft particles off default | Micro FPS | BAJO | OK |
| `-nomansky` | Bug fix viejo | Innecesario en 2026 | BAJO (deprecated) | OK |
| `+cl_showfps 1`/`4` | FPS counter | Debug | ALTO | OK |
| `-cpuCount=X -exthreads=X` | **NO son flags Source/Apex** — Dunia/Ubisoft. No funcionan | — | MITO | — |
| `-eac_launcher_settings` | Flag DX12 beta viejo. **Obsoleto 2026**, causa crashes | — | DEPRECATED | — |
| `+mat_letterbox_aspect_goal 0` | Elimina black bars en res no-16:9 (4:3/5:4) | Crítico stretched | ALTO stretched | OK |
| `+mat_letterbox_aspect_threshold 0` | Complemento | — | ALTO stretched | OK |
| `-freq 144/240/360` | Fuerza refresh rate launch | Si monitor default mal | ALTO | OK |
| `-dxlevel 95` | Fuerza DX9 legacy | **NO funciona**, DX12 only 2026 | DEPRECATED | — |
| `+lobby_max_fps 0` | Quita cap lobby (144) | Reduce calor/ruido | ALTO | OK |

**Stack recomendado 2026 (Steam):**
```
-novid -dev +fps_max 0 +m_rawinput 1 -forcenovsync -fullscreen +exec autoexec +mat_letterbox_aspect_goal 0 +mat_letterbox_aspect_threshold 0
```

---

## 2. autoexec.cfg Comentado

**Ubicación:**
- Steam: `\Steam\steamapps\common\Apex Legends\cfg\autoexec.cfg`
- Origin/EA: `\Origin Games\Apex\cfg\autoexec.cfg`
- Requiere `+exec autoexec`. EA App bloquea `+`, usar shortcut directo o Steam.

```cfg
// =============== NETWORK ===============
rate 786432              // packet size max (default 65536). Apex 20Hz server
cl_cmdrate 40            // client→server cmds/s. Default 20. Cap real 40
cl_updaterate_mp 40      // server→client updates/s. Default 20
cl_interp 0              // minimal interp, Apex clampea
cl_interp_ratio 1        // 1 = ventana mínima
cl_lagcompensation 1
cl_smooth 0              // client prediction error smoothing off (pros)
cl_smoothtime 0.01
cl_pred_optimize 2
cl_timeout 30
cl_resend 6
cl_cmdbackup 2
net_compresspackets 1
net_maxcleartime 0.020346

// =============== MOUSE ===============
m_rawinput 1
m_acceleration 0
m_customaccel 0
m_filter 0
mouse_sensitivity 1.1      // ajustar eDPI personal
mouse_use_per_scope_sensitivity_scalars 0

// =============== AUDIO ===============
miles_channels 2            // 2=stereo, 6=5.1, 8=7.1 (headphone = 2)
sound_num_speakers 2
miles_occlusion 0           // disable sound occlusion walls
miles_occlusion_force 0
miles_occlusion_partial 0
miles_nonactor_occlusion 0
snd_mixahead 0.05
snd_async_fullyasync 1
snd_musicvolume 0           // CRÍTICO: música 0
sound_volume_music_game 0
sound_volume_music_lobby 0
cl_footstep_event_max_dist 4000  // default 2500, escucha pasos lejos
snd_setmixer PlayerFootsteps vol 0.1   // tus pasos bajos
snd_setmixer GlobalFootsteps vol 1.2   // pasos enemigos altos
snd_headphone_pan_exponent 2
sound_without_focus 1                   // sigue sonando tab-out

// =============== VIDEO ===============
fps_max 0
mat_queue_mode 2            // multi-threaded rendering obligatorio
gfx_nvnUseLowLatency 1      // NVIDIA Reflex ON
gfx_nvnUseLowLatencyBoost 1 // Reflex Boost ON
r_dynamic 0
mat_disable_bloom 1
cl_ragdoll_maxcount 0
cl_phys_props_enable 0
cl_phys_props_max 0
cl_ejectbrass 0
cl_show_splashes 0
cl_jiggle_bone_framerate_cutoff 0
mat_depthfeather_enable 0
mat_screen_blur_enabled 0
r_blurmenubg 0
fog_enable 0
fog_enableskybox 1
violence_ablood 0
violence_agibs 0
mp_usehwmmodels -1
mp_usehwmvcds -1

// =============== HUD / GAMEPLAY ===============
cl_fovScale 1.55            // 1.0=70, 1.27=90, 1.55=110, 1.7=120
fov_disableAbilityScaling 1
hud_setting_minimapRotate 1
hud_setting_adsDof 0
hud_setting_damageTextStyle 1
hud_setting_pingDoubleTapEnemy 1
sprint_view_shake_style 1
ordnanceSwapSelectCooldown 0
sidearmSwapSelectCooldown 0
sidearmSwapSelectDoubleTapTime 0
colorblind_mode 3           // 3=TRITANOPIA pro meta
reticle_color "0 255 0"
chroma_enable 0
net_netGraph2 1
cl_showfps 4
cl_showpos 1

// =============== COLOR / EXPOSURE (controversial) ===============
mat_light_edit 1
map_settings_override 1
mat_autoexposure_override_min_max 1
mat_autoexposure_min 1.9
mat_autoexposure_max 1.9
mat_autoexposure_min_multiplier 1.7
mat_autoexposure_max_multiplier 1.7
mat_fullbright 1            // self-illumination (puede parchearse)
mat_hide_sun_in_last_cascade 1

// =============== BINDS ÚTILES ===============
bind "=" "exec autoexec"
bind "MWHEELDOWN" "+jump"
bind "SPACE" "+jump"
bind "F" "+use; +use_long"
bind "4" "use_consumable SHIELD_LARGE; use_consumable SHIELD_SMALL"
bind "5" "use_consumable HEALTH_LARGE; use_consumable HEALTH_SMALL"
bind "MOUSE4" "+attack; -attack"      // single shot, NO macro automation
```

**RIESGO EAC:** `miles_channels 2` con headset 7.1 no es banneable pero reduce info. `mat_fullbright 1` parchado en patches pasados. **Scripts movimiento (superglide.cfg con multi-comando)**: zona gris. EA Forums reports 2024-2025 de bans aislados por autoexec agresivos. Respawn nunca declaró trigger específico.

---

## 3. videoconfig.txt Avanzado

**Ubicación:** `%USERPROFILE%\Saved Games\Respawn\Apex\local\videoconfig.txt`

**Flujo crítico:**
1. Cerrar Apex
2. Editar `videoconfig.txt`
3. **Botón derecho → Properties → Read-only** (si no, juego sobreescribe)

**Config version actual:** `7` default u `8` (streaming budget dynamic).

```
"VideoConfig"
{
    // === RESOLUTION / WINDOW ===
    "setting.defaultres"                    "1920"
    "setting.defaultresheight"              "1080"
    "setting.fullscreen"                    "1"      // 1=exclusive
    "setting.nowindowborder"                "0"
    "setting.last_display_width"            "1920"
    "setting.last_display_height"           "1080"

    // === SHADOWS ===
    "setting.shadow_enable"                 "0"
    "setting.shadow_depth_dimen_min"        "0"
    "setting.shadow_depth_upres_factor_max" "0"
    "setting.shadow_maxdynamic"             "0"
    "setting.csm_enabled"                   "0"      // CSM OFF (Hal tweet famoso)
    "setting.csm_coverage"                  "0"
    "setting.csm_cascade_res"               "0"
    "setting.new_shadow_settings"           "0"

    // === PARTICLES / RAGDOLLS / DECALS ===
    "setting.cl_gib_allow"                  "0"
    "setting.cl_ragdoll_maxcount"           "0"
    "setting.cl_ragdoll_self_collision"     "0"
    "setting.cl_particle_fallback_base"     "3"      // pros razonables; agresivos -999999
    "setting.cl_particle_fallback_multiplier" "2"
    "setting.particle_cpu_level"            "0"
    "setting.r_createmodeldecals"           "0"
    "setting.r_decals"                      "0"
    "setting.r_lod_switch_scale"            "0.4"    // < 0.3 causa model pop-in

    // === TEXTURES ===
    "setting.mat_forceaniso"                "0"      // 0=bilinear, 2/4/8/16
    "setting.mat_mip_linear"                "0"
    "setting.mat_picmip"                    "2"      // -1 ultra, 0 high, 1 med, 2 low, 4 potato
    "setting.stream_memory"                 "160000" // KB. 0=min, 160000≈1.5GB. VRAM-dep
    // Ajustar: 2GB VRAM=Low, 4GB=Medium, 6GB+=High. "0" = Very Low controversial

    // === AA / VSYNC / RENDER ===
    "setting.mat_antialias_mode"            "0"      // 0=none, 6=TSAA low, 10=TSAA med
    "setting.mat_vsync_mode"                "0"
    "setting.mat_backbuffer_count"          "1"      // 1=double buffer
    "setting.dvs_enable"                    "0"      // Dynamic Video Scaling OFF
    "setting.dvs_gpuframetime_min"          "15000"
    "setting.dvs_gpuframetime_max"          "16500"
    "setting.dvs_supersample_enable"        "0"

    // === LIGHTING / AO ===
    "setting.ssao_enabled"                  "0"
    "setting.ssao_downsample"               "3"
    "setting.volumetric_lighting"           "0"      // enorme ganancia FPS
    "setting.volumetric_fog"                "0"

    // === MISC ===
    "setting.fadeDistScale"                 "1.000000"
    "setting.gamma"                         "1.000000"
    "setting.configversion"                 "7"
    "setting.set_dress_level"               "1"      // decoración. 0 puede causar artifacts
}
```

**Tweaks fuera del menú:**
- `mat_depthfeather_enable 0` → DOF ADS
- `stream_memory 0` → texture streaming VERY LOW
- `mat_picmip 2` → mipmap level manual
- `r_lod_switch_scale 0.3-0.4` → LOD agresivo
- `csm_enabled 0` → disable CSM sin lockear shadow_enable

**EAC risk:** ninguno flag directo. Valores extremos (picmip 999999, particle_fallback -999999) son configs "agresivos" — reviews EAC al detectar anomaly extrema.

---

## 4. settings.cfg (Keybinds/Client)

**Ubicación:** `%USERPROFILE%\Saved Games\Respawn\Apex\local\settings.cfg`

Se escribe automático al cambiar in-game. Editable para:
- Binds que menú no expone
- "Thirst bind" (auto-switch weapon post-knockdown, no macro)
- Cambios persistentes de toggles

**No poner read-only.** Editar con juego cerrado.

---

## 5. In-Game Video Settings (ProSettings 89 pros)

| Setting | Valor recomendado | % Pros | FPS impact |
|---------|-------------------|--------|-----------|
| Display Mode | Full Screen (exclusive) | ~100% | Input lag↓ |
| Aspect Ratio | 16:9 nativo o 4:3/16:10 stretched | 60/40 | Stretched = models wider |
| Resolution | 1920x1080 | 77% | — |
| — stretched | 1440x1080 (Hal), 1728x1080 (Faide), 1680x1050 (Hakis/Sweet) | — | — |
| FOV | 110 | 50%+ | 104 target prio |
| FOV Ability Scaling | Disabled | ~100% | — |
| Color Blind Mode | **Tritanopia (3)** | meta | Visibility |
| V-Sync | Disabled | 100% | Input lag |
| Adaptive Resolution FPS Target | 0 | 100% | — |
| Adaptive Supersampling | Disabled | 100% | — |
| Anti-Aliasing | **None** | mayoría | +10-15% FPS; TSAA low alt |
| Texture Streaming Budget | Medium (o Very Low controversial) | 50/30 | 1% lows ↓ VRAM bajo |
| Texture Filtering | Bilinear o Anisotropic 2x | Bilinear pros (Hal), 2x mixto | Mínimo |
| Ambient Occlusion Quality | Disabled | 100% | +10-15% FPS |
| Sun Shadow Coverage | Low | 100% | — |
| Sun Shadow Detail | Low | 100% | — |
| Spot Shadow Detail | Disabled | ~100% | — |
| Volumetric Lighting | Disabled | 100% | **Biggest single FPS gain** |
| Dynamic Spot Shadows | Disabled | ~100% | — |
| Model Detail | Low (algunos Faide High) | mixto | High = cuerpos más claros |
| Effects Detail | Low | 100% | — |
| Impact Marks | Low o Disabled | 100% | Cosmético |
| Ragdolls | Low | ~100% | — |
| Reflex Low Latency | **Enabled + Boost** | 100% NVIDIA | -5/-30ms |

**Refresh rate:** 240Hz = 83% pros. 360Hz/480Hz OLED subiendo 2026 (Sony INZONE M10S 480Hz que usa ImperialHal).

**Stretched:** Respawn oficialmente **permite stretched en tournaments** (ALGS). 16:9 más "correcto" según Respawn para hitboxes pero stretched legal.

---

## 6. NVIDIA Profile Inspector — `r5apex.exe`

| Setting | Valor | Razón |
|---------|-------|-------|
| Power management mode | **Prefer maximum performance** | — |
| Low Latency Mode | **Off** | Reflex in-game hace mejor |
| Vertical sync | Off (o On con GSync + cap 3 below refresh) | — |
| Preferred refresh rate | Highest available | — |
| Maximum pre-rendered frames | 1 | Irrelevante con Reflex |
| Shader Cache Size | Unlimited | Reduce compile stutter |
| Threaded Optimization | **On** | Apex Source engine multi-thread-friendly |
| Texture filtering Anisotropic Sample opt | On | — |
| Texture filtering Negative LOD bias | Allow | — |
| Texture filtering Quality | **High Performance** | — |
| Texture filtering Trilinear opt | On | — |
| Anisotropic filtering | Application-controlled u 8x override | — |
| CUDA — GPUs | All | — |
| **Background Application Max Frame Rate** | 60 | **Reemplaza `fps_max_unfocused` inexistente** |
| DSR Factors | None | — |
| Monitor Technology | G-SYNC si aplica | — |

**Caveat EA:** "NVIDIA Profile Inspector tweaks are allowed, but not if your settings surpass the in-game ones" (EA Forums). En práctica nadie baneado por NVCP/NPI, pero evitar Negative LOD bias extremos.

---

## 7. AMD Adrenalin — Per-Game Apex

| Setting | Valor | Nota |
|---------|-------|------|
| **Anti-Lag** (no Anti-Lag+) | **Enabled** | Anti-Lag+ causó bans CS2/Apex 2023-2024. Anti-Lag 2 es **game-integrated SDK** pero Apex **NO soporta Anti-Lag 2 aún** → solo Anti-Lag clásico |
| Anti-Lag+ | **NEVER** | Historial bans EAC |
| HYPR-RX | **Off** | Bundle activa FSR+Boost+Anti-Lag, impredecible EAC |
| Radeon Boost | Off | Reduce resolución en movement |
| Radeon Chill | Off | FPS cap variable |
| Texture Filtering Quality | Performance | — |
| Surface Format Optimization | On | — |
| Tessellation Mode | Override → 8x | — |
| Morphological AA | Off | — |
| Anisotropic Filtering | Off u 8x override | — |
| RIS | 50-70% | Ayuda con texturas bajas |
| FSR / FSR 3 | **N/A — Apex no soporta** | Ni DLSS |

---

## 8. EAC (Easy Anti-Cheat) — Compatibilidad

**Arquitectura:** Apex usa **EAC via EOS** (Epic Online Services) — `EasyAntiCheat_EOS.sys` kernel driver.

**Requisitos:**

| Requisito | Apex | vs Valorant | vs BF6 |
|-----------|------|-------------|--------|
| Secure Boot obligatorio | **NO** | Sí (Vanguard) | Sí |
| TPM 2.0 obligatorio | NO | Sí | Sí |
| VBS habilitado | NO | Recomendado | Sí |
| HVCI (Memory Integrity) | NO | Sí | Sí |
| Kernel driver | Sí (EAC) | Vanguard (más estricto) | Sí |

**Apex es mucho más permisivo** que Valorant/BF6/FACEIT.

**Tweaks verificados OK:**

| Tweak | EAC status |
|-------|-----------|
| VBS disable | OK (Apex no lo requiere, ganancia FPS minor) |
| HVCI disable | OK (+5% CPU) |
| Game Mode Win11 | OK |
| HAGS | OK (habilitar para Reflex óptimo) |
| MSI Mode Util GPU | OK |
| Timer resolution (ISLC 0.5ms) | OK (bajo en Win11 24H2+ que fuerza per-app) |
| ISLC cleaner | OK |
| Process Lasso + ProBalance | OK |
| Bitsum BHP power plan | OK |
| RTSS (RivaTuner) | **RIESGO BAJO** — Season 21 cambió algo. No ban-worthy, pero frame limiter puede fallar |
| Afterburner OSD | OK (versiones recientes; viejas pre-2023 causaban crashes EAC) |
| Defender exclusions | OK |
| NVIDIA Profile Inspector | OK |
| CRU custom res | OK |
| Razer Synapse / Logitech GHub macros **movimiento** | **RIESGO ALTO** — bans por macros movimiento |
| Autoexec bind multi-comando | **Zona gris** — reportes aislados 2023-2025 |
| AHK scripts detectables | **BAN** |
| Cheat Engine / DLL inject | BAN instant |

**False-positives conocidos:**
- HWID changes (cambio CPU/mobo) → ban — reportes EA Forums 2025-2026
- AMD Anti-Lag+ (2023 ban wave)
- Overlays viejos (Discord pre-2023, OBS Browser source)
- MSI Afterburner + Rivatuner combo con ciertos AV
- Apex Config agresivo valores fuera de rango

---

## 9. Network

**Server tickrate:** **20Hz** (Respawn defiende — upgrade a 60Hz triplicaría bandwidth sin resolver hit reg).

**Client send:** 58Hz → server; receive 31Hz avg inestable.

**Latency promedio:**
- Damage delay: ~94.2ms
- Gunfire delay: ~165.2ms
- Movement: ~136ms

**Datacenter selection:**
- Acceso: Menu principal esquina superior izq → Data Center → lista con ping + packet loss
- Abril 2025: Respawn migró a **AWS**. Pings subieron post-migración, ajuste ongoing

**Mejores servers por región:**
- LATAM: São Paulo, Virginia
- NA East: Virginia, Ohio
- NA West: California, Oregon
- EU: Frankfurt, Amsterdam, London
- APAC: Tokyo, Singapore, Sydney

**Chile:** Virginia (VA) ping 140-180ms típico desde Santiago; São Paulo 40-80ms pero peak time MM lento.

**ExitLag:** oficialmente **safe** con EAC según ExitLag (network layer, no inyecta). Solo ExitLag — combinado con VPNs o cheats puede flagear.

**VPN:** No prohibido explícito. Conflictos IP VPN/real pueden romper EAC (split-tunneling fix). "Cheaper regional pricing" = bannable.

---

## 10. Process Priority + Affinity

**r5apex.exe:**
- Objetivo: **High priority**
- **Task Manager manual NO funciona** (EAC revierte)
- Método registry (persistent):
  ```
  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\r5apex.exe\PerfOptions
  CpuPriorityClass = 3 (DWORD) → 3=High, 4=RealTime(NO USAR)
  ```
- **Realtime:** NO usar. Bloquea drivers, crashes, no ayuda
- Alternativa: Process Lasso rule persistente

**EasyAntiCheat.exe / EasyAntiCheat_EOS.exe:** **no modificar priority**. EAC depende timing específico.

**Process Lasso ProBalance:**
- Default: baja priority de offenders no-Apex a Below Normal
- Config Apex-específica:
  - `r5apex.exe` → CPU Affinity: P-cores only (12 Gen+ Intel: cores 0-15 excluye E-cores)
  - Priority Class: High
  - I/O Priority: High
  - Exclude from ProBalance
- Bitsum BHP como alternativa

**Intel 12/13/14 Gen:**
- Bug conocido: Apex a veces ejecuta en E-cores → FPS tanking. Fix: affinity P-cores via Lasso o Intel APO
- Scheduled task que bindea `r5apex.exe` a cores 0,2,4,6,8,10,12,14 (P-cores)

**AMD Zen 4/5 X3D:** bindear al CCD con V-Cache (7800X3D single CCD automático; 7950X3D/9950X3D dual CCD requiere manual).

---

## 11. Input

**Mouse:**
- Polling rate **1000Hz** consenso pro (89/89)
- 2000Hz experimentos — mayoría vuelve a 1000Hz
- **4000Hz / 8000Hz causan frame drops confirmados**: engine se satura, CPU spike, freeze al flick. EA bug report oficial. Fix: **cap 1000Hz**
- Windows pointer **6/11** (default)
- Windows "Enhance pointer precision" **OFF**
- In-game `m_acceleration 0`, `m_rawinput 1`, `m_filter 0`

**eDPI pro average:** 800-1200
- 400 DPI: 14%
- 800 DPI: **63%**
- 1600 DPI: 16%
- Sens in-game: 1.0-2.0

**Scroll wheel jump (`mwheeldown`/`mwheelup` → `+jump`):**
- **Estándar pro**, no-baneable
- Tap strafe uses: `bind "mwheeldown" "+jump;+moveleft;+forward"` — **controversial**. Respawn ha hablado contra scripts movimiento pero no baneado autoexec básico. Binds multi-comando = zona gris

**Keyboard:**
- Hall effect (Wooting, Razer HE) = legal
- SOCD cleaner / null bind **banned ALGS 2024+**. Fuera tournaments funcional pero riesgo reputación

---

## 12. Audio

**Source modificado Apex:** audio 3D limitado. Pasos dependen de `miles_channels` + HRTF Windows.

**Windows Sonic vs Dolby Atmos:**
- **Windows Sonic for Headphones**: más "hard" y directo, mejor separación pasos. **Recomendado 2026**
- **Dolby Atmos** ($15): más "cinematic", menos ventaja Apex
- **DTS Headphone:X**: alt popular
- **Stereo puro**: algunos pros

**Focused Audio Mix (Season 27+):**
- Nov 2025: reduce local weapon volume, **elimina squadmate footsteps**, aumenta enemy footsteps
- Recomendado competitivo: **Focused Mix ON**
- "Performance Mode" audio = -12ms latencia (Season 26+)

**Settings críticos:**
- Music Volume: **0%** (no negociable)
- Lobby Music: 0
- Dialogue: 50-70%
- SFX: 100%
- Master: 50-70%
- Speaker Config Windows: **Stereo Headphones**

**Footstep autoexec hacks:**
- `cl_footstep_event_max_dist 4000` (default 2500)
- `snd_setmixer GlobalFootsteps vol 1.2`
- `snd_setmixer PlayerFootsteps vol 0.1`
- `miles_occlusion 0` — sonido atraviesa walls (overpowered, zona gris)

---

## 13. System-level Windows — Tabla EAC

| Tweak | FPS Gain | EAC | Rec Apex |
|-------|----------|-----|----------|
| VBS disable | 2-8% | OK | SÍ |
| HVCI disable | 3-5% | OK | SÍ |
| Game Mode Win11 ON | Minor | OK | SÍ |
| HAGS ON | Req Reflex | OK | SÍ |
| MSI Mode GPU | 1-3% | OK | SÍ |
| Timer Resolution 0.5ms | Frame pacing | OK | SÍ (Win10) / menos relevante Win11 24H2 |
| ISLC | 1% lows | OK | SÍ (8-16GB RAM) |
| Process Lasso | Variable | OK | SÍ |
| Bitsum BHP | 1-3% | OK | SÍ |
| RTSS | Input lag↓ | **Riesgo BAJO** (S21+ quirks) | Opcional |
| Afterburner OSD | Debug | OK (versión 2024+) | SÍ |
| Defender exclude Apex folder | Loading↓ | OK | SÍ |
| BIOS: XMP/EXPO RAM | Big gain | OK | SÍ |
| BIOS: CPU undervolt | Thermals | OK | SÍ |
| Focus Assist / Notifications OFF | — | OK | SÍ |
| Fullscreen Optimizations OFF per-exe | Disputed | OK | Testing — cambió Win11 24H2 |
| Run as Admin | Memory leak fix | OK | SÍ (fix memleak conocido) |

Apex **mucho menos restrictivo** que Valorant/BF6. Casi todos tweaks Windows en verde EAC.

---

## 14. Pro Configs Verificados 2026

| Pro | Team | Mouse | DPI | Sens | Res | FOV | Hz |
|-----|------|-------|-----|------|-----|-----|-----|
| **ImperialHal** | Falcons | FinalMouse Starlight Pro | 800 | 1.1 | **1440x1080 (4:3)** | 110 | 480Hz OLED |
| **HAL** (stream) | Content | Razer DeathAdder V3 Pro | 800 | 2.0 | 1920x1080 | 104 | 1000Hz |
| **Verhulst** | 100T | — | — | Controller | 1920x1080 | 110 | 240Hz |
| **Genburten** | 100T | Razer Viper Ultimate | 800 | 1.5 | 1920x1080 | 110 | 1000Hz |
| **Zer0** | Team Liquid | Finalmouse Ultralight X | 1600 | 0.6 | 1440x1080 | 110 | 1000Hz |
| **Faide** | Content | Logitech G Pro X Superlight 2 | 1700 | 0.8 | **1728x1080 (16:10)** | 110 | 240Hz |
| **Mande** | Content | Logitech GPX Superlight 2 | 800 | 1.5 | 1920x1080 | 110 | 1000Hz |
| **rpr** | Content | Lamzu Atlantis Mini | 800 | 1.2 | 1920x1080 | 110 | 1000Hz |
| **Reps** | FA | Logitech G305 | 800 | 1.1 | 1920x1080 | 110 | — |
| **Hakis** | Alliance | Razer Viper V4 Pro | 1600 | 0.8 | **1680x1050 (16:10)** | 104 | — |

**Patrones:**
1. 800 DPI = default (63%)
2. eDPI pro range: 600-1600 (vs CS2 800-1200, Apex más bajo por FOV amplio)
3. 110 FOV mayoritario
4. Stretched significativo entre M&K (Hal 4:3, Faide/Hakis 16:10)
5. Controller pros (Verhulst, Genburten) usan 16:9 nativo siempre
6. Video settings: 99% todo low/disabled + model detail variable (Faide High)
7. Reflex Enabled+Boost: 100% NVIDIA pros

**Insights específicos:**
- **ImperialHal**: tweet famoso recomendando editar `setting.csm_enabled 1 → 0` manual en videoconfig.txt
- **Faide**: único pro top-tier con Model Detail HIGH (character outline clarity)
- **Genburten**: 16:9 + controller linear curve
- **Zer0**: 1600 DPI + 0.6 sens (high DPI outlier), custom stretched 1440x1080

---

## 15. Troubleshooting

| Problema | Causa | Fix |
|----------|-------|-----|
| **FPS drops random partida larga** | Memory leak Source engine | Run as Admin + ISLC + reinicio cada 2-3 partidas (conocido, **sin fix oficial** Respawn) |
| **Stutter drop ship** | Asset streaming sobrecarga | `cl_forcepreload 1` autoexec, SSD NVMe, RAM dual channel |
| **Micro-stutter al flick** | Mouse polling 4000/8000Hz | **1000Hz** |
| **Audio lag/desync** (S21-27) | Engine audio bugs | Bind `"o" "miles_reboot; miles_stop_all"`, reinicio cliente |
| **Mic not working** | Windows exclusive mode | Disable exclusive mode Recording Properties |
| **EAC launcher fails** | EAC_EOS.sys kernel loop | Repair via `EasyAntiCheat\EasyAntiCheat_EOS_Setup.exe /repair`, Secure Boot toggle |
| **Low FPS con GPU potente** | CPU-bound (Apex single-threaded heavy) | Upgrade CPU 3D V-Cache; disable efficiency cores affinity |
| **DX12 crash** | Launch options viejos (`-eac_launcher_settings`) | Limpiar launch options, verify files, update NVIDIA 555.89+ |
| **HWID ban falso** | EAC flagged por cambio hardware | Appeal EA Support — reportes 1 año para unban |
| **G-Sync no funciona/tearing** | In-game Adaptive Sync = Fast Sync (incompatible G-Sync) | Adaptive Sync **Disabled**, G-Sync NVCP, cap 3 FPS debajo refresh, V-Sync ON NVCP |
| **Shader compile stutter** | Primera vez cache vacío | Shader Cache Size **Unlimited** NVCP, dejar cargar 1 partida |

---

## 16. Legend-Specific FPS

No hay "per-legend graphics profile". Todos mismos shaders/meshes.

**Legends con FPS impact:**

| Legend | Ability | FPS hit | Mitigation |
|--------|---------|---------|-----------|
| **Horizon** | Black Hole ult | 10-20% drop | `cl_phys_props_enable 0` autoexec |
| **Valkyrie** | Ult jetpack launch | Drop puntual transición | — |
| **Maggie** | Wrecking Ball rodando | Particles overhead | `cl_particle_fallback` agresivo |
| **Fuse** | Motherlode ult fire ring | Particles/volumetric | `volumetric_lighting 0`, `particle_cpu_level 0` |
| **Seer** | Tactical heart beat + ult field | CPU raycast | — |
| **Catalyst** | Ferrofluid walls | Material shader | `mat_picmip` bajar |
| **Revenant** | Shadow Form | Post-processing | — |
| **Watson/Wattson** | Ult ice + fences | Overlapping volumetric | — |

**General:** `mat_queue_mode 2` + `r_dynamic 0` + particle fallbacks cubren 90% de hits. No hay tweaks legend-específicos oficiales.

---

## 17. Notas finales app

1. **EA App bloquea launch options con `+`**: detectar launcher y avisar: Steam = full support, EA App = shortcut workaround o Steam version
2. **`+fps_max_unfocused` NO es launch option real** — eso se hace vía NVCP "Background Application Max Frame Rate"
3. **`-cpuCount` / `-exthreads` NO son flags Source/Apex** — son Dunia/Ubisoft. Borrar del UI
4. **`-eac_launcher_settings`, `-dxlevel`, `-nomansky`** deprecated/inútiles 2026 — marcar legacy
5. **AMD Anti-Lag+ NUCLEAR** — never enable. Solo Anti-Lag clásico
6. **Superglide/tap-strafe binds**: zona gris — warning modal
7. **Mouse polling slider**: hard-cap 1000Hz; 2000Hz experimental; 4000/8000Hz = block con warning
8. **NVIDIA Reflex**: default ON + Boost NVIDIA users; AMD no tiene equivalente Apex (Anti-Lag 2 no soportado)
9. **videoconfig.txt read-only toggle**: crítico UX
10. **Process Lasso P-core affinity** para 12 Gen+ Intel es el tweak menos conocido pero mayor impacto 1% lows

---

## Fuentes clave

- [Apex Pro Settings 89 pros - ProSettings](https://prosettings.net/guides/apex-legends-options/)
- [ImperialHal/Faide/Verhulst - ProSettings](https://prosettings.net/players/)
- [Best Apex Launch Commands - ProSettings](https://prosettings.net/blog/best-apex-legends-launch-commands/)
- [noxtgm/apex-legends-autoexec GitHub](https://github.com/noxtgm/apex-legends-autoexec)
- [V3nilla/Apex-Legends-Config-And-Tweaks GitHub](https://github.com/V3nilla/Apex-Legends-Config-And-Tweaks)
- [kretz1xD/Apex-Legends-Tweaks](https://github.com/kretz1xD/Apex-Legends-Tweaks/blob/xD/videoconfig.txt)
- [Apex Movement Wiki - Launch Commands](https://apexmovement.tech/wiki/articles/Setup%20&%20Hardware%3ELaunch%20commands)
- [Apex FPS increase - EsportsTales](https://www.esportstales.com/apex-legends/how-to-increase-fps-videoconfig-settings-launch-options)
- [CPU Bottleneck Apex 2026](https://evezone.evetech.co.za/performance-pulse/apex-legends-cpu-bound-optimization-guide-2026/)
- [Mouse Polling Overclock Apex](https://evezone.evetech.co.za/performance-pulse/apex-legends-mouse-polling-rate-overclock/)
- [Best Settings Competitive 2026](https://gamersdignity.com/pc-gaming/best-apex-legends-settings-competitive-2026/)
- [Servers and Netcode Deep Dive - EA](https://www.ea.com/games/apex-legends/apex-legends/news/servers-netcode-developer-deep-dive)
- [EA Forums - EAC HWID False Positive](https://forums.ea.com/discussions/apex-legends-technical-issues-en/1-year-wrongful-apex-ban---eac-hwid-false-positive-ea-id-huaiwumu/13074891)
- [EA Forums - 8000Hz Mouse Stutter](https://forums.ea.com/discussions/apex-legends-bug-reports-archive-en/the-8000-hz-mouse-polling-rate-causes-massive-nano-stutter-during-mouse-moves/5357767)
- [EA Forums - NVIDIA Profile Inspector allowed](https://forums.ea.com/discussions/apex-legends-general-discussion-en/re-nvidia-profile-inspector-allowed-or-not/5206020)
- [Blur Busters - Currently best Apex settings](https://forums.blurbusters.com/viewtopic.php?t=11942)
- [Fix Launch Arguments EA Desktop - TroubleChute](https://hub.tcno.co/software/ea-desktop/fix-arguments/)
- [Apex AWS Migration](https://alegends.gg/apex-legends-officially-moves-to-amazon-servers/)
- [ExitLag Apex Ban Risk](https://www.exitlag.com/blog/can-i-get-banned-for-using-exitlag/)
- [AMD Anti-Lag+ Apex Ban Wave - VideoCardz](https://videocardz.com/newz/amd-anti-lag-issues-extend-beyond-cs2-reportedly-affecting-multiplayer-games-such-as-call-of-duty-and-apex-legends)
- [DX11 Support Dropped](https://forums.ea.com/discussions/apex-legends-general-discussion-en/heads-up-dx11-support-will-be-dropped-soon/11928628)
- [ImperialHal csm_enabled tweet](https://x.com/ImperialHal/status/1353829237208326144)
- [Apex Stretched Resolution - Dot Esports](https://dotesports.com/apex-legends/news/respawn-removes-restriction-on-resolution-for-apex-legends-tournaments)
- [Season 27 Audio Focused Mix](https://www.ea.com/en/games/apex-legends/apex-legends/news/showdown-audio-update)
- [Apex Memory Leak Discussion](https://forums.ea.com/discussions/apex-legends-technical-issues-en/memory-leak/12084752)
