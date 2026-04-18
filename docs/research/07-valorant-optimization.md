# Valorant Optimization — Guía para Tab Valorant (con foco Vanguard)

**Fecha:** 2026-04
**Foco crítico:** Compatibilidad con Vanguard anti-cheat (kernel-level)

---

## 0. TL;DR para Desarrolladores

Valorant es **fundamentalmente distinto** a CS2, Apex:

1. **Vanguard es kernel-level y carga en boot.** No se puede "cerrar" antes de jugar.
2. **Vanguard EXIGE seguridad activada**: TPM 2.0 + Secure Boot + HVCI/VBS + IOMMU + UEFI. Esto **invalida** la mayoría de tweaks "modo extremo" (disable VBS/HVCI, Legacy boot, etc.).
3. **Engine.ini / GameUserSettings.ini NO deben marcarse read-only.** Vanguard hace heartbeat checks → flaggea "unusual configuration".
4. **No todas las modificaciones de Engine.ini son seguras.** Riot no publica lista oficial.
5. **AMD Anti-Lag+ tuvo bans masivos en CS2/Apex en 2023**. Anti-Lag 2 se rediseñó con integración oficial.

---

## 1. Vanguard — Qué Permite y Qué NO (CRÍTICO)

### 1.1 Requisitos obligatorios Win11

Según Riot oficial ("Vanguard Restrictions"). Si alguna falla → `VAN: RESTRICTION` y el juego no inicia:

| Requisito | Estado | Error si falla |
|---|---|---|
| **TPM 2.0** habilitado y "ready" | Obligatorio desde lanzamiento Win11 | VAN 9001, VAN 9090 |
| **Secure Boot** enabled | Obligatorio Win11 | VAN 9003 |
| **UEFI boot** (no Legacy/CSM) | Obligatorio. OS drive debe ser GPT | VAN 9005 |
| **IOMMU** enabled BIOS | Obligatorio ("Pre-Boot DMA Protection") | VAN:RESTRICTION |
| **HVCI / Memory Integrity** | **Obligatorio desde jul-2024** | VAN:RESTRICTION:5 |
| **VBS** | Obligatorio (dependencia HVCI) | VAN 9005 |
| **Windows actualizado** | Obligatorio | VAN 9004, VAN 9006 |
| **Exploit Protection** enabled | Obligatorio | VAN 9002 |

**Implicación CRÍTICA:** Si preset "Extremo" incluye:
- Disable VBS / HVCI / Memory Integrity
- Disable Core Isolation
- Switch to Legacy BIOS
- Disable TPM / Secure Boot

→ Tu app **DEBE detectar Valorant instalado y bloquear/warnear** esos toggles. Si se aplican, próximo login a Valorant falla, potencialmente genera `VAN-152` si percibe patrón de evasión.

**BIOS vulnerable CVEs 2025:** Pre-Boot DMA Protection mal implementado en firmwares: Asus (CVE-2025-11901), Gigabyte (CVE-2025-14302), MSI (CVE-2025-14303), ASRock (CVE-2025-14304). Vanguard bloquea hasta BIOS update.

### 1.2 Detección VMs — nunca

Vanguard bloquea activamente:
- VMware (Workstation/Player)
- Hyper-V (si partición parent tiene hypervisor role activo)
- VirtualBox
- Windows Sandbox habilitado
- Virtual Machine Platform activada

Error: **VAN 138** "Virtual machine detected" o **VAN 9100** "Unsupported Virtualized Environment".

**NOTA:** WSL2/Windows Sandbox/Hyper-V aunque no corran VMs → reservan recursos de virtualización → impiden HVCI iniciar → VAN 9005. **Advertir explícitamente**.

### 1.3 Software de terceros (compat abril 2026)

