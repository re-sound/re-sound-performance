# Análisis Profundo — Ecosistema de Optimizadores Windows 11 para Gaming (2026)

> Documento entregado como base arquitectónica para una app propia de optimización. Centrado en **análisis técnico**, no simple enumeración. Fuentes primarias: repos de GitHub (API + raw READMEs + archivos fuente), documentación oficial de cada proyecto, reviews técnicos 2025-2026.

---

## 0. Resumen ejecutivo

- El mercado está **saturado** en privacy/debloat pero **escaso** en *gaming real* (latencia DPC, timer resolution, MSI mode, ISR/DPC priority, HPET, MMCSS).
- Solo **WinUtil** (CTT) y **Sophia Script** dominan la escena open-source mainstream "a la WPF". **Optimizer** (hellzerg) fue el rey en C# pero está *archivado/deprecated* → **hueco gigante en la esquina C#/.NET**.
- **Atlas OS / ReviOS** dominan "extreme debloat" pero requieren AME Wizard y tocan ISO/componentes — no son aplicaciones, son *playbooks*. Esto genera una oportunidad: ofrecer el **85% del beneficio de Atlas sin el 50% del riesgo de irreversibilidad**.
- Ninguna herramienta "grande" integra **a la vez** debloat + tweaks de latencia real (MSI Utility, TimerResolution, ProBalance-like) + gestión GPU (NVCleanstall) + undervolt (ThrottleStop). Cada área vive en una app distinta.
- **La oportunidad**: una **suite unificada de optimización gaming con fuerte sistema de revert/snapshot granular, presets por género (FPS competitivo / AAA / streaming) y un motor declarativo de tweaks auditables**.

---

## 1. Análisis por proyecto (Tier 1)

### 1.1 Chris Titus WinUtil — ChrisTitusTech/winutil

- **URL**: https://github.com/ChrisTitusTech/winutil
- **Stack**: PowerShell 5.1 + WPF (XAML) — "compilado" a un solo `.ps1` via `Compile.ps1`
- **Licencia**: MIT
- **Popularidad**: **52,162 stars / 2,838 forks** (abril 2026) — el rey open-source en esta categoría
- **Última actualización**: 17-abr-2026 (muy activo)
- **Size**: ~14 MB repo, ~108 KB XAML compilado (`xaml/inputXML.xaml`)
- **Categorías de tweaks** (derivadas de `config/tweaks.json`):
  - `Essential Tweaks` (telemetría, activity history, GameBar/DVR, hibernación, disk cleanup, restore point)
  - `z__Advanced Tweaks - CAUTION` (servicios masivos, Powershell7 Telemetry, Consumer Features, WPBT)
  - Con `Type: Toggle` para features instantáneos de alto impacto (dark mode, end task, show file extensions)
- **Cantidad de tweaks**: El JSON pesa **80 KB** — aproximadamente **60-80 tweaks distintos** + ~450 apps en `applications.json` (160 KB)
- **Windows 11 24H2**: **SÍ**. Activamente testeado. Maneja `autounattend.xml` para ISOs customizadas.
- **Soporte revert**: **SÍ granular**. Cada tweak declara:
  - `InvokeScript` — aplicar
  - `UndoScript` — revertir
  - `registry[].OriginalValue` — valor antes del cambio (o `<RemoveEntry>` si había que crear clave)
  - `service[]` — los servicios tienen `OriginalType` implícito (guardado antes de aplicar)
- **Restore Point automático**: **SÍ opcional** — `WPFTweaksRestorePoint` en preset estándar. No fuerza.
- **UI**: WPF con tabs (Install, Tweaks, Config, Updates). Search box con debounce. Estilo "Chris Titus" oscuro minimalista.
- **Lo que hace BIEN**:
  1. **Motor declarativo JSON-first**: cada tweak es un objeto con schema. Separación limpia datos/código.
  2. **Compile.ps1** (custom compiler): junta `functions/private/*`, `functions/public/*`, `config/*.json`, `xaml/inputXML.xaml`, `scripts/main.ps1` → un único `winutil.ps1` que se ejecuta con `irm | iex`. Brillante para distribución sin instalador.
  3. **Runspace pool con `$sync` hashtable compartida** para UI no-bloqueante (`functions/public/Invoke-WPFRunspace.ps1`)
  4. **Presets** (`config/preset.json`): "Standard" (13 tweaks) y "Minimal" (4 tweaks) = superficie para extender con **presets gaming**.
  5. Dispatcher `Invoke-WinUtilTweaks` maneja 5 primitivas: registry / service / scheduledtask / appx / InvokeScript+UndoScript — es el patrón maestro a imitar.
  6. `KeepServiceStartup` flag: si detecta que el usuario modificó un servicio manualmente, **no lo pisa**. Diseño respetuoso.
- **Lo que hace MAL / le falta**:
  1. **CERO gaming-específico real**: no tiene timer resolution, no MSI mode, no MMCSS SystemResponsiveness, no HAGS toggle, no Process Lasso-like priorities.
  2. Power plan: solo "Ultimate Performance" básico (`Invoke-WPFUltimatePerformance.ps1`) — útil pero primitivo.
  3. No hay **perfiles de usuario** persistentes (exportar config = JSON manual, no UI-friendly).
  4. La carga inicial de `winutil.ps1` es lenta (~2-3s por el `.Compile` gigante).
  5. Sin telemetría de éxito/fallo por tweak — no sabe qué tweaks rompen en un sistema específico.
  6. Un solo XAML monolítico (108 KB). Mantenimiento UI complicado.
- **Features para ROBAR**:
  - Schema JSON declarativo para tweaks con `Invoke/Undo/registry/service/scheduledtask/appx` — **arquitectura core a copiar**.
  - Presets exportables.
  - `irm | iex` para distribución one-liner.
  - `KeepServiceStartup` (respetar override manual del user).
- **Errores a EVITAR**:
  - XAML monolítico.
  - Dependencia 100% de PowerShell (arranque lento, dependencia de versiones).
  - Falta de validación post-tweak (¿el registro realmente quedó en 0?).

---

### 1.2 hellzerg/optimizer (Optimizer) — DEPRECATED

- **URL**: https://github.com/hellzerg/optimizer (archivado; sucesor: **OptimizerNXT** privado/comercial)
- **Stack**: C# WinForms sobre .NET Framework 4.8.1 + Newtonsoft.Json (711 KB dll)
- **Licencia**: GPL-3.0
- **Popularidad**: **18,112 stars / 1,178 forks** — era referencia mainstream
- **Última actualización**: 20-ene-2026 (último commit antes de archivar)
- **Categorías** (de la documentación y source):
  - Privacy/Telemetry (+Cortana, CoPilot AI en Edge)
  - UWP apps removal
  - System Services (incl. Windows Updates)
  - Advanced (HPET, OneDrive, UTC time, HiDPI fixes)
  - Network tools (DNS presets, ping, SHODAN, HOSTS editor)
  - Cleanup (drives, browser profiles)
  - Startup items
  - Hardware inspector + env path editor
