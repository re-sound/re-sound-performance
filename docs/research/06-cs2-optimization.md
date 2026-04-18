# CS2 Optimization — Tab "Counter-Strike 2" para app Windows 11

**Fecha:** 2026-04
**Fuentes cruzadas:** ProSettings.net (883 pros tracked), HLTV, Dexerto, BLAST.tv, Team Liquid, Blur Busters, FACEIT oficial, Refrag, Thour benchmarks.

## 0. Nota crítica inicial (UX de la app)

**Realidad 2025-2026:** Source 2 optimiza muy bien por defecto. La mayoría de las "launch options" legendarias de CS:GO **ya no hacen nada** en CS2. Un empleado de Valve dijo públicamente: *"Best launch options are no launch options"*.

La app debe marcar claramente **REAL / PLACEBO / ROTO** cada tweak. Más impacto tiene bajar Global Shadow Quality de Medium a Low que toda la sopa de flags `-xxx` junta.

---

## 1. Launch Options

| Flag | Función | Estado CS2 | Pro rec. | Consenso |
|---|---|---|---|---|
| `-novid` | Skip intro Valve | **REAL** (ahorra 3-5s) | s1mple SÍ | ALTO |
| `-console` | Abre consola al inicio | **REAL** | s1mple SÍ | ALTO |
| `-high` | Prioridad High proceso | **REAL pero controversial** | NO mayoría | MEDIO — Windows reajusta prioridad al ganar foco |
| `-threads N` | Fija hilos CPU | **ROTO / CONTRAPRODUCENTE** | NO | ALTO — Source 2 auto-gestiona; forzarlo puede **bajar FPS** |
| `-tickrate 128` | Tick privados | **REAL SOLO en servidores privados** | Algunos | MEDIO — CS2 oficial usa **subtick 64** |
| `-refresh N` / `-freq N` | Forzar Hz | **REAL condicional** | s1mple `-freq 360` | MEDIO |
| `-fullscreen` | Forzar fullscreen exclusive | **REAL** | SÍ mayoría | ALTO |
| `-windowed -noborder` | Borderless | **REAL** pero agrega latencia vs exclusive | NO competitivo | ALTO |
| `-nojoy` | Disable joystick | **PLACEBO / ya default** | algunos | BAJO |
| `-noaafonts` | Reduce uso GPU en fuentes | **PLACEBO CS2** | NO | ALTO |
| `-nosync` | Desactivar vsync | **PLACEBO** — usar V-Sync OFF in-game | NO | ALTO |
| `-allow_third_party_software` | Permite hooks (ReShade/OBS/NVIDIA Filter) | **REAL** pero **baja Trust Factor** | ZywOo SÍ (streaming) | ALTO — **solo si necesitas** |
| `+fps_max 0` | Uncap FPS | **REAL** | s1mple `+fps_max 999`, donk 600 | ALTO |
| `+exec autoexec` | Ejecuta autoexec.cfg | **REAL necesario** | SÍ | ALTO |
| `-d3d11` | Forzar DX11 | **REAL** (default anyway) | — | BAJO |
| `-vulkan` | Forzar Vulkan | **REAL condicional** — mejor AMD + CPU-bottleneck; peor NVIDIA high-end (Thour bench: DX11 265 FPS vs Vulkan 216) | NO pros top | MEDIO — **TESTAR** |
| `-language english` | Fuerza inglés | **REAL** — fix textos rotos | Recomendado | ALTO |
| `-autoconfig` | Reset settings al abrir | **REAL destructivo** | NO | ALTO |
| `-noreflex` | Desactiva NVIDIA Reflex | **REAL y controversial** | Meta 2025-2026 en high-FPS | MEDIO |

**Recomendado app (seguro):**
```
-novid -console -language english +fps_max 0 +exec autoexec
```

**Recomendado power users:**
```
-novid -console -high -language english +fps_max 0 +exec autoexec
```

**NUNCA auto-agregar**: `-threads`, `-high` (sin permiso), `-allow_third_party_software` (Trust), `-autoconfig`.

---

## 2. autoexec.cfg Modelo (Copy-Paste Ready)

**Ubicación:**
- Principal: `C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg\autoexec.cfg`
- Usuario: `C:\Program Files (x86)\Steam\userdata\[SteamID]\730\local\cfg\autoexec.cfg`
- Carpeta sigue llamándose `csgo` (legacy) — **confundir al user lo rompe**.

