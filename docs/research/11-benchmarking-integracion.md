# Benchmarking Automático Integrado — Windows 11 Gaming Optimizer

**Propósito**: Diseño técnico de la capa de benchmarking que separa esta app de la competencia. Mide antes/después con números reales, no promesas.

**Alcance**: PresentMon 2.x como capa primaria, sensores HW para contexto, UI/UX para el wizard, metodología estadística, y diseño privacy-first para telemetría opcional.

**Fecha de investigación**: Abril 2026. Versiones referenciadas: PresentMon 2.4.1 (Jan 2026) y 2.5.0 (multi-device), CapFrameX 1.8.3, FrameView 1.8.1, LibreHardwareMonitor 0.9.7+.

---

## 1. Métricas clave recomendadas — shortlist priorizado

La regla de oro: **el promedio de FPS miente; los percentiles no**. Una app que solo muestre "X FPS promedio" es indistinguible de la competencia. Lo que vende el producto es mostrar **frame pacing** y **latencia real**, porque ahí es donde los tweaks de Windows realmente se notan.

### Tier 1 — Mostrar siempre en UI principal (must-have)

| Métrica | Fuente | Por qué importa |
|---|---|---|
| **Average FPS** | PresentMon `MsBetweenPresents` | Número ancla que el usuario espera ver |
| **1% low FPS** | percentil 99 de frametimes | El "suelo" que percibe el jugador; mejoras aquí = menos stutter |
| **0.1% low FPS** | percentil 99.9 de frametimes | Micro-stutter; el tweak que lo arregla vende solo |
| **Frametime P99 (ms)** | percentil 99 directo | Más legible que "1% low" para power users |
| **Stutter count** | frames con `MsBetweenPresents > 2x median` | Conteo humano: "antes 47 hipos, ahora 3" |
| **PC Latency (ms)** | PresentMon `MsPCLatency` / `MsClickToPhotonLatency` | PresentMon 2.x expone input-to-photon; GRAN diferenciador |

### Tier 2 — Diagnóstico (tab secundaria, on demand)

| Métrica | Fuente | Por qué importa |
|---|---|---|
| **GPU Busy %** | PresentMon `MsGPUBusy / MsBetweenPresents` | Detecta CPU-bound vs GPU-bound |
| **GPU Wait %** | PresentMon `MsGPUWait` | Si es alto, tu CPU no alimenta la GPU a tiempo |
| **CPU Busy / Wait** | PresentMon `MsCPUBusy` / `MsCPUWait` | Complemento del anterior |
| **Animation Error** | PresentMon `MsAnimationError` (beta) | Detecta stutter de simulación vs presentación (GN whitepaper) |
| **Display Latency** | PresentMon `DisplayLatency` | Queue depth real al monitor |
| **Frametime stddev / CV%** | cálculo propio sobre frametimes | "Varianza" — mejor que media para comparar runs |

### Tier 3 — Contexto HW (overlay durante captura)

| Métrica | Fuente | Cómo |
|---|---|---|
| GPU clock real (MHz) | PresentMon Service telemetry / LibreHardwareMonitor | Ver throttling |
| GPU power (W) | mismo | ¿el undervolt tuvo efecto? |
| GPU / VRAM temp | mismo | Thermal story |
| CPU clock real P-cores/E-cores | LibreHardwareMonitor | Detectar core parking mal configurado |
| CPU temp per-core | LibreHardwareMonitor | Thermal throttle |
| Memory usage | WMI / PerfCounters | OOM near-miss |
| Page file activity | PerfCounters `\Paging File(_Total)\% Usage` | Commit presión |
| DPC/ISR latency | LatencyMon parse | Solo en baseline pre-tweak (no durante gaming) |

### Lo que **NO** mostrar por default

- **FPS instantáneo sin contexto**: es ruido. Usar ventanas móviles de ~1 s.
- **Overlay estilo Afterburner** durante gameplay: no es el producto; molesta.
- **FurMark-style stress numbers**: no representan gaming.

---

## 2. PresentMon 2.x — integración técnica completa

PresentMon 2.x tiene **dos modos** de integración. Elegir según fase:

### 2.1 Modo CLI (recomendado para v1 del producto)

Binario: `PresentMon-2.X.X-x64.exe` (2.4.1 en enero 2026, 2.5.0 con multi-device).

**Requisitos**:
- Windows 10 2004+ (Build 19041+)
- Admin para la primera ejecución (instala ETW session permanente vía service) o `--restart_as_admin`
- Para `MsPCLatency` / `InstrumentedLatency`: requiere flags `--track_pc_latency` y **opcionalmente** instrumentación del juego (no todos los juegos la exponen).

**Flags exhaustivos** (válidos a 2.4.x):

```
# Capture target
--process_name <name.exe>         # Repetible. Nombre del EXE
--exclude <name.exe>              # Blacklist
--process_id <pid>                # Por PID específico
--etl_file <path>                 # Analyze offline ETL

# Output
--output_file <path>              # CSV destino
--output_stdout                   # Stream a consola (pipe parseable)
--multi_csv                       # Un CSV por proceso
--no_csv                          # Sin escritura a disco
--no_console_stats                # Silencia la tabla en consola
--qpc_time                        # Timestamps en QPC ticks
--qpc_time_ms                     # QPC convertido a ms
--date_time                       # ISO datetime con ns
--exclude_dropped                 # Ignora frames no presentados
--v1_metrics                      # Schema legacy
--v2_metrics                      # Schema 2.x (recomendado)

# Recording
--hotkey ALT+SHIFT+F11            # Toggle manual
--delay <seconds>                 # Espera antes de empezar
--timed <seconds>                 # Auto-stop tras N segundos
--scroll_indicator                # Scroll-lock LED como indicador
--track_gpu_video                 # Separa video encode/decode
--no_track_display                # Omite display tracking
--no_track_input                  # Omite input latency
--no_track_gpu                    # Omite GPU metrics

# Execution
--session_name <name>             # ETW session custom
--stop_existing_session           # Mata sessions previas
--terminate_existing_session      # Mata y sale
--restart_as_admin                # Auto-elevación
--terminate_on_proc_exit          # Cierra cuando muere target
--terminate_after_timed           # Cierra tras --timed

# Beta (requieren game instrumentation en varios casos)
--track_frame_type
--track_hw_measurements           # LMT / PCAT hardware devices
--track_app_timing
--track_hybrid_present            # Cross-adapter copy
--track_pc_latency                # Input-to-photon
```

**Invocación estándar desde la app** (90 s cap, auto-stop, sin UI de consola):

```
PresentMon-2.4.1-x64.exe ^
  --process_name cs2.exe ^
  --output_file "%TEMP%\bench_baseline_20260418_141200.csv" ^
  --v2_metrics ^
  --timed 90 ^
  --delay 5 ^
  --stop_existing_session ^
  --terminate_after_timed ^
  --no_console_stats ^
  --restart_as_admin
```

