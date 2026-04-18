# App de Optimización Windows 11 Gaming — BIOS/CPU/RAM Tuning Avanzado

Reporte técnico research abril 2026. Estructura: qué existe, qué se puede automatizar, qué es comercialmente restringido, cómo implementarlo con guardrails de seguridad.

## Resumen ejecutivo

El espacio "optimizer" mainstream (Razer Cortex, Iobit) toca solo registry y power plans. Vectores reales de performance en Windows 11:

1. **BIOS-side** (reinicio): Intel Default Settings, EXPO/XMP, PBO, Curve Optimizer, Curve Shaper (Zen 5), DRAM secondaries
2. **Runtime** (Windows-side): Undervolt via FIVR/PBO2 Tuner/XTU, power plans, core parking, Intel APO, game/thread affinity
3. **Firmware**: BIOS update detection, microcode checks (Intel 0x12B), AGESA versions (AMD 1.2.0.0a+)

Pocas APIs oficiales — ecosistema serio es **parcialmente reverse-engineered** (.NET decompilation XTU, IPC a tools con scheduled tasks, LibreHardwareMonitor sensores). Nicho "unified tuning app with guardrails" **está vacante** y tiene demanda.

---

## 1. Tools per-vendor — tabla comparativa

| Vendor | Tool principal | CLI/API oficial | Automatable | Nota |
|--------|----------------|-----------------|-------------|------|
| **Intel** | Intel XTU 7.14 (Raptor/14th) / XTU 10.0 (Arrow Lake) | `XtuCli.exe` removido desde v6.4.1.25 (2019). `ProfilesApi.dll` accesible via PS reflection | Parcial | XTU AI Assist solo 14900K/KF/KS |
| **Intel** | ThrottleStop 9.7.3 beta | `ThrottleStop.ini` editable, watchdog mode | Sí (editar INI + relaunch) | Soporta Arrow Lake desde 9.7.3 |
| **AMD** | Ryzen Master 3.0.x | `AMDRyzenMasterCLI.exe`, SDK público | Sí (SDK + CLI) | SDK v2.6/3.0 para Zen 5 |
| **AMD comunidad** | PBO2 Tuner (PJVol) | CLI nativa: `pbo2tuner.exe -30 -30 ... ppt tdc edc` | Sí (scheduled task) | Único método runtime Curve Optimizer Windows |
| **AMD comunidad** | Hydra 1.8A PRO (1usmus) | Patreon-paid, no scriptable | No integrable | Zen 5 full, auto CO, auto DCFR RAM |
| **MSI** | MSI Center 3.x + SDK 3.2026.211.1 | SDK interno, no público | Limitado | Registry keys reverse-engineerables |
| **ASUS** | Armoury Crate / AI Suite 3 / AI Overclocking | Sin SDK público | No oficial | G-Helper alt OSS pero solo laptops |
| **Gigabyte** | GCC 26.03.x | Sin CLI | No | Historial CVEs |
| **ASRock** | A-Tuning / Polychrome | Sin API | No | |
| **Bajo-nivel** | RWEverything 1.7.x | CLI scripting Tcl-like | Peligroso | PCI/MSR/ACPI direct |
| **LibreHardwareMonitor** | NuGet LibreHardwareMonitorLib 0.9.7+ | Read-only C#/.NET 8/9/10 | Sí (sensores) | Admin + x64 |

**Conclusión integración**: **ThrottleStop + XTU (Intel) + PBO2 Tuner + Ryzen Master SDK (AMD) + LibreHardwareMonitorLib (monitoring)**. Vendor tools (MSI/ASUS/Gigabyte) — solo recomendar al usuario.

---

## 2. Intel XTU / ThrottleStop workflow técnico

### 2.1 Intel XTU 7.14 — profile apply programático

Ruta oficial `XtuCli.exe` hasta v6.4.1.25 (abril 2019). Después Intel lo removió. Método actual via **.NET reflection** sobre `ProfilesApi.dll`:

```powershell
# CRITICAL: PowerShell 32-bit (x86) porque ProfilesApi.dll es x86
[System.Reflection.Assembly]::LoadFrom(
  "C:\Program Files (x86)\Intel\Intel(R) Extreme Tuning Utility\Client\ProfilesApi.dll"
) | Out-Null

$api = [ProfilesApi.XtuProfiles]::new()
$api.Initialize() | Out-Null

[ProfilesApi.XtuProfileReturnCode]$result = 0
$profiles = $api.GetProfiles([ref] $result)

$p = $profiles | Where-Object { $_.ProfileName -eq "Gaming_Undervolt_Safe" } | Select -First 1
$applyResult = 0
$ok = $api.ApplyProfile($p.ProfileID, [ref]$applyResult)
```

**DLLs relevantes**: `IntelOverclockingSdk.dll` + `IntelBenchmarkSdk.dll` (divididos en versión reciente), firmados.

**Limitación UndervoltProtection**: Cuando Intel 14th Gen tiene UV Protection (post-microcode 0x12B), sliders con rayo amarillo no pueden bajar del BIOS voltage/boot voltage. Workaround = BIOS setting, no runtime.

**Arrow Lake / Core Ultra 200S**: requiere **XTU 10.0+** (10.0.1.45 marzo 2026). XTU 7.14 no ve Arrow Lake. Detectar generación → escoger DLL.