- **Cantidad**: ~100 toggles + herramientas utilitarias
- **W11 24H2**: SÍ, pero con `/unsafe` para server. No garantizado a largo plazo dado el archivado.
- **Revert**: **SÍ — Restore Point automático** antes de aplicar batch. Cada toggle recuerda estado original via `OptionsHelper.cs`.
- **UI**: WinForms clásico con tabs; traducido a 24 idiomas (fortaleza enorme — español incluido).
- **Arquitectura** (del árbol del source):
  - `OptimizeHelper.cs` **143 KB** — MONOLITO GIGANTE: todo el dispatcher de tweaks en un archivo (antipatrón a NO imitar)
  - `SilentOps.cs` 53 KB, `IndiciumHelper.cs` 28 KB, `CleanHelper.cs` 19 KB
  - `Forms/`, `Controls/`, `Models/`, `Resources/` — separación clásica WinForms
  - `feed.json` (30 KB) en raíz — distribuye updates del tool
- **Lo que hace BIEN**:
  1. UI responsive y rápida (WinForms nativo sin overhead PowerShell).
  2. Localización profesional (24 idiomas) — WinUtil no tiene esto.
  3. HOSTS editor integrado con bloqueo masivo de dominios telemetry.
  4. Restore point **automático antes de batch** — mejor UX que pedirlo.
  5. Hardware inspector = útil para mostrar contexto "cuál es tu CPU/GPU antes de recomendar".
- **Lo que hace MAL**:
  1. **Monolito `OptimizeHelper.cs` 143 KB** = deuda técnica inmensa.
  2. WinForms = look anticuado (sin Fluent/Mica).
  3. **Proyecto archivado** — oportunidad de capturar a sus ex-usuarios.
  4. Sin perfiles gaming, cero latencia real.
  5. .NET Framework 4.8 (legacy) en vez de .NET 8/9.
- **Features para ROBAR**:
  - Localización multilenguaje desde día 1.
  - Restore point pre-batch automático.
  - Hardware inspector dashboard.
  - HOSTS editor con categorías (telemetry / adware / malware lists).
- **Errores a EVITAR**:
  - Monolito de 143 KB en un archivo.
  - Stack legacy (elegir .NET 8+).
  - No tener sucesor open-source (→ oportunidad).

---

### 1.3 Atlas OS — Atlas-OS/Atlas

- **URL**: https://github.com/Atlas-OS/Atlas | https://docs.atlasos.net/
- **Stack**: Playbook para **AME Wizard**. Archivos: `.apbx` (zip encriptado password) con YAML + scripts batch + paquetes CAB SXSC para remover componentes del Windows Component Store.
- **Licencia**: GPL-3.0
- **Popularidad**: **20,236 stars / 714 forks**
- **Última actualización**: 26-mar-2026
- **Categorías**: NO es app con UI granular — es una transformación OS end-to-end. User elige toggles en AME Wizard.
- **Qué remueve/deshabilita** (verificado en docs oficiales):
  - **Permanentemente borrado**: Smart App Control
  - **Apps deinstaladas**: Internet Explorer, OneDrive (condicional), Microsoft Edge, Steps Recorder, Legacy Windows Media Player, Maths Recognizer
  - **Store apps**: Teams, Cortana, Tips, Get Help, Feedback Hub, Voice Recorder, Snipping Tool, Snip & Sketch, Clipchamp, Xbox Console Companion, Films & TV, 3D Viewer, Microsoft Office Hub, People, Skype, Windows Alarms & Clock, Windows Maps, Disney+, Mail and Calendar, Microsoft Family Safety, Dev Home, Weather, News, Sticky Notes, OneNote, To Do, Phone Link
  - **Telemetry package** (`Z-Atlas-NoTelemetry-Package`): 14 componentes core (DiagTrack service + ecosistema). **NO** toca `Microsoft-Windows-Application-Experience-Infrastructure`, `Microsoft-Windows-CoreSystem-Bluetooth-Telemetry`, `Microsoft-Windows-DeviceCensus`, ni el runtime de `Microsoft-Windows-Unified-Telemetry-Client` (solo resources/extensions).
  - **Toggleables con advertencia**: Defender, SmartScreen, Windows Update, CPU mitigations (Spectre/Meltdown), UAC, Core Isolation
- **W11 24H2**: SÍ (25H2 también soportado en releases recientes).
- **Soporte revert**: **Limitado**. SXSC removal de componentes es **IRREVERSIBLE** sin reinstalar Windows. Los toggles de políticas/servicios SÍ son reversibles.
- **Restore Point**: AME Wizard lo crea opcionalmente.
- **UI**: AME Wizard (third-party) — GUI para seleccionar toggles del playbook.
- **Lo que hace BIEN**:
  1. **Transparencia total**: playbooks son archivos plaintext → auditable por comunidad.
  2. Anti-placebo: documentación explícita de qué hace cada tweak y por qué.
  3. Remueve **componentes a nivel SXS** (`sxsc/`) — imposible de lograr con registry tweaks.
  4. `CPU mitigations toggle` — esto SÍ impacta FPS real (~5-15% en Ryzen/Intel afectados).
  5. Documenta trade-offs de seguridad sin paternalismo.
- **Lo que hace MAL / le falta**:
  1. **No es app** — dependencia de AME Wizard.
  2. **Irreversibilidad** de SXSC changes — una vez aplicado, no vuelves atrás sin reinstalar.
  3. Sin GUI propia post-instalación.
  4. User que quiere "algo menos extremo" no tiene puntos intermedios.
- **Features para ROBAR**:
  - Documentación **transparente de trade-offs** por tweak.
  - Toggles de CPU mitigations (tweak gaming REAL de alto impacto).
  - Remoción de Smart App Control como opción avanzada.
- **Errores a EVITAR**:
  - Irreversibilidad total. **Nuestra app debe mantener reversibilidad en el 95%+ de tweaks**.
  - Dependencia de herramienta externa (AME Wizard).

---

### 1.4 ReviOS — meetrevision/playbook

- **URL**: https://revi.cc | https://github.com/meetrevision/playbook
- **Stack**: Playbook AME Wizard (mismo formato que Atlas)
- **Filosofía**: "Balance entre performance y compatibilidad" (menos agresivo que Atlas)
- **W11 24H2**: SÍ — Windows 11 23H2, 24H2, 25H2 (x64 + ARM64), incl. LTSC/Enterprise/Pro/Home
- **W10**: 21H2, 22H2 x64
- **Diferencias con Atlas**:
  - Preserva más funcionalidad Windows "estudiante/oficina"
  - Integra un "**Revision Tool**" post-install para ajustes adicionales
  - Menos agresivo con Defender/Update (defaults más conservadores)
- **Target**: usuarios mixtos gaming + productividad. Atlas es gaming-only.
- **Features para ROBAR**:
  - El concepto de **Revision Tool post-install** — es justamente lo que queremos: GUI para ajustar después.
  - Soporte ARM64 declarado (considerar si lanzamos para Snapdragon X)
  - Preservar más compatibilidad en modo "Gaming Casual" vs "Competitivo".

---

### 1.5 Tiny11 Builder — ntdevlabs/tiny11builder

- **URL**: https://github.com/ntdevlabs/tiny11builder
- **Stack**: PowerShell (pipeline DISM)
- **Popularidad**: **18,441 stars / 1,424 forks**
- **Última actualización**: 12-sep-2025 (menos activo que Win11Debloat)
- **Qué hace**: construye un ISO Windows 11 reducido **antes de instalar**
- **Dos variantes**:
  - `tiny11maker`: remueve 22 apps (Clipchamp, Edge, OneDrive, Xbox, Mail, Feedback Hub, etc.), mantiene serviceability
  - `tiny11coremaker`: agrega remoción de WinSxS, Defender, Windows Update, WinRE → ISO no mantenible