| Software | Estado | Notas |
|---|---|---|
| **MSI Afterburner** (con RTCore64.sys) | **Bloqueado** | Fix: desactivar "Enable low-level IO driver" o cerrar antes. Solo v4.6.2 final tiene driver vulnerable firmado |
| **RivaTuner (RTSS)** | Bloqueado como injector overlay | Debe cerrarse antes del "Play" |
| **Discord overlay** | Compatible si inicia DESPUÉS del juego | Al momento "Play" → black screen. Desactivar In-Game Overlay |
| **NVIDIA GeForce Experience / ShadowPlay** | Compatible si después | Mismo patrón |
| **Xbox Game Bar** | Compatible pero desaconsejado | — |
| **MSI Center / Dragon Center** | Warning — algunas versiones con RTSS bundled | Verificar RTCore64.sys |
| **CPU-Z** (`cpuz_149x64.sys`) | Bloqueado hasta v1.81+ | Actualizar |
| **Cheat Engine / DBK64** | Bloqueado | HWID ban posible |
| **OpenHardwareMonitor** (WinRing0 driver) | Bloqueado | LibreHardwareMonitor también |
| **HWInfo64** | Mayormente OK si versión actualizada | — |
| **AIDA64** | Usar versión reciente | Driver viejo blocklisted |
| **Process Lasso** | **Compatible — 0 casos bans** | Bitsum confirma. Precaución con reglas agresivas en RiotClientServices |
| **ISLC** | **Compatible** | Opera a nivel sistema, no toca game/anticheat |
| **AMD Anti-Lag+ (2023)** | **Bans masivos CS2/Apex oct-2023** | AMD deshabilitó feature |
| **AMD Anti-Lag 2 (2024+)** | **Compatible oficialmente** | Riot integró soporte API |
| **iCUE (Corsair)** | Hay reports flags por driver antiguo | Mantener actualizado |
| **Razer Synapse** | Generalmente OK | — |
| **DDU** | OK (solo safe mode) | Usar ANTES de jugar |
| **NVIDIA Profile Inspector** | OK | Modifica driver profile, no game files |
| **InSpectre (Spectre toggle)** | **Riesgoso** | Desactivar Spectre vía registry puede romper HVCI → VAN:RESTRICTION |
| **TimerResolution 0.5ms** | OK | No interfiere |

### 1.4 `VAN: INCOMPATIBLE SOFTWARE`

Vanguard muestra path exacto del `.sys` culpable. La app puede capturar ese error y ofrecer eliminar el archivo. Ejemplo típico: `inpoutx64.sys`.

---

## 2. Engine.ini + GameUserSettings.ini

### 2.1 Ubicación

```
%LOCALAPPDATA%\VALORANT\Saved\Config\Windows\
```

Archivos:
- `GameUserSettings.ini` — resolución, scalability, framerate
- `Engine.ini` — tweaks UE-level (a veces no existe, crear manual)
- `Input.ini` — raw input
- `Game.ini`

### 2.2 ADVERTENCIAS CRÍTICAS RIOT/VANGUARD

> **NO poner archivos en read-only.** Vanguard hace verificaciones periódicas → read-only = "unusual configuration" → VAN:RESTRICTION

> **Modificaciones agresivas de Engine.ini pueden ser revertidas** por Valorant al lanzar. No hay lista pública de flags permitidos.

### 2.3 GameUserSettings.ini — Tweaks seguros

```ini
[/Script/ShooterGame.ShooterGameUserSettings]
FrameRateLimit=0.000000
bUseVSync=False
ResolutionSizeX=1920
ResolutionSizeY=1080
LastUserConfirmedResolutionSizeX=1920
LastUserConfirmedResolutionSizeY=1080
FullscreenMode=0                 ; 0 = Fullscreen, 1 = Windowed Fullscreen, 2 = Windowed
LastConfirmedFullscreenMode=0
PreferredFullscreenMode=0

[ScalabilityGroups]
sg.ResolutionQuality=100         ; No bajar <80 (blur extremo)
sg.ViewDistanceQuality=0
sg.AntiAliasingQuality=0
sg.ShadowQuality=0
sg.PostProcessQuality=0
sg.TextureQuality=0
sg.EffectsQuality=0
sg.FoliageQuality=0
sg.ShadingQuality=0
```

Son **los tweaks más seguros** — equivalentes a dropdowns in-game.

### 2.4 Engine.ini — Tweaks que circulan (USAR CON CAUTELA)

- **`bSmoothFrameRate=False`** — Tolerado. Seguro.
- **`r.OneFrameThreadLag=0`** — UE classic. Riesgo bajo pero Riot lo ha revertido en passes. Flags que modifican threading pueden ser flagged.
- **`r.GTSyncType=1`** — Mismo riesgo.
- **`r.FinishCurrentFrame=1`** — Puede REDUCIR FPS; usado para menor input lag. Riesgo bajo.
- **`r.RHICmdBypass=1`** — Raramente estable en Valorant.
- **`r.bForceCPUAccessToGPUSkinVerts=True`** — Beneficio dudoso.
- **`gc.TimeBetweenPurgingPendingKillObjects=180`** — Relativamente seguro pero memory leaks sesiones largas.