```cfg
// =======================================================
// autoexec.cfg - CS2 Optimized Pro Template 2026
// =======================================================

// --- NETWORK (subtick 64 oficial) ---
rate "786432"                              // Máx en CS2
cl_cmdrate "128"                           // Deprecated pero no rompe
cl_updaterate "128"                        // Deprecated pero no rompe
cl_interp "0"                              // Auto-ajusta al mínimo seguro
cl_interp_ratio "1"                        // 1 = conexiones estables, 2 = ping alto
cl_net_buffer_ticks "0"                    // 0 = mínima latencia (default 2); 1 compromise

// --- SUBTICK / LATENCY ---
cl_predict "1"
cl_predictweapons "1"
engine_low_latency_sleep_after_client_tick "true"  // Frametimes más planos

// --- MATCHMAKING ---
mm_dedicated_search_maxping "60"
cl_mm_session_search_qos_timeout "10"

// --- FPS & VIDEO ---
fps_max "0"
fps_max_ui "240"
mat_queue_mode "-1"
r_dynamic "0"
r_drawparticles "1"                        // 0 remueve humo pero ILEGAL competitivo

// --- MOUSE ---
m_rawinput "1"
m_customaccel "0"
m_mouseaccel1 "0"
m_mouseaccel2 "0"
zoom_sensitivity_ratio_mouse "1.0"
sensitivity "2.0"                          // USER CONFIGURA

// --- CROSSHAIR (pro codes via apply_crosshair_code) ---
// apply_crosshair_code "CSGO-E8xcE-27Lmw-2ipNt-3HZvp-pevvE"  // s1mple
// apply_crosshair_code "CSGO-33tjH-QWX3U-KcSnD-wu3wu-i2pxA"  // ZywOo
// apply_crosshair_code "CSGO-pDjKq-MWKVt-6JBOs-uw8ic-45qmJ"  // donk
// apply_crosshair_code "CSGO-eJRUn-yYTxp-bsSyA-RFOMh-DzurD"  // m0NESY
// apply_crosshair_code "CSGO-cqOyh-CSfhe-mnOnK-csY5s-s44RG"  // Twistzz
cl_crosshair_sniper_width "1"

// --- VIEWMODEL (pro template) ---
viewmodel_fov "68"
viewmodel_offset_x "2.5"
viewmodel_offset_y "0"
viewmodel_offset_z "-1.5"
viewmodel_presetpos "0"
cl_bob_lower_amt "5"
cl_bobamt_lat "0.1"
cl_bobamt_vert "0.1"
cl_bobcycle "0.98"

// --- HUD ---
hud_scaling "0.9"
cl_hud_color "2"
cl_hud_background_alpha "0.5"
cl_hud_radar_scale "1.0"
cl_showloadout "1"
cl_autohelp "0"
gameinstructor_enable "0"
hud_showtargetid "1"

// --- RADAR (90% HLTV Top 20) ---
cl_radar_rotate "1"
cl_radar_always_centered "1"               // 90% pros = 1
cl_radar_scale "0.4"
cl_radar_icon_scale_min "0.8"
cl_radar_square_with_scoreboard "1"
alias "+bigmap" "cl_radar_scale 0.7"
alias "-bigmap" "cl_radar_scale 0.4"
bind "n" "+bigmap"

// --- AUDIO ---
volume "0.35"
snd_mixahead "0.025"                       // Default 0.05; pro 0.025
snd_headphone_pan_exponent "1.2"
snd_front_headphone_position "45.0"
snd_rear_headphone_position "135.0"
snd_headphone_pan_radial_weight "1.0"
dsp_enhance_stereo "0"
snd_deathcamera_volume "0"
snd_mapobjective_volume "0"
snd_roundstart_volume "0"
snd_tensecondwarning_volume "0.08"
snd_mvp_volume "0"
snd_menumusic_volume "0"
snd_musicvolume_multiplier_inoverlay "0"
voice_scale "0.6"
voice_loopback "0"
voice_enable "1"
voice_modenable "1"

// --- COMPETITIVE HELPERS ---
cl_sanitize_player_names "0"
cl_showfps "1"
cl_show_clan_in_death_notice "1"

// --- BUY BINDS (numpad) ---
bind "kp_5" "buy ak47; buy m4a1;"
bind "kp_6" "buy awp"
bind "kp_4" "buy ssg08"
bind "kp_1" "buy vesthelm; buy vest"
bind "kp_2" "buy defuser"
bind "kp_3" "buy flashbang; buy flashbang; buy hegrenade; buy smokegrenade; buy molotov; buy incgrenade"
bind "kp_7" "buy deagle"
bind "kp_8" "buy tec9; buy fiveseven; buy cz75a"
bind "kp_9" "buy p250"
bind "kp_0" "buy mp9; buy mac10"

// --- GRENADE BINDS ---
bind "z" "slot7"
bind "x" "slot8"
bind "c" "slot6"
bind "v" "slot10"
bind "b" "slot9"

// --- UTILITY ---
bind "F1" "autobuy"
bind "F2" "rebuy"
bind "F3" "toggleconsole"
bind "F5" "jpeg"
bind "t" "+lookatweapon"
bind "mouse3" "player_ping"
bind "mouse4" "+voicerecord"

// --- MISC QoL ---
cl_join_advertise "2"
developer "0"
con_enable "1"
r_show_build_info "false"
cl_allow_animated_avatars "0"

// --- SAVE ---
host_writeconfig
echo "[autoexec] Config loaded successfully"
```