Flags importantes que muchos guides olvidan:
- `--stop_existing_session` **siempre**, porque RTSS/Afterburner/overlays dejan sessions zombie.
- `--restart_as_admin` evita que el usuario tenga que saber qué hacer con UAC.
- `--delay 5` da tiempo a que el juego entre a gameplay real (no menu/loading).

### 2.2 Modo SDK (recomendado cuando el producto madure)

PresentMon 2.3.1+ expone API nativa vía `PresentMonAPI2.dll` + header `PresentMonAPI.h`. Dos formas:

1. **Link directo** a `PresentMonAPI2Loader.lib` y deployar `PresentMonAPI2Loader.dll`.
2. **LoadLibrary** dinámico de `PresentMonAPI2.dll`.

Para C#/.NET hay que hacer P/Invoke. El lifecycle es:

```
pmOpenSession()               → PM_SESSION_HANDLE
pmStartTrackingProcess(pid)
pmRegisterDynamicQuery(...)   → PM_DYNAMIC_QUERY_HANDLE
  (o pmRegisterFrameQuery)
loop { pmPollDynamicQuery() / pmConsumeFrames() }
pmStopTrackingProcess()
pmCloseSession()
```

Tres tipos de query:
- **Dynamic**: métricas agregadas (promedios móviles, percentiles) sobre ventana configurable. Ideal para overlay/UI en vivo.
- **Static**: `pmQueryMetric()` one-shot.
- **Frame**: raw frame-by-frame, equivalente al CSV.

Introspección: `pmGetIntrospectionRoot()` devuelve todas las métricas disponibles (para construir UI dinámica que se adapte a versiones nuevas de PresentMon).

**Recomendación de roadmap**: v1 usa CLI + CSV parse (menos superficie de bugs, más fácil de diagnosticar). v2+ migra a SDK para overlay en vivo sin tener que leer CSVs parcialmente escritos.

### 2.3 CSV schema v2 (columnas relevantes)

El CSV se escribe frame-a-frame. Columnas clave del schema v2:

```
Application, ProcessID, SwapChainAddress, PresentRuntime, SyncInterval,
PresentFlags, AllowsTearing, PresentMode, FrameType,
CPUStartTime | CPUStartQPC | CPUStartQPCTime | CPUStartDateTime,
MsCPUBusy, MsCPUWait,
MsGPULatency, MsGPUTime, MsGPUBusy, MsGPUWait, VideoBusy,
DisplayLatency, DisplayedTime,
MsAnimationError, AnimationTime,
MsClickToPhotonLatency, MsAllInputToPhotonLatency, InstrumentedLatency,
MsBetweenPresents, MsInPresentAPI, MsBetweenDisplayChange, MsUntilDisplayed,
MsRenderPresentLatency, MsBetweenSimulationStart, MsPCLatency, MsBetweenAppStart
```

Todos los tiempos en **milisegundos**. Un frame displayed = una row. Rows con `DisplayedTime` vacío = frame dropeado (usar `--exclude_dropped` si no los quieres).

`PresentMode` values observados: `Hardware: Legacy Flip`, `Hardware: Legacy Copy to front buffer`, `Hardware: Independent Flip`, `Composed: Flip`, `Hardware Composed: Independent Flip`, `Composed: Copy with GPU GDI`, `Composed: Copy with CPU GDI`. Un objetivo tácito de "tweaks gaming" es empujar al juego de *Composed: Flip* a *Hardware: Independent Flip* (menos latencia del DWM).

### 2.4 PresentMon Service (opcional pero recomendado)

El **servicio** (`PresentMonService.exe`) combina:
- ETW frame events (lo mismo que la CLI)
- Hardware telemetry via NVAPI, ADL (AMD), IGCL (Intel)

Se instala una vez, corre como Windows Service, y expone la API por named pipe. Clientes se conectan con `pmOpenSession()` o `pmOpenSessionWithPipe()`.

**Ventaja sobre la CLI**: no hay que spawn/kill processes por cada captura, no hay CSV parcial que parsear, puedes leer GPU temp/power desde el mismo API sin depender de LibreHardwareMonitor para esas dos.

**Desventaja**: una instalación más (service), más complejidad de permisos.

**Recomendación**: **v1 con CLI, v2 con service**. El service es el camino correcto a largo plazo porque unifica frame events + HW telemetry bajo un mismo API y el overhead de overlay en tiempo real es mucho menor.

### 2.5 Parse del CSV — snippet C#

```csharp
// Usar CsvHelper (NuGet: CsvHelper)
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public sealed class FrameRow
{
    public string Application { get; set; }
    public int ProcessID { get; set; }
    public double MsBetweenPresents { get; set; }
    public double MsGPUBusy { get; set; }
    public double MsGPUWait { get; set; }
    public double MsCPUBusy { get; set; }
    public double MsCPUWait { get; set; }
    public double MsPCLatency { get; set; }
    public double MsAnimationError { get; set; }
    public string PresentMode { get; set; }
    // ... resto
}

public static List<FrameRow> Load(string csvPath)
{
    var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HeaderValidated = null,          // schema cambia entre versiones
        MissingFieldFound = null,        // tolerar columnas nuevas/viejas
        BadDataFound = null
    };
    using var reader = new StreamReader(csvPath);
    using var csv = new CsvReader(reader, cfg);
    return csv.GetRecords<FrameRow>().ToList();
}
```