**Recomendación app:**
1. Sección "Experimental" con warning explícito
2. No incluirlos en preset "Safe"
3. Backup archivo original
4. Disclaimer: "Riot puede revertir; riesgo restriction"
5. Toggle revertir 1-click

---

## 3. In-Game Video Settings (Pro Consensus)

### 3.1 Resolución/display

| Setting | Pro Consensus |
|---|---|
| Resolution | **1920x1080 native** (mayoría) |
| Alt | 1280x960 4:3 stretched (nAts, algunos EU) |
| Aspect Ratio | 16:9 (~90%) vs 4:3 stretched (~10%) |
| Display Mode | **Fullscreen Exclusive** |
| Refresh Rate | Máximo monitor (XL2566K 360Hz, INZONE M10S 480Hz, XL2586X 540Hz) |

### 3.2 Graphics Quality

| Setting | Pro Consensus |
|---|---|
| Material Quality | **Low** |
| Texture Quality | **Low** (Low/Medium en 8GB+ VRAM) |
| Detail Quality | **Low** |
| UI Quality | **Low** |
| Vignette | **Off** |
| V-Sync | **Off siempre** |
| Anti-Aliasing | **None** (mayoría), **MSAA 2X** (TenZ), **MSAA 4X** (Derke/Boaster) |
| Anisotropic Filtering | **1X** (mayoría) hasta **16X** (nAts) |
| Improve Clarity | **Off** (aspas, Derke) vs **On** (Boaster) |
| Experimental Sharpening | **Off** (beta) |
| Bloom | **Off** |
| Distortion | **Off** |
| Cast Shadows / FPP Shadows | **Off** |
| First Person Shadows | **Off** |
| NVIDIA Reflex Low Latency | **On + Boost** crítico (-6-10ms) |
| HDR | No disponible |

### 3.3 Configuraciones pros élite (abril 2026)

| Pro | Team | DPI | Sens | eDPI | Polling | Res | AA | AF | Monitor |
|---|---|---|---|---|---|---|---|---|---|
| **TenZ** | Sentinels | 1600 | 0.1 | **160** | 1000Hz | 2560x1440 | MSAA 2X | 2X | Sony INZONE M10S 480Hz |
| **aspas** | LOUD/Leviatán | 800 | 0.5 | 400 | **4000Hz** | 1920x1080 | None | 1X | ZOWIE XL2586X 540Hz |
| **Derke** | Fnatic | 400 | 0.74 | 296 | **2000Hz** | 1920x1080 | MSAA 4X | 8X | 240Hz |
| **Boaster** | Fnatic | 800 | 0.24 | 192 | 1000Hz | 1920x1080 | MSAA 4X | 1X | 240Hz |
| **nAts** | Team Heretics | 800 | 0.49 | 392 | 1000Hz | **1280x960 4:3** | MSAA 4X | **16X** | 360Hz |

Notas:
- **TenZ bajó sens drásticamente** — eDPI 160 vs histórico ~280-320. Ahora usa 1440p
- **aspas adoptó 4000Hz polling**
- **eDPI promedio pro 2026: ~260-280** (mucho más bajo que CS2 ~800+)

---

## 4. NVIDIA Profile Inspector — Perfil VALORANT