**CRÍTICO:** `cl_allow_multi_input_binds 0` fue añadido en CS2 — jumpthrow bind CS:GO-style **no funciona**. Hacer timing manual.

---

## 3. In-Game Video Settings (Pro Consensus 883 pros)

| Setting | Pro Consensus | Alt | Razón | Impacto FPS |
|---|---|---|---|---|
| **Resolution** | 1280x960 (53%) | 1024x768 (15%) / 1920x1080 (13%) | Modelos más anchos, +FPS | +30-50% low-end |
| **Aspect Ratio** | 4:3 Stretched (~60%) | 16:9 | Hitboxes percibidos mayores | — |
| **Scaling Mode** | Stretched (mayoría) o Black Bars (Twistzz, sh1ro) | — | — | — |
| **Display Mode** | **Fullscreen Exclusive** | — | Menos latencia | ALTO |
| **Brightness** | 93-110% | — | Ver sombras | — |
| **V-Sync** | **Disabled** | — | Latencia | ALTO |
| **NVIDIA Reflex** | **Enabled + Boost** / `-noreflex` (meta high-FPS) | — | -15-30ms GPU-bound; -noreflex +3-10% FPS CPU-bound | VARIA |
| **Max FPS** | 0 o 400-999 | 600 (donk) | — | — |
| **Max FPS Menu** | 240 | — | — | — |
| **MSAA** | **8x (s1mple, m0NESY, donk)** / 4x (ZywOo) / 2x | CMAA2 low-end | — | -10-15% con 8x |
| **Global Shadow Quality** | **High** (pros visibility) | — | **No "Low"** — sombras enemigos son info | MEDIO |
| **Dynamic Shadows** | All | — | Ver enemigos por sombras | — |
| **Model/Texture Detail** | **Low** | Medium | Modelos claros, menos clutter | ALTO |
| **Texture Filtering** | Bilinear (s1mple) / Trilinear (donk) / Anisotropic 4x (ZywOo) | 8x | — | BAJO |
| **Shader Detail** | Low | — | — | MEDIO |
| **Particle Detail** | Low | — | Menos ruido smokes/mollies | MEDIO |
| **Ambient Occlusion** | **Disabled** | — | Claridad > realismo | MEDIO |
| **HDR** | **Quality** (cambió vs CS:GO) o Performance | Off GPU vieja | CS2 fuerza HDR pipeline | — |
| **FidelityFX Super Resolution** | **Disabled** o **CAS (sharpen-only)** | FSR Performance si <60FPS | — | +/- |
| **Boost Player Contrast** | **Enabled** (NAF, mayoría) / **Disabled** (s1mple, m0NESY) | — | Halo sutil. 3% FPS | BAJO |

**Nota HDR**: a diferencia de CS:GO donde era "off", CS2 tiene pipeline HDR integrada; Quality es preset competitivo.

---

## 4. NVIDIA Per-Game Profile ("Counter-Strike 2")