En **PowerShell** (útil para scripting rápido antes de tener la app C# completa):

```powershell
$rows = Import-Csv "baseline.csv"
$frametimes = $rows | Where-Object { $_.MsBetweenPresents } |
              ForEach-Object { [double]$_.MsBetweenPresents }
$avgFps = 1000.0 / ($frametimes | Measure-Object -Average).Average
```

---

## 3. Workflow before/after — paso a paso técnico

### 3.1 Secuencia completa

```
[1] SYSTEM CHECK
    - Detectar GPU vendor (NVAPI probe, ADL probe, IGCL probe)
    - Detectar anticheat instalado (Vanguard, EAC, BattlEye) — ver §7
    - Verificar PresentMon service installed; si no, ofrecer install
    - Verificar admin rights
    - Verificar juego target instalado

[2] BASELINE RECORD
    - Usuario selecciona juego de dropdown (auto-detectado)
    - App muestra instrucciones per-juego (ver §6)
    - Pre-run checklist:
        □ Monitor en modo nativo (Hz y resolución)
        □ Overlays externos cerrados (RTSS opcional, Afterburner opcional)
        □ No hay Discord/Chrome en background
        □ Plan de energía en High Performance / Ultimate
    - Launch juego (automatizado via Steam/EGS CLI o manual)
    - Warmup 30-60 s (primer run siempre es peor, thermal + JIT shader cache)
    - Start PresentMon capture (90 s, --timed)
    - Recording indicator (scroll lock LED + UI badge)
    - Stop automático

[3] APPLY TWEAKS
    - Aplicar preset (Competitivo / Balanced / Visual)
    - Algunos tweaks requieren reboot o re-login → UI detecta y guía
    - Estado: "Tweaks aplicados, por favor reinicia el juego"

[4] POST-TWEAK RECORD
    - Mismo workflow, mismo juego, misma escena
    - Warmup 30-60 s
    - Capture 90 s
    - Stop automático

[5] COMPARE & REPORT
    - Cálculo percentiles, deltas, verdict
    - UI de comparación (§9)
    - Export opcional

[6] ITERATE (opcional)
    - "Probar otro preset" → loop a [3]
    - Historial acumulativo
```

### 3.2 Variance control (§8 expande esto)

- **Mínimo 3 runs** por configuración. Reportar mediana + IQR, no media.
- **Warmup 30 s** antes del run que cuenta. El primer run de CS2/Apex es ~3-5% peor por shader compile.
- **Thermal soak**: advertir al usuario si el run #1 tiene GPU temp > target (ej. >80°C en GPUs consumer) — sugerir esperar 60 s.
- **Descartar runs con CV% > 15%** automáticamente y pedir re-run.

### 3.3 Estructura de archivos (sugerida)

```
%APPDATA%\GamingOptimizer\
├── captures\
│   ├── 2026-04-18_14-12-00_cs2_baseline_run1.csv
│   ├── 2026-04-18_14-12-00_cs2_baseline_run2.csv
│   ├── 2026-04-18_14-12-00_cs2_baseline_run3.csv
│   ├── 2026-04-18_14-25-00_cs2_post-competitive_run1.csv
│   └── ...
├── sessions\
│   └── 2026-04-18_14-12-00.json   # metadata: HW, preset, tweaks aplicados
├── reports\
│   └── 2026-04-18_14-12-00.pdf
└── config.json
```

Cada sesión como JSON con metadata reproducible (hardware, driver version, tweaks, scene, timestamps) para poder **rehacer un compare a posteriori** y para export a cloud si el usuario opt-in.

---

## 4. UI del wizard de benchmark (mockup descriptivo)

El wizard es **tipo stepper**, no modal — porque entre pasos el usuario tiene que alt-tab al juego.

### Step 0 — Selector de escenario
```
┌─ Run Benchmark ──────────────────────────────────────────┐
│                                                          │
│ ¿Qué juego quieres medir?                                │
│                                                          │
│  [▼] Counter-Strike 2                                    │
│      • Detectado en Steam (C:\...\cs2.exe)               │
│      • Preset: "FPS Benchmark (Ancient)" — 100 s         │
│      • Scene: workshop_id:3472126051                     │
│                                                          │
│ ¿Número de runs por captura?                             │
│  (o) 3  ( ) 5  ( ) 1 (solo diagnóstico, no comparable)  │
│                                                          │
│ Duración de cada run:                                    │
│  [========|===] 90 s  (recomendado 60-120)               │
│                                                          │
│               [ Cancel ]   [ Start Baseline → ]          │
└──────────────────────────────────────────────────────────┘
```

### Step 1 — Captura en vivo
```
┌─ Baseline Run 1/3 ───────────────────────── 00:47 / 01:30┐
│                                                          │
│  ● REC   Scroll Lock parpadeando                         │
│                                                          │
│  FPS en vivo:   ┌─────────────────────────────────────┐  │
│                 │       ╱╲    ╱╲╱╲                     │  │
│                 │ ╱╲╱╲╱╱  ╲╱╲╱    ╲╱╲╱╲╱╲╱╲╱          │  │
│                 │                                      │  │
│                 └─────────────────────────────────────┘  │
│                  0                                     90s│
│                                                          │
│  Live stats (último 10 s):                               │
│    avg 247 fps    min 189    max 312                     │
│    GPU 91%    CPU 54%    GPU temp 68°C                   │
│                                                          │
│               [ Abort ]                                  │
└──────────────────────────────────────────────────────────┘
```

El live graph se alimenta por:
- v1: tailing del CSV parcial cada 500 ms (funciona pero hacky)
- v2: frame query del SDK con ventana móvil

### Step 2 — Aplicar preset
```
┌─ Apply Optimization ─────────────────────────────────────┐
│                                                          │
│  Baseline capturado (3/3 runs, CV 3.1%). ✓               │
│                                                          │
│  Selecciona preset a aplicar:                            │
│                                                          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐      │
│  │ Competitivo  │ │   Balanced   │ │    Visual    │      │
│  │              │ │              │ │              │      │
│  │ 47 tweaks    │ │ 28 tweaks    │ │ 12 tweaks    │      │
│  │ Máx FPS      │ │ Equilibrio   │ │ Calidad      │      │
│  │              │ │              │ │              │      │
│  │   [Apply]    │ │   [Apply]    │ │   [Apply]    │      │
│  └──────────────┘ └──────────────┘ └──────────────┘      │
│                                                          │
│  ⓘ Algunos tweaks requieren reiniciar sesión            │
└──────────────────────────────────────────────────────────┘
```

### Step 3 — Post-tweak run (idéntico a Step 1)

### Step 4 — Resultados (§9 detalla)

---

## 5. Librerías de gráficos — comparativa

Las tres opciones son válidas. La decisión depende de **cuán responsivo** debe sentirse el overlay/live view.

| Lib | Lic | Aesthetics | Live data | Complejidad | WPF | Avalonia |
|---|---|---|---|---|---|---|
| **ScottPlot** | MIT | Buena, estilo científico | Excelente (>100k pts) | Baja | Sí | Sí |
| **LiveCharts2** | MIT | Mejor (animaciones) | Buena | Media-alta (MVVM estricto) | Sí | Sí |
| **OxyPlot** | MIT | Básica, funcional | OK | Baja | Sí | Parcial |
| **SciChart** | Comercial | Excelente | Excelente | Baja | Sí | Sí |

### Recomendación

- **Live FPS overlay durante captura**: **ScottPlot**. La API `AddDataStreamer` está diseñada exactamente para ese caso. Cero fricción.
- **Reportes estáticos (histogramas, comparación antes/después)**: **ScottPlot** sigue siendo válido y mantiene consistencia. Si necesitas animaciones al mostrar los resultados (efecto "wow"), **LiveCharts2** ahí brilla.
- **Export a PNG/PDF**: ambos lo hacen; ScottPlot es más directo.

### Snippet ScottPlot (live data + WPF)

```csharp
// XAML
// <WpfPlot Name="FpsPlot" />

private readonly System.Timers.Timer _tick = new(500);
private ScottPlot.DataStreamer _fpsStream;
private ScottPlot.DataStreamer _frametimeStream;

private void InitLiveCharts()
{
    var plt = FpsPlot.Plot;
    plt.Title("Live FPS");
    plt.Axes.Bottom.Label.Text = "Seconds";
    plt.Axes.Left.Label.Text = "FPS";

    _fpsStream = plt.Add.DataStreamer(200);      // 200 points rolling
    _fpsStream.ViewSlide();                       // slide instead of wrap
    _tick.Elapsed += PushSample;
    _tick.Start();
}

private void PushSample(object? s, ElapsedEventArgs e)
{
    // _latestFps se alimenta desde el tail del CSV o del SDK
    Dispatcher.Invoke(() =>
    {
        _fpsStream.Add(_latestFps);
        FpsPlot.Refresh();
    });
}
```

### Snippet: histograma comparativo antes/después

```csharp
var plt = ResultPlot.Plot;
var beforeHist = ScottPlot.Statistics.Histogram.WithBinCount(30, beforeFrametimes);
var afterHist  = ScottPlot.Statistics.Histogram.WithBinCount(30, afterFrametimes);

var b = plt.Add.Bars(beforeHist.Bins, beforeHist.Counts);
b.Color = Colors.Red.WithOpacity(0.5);
b.LegendText = "Baseline";

var a = plt.Add.Bars(afterHist.Bins, afterHist.Counts);
a.Color = Colors.Green.WithOpacity(0.5);
a.LegendText = "Optimized";

plt.Axes.Bottom.Label.Text = "Frametime (ms)";
plt.Axes.Left.Label.Text = "Frame count";
plt.ShowLegend();
ResultPlot.Refresh();
```

---

## 6. Scenarios de benchmark per-juego

### 6.1 Counter-Strike 2

**Recomendación primaria**: Workshop Map "CS2 FPS BENCHMARK" (IDs conocidos: 3240880604 Dust2, 3472126051 Ancient). Son **deterministas**: script interno hace bot match + smokes + gunfire + grenades en timing fijo, termina mostrando resultado en consola. Hay un proyecto open-source `Ark0N/-CS2-Benchmark-Automation` que integra esta aproximación con CapFrameX vía AutoHotkey.

**Launch options para benchmark** (añadir en Steam launch options):
```
-novid -high +fps_max 0 +cl_showfps 0 +mat_queue_mode 2
```

**Scene del PresentMon capture**:
- Subir al mapa, esperar que termine loading + player spawn.
- Delay 6-10 s para estabilizar.
- Capture 90-100 s.
- El mapa termina antes; trim si hace falta.

**Sin workshop map (fallback)**:
- Casual match en Dust2, lado CT, buy round + 2 frag round.
- Más ruidoso. Requiere 5+ runs para converger. **Descartado como default**.

### 6.2 Valorant

Valorant es **el escenario problemático** por Vanguard (kernel-level, driver a boot). No hay `-benchmark` flag. No hay Workshop. Los replays son cliente-only y no expone API.

**Recomendación primaria**: Custom Game en **The Range**.
- Settings: `Practice Mode → The Range`
- Scene: "Shooting Test" (Hard) — 30 bots, scripted spawn. Dura ~90 s con jugador promedio.
- Repeatable porque los spawn points son fijos.

**Alternativa de carga alta**: Deathmatch as proxy. Más variance (descartar si CV% > 15%).

**Precaución anti-cheat**: PresentMon **usa ETW de kernel sessions** y corre con admin. Vanguard tiene historial de matar processes con debug privileges. Ver §7; protocol de test: 1) correr PresentMon CLI antes de Valorant, 2) lanzar Valorant, 3) si se cierra → no soportado con ese setup.