### 2.2 ThrottleStop 9.7.3 — automation INI-based

Ubicación config: `<ThrottleStop_folder>\ThrottleStop.ini`. Claves relevantes:

- `FIVRVoltage00=0xECC00000` → offset core (hex little-endian)
- `FIVRVoltage01=...` → offset cache (debe igualar core)
- `UnlockVoltage=1`
- `AddOffset=1`
- Perfiles duales AC/DC

**Workflow app**:
1. Parar servicio ThrottleStop (`sc stop`)
2. Backup `ThrottleStop.ini` → `.backup.<timestamp>`
3. Escribir nuevo INI con offsets target
4. Relanzar con `-minimized` y `SaveOptionsToINI=1`
5. Registrar scheduled task `ThrottleStopService` boot

**Conversión mV ↔ hex**: `(2^32) - (offset_mV / 0.9765625)`. Ejemplo -100mV ≈ `0xF3340000`. Usar tabla pre-computada.

**Soporte 14th Gen + Arrow Lake**: ThrottleStop 9.7.3 beta (abril 2025) agrega HX/K 10th-gen+ FIVR unlock y Arrow Lake. Laptops muchos FIVR locked — detectar flag "Locked" GUI y graceful-fallback.

---

## 3. AMD Ryzen Master / PBO2 Tuner / Hydra

### 3.1 Ryzen Master SDK

- Versión: **3.0.0.3620** (feb 2026)
- SDK archive.org + Chocolatey `amd-ryzen-master`
- APIs C++/C#/Java, 20+ calls
- `AMDRyzenMasterCLI.exe` existe pero documentación CLI escasa
- **Integración app**: SDK C++ via P/Invoke desde .NET, o embeber. Profile import/export = XML.

**Limitación crítica**: Ryzen Master **NO** expone Curve Optimizer/Curve Shaper. Esos requieren BIOS o PBO2 Tuner.

### 3.2 PBO2 Tuner — único runtime CO tool

Creado por **PJVol** (comunidad), standard de facto undervolt Zen 3/4/5 runtime.

**CLI format**:
```
pbo2tuner.exe <CO_core0> <CO_core1> ... <CO_coreN> [PPT] [TDC] [EDC] [BoostOverride]
# 9800X3D: todos -25
pbo2tuner.exe -25 -25 -25 -25 -25 -25 -25 -25 120 75 105 0
# 9950X3D: CCD0 (V-cache 0-7) conservador, CCD1 agresivo
pbo2tuner.exe -15 -15 -15 -15 -15 -15 -15 -15 -30 -30 -30 -30 -30 -30 -30 -30 200 140 170 200
```

**Persistencia**: Scheduled Task at logon con RunLevel=Highest. Referencia: `zoicware/PBOTuner2`, `vilmire/PBO2_Helper`.

**Detección cores**: `Get-CimInstance Win32_Processor | Select NumberOfCores`. Distinguir CCD0 vs CCD1 en dual-CCD (9950X3D): cores 0-7 CCD0 (V-cache), 8-15 CCD1 frequency.

### 3.3 Hydra PRO 1.8A (1usmus)

- **Licencia Patreon** (pago), **NO integrable** producto comercial
- Zen 5 full: 9800X3D/9950X3D con CO diag + DCFR RAM
- Auto-tune CO por-core (reemplazo CTR)
- **Decisión**: Solo recomendar UI "expert". No redistribuir, no automatizar.

### 3.4 CoreCycler (sp00n) — validación CO

PowerShell script estable para test single-core con Prime95/y-cruncher/Aida64. Ideal backend validación post-apply:
- Config `config.ini` editable
- Test modes: SSE, AVX, AVX2, CUSTOM
- Rotación per-core configurable
- `test_programs/y-cruncher/Command Lines.txt` tiene args exactos

---

## 4. RAM timing — presets per-IC + per-arch

### 4.1 Identificación IC

- **Taiphoon Burner**: funciona DDR4; **limitado en DDR5** (muchos kits no reconocidos)
- **Fallback DDR5**: Label physical (G.Skill ending `820A`=A-die, `820M`=M-die)
- **SPD via SMBus**: RWEverything puede, peligroso
- **ZenTimings v1.38+**: read-only, muestra timings + VDDIO DDR5
- **Recomendación app**: Taiphoon → fallback WMI SMBIOS string matching → fallback DB interna SKU→IC

### 4.2 Tabla presets DDR5 por IC + CPU

| IC | "Safe" Zen 4 (7800X3D) | "Safe" Zen 5 (9800X3D) | "Safe" Intel 14th | "Safe" Arrow Lake |
|----|------------------------|------------------------|-------------------|-------------------|
| **Hynix A-die** | 6000 CL30-36-36-76, VDD 1.40, VSOC 1.20, FCLK 2000 1:1 | 6000 CL28-36-36-76, VDDIO 1.40, VSOC 1.20, FCLK 2100 1:1 | 7200 CL34-45-45 Gear 2, SA 1.18 | 8000 CL38-48-48 Gear 4 UDM, SA 1.20, VDDQ 1.40 |
| **Hynix M-die** | 6000 CL30-38-38-80, VDD 1.35, VSOC 1.18 | 6000 CL30-38-38-80, VDDIO 1.38, VSOC 1.20 | 7200 CL36-46-46, Gear 2 | 8400 CL40-50-50 Gear 4 |
| **Samsung B-die DDR4** | N/A | N/A | 4000 CL15-16-16 Gear 1 | N/A |
| **Micron A/B-die DDR5** | 5600 CL36-38-38 XMP/EXPO stock | 5600 CL36-38-38 stock, o 6000 manual | 6400 CL36-38-38 | 7200 CL38-46-46 |