- **W11 24H2**: "Cualquier release Win11" — sí
- **Licencia**: sin especificar (🚩 — cuidado al reutilizar)
- **Lo que hace BIEN**:
  1. Pipeline DISM reproducible.
  2. "Serviceable" vs "Core" — conceptos a copiar como "niveles de profundidad".
- **Lo que hace MAL**:
  1. Sin license clara.
  2. Algunos residuos Edge persisten.
  3. Outlook/Dev Home reaparecen con updates.
- **Features para ROBAR**:
  - Modo "ISO Builder" como feature avanzada opcional (pero quizás fuera del scope V1).
- **Errores a EVITAR**:
  - No tener license explícita.

---

### 1.6 Privatezilla — builtbybel/privatezilla

- **URL**: https://github.com/builtbybel/privatezilla
- **Stack**: C# (presumiblemente WinForms) + scripts PowerShell para acciones
- **Licencia**: MIT
- **Popularidad**: **3,725 stars / 169 forks**
- **Última actualización**: 18-abr-2023 — **NO actualizado**. Solo W10 1809-2009 oficialmente.
- **Features**: 60 privacy settings con indicador "Configured/Not Configured"
- **Features para ROBAR**:
  - **Indicador de estado actual** de cada tweak (crítico — ver "gap analysis" abajo). Privatezilla hace esto bien.
  - Plantillas de automation compartibles.
- **Por qué NO imitar**:
  - Stale desde 2023.
  - Scope muy estrecho (solo privacy, sin gaming).

---

### 1.7 Win11Debloat — Raphire/Win11Debloat

- **URL**: https://github.com/Raphire/Win11Debloat
- **Stack**: PowerShell puro + batch launcher
- **Licencia**: MIT
- **Popularidad**: **44,857 stars / 1,818 forks** — el debloat script más popular 2024-2026
- **Última actualización**: 17-abr-2026 (muy activo)
- **Tres modos ejecución**:
  1. Quick — `irm | iex` interactivo
  2. Traditional — descargar + launcher GUI batch
  3. Advanced — PowerShell con parámetros CLI (automatizable)
- **Categorías (8 mayores)**:
  - Privacy & Telemetry, AI Features (Copilot, Recall), System Settings (context menu, mouse accel, BitLocker), Windows Update delays, Visual (dark mode/animations), Start Menu & Search (Bing OFF), Taskbar & Explorer, Multi-tasking (snap)
- **W11 24H2**: SÍ — activamente soportado
- **Revert**: Wiki con guía manual de reversión + re-install apps via Store
- **Lo que hace BIEN**:
  1. **CLI parametrizada** → deployment automatizado en empresas/VMs.
  2. "Recommended for most people" preset → UX bajísima fricción.
  3. README extenso y mantenido.
- **Lo que hace MAL**:
  1. Sin UI gráfica (solo launcher batch minimalista).
  2. Sin restore point automático declarado.
  3. Revert manual, no integrado.
- **Features para ROBAR**:
  - Preset "Recommended" one-click + "Advanced" para power users.
  - Modo CLI completo (`--NoCopilot --NoBloat --SilentDark`) para scripts/deployment.
- **Errores a EVITAR**:
  - Falta de GUI — esto es exactamente lo que nos diferencia.

---

### 1.8 Sophia Script for Windows — farag2/Sophia-Script-for-Windows

- **URL**: https://github.com/farag2/Sophia-Script-for-Windows
- **Stack**: PowerShell 5.1/7 con **150+ funciones documentadas**, opcionalmente GUI C# WinUI 3 (**SophiApp 2.0** en desarrollo)
- **Licencia**: MIT
- **Popularidad**: **9,226 stars / 636 forks**
- **Última actualización**: 17-abr-2026
- **Cobertura**: Win10 (incl. LTSC 2019/2021) + Win11 (incl. LTSC 2024, 24H2, 25H2). x64 + ARM64.
- **Distribuido en**: Scoop, Chocolatey, WinGet
- **Lo que hace BIEN (estándar de oro técnico)**:
  1. **Cada tweak tiene su función de reverso** → "Every tweak has its corresponding function to restore default settings". Este es el modelo canónico.
  2. Usa **solo vías oficialmente documentadas por Microsoft** → cero dependencia de clave registry no-documentada que Microsoft puede romper.
  3. Registry policies aparecen en gpedit.msc → transparente para admins corporativos.
  4. Localizaciones via PSData files.
  5. Tab completion en consola (muy pro).
  6. **"No conflict with VAC"** — explícitamente testado con anti-cheat Steam.
  7. Stability monitoring integrado.
- **Lo que hace MAL**:
  1. GUI wrapper "third-party closed-source" — ruptura de filosofía open.
  2. 150 funciones = curva de aprendizaje empinada.
  3. SophiApp 2.0 (GUI oficial nueva) **aún en desarrollo** → hueco de GUI moderna aprovechable.
- **Features para ROBAR**:
  - **Paridad estricta apply↔revert** como invariante de diseño.
  - "Ways officially documented by Microsoft" como regla de aceptación de cualquier tweak nuevo.
  - Anti-cheat compatibility testing explícito.
  - Distribución en Scoop/Choco/WinGet.
- **Errores a EVITAR**:
  - Tener la GUI principal cerrada (romper confianza open-source).

---

### 1.9 W10Privacy

- **URL**: https://www.w10privacy.de (sin GitHub público canónico)
- **Stack**: .NET (cerrado, freeware)
- **Cobertura**: **316 opciones en 16 categorías** — Privacy, Apps, Telemetry, Search, Network, Explorer, Services, Edge, IE, OneDrive, Tasks, Tweaks, Firewall, Background-Apps, User-Apps, System-Apps
- **W11 24H2**: SÍ (pese al nombre "W10")
- **Última versión**: 5.4.0.0 (enero 2026)
- **Lo que hace BIEN**:
  1. **Escala bruta**: 316 opciones = granularidad extrema.
  2. Tooltips explican cada opción al hover.
  3. Bloqueo IPs Microsoft telemetry vía HOSTS + firewall rules.
- **Lo que hace MAL**:
  1. Cerrado.
  2. UI anticuada.
  3. Sin presets gaming.
- **Features para ROBAR**:
  - Granularidad extrema para modo "Expert" (sin sobrecargar al newbie).
  - Tooltip técnico por tweak con link a fuente.

---

### 1.10 O&O ShutUp10++

- **URL**: https://www.oo-software.com/en/shutup10
- **Stack**: .NET freeware cerrado
- **Versión**: 2.2.1024 (4-feb-2026)
- **Distintivo**: **color-coded recommendations** (verde seguro / amarillo cuidado / rojo puede romper)
- **Controles 2026**: dedicados para Windows Copilot, Recall, AI en Paint/Photos/Office
- **Credencial confianza**: usado por orgs NATO-affiliated y 76% empresas DAX listadas
- **Features para ROBAR**:
  - **Clasificación verde/amarillo/rojo** por nivel de riesgo — UX brillante para debloat responsable.
  - Accent en novedades: cuando sale una versión W11, actualizar dedicated controls rápidamente.

---

### 1.11 MSMG Toolkit