| Setting | Valor |
|---|---|
| Power management mode | **Prefer maximum performance** |
| Low Latency Mode | **Off** si Reflex in-game / **Ultra** si `-noreflex` |
| Threaded Optimization | **Auto** (no forzar On — puede hurtar frame pacing high-end; Off a veces mejor) |
| Texture Filtering — Quality | **High Performance** |
| Texture Filtering — Anisotropic sample opt | On |
| Texture Filtering — Trilinear opt | On |
| Texture Filtering — Negative LOD bias | Allow |
| Preferred Refresh Rate | **Highest available** |
| Vertical sync | **Off** |
| Maximum pre-rendered frames | **1** |
| Shader Cache Size | **Unlimited** |
| Monitor Technology | G-SYNC Compatible (si tienes) |
| Antialiasing — FXAA | Off |
| Antialiasing — Mode | Application-controlled |
| MFAA | Off (bugs en CS2) |
| Triple buffering | Off |
| Image Sharpening | Off (CS2 tiene CAS in-game) |
| Power State Transitions | Minimum |

**Driver 2026:** community recomienda **572.42+** (o 537.58 como fallback estable si nuevo driver rompe CS2).

---

## 5. AMD Adrenalin Per-Game Profile CS2

| Setting | Valor |
|---|---|
| **Radeon Anti-Lag 2** | **Enabled in-game** (CS2 integra SDK oficialmente) |
| **HYPR-RX** | **Off** (mete AFMF = input lag) |
| **AMD Fluid Motion Frames** | **Off** |
| **Radeon Chill** | **Off** |
| **Radeon Boost** | **Off** (baja resolución en movement = enemigos borrosos) |
| **Radeon Super Resolution (RSR)** | **Off** |
| **Radeon Image Sharpening** | **Off** (CS2 CAS in-game hace el trabajo) |
| **Enhanced Sync** | **Off** |
| **Wait for Vertical Refresh** | **Always Off** |
| **Anti-Aliasing** | **Use Application Settings** |
| **Morphological AA** | Disabled |
| **Anisotropic Filtering** | Application Controlled |
| **Texture Filtering Quality** | **Performance** |
| **Surface Format Optimization** | Disabled (conflictos Source 2) o Enabled según source — **TESTAR** |
| **Tessellation Mode** | Override — 8x o 16x |
| **Frame Rate Target Control** | Off |
| **Graphics Profile** | **eSports** |

**Mantenimiento:** resetear shader cache tras cada driver update.

---

## 6. Anti-cheat Compatibility Matrix

### 6.1 VAC + Trust Factor

| Acción | VAC | Trust Factor |
|---|---|---|
| Launch options estándar | **No ban** | Neutral |
| `-allow_third_party_software` | **No** | **Baja Trust Factor** |
| autoexec commands legales | No | Neutral |
| NVIDIA/AMD CP tweaks | No | Neutral |
| RTSS frame limiter | No | Requiere `-allow_third_party_software` |
| ReShade | No ban pero **CS2 bloquea inyección** | N/A |
| Editar archivos engine/DLL | **VAC ban** | N/A |
| Skin changers | **VAC ban instantáneo** | N/A |
| Macros multi-input jumpthrow | **No ban** pero `cl_allow_multi_input_binds 0` los **desactiva** | |
| NVIDIA Freestyle Filter | Requiere `-allow_third_party_software` | Baja Trust |

### 6.2 FACEIT AC (timeline abril 2026)

| Requisito | Desde |
|---|---|
| TPM 2.0 | 25 nov 2025 mandatorio |
| Secure Boot | 25 nov 2025 mandatorio |
| UEFI mode | Abril 2025 |
| IOMMU (VT-d/AMD-Vi) | Abril 2025 gradual; 3K+ Elo desde ago 2025; expansión nov 2025 |
| VBS | Junto con IOMMU |
| HVCI / Memory Integrity | Rollout fases |
| **Windows 11** | **14 octubre 2026 mandatorio** |

**Impacto perf:** TPM/Secure Boot = 0%. VBS/HVCI = 2-7% (más CPUs older, menos Zen 4/5/Intel 13/14 con firmware maduro). IOMMU = 0-2%.

**Checker:** `msinfo32` + `systeminfo` → "Virtualization-based security Status = Running".

### 6.3 Whitelist app (tweaks NO flaggean)

- autoexec.cfg (commands legales no modificados)
- Launch options salvo `-allow_third_party_software`
- NVIDIA/AMD Control Panel + Profile Inspector
- Windows registry tweaks (timer, MSI mode)
- Process Lasso (priority/affinity)
- RTSS solo con `-allow_third_party_software` (Trust baja)
- Disable Xbox Game Bar, HAGS, Game Mode

---

## 7. Network / Netcode (subtick 64)