**Sweet spots 2026**:
- **Zen 4 (7000)**: DDR5-6000 CL30, FCLK 2000, UCLK:MCLK 1:1 — baseline
- **Zen 5 (9000)**: DDR5-6000 CL28-30, FCLK 2100, IMC acepta 6200/6400 con M-die
- **Zen 5 X3D Refresh (9950X3D2)**: rumoreado DDR5-9800 (CES 2026 leaks)
- **Intel 13/14 Gen**: DDR5-7200 CL34 (G2), sin IF ratio constraint
- **Arrow Lake (200S)**: Stock DDR5-5600 G2 / 6400 G4. "200S Boost" sube a 8000 con VCCSA 1.20 + VDDQ 1.40. Extreme 8800-9600 demanda SA 1.25-1.35
- **Arrow Lake Refresh (270K/250K Plus)**: DDR5-8400+ target

### 4.3 EXPO vs XMP — aplicar desde app

**Realidad técnica**: EXPO/XMP se escriben en SPD del módulo, pero **aplicación la hace BIOS al POST**. Desde Windows no se puede toggle XMP runtime sin reinicio y modificar NVRAM.

**Opciones desde app**:
1. **Guiar al usuario**: "Reiniciar → F2/Del → XMP I/II → Save" con screenshots vendor
2. **Vendor tools**: Algunas BIOS exponen `BIOSFlashBackPlus` / UEFI API, no documentadas
3. **NVRAM flip experimental**: `efibootmgr` / RWEverything escribir BIOS vars — **NO RECOMENDAR**, risk brick
4. **AGESA 1.2.0.0a+ "EXPO on-the-fly"** (AMD Zen 5): usuario cambia profile sin reboot — sigue BIOS-side, no Windows API

**Decisión arquitectura**: App **detecta** si XMP/EXPO activo (SPD read + timings actuales from ZenTimings/WMI), si no → wizard reinicio.

---

## 5. Safety guards obligatorios

### 5.1 Pre-change

- [ ] **Backup BIOS settings** — AFUWINx64 / `mogelpeter/BIOS-Utility`, guardar `bios_backup_<date>.rom`
- [ ] **Snapshot Windows state** — System Restore point, export registry power schemes
- [ ] **Log baseline** — temps idle/load, Cinebench R23 score, Time Spy, IDLE Vcore (HWiNFO64 CSV 10 min)
- [ ] **BIOS version check**: `Get-CimInstance Win32_BIOS`. Contrastar con DB vendor latest. Warnear si >6 meses behind
- [ ] **Microcode check Intel**: registry `HKLM\HARDWARE\DESCRIPTION\System\CentralProcessor\0\Update Revision` — debe ser ≥ `0x12B` en 13/14 Gen
- [ ] **AGESA check AMD**: Parse BIOS version string → DB → warn si < 1.2.0.0a (Zen 5 requiere Curve Shaper)
- [ ] **Rowhammer check**: Hynix DDR5 manufactured 2021-01 to 2024-12 → advertir CVE-2025-6202. Opción: triple refresh rate (−8.4% perf)

### 5.2 During change

- [ ] **Hard limits hardcoded** (no override user):
  - Intel 14 Gen: NO permitir Vcore > 1.40V, NO PL1 > 253W sin "Extreme warning"
  - Intel Arrow Lake: VCCSA ≤ 1.35V, VDDQ ≤ 1.45V
  - Zen 4: SOC ≤ 1.30V (SOC voltage scandal 2023)
  - Zen 5: SOC ≤ 1.25V, VDDIO ≤ 1.45V
  - CO negativos: max -30 all-core primera pasada, max -50 expert mode
- [ ] **Temp monitoring durante stress**: abort si Package > 95°C (Intel) / 90°C (AMD Zen 5) / 85°C (Zen 4 V-cache)
- [ ] **LibreHardwareMonitorLib** polling cada 500ms Vcore + temp + fan RPM
- [ ] **Abort trigger WHEA**: eventos `WHEA-Logger` id 17/18/19 Windows Event Log = inestabilidad → revert inmediato

### 5.3 Post-change

- [ ] **Boot counter registry**: `HKCU\Software\<MyApp>\TuningBootCounter` incrementar al aplicar, decrementar boot exitoso. Si counter ≥ 2 sin decrement → revert auto (BSOD loop)
- [ ] **Stability validation obligatoria**: Cinebench R23 loop 30 min + y-cruncher HNT+SFT+C17 10 min + TM5 extreme@anta777 1h (abortable con warning)
- [ ] **WHEA scan**: Post-stress, parse último hour Event Log buscando WHEA-Logger. Cualquier hit = reapply previous stable preset
- [ ] **Temp delta check**: Comparar stress temp pre vs post. Si post > pre + 10°C = probable voltage raise
- [ ] **Age-verify >18**: modal legal warning pre-expert, log aceptación