- **URL**: https://msmgtoolkit.in (cerrado pero gratis)
- **Stack**: Batch CLI + DISM
- **UI**: ncurses/DOS-style
- **Scope**: customización offline de ISO (remover componentes, integrar updates, drivers, features)
- **Features para ROBAR**:
  - Modo "offline ISO customization" como avanzada opcional.
- **Por qué NO imitar UI**:
  - DOS ncurses es barrera de entrada alta.

---

## 2. Análisis Tier 2 — Especializadas Gaming

### 2.1 Process Lasso — Bitsum (comercial)

- **Versión 2026**: **18.0.1.24** (lanzada 2026, nuevo: GPU metrics, ThreadRacer refactor, regex rules más rápidas)
- **Features clave**:
  - **ProBalance**: ajusta dinámicamente prioridades para mantener responsiveness bajo carga alta
  - **Gaming Mode**: power plan highest-performance + behavior tweaks ProBalance
  - **Persistent CPU affinity / priority** por proceso
  - Threshold-based actions (cuando un proceso supera X% CPU → cambiar prioridad)
  - ThreadRacer (benchmark hilos)
- **Lo que podemos adaptar en open-source**:
  - Prioridades persistentes por ejecutable (clave registry `HKLM\...\ImageFileExecutionOptions\<exe>\PerfOptions`)
  - Auto-boost de prioridad al detectar foreground game (Win32 APIs GetForegroundWindow + SetPriorityClass)

### 2.2 ISLC — Wagnardsoft

- **Versión**: 1.0.4.5
- **Mecanismo**: monitorea standby list + free RAM. Purga via APIs Windows documentadas (mismas que RAMMap). Umbral default: purgar cuando standby > 1024 MB.
- **Safety**: no toca archivos/registro permanentes.
- **Lo que podemos adaptar**:
  - Módulo "RAM Cleaner" opcional con configuración de threshold.
  - Llamar `NtSetSystemInformation(SystemMemoryListInformation, ...)` — API nativa.

### 2.3 Timer Resolution / SetTimerResolutionService — Lucas Hale

- **Mecanismo**: API documentada `NtSetTimerResolution`. Sin drivers, sin admin (para Timer Resolution). `SetTimerResolutionService` es servicio que persiste el tick entre reboots.
- **Default Windows**: 15.6ms. Esports: 0.5-1.0ms.
- **Impacto real**: FPS promedio no cambia mucho, pero **1% / 0.1% lows mejoran dramáticamente** (25-30% reducción micro-stutter en CS:GO/Apex).
- **HPET trade-off**: `bcdedit /deletevalue useplatformclock` + timer 0.5ms ayuda en algunos chipsets, no en todos.
- **Lo que podemos adaptar**:
  - Instalar servicio propio en C# equivalente (P/Invoke `NtSetTimerResolution`) para persistencia.
  - Toggle `useplatformclock` con warning por-chipset.

### 2.4 MSI Mode Utility v3 — Sathango/Msi-Utility-v3

- **Mecanismo**: GUI para togglear Message Signaled Interrupts en devices PCI/PCIe. Mejora DPC latency.
- **Registry**: `HKLM\SYSTEM\CurrentControlSet\Enum\PCI\VEN_XXXX&DEV_XXXX\...\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties\MSISupported=1`
- **Priority**: `DevicePriority` también configurable por device.
- **Impacto**: reduce DPC latency en GPU/NIC/USB controllers que lo soportan.
- **Lo que podemos adaptar**:
  - Módulo "Interrupt Optimization" que enumera dispositivos PCI, muestra estado MSI actual, permite toggle con preview del Hardware ID.
  - **Diferenciador grande**: ninguna app grande open-source incluye esto integrado.

### 2.5 DDU — Display Driver Uninstaller (Wagnardsoft)

- **Versión**: 18.1.5.2
- **Mecanismo**: escanea registro + driver store + archivos + servicios + caches relacionados al driver GPU. Requiere Safe Mode para full clean.
- **Integración posible**:
  - Lanzador "Clean GPU Driver" que programa reboot a safe mode + ejecuta DDU unattended + reboot normal + instala driver limpio.
  - Distribuir DDU binario con user approval + license — o llamar si ya instalado.

### 2.6 NVIDIA Profile Inspector (Orbmu2k)

- **Versión**: v3.0.1.8
- **Acceso**: driver database interno NVIDIA, **settings ocultos del Control Panel**
- **Settings clave gaming**: V-Sync behavior, Anti-Aliasing override, Ambient Occlusion, Threaded Optimization, Low Latency Mode, Power Management Mode
- **Integración posible**:
  - Aplicar profile NVIDIA preconfigurado por juego (CS2, Valorant, Apex, etc.) via su API de perfiles (nvapi.dll).

### 2.7 NVCleanstall (TechPowerUp)

- **Versión 1.19.0**
- **Mecanismo**: descarga driver NVIDIA de la fuente oficial (o acepta un .exe local), permite deseleccionar componentes **más granular que el custom install oficial** — incluyendo **Telemetry**.
- **Warning crítico**: driver sin telemetry puede ser flagged por anti-cheat (Vanguard, BattlEye).
- **Integración posible**:
  - Módulo "Driver Installer" clean que usa NVCleanstall si el user ya lo tiene, o solamente **links al download oficial** (evitamos bundle por licencia).

### 2.8 CRU — Custom Resolution Utility (ToastyX)

- **Mecanismo**: editor EDID para resoluciones custom + override refresh rate
- **Gaming relevance**: overclock refresh rate monitor, custom resolutions para sweet spots, HDR metadata override.
- **Limitaciones**: G-Sync monitors con processor dedicado limitan resoluciones escalables. DSC activo bypassea EDID overrides.
- **Integración posible**:
  - Module "Display Tweaks" que lee EDID, muestra máximos, permite overclock con warning.

### 2.9 ThrottleStop

- **Versión stable**: 9.7 (beta 9.7.3 abril 2025)
- **Features**:
  - **FIVR**: undervolt CPU Intel (Fully Integrated Voltage Regulator)
  - **TPL**: Turbo Power Limit + Speed Shift
  - **BD PROCHOT**: disable signal throttle externo
  - **Turbo disable**: opcional
- **Gaming relevance**: undervolt = menos temp = menos throttle = FPS más estable en laptops.
- **Integración**: delicado (requiere drivers kernel). Mejor link externo + detección de presencia.

### 2.10 TronScript

- **GitHub**: bmrf/tron
- **Stages**: Prep → TempClean → De-bloat → Disinfect (Kaspersky/Sophos/MBAM) → Repair → Patch → Optimize → Wrap-Up
- **Duración**: 3-10 horas (overnight tool)
- **Relevance**: modelo de **pipeline por stages con logging exhaustivo** — buen patrón para modo "Full System Maintenance".

---

## 3. Tabla comparativa maestra