| Comando | Valor pro | Efecto CS2 | Consenso |
|---|---|---|---|
| `rate` | **786432** (o 1000000 estable) | Máximo oficial | ALTO |
| `cl_interp` | 0 | Auto-mínimo | ALTO |
| `cl_interp_ratio` | 1 (<30ms) / 2 (>50ms) | Deprecated pero influye UI buffering | MEDIO |
| `cl_updaterate` | 128 | Deprecated, hardcoded 64 | BAJO |
| `cl_cmdrate` | 128 | Deprecated, hardcoded 64 | BAJO |
| `cl_net_buffer_ticks` | **0** / 1 compromise / 2 default | **SÍ impacta** 2025+ | ALTO |
| `mm_dedicated_search_maxping` | 50-75 (60 sweet spot) | Límite ping MM | ALTO |
| `cl_lagcompensation` | 1 | Default | ALTO |
| `engine_low_latency_sleep_after_client_tick` | true | Frametimes más planos | ALTO — confirmado Valve dev |

**DNS:**
- **Cloudflare 1.1.1.1** — fastest (pero **desactivar Cloudflare WARP** — rompe matchmaking CS2)
- **Quad9 9.9.9.9** — alternative threat blocking
- **Google 8.8.8.8** — fallback

**Conexión:** Ethernet > Wi-Fi. 5GHz > 2.4GHz. Disable Windows QoS si router lo maneja.

---

## 8. Audio Tweaks

CS2 **fuerza HRTF** (Steam Audio integrado, no se puede desactivar — cambio vs CS:GO). **No pelees contra el engine, úsalo.**

| Setting | Valor pro |
|---|---|
| Speaker Config | **Stereo Headphones** obligatorio |
| Windows Sonic | **Off** (conflicto) |
| Dolby Atmos | **Off** (conflicto) |
| DTS Headphone:X | **Off** (conflicto) |
| `snd_mixahead` | **0.025** pro / 0.05 default |
| `snd_headphone_pan_exponent` | **1.2** natural / 2.0 dramático |
| `snd_front_headphone_position` | 45.0 |
| `snd_rear_headphone_position` | 135.0 |
| `dsp_enhance_stereo` | **0** |
| Advanced 3D Audio Processing | **On** |
| `voice_scale` | 0.5-0.7 |
| `voice_loopback` | **0** |
| `volume` | 0.3-0.4 |
| L/R Isolation | 0% |
| Perspective Correction | Enabled |
| EQ Profile | Natural |

---

## 9. Input Tweaks

### Mouse
| Parámetro | Recomendación |
|---|---|
| **Polling Rate** | **1000 Hz** (gold). 4000 Hz solo CPU top + mouse sin bugs (m0NESY, Twistzz). **8000 Hz desaconsejado** |
| **Windows pointer speed** | **6/11** |
| **"Enhance pointer precision"** | **Off** |
| **DPI** | 400 (66% pros) / 800 (28%) / 1600 (5%) |
| **Raw Input** | `m_rawinput 1` obligatorio |
| **eDPI mediana** | **830** (~47cm/360°) |

### Keyboard
- NKRO
- Polling 1000Hz
- Rapid trigger (Wooting, Apex Pro, Huntsman V3): activar
- **Snap Tap (SOCD) prohibido desde 2024 — VAC banneable**. No implementar.

---

## 10. Process Priority + Affinity

### Priority
| Nivel | Recomendación |
|---|---|
| **High** | Pro consenso |
| **Realtime** | **NO** — congela OS, audio bugs, BSOD |
| **Above Normal** | Alternativa si `-high` baja Trust |

Mejor: Process Lasso rule "CPU Priority: High" para `cs2.exe`.

### Affinity

**Intel 12/13/14 Gen:** 
- Valve agregó 2024: **Settings > Game > CPU core usage preference > "Prefer performance cores"**. Usar esto primero.
- Manual: Process Lasso > cs2.exe > CPU Affinity > **solo P-cores**
- 13900K/14900K: cores 0-15 (8 P-cores con HT)
- **YA NO necesario disable E-cores BIOS** — actualizar BIOS arregla voltaje
- Evitar CPU 0 (core del OS)

**AMD X3D (7800X3D, 9800X3D):**
- Pinnear a CCD con V-cache (CCD0 en 7800X3D, ambos fusionados en 9800X3D)
- Process Lasso rule afinity CCD V-cache
- Disable SMT BIOS (opcional, mejora 1% lows)