Ejecutable: `VALORANT-Win64-Shipping.exe` (`C:\Riot Games\VALORANT\live\ShooterGame\Binaries\Win64\`)

| Setting | Valor |
|---|---|
| Power management mode | **Prefer maximum performance** |
| Low Latency Mode | **Off** (deja Reflex in-game) |
| Texture filtering - Anisotropic sample opt | On |
| Texture filtering - Quality | **High performance** |
| Texture filtering - Negative LOD bias | Allow |
| Texture filtering - Trilinear opt | On |
| Threaded optimization | **Auto** (o On) |
| Triple buffering | Off |
| Vertical sync | Off |
| Max frame rate | 0 (uncapped) o refresh - 3 si G-Sync |
| Preferred refresh rate | **Highest available** |
| Max Frames Allowed (render-ahead) | **1** |
| Shader Cache Size | Unlimited |
| Monitor Technology | G-SYNC Compatible si aplica |
| Anisotropic filtering mode | Application-controlled |
| Antialiasing - FXAA | Off |
| Antialiasing - Mode | Application-controlled |
| Background Application Max Frame Rate | 30 |
| DSR - Factors | Off |
| Reflex Low Latency (hidden 0x1074DF) | **Enabled + Boost** |

---

## 5. AMD Adrenalin — Per-Game Valorant

**Contexto histórico:** Oct 2023, AMD lanzó Anti-Lag+ que hacía detour engine.dll. Valve/VAC/Respawn/Riot lo flagearon → bans masivos. AMD deshabilitó.

**Anti-Lag 2 (2024+)** usa API game-integrated (oficialmente habilitada por devs). Valorant soporta oficialmente.

| Setting | Valor |
|---|---|
| **Anti-Lag 2** | **On** si disponible |
| Anti-Lag (v1) | Off |
| Radeon Boost | **Off** |
| Radeon Chill | **Off** |
| HYPR-RX | **Off** |
| Image Sharpening | Off (o ~30-40%) |
| Radeon Super Resolution / FSR | **Off/Irrelevant** (Valorant no soporta FSR) |
| Enhanced Sync | Off |
| Wait for Vertical Refresh | Always Off |
| Texture Filtering Quality | **Performance** |
| Surface Format Optimization | On |
| Tessellation Mode | Override → 8x o 16x |
| Morphological AA | Off |
| Anti-Aliasing Mode | Use Application Settings |
| GPU Workload | Graphics |
| Frame Rate Target Control | Off (o monitor Hz - 3) |

**Driver install:** DDU en Safe Mode entre major releases + AMD Cleanup Utility + solo "Driver + Adrenalin" sin bundled.

---

## 6. Network

### 6.1 Servidores (Riot Server Select)

**NA:** Portland, San Jose, Dallas, Atlanta, Ashburn (VA), Chicago
**EMEA:** Frankfurt, London, Paris, Stockholm, Warsaw, Madrid, Istanbul, Bahrain, Dubai
**BR/LATAM:** São Paulo, Santiago, Miami
**AP:** Tokyo, Seoul, Singapore, Hong Kong, Sydney, Mumbai
**KR:** Seoul

### 6.2 Configuración óptima

| Setting | Valor |
|---|---|
| Ethernet vs WiFi | **Ethernet obligatorio** competitive |
| Network Buffering | **Minimum** (7.8125ms delay + full 128 tick-rate send) |
| DNS primario | **1.1.1.1 (Cloudflare)** fastest (~10-15ms) |
| DNS secundario | 1.0.0.1 |
| IPv6 | Disable si ISP routing pobre |
| MTU | 1500 (default ethernet) |

### 6.3 Netcode — hechos técnicos

- **128 tick-rate server** (confirmed Riot)
- **Server-authoritative hit registration**
- No hay client-side prediction agresiva (a diferencia de CS2 sub-tick)
- **Network Buffering Minimum** permite client → server 128 updates/s

### 6.4 Route optimization

| Tool | Compat |
|---|---|
| **ExitLag** | Compatible (más recomendado Asia/LatAm/MENA) |
| **WTFast** | Compatible |
| **NoPing** | Alternative LatAm |
| **Outfox** | Compatible |

**Importante:** ninguno asociado con bans hasta la fecha, pero proxy/VPN gaming puede violar ToS en regiones específicas (geofencing ranked). **No integrar directamente**; solo detectar y advertir.

### 6.5 Packet Loss Troubleshoot

```
ping -n 100 -l 32 riotcdn.net
tracert -d vanguard.riotgames.com
pathping vanguard.riotgames.com
```

>1% → Network Buffering Moderate. >3% → diagnosticar ISP.

---

## 7. Launch Options (Riot Client)

### 7.1 Launch CLI

```
"C:\Riot Games\Riot Client\RiotClientServices.exe" --launch-product=valorant --launch-patchline=live
```

### 7.2 Limitaciones

A diferencia de Steam, Riot Client **no acepta launch parameters custom**. No se puede pasar `-windowed`, `-high`, `-threads`. **Única vía** = `GameUserSettings.ini`.

### 7.3 Task Scheduler High Priority

```xml
<Actions>
  <Exec>
    <Command>cmd.exe</Command>
    <Arguments>/c start "" /HIGH "C:\Riot Games\Riot Client\RiotClientServices.exe" --launch-product=valorant --launch-patchline=live</Arguments>
  </Exec>
</Actions>
```

**CUIDADO:** Priority High en `VALORANT-Win64-Shipping.exe` vía Task Manager o Process Lasso OK. **NO tocar priority/affinity de `vgc.exe` o `vgk.sys`** — son Vanguard → VAN:RESTRICTION.

### 7.4 Disable Fullscreen Optimizations

Props de `VALORANT-Win64-Shipping.exe` + `VALORANT.exe`:
- Tab Compatibility → check "Disable fullscreen optimizations"
- Elimina Windows fullscreen optimization layer. Reduce ~1-2ms input lag.

---

## 8. Audio

### 8.1 Spatial Audio — UNA SOLA capa

Si activas HRTF nativa Valorant, **desactiva**:
- Windows Sonic
- Dolby Atmos for Headphones
- DTS Headphone:X
- Razer Surround, Waves NX

Multiple layers → phase conflicts, positional smearing.

### 8.2 HRTF Premium (Valorant)

- Settings → Audio → Sound → **HRTF: On**
- Desarrollado específicamente para Valorant audio engine (ray-casting sound con propagation)
- Tests YouTube (Sliggy, Woohoojin): HRTF nativo > Windows Sonic > Dolby Atmos

### 8.3 Alternativas

- **Dolby Atmos for Headphones** ($15)
- **DTS Headphone:X v2**
- **Windows Sonic** (gratis, peor)

### 8.4 Windows audio config

| Setting | Valor |
|---|---|
| Sample rate default | 48000 Hz |
| Bit depth | 24 o 32 bit |
| Exclusive mode apps | Allow |
| Audio enhancements | Off |
| Communications device | Headset both default + communication |
| Spatial audio Windows | **Off** (si usas HRTF Valorant) |

---

## 9. Input

### 9.1 Mouse

| Setting | Recomendado |
|---|---|
| Polling rate | **1000Hz** mayoría. **4000Hz** (aspas) con CPU fuerte. **8000Hz** solo CPU tope |
| DPI | 400 / 800 / 1600 |
| eDPI competitive | **200-400 rango**, promedio ~260-300 |
| Raw input | Siempre On (hardcoded) |
| Raw Input Buffer (in-game) | **On** si ≥4000Hz. **Off** si 1000Hz + CPU débil |
| Windows mouse acceleration | **Off** |
| Windows pointer speed | **6/11** |
| Mouse smoothing | Off |

### 9.2 Keyboard

- Polling 1000Hz
- Anti-ghost obligatorio
- N-key rollover
- Hall Effect (Wooting, Keychron Q) legal en Valorant (no detona Vanguard)

### 9.3 Controller

Permitido pero sin aim assist competitive → no recomendado ranked.

---

## 10. System-Level — Matriz Vanguard

| Tweak | Vanguard Compat |
|---|---|
| VBS disable | **❌ NO — bloquea login** |
| HVCI / Memory Integrity disable | **❌ NO — bloquea login** |
| TPM disable | **❌ NO** |
| Secure Boot disable | **❌ NO** |
| UEFI → Legacy/CSM | **❌ NO** |
| IOMMU disable | **❌ NO** |
| WSL2 enabled | ⚠️ Riesgoso (reserva vcpu → HVCI fail) |
| Hyper-V / VM Platform enabled | ⚠️ Riesgoso |
| Windows Sandbox enabled | ⚠️ Riesgoso |
| Spectre/Meltdown mitigations disable | ⚠️ **Muy riesgoso** (puede romper HVCI) |
| Game Mode | ✅ OK |
| HAGS (Hardware-Accel GPU Scheduling) | ✅ OK, **requerido para Reflex 2** |
| MSI Mode GPU interrupt | ✅ OK (beneficio hitreg documentado) |
| Timer Resolution 0.5ms (ISLC/TimerTool) | ✅ OK |
| **ISLC** | ✅ OK |
| **Process Lasso** | ✅ OK |
| Bitsum BHP power plan | ✅ OK |
| Ultimate Performance power plan | ✅ OK |
| Core Parking disable | ✅ OK |
| Disable Xbox Game Bar | ✅ OK |
| Defender exclude Valorant folder | ✅ OK (con warning) |
| Defender real-time scan disable | ❌ Mal idea |
| NVIDIA Profile Inspector per-game | ✅ OK |
| **RTSS (RivaTuner)** | ⚠️ Version-specific, cerrar antes |
| **MSI Afterburner** | ⚠️ **RTCore64.sys bloqueado** |
| Driver updates GPU | ✅ OK |
| Debloat Windows (agresivo) | ⚠️ Puede romper Defender → VAN 9006 |
| Disable Windows Update | ❌ Mal idea |
| Disable SuperFetch/SysMain | ✅ OK |
| Intel XTU undervolt/underclock | ✅ OK (fix BSOD vgk.sys 13/14 gen) |
| AMD PBO tuning | ✅ OK |
| Overclocking | ✅ OK (si estable) |

### Presets recomendados

**"Conservative / Safe for Valorant":**
- Game Mode On, HAGS On, Game Bar Off
- Power plan: Bitsum BHP o Ultimate Performance
- Core parking disable
- Timer resolution 0.5ms
- NVIDIA/AMD per-game profile
- MSI mode GPU
- Disable SysMain
- ISLC enabled

**"Performance / Aggressive":**
- Todo lo anterior
- Windows visual effects minimal
- Disable startup bloat
- TCP optimizer applied
- DNS Cloudflare
- Defender exclude Valorant folder

**"Extreme" — BLOQUEAR si Valorant detectado:**
- VBS/HVCI disable → **Bloqueado**
- Spectre mitigations → **Bloqueado**
- Virtualization features → **Warning explícito**
- Legacy BIOS → **Bloqueado**

---

## 11. Pro Configs — Links verificados

### Bases de datos
- **ProSettings.net** — https://prosettings.net/games/valorant/ — 630+ pros
- **specs.gg** — https://specs.gg/
- **VLR.gg player pages** — https://www.vlr.gg/
- **Tracker.gg crosshair gallery** — https://tracker.gg/valorant/crosshairs/gallery?players=pros

### Crosshair codes

```
TenZ:     0;s;1;P;c;5;u;2AFF00FF;o;0;f;0;0l;2;0v;2;0g;1;0o;1;0a;1;0f;0;1b;0
aspas:    0;P;h;0;0l;4;0o;0;0a;1;0f;0;1b;0
Derke:    0;P;c;8;b;1;t;1;o;1;z;2;a;1;0t;2;0l;6;0v;6;0o;3;0a;0.8;0s;1;0e;1;1t;2;1l;2;1v;2;1o;10;1a;0.35;1s;1;1e;1;u;FFFFFF;d;1;h;1
Boaster:  0;s;1;P;c;1;o;1;d;1;0l;0;0o;2;0a;1;0f;0;1t;0;1l;0;1o;0;1a;0;S;c;1;o;1
nAts:     0;P;c;1;o;1.000;0a;1.000;0l;2;0t;1;0o;2;0f;0;1b;0
```

### Canales referencia
- **Woohoojin** (coach)
- **Sliggy** (análisis técnico)
- **Ethos** (crosshair/settings)
- **ProGuides Valorant**
- **TweakingGuy** — Twitter @TweakingGuy — CUIDADO: algunos tweaks BIOS agresivos para Intel 13/14 gen

---

## 12. Troubleshooting — VAN Error Codes

| Código | Significado | Acción |
|---|---|---|
| **VAN 0** | Client timeout (7+ días) | Restart |
| **VAN 1, 6, 68, 84** | Connection issues | Network, restart |
| **VAN -81** | Connection error | Firewall exception, admin |
| **VAN 128, 133-137, 140** | Connection failures | Restart PC, reinstall Vanguard |
| **VAN 135** | Connection error | Restart, cerrar FRAPS/recorders |
| **VAN -117** | Vanguard inactive | "Run Riot Vanguard" + clean boot |
| **VAN 138** | **VM detectada** | Host real |
| **VAN 148** | Vanguard error | Reinstall Vanguard |
| **VAN 152** | **HWID BAN** | Contact Riot Support. Permanente |
| **VAN 185** | Multi-device login timeout | Logout otros |
| **VAN 1067** | Connection failure | Reinstall Vanguard |
| **VAN 57** | Vanguard not running | Restart |
| **VAN 61** | **Cheating ban** | Permanente |
| **VAN 9001** | **TPM 2.0 disabled** | Enable BIOS |
| **VAN 9002** | **Exploit Protection off** | Enable Windows Security |
| **VAN 9003** | **Secure Boot disabled** | Enable BIOS |
| **VAN 9004** | **Windows outdated** | Update |
| **VAN 9005** | **UEFI/TPM/VBS requirement** | UEFI + TPM 2.0 + Memory Integrity |
| **VAN 9006** | Windows outdated | Update |
| **VAN 9051** | Launch error | Restart + reinstall |
| **VAN 9090** | TPM failed init | tpm.msc → "Prepare" → BIOS update |
| **VAN 9100** | **Virtualized env** | Disable Hyper-V/VirtualBox/VMware |
| **VAN 9101** | Untrusted machine | Update drivers, remove AV |
| **VAN:RESTRICTION** | BIOS/security missing | TPM + SB + UEFI + HVCI + IOMMU |
| **VAN:RESTRICTION:5** | **HVCI failed** | Device Security → driver list |

### vgk.sys BSOD en Intel 13/14 Gen

Causa: inestabilidad CPU Raptor Lake (voltage degradation scandal). Fix oficial:

1. **Actualizar BIOS al latest**
2. **Aplicar Intel Default Settings** (MSI), **Intel Baseline Profile** (ASUS)
3. BIOS:
   - Disable overclocking auto
   - Long duration power limit: 125W (13600K/14600K)
   - Short duration power limit: 253W
   - Disable enhanced modes, Thermal Velocity Boost
   - Reducir P-core multiplier 55x → 53-54x
4. Intel XTU para bajar P-core mult runtime
5. Último recurso: RMA Intel warranty

### FPS drops drop-in

Causas:
- Shader compilation (primera partida post-patch)
- Vanguard heartbeat coincidiendo carga map
- Antivirus escaneando game files
- SSD throttling temperatura

### Stuttering post-patch

Patrón conocido bi-weekly. Fix:
- Clear shader cache: `C:\Riot Games\VALORANT\live\ShooterGame\Saved\`
- Reiniciar PC tras patch
- Verify via Riot Client "Repair"

### Input lag percibido

1. Reflex On + Boost → sí
2. V-Sync off → sí
3. Fullscreen exclusive → sí
4. Polling rate estable → `mouserate.exe`
5. Power plan high performance
6. G-Sync/FreeSync + cap 3 below refresh
7. Disable Fullscreen Optimizations en exe

### Vanguard no inicia

1. `services.msc` → "vgc" → Automatic
2. Reinstall: Control Panel → uninstall Riot Vanguard → reboot → open Valorant
3. Clean boot (msconfig)
4. Windows Defender: exclusion `C:\Program Files\Riot Vanguard\`
5. Contact Riot Support

---

## 13. Process Priority & Affinity

### Seguros

| Proceso | Priority | Affinity |
|---|---|---|
| `VALORANT-Win64-Shipping.exe` | **High** | All cores (o P-cores only Intel hybrid) |
| `RiotClientServices.exe` | Normal/AboveNormal | All |
| `RiotClientUx.exe` | Normal | All |
| `Valorant.exe` | Normal | All |

### NO TOCAR

| Proceso | Razón |
|---|---|
| `vgc.exe` | Vanguard controller → VAN:RESTRICTION |
| `vgk.sys` | Kernel driver Vanguard |
| `VanguardWinSvc` | Servicio Windows |

### Intel Hybrid Affinity (12/13/14 Gen)

Valorant es single-threaded heavy + helpers → fijar **P-cores only**:
- 12 Gen i5/i7: 6 P-cores → mask `0xFFF` (con HT) o `0x3F` (sin HT)
- 13/14 Gen i7/i9: 8 P-cores → `0xFF` sin HT, `0xFFFF` con HT
- Ryzen 7950X3D/9950X3D → fijar al CCD con 3D V-Cache

**Herramientas:**
- Process Lasso (GUI + reglas persistentes)
- PowerShell: `Get-Process VALORANT-Win64-Shipping | ForEach-Object { $_.ProcessorAffinity = 0xFF }`

---

## 14. Lista NO-HACER (app DEBE bloquear/warnear)

1. ❌ Disable VBS/HVCI/Memory Integrity → VAN:RESTRICTION
2. ❌ Disable TPM 2.0 → VAN 9001
3. ❌ Disable Secure Boot → VAN 9003
4. ❌ Switch Legacy BIOS/CSM → VAN 9005
5. ❌ Disable IOMMU → VAN:RESTRICTION
6. ❌ Spectre mitigation disable → puede romper HVCI
7. ❌ Set GameUserSettings.ini/Engine.ini read-only → heartbeat flag
8. ❌ Engine.ini agresivos sin backup
9. ❌ Modificar priority/affinity de vgc.exe/vgk.sys
10. ❌ AMD Anti-Lag+ (v1) — Anti-Lag 2 OK
11. ❌ MSI Afterburner v4.6.2 con RTCore64.sys activo
12. ❌ Cheat Engine, WinRing0 drivers
13. ❌ CPU-Z v1.80 o anterior
14. ❌ Correr Valorant en VM
15. ❌ Habilitar WSL2/Hyper-V/Sandbox sin advertir HVCI fail
16. ❌ Disable Windows Update completamente
17. ❌ Debloater agresivo removiendo Defender components

---

## 15. Implementación — Detección Valorant

```
# Paths típicos
C:\Riot Games\VALORANT\
C:\Program Files\Riot Vanguard\
%LOCALAPPDATA%\VALORANT\
```

Verificar:
- Exe `VALORANT-Win64-Shipping.exe`
- Servicio `vgc` (reg: `HKLM\SYSTEM\CurrentControlSet\Services\vgc`)
- Driver `vgk.sys` en `C:\Program Files\Riot Vanguard\`

Si detectado → activar modo "Valorant-safe" automáticamente.

### UI sugerida

Tab "Valorant":
1. Status badge: "Vanguard Compatible ✓" / "⚠ Conflict Detected"
2. Secciones colapsables:
   - Video Settings (pro presets importables)
   - NVIDIA/AMD Profile
   - Network (DNS, Buffering)
   - System Safe Tweaks (Game Mode, Power, Timer Res)
   - **Experimental (disclaimer)** — Engine.ini tweaks
   - Troubleshooting (botón "Fix VAN 9001", "Fix VAN 9005")
3. Botón grande **"Revert All Valorant Changes"**

---

## Fuentes clave

- [Vanguard Restrictions - Riot](https://support-valorant.riotgames.com/hc/en-us/articles/22291331362067-Vanguard-Restrictions)
- [Vanguard Error Codes - Riot](https://support-valorant.riotgames.com/hc/en-us/articles/45690787593875-Vanguard-Error-Codes)
- [VAN:Restriction and Pre-Boot DMA - Riot](https://www.riotgames.com/en/news/vanguard-security-update-motherboard)
- [Addressing VBS on Windows 10 | VAN9005](https://support-valorant.riotgames.com/hc/en-us/articles/16941220890899-Addressing-Virtualization-based-security-VBS-settings-on-Windows-10-VAN9005-VALORANT)
- [vgk.sys Error Troubleshooting 13/14 Gen Intel](https://support-valorant.riotgames.com/hc/en-us/articles/30677093498515-vgk-sys-Error-Troubleshooting-13th-and-14th-Generation-Intel-CPUs)
- [VALORANT 128-Tick Servers - Riot](https://technology.riotgames.com/news/valorants-128-tick-servers)
- [Anti-Cheat Update Winter 2023](https://playvalorant.com/en-us/news/dev/anti-cheat-update-winter-2023/)
- [Process Lasso FAQ - Bitsum](https://bitsum.com/process-lasso-faq/)
- [RTCore64.sys and Valorant / Vanguard](https://forums.guru3d.com/threads/rtcore64-sys-and-valorant-vanguard.431963/)
- [AMD Anti-Lag+ Bans CS2/Apex](https://ggboost.com/blog/post/cs2-apex-valorant-amd)
- [AMD Radeon Anti-Lag Technology](https://www.amd.com/en/products/software/adrenalin/radeon-software-anti-lag.html)
- [NVIDIA Reflex 2.0 Technology Explained](https://www.hp.com/th-en/shop/tech-takes/post/nvidia-reflex-2-technology-gaming-experience)
- [ProSettings.net Valorant](https://prosettings.net/games/valorant/)
- [VALORANT Best Settings Guide - ProSettings](https://prosettings.net/guides/valorant-options/)
- [Mouse Polling Rate Explained](https://pollingratetester.com/mouse-polling-rate-explained-best-settings/)
- [Best Sound Settings for Valorant 2026](https://bo3.gg/valorant/articles/best-sound-settings-for-valorant)
- [How to Optimize Valorant Performance 2026](https://iqondigital.com/learn/games/optimize-valorant-performance)
- [Valorant FPS Boost Guide 2026 UE5](https://battlepooja.com/valorant-fps-boost-guide/)