**Launch options**: Valorant no acepta launch options custom vía Riot Client; solo los que el cliente provee internamente.

### 6.3 Apex Legends

**Recomendación primaria**: Firing Range.
- Scene: spawn → primer target rack.
- **Warm-up mandatorio**: 60 s (Source engine shader cache es brutal).
- Capture 90 s.
- Variance baja, ~2-4% típico.

**Launch options útiles** (via Steam/Origin properties):
```
-novid +fps_max unlimited -forcenovsync -high -freq 240 -preload -eac_launcher_settings SettingsDX12.json
```
(`-freq` al refresh nativo; `-preload` reduce el primer-run penalty; DX12 opcional).

**No hay** `-benchmark` flag oficial. El `+cl_showfps 4` activa overlay interno pero es informativo, no un benchmark mode.

**Precaución anti-cheat**: EAC en modo user-mode (no kernel como Vanguard). PresentMon funciona. No se han reportado bans por PresentMon ni por FrameView. Comunidad lo usa rutinariamente.

### 6.4 Otros juegos soportados out-of-the-box

| Juego | Built-in bench | Notas |
|---|---|---|
| Cyberpunk 2077 | Sí (`Settings → Graphics → Benchmark`) | Ideal para GPU-heavy |
| Shadow of the Tomb Raider | Sí | Gold standard de reviewers |
| Forza Horizon 5 | Sí | CPU-heavy |
| Red Dead Redemption 2 | Sí | Ultra-detailed |
| Hitman 3 / WoA | Sí (Dartmoor benchmark) | CPU+GPU mix |
| 3DMark Time Spy | Comando | Pago, metodología estándar |

Para **first release**, recomiendo soportar: **CS2, Valorant, Apex, Fortnite, Cyberpunk 2077, Shadow of TR**. Cubre 80% del uso esperado.

### 6.5 Sintéticos cross-game

| Tool | Tipo | CLI | Uso recomendado |
|---|---|---|---|
| **3DMark Time Spy** | DX12 GPU | Sí (`3DMarkCmd.exe --definition=...`), requiere Pro | Bench sistémico, normalizar comparación HW |
| **3DMark Port Royal** | DXR | Sí, Pro | RT-specific |
| **3DMark CPU Profile** | CPU | Sí, Pro | Eval de CPU tweaks per-thread |
| **Unigine Superposition** | DX12/Vulkan | Sí, Pro ($) | GPU, escena real |
| **Unigine Heaven** | DX11 | Limitado | Legacy, OK baseline |
| **Cinebench R24** | CPU render | Sí | No gaming; excluir |

Integración sugerida: **PCMark 10 + 3DMark Time Spy** como "system-level impact" para tweaks que no se notan en juego específico (timer resolution, core parking, file system tweaks).

### 6.6 Benchmarks no-gaming (sistema global)

- **BootRacer** (freeware) — mide tiempo de boot. Valioso para tweaks de startup items.
- **Crystal DiskMark** — SSD quick test.
- **AIDA64 Memory Benchmark** — latencia RAM (ns). Tweaks de memoria (XMP, subtimings) se notan aquí.
- **PCMark 10** — productividad.
- **UserBenchmark** — **evitar**. La comunidad tiene fuerte sesgo negativo contra ellos (scoring sesgado pro-Intel histórico 2019-2023, credibilidad dañada). Si se integra, warning explícito.

---

## 7. Anti-cheat compatibility

Este es **el tema espinoso**. No hay statement oficial de Riot/Epic/BattlEye sobre PresentMon específicamente. Lo que sí se sabe:

| Anti-cheat | Juegos | Modo | Compat PresentMon | Notas |
|---|---|---|---|---|
| **EAC** | Apex, Fortnite, Rust, DBD | User-mode (mayoría) | **OK**, uso rutinario | GamersNexus, HUB, reviewers lo usan diariamente |
| **BattlEye** | PUBG, R6S, DayZ, EFT | Kernel | **OK mayoría**; reportes ocasionales en PUBG | Nunca ban; puede que logee |
| **Vanguard** | Valorant, LoL | Kernel + boot driver | **Riesgo bajo, no zero** | Historia de matar MSI Afterburner, Cheat Engine; PresentMon como tal no ha sido reportado como killed. Pero **no es oficialmente soportado** |
| **RICOCHET** | CoD MW/Warzone | Kernel | **OK** históricamente | Activision ha sido tolerante con PresentMon/FrameView |
| **FACEIT AC** | CS2 competitive | Kernel | **Precaución** | Similar paranoia a Vanguard |