| Feature | WinUtil | Optimizer (hellzerg) | Atlas OS | ReviOS | Win11Debloat | Sophia Script | Privatezilla | W10Privacy | ShutUp10++ | Tiny11 | Process Lasso | ISLC | TimerRes | MSI Util | DDU | NVCleanstall |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Tweaks gaming** (timer/MSI/MMCSS/HAGS) | Parcial | No | Parcial | Parcial | No | Parcial | No | No | No | No | Sí | Sí (RAM) | Sí | Sí | No | No |
| **Debloat UWP** | Sí | Sí | Sí++ | Sí | Sí | Sí | No | Sí | No | Sí++ | No | No | No | No | No | No |
| **Privacidad/Telemetry** | Sí | Sí | Sí | Sí | Sí | Sí | Sí++ | Sí++ | Sí++ | Sí | No | No | No | No | No | Parcial |
| **Registry tweaks declarativos** | Sí (JSON) | Sí (hardcoded) | Sí (YAML) | Sí | Sí | Sí (PS funcs) | Sí | Sí | Sí | No | N/A | N/A | N/A | Sí | N/A | N/A |
| **Servicios** | Sí | Sí | Sí (SXSC) | Sí | Parcial | Sí | No | Sí | Parcial | Sí | No | No | Sí (svc) | No | No | No |
| **Power plans** | Básico | Sí | Sí | Sí | No | Sí | No | No | No | No | Sí++ | No | No | No | No | No |
| **GPU tweaks** | No | No | No | No | No | Parcial | No | No | No | No | GPU metrics | No | No | Sí (MSI) | Sí | Sí (bloat) |
| **Revert granular** | Sí | Sí | Parcial (SXSC no) | Parcial | Manual | Sí++ | Sí | Sí | Sí | No | Sí | Sí | Sí | Sí | N/A | N/A |
| **Restore Point automático** | Opcional | **Sí (batch)** | Vía AME | Vía AME | No | No | No | No | No | No | No | No | No | No | No | No |
| **Perfiles/Presets** | 2 básicos | No | No | No | "Recommended" | No | Plantillas | Sí | No | 2 modos | Gaming Mode | Thresholds | Profiles | No | No | No |
| **Win11 24H2** | **Sí** | Sí | **Sí** | **Sí** (25H2) | **Sí** | **Sí** (25H2) | No | Sí | Sí | Sí | Sí | Sí | Sí | Sí | Sí | Sí |
| **Localización** | No/EN | **24 idiomas** | EN | EN + varios | EN | ES incluido | EN+DE | DE+EN | Multi | EN | EN | EN | EN | EN | EN | EN |
| **Open source** | MIT | GPL-3 (archived) | GPL-3 | Sí | MIT | MIT | MIT | No | No | sin license | No | No | No | Sí | No | No |
| **GUI moderna** | WPF clásico | WinForms legacy | AME Wizard | AME + Revi Tool | No (CLI) | 3rd party o dev | Sí | Legacy | Simple | CLI | Sí | Sí | Sí | Sí | Sí | Sí |

---

## 4. Gap analysis — dónde está la oportunidad

### 4.1 Qué tienen TODOS (commodity — no diferenciador)
- Debloat UWP (Cortana, Teams, Xbox Game Bar, Widgets)
- Disable telemetry via registry policy (`HKLM\...\DataCollection\AllowTelemetry=0`)
- Disable consumer features + Activity History + Advertising ID
- Dark mode toggle + File Explorer tweaks
- Remove bloatware pinned apps

### 4.2 Qué hacen ALGUNOS (medio-diferenciador)
- **Restore point automático antes de batch** → solo Optimizer
- **Revert granular paridad 100%** → solo Sophia Script y WinUtil
- **CLI parametrizada para deployment** → solo Win11Debloat y Sophia
- **Localización multi-idioma profesional** → solo Optimizer (24 idiomas)
- **Presets exportables/compartibles** → solo WinUtil (básico) y Privatezilla (templates)

### 4.3 Qué NO hace NINGUNO (la oportunidad)

1. **GUI moderna + tweaks de latencia real (timer/MSI/DPC/MMCSS/HAGS) en una sola app**
   - WinUtil no los tiene. Process Lasso los tiene pero es cerrado/de pago. MSI Util es standalone técnico.
2. **Profiler de estado pre/post aplicación**
   - Nadie mide latencia antes/después, FPS antes/después, tiempo de boot antes/después. Sería un killer feature.
3. **Perfiles de juego (game-aware)**
   - Nadie detecta "ejecutándose CS2" y aplica perfil "CS2 competitive" (affinity + prio + NVIDIA profile + timer 0.5ms + desactivar Widgets).
4. **Snapshot/Rollback stack completo**
   - Nadie guarda un snapshot de estado completo (registry + services + scheduled tasks + power plan) con diff visual. Todos confían en restore point de Windows que es unreliable en 24H2.
5. **Tweaks auditables con fuente**
   - Nadie muestra "este tweak viene de [link a documentación Microsoft]" o "[thread con benchmark]". Sophia se acerca pero no lo expone en UI.
6. **CPU mitigations toggle en GUI común**
   - Solo Atlas lo tiene. Registry `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\FeatureSettingsOverride` + `FeatureSettingsOverrideMask` — 5-15% FPS en Zen2/Intel 8th-10th gen afectados.
7. **Integración anti-cheat awareness**
   - Detectar si user juega con Vanguard/BattlEye/EAC antes de aplicar tweaks que les caen mal (como disable Defender o MSI mode en GPU — algunos anti-cheats lo ven raro).
8. **Un "dry run" mode**
   - Ningún tool ofrece "aplicar en sandbox/virtual" o "mostrar exactamente qué va a cambiar con un diff antes de confirmar".
9. **Benchmark integrado pre/post**
   - 3DMark mini, latencymon-like check de DPC, timerbench check. Nadie lo integra.
10. **Undervolt GPU + tweaks OS combinados**
    - Atlas toca OS. NVIDIA Inspector toca GPU driver. NVCleanstall toca install. **Nadie los combina**.

---

## 5. Arquitectura recomendada para la app propia

### 5.1 Stack

**Recomendación principal**: **C# .NET 8/9 + WPF + Fluent theme (.NET 9 default)**

Razones:
- **WPF está en activo mantenimiento** en .NET 9 con Fluent theme nativo (dark/light + accent Windows 11) sin hacks.
- Ecosistema de controles maduro (MahApps.Metro, ModernWPF, WPFUI).
- Menor fricción que WinUI 3 (que tiene deployment quirks con MSIX/unpackaged).
- **Perf superior** vs PowerShell-cargado-en-memoria (arranque sub-segundo).
- Avalonia sería cross-platform pero no necesitas eso — es solo Windows.
- WinForms es lo que usó Optimizer y quedó obsoleto visualmente.

**Complementos**:
- **CommunityToolkit.Mvvm** para MVVM patterns.
- **Serilog** para logging estructurado.
- **YamlDotNet** o **System.Text.Json** para config files.
- **Microsoft.Win32.Registry** para registry (o direct P/Invoke a `RegOpenKeyEx` para claves sensibles).
- **System.ServiceProcess.ServiceController** para servicios.
- **Microsoft.PowerShell.SDK** si necesitas invocar scripts PS embebidos (fallback para lo que es más fácil en PS).

### 5.2 Estructura del repo (basada en lo mejor de WinUtil + separation of concerns)