### 5.4 Revert paths

- [ ] **Intel XTU**: API `ProfilesApi.ApplyProfile(DefaultProfileID)` → reaplica BIOS default
- [ ] **ThrottleStop**: Delete `ThrottleStop.ini`, remove scheduled task, reboot
- [ ] **PBO2 Tuner**: Remove scheduled task, reboot (runtime offsets se pierden = autofix)
- [ ] **BIOS hard-brick**: Q-Flash Plus / BIOS Flashback (ASUS) / Flash BIOS Button (MSI) — documentar UI
- [ ] **"Nuclear" button**: Reset BIOS CMOS (guiar vendor-specific)

---

## 6. Wizard "Guided Undervolt" — UX step-by-step

### Fase 0 — Detection (auto)

1. CPU ID via WMI + registry CPUID leaf (distinguir 14 Gen vs 14 KS vs Arrow Lake)
2. Motherboard + BIOS version
3. Cooling tier inference (CPU tpkg idle + idle RPM perfil) → classify Air/AIO240/AIO360/Custom
4. RAM kit + IC (Taiphoon → SPD fallback → SKU DB)
5. Prerequisites: microcode Intel, AGESA AMD, BIOS age

### Fase 1 — Safe Auto-Config (reboot req)

- [x] **Apply EXPO/XMP** — warn "reboot required, guide into BIOS"
- [x] **Intel Default Settings (Performance profile)** — 14 Gen post-scandal BASELINE
- [x] **Ultimate Performance power plan** — `powercfg /duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61`
- [x] **Disable core parking**
- [ ] **Disable C-States** — requiere BIOS, solo pro audio gamers. Warn
- [x] **Enable Intel APO** (Arrow Lake/14 Gen only)
- [x] **Game Bar enabled** (9950X3D/dual CCD) — requerido para CCD parking en Win11 26H1
- [x] **CPPC Preferred Cores = Driver** (AMD) — BIOS side

### Fase 2 — Guided Undervolt Wizard

**Step 1: Conservative**
- Intel: -50mV FIVR core+cache (ThrottleStop)
- AMD Zen 4/5: -10 CO all-core (PBO2 Tuner)
- Apply → Cinebench R23 single + y-cruncher SFT 2 min → pass? next

**Step 2: Balanced**
- Intel: -80mV
- AMD: -20 CO all-core
- Apply → y-cruncher HNT+SFT+C17 10 min → pass? next

**Step 3: Aggressive (expert opt-in)**
- Intel: -100mV (warn: >-100mV often unstable)
- AMD Zen 5: -30 all-core, -35 CCD1 only (9950X3D split)
- Apply → CoreCycler per-core 15 min/core → pass? commit

**Step 4: Per-core tune (optional)**
- Reject weakest core via CoreCycler → bump CO back +5, repeat

**Safe max per-CPU (comunidad 2026)**:

| CPU | All-core safe CO | Aggressive | Typical good silicon |
|-----|------------------|------------|----------------------|
| 7800X3D | -20 | -30 | -25 |
| 9800X3D | -20 | -35 | -30 |
| 9950X3D CCD0 (V-cache) | -15 | -25 | -20 |
| 9950X3D CCD1 (frequency) | -25 | -40 | -30 |
| 14700K | -50mV | -100mV | -80mV |
| 14900K/KS | -40mV | -80mV | -60mV |
| Core Ultra 7 270K Plus | -60mV | -100mV | -75mV at 4.6GHz |
| Core Ultra 9 285K | -50mV | -90mV | -75mV |

---

## 7. Stress test integration

| Tool | Tipo | CLI | Duración app | Notas |
|------|------|-----|--------------|-------|
| **Cinebench R23** | CPU sustained | `Cinebench.exe g_CinebenchMinimumCpuTestDuration=600` | 10-30 min loop | Baseline + thermal |
| **Prime95 30.19** | CPU torture | `prime95.exe -t` + prime.txt | 1h initial, 12-24h final | Small FFTs = L1/L2. AVX2/512 select |
| **OCCT Pro/Enterprise** | All-in-one | `occt.exe /t CPU /d 600 /l 0 /i AVX2` | 1h CPU + 1h RAM | **Solo Enterprise** CLI GUI automation |
| **y-cruncher 0.9.3+** | CPU+RAM combo | `SFTv4,HNT,SNT,C17,NTT63,VT3` | 10-30 min | **Mejor detector errores 2026** |
| **TestMem5 (TM5)** | RAM | Config `Cfg.link` → `extreme@anta777.cfg` | 1h initial, 3h final | Standard RAM runtime |
| **Karhu RAM Test** | RAM | GUI-only | 1-2h, 10k% coverage | $10 license, 15x faster HCI. Best DDR5 |
| **HCI MemTest Pro** | RAM | Silent batch | 400% × N instances | Legacy |
| **Memtest86+** | RAM bootable | Boot USB | 4 passes = 4-18h | Único post-BIOS válido |
| **Linpack Xtreme** | CPU HPC | CLI config | 1h | Thermal heavy |
| **AIDA64 Stability** | Mixed | GUI-automatable | 1-2h | Comercial |
| **CoreCycler (sp00n)** | Per-core stability | PowerShell CLI | 15 min × cores | Wrapper P95/y-cruncher |