### Recomendaciones prácticas

1. **UI debe advertir** antes de baseline en juegos con kernel AC: "Este juego usa Vanguard/FACEIT. PresentMon requiere privilegios que pueden interactuar con el anti-cheat. Recomendamos benchmark en Unrated/Casual, no Competitive." El riesgo real de ban por frame-capture es cercano a 0, pero el riesgo de false-positive kick es real.

2. **Priorizar PresentMon CLI** por sobre service para juegos kernel-AC: sesión ETW temporal vs servicio permanente es menos "signature" detectable.

3. **No usar `--restart_as_admin`** mientras el juego corre. Arrancar PresentMon **primero**, luego el juego.

4. **Dejar opt-out**: el usuario puede marcar "no ejecutar PresentMon automáticamente en Valorant", y usar solo built-in overlay del juego como fallback.

5. **FrameView (NVIDIA)**: NVIDIA explícitamente documenta que es safe con EAC/BattlEye. Para Valorant: **mismo hole legal que PresentMon**. Como FrameView envuelve a PresentMon, el perfil de riesgo es idéntico.

---

## 8. Variance control — metodología estadística

### 8.1 Problema

Benchmark gaming tiene ruido alto:
- **Thermal state** (primer run frío, runs 4+ pueden throttle si sostenido)
- **Background processes** (Windows Update arrancó entre runs)
- **Game state** (bot positions, asset streaming, fog of war)
- **Driver state** (shader cache cold/hot)

Un run único **no es benchmark**, es una muestra. Tirar conclusiones de un run es una de las señales de "app sospechosa" (lo hacen Hone, Advanced System Care, etc.).

### 8.2 Metodología recomendada (default en el producto)

1. **Warmup run**: 30-60 s descartados. Deja el shader cache caliente.
2. **3 runs mínimo** de la medición real.
3. **Reportar mediana + IQR** (intercuartil range), no media + stddev. Robusto a outliers.
4. **Calcular CV% = stddev / mean × 100**. Si CV% > 15% → pedir re-run automáticamente.
5. **Cooldown mínimo 30 s** entre runs (GPU vuelva a temp nominal).
6. **Detectar thermal throttle**: si GPU clock durante run N < (GPU clock durante run 1) × 0.95, flag el run y pedir repeat tras cooldown más largo.

### 8.3 Fórmulas

Para N frametimes ordenados de menor a mayor:

```
Average FPS           = N × 1000 / sum(frametimes)
Median frametime      = frametimes[N/2]
P99 frametime         = frametimes[floor(0.99 × N)]
P99.9 frametime       = frametimes[floor(0.999 × N)]
1% low FPS (avg)      = 1000 / mean(frametimes[floor(0.99 × N) .. N])
0.1% low FPS (avg)    = 1000 / mean(frametimes[floor(0.999 × N) .. N])
1% low FPS (percentil)= 1000 / P99_frametime
Stutter count         = count(frametimes where ft > 2 × median)
Frametime CV%         = stddev(frametimes) / mean(frametimes) × 100
```

**Nota crítica**: hay dos convenciones para "1% low". CapFrameX ofrece ambas:
- **"x% low average"** (el de GamersNexus): promedio de los peores x%. Más conservador, más común en reviews.
- **"x% low integral"** (basado en tiempo, el de MSI Afterburner): FPS tal que el frame estuvo bajo él x% del tiempo total.

**Decisión de producto**: por consistencia con reviewers, usar **"1% low average"** como default. Ofrecer toggle en settings.

### 8.4 Snippet de percentiles en C#

```csharp
public sealed class FrameStats
{
    public double AvgFps { get; init; }
    public double MedianFrametimeMs { get; init; }
    public double P99FrametimeMs { get; init; }
    public double P999FrametimeMs { get; init; }
    public double OnePercentLowFps { get; init; }
    public double ZeroOnePercentLowFps { get; init; }
    public int StutterCount { get; init; }
    public double CoefficientOfVariation { get; init; }
}

public static FrameStats Compute(IReadOnlyList<double> frametimesMs)
{
    if (frametimesMs.Count < 100)
        throw new InvalidOperationException("Run too short for reliable stats");

    var sorted = frametimesMs.OrderBy(x => x).ToArray();
    int n = sorted.Length;

    double sum = 0;
    foreach (var f in sorted) sum += f;
    double mean = sum / n;

    double sqDev = 0;
    foreach (var f in sorted) sqDev += (f - mean) * (f - mean);
    double stddev = Math.Sqrt(sqDev / n);

    int p99Idx  = (int)Math.Floor(0.99 * n);
    int p999Idx = (int)Math.Floor(0.999 * n);

    double worstOnePct   = sorted.Skip(p99Idx).Average();
    double worstTenthPct = sorted.Skip(p999Idx).Average();

    double median = sorted[n / 2];
    int stutters = frametimesMs.Count(f => f > 2.0 * median);

    return new FrameStats
    {
        AvgFps = 1000.0 / mean,
        MedianFrametimeMs = median,
        P99FrametimeMs = sorted[p99Idx],
        P999FrametimeMs = sorted[p999Idx],
        OnePercentLowFps = 1000.0 / worstOnePct,
        ZeroOnePercentLowFps = 1000.0 / worstTenthPct,
        StutterCount = stutters,
        CoefficientOfVariation = stddev / mean * 100.0
    };
}
```

### 8.5 Mismo en PowerShell (para scripts)

```powershell
function Get-FrameStats {
    param([double[]]$Frametimes)
    $sorted = $Frametimes | Sort-Object
    $n      = $sorted.Count
    $mean   = ($sorted | Measure-Object -Average).Average
    $std    = [Math]::Sqrt( ($sorted | ForEach-Object { [Math]::Pow($_ - $mean, 2) } |
                             Measure-Object -Sum).Sum / $n )
    $p99    = $sorted[[int][Math]::Floor(0.99 * $n)]
    $p999   = $sorted[[int][Math]::Floor(0.999 * $n)]
    $median = $sorted[[int]($n/2)]
    $stutters = ($Frametimes | Where-Object { $_ -gt 2 * $median }).Count

    [PSCustomObject]@{
        AvgFps      = 1000.0 / $mean
        Median      = $median
        P99ft       = $p99
        P999ft      = $p999
        OnePctLow   = 1000.0 / (($sorted | Select-Object -Skip ([int][Math]::Floor(0.99*$n)) |
                                Measure-Object -Average).Average)
        Stutters    = $stutters
        CVpct       = $std / $mean * 100
    }
}
```

---

## 9. Reporting UI — comparación antes/después

### 9.1 Principio de diseño