**Process Lasso rules `cs2.exe`:**
1. Efficiency Mode: OFF
2. CPU Affinity: Only P-cores / V-cache CCD
3. I/O Priority: High
4. Power Plan: Ultimate Performance on game start
5. ProBalance exclude: `cs2.exe`

---

## 11. System-Level CS2-Specific

| Tweak | Comando/Acción | Impacto | Riesgo |
|---|---|---|---|
| **Timer Resolution** | `bcdedit /set useplatformtick yes` + `bcdedit /set disabledynamictick yes` | Timer 0.5ms fijo | Low |
| **MSI Mode GPU** | MSI_util_v3 → GPU MSI, Priority High | Reduce DPC/ISR latency | Low |
| **MSI Mode NIC** | MSI_util_v3 NIC | Mejora networking | Low |
| **MSI Mode Audio** | MSI_util_v3 | Reduce audio jitter | Low |
| **HAGS** | Settings > System > Display > Graphics | **TESTAR**: ON con RTX 30/40+Reflex; OFF algunas AMD | Medium |
| **Game Mode** | Settings > Gaming | **TESTAR** — Valve "Lag Mode" thread | — |
| **Xbox Game Bar** | Settings > Gaming **OFF** | Reduce overhead | Zero |
| **Fullscreen Optimizations** | cs2.exe Properties > Compatibility > Disable | Reduce input lag | Zero |
| **MPO** | Registry `OverlayTestMode = 5` | Reduce stutter multi-monitor | Medium |
| **Core Isolation / Memory Integrity** | OFF (salvo FACEIT) | FACEIT req ON; solo MM: off = 2-7% FPS | — |
| **Power Plan** | `powercfg /duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61` | P-states locked high | Zero |
| **Visual Effects** | sysdm.cpl > Performance > "Adjust for best" | Desactiva animaciones | Zero |
| **RTSS Frame Cap** | Requiere `-allow_third_party_software` → baja Trust. Alt: NVCP "Max Frame Rate" | Trust |
| **Windows Update paused** | Pause 7 días antes de torneo | Evita driver rollback mid-match | Zero |

### Frame cap decision tree
1. Tienes VRR (G-Sync/FreeSync)? → cap = refresh - 3 (240Hz → 237)
2. No VRR? → cap = refresh o uncap
3. Location: **NVCP "Max Frame Rate"** primero, **RTSS** si Trust no importa, in-game `fps_max` último

---

## 12. Pro Configs (verificados abril 2026)

| Pro | Team | Resolución | Sens | eDPI |
|---|---|---|---|---|
| **s1mple** | Navi | 1280x960 4:3 stretch | 3.09 | 1236 |
| **ZywOo** | Vitality | 1280x960 4:3 stretch | 2.00 | 800 |
| **donk** | Spirit | 1280x960 4:3 stretch | 1.25 | 1000 |
| **m0NESY** | Falcons | 1280x960 4:3 stretch | 2.30 | 920 |
| **NiKo** | Falcons | 1024x768 4:3 | 1.00 | 800 |
| **Twistzz** | FaZe | 1280x960 4:3 | 2.30 | 920 |
| **sh1ro** | Spirit | 1152x864 4:3 | 1.04 | 832 |
| **ropz** | Vitality | 1920x1080 16:9 | 1.77 | 708 |
| **dev1ce** | Astralis | — | 2.00 | 800 |
| **EliGE** | Liquid | 1680x1050 16:10 | 0.74 | 1184 |

**Observaciones:**
- **m0NESY**: único top-pro con **polling rate 4000 Hz**
- **donk**: monitor 600Hz (ZOWIE XL2586X+), brightness 109%
- **s1mple**: launch options públicos `-freq 360 -novid -console +fps_max 999`
- **EliGE**: **1680x1050 16:10** — outlier
- **ropz**: **1920x1080 16:9 nativo** — minoría top-tier

---

## 13. Troubleshooting (guided UX)

### "Stutters / microstutters"
1. Delete DirectX Shader Cache
2. Verify game files (Steam)
3. Update audio drivers (causa común 2025)
4. Clean reinstall GPU driver (DDU + NVCleanstall)
5. Disable Xbox Game Bar + HAGS
6. Intel 12-14 Gen: BIOS update + CPU preference = P-cores
7. Fallback: fresh config (delete cs2_user_convars.vcfg)