**Pipeline integration app**:

```
[Quick Validate]  : Cinebench R23 single (5 min) + y-cruncher HNT (3 min)      = 10 min
[Standard]        : + TM5 anta777 extreme (1h) + CoreCycler y-cruncher (15/core) = 2-3h
[Stability Final] : + Prime95 Small FFTs (12h) + Memtest86 4 passes             = 24h+
```

Default usuario = Standard.

---

## 8. Warranty concerns

| Vendor | Undervolt | Overclock | Protection opt-in |
|--------|-----------|-----------|-------------------|
| **Intel** | Técnicamente voids, imposible probar CPUs recientes | Voids excepto K SKUs (Intel Performance Tuning Protection Plan — discontinuado) | Warranty +2 años automática post-scandal 13/14 Gen |
| **AMD** | Permitido con Ryzen Master/PBO2 | Voids técnicamente pero rara enforcement. EXPO = on-brand profiles cubiertos | PBO = "outside factory specifications" → voids según ToS |
| **MoBo vendors** | No voids | XMP/EXPO covered excepto daño IMC | Generalmente lenient |

**Legal UI app**:
- Pre-expert mode modal: "Acepto que tuning agresivo puede voidear warranty y dañar hardware. >18 años."
- Log timestamped de aceptación
- Botón "Reset to Intel Default / AMD stock" siempre visible

**Chile/LatAm consumer law**: SERNAC permite reclamo por "vicio oculto" independiente warranty fabricante. Dejar claro UI (ES).

---

## 9. Intel 14th Gen post-scandal — workflow seguro

**Root cause Intel oficial**: Vmin Shift Instability en clock tree IA core, vulnerable a aging con voltage+temp elevado. 4 vectores:
1. Motherboard power delivery sobre Intel guidance → Intel Default Settings
2. eTVB microcode high thermals → microcode `0x125`
3. SVID algorithm high voltages → microcode `0x129`
4. Idle voltage spikes → microcode `0x12B` (encompass 0x125 + 0x129)

**Workflow app 13/14th Gen**:

1. **Detect CPU** — i9-13900K/KS, i7-13700K, i9-14900K/KS, i7-14700K = high-risk. i5-14600K = low-risk
2. **Check microcode**: registry `Update Revision`. Si < `0x12B` → BLOQUEAR tuning agresivo, "Actualiza BIOS primero"
3. **Apply Intel Default Settings (Performance)**: PL1/PL2=125/253W, Iccmax=307A, 15-40°C VR thermal limits
4. **Undervolt SAFE** (post-microcode): XTU/ThrottleStop -50mV max. Más allá: no menciona Intel oficial
5. **Check degraded CPU**: Pre-existing degradation no reversible. Síntomas: crashes random loading shaders (Unreal 4/5), out-of-video-memory, instability. App correr diag: y-cruncher HNT 30 min stock → si falla = probable degradation → recomendar RMA
6. **Warranty +2 años auto**: 5 años desde purchase. Confirmar serial + purchase date

**MSI-specific (común en Chile)**: BIOS expone "CPU Cooler Tuning → Boxed Cooler" preset one-click PL=253W stock. "CPU Lite Load Control → Intel Default" voltage. App puede guide screenshot-by-screenshot.

**ASUS**: BIOS aplica automático Intel Default Settings como factory default en updates post-agosto 2024.

---

## 10. Zen 5 (9800X3D / 9950X3D / 9950X3D2) presets

### 9800X3D (single CCD 8-core V-cache)

AGESA 1.2.0.0a+ obligatorio (Curve Shaper).

| Tier | CO all-core | PPT/TDC/EDC | RAM | Expected |
|------|-------------|-------------|-----|----------|
| Stock | 0 | 230/160/225 | DDR5-6000 EXPO | Baseline |
| Safe | -15 | 230/160/225 | DDR5-6000 CL30 | -5°C, +2-3% perf |
| Balanced | -25 | 230/160/225 | DDR5-6000 CL28 tuned | -8°C, +5% perf |
| Aggressive | -30 per core | 250/170/240 | DDR5-6200 CL30 | -12°C, +7-8% |
| Max daily | -35 weakest +5 | 300/200/270 | DDR5-6400 CL32 manual | Risk silicon lottery |

**Curve Shaper bonus** (AGESA 1.2.0.0a+): Reshape voltaje por banda frecuencia. Bajar bandas estables, subir inestables. App exponer 5 bandas × 5 temps. Default: zeros. Advanced tier.

### 9950X3D (dual CCD asymmetric)

- **CCD0**: 8 cores V-cache, 5.2 GHz boost, gaming
- **CCD1**: 8 cores frequency, 5.7 GHz boost, productivity

**Core parking issue Win11 26H1 "Bromine"**:
- `AMD 3D V-Cache Performance Optimizer` intenta park CCD1 durante gaming
- Nuevo AI-Scheduler kernel intenta balancear heat entre CCDs
- **Conflict = CCD parking fails randomly**

**Fix community 2026**:

```powershell
# BIOS: CPPC Dynamic Preferred Cores = DRIVER (guide user)
# Windows: Enable Xbox Game Bar (Settings → Gaming → Xbox Game Bar = ON)
# powercfg: Balanced power plan (AMD recomend, NO Ultimate)
powercfg /setactive 381b4222-f694-41f0-9685-ff5bb260df2e

# Disable problematic GameDVR si rompe parking
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowGameDVR" /t REG_DWORD /d 0 /f

# Verify AMD 3D V-Cache Performance Optimizer running
Get-Service -Name "AMD3DVCacheOptimizer" | Select Status
```

**Curve Optimizer split 9950X3D**:

```
CCD0 (cores 0-7, V-cache): -15 to -25 safe, Curve Shaper conservative
CCD1 (cores 8-15, frequency): -25 to -40 safe (más headroom)
```

PBO2 Tuner arg order matters. App detecta topología con `Get-CimInstance Win32_Processor` + `Win32_PerfFormattedData_PerfOS_Processor` para identificar cores por cluster.

### 9950X3D2 / 9850X3D (CES 2026 leaks)

Rumor: **dual V-cache** ambos CCDs — elimina parking issue. DDR5-9800 nativo. No confirmar en app hasta launch oficial.

---

## 11. Arrow Lake / Core Ultra 200S específico

### Hardware layout

- P-cores: hasta 8 (5.7 GHz boost 285K)
- E-cores: hasta 16 (Skymont arch, +32% IPC vs Raptor Lake — **NO DESACTIVAR**)
- No HyperThreading (primera gen desktop Intel sin HT desde ~2005)
- NPU (no gaming-relevant)
- IMC DDR5-5600 Gear 2 stock / 6400 Gear 4

### Tunings

1. **Intel APO activation** — required para boost gaming. Intel APO UI tool (download separado). Registrar app post-install
2. **Undervolt P-cores** — -75mV a 4.6 GHz típico. AVO (Advanced Voltage Offset) nuevo. V/F curve 7-8 puntos per cluster
3. **Undervolt E-cores** — cluster independiente
4. **Ring/Uncore** — undervolt separado, -30mV típico
5. **SA voltage (VCCSA)** — nuevo Core Ultra. Stock 1.05-1.12V para DDR5-5600/6400. 1.20V para 8000 Gear 4 ("200S Boost"). 1.25-1.35V para 8800-9600 (extreme)
6. **VDDIO** — 1.20-1.40V stock, hasta 1.45V extreme
7. **Arrow Lake Refresh (270K/250K Plus marzo 2026)**: ratios más alto stock, OC Ratio point fused + último user-defined

### Specific fixes

- **Scheduling issues**: resueltos en **KB5044384** (noviembre 2024). Verificar `Get-HotFix -Id KB5044384`, warn missing
- **BIOS críticos**: Intel Dynamic Tuning = Enabled (DTT), APO = Enabled, HT = N/A, E-cores = Enabled (NUNCA disable)

---

## 12. BIOS update detection + outdated warning

### Workflow

```powershell
# 1. Get current BIOS info
$bios = Get-CimInstance Win32_BIOS | Select Manufacturer, SMBIOSBIOSVersion, ReleaseDate, SerialNumber
$mobo = Get-CimInstance Win32_BaseBoard | Select Manufacturer, Product, Version, SerialNumber

# 2. Extract vendor-specific version schema
# ASUS: "1602" → parse int
# MSI: "7D91vAF" → parse letter+number
# Gigabyte: "F12" → F<digit>
# ASRock: "P1.40" → P<version>

# 3. Age check
$ageDays = (Get-Date) - [Management.ManagementDateTimeConverter]::ToDateTime($bios.ReleaseDate)
if ($ageDays.Days -gt 180) { Write-Warning "BIOS > 6 months old" }

# 4. Vendor API lookup (WebRequest)
# No vendor tiene public REST API para BIOS versions
# Workaround: scrape vendor support pages per mobo SKU
# Cache 7 días
```

### Critical checks

- **Microcode Intel**: `HKLM\HARDWARE\DESCRIPTION\System\CentralProcessor\0\Update Revision`:
  - 13/14 Gen: actual = `0x12B` (oct 2024), antes `0x129` (sep), `0x125` (aug). `<0x12B` → warn
  - Arrow Lake: actual = `0x114` baseline, 0x11X updates performance
- **AGESA AMD**: Parse BIOS version:
  - AM5 actual = `1.2.0.2c` (feb 2026) / `1.2.8.0` (Rowhammer mitigation + 2026 CPU)
  - Zen 5 Curve Shaper requires ≥ `1.2.0.0a`
  - Warn si < 1.2.0.0a en Zen 5
- **CVE awareness**:
  - DDR5 Hynix 2021-2024 → CVE-2025-6202 Phoenix Rowhammer
  - BIOS CVEs vendor-specific (LogoFAIL, BRLY-2022-XXX)
  - TPM CVEs, Intel SA-* advisories
- **Firmware capsule Windows Update**: distribuye BIOS updates en algunos mobos (Surface, algunos ASUS). Chequear `Get-CimInstance -Namespace root/Microsoft/Windows/Updates`

### UI pattern