- **Verdict arriba, números abajo, gráficos expandibles**.
- Mostrar **delta absoluto + porcentaje** siempre.
- Código de color: verde (mejora significativa, >3%), gris (±3%), rojo (regresión >3%).
- **"Statistical significance"** si CV% lo permite. Si no, warning "runs noisy, delta puede no ser real".

### 9.2 Mockup

```
┌─ Benchmark Results — CS2 ────────────────────────────────┐
│                                                          │
│   VERDICT: MEJORA SIGNIFICATIVA  (+8.3% average)         │
│                                                          │
│  ┌────────────────────────┬──────────┬──────────┬──────┐ │
│  │ Metric                 │ Baseline │ Optimized│ Δ    │ │
│  ├────────────────────────┼──────────┼──────────┼──────┤ │
│  │ Avg FPS                │   238.4  │  258.2   │+8.3% │ │
│  │ 1% low FPS             │   162.1  │  198.5   │+22% ✓│ │
│  │ 0.1% low FPS           │   118.0  │  161.2   │+37% ✓│ │
│  │ P99 frametime          │   6.17ms │  5.03ms  │-18% ✓│ │
│  │ Stutters (>2× median)  │    47    │    3     │-94% ✓│ │
│  │ PC Latency (ms)        │   23.1   │  18.9    │-18% ✓│ │
│  │ GPU Busy %             │   91.2   │   94.6   │+3.7% │ │
│  │ CV%                    │   3.1    │   2.8    │ ok   │ │
│  └────────────────────────┴──────────┴──────────┴──────┘ │
│                                                          │
│  ▼ Frametime distribution (expandir)                     │
│  ▼ Live graph overlay (expandir)                         │
│  ▼ Run details 1/3, 2/3, 3/3 (expandir)                  │
│                                                          │
│  [Export PDF]  [Export PNG]  [Share anonymously]         │
│  [Run again]   [Try another preset]                      │
└──────────────────────────────────────────────────────────┘
```

### 9.3 Gráficos del detail view

1. **Frametime overlay** (el más "famoso"): misma gráfica de CapFrameX, dos líneas superpuestas (baseline rojo semi-transparente, optimized verde). Eye-candy.
2. **Histograma de frametimes**: igual, dos bars stackeados por bin.
3. **Percentile curve**: frametime (y) vs percentile (x 0–100%). Las curvas que van más "abajo" son mejores. Muy efectiva para ver mejoras en tails.
4. **Per-run breakdown**: bar chart con 3 runs de cada, para ver variance.

### 9.4 Export

- **PDF** (via `iTextSharp` o `QuestPDF`): una página, verdict + tabla + gráfico principal + metadata HW.
- **PNG**: screenshot plano del result view, shareable en Discord/Reddit/Twitter.
- **JSON**: sidecar con todos los datos (para power users que quieren procesar fuera).

### 9.5 Verdict logic

```csharp
public static string ComputeVerdict(FrameStats baseline, FrameStats optimized)
{
    double avgDelta      = (optimized.AvgFps - baseline.AvgFps) / baseline.AvgFps * 100;
    double onePctDelta   = (optimized.OnePercentLowFps - baseline.OnePercentLowFps)
                           / baseline.OnePercentLowFps * 100;
    double latencyDelta  = (optimized.PCLatencyMs - baseline.PCLatencyMs)
                           / baseline.PCLatencyMs * 100;

    bool noisy = baseline.CoefficientOfVariation > 10 ||
                 optimized.CoefficientOfVariation > 10;

    // Ponderar 1% low más que avg; latencia también cuenta
    double compositeScore = avgDelta * 0.3 + onePctDelta * 0.5 + (-latencyDelta) * 0.2;

    if (noisy)
        return "Runs ruidosos — delta no confiable. Repite con menos apps en background.";
    if (compositeScore > 5)  return "Mejora significativa";
    if (compositeScore > 2)  return "Mejora marginal";
    if (compositeScore > -2) return "Sin cambio estadísticamente significativo";
    if (compositeScore > -5) return "Regresión marginal";
    return "Regresión — revertir preset recomendado";
}
```

---

## 10. Telemetría opcional — diseño privacy-first

### 10.1 Principios

1. **Opt-in explícito**. Default OFF.
2. **Sin PII**. Nunca nombre de usuario, hostname, ni IP del cliente (el backend no logea IPs o las ofusca inmediatamente).
3. **HW como hash**: enviar `CPU model, GPU model, RAM GB, OS build` — no serials.
4. **Open source backend**. El usuario puede ver qué se envía y dónde.
5. **Export del payload antes de enviar**, en UI.
6. **Kill switch**: un toggle en Settings desactiva y borra la `installId` anónima.

### 10.2 Payload mínimo

```json
{
  "installId": "c8c7a5e1-...-anon-uuid-v4",
  "appVersion": "1.2.3",
  "timestamp": "2026-04-18T14:30:00Z",
  "hardware": {
    "cpu": "AMD Ryzen 7 9800X3D",
    "cpuCoreCount": 8,
    "gpu": "NVIDIA GeForce RTX 4080",
    "gpuDriver": "572.61",
    "ramGB": 32,
    "ramSpeedMTs": 6000,
    "os": "Windows 11 24H2",
    "osBuild": "26100.3194"
  },
  "session": {
    "game": "cs2",
    "scene": "workshop:3472126051",
    "preset": "Competitive",
    "runs": 3,
    "captureDurationSec": 90
  },
  "tweaksApplied": ["hag_off", "mouse_acc_off", "core_park_off", "timer_res_0_5ms", "..." ],
  "baseline": {
    "avgFps": 238.4,
    "onePctLowFps": 162.1,
    "p99FrametimeMs": 6.17,
    "pcLatencyMs": 23.1,
    "stutterCount": 47,
    "cvPct": 3.1
  },
  "optimized": {
    "avgFps": 258.2,
    "onePctLowFps": 198.5,
    "p99FrametimeMs": 5.03,
    "pcLatencyMs": 18.9,
    "stutterCount": 3,
    "cvPct": 2.8
  }
}
```

### 10.3 Backend stack recomendado (self-host friendly)

Tres arquitecturas a elegir según ambición:

1. **Minimalista (v1)**: Supabase Postgres + Edge Function. Una tabla, un endpoint. Queries manuales o Grafana encima. 0-20 USD/mes hasta 10k users.
2. **Analíticas web-style**: **Umami** o **Plausible** self-host. Genial para contar eventos (qué preset aplican más), malo para storear full payloads.
3. **Producto completo**: **PostHog** self-host. Dashboards built-in, funnels, A/B de presets, user-journey. Sobrado si el objetivo es product analytics.

**Recomendación**: Supabase + Postgres + Grafana. La razón: los datos son 90% numéricos agregables (SQL queries del estilo `SELECT AVG(delta_avg_fps) GROUP BY cpu, gpu`), que es exactamente donde SQL brilla. Plausible/Umami son optimizados para URL hits, no para payloads de benchmark. PostHog es overkill para un solo tipo de evento.

### 10.4 Community page