```
/ (repo root)
├── src/
│   ├── App/                        # WPF entry point (App.xaml, MainWindow)
│   ├── App.Core/                   # Logic core, tweak engine
│   │   ├── Engine/                 # TweakExecutor, Reverter, Snapshotter
│   │   ├── Primitives/             # RegistryTweak, ServiceTweak, AppxTweak, ScheduledTaskTweak, ScriptTweak
│   │   ├── Detection/              # GameDetector, HardwareDetector, AntiCheatDetector
│   │   └── Snapshot/               # SystemSnapshot serialization
│   ├── App.Data/                   # Data access (JSON/YAML loaders)
│   ├── App.UI/                     # XAML views, controls, converters
│   │   ├── Views/                  # One XAML per tab (NOT monolithic)
│   │   │   ├── DashboardView.xaml
│   │   │   ├── TweaksView.xaml
│   │   │   ├── AppsView.xaml
│   │   │   ├── GamingProfilesView.xaml
│   │   │   ├── LatencyView.xaml    # DPC/timer/MSI/MMCSS
│   │   │   ├── SnapshotsView.xaml
│   │   │   └── SettingsView.xaml
│   │   ├── Controls/               # Reusable TweakCard, PresetSelector, etc.
│   │   └── ViewModels/
│   ├── App.Service/                # Background service (timer resolution, ISLC-like, game detection)
│   │   └── (Service que corre SetTimerResolution + standby cleaner configurable)
│   └── App.Cli/                    # CLI optional (headless deployment)
├── data/
│   ├── tweaks/                     # UN YAML por tweak (o agrupados por categoría)
│   │   ├── essential/
│   │   ├── privacy/
│   │   ├── gaming/
│   │   ├── latency/
│   │   └── advanced/
│   ├── presets/
│   │   ├── fps-competitive.yaml
│   │   ├── aaa-singleplayer.yaml
│   │   ├── streaming.yaml
│   │   ├── laptop-battery.yaml
│   │   └── minimal-debloat.yaml
│   ├── apps/                       # Winget/choco catálogo
│   └── localization/
│       ├── es.json
│       ├── en.json
│       └── ...
├── tests/
│   ├── Unit/                       # Lógica tweak engine
│   └── Integration/                # En Windows VMs con snapshots
├── docs/
└── tools/
    └── tweak-validator/            # Valida que cada tweak tenga apply + revert
```

### 5.3 Modelo de datos (schema de tweak — YAML-first)

**Invariante de diseño**: todo tweak DEBE tener `apply` + `revert` + `source` + `risk_level` + `anti_cheat_safe`.

```yaml
id: gaming.disable-mouse-acceleration
display_name: "Mouse Acceleration - Disable"
description: >
  Disables "Enhance pointer precision" which applies nonlinear acceleration.
  Critical for FPS gaming to achieve 1:1 mouse input.
category: gaming.input
risk_level: safe      # safe | caution | risky
anti_cheat_safe: true
source:
  - type: microsoft_docs
    url: https://learn.microsoft.com/en-us/windows-hardware/design/...
  - type: benchmark
    url: https://www.reddit.com/r/GlobalOffensive/...
tags: [fps, input, mouse]
impact:
  fps: "none"
  latency: "1-2ms mouse input"
  compatibility: "universal"
apply:
  - type: registry
    path: "HKCU:\\Control Panel\\Mouse"
    entries:
      - { name: MouseSpeed,        value: "0", kind: String }
      - { name: MouseThreshold1,   value: "0", kind: String }
      - { name: MouseThreshold2,   value: "0", kind: String }
revert:
  - type: registry
    path: "HKCU:\\Control Panel\\Mouse"
    entries:
      - { name: MouseSpeed,        value: "1", kind: String }
      - { name: MouseThreshold1,   value: "6", kind: String }
      - { name: MouseThreshold2,   value: "10", kind: String }
validation:
  post_apply:
    - type: registry_equals
      path: "HKCU:\\Control Panel\\Mouse"
      name: MouseSpeed
      expected: "0"
```

### 5.4 Motor de ejecución (pseudocode C#)

Tomado del patrón `Invoke-WinUtilTweaks` de WinUtil pero en C#:

```csharp
public interface ITweakPrimitive {
    Task<TweakResult> ApplyAsync(TweakContext ctx, CancellationToken ct);
    Task<TweakResult> RevertAsync(TweakContext ctx, CancellationToken ct);
    Task<bool> ValidateAsync(TweakContext ctx, CancellationToken ct);
}

public class TweakEngine {
    private readonly ISnapshotManager _snapshot;
    private readonly IRestorePointService _restorePoint;
    private readonly ILogger _log;

    public async Task<BatchResult> ApplyBatchAsync(
        IEnumerable<TweakDefinition> tweaks,
        ApplyOptions opts,
        CancellationToken ct)
    {
        // 1. Pre-flight: system restore point if enabled
        if (opts.CreateRestorePoint)
            await _restorePoint.CreateAsync("PreApp-Tweaks");

        // 2. Take snapshot (serializable diff target)
        var snap = await _snapshot.CaptureAsync(tweaks);

        // 3. Apply each tweak primitive with per-tweak try/catch
        var results = new List<TweakResult>();
        foreach (var tweak in tweaks.OrderBy(t => t.Priority)) {
            foreach (var prim in tweak.Primitives) {
                var res = await prim.ApplyAsync(ctx, ct);
                results.Add(res);
                if (res.Failed && tweak.StopOnFail) break;
            }

            // 4. Post-apply validation
            if (tweak.Validation != null) {
                var ok = await ValidateAsync(tweak, ct);
                if (!ok) _log.Warning("Tweak {Id} applied but validation failed", tweak.Id);
            }
        }

        // 5. Persist snapshot for potential revert
        await _snapshot.PersistAsync(snap, results);
        return new BatchResult(results, snap.Id);
    }
}
```

### 5.5 Sistema de Snapshot/Revert (el diferenciador)

- **Snapshot = JSON serializado** con:
  - Todas las claves de registry que el tweak tocará (con su valor actual) ANTES.
  - Estado de cada servicio tocado (StartType + Status).
  - Estado de scheduled tasks.
  - AppX packages por user+machine antes de remover.
  - Power plan activo + GUID.
- **Storage**: `%LOCALAPPDATA%\NuestraApp\snapshots\{timestamp}_{preset}.json`
- **UI**: vista "Snapshots" con diff visual (before/after), botón "Rollback to this snapshot" que re-aplica inverso.
- **Complementa, no reemplaza**, el Restore Point de Windows — es más granular y más confiable.

### 5.6 Detección de juego en foreground (game-aware profiles)

```csharp
// Background service enumera procesos + ventana foreground
// Matchea contra catálogo de juegos conocidos (csgo.exe, cs2.exe, VALORANT-Win64-Shipping.exe, r5apex.exe)
// Al detectar: aplica perfil temporal (timer 0.5ms, affinity, kill Widgets, HAGS toggle si configurado)
// Al cerrar: revierte perfil temporal
```

Win32 APIs: `GetForegroundWindow`, `GetWindowThreadProcessId`, `OpenProcess`, `QueryFullProcessImageName`.

Evento: `ManagementEventWatcher` sobre `__InstanceCreationEvent` de `Win32_Process` para detectar launch instantáneo.

### 5.7 Módulos especializados (gaming-only)

| Módulo | Función | API/Método |
|---|---|---|
| `LatencyTuner` | Timer resolution persistente | `NtSetTimerResolution` via P/Invoke + servicio propio |
| `StandbyListCleaner` | ISLC-like integrado | `NtSetSystemInformation(SystemMemoryListInformation)` |
| `MSIConfigurator` | MSI mode + priority por device PCI | Registry `...Interrupt Management\MessageSignaledInterruptProperties` |
| `MmcssTuner` | `SystemResponsiveness` global + profiles por app | `HKLM\...\Multimedia\SystemProfile` + `\Tasks\{Games}` |
| `HagsToggle` | Hardware-accelerated GPU scheduling | `HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\HwSchMode` |
| `CpuMitigations` | Disable Spectre/Meltdown mitigations (con warning) | `FeatureSettingsOverride` + `Mask` |
| `PowerPlanManager` | Ultimate Performance + custom plan por perfil | `powercfg` CLI wrappers |
| `GameBoostDaemon` | Affinity + prio persistente por exe | `ImageFileExecutionOptions\<exe>\PerfOptions` |
| `HpetConfigurator` | `useplatformclock` toggle con chipset warning | `bcdedit /deletevalue useplatformclock` |