```
┌─────────────────────────────────────────┐
│ BIOS Status                             │
├─────────────────────────────────────────┤
│ Vendor: MSI MPG X670E Carbon            │
│ Version: 7D70vAH                        │
│ Release: 2024-09-12  (18 meses atrás)   │
│ ⚠ Outdated — latest: 7D70vAM (2026-02) │
│                                          │
│ [Open vendor page]  [Skip warning]      │
└─────────────────────────────────────────┘
```

---

## 13. Backup / restore workflow completo

### Backup layers (pre-tuning)

| Layer | Method | Restore |
|-------|--------|---------|
| **BIOS settings** | AFUWINx64 read → `.rom` / foto manual | AFUWIN flash (risk) / foto-guided re-entry |
| **NVRAM vars (UEFI)** | `bcdedit /export` boot, vendor Q-Flash BIOS vars | bcdedit import, vendor tool |
| **Registry power plan** | `powercfg /export <GUID> file.pow` | `powercfg /import file.pow` |
| **Registry tweaks** | `reg export HKLM\... file.reg` per-key | `reg import file.reg` |
| **ThrottleStop.ini** | File copy `.backup.<timestamp>` | File restore |
| **XTU profile** | Profile export UI / `ProfilesApi.GetProfile().Serialize()` | `ProfilesApi.ImportProfile()` |
| **PBO2 Tuner state** | Scheduled task XML + args | Delete task, reboot |
| **System state** | Windows System Restore point | System Restore UI |
| **Audit log** | JSON log timestamped de cada cambio | Replay inverse from log |

### Restore UI

```
┌─────────────────────────────────────────┐
│ Tuning History                          │
├─────────────────────────────────────────┤
│ ● 2026-04-18 14:32  Gaming_Undervolt   │
│   CO -25, PPT 230, FCLK 2100            │
│   [Revert]  [View diff]                 │
│                                          │
│ ● 2026-04-17 10:15  Safe_Baseline      │
│   [Revert]  [Active]                    │
│                                          │
│ [Restore factory defaults (nuclear)]    │
└─────────────────────────────────────────┘
```

Nuclear: Intel Default Settings + remove PBO2 Tuner scheduled task + delete ThrottleStop.ini + restore default power plan + clear XTU profiles + recommend CMOS reset.

### Boot loop protection

Counter pattern:

1. App escribe `HKCU\Software\MyApp\BootGuard\PendingReboot=1` + `TuningApplied=<hash>`
2. App registra scheduled task `BootGuard` corre at boot delay 60s
3. BootGuard: lee flag. Si `PendingReboot=1` post-boot = OK, setea `=0`. Si next boot aún `=1` = BSOD reboot loop → rollback pending tuning
4. Counter ≥ 2 sin decrement → full nuclear revert

---

## Decisiones arquitectura app

**Stack recomendado**:
- **C# .NET 8+ WPF** (o WinUI 3 moderno) — Windows-native, fácil P/Invoke a AMD SDK / XTU .NET DLL
- **LibreHardwareMonitorLib NuGet** — sensores
- **Embedded wrappers**:
  - ThrottleStop: process wrapper, INI edit + scheduled task
  - PBO2 Tuner: bundled exe + scheduled task
  - CoreCycler: bundled PowerShell + invoke
  - OCCT Personal: user-install, detect path, integrate CLI if Pro licensed
- **AMD Ryzen Master SDK**: C++ → C# P/Invoke or embedded native dep
- **Intel XTU**: require user-install (licensing), `ProfilesApi.dll` reflection

**NO integrar (comerciales/licenses)**:
- Hydra PRO (Patreon) — solo recomendar
- Karhu RAM Test ($10) — solo recomendar
- AIDA64 — solo recomendar

**Modular architecture**:

```
[UI Layer WPF]
     ↓
[Orchestration Service]
     ↓
┌─────────┬─────────┬─────────┬─────────┐
[Detection] [Tuning] [Validation] [Safety]
     ↓         ↓          ↓          ↓
 [Vendor    [XTU/TS   [Stress   [Monitor+
  detect]   PBO2T/    tests]    rollback]
            RM SDK]
     ↓
[Persistence: SQLite config + JSON audit log]
     ↓
[LibreHardwareMonitor sensor polling]
```

**Licencias app**:
- Free tier: Fase 1 safe + simple CO/UV
- Pro tier: Curve Shaper, per-core tune, expert mode, Arrow Lake advanced, stress test suite integration

---

## Fuentes (priorizadas autoridad)

