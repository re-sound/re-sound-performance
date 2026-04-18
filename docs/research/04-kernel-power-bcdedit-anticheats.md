# Windows 11 Gaming Optimization — Low-Level Deep Tweaks Research

Capa avanzada: kernel scheduler, timer resolution, BCDEdit, power management, memory, DPC latency, Spectre/Meltdown mitigations, VBS/HVCI, anticheat compatibility.

Prioridad: evidencia sobre opinión. Para cada tweak se indica impacto medible, consenso actual (Win11), riesgo de inestabilidad y compatibilidad con anticheats kernel-level.

---

## Tabla de contenidos

1. [Timer / Scheduler / Kernel](#1-timer--scheduler--kernel)
2. [BCDEdit completo](#2-bcdedit-completo)
3. [Power plans](#3-power-plans)
4. [Memory management](#4-memory-management)
5. [DPC latency (tools + workflow)](#5-dpc-latency-tools--workflow)
6. [Spectre / Meltdown mitigations](#6-spectremeltdown-mitigations)
7. [VBS / HVCI / Core Isolation](#7-vbs--hvci--core-isolation)
8. [File system / Storage](#8-file-system--storage)
9. [Boot / Fast Startup / Hibernate](#9-boot--fast-startup--hibernate)
10. [Defender + misc](#10-defender--miscelaneos)
11. [Anti-cheat compatibility matrix](#11-anti-cheat-compatibility-matrix)
12. [Presets: Extremo vs Seguro vs Competitivo](#12-presets-extremo--seguro--competitivo)

---

## 1. Timer / Scheduler / Kernel

### 1.1 Timer Resolution — ¿sigue sirviendo en Windows 11?

**Contexto.** Desde siempre Windows ha usado un "clock interrupt" con resolución default de 15.625 ms (= 64 Hz). Aplicaciones (juegos, navegadores, media) pueden llamar a `NtSetTimerResolution` o `timeBeginPeriod` para bajarlo a 1 ms o 0.5 ms. Eso aumenta la precisión de `Sleep()`, `WaitForSingleObject`, etc. — cosa clave en bucles de juego.

**Cambio clave en Windows 10 2004 (aplica a Win11).** Microsoft pasó de **global** a **per-process** el efecto de las APIs de timer resolution. Es decir: si ISLC o un servicio setean 0.5 ms, **el juego no lo hereda** salvo que ese mismo proceso lo pida. Muchas apps (ej. Chrome, DWM, Discord) ya piden 1 ms, así que en la práctica Win11 suele estar entre 1 ms y la default, pero no hay garantía.

**Verificación.** PowerShell (admin):
```
powercfg /energy /duration 60
# luego abrir energy-report.html — sección "Platform Timer Resolution"
```
También: `clockres.exe` (Sysinternals).

**Cómo forzar 0.5 ms globalmente en Win11+ (oficial).** Existe un registry switch que revierte al comportamiento pre-2004:
```
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel]
"GlobalTimerResolutionRequests"=dword:00000001
```
Documentado por `valleyofdoom/TimerResolution` y en release notes de ISLC v1.0.2.9 (opción "Windows 11 system timer"). **Requiere reboot.**

**Tools concretos:**

| Tool | Qué hace | Cuándo usarlo |
|---|---|---|
| **TimerResolution (Lucas Hale)** | GUI que llama `NtSetTimerResolution` en un proceso corriente. **Per-process** en Win11 sin el flag — casi inútil como tweak global. | Legacy / medir |
| **SetTimerResolutionService** | Servicio que arranca al boot y deja pinned 0.5 ms. Con `GlobalTimerResolutionRequests=1` es system-wide. | Recomendado Win11 |
| **valleyofdoom/TimerResolution** (GitHub) | C++: `SetTimerResolution.exe` + `MeasureSleep.exe` para benchmark precisión real. GPL-3.0. | Validar resultados |
| **ISLC (IntelligentStandbyListCleaner)** | Limpia standby list + set timer resolution. v1.0.2.9+ expone toggle Win11 system timer. | Si hay stutter por standby lleno |
| **MSIUtil v3** (aparte, ver §5) | No es timer sino MSI interrupts. | Complementario |

**Consenso actual (Win11):**
- En **desktops con TSC invariante** + CPU moderna, subir a 0.5 ms **rara vez mueve 1% lows** medibles. Blur Busters / r/overclocking reportan diferencias dentro del margen de error en la mayoría de juegos.
- Sigue siendo relevante en **audio real-time** (DAWs, VoIP) y en juegos **CPU-bound con engines antiguos** que usan `Sleep(1)` para throttling.
- **Microsoft** desaconseja valores <1 ms en docs oficiales: aumenta consumo, reduce batería en laptops, y en servidores no rinde.

**Riesgo.** En laptops: menos battery life (interrumpe C-states profundos). En desktops: nada crítico, pero si un proceso queda "atascado" pidiendo 0.5 ms cuando no debería, genera wakeups innecesarios.

**Veredicto app.** Ofrecer toggle "System-wide 0.5 ms timer" con warning de "Solo si Win11, puede reducir autonomía en laptop". Aplicar `GlobalTimerResolutionRequests=1` + instalar SetTimerResolutionService como boot-start service. No usar ISLC salvo que el usuario tenga >32 GB RAM y stutter documentado por standby list growth.

### 1.2 ISLC (Standby List Cleaner) — ¿mito o real?

**Qué hace.** Llama `SetSystemFileCacheSize` / `SetSystemFileCacheSize` / `EmptyWorkingSet` para forzar al MM a descartar standby list cuando pasa un umbral (ej. 1024 MB). Idea: evitar que el MM tenga que "evict-under-pressure" cuando un juego pide RAM de golpe.

**Realidad en 2025.**
- En sistemas con 16+ GB, el MM de Win11 evict mucho mejor que Win7/8. La standby list no es "memoria perdida", sirve como cache.
- Vaciarla agresivamente **aumenta cache misses** en juegos con streaming de assets (Cyberpunk, Star Citizen).
- Útil **solo** si tu sistema con <16 GB presenta stutter periódico que correlacionas con standby list >50% del RAM — medible con RamMap.

**Recomendación.** No habilitar por default. Exponer como feature avanzada con warning.

### 1.3 Win32PrioritySeparation

Registry:
```
HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl
Win32PrioritySeparation = DWORD
```

Bits (6 bits): `PS PS FB FB GB GB`
- **PS** (Process Scheduler): quantum short/long
- **FB** (Foreground Boost): none/medium/high
- **GB** (Quantum): fixed/variable

Valores comunes (hex → decimal):
- `0x02` = 2 (default Win11) — short/variable/none
- `0x26` = 38 — short, variable, high foreground boost
- `0x2A` = 42 — short, fixed, high foreground boost
- `0x29` = 41 — short, fixed, medium foreground boost

**Evidencia.**
- 0x26 / 0x2A son los más recomendados en foros (Blur Busters, r/optimizedgaming).
- Benchmarks del canal "I Tested All Win32PrioritySeparation Values" (YouTube) y threads en Ten/Eleven Forums: **diferencia medible en 0.1% lows cuando hay >5 procesos background activos**. En sistema limpio, dentro del margen de error.
- Se aplica en runtime sin reboot (confirmado por WinDBG debugging en `djdallmann/GamingPCSetup`).

**Veredicto app.** Default `0x26`. Toggle "agresivo" = `0x2A`. No conflictúa con anticheats (es registry OS, no driver).

### 1.4 CPU Core Parking

- **Core parking** = Windows apaga cores lógicos para ahorrar energía. Latencia al despertar uno: 100-300 µs, suficiente para romper frametime.
- Ya **deshabilitado** en Ultimate Performance / High Performance plans de Win11 (CPMinCores=100 por default).
- Tool: **Bitsum ParkControl** GUI, o vía `powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100`.

**Veredicto app.** Forzar CPMinCores=100 + CPMaxCores=100 en el power plan activo al entrar en "Game Mode".

### 1.5 Interrupt Affinity

Tool: **Microsoft Interrupt Affinity Policy Tool** (`IntPolicy.exe`, free en TechPowerUp).

Uso típico: forzar que GPU DPCs corran en core 0-1, audio en core 2, reservar core 3+ para el juego.

**Cuidado:**
- **NO tocar NIC**: RSS (Receive Side Scaling) ya distribuye óptimamente.
- Sí puede ayudar en GPU si LatencyMon muestra `nvlddmkm.sys` / `amdkmdag.sys` altos.
- Sí en **audio** (Realtek HD Audio saturando un core).

**Regla práctica.** Fijar GPU a un CCX completo en Ryzen (core 0-5 en un 5800X3D p.ej.). Core 0 solo para ISRs/DPCs del sistema. Juego en cores 4+.

### 1.6 Process Lasso (Bitsum ProBalance)

- **ProBalance** baja priority class (no afinidad) de procesos background cuando uno foreground se satura. Muy útil en sistemas con Chrome + 50 tabs + game.
- **CPU Sets API** — más moderno que afinidad rígida; permite "soft affinity" que Win11 respeta sin bloquear migración entre cores.
- Compatible con anticheats: ProBalance **no** inyecta en procesos protegidos. Vanguard/EAC/BE no lo marcan porque solo llama `SetPriorityClass`/`SetProcessAffinityMask` desde user-mode.

**Veredicto.** Incluir botón opcional "Instalar Process Lasso" en la app (o integrar su lógica sencilla vía `SetPriorityClass`).

---

## 2. BCDEdit completo

BCDEdit toca el BCD store (Boot Configuration Data). **Siempre hacer backup:**
```
bcdedit /export C:\bcd-backup.bcd
# revert: bcdedit /import C:\bcd-backup.bcd
```

### 2.1 `disabledynamictick`

```
bcdedit /set disabledynamictick yes   # disable
bcdedit /deletevalue disabledynamictick  # revert
```

**Qué hace.** Dynamic tick = Windows deja de enviar interrupts periódicos al timer cuando un core está idle (power-saving). Desactivarlo fuerza un tick constante = **latencia más consistente** a costa de consumo.

**Win11.** Microsoft **recomienda dejarlo ON** (default) en laptops. En desktops competitivos hay consenso tibio de que `yes` reduce micro-jitter en frametime. No es un "free win": benchmarks de Blur Busters y guru3D muestran diferencia <1 ms en frametime variance.

**Riesgo.** Consumo eléctrico mayor (5-15 W extra en idle). Ningún BSOD conocido.

**Anticheat.** No detectado.

### 2.2 `useplatformclock`

```
bcdedit /set useplatformclock true   # forzar HPET como perf counter
bcdedit /set useplatformclock false  # forzar no-HPET
bcdedit /deletevalue useplatformclock  # default (recomendado)
```

**Qué hace.** Fuerza el *query performance counter* (QPC) a usar HPET en vez de TSC.

**Controversial.** CPUs modernas tienen **TSC invariante** (= no cambia con turbo/C-states), es más rápido y preciso que HPET. Forzar HPET introduce overhead y **stutter documentado** en varios threads (Overclock.net, Blur Busters).

**Consenso Win11.** **NO tocar.** Dejar default (que es "TSC si invariante, HPET solo fallback"). Si viene "true" por tweaks viejos, ejecutar `bcdedit /deletevalue useplatformclock` + `bcdedit /set useplatformtick no`.

**Riesgo.** FPS drops reportados de 165 → 100 con fluctuación en juegos comp. Input lag notable.

### 2.3 `useplatformtick`

```
bcdedit /set useplatformtick yes   # tick desde HPET
bcdedit /set useplatformtick no    # tick desde TSC (recomendado)
```

**Qué hace.** Similar al anterior pero para el *tick* del kernel (no el perf counter).

**Win11.** Microsoft: "Windows 11 uses RTC-tick by default, you don't need any bcdedit settings." Dejar en default / `no`.

### 2.4 `tscsyncpolicy`

```
bcdedit /set tscsyncpolicy Enhanced   # sync estricto TSC entre cores
bcdedit /set tscsyncpolicy Default    # default
bcdedit /set tscsyncpolicy Legacy     # legacy
```

**Qué hace.** Controla cómo Windows sincroniza TSC entre cores al boot. `Enhanced` hace más esfuerzo de nivelación (útil en multi-socket). En desktop 1-socket no hay diferencia medible.

**Veredicto.** No tocar en consumer. Relevante solo en workstation Xeon/EPYC multi-socket.

### 2.5 `usefirmwarepcisettings`

```
bcdedit /set usefirmwarepcisettings yes   # respetar firmware PCI
```

**Qué hace.** Usa asignaciones PCI del firmware UEFI en vez de reasignar en OS. A veces soluciona problemas de interrupt sharing. Innecesario en sistemas modernos, potencialmente causa issues con hotplug.

### 2.6 `bootmenupolicy`

```
bcdedit /set bootmenupolicy Legacy   # habilita F8 al boot
bcdedit /set bootmenupolicy Standard # default (menú grafico moderno)
```

**Qué hace.** `Legacy` habilita F8 tradicional (Safe Mode, Last Known Good). **No afecta performance**, solo accesibilidad de recovery. Recomendable habilitar siempre en sistemas de gaming por si un driver tweaked rompe el boot.

### 2.7 Comandos que NO recomendar

- `bcdedit /set increaseuserva 3072` — legacy de 32-bit, inútil en x64.
- `bcdedit /set nx AlwaysOff` — **Nunca**. Desactiva DEP = vulnerabilidad enorme.
- `bcdedit /set testsigning on` — solo para dev. Rompe algunos anticheats.

### 2.8 Veredicto BCDEdit para la app

**Aplicar siempre (seguros):**
- `bootmenupolicy Legacy` (recovery util)

**Aplicar en preset "Competitivo":**
- `disabledynamictick yes` (solo desktops, toggle off en laptops)

**NO aplicar / revertir si están puestos:**
- `useplatformclock true` — deletevalue
- `useplatformtick yes` — set no

---

## 3. Power plans

### 3.1 Comparativa

| Plan | Origen | Core parking | Min CPU % | Boost mode | Uso |
|---|---|---|---|---|---|
| **Balanced** | Win11 default | enabled | 5% | Enabled | Daily driver |
| **High Performance** | Win11 | disabled | 100% | Enabled | Gaming básico |
| **Ultimate Performance** | Win11 (hidden) | disabled | 100% | Aggressive | Workstations / E-cores managed |
| **Bitsum Highest Performance** | Bitsum/ParkControl | disabled | 100% | Aggressive | Latencia mínima |

### 3.2 Activar Ultimate Performance (oculto por default en desktops)

```
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61
```

GUID `e9a42b02-d5df-448d-aa00-03f14749eb61` es UP. En laptops suele estar más escondido; este comando lo materializa y sale en `powercfg -list`.

### 3.3 Bitsum Highest Performance

- Se instala al instalar ParkControl o Process Lasso.
- Alternativa: descargar el `.pow` directamente del repo `DaddyMadu/Windows10GamingFocus` (GitHub) y `powercfg -import Bitsum-Highest-Performance.pow`.
- Diferencia vs Ultimate: Bitsum fue calibrado para **Intel Speed Shift** y deja CPU siempre en base freq mínimo, no deja que baje a idle freq. Bitsum reporta menor variabilidad en gaming bursts. No hay benchmark académico comparativo; Tom's Hardware Forum, r/overclocking: diferencia marginal, subjetiva.

### 3.4 Settings críticos DENTRO del plan (via powercfg)

```
# Min y max CPU
powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100
powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX 100

# Core parking OFF
powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100
powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100

# Performance boost mode = Aggressive (2)
powercfg -setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PERFBOOSTMODE 2

# PCI Express Link State Power Management = Off (mejor latencia, más consumo)
powercfg -setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 0

# USB selective suspend = Disabled
powercfg -setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0

# Hard disk turn off = Never
powercfg -setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE 0

# Apply
powercfg -setactive SCHEME_CURRENT
```

### 3.5 Unhide performance boost mode

```
powercfg -attributes SUB_PROCESSOR 54533251-82be-4824-96c1-47b60b740d00 -ATTRIB_HIDE
```

Valores de `PERFBOOSTMODE`:
- 0 = Disabled
- 1 = Enabled
- 2 = Aggressive (default HP/UP plans)
- 3 = Efficient Enabled
- 4 = Efficient Aggressive
- 5 = Aggressive at Guaranteed
- 6 = Efficient Aggressive at Guaranteed

**Gaming desktop:** 2 (Aggressive).
**Laptop thermals-bound:** 3 o 4.
**Laptops delgados con throttle térmico crónico:** 0 + undervolt via ThrottleStop puede dar más FPS sostenido que Aggressive con throttling.

### 3.6 .pow files conocidos descargables

- `DaddyMadu/Windows10GamingFocus` (GitHub) — Bitsum Highest Performance y variantes
- `simp4sims` (Patreon) — Reduce stutters plan
- `BoringBoredom/PC-Optimization-Hub` — referencias a varios .pow curados
- `bitsum.com/bhp/` — Bitsum Highest Performance oficial

### 3.7 Veredicto app

- Default **Ultimate Performance** (auto-creado si no existe).
- Opción "usar Bitsum Highest Performance" con botón de importación automática del .pow.
- Preset "Balanced Gaming" para laptops que respeta battery.

---

## 4. Memory management

### 4.1 Registro: `Memory Management`

Path: `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management`

#### `LargeSystemCache`

- **Default Win11: 0**
- Cambio a 1: dedica más RAM a file cache vs working set de apps.
- **Microsoft la marca como deprecated** — en Win11 modernos no tiene efecto o es mínimo.
- Comunidad gaming dividida: algunas guías la setean en 1, otras en 0.
- **Recomendación.** Dejar en 0. No hay evidencia de beneficio en gaming consumer.

#### `DisablePagingExecutive`

- **Default: 0**
- Valor 1: impide que el kernel page-out drivers/kernel a disk.
- **Microsoft docs explícito**: "only intended for debugging device drivers, not for end users."
- Con 16+ GB RAM, el kernel raramente se pagea de todos modos.
- **Recomendación.** Setear en 1 si se tiene 32+ GB, no mueve frametime medible pero reduce eventual page-in stutters. Inofensivo.

#### `ClearPageFileAtShutdown`

- **Default: 0**
- Valor 1: zero el pagefile al shutdown (seguridad vs performance en shutdown).
- Alarga shutdown 30-90 s. Sin impacto en running performance.
- **Recomendación.** Dejar en 0.

#### `NonPagedPoolQuota`, `PagedPoolSize`

- Dejar en 0 (auto). Microsoft maneja estos dinámicamente en Win11 mucho mejor que Win7.
- Tocarlos a valores fijos puede limitar drivers que pidan chunks grandes (antivirus, anticheats).

### 4.2 Pagefile

**Recomendaciones modernas (consenso r/hardware, Microsoft docs):**

- **Dejar system-managed** en la mayoría de casos. Win11 es bueno dimensionándolo.
- Si RAM >= 32 GB: se puede fijar 4096-8192 MB (mínimo y máximo iguales para evitar fragmentación).
- **Mover a SSD secundario** si existe: sí, reduce IO contention cuando se pagea. No mover a HDD nunca.
- **Nunca deshabilitar** completamente: apps asumen pagefile disponible; algunas crashean sin él (Photoshop, engines que hacen `VirtualAllocEx` grande).

### 4.3 Hibernation

```
powercfg -h off    # disable, libera hiberfil.sys (40% de RAM en GB)
powercfg -h on     # re-enable
```

**Side effect crítico.** `powercfg -h off` **deshabilita Fast Startup** automáticamente (Fast Startup usa hiberfil.sys).

**Veredicto.** Desktops sin laptop-style hibernación: `off` libera 8-32 GB en C:\. En laptops que se usan con tapa cerrada: dejar `on`. Offer como toggle en la app: "Disable hibernation (libera ~X GB)".

### 4.4 SysMain (ex-Superfetch)

- Servicio que precarga apps frecuentes a standby list.
- **Microsoft Win11**: ya detecta SSD y ajusta comportamiento (no precarga tan agresivamente).
- Disable: `sc config SysMain start=disabled` + `net stop SysMain`.
- **Evidencia**: en sistemas NVMe + 16 GB+ no mejora gaming. En laptops con HDD 5400 rpm sí mejora tiempos de carga de apps.
- **Recomendación app.** No tocarlo por default. Toggle "Disable SysMain" como avanzado. Especialmente **no** en laptops con eMMC o SATA SSD donde precarga sí ayuda.

### 4.5 Memory Compression

PowerShell (admin):
```
Get-MMAgent                 # check state
Disable-MMAgent -mc         # disable
Enable-MMAgent -mc          # enable (default)
```

**Recomendación.** Dejar **habilitado**. Win11 lo tiene ON por default, reduce I/O a pagefile, impacto CPU despreciable (<1%). Desactivar solo tiene sentido con RAM >= 64 GB y usos muy específicos.

### 4.6 ReadyBoost

Obsoleto en sistemas con SSD. Windows lo deshabilita automáticamente si ve SSD. No hay nada que tocar.

---

## 5. DPC Latency (tools + workflow)

### 5.1 Qué es

DPC = Deferred Procedure Call, mecanismo del kernel para diferir trabajo de un ISR. Alta latencia DPC = micro-stutters, audio pops, click de ratón retrasado.

### 5.2 Herramientas

| Tool | Uso |
|---|---|
| **LatencyMon** (Resplendence, free) | Mide ISR + DPC + driver culprit. |
| **DPC Latency Checker** (Thesycon) | Versión vieja, no confiable en Win10/11. Evitar. |
| **xperf / WPR / WPA** (Windows Performance Toolkit) | Profiling profesional. |
| **xtw.exe** (djdallmann) | xperf wrapper para workflow repetible. |

### 5.3 Workflow LatencyMon

1. Instalar LatencyMon. Ejecutar como admin.
2. Dejar correr **10-15 min** en idle + 10 min bajo carga (navegar, reproducir video).
3. Thresholds (Resplendence docs oficiales):
   - **Verde / <1000 µs**: sistema sano.
   - **Amarillo 1000-2000 µs**: dudoso, algún driver requiere atención.
   - **Naranja 2000-4000 µs**: problema real. Impacto en gaming audible.
   - **Rojo >4000 µs**: sistema no apto para real-time.
4. Ver tab "Drivers" — ordenar por "Highest DPC execution". Culpables típicos:
   - `nvlddmkm.sys` (NVIDIA) → actualizar driver, habilitar MSI mode via MSIUtil v3
   - `amdkmdag.sys` (AMD) → mismo workflow
   - `ndis.sys` + `tcpip.sys` → NIC driver malo. Realtek integrado es clásico ofensor. Update driver fabricante (no Windows Update), o instalar `Realtek Diagnostic` / alternativa Intel/Killer firmware.
   - `usbport.sys` / `usbxhci.sys` → controller USB saturado. Revisar devices USB conectados (DAC, webcams).
   - `HDAudBus.sys` / `RTKVHDA64.sys` (Realtek audio) → tweaks específicos Realtek (disable enhancements + update driver UAD).
   - `dxgkrnl.sys` → GPU scheduler. Toggle HAGS (Hardware Accelerated GPU Scheduling) on/off y medir.
5. Después de cada cambio, **relanzar LatencyMon y medir 10 min** — no basar en "feeling".

### 5.4 MSI Mode (Message Signaled Interrupts)

**Qué es.** Línea-base (legacy): un IRQ compartido por varios devices. MSI: cada device pide interrupt via write a memoria, sin compartir. Resultado: menos contention, menor DPC.

**Tool**: **MSIUtil v3** (Sathango fork, GitHub `Sathango/Msi-Utility-v3`). Versión anterior: MSI_util v2 de guru3D.

**Workflow:**
1. Admin run.
2. Listar devices. Ver columna "Interrupt Method": Line-based / Message Signaled.
3. Habilitar MSI en:
   - **GPU** (NVIDIA/AMD) — mayor ganancia, clásico
   - **NVMe controller** — puede ayudar, pero muchos ya vienen en MSI-X
   - **USB controllers xHCI** — útil con DACs USB
   - **NIC** (si no está ya) — Killer/Intel ya están. Realtek a veces no.
4. **Priority** alta para GPU y NIC (pestaña Interrupt Priority en MSIUtil v3).
5. **Reboot obligatorio.**
6. Medir con LatencyMon pre/post.

**Evidencia.** Guru3D thread "Line-Based vs. Message Signaled-Based Interrupts": reducciones de DPC de milliseconds a microseconds en muchos setups. RME Audio Forum también lo confirma para audio pro.

**Riesgo.** En raros casos un driver antiguo no soporta MSI bien y genera BSOD. Siempre anotar qué se cambió — MSIUtil v3 permite revertir.

**Anticheat.** MSIUtil **no** modifica drivers ni inyecta; cambia un registry key que el driver lee al cargar. No detectado por BE/EAC/Vanguard/FACEIT.

### 5.5 Interrupt Affinity

Ver §1.5. Usar **solo después** de medir con LatencyMon que hay un driver concreto saturando un core.

### 5.6 Network adapter tweaks

Windows en Device Manager → NIC → Advanced tab. Settings típicos:
- **Interrupt Moderation** — default Enabled. Desactivarlo reduce latencia a costa de CPU. En comp FPS online con <1ms ping local puede ayudar. Desactivar en sistemas con >=6 cores.
- **Receive Side Scaling (RSS)** — dejar Enabled.
- **Flow Control** — Disabled (gaming). Enabled es para servidores.
- **Jumbo Packet** — Disabled (los juegos no usan jumbo).
- **Energy Efficient Ethernet / Green Ethernet** — Disabled (añade latencia al despertar el link).

Realtek-specific: actualizar firmware desde sitio de Realtek (no Windows Update). Pack comunitario "Realtek RTL8168/8111 Diagnostic" mejora sustancialmente DPC.

---

## 6. Spectre/Meltdown mitigations

### 6.1 Context

Microcode + OS patches desde 2018 agregan overhead a syscalls y context switches. Magnitud del hit varía según:
- **CPU**: Intel pre-10th gen muy afectado (~10-20% en ciertos workloads). Ryzen 3000+ / Intel 11th gen+ tienen mitigations en hardware (retpoline, IBRS-in-silicon) = impacto <5%.
- **Workload**: gaming es bajo-syscall-rate comparado con compilación, DBs, etc. **Impacto gaming típico: 2-5%** (Phoronix benchmarks, AMD Zen 2 review).

### 6.2 Cómo deshabilitar

⚠️ **ADVERTENCIA DE SEGURIDAD FUERTE** ⚠️

Spectre v2 (BranchScope, CVE-2017-5715) y Downfall (CVE-2022-40982) son **activamente explotables en JavaScript del navegador** para robar memoria de otros procesos. Deshabilitar mitigations:
- Permite que páginas web lean memoria de tu navegador (cookies, passwords, tokens).
- Permite que malware escape sandboxes (Docker, Hyper-V).
- NO hay "solo juegos": una vez desactivado, el SO entero es vulnerable.

Registry:
```
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverride /t REG_DWORD /d 3 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverrideMask /t REG_DWORD /d 3 /f
```

Valor `3` = bit0 (deshabilita Meltdown/KPTI) + bit1 (deshabilita Spectre v2). Para incluir MDS/Zombieload/Downfall se usan valores mayores (`72` = 0x48) — no documentado oficialmente, trial-and-error comunidad.

Revert:
```
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverride /f
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverrideMask /f
```

Reboot obligatorio.

### 6.3 Verificar estado

- **InSpectre** (Gibson Research Corporation, grc.com/inspectre.htm) — GUI simple.
- **Get-SpeculationControlSettings** (PowerShell module SpeculationControl, ver Microsoft docs).

### 6.4 Veredicto app

- **NO habilitar por default.** Nunca.
- Exponer **solo** en preset "Extremo" con **consent modal de dos pantallas**:
  1. "Esto desactiva mitigaciones contra Spectre/Meltdown. Tu navegador, sandboxes y VMs quedan vulnerables a lectura de memoria por código hostil. ¿Entiendes?"
  2. "En Ryzen 3000+ / Intel 11th+ el beneficio es 2-5% FPS. En CPUs viejas 5-15%. ¿Continuar?"
- Si el usuario confirma, aplicar + loggear en app que la feature está activa con timestamp y ofrecer revert prominente.
- **No lo recomendaría activar nunca** en un sistema que se usa para cualquier cosa además de gaming offline single-player.

### 6.5 Intel 13th/14th gen / Ryzen 7000+

Modelos recientes tienen mitigations casi "free" vía silicon. Desactivar aquí es **todo riesgo, cero beneficio medible**.

---

## 7. VBS / HVCI / Core Isolation

### 7.1 Jerarquía

- **VBS (Virtualization-Based Security)**: paraguas. Requiere CPU con virtualization ext + Secure Boot + IOMMU.
- **HVCI (Hypervisor-Protected Code Integrity)** = **Memory Integrity** (nombre UX de Win11): usa VBS para proteger integridad del kernel. Hace que cualquier driver cargado se valide antes de ejecutar en kernel mode.
- **Credential Guard**: protege credenciales en LSA. No afecta gaming.
- **MBEC (Mode-Based Execution Control)**: extensión hardware que reduce el overhead de HVCI dramáticamente. Intel 7th gen+ / AMD Zen 2+.

### 7.2 Impacto en gaming (benchmarks Tom's Hardware, octubre 2021)

| CPU | VBS solo | HVCI |
|---|---|---|
| Intel i7-11700K | -4.9% | -5.6% |
| Intel i7-10700K | -5.7% | -5.7% |
| AMD Ryzen 7 5800X | -4.0% | -3.3% |
| AMD Ryzen 7 3800X | -4.1% | -4.1% |

Peaks por juego: Project Cars 3 -8.1%, RDR2 DX12 -7.3%, Shadow of the Tomb Raider DX12 -7.2%. GTA V apenas -1%.

En **CPUs sin MBEC** (Intel <=6th gen, Ryzen 1000) el hit sería sustancialmente peor — pero esas CPUs no soportan Win11 oficialmente.

### 7.3 Deshabilitar

GUI: Settings → Privacy & security → Windows Security → Device security → Core isolation → Memory integrity = Off.

Registry:
```
reg add "HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity" /v "Enabled" /t REG_DWORD /d 0 /f

# deshabilitar VBS entero:
reg add "HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard" /v "EnableVirtualizationBasedSecurity" /t REG_DWORD /d 0 /f
```

Reboot.

Para sistemas con Credential Guard:
```
# requiere DG_Readiness tool de Microsoft para uninstall properly
```

### 7.4 ⚠️ Advertencia crítica

**ANTICHEATS CADA VEZ MÁS LO REQUIEREN:**

- **Vanguard (Valorant)**: desde **julio 2024** exige HVCI = ON. Sin él, error VAN 9005.
- **FACEIT AC**: desde **abril 2025** exige IOMMU + VBS en ~60% de jugadores. Desde **agosto 2025** obligatorio para >3000 ELO. **Desde 25 de noviembre 2025 TPM 2.0 + Secure Boot obligatorios para todos.**
- **BattlEye**: aún no exige HVCI pero pone warnings en juegos comp.
- **EAC**: misma situación que BattlEye.

**Esto significa:** si el usuario desactiva HVCI/VBS para ganar 5% FPS, **no puede jugar Valorant ni FACEIT competitive**. App debe detectar juegos instalados y advertir.

### 7.5 Veredicto app

- **Desktops con CPU 12th gen+/Ryzen 5000+ y MBEC**: HVCI hit es 3-5%. Dejar ON. No vale la pena.
- **CPUs 8th-11th gen Intel / Ryzen 3000-4000**: hit 5-7%. Toggle con warning de anticheats.
- **CPUs viejas forzadas a Win11 por bypass** (TPM bypass instalations): HVCI puede ser 10%+. Usuario típicamente lo quiere off.
- **Preset Competitivo** debe **forzar HVCI ON** y advertir que así es compatible con Valorant/FACEIT.

### 7.6 VBS detect

PowerShell:
```
Get-CimInstance -ClassName Win32_DeviceGuard -Namespace root\Microsoft\Windows\DeviceGuard
# VirtualizationBasedSecurityStatus: 2 = Running
# SecurityServicesConfigured: array, 2 = HVCI configured
# SecurityServicesRunning: array, 2 = HVCI running
```

---

## 8. File system / Storage

### 8.1 NTFS behavior (fsutil)

```
# disable last-access time (reduce disk writes en every file access)
fsutil behavior set disablelastaccess 1

# disable 8.3 short filenames (dupe entries innecesarias)
fsutil behavior set disable8dot3 1

# no encriptar pagefile (más rápido)
fsutil behavior set encryptpagingfile 0

# check current
fsutil behavior query disablelastaccess
fsutil behavior query disable8dot3
fsutil behavior query encryptpagingfile
```

**Microsoft docs**:
- `disablelastaccess`: "improves the speed of file and directory access". Default en Win10 1803+ es `System Managed` que suele ser disabled. Valor `1` = disabled system-wide.
- `disable8dot3`: 8.3 names son para apps 16-bit legacy. Ninguna razón para mantener en Win11.

**Impacto gaming:** marginal pero 100% seguro. Sin anticheat flags.

### 8.2 TRIM verification

```
fsutil behavior query DisableDeleteNotify
# output 0 = TRIM activo (correcto)
# output 1 = TRIM deshabilitado (malo en SSD)

# forzar activar:
fsutil behavior set DisableDeleteNotify NTFS 0
```

### 8.3 Storage Sense / Indexing

- **Storage Sense**: limpieza automática de temp/recycle. Inofensiva, dejar default.
- **Windows Search**: indexing.
  - Pros: búsqueda de archivos rápida en File Explorer/Start.
  - Cons: lee disco continuamente. `searchindexer.exe` es un clásico ofensor en LatencyMon en sistemas con muchos archivos.
  - Recomendación: **excluir** carpetas de juegos (Steam, Epic, Ubisoft) en Indexing Options. Desactivar completamente (`sc config WSearch start=disabled`) puede ser over-kill, pero en gaming puro sin búsqueda frecuente: viable.

### 8.4 Prefetch

- Similar a SysMain, Win11 auto-detecta SSD. No tocar.
- No confundir con Steam shader pre-caching — eso sí ayuda, dejar ON.

---

## 9. Boot / Fast Startup / Hibernate

### 9.1 Fast Startup (Hybrid Shutdown)

**Qué hace.** Al "shutdown" en realidad hiberna kernel + drivers a hiberfil.sys. Al "boot" lee ese snapshot. **NO es un reboot real.**

**Problemas documentados:**
- Drivers no reciben initialization sequence limpia: Wi-Fi, GPU, audio, USB controllers pueden quedar en estado inconsistente.
- Updates de Windows a veces no aplican correctamente.
- **Dual-boot**: puede corromper filesystems de otros OS porque NTFS queda "dirty".
- Después de tweaks registry/drivers, el snapshot puede contener estado viejo — el tweak no surte efecto hasta que se hace "Restart" (que sí es cold boot).

**Para gaming tweaking: deshabilitar** obligatorio. Si no, aplicar tweaks → Shutdown → Power on → tweaks no aplicados → confusión.

```
# GUI
Control Panel → Power Options → Choose what the power buttons do → Change settings currently unavailable → uncheck "Turn on fast startup"

# registry equivalente
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power" /v HiberbootEnabled /t REG_DWORD /d 0 /f

# o al desactivar hibernation, Fast Startup cae solo:
powercfg -h off
```

### 9.2 Startup apps

Task Manager → Startup apps. Eliminar:
- Launchers (Steam, Epic, GOG) que arrancan con Windows → deshabilitar, iniciar manual.
- OEM bloatware (Asus Armoury Crate, MSI Center) — considerar si se usan features, sino quitar.
- RGB software: muchos son ofensores graves en DPC. Si no se usa, quitar.

### 9.3 Services startup audit

Tool: **autoruns.exe** (Sysinternals) o `services.msc`.

Servicios que se pueden deshabilitar en gaming (con cuidado):
- `Diagnostic Policy Service` → no, necesario para varios drivers
- `Connected User Experiences and Telemetry` (`DiagTrack`) → sí, si no usas Feedback Hub
- `Windows Search` (`WSearch`) → opcional, ver §8.3
- `Print Spooler` → sí si no imprimes
- `Fax` → sí
- `Windows Mobile Hotspot` (`icssvc`) → sí si no usas hotspot

**Nunca deshabilitar:**
- `Windows Update` — sí lo puedes pausar, no deshabilitar (rompe actualizaciones de Defender + firmas anticheat)
- `Windows Defender` services (hay 4-5) — si no tienes AV tercero, dejarlos
- Cualquier `*AntiCheat*` service.

---

## 10. Defender + Misceláneos

### 10.1 Windows Defender exclusions

**Microsoft tiene feature oficial: "Performance Mode" para Dev Drive** (Win11). No está disponible para gaming per-se aún.

Para gaming:
```powershell
Add-MpPreference -ExclusionPath "C:\Program Files (x86)\Steam"
Add-MpPreference -ExclusionPath "C:\Program Files\Epic Games"
Add-MpPreference -ExclusionPath "D:\SteamLibrary"   # si tienes librerías extra
Add-MpPreference -ExclusionProcess "steam.exe"
```

**Cuidado**: los anticheats escanean exclusions. Si excluyes C:\ entero o carpetas críticas del sistema, algunos (Vanguard sobre todo) lo detectan como cheating attempt.

**Solo excluir**:
- Carpetas de install de juegos (Steam library, Epic, Battle.net).
- Carpetas de datos de juego (Documents\My Games, AppData\Local\*).
- No excluir System32, Program Files completo, AppData\Roaming entero.

### 10.2 Game Mode

Settings → Gaming → Game Mode = On. Inofensivo. Windows simplemente limita Windows Update/driver installs durante gaming. Dejar ON.

### 10.3 Visual effects (Performance Options)

`SystemPropertiesAdvanced` → Performance Settings. "Adjust for best performance" desactiva animaciones. Micro-impacto pero free. Se puede mantener custom: animations off, ClearType on, smooth edges for fonts on.

### 10.4 Crash dump settings

- Default: Automatic memory dump (full dump si MEMORY.DMP no supera pagefile).
- Gaming dedicated: **Small memory dump** (256 KB) o **None**. Ahorra GBs + tiempo de write si hay BSOD. Trade-off: si BSOD, menos info para debug.
- `Control Panel → System → Advanced → Startup and Recovery → Write debugging information`.

### 10.5 Background apps

Settings → Apps → Installed apps → click on each → Advanced options → Background apps permissions = Never. Para apps UWP que corren en background sin razón.

### 10.6 Notifications during gaming

Settings → System → Notifications → Focus assist / Priority notifications. Set to "Alarms only" durante gaming. Win11 tiene "Do not disturb" auto-enable cuando detecta fullscreen.

---

## 11. Anti-cheat compatibility matrix

Fuentes: BattlEye FAQ oficial, FACEIT Support KB, Riot Vanguard support, arXiv "Critical Examination of Kernel-Level Anti-Cheat Systems" (2024).

| Tweak | BattlEye | EAC | Vanguard | FACEIT AC |
|---|---|---|---|---|
| HVCI / Memory Integrity = ON | OK (preferred) | OK | **REQUERIDO** (VAN 9005 si off, desde Jul 2024) | **REQUERIDO** (staged rollout) |
| VBS = ON | OK | OK | REQUERIDO | **REQUERIDO** desde nov 2025 |
| Secure Boot = ON | OK | OK | REQUERIDO | **REQUERIDO** desde nov 2025 |
| TPM 2.0 enabled | OK | OK | REQUERIDO | **REQUERIDO** desde nov 2025 |
| UEFI (no Legacy/CSM) | OK | OK | REQUERIDO | REQUERIDO |
| IOMMU = ON | OK | OK | REQUERIDO | REQUERIDO (staged) |
| Spectre mitigations disabled | OK (flagged si detecta cheat patterns) | OK | OK pero **monitoreado** | OK |
| Test signing mode ON | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** |
| Kernel debugger attached | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** |
| MSIUtil v3 tweaks (MSI mode) | OK | OK | OK | OK |
| Interrupt Affinity Policy Tool | OK | OK | OK | OK |
| BCDEdit tweaks (dynamictick etc) | OK | OK | OK | OK |
| Registry Memory Management tweaks | OK | OK | OK | OK |
| Win32PrioritySeparation | OK | OK | OK | OK |
| fsutil disablelastaccess | OK | OK | OK | OK |
| Process Lasso / ProBalance | OK | OK | OK | OK |
| Windows Defender disabled/tampered | Warning si sin AV tercero | Warning | Warning | **BLOQUEADO** |
| Hypervisor (VMware/VirtualBox activo) | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** (Vanguard boot-start conflict) | **BLOQUEADO** |
| Hyper-V habilitado (no VM corriendo) | OK (en su mayoría) | OK | Conflicto histórico resuelto | OK |
| Vulnerable drivers (old RivaTuner, old MSI Afterburner, old Aida64, EVGA Precision X1, ASUS AI Suite old) | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** | **BLOQUEADO** |
| Overclocking tools (OCCT, CoreTemp, HWiNFO) | OK | OK | OK recientemente | OK |
| ThrottleStop | OK | OK | OK | OK (recent versions) |

### Puntos clave

- **Vanguard** es el más estricto: boot-start driver, verifica todo el boot chain vía TPM attestation. Su lista de drivers bloqueados es pública-ish (ver `not-matthias.github.io`).
- **FACEIT AC** desde finales 2025 es casi idéntico a Vanguard en requisitos.
- **BattlEye y EAC** más permisivos pero hacen scanning continuo de integridad de sus propios procesos. Cualquier tool que inyecte en el proceso del juego → ban.
- Tweaks **puramente registry/kernel config** (todo lo de este doc excepto Spectre disable y drivers con exploit) son **seguros con todos**.

### Drivers problemáticos conocidos (Vanguard bloquea)

- MSI Afterburner <= 4.6.4 (vieja versión con RTCore64.sys vulnerable) — usar 4.6.5+
- Old ASUS drivers con AsrDrv101.sys
- Intel Extreme Tuning Utility old versions
- EVGA Precision X1 old
- AIDA64 old versions (kerneld.xxx)

**Recomendación app.** Detectar presencia de estos drivers vulnerables y avisar al usuario. No dar "optimize" si están instalados; primero update.

---

## 12. Presets: Extremo / Seguro / Competitivo

### Preset "Seguro" (recomendado default)

Tweaks con evidencia de beneficio o neutrales, sin breaking changes:

**Power:**
- Ultimate Performance plan activo
- Core parking OFF, CPMinCores=CPMaxCores=100
- PERFBOOSTMODE=2 Aggressive
- USB selective suspend=Off
- PCIe ASPM=Off
- Hard disk idle=Never

**Boot/Memory:**
- `powercfg -h off` (toggle, default ON; off recupera espacio)
- Fast Startup OFF
- Memory compression ON (default)
- Pagefile system-managed (no tocar)

**Filesystem:**
- `fsutil behavior set disablelastaccess 1`
- `fsutil behavior set disable8dot3 1`
- Indexing: excluir carpetas de juegos

**Network:**
- NIC advanced: Flow Control Off, Jumbo Off, EEE Off
- NIC RSS On
- Update NIC driver manual (si Realtek)

**Defender:**
- Exclusions de Steam/Epic/Battle.net folders
- Game Mode ON

**Misc:**
- Startup apps cleanup (no bloatware)
- Visual effects: custom sin animaciones
- Crash dump: Small memory dump

**No incluye:**
- BCDEdit tweaks (default Win11)
- Spectre mitigations disable
- HVCI disable
- Win32PrioritySeparation cambio (mantener 0x02 default)
- Timer resolution global

**Anticheat compat:** 100% con todos.

### Preset "Competitivo" (FPS/latencia, preserva anticheat compat)

Lo del "Seguro" más:

- **Timer Resolution**: `GlobalTimerResolutionRequests=1` + SetTimerResolutionService a 0.5 ms
- **Win32PrioritySeparation** = `0x26` (38 dec)
- **BCDEdit**: `disabledynamictick yes` (solo desktops), `bootmenupolicy Legacy`
- **MSI Mode**: habilitar en GPU + audio controller vía MSIUtil v3
- **Interrupt Affinity**: GPU fijado a cores 0-1 (si LatencyMon muestra beneficio; sino no tocar)
- **NIC Interrupt Moderation**: Off (en CPUs 6+ cores)
- **HVCI**: **MANTENER ON** (requerido por Vanguard/FACEIT)
- **VBS**: **MANTENER ON**
- **TPM + Secure Boot**: ON
- **Registry Memory Management**: `DisablePagingExecutive=1` (solo >=32 GB RAM)
- **Windows Search service**: disabled
- **Telemetry** (`DiagTrack`): disabled
- **Services audit** ligero: desactivar Print Spooler si no imprime, Fax, MobileHotspot
- **Bitsum Highest Performance** opcional si el usuario ya tiene Process Lasso

**Anticheat compat:** 100% con todos (incluido Vanguard y FACEIT post-nov-2025).

### Preset "Extremo" (singleplayer / offline / benchmark)

⚠️ Advertencias múltiples. Todo lo del Competitivo, más:

- **HVCI: OFF** (~3-6% FPS ganancia)
- **VBS: OFF**
- **Spectre/Meltdown mitigations: OFF** (`FeatureSettingsOverride=3`) con **warning doble-confirmación**
- **Core Isolation: OFF**
- **SysMain: disabled** (si usuario tiene NVMe + 16GB+)
- **Windows Search: disabled completo**
- **Defender real-time: toggleable via schedule** (off durante sesión gaming, on resto)
- **Windows Update**: delivery optimization off, active hours 24/7, pausar updates
- `useplatformclock` **deleted** si estuviera por error (confirmar estado)
- Process Lasso con ProBalance + rule específico para el juego activo
- **ISLC** habilitado con threshold agresivo (solo si standby list >70% RAM documentado)

**Anticheat compat:**
- BattlEye/EAC: probablemente OK, pero el usuario puede recibir warnings.
- Vanguard: **NO juega Valorant con este preset.**
- FACEIT: **NO juega FACEIT comp con este preset.**

**Rollback obligatorio.** App debe crear un **restore point Windows** antes de aplicar "Extremo" y ofrecer "Revert all" que deshace todo en un click.

### Matrix decisión

| Usuario | Preset |
|---|---|
| Casual gamer, no competitive | Seguro |
| Juega Valorant, FACEIT, Fortnite comp | Competitivo |
| Solo juegos singleplayer, no online comp, benchmark runs | Extremo (con consent completo) |
| Laptop | Seguro con toggle thermal (ThrottleStop opcional) |
| Desktop high-end (5800X3D/13700K+) + RTX 4070+ | Competitivo (el extra del Extremo es <5% y lockea de comp) |

---

## Apéndice A — Fuentes autoritativas consultadas

- **djdallmann/GamingPCSetup** (GitHub): research-based guide. Página `djdallmann.github.io/GamingPCSetup/` para HTML rendering.
- **BoringBoredom/PC-Optimization-Hub** (GitHub): collection referenciando más guides.
- **valleyofdoom/TimerResolution** (GitHub): SetTimerResolution tools + docs sobre `GlobalTimerResolutionRequests`.
- **Blur Busters**: `blurbusters.com/category/inputlag/` y `forums.blurbusters.com`.
- **Resplendence**: `resplendence.com/latencymon_using` y `latencymon_interrupt2process`.
- **Bitsum**: `bitsum.com/bhp/`, `bitsum.com/parkcontrol/`, `bitsum.com/how-probalance-works/`.
- **Microsoft Learn**: BCDEdit, fsutil behavior, Interrupt Affinity, VBS/HVCI, fast startup troubleshooting, powercfg power-performance-tuning.
- **Riot Vanguard Support**: Restrictions, VAN 9005.
- **FACEIT Support**: Windows Security Requirements FAQ, TPM attestation articles.
- **Tom's Hardware**: VBS/HVCI benchmarks (Intel + AMD), ultimate-vs-high-performance comparison thread.
- **Phoronix**: Ryzen 3000/Spectre mitigation benchmarks.
- **arXiv 2408.00500**: "Critical Examination of Kernel-Level Anti-Cheat Systems".
- **GRC InSpectre**: `grc.com/inspectre.htm`.
- **not-matthias.github.io/posts/anticheat-update-tracking/**: driver blocklist intel.

## Apéndice B — Orden de aplicación recomendado

1. **Snapshot / Restore point** (`wmic.exe /Namespace:\\root\default Path SystemRestore Call CreateRestorePoint`).
2. Export BCD, export relevant registry branches.
3. Driver hygiene (NIC, GPU, chipset update from vendor, NOT Windows Update).
4. Power plan import/create.
5. Registry tweaks (Memory Management, PriorityControl, Session Manager).
6. BCDEdit tweaks (con reboot al final).
7. MSIUtil v3 MSI mode tweaks (reboot).
8. Services audit (disable bloat).
9. Defender exclusions.
10. Measure baseline LatencyMon 15 min.
11. Iterate: cada cambio → medir → si empeora, revertir.

## Apéndice C — Comandos de revert consolidados

```cmd
:: Power plan reset to Balanced
powercfg -setactive SCHEME_BALANCED

:: BCDEdit revert
bcdedit /deletevalue disabledynamictick
bcdedit /deletevalue useplatformclock
bcdedit /deletevalue useplatformtick
bcdedit /set bootmenupolicy Standard

:: Spectre re-enable
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverride /f
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverrideMask /f

:: HVCI re-enable
reg add "HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity" /v "Enabled" /t REG_DWORD /d 1 /f

:: Hibernation back
powercfg -h on

:: fsutil back to defaults
fsutil behavior set disablelastaccess 2
fsutil behavior set disable8dot3 2

:: Timer resolution global OFF
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v GlobalTimerResolutionRequests /f
```

## Apéndice D — Quick wins "risk-free" (para mostrar en UI como "aplicables sin riesgo")

1. Ultimate Performance plan + core parking off
2. USB selective suspend off
3. PCIe ASPM off
4. Defender exclusions (game folders)
5. `fsutil disablelastaccess 1`
6. `fsutil disable8dot3 1`
7. Disable hibernation (`powercfg -h off`)
8. Disable Fast Startup
9. Disable SysMain (solo NVMe + 16GB+)
10. Startup apps cleanup
11. NIC advanced settings (Flow Control off, EEE off)
12. Disable Print Spooler service (si no imprime)
13. Indexing exclude game folders
14. Memory compression (ya es default ON)
15. `bootmenupolicy Legacy` (recovery utility)

Estos ~15 tweaks son **100% compatibles anticheat**, **ningún caso de BSOD** documentado, **benefit marginal pero real**. Ideal preset "Un click optimize" sin disclaimer necesario.