---

## 6. Features diferenciadoras propuestas

### 6.1 MUST-HAVE V1 (competir con WinUtil)

1. **Motor declarativo YAML** (superior a JSON de WinUtil — comentarios y multiline friendly).
2. **Apply + Revert paridad estricta** (copia Sophia Script).
3. **Snapshot granular pre-batch** con rollback diff visual.
4. **Restore Point automático** (copia Optimizer).
5. **GUI WPF Fluent** dark/light (WinUtil es feo, Optimizer es arcaico — hueco).
6. **Localización ES/EN** día 1 (copia Optimizer).
7. **Presets gaming**: "FPS Competitivo", "AAA Single Player", "Streaming + Gaming", "Battery Saver", "Minimal Debloat".
8. **Distribución**: MSIX + portable .exe + Winget + Scoop + Choco.
9. **CLI headless** para deployment (copia Win11Debloat).

### 6.2 SHOULD-HAVE V1.5 (diferenciar)

10. **Latency Tuner tab**: timer resolution, MSI mode, MMCSS, HAGS — todos en uno.
11. **Game Profile Daemon**: detección foreground + perfil temporal auto-aplicado.
12. **Auditoría/fuente por tweak**: cada tweak muestra link Microsoft Docs / benchmark / Sophia.
13. **Anti-cheat awareness**: detectar Vanguard/EAC/BattlEye/FaceIt AC instalado → flag tweaks riesgosos.
14. **Hardware dashboard**: CPU/GPU/RAM/Motherboard con tooltips "tweaks recomendados para tu hardware específico".
15. **Dry-run mode**: mostrar exactamente qué va a cambiar (diff registry + services) antes de confirmar.

### 6.3 NICE-TO-HAVE V2 (consolidar liderazgo)

16. **DPC Latency check integrado** (equivalente LatencyMon mini — usa `ETW providers` kernel).
17. **FPS benchmark pre/post** (usar PresentMon wrapper o Raylib ventana-test 60s).
18. **Preset marketplace**: comunidad publica `.preset.yaml` firmados, app los importa.
19. **Integration launchers**: Steam/Epic/Xbox Game Pass — detectar games instalados, sugerir profile por cada uno.
20. **"Health check" pre-tweak**: verificar que el sistema esté en estado conocido-bueno antes de aplicar (no aplicar si hay pending updates/reboots/disk errors).
21. **Scheduled snapshots** semanales auto (daemon).
22. **Export/Import + sync en la nube** (opcional, opt-in con OneDrive/GDrive folder).

### 6.4 Features que NO hay que hacer (risk/reward malo)

- Remover componentes SXSC (Atlas territory — rompe update path de Windows; irreversible).
- Bundle de DDU/NVCleanstall (licencias third-party complican distribución).
- UI con muchos effects 3D/acrylic excesivos que degrade perf en integrated GPUs.
- CPU mitigations toggle por default (solo bajo "Expert mode" con warning explícito y doble confirmación).

---

## 7. Snippets ejemplares (con fuente + licencia)

### 7.1 Schema JSON de tweak — adaptado de WinUtil (MIT)

Fuente: `ChrisTitusTech/winutil/config/tweaks.json` (MIT)

```json
{
  "WPFTweaksActivity": {
    "Content": "Activity History - Disable",
    "Description": "Erases recent docs, clipboard, and run history.",
    "category": "Essential Tweaks",
    "registry": [
      {
        "Path": "HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\System",
        "Name": "EnableActivityFeed",
        "Value": "0",
        "Type": "DWord",
        "OriginalValue": "<RemoveEntry>"
      }
    ]
  }
}
```

**Cómo adaptarlo**: conservamos el patrón `Path/Name/Value/Type/OriginalValue` pero migramos a YAML y agregamos `source`, `risk_level`, `anti_cheat_safe`, `validation` (ver §5.3).

### 7.2 Registry tweak helper — adaptado de WinUtil (MIT)

Fuente: `functions/private/Set-WinUtilRegistry.ps1`. Port a C#:

```csharp
public async Task<TweakResult> ApplyRegistryAsync(RegistryTweak t, CancellationToken ct)
{
    try {
        using var root = OpenRoot(t.Hive);  // HKLM, HKCU, HKU
        using var key = root.CreateSubKey(t.SubKey, writable: true)
            ?? throw new InvalidOperationException($"Cannot open {t.Path}");

        if (t.Value == RegistryTweak.RemoveEntryMarker) {
            key.DeleteValue(t.Name, throwOnMissingValue: false);
        } else {
            key.SetValue(t.Name, t.TypedValue(), t.Kind);
        }
        return TweakResult.Ok(t.Id);
    }
    catch (UnauthorizedAccessException ex) {
        return TweakResult.Fail(t.Id, "Permission denied", ex);
    }
    catch (SecurityException ex) {
        return TweakResult.Fail(t.Id, "Security exception", ex);
    }
}
```

### 7.3 Service tweak — adaptado de WinUtil (MIT)

```csharp
public async Task<TweakResult> ApplyServiceAsync(ServiceTweak t, CancellationToken ct)
{
    var svc = new ServiceController(t.Name);
    if (!ServiceExists(svc)) return TweakResult.Skip(t.Id, "service not installed");

    // Respetar override manual del user (KeepServiceStartup pattern de WinUtil)
    var current = ServiceStartType(svc);
    if (!t.ForceOverride && current != t.OriginalStartType) {
        return TweakResult.Skip(t.Id, "user-modified; preserving");
    }

    // Use ChangeServiceConfig via P/Invoke for AutomaticDelayedStart support
    // that ServiceController doesn't natively expose.
    return await SetStartTypeAsync(t.Name, t.TargetStartType, ct);
}
```

### 7.4 Timer resolution P/Invoke (basado en docs oficiales MSDN + Lucas Hale)

```csharp
[DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
internal static extern int NtSetTimerResolution(
    uint DesiredResolution,       // 100ns units (5000 = 0.5ms)
    bool SetResolution,
    out uint CurrentResolution);

public static void SetTimer(double milliseconds)
{
    uint hundredNs = (uint)(milliseconds * 10000);
    NtSetTimerResolution(hundredNs, true, out _);
}
```

### 7.5 Standby list cleanup (ISLC approach)

```csharp
[StructLayout(LayoutKind.Sequential)]
struct SYSTEM_MEMORY_LIST_COMMAND { public int Command; }

[DllImport("ntdll.dll")]
static extern int NtSetSystemInformation(
    int InfoClass,
    ref SYSTEM_MEMORY_LIST_COMMAND Info,
    int Size);

const int SystemMemoryListInformation = 80;
const int MemoryPurgeStandbyList = 4;

public static void PurgeStandbyList() {
    var cmd = new SYSTEM_MEMORY_LIST_COMMAND { Command = MemoryPurgeStandbyList };
    NtSetSystemInformation(SystemMemoryListInformation, ref cmd, Marshal.SizeOf(cmd));
}
```