### Oficial Intel/AMD
- [Intel XTU Download](https://www.intel.com/content/www/us/en/download/17881/intel-extreme-tuning-utility-intel-xtu.html)
- [Intel Application Optimization](https://www.intel.com/content/www/us/en/support/articles/000095419/processors.html)
- [Intel Core 13/14 Instability Root Cause](https://community.intel.com/t5/Blogs/Tech-Innovation/Client/Intel-Core-13th-and-14th-Gen-Desktop-Instability-Root-Cause/post/1633239)
- [AMD Ryzen Master User Guide 3.0.1](https://docs.amd.com/r/en-US/68886-ryzen-master-user-guide/)
- [AMD Ryzen Master SDK archive](https://archive.org/details/amd-ryzen-master-sdk)

### Reviewers autoritativos
- [SkatterBencher — 9800X3D OC](https://skatterbencher.com/2024/11/06/skatterbencher-82-ryzen-7-9800x3d-overclocked-to-5750-mhz/)
- [SkatterBencher — Core Ultra 7 270K Plus](https://skatterbencher.com/2026/03/23/skatterbencher-101-core-ultra-7-270k-plus-overclocked-to-5800-mhz/)
- [SkatterBencher — Arrow Lake Ring OC](https://skatterbencher.com/2024/10/24/arrow-lake-ring-overclocking/)
- [Tom's Hardware — Arrow Lake OC Guide](https://www.tomshardware.com/pc-components/overclocking/overclocking-arrow-lake-how-i-set-world-records-and-pushed-it-to-the-limit)
- [Tom's Hardware — PBO & Curve Optimizer](https://www.tomshardware.com/pc-components/cpus/how-to-use-precision-boost-overdrive-and-curve-optimizer-to-improve-ryzen-cpu-performance)
- [Tom's Hardware — DDR5 OC Guide](https://www.tomshardware.com/how-to/overclock-ddr5-ram)
- [Hardware Busters — 9800X3D Review](https://hwbusters.com/cpu/amd-ryzen-7-9800x3d-cpu-review-performance-thermals-power-analysis/)
- [Hardware Busters — Intel Default Profiles](https://hwbusters.com/cpu/checking-intels-power-profiles-baseline-performance-and-extreme/)
- [Igor's Lab — Intel Default Settings](https://www.igorslab.de/en/intel-spielt-mit-dem-namen-und-den-daten-das-intel-baseline-profile-wird-zu-intel-default-settings/)

### Community devs
- [1usmus Patreon (Hydra)](https://www.patreon.com/1usmus)
- [Hydra 1.8A PRO Zen 5](https://www.patreon.com/posts/hydra-1-8a-pro-5-111128881)
- [sp00n/CoreCycler](https://github.com/sp00n/corecycler)
- [integralfx MemTestHelper](https://github.com/integralfx/MemTestHelper)
- [zoicware/PBOTuner2](https://github.com/zoicware/PBOTuner2)
- [vilmire/PBO2_Helper](https://github.com/vilmire/PBO2_Helper)
- [irusanov/ZenTimings](https://github.com/irusanov/ZenTimings/releases)
- [TestMem5 official](https://testmem5.com/)
- [Apply Intel XTU from PowerShell](https://gist.github.com/michael-baker/f3962ba8d21ebd680b8e76c20eaa48c1)

### Forums
- [Overclock.net — AMD Hynix DDR5 Guide](https://www.overclock.net/threads/amd-hynix-ddr5-overclocking-guide.1801842/)
- [Overclock.net — 9800X3D Curve Optimizer Tips](https://www.overclock.net/threads/9800x3d-curve-optimizer-tips.1814218/)
- [Overclock.net — CoreCycler thread](https://www.overclock.net/threads/corecycler-tool-for-testing-single-core-stability-e-g-curve-optimizer-settings.1777398/)
- [Overclock.net — Arrow Lake OC](https://www.overclock.net/threads/overclocking-arrow-lake-285k-265k-245k-etc-results-bins-and-discussion.1811860/)
- [Overclock.net — 9950X3D core parking](https://www.overclock.net/threads/how-i-fixed-core-parking-on-my-9950x3d-and-taichi-lite.1815819/)
- [Overclockers.com — AM5 Hynix 6000 Timings](https://www.overclockers.com/forums/threads/am5-6000mhz-memory-timings-thread-sk-hynix.803076/)

### Stability testing
- [OCCT Enterprise](https://www.ocbase.com/occt/enterprise)
- [Prime95](https://prime95.net/)
- [y-cruncher](http://www.numberworld.org/y-cruncher/)
- [Karhu Software RAM Test](https://www.karhusoftware.com/)
- [MemTest86 — PassMark](https://www.memtest86.com/)
- [TechPowerUp ThrottleStop 9.7.3](https://www.techpowerup.com/download/techpowerup-throttlestop/)

### Other
- [UltrabookReview — ThrottleStop Guide 2026](https://www.ultrabookreview.com/31385-the-throttlestop-guide/)
- [MSI Blog — Intel 13/14 Default Settings](https://www.msi.com/blog/improving-stability-of-13th-14th-gen-intel-core-processors-with-intel-default-settings)
- [LibreHardwareMonitor GitHub](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- [RWEverything](http://rweverything.com/)
- [CVE-2025-6202 Phoenix Rowhammer](https://nvd.nist.gov/vuln/detail/CVE-2025-6202)
- [Tekknological — Intel APO games list 2026](https://tekknological.com/2026/03/25/list-of-games-supporting-intel-apo/)
- [WCCFTech — AGESA 1.2.0.0a Curve Shaper MSI](https://wccftech.com/msi-agesa-1-2-0-0a-bios-am5-motherboards-adds-curve-shaper-expo-on-the-fly-opp-support-amd-ryzen-9000/)
- [TechPowerUp — ZenTimings](https://www.techpowerup.com/download/amd-ryzen-zen-timings/)
- [Tech Review Guide — 9950X3D Core Parking Fix](https://techreviewguide.com/fix-ryzen-9-9950x3d-core-parking-issue-powercfg-command/)
- [mogelpeter/BIOS-Utility](https://github.com/mogelpeter/BIOS-Utility)