"Para tu 9800X3D + RTX 4080, 1,247 usuarios han probado este preset. Ganancia promedio: **+8.5% avg FPS, +19% 1% low**. 93% positivo." Eso es el valor real del opt-in — el usuario ve qué esperar de los tweaks antes de aplicarlos.

### 10.5 Legal / GDPR

- Política de privacidad a un click en el toggle del opt-in.
- Artículo 6(1)(a) GDPR: consent. El install ID anónimo evita el territorio gris de "pseudonymous data".
- Data retention: 12 meses.
- `DELETE /api/install/{installId}` endpoint público para ejercer "right to erasure".

---

## 11. Integración con sensores HW durante captura

PresentMon Service expone algunos sensors (GPU busy/power/temp) vía su API. Pero CPU per-core, RAM activity, storage queue no. Para eso:

### 11.1 LibreHardwareMonitor (recomendado)

NuGet: `LibreHardwareMonitorLib`. Compatible con .NET 8/9/10 + Framework 4.7.2.

```csharp
using LibreHardwareMonitor.Hardware;

public class HwSampler : IDisposable
{
    private readonly Computer _pc;
    public HwSampler()
    {
        _pc = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true,
        };
        _pc.Open();
    }

    public IEnumerable<SensorSample> Sample()
    {
        foreach (var hw in _pc.Hardware)
        {
            hw.Update();
            foreach (var s in hw.Sensors)
                if (s.Value.HasValue)
                    yield return new SensorSample(hw.Name, s.Name, s.SensorType, s.Value.Value);
        }
    }

    public void Dispose() => _pc.Close();
}

public record SensorSample(string Hardware, string Sensor, SensorType Type, float Value);
```

**Requiere admin**. Manifest con `requireAdministrator` o `--restart_as_admin` equivalente.

### 11.2 HWiNFO64 Shared Memory (alternativa)

Si el usuario ya tiene HWiNFO64 corriendo (muchos enthusiasts lo hacen), leer su shared memory es más eficiente que bootear nuestro propio motor. Librería C# recomendada: **HWHash** (NuGet, open source, sub-ms para iterar 300+ sensores).

```csharp
using HWHash;
HWHash.Start();  // singleton, arranca reader del shared mem
var snapshot = HWHash.GetSensorsAsList();
var cpuTemp  = snapshot.FirstOrDefault(s => s.Name.Contains("CPU Package"))?.Value;
```

**Caveat**: HWiNFO Shared Memory es **feature pago** en HWiNFO Pro desde v7.0 (o de-habilitado cada 12 h en Free). La lib funciona, pero depende del setup del usuario.

### 11.3 Recomendación producto

**LibreHardwareMonitor como primario** (siempre funciona out-of-the-box, NuGet, MIT). HWiNFO como fallback opcional detectable automáticamente (si el user lo tiene, usarlo por mejor calidad de lecturas en chipsets raros).

### 11.4 Sample rate durante benchmark

- **Frame stats (PresentMon)**: per-frame (240+ events/s en juegos rápidos).
- **HW sensors**: **1 Hz** es suficiente. Más es CPU wasted y no agrega info.
- **Merge**: en post-proceso, asignar a cada frame la lectura HW más cercana en el tiempo.

---

## 12. Referencias a precedentes / investigación competitiva

### Precedentes similares

- **3DMark Professional Edition** — el estándar oro en automation (CLI + XML profile). Aprender: su archivo `.3dmdef` es una definición declarativa de un benchmark. Nuestra app puede tener `.gobench` por juego.
- **UserBenchmark** — crowdsource hardware. Anti-precedente: credibilidad dañada por sesgos en scoring. Aprender: **no** inventar un "score único"; mostrar métricas desagregadas.
- **PCMark 10** — productividad. Aprender: separar "batterylife", "gaming", "productivity" en runs distintos.
- **Hone.gg** — competidor directo más cercano. Recomienda **CapFrameX externo** para bench. Oportunidad: internalizar esa capa.
- **GamersNexus methodology whitepaper 2024-2025** — referencia absoluta. Seguir sus conventions de reporting (P99, 1% low average, frametime variance explícito).
- **Tom's Hardware / HWUnboxed protocols** — test scene docs. Buena fuente para scenes per-juego reproducibles.

### Community research a reutilizar

- `Ark0N/-CS2-Benchmark-Automation` (GitHub) — AutoHotkey + CapFrameX glue. Ver por diseño de timing + workshop map integration.
- `CXWorld/CapFrameX` — formato JSON de capture es referencia; podemos importar/exportar `.cfx` para interop.
- `GameTechDev/PresentMon` — PresentMon es el backbone; seguir releases, adaptar schema a versiones nuevas.

---

## 13. Roadmap de implementación sugerido

### v1 (MVP)
- PresentMon CLI + CSV parse
- 3 juegos: CS2 (workshop map), Apex (Firing Range), Cyberpunk (built-in bench)
- 3 runs default, mediana+IQR
- UI stepper (baseline → apply → post → compare)
- Export PDF
- ScottPlot para todo
- LibreHardwareMonitor para HW context

### v1.5
- Añadir Fortnite + SoTR + Valorant (con warning)
- 3DMark Time Spy integration (opcional, auto-detect)
- Historial persistente de runs
- Telemetry opt-in minimalista (Supabase + Postgres)

### v2
- Migrar a PresentMon Service + SDK
- Live FPS overlay con SDK (sin tail-CSV hack)
- Community page ("para tu HW, otros obtuvieron…")
- Diff visualizer avanzado (per-frame stutter comparison)

### v3
- Leaderboard optional
- "Recommended preset" auto basado en HW + historial del usuario
- ML-assisted tweak suggestion from community data

---

## 14. Checklist técnico final

### Antes de release v1