### "Bajo FPS (drops en smokes/multiplayer)"
1. Resolution → 1280x960 stretched
2. Model/Texture detail → Low
3. MSAA → CMAA2 o 2x
4. Verify Threaded Optimization NVCP
5. Test `-vulkan` (AMD especialmente)
6. Close Chrome/Discord background
7. Disable overlays (Steam, Discord, GeForce, NVIDIA App)
8. Test `-noreflex` si high-FPS sistema

### "Input lag percibido"
1. V-Sync OFF
2. NVIDIA Reflex ON + Boost (o test -noreflex)
3. Fullscreen Exclusive
4. Windows Pointer 6/11
5. Polling 1000 Hz
6. MSI Mode GPU enable
7. Timer resolution 0.5ms
8. Disable HAGS si no RTX 30/40/50
9. FSO off en cs2.exe properties

### "Packet loss / hit-reg inconsistente"
1. rate 786432
2. Ethernet cable
3. `mm_dedicated_search_maxping 60`
4. DNS Cloudflare 1.1.1.1 (disable WARP)
5. Disable Wi-Fi power saving
6. QoS disable si router interfiere

### "Stutter tras update"
→ Delete shader cache (fix 90%)

### "CS2 no arranca / crash launch"
1. Verify files
2. Remove `-threads`
3. Disable `-autoconfig`
4. Delete `cs2_user_convars.vcfg`
5. Run as admin once

---

## 14. Observaciones para diseño de app

1. **No auto-agregar `-threads`, `-high`, `-allow_third_party_software`** sin pedir permiso
2. **Detectar hardware** antes de presets: Intel 12-14 → P-core affinity; AMD X3D → CCD pinning; AMD GPU → Anti-Lag 2; NVIDIA RTX 40+ → Reflex decision
3. **FACEIT AC detection**: si usuario corre FACEIT, **forzar HVCI/VBS/Secure Boot checker** antes de tweaks de perf
4. **Trust Factor warning**: tweaks que requieran `-allow_third_party_software` = aviso explícito
5. **Backup automático** antes de modificar `cs2_video.txt`, `cs2_user_convars.vcfg`, autoexec, registry
6. **Pro configs como import one-click**: crosshair codes + video preset + autoexec template

---

## Fuentes clave

- [ProSettings.net CS2 — 883 pros](https://prosettings.net/guides/cs2-options/)
- [s1mple/ZywOo/donk/m0NESY pro pages](https://prosettings.net/players/)
- [BLAST.tv Pro Configs](https://blast.tv/article/cs2-pro-config-settings)
- [Blur Busters — Lowest input latency for CS2](https://forums.blurbusters.com/viewtopic.php?t=14245)
- [Thour CS2 MSAA Benchmark](https://x.com/ThourCS2/status/1810331191184117976)
- [Thour CS2 DX11 vs Vulkan](https://x.com/ThourCS2/status/1701555779633807389)
- [FACEIT Windows Security Requirements](https://support.faceit.com/hc/en-us/articles/23117181142556-Windows-Security-Requirements-FAQ)
- [FACEIT TPM/Secure Boot/IOMMU/VBS rollout](https://www.faceit.com/en/news/faceit-rollout-of-tpm-secure-boot-iommu-and-vbs)
- [Total CS — Best CS2 Launch Options 2026](https://totalcsgo.com/launch-options)
- [Refrag — CS2 Optimization Guide](https://refrag.gg/blog/the-ultimate-cs2-optimization-guide/)
- [Steam Support — CS2 Trusted Mode](https://help.steampowered.com/en/faqs/view/09A0-4879-4353-EF95)
- [Valve Developer Community — CS2 console commands](https://developer.valvesoftware.com/wiki/List_of_Counter-Strike_2_console_commands_and_variables)
- [NVIDIA Reflex CS2 — On or Boost](https://evezone.evetech.co.za/performance-pulse/nvidia-reflex-cs2-on-or-boost)
- [CS2 released with NVIDIA Reflex](https://www.nvidia.com/en-us/geforce/news/gfecnt/20239/counter-strike-2-released-featuring-nvidia-reflex/)
- [Best NVIDIA Settings CS2 (Tradeit)](https://tradeit.gg/blog/best-nvidia-settings-for-cs2/)
- [Best AMD Settings CS2 (Hone.gg)](https://hone.gg/blog/amd-settings-for-cs2/)
- [gucu112/cs2-config (GitHub)](https://github.com/gucu112/cs2-config/blob/master/cs2_video.txt)