*Requiere privilegio `SeProfileSingleProcessPrivilege` + admin.*

---

## 8. Decisiones críticas recomendadas

1. **Stack**: C# .NET 8/9 + WPF + Fluent theme. MVVM con CommunityToolkit.Mvvm. Descartar PowerShell como core (aunque soportar scripts PS como primitiva opcional para extensibilidad).
2. **Config format**: **YAML** (mejor que JSON — comentarios, legibilidad) para tweaks + presets + localization.
3. **License**: **MIT** (máximo alcance + permite fork corporativo). Copia WinUtil/Win11Debloat/Sophia.
4. **Distribución V1**: MSIX signed + portable .exe + Winget. V1.5 agregar Scoop/Choco.
5. **Invariante central**: todo tweak requiere `apply` + `revert` + `source` + `risk_level` validados por CI antes de merge.
6. **UI pattern**: NO monolito XAML como WinUtil. Un view por tab, con UserControls reutilizables (`TweakCard`, `PresetSelector`, `RiskBadge`).
7. **Snapshot system**: propio, en disco, con rollback granular. Complementa pero no depende de Windows Restore Point.
8. **Service companion**: servicio Windows de bajo peso para persistir timer resolution + ISLC-like + game detection. Instalable opcional.
9. **No hacer V1**: SXSC removal, ISO customization, undervolt BIOS/FIVR, driver bundling.
10. **Primero shipear**: presets gaming + snapshot + latency tab. Eso ya es diferenciador frente a WinUtil.

---

## 9. Síntesis — posicionamiento del producto

> **"La GUI moderna que WinUtil debería haber sido, con las tweaks gaming que Process Lasso cobra, el debloat granular de Sophia, la usabilidad de O&O ShutUp10, el snapshot robusto que nadie tiene, y zero dependencia de AME Wizard."**

El análisis muestra tres clusters de competencia:
1. **Debloat general** (WinUtil / Optimizer / Win11Debloat / Sophia) — nuestra V1 debe alcanzar paridad aquí.
2. **Privacy obsesivo** (ShutUp10++ / W10Privacy / Privatezilla) — paridad en cobertura pero mejor UX (color-coded + presets).
3. **Gaming latency deep** (Process Lasso / MSI Util / Timer Res / ISLC) — **aquí casi nadie lo integra** → **core diferenciador**.

El sweet spot real de producto está en fusionar los tres clusters con UX moderna (Fluent WPF), documentación transparente (estilo Atlas), reversibilidad total (estilo Sophia), y **Latency Tab / Game Profiles** como features bandera que ninguno grande tiene.

---

Sources:
- [ChrisTitusTech/winutil (GitHub)](https://github.com/ChrisTitusTech/winutil)
- [WinUtil tweaks.json (raw)](https://raw.githubusercontent.com/ChrisTitusTech/winutil/main/config/tweaks.json)
- [WinUtil main.ps1 (raw)](https://raw.githubusercontent.com/ChrisTitusTech/winutil/main/scripts/main.ps1)
- [hellzerg/optimizer (GitHub)](https://github.com/hellzerg/optimizer)
- [Atlas-OS/Atlas (GitHub)](https://github.com/Atlas-OS/Atlas)
- [Atlas removed features](https://docs.atlasos.net/faq/install-faq/removed-features/)
- [Atlas telemetry controls (DeepWiki)](https://deepwiki.com/Atlas-OS/Atlas/5.2-telemetry-controls)
- [Atlas and Security](https://docs.atlasos.net/faq/general-faq/atlas-and-security/)
- [ReviOS / Revision](https://revi.cc)
- [meetrevision/playbook](https://github.com/meetrevision/playbook)
- [ntdevlabs/tiny11builder](https://github.com/ntdevlabs/tiny11builder)
- [Raphire/Win11Debloat](https://github.com/Raphire/Win11Debloat)
- [builtbybel/privatezilla](https://github.com/builtbybel/privatezilla)
- [farag2/Sophia-Script-for-Windows](https://github.com/farag2/Sophia-Script-for-Windows)
- [W10Privacy Review (ProPrivacy)](https://proprivacy.com/privacy-service/review/w10privacy)
- [O&O ShutUp10++](https://www.oo-software.com/en/shutup10)
- [BoosterX reviews (Trustpilot)](https://www.trustpilot.com/review/boosterx.org)
- [Process Lasso (Bitsum)](https://bitsum.com/)
- [Process Lasso Gaming Mode](https://bitsum.com/docs/pl/gaming/)
- [Process Lasso v18.0](https://bitsum.com/product-update/process-lasso-v18-0-gpu-metrics-graph-aesthetics-and-more/)
- [ISLC v1.0.4.5](https://www.wagnardsoft.com/content/Download-Intelligent-standby-list-cleaner-ISLC-1045)
- [Timer Resolution FAQ](https://www.lucashale.com/timerresolution/faq/)
- [MSI Mode Utility (Grokipedia)](https://grokipedia.com/page/MSI_Mode_Utility)
- [DDU Guide (Wagnardsoft)](https://www.wagnardsoft.com/content/How-use-Display-Driver-Uninstaller-DDU-Guide-Tutorial)
- [NVIDIA Profile Inspector (Orbmu2k)](https://github.com/Orbmu2k/nvidiaProfileInspector)
- [NVCleanstall (TechPowerUp)](https://www.techpowerup.com/nvcleanstall/)
- [CRU (ToastyX)](https://www.monitortests.com/forum/Thread-Custom-Resolution-Utility-CRU)
- [ThrottleStop Guide 2026](https://www.ultrabookreview.com/31385-the-throttlestop-guide/)
- [TronScript (bmrf/tron)](https://github.com/bmrf/tron)
- [MSMG Toolkit (itechtics)](https://www.itechtics.com/msmg-toolkit/)
- [Windows 11 24H2 gaming tweaks 2026 (XDA)](https://www.xda-developers.com/im-stuck-with-windows-for-gaming-in-2026-but-heres-how-im-optimizing-it/)
- [MMCSS / SystemResponsiveness (MS Learn)](https://learn.microsoft.com/en-us/windows/win32/procthread/multimedia-class-scheduler-service)
- [Windows 11 Gaming Tuning Guide 2026](https://windowsforum.com/threads/windows-11-gaming-tuning-guide-2026-safe-consistent-performance.401517/)
- [MSI Mode registry (MS Learn)](https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/enabling-message-signaled-interrupts-in-the-registry)
- [WinUI vs WPF 2026 (CTCO)](https://www.ctco.blog/posts/winui-vs-wpf-2026-practical-comparison/)
- [Avalonia UI](https://avaloniaui.net/)
- [Guia hardware Atlas vs Revi](https://www.guiahardware.es/en/Atlases-vs.-Reviews:-Differences--Risks--and-Which-to-Choose-to-Optimize-Windows/)
- [Windows 11 debloat 24H2 guide (ProSoftKeys)](https://prosoftkeys.com/debloat-windows11-24h2/)
- [kaylerberserk/Optimizer](https://github.com/kaylerberserk/Optimizer)
- [Exoptimizer](https://github.com/MobinMardi/Exoptimizer)
- [DaddyMadu/Windows10GamingFocus](https://github.com/DaddyMadu/Windows10GamingFocus)