- [ ] PresentMon 2.x binary bundled o descarga firmada desde intel.com
- [ ] Manifest con `requireAdministrator`
- [ ] UAC elevation UX bien pulida (no popup sorpresa)
- [ ] Stop existing ETW sessions al arrancar y al salir
- [ ] Todos los CSV files en `%APPDATA%\GamingOptimizer\captures\` (no en TEMP)
- [ ] CsvHelper con `HeaderValidated = null` para tolerar cambios de schema
- [ ] Detección y parsing de versión de PresentMon (fallback si binary no matchea lo esperado)
- [ ] Mínimo 3 runs + rechazo automático si CV% > 15%
- [ ] Warmup de 30 s antes de run oficial
- [ ] Anti-cheat warning UI para Valorant / FACEIT CS2 Competitive
- [ ] Stats robustas (mediana, no media, donde aplique)
- [ ] Export PDF con metadata HW
- [ ] Kill switch para telemetry, default OFF

### Riesgos conocidos

- PresentMon CSV schema puede cambiar entre minor versions — pin the version bundled o test matrix
- Vanguard / FACEIT pueden empezar a flaggear ETW sessions kernel en cualquier update — necesidad de monitorear community reports
- Frame Generation (DLSS 3 FG, FSR 3 FG) genera frames sintéticos que PresentMon 2.x sí detecta (`--track_frame_type`) pero cuyo impacto en 1% low es debatible (los frames generados no tienen input real). Reportar claramente "with FG" / "without FG".
- FrameView 1.7+ agregó soporte 800+ FPS accurately; si sigue siendo problema en ciertas versiones para bench muy rápidos (CS2 en hardware high-end), testear.

---

## Fuentes

### Oficiales
- [PresentMon GitHub — GameTechDev](https://github.com/GameTechDev/PresentMon)
- [PresentMon Console Application README](https://github.com/GameTechDev/PresentMon/blob/main/README-ConsoleApplication.md)
- [PresentMon Service README](https://github.com/GameTechDev/PresentMon/blob/main/README-Service.md)
- [PresentMon API Reference (DeepWiki)](https://deepwiki.com/GameTechDev/PresentMon/6.2-api-reference)
- [Intel PresentMon — Intel Gaming Access](https://game.intel.com/us/intel-presentmon/)
- [NVIDIA FrameView](https://www.nvidia.com/en-us/geforce/technologies/frameview/)
- [NVIDIA FrameView User Guide PDF](https://www.nvidia.com/content/dam/en-zz/Solutions/GeForce/technologies/frameview/frameview-user-guide-1-1-web.pdf)
- [CapFrameX](https://www.capframex.com/)
- [CapFrameX performance metrics explanation](https://www.capframex.com/blog/post/Explanation%20of%20different%20performance%20metrics)
- [CapFrameX GitHub](https://github.com/CXWorld/CapFrameX)
- [LatencyMon — Resplendence](https://www.resplendence.com/latencymon)
- [LibreHardwareMonitor GitHub](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- [LibreHardwareMonitorLib NuGet](https://libraries.io/nuget/LibreHardwareMonitorLib)
- [Unigine Superposition benchmark](https://benchmark.unigine.com/superposition)
- [UL 3DMark command line guide](https://support.benchmarks.ul.com/support/solutions/articles/44002145411-run-3dmark-benchmarks-from-the-command-line)
- [AMD OCAT — GPUOpen](https://gpuopen.com/ocat/)

### Metodología / Methodology
- [GamersNexus — Animation Error engineering discussion](https://gamersnexus.net/gpus-cpus-deep-dive/fps-benchmarks-are-flawed-introducing-animation-error-engineering-discussion)
- [GamersNexus — 1% Lows and Delta T Methodology](https://gamersnexus.net/site-news/2513-testing-methodology-explained-1percent-lows-and-delta-t)
- [GamersNexus — GPU Benchmarks Reality vs Numbers whitepaper](https://gamersnexus.net/gpus-gn-extras-cpus/problem-gpu-benchmarks-reality-vs-numbers-animation-error-methodology-white)
- [Statistical Methods for Reliable Benchmarks](https://modulovalue.com/blog/statistical-methods-for-reliable-benchmarks/)
- [ASUS ROG — What are 1% lows](https://rog.asus.com/articles/guides/what-are-1-lows-how-to-monitor-and-fix-stutters-in-your-pc-games/)

### Game scenarios
- [Steam Workshop — CS2 FPS BENCHMARK Ancient (id 3472126051)](https://steamcommunity.com/sharedfiles/filedetails/?id=3472126051)
- [Steam Workshop — CS2 FPS BENCHMARK Dust2 (id 3240880604)](https://steamcommunity.com/workshop/filedetails/?id=3240880604)
- [Steam Workshop — FPS Benchmark (original)](https://steamcommunity.com/sharedfiles/filedetails/?id=500334237)
- [Ark0N — CS2 Benchmark Automation (AutoHotkey + CapFrameX)](https://github.com/Ark0N/-CS2-Benchmark-Automation)
- [RankedKings — Valorant benchmarking guide](https://rankedkings.com/blog/how-to-benchmark-valorant)
- [Dodge.gg — Valorant Deathmatch / Range guides 2026](https://www.dodge.gg/news/deathmatch-mode-guide-2026)
- [Apex Legends launch options — ProSettings](https://prosettings.net/blog/best-apex-legends-launch-commands/)
- [Apex Legends best settings 2026 — Switchblade Gaming](https://www.switchbladegaming.com/game-settings/apex-legends-best-settings/)

### Sensores HW
- [HWHash GitHub](https://github.com/layer07/HWHash)
- [Hwinfo.SharedMemory.Net](https://github.com/Seraksab/Hwinfo.SharedMemory.Net)
- [HWiNFO Shared Memory Support forum](https://www.hwinfo.com/forum/threads/shared-memory-support.18/)

### Plot libraries
- [ScottPlot](https://scottplot.net/)
- [ScottPlot FAQ — live data](https://scottplot.net/faq/live-data/)
- [LiveCharts2 vs ScottPlot comparison](https://www.libhunt.com/compare-LiveCharts2-vs-ScottPlot)
- [OxyPlot WPF guide](https://copyprogramming.com/howto/oxyplot-with-wpf)

### Anti-cheat context
- [TATEWARE — EAC vs BattlEye vs Vanguard vs RICOCHET 2026](https://tateware.com/blog/anti-cheat-comparison-2026)
- [Anti-cheat compatibility list](https://aclist.github.io/)
- [Riot — VAN:Restriction & Vanguard](https://www.riotgames.com/en/news/vanguard-security-update-motherboard)

### Telemetry backends
- [Umami vs Plausible vs Matomo comparison](https://aaronjbecker.com/posts/umami-vs-plausible-vs-matomo-self-hosted-analytics/)
- [PostHog self-hosted analytics](https://posthog.com/blog/best-open-source-analytics-tools)
- [Coders Stop — Setting up self-hosted analytics](https://medium.com/@coders.stop/setting-up-self-hosted-analytics-posthog-plausible-umami-comparison-ac4e7e826486)

### Reviewers / coverage
- [TechSpot — How to use Intel PresentMon](https://www.techspot.com/article/2723-intel-presentmon/)
- [VideoCardz — PresentMon 2.2.0 lowered event latency](https://videocardz.com/newz/intel-presentmon-2-2-0-offers-significantly-lowered-event-latency)
- [VideoCardz — PresentMon 2.5.0 multi-device metrics](https://videocardz.com/newz/presentmon-2-5-0-adds-multi-device-metrics-support-and-enhanced-metrics)
- [Tom's Hardware — FrameView 1.7 update 800+ FPS](https://www.tomshardware.com/pc-components/gpus/nvidia-updates-frameview-performance-measurement-tool-version-1-7-promises-accurate-results-even-at-800-fps)
- [XbitLabs — Understanding GPU Busy](https://www.xbitlabs.com/blog/gpu-busy/)
- [XDA — 5 reasons Intel PresentMon is best](https://www.xda-developers.com/5-reasons-intel-presentmon-is-the-best-tool-for-checking-exactly-what-your-gaming-pcs-bottleneck-is/)
- [SmoothFPS — Frame Time Explained](https://smoothfps.com/guides/frametime-explained)
- [Hone — How to optimize PC for gaming](https://hone.gg/blog/optimize-pc-for-gaming/)
- [Timer Resolution download](https://timerresolution.com/downloads/)
