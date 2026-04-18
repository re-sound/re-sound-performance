Tengo información sólida ahora. Procedo a sintetizar el entregable completo.

---

# UI/UX Research Report — Windows 11 Gaming Optimization App

**Deliverable: Design research + recommendations para construir UI premium propia, única, basada en análisis del estado del arte.**

---

## 1. Review por producto (análisis UX + fortalezas/debilidades)

### 1.1 BoosterX

**Layout**: Dashboard-first con sidebar de categorías (Performance, Gaming, System, Privacy, Debloat). Header con "Optimization Level %" gauge central tipo velocímetro. Cards con toggles para cada tweak, agrupados por severidad (Safe / Advanced / Risky).

**Visual identity**: Dark theme saturado (azul marino + acentos neón violeta/cyan). Tipografía sans-serif condensada para headers, mono para data. Iconografía custom con glyphs gaming (crosshair, lightning). Gradientes sutiles en cards, glow sutil en botón primario.

**Fortalezas**:
- Gauge central comunica progreso/estado en 1 mirada ("78% optimized")
- Un-click "Optimize Now" con preview de cambios antes de aplicar
- Tabs per-preset (Gaming / Streaming / Office) bien diferenciados
- Restore point automático visible en toolbar

**Debilidades**:
- Marketing hype en textos ("Extreme Performance Mode!") se siente bajo-nivel
- Algunas stats no citan fuente (claim "+30% FPS")
- Mezcla "debloat" con "optimization" confundiendo al usuario

**Learnings para nuestra app**: El gauge central es excelente anchor visual. Agrupar por severidad es claro. Evitar el hype en microcopy.

### 1.2 Razer Cortex

**Layout**: Top-tabs horizontales (Game Booster / System Booster / Game Launcher / Deals). Dashboard con game library + "Boost" CTA prominente. Overlay in-game customizable.

**Visual identity**: Dark con acento Razer green (#44D62C). Hero images de juegos (key art grande). Ribbon de notificaciones en header. Tipografía Roboto/Segoe, limpia pero reconocible marca.

**Fortalezas**:
- Auto-boost on launch (detección del proceso de juego → activa sin click)
- Restore state post-session (clave para confianza del usuario)
- In-game overlay con stats, fully customizable (metrics / position / colors)
- Brand consistency fortísima (el verde Razer es icónico)

**Debilidades**:
- Mezcla store/deals con optimizer (monetización evidente que baja percepción)
- Machine learning "auto-tune" es caja negra (usuarios pro no confían)
- Pesado en recursos para lo que hace

**Learnings**: La detección automática de juegos + restore post-session es tabla mínima. Overlay customizable es diferenciador. Marca fuerte > neutra, pero sin forzar branding en cada pantalla.

### 1.3 Hone.gg

**Layout**: Single-page scroll largo con secciones colapsables (Recommended / All tweaks / Pro configs / Real-time). Search bar prominente. Sidebar mínima (Home / Games / Settings).

**Visual identity**: Dark mode con acento magenta/rosa fluorescente. Cards grandes con hover states pronunciados. Iconos custom. Microanimaciones en toggles.

**Fortalezas**:
- "Pro Config import" como feature flagship (importa config de jugador famoso → aplica)
- Real-time optimization background service (no bloquea UI)
- Free tier + premium bien separados sin paywalls agresivos en UI
- UI simple y moderna; comunidad valida que es "fácil"

**Debilidades**:
- Overwhelming al primer contacto (demasiado scroll vertical)
- Pocos tooltips explicando qué hace cada tweak
- Algunos usuarios reportan freezes (implementación, no UI)

**Learnings**: "Pro Config" es una idea killer — nuestro app debería tener algo similar. Search-first es buena idea para 100+ tweaks.

### 1.4 NVIDIA App (reemplazo GeForce Experience)

**Layout**: Sidebar izquierdo con iconos + labels (Home / Drivers / Graphics / Display / Video / Broadcasting / Redeem). Content area con cards grandes. Overlay in-game como sidebar izquierda, customizable.

**Visual identity**: Dark theme (casi negro puro), acento NVIDIA green mínimo. Tipografía clean sans. Muy al estilo WinUI 3 / Fluent. Cards con bordes sutiles, sin gradientes llamativos.

**Fortalezas**:
- Login opcional (gigantesco win vs GeForce Experience que forzaba)
- Overlay sidebar (no fullscreen) — menos intrusivo que el anterior
- Separación clara: drivers (technical) vs graphics (gaming)
- Per-game optimization con profile picker

**Debilidades**:
- Todavía bugs en beta
- Algunas features requieren login (confuso)

**Learnings**: El shift de "toda la app como un monolito" a "cards modulares en sidebar" es la dirección correcta. Overlay sidebar > overlay fullscreen.

### 1.5 Chris Titus WinUtil (baseline competitor)

**Layout**: 4 tabs top (Install / Tweaks / Config / Updates). Tweaks tab con collapsible tree nodes por categoría. No sidebar. Sin dashboard home.

**Visual identity**: PowerShell WPF crudo. Dark mode default, minimal theming (muy monocromo). Segoe UI. Algunos iconos Material.

**Fortalezas**:
- Funcionalidad enorme (install apps + tweaks + Win11 debloat en 1)
- Reversibilidad (restore point automático)
- Cross-platform compilation (F#/PowerShell)
- Comunidad activa

**Debilidades** (según feedback de sus propios issues):
- UI se siente "DIY script" sin identidad
- Collapsibles ocultan demasiado; costoso discover
- Todos los iconos apps iguales (sin branding)
- Falta de consistencia de spacing/padding
- Sin onboarding para newbies

**Learnings (crítica directa)**: Esto es exactamente lo que NO queremos. Necesitamos: (1) identidad visual propia, (2) no ocultar acciones detrás de collapsibles si son primarias, (3) iconografía diferenciada, (4) spacing system consistente.

### 1.6 Optimizer (hellzerg)

WinForms C# clásico, UI funcional pero "vieja" (2015 vibes). Checkboxes densas. Multi-idioma fuerte. Sin gaming focus, sin dashboard. Buen ejemplo de "herramienta seria" pero no premium.

### 1.7 AMD Adrenalin 2024/2026

Sidebar izquierda + cards grandes en content area. Hero game art per-game. HYPR-RX como preset visual. Performance metrics overlay customizable (background/text colors). AI chat integrado.

**Learning**: Hero per-game card + preset HYPR-RX model es copiable conceptualmente.

### 1.8 Intel Graphics Software (ex Arc Control)

Rediseño 2024/2025 con estructura all-in-one: sidebar + content. Per-app profiles. Performance tuning tab. Autoaplica profile al detectar app.

**Learning**: Per-app auto-profile es tabla mínima 2026.

### 1.9 Razer Synapse 4

Rediseño 2024. Tab-based lateral. Menos verde saturado → más neutro moderno. Tips flotantes contextuales. 30% más rápido. Feedback mixto (algunos extrañan verde dominante).

**Learning**: Tips flotantes (no modals) es UX patrón moderno. Tempering del color brand.

### 1.10 Logitech G HUB

Per-game profile auto-detect. Update 2026 hace detección instantánea. Games tab con curated recommendations de pros.

**Learning**: "Curated from pros" tiene tracción — fit con pro-config import.

### 1.11 NZXT CAM

Tabs top + dashboard con 3 views (Basic / Advanced / Expanded). Hero CPU/GPU cards con temp + load. Real-time monitoring durante gaming. Dashboard personalizable/modular.

**Learning**: Progressive disclosure (Basic → Advanced → Expanded) es un patrón clave para mezclar newbies + power users. Modular dashboard.

### 1.12 Corsair iCUE

Dashboard movible/redimensionable por sección. Widget system (sensors, RGB, fans, etc). Per-screen backgrounds personalizables. Toggle widgets on/off.

**Learning**: Widget-based dashboard es flexible. Quizás overkill para gaming-focus app, pero ciertas secciones sí (monitoring).

### 1.13 SteelSeries GG

Side navigation refinada iterativamente 2024-25. Widgets de Volumen + Mic agrupados por lado (izq: mic, der: audio). Public profile renovado. Design incremental no revolutionary.

### 1.14 ASUS Armoury Crate 6

Modular install (solo instala módulos necesarios). Dashboard resumen sistema. Carousel de juegos. "Console-like UI" en edición ROG Ally X.

**Learning**: Modular install reduce bloat sentido; nuestra app puede ofrecer "features on demand".

### 1.15 MSI Afterburner + RTSS (overlay)

Skinnable OSD con colores condicionales (rojo si FPS<60, verde si >60). Pixel-level position adjust. Templates community.

**Learning**: Colores condicionales es clave en overlays. Templates community es killer para onboarding.

### 1.16 CapFrameX (frametime tool)

UI técnica pero respetada: pie charts (stuttering %, variance), bar charts (FPS thresholds), L-shape distribution, side-by-side comparison tab. Switch raw/filtered.

**Learning**: Si incluimos benchmark, L-shape graph + comparison tab es standard del nicho pro.

### 1.17 PresentMon Beta Intel

Nuevo overlay con métrica "GPU Busy" (CPU↔GPU balance). Overlay real-time con voltaje, temp. Open-source.

**Learning**: GPU Busy como concepto vale explicar; overlay minimalista.

### 1.18 HWiNFO64

Dense sensor list. Dashboards custom vía Jonitor/Rainmeter externos. Gold standard de data pero UI cruda.

**Learning**: Power users toleran density si data es correcta. Pero mainstream necesita abstracción (cards, no tablas).

### 1.19 Linear (no-gaming, referencia de design system)

Sistema propio "Orbiter" basado en Radix UI. Clean, fast, keyboard-first. Layout con sidebar colapsable + main area + inspector derecho. Typography enfocada.

**Learnings portables**: Keyboard shortcuts pervasive, sidebar colapsable con persistencia, transitions nativas (no bounce animation), spacing system de 4/8/12/16/24/32 px.

### 1.20 Raycast

Text-first UI (99% texto), command palette como interacción primaria. Compact mode. Keycap icon como brand. Extensions marketplace.

**Learnings portables**: Command palette `Ctrl+K` para buscar tweaks/acciones rápido — killer feature para power users. Compact/expanded mode dual.

### 1.21 Stripe Dashboard

Stacked metric cards, gross volume card grande, badges para trends, enterprise clarity. Primer design tokens.

**Learnings portables**: Metric cards top del dashboard (FPS avg / 1% Low / Temp / Latency). Badges compact de estado (Active / Idle / Warning). No overload de color.

### 1.22 1Password 8

Native-ish Electron (Rust core + React UI). Design language propio "Chamber". Fast, native-feel a pesar de Electron. Design-first team.

**Learning**: Web tech (HTML/CSS/JS) puede lograr UI premium si design team está integrado. No hay que descartar Electron/Tauri por prejuicio.

---

## 2. Design system recomendado para tu app

### Color palette

**Modo oscuro (primary)**:
- **Background 0**: `#0B0D10` (casi negro, ligeramente azul)
- **Background 1 (surface)**: `#14171C`
- **Background 2 (elevated card)**: `#1C2027`
- **Background 3 (hover)**: `#232933`
- **Border subtle**: `#2A3039`
- **Border focus**: `#3D4551`

**Neutrales**:
- **Text primary**: `#EAEEF3`
- **Text secondary**: `#A0A8B4`
- **Text tertiary/disabled**: `#5A6272`

**Accent primary (brand)** — recomiendo un color no usado por competidores directos:
- **Accent**: `#7C5CFF` (purple eléctrico, distinto de Razer green, NVIDIA green, AMD red, Corsair yellow, Logitech cyan)
- **Accent hover**: `#8F73FF`
- **Accent pressed**: `#6A47F0`

**Semánticos** (sobrios, no saturados):
- **Success**: `#3FBF8F` (verde menta, no neón)
- **Warning**: `#F2B857` (amarillo ámbar)
- **Danger**: `#E5544A` (rojo coral, no sangre)
- **Info**: `#4E9CFF` (azul suave)

**Severidad de tweaks** (para etiquetas/iconos):
- **Safe**: verde menta (#3FBF8F)
- **Moderate**: ámbar (#F2B857)
- **Advanced**: naranja (#F08658)
- **Risky/Expert**: rojo coral (#E5544A)

**Modo claro (secundario, opcional)**: Mismo accent, background `#F7F8FA`, surface blanco, text `#0B0D10`.

**Principio**: NO usar gradientes rainbow. NO RGB vomit. Un solo accent primario. Semánticos solo cuando comunican estado real.

### Tipografía

- **UI sans**: `Inter` o `Segoe UI Variable` (si Windows 11). Fallback: `-apple-system, Segoe UI, Roboto`
- **Monospace (data/configs/hex)**: `JetBrains Mono` o `Cascadia Code`
- **Jerarquía** (escalas):
  - Display (dashboard hero): 32px / weight 600 / line 1.2
  - H1: 24px / 600 / 1.3
  - H2: 18px / 600 / 1.4
  - Body: 14px / 400 / 1.5
  - Caption: 12px / 500 / 1.4
  - Micro (labels): 11px / 600 / 1.2 uppercase tracking 0.5px

**NO**: fuentes "gamer" (Audiowide, Orbitron, Aldrich, etc.) en UI. Solo aceptables en splash screen/logo si realmente son necesarias.

### Iconografía

- **Base**: Fluent System Icons (Microsoft, open source, gratuito, consistente con Win11)
- **Custom**: sólo iconos específicos de tu app (logo, presets, per-game marks)
- **Tamaños**: 16px (inline), 20px (botones), 24px (navegación), 32px (cards hero), 48px (empty states)
- **Style**: outline por default, filled para estados activos/seleccionados
- **Stroke weight**: 1.5px consistente

### Spacing system (múltiplos de 4)

`4, 8, 12, 16, 24, 32, 48, 64, 96`

- Gap entre elementos relacionados: 8
- Gap entre secciones: 24
- Padding cards interno: 16–24
- Margen entre cards: 12
- Padding de páginas: 32 (desktop amplio)

### Elevación / sombras (sutil, modo oscuro)

- **Level 0 (surface flat)**: sin sombra
- **Level 1 (card)**: `0 1px 2px rgba(0,0,0,0.4)`, border `1px solid #2A3039`
- **Level 2 (hover/floating)**: `0 4px 12px rgba(0,0,0,0.5)`, border `1px solid #3D4551`
- **Level 3 (modal/popover)**: `0 12px 32px rgba(0,0,0,0.6)`
- **Mica backdrop** (solo ventana principal Win11): usar `WindowBackdropType.Mica` vía WPF-UI

### Animaciones

- **Duración base**: 150ms (micro), 250ms (componente), 350ms (transición page)
- **Easing**: `cubic-bezier(0.16, 1, 0.3, 1)` (ease-out expo) para entrada, `cubic-bezier(0.7, 0, 0.84, 0)` para salida
- **Evitar**: bouncy/spring (se ve "gamer" barato). Preferir smooth.
- **Hover**: subtle scale(1.01) o brighten bg, no bounces
- **Toggle**: 200ms sliding
- **Page transition**: fade+slide 8px de izquierda/derecha

---

## 3. Layout recomendado

### Estructura principal

**Sidebar izquierda (colapsable, 240px expandida / 64px collapsed)**:
- Logo (top)
- Dashboard (home)
- Optimize (todas las tweaks)
- **Games** (expandible, muestra CS2 / Valorant / Apex / Custom)
- Monitor (sensors/FPS live)
- Benchmark (before/after)
- Pro Configs (import de jugadores)
- Settings
- (bottom) System status indicator + restore point quick

**Top bar mínima (40px)**:
- Breadcrumb/page title
- **Command palette trigger** (`Ctrl+K`) — search global + acciones
- Notification bell
- User/profile mini
- Minimize/close (WPF borderless)

**Content area**:
- Padding 32px
- Max width 1400px (centrada si viewport mayor)
- Responsive breakpoints: 1024 (sidebar auto-collapse), 1440 (cómodo), 1920 (generoso)

**Inspector/detail panel derecha (opcional, slide-in 400px)**:
- Se abre al hacer click en un tweak → muestra: descripción técnica, qué hace registry-level, severidad, reversible sí/no, fuente/referencia, toggle para aplicar
- Evita modales (dark pattern moderno: inspector > modal)

**Jerarquía visual del Dashboard home**:
1. Hero: Health Score del sistema (grande, 0–100, gauge radial)
2. Metric cards row: FPS avg actual, Temp CPU, Temp GPU, Memoria usada, Latencia red
3. "Quick optimize" CTA card grande (un-click para preset "Gaming Baseline")
4. "Detected games" lista con per-game profile status
5. "Recent actions" log (qué se aplicó, cuándo, rollback individual)

**Páginas secundarias**:
- **Optimize**: tweaks list con filters (severidad / categoría / estado aplicado). Search. Lista virtualizada (1000+ items ok).
- **Per-game tab** (CS2/Valorant/Apex): dashboard específico con: settings recomendados, pro configs importables, anti-cheat warning (severidad por game), tweaks compatibles only.
- **Monitor**: grid de gráficos live (FPS, frametime, CPU%, GPU%, temps, RAM). Customizable widgets.
- **Benchmark**: "Run baseline" → aplica tweaks → "Run after" → compare side-by-side con L-shape graph.

---

## 4. Framework stack recomendado

### Decisión final: **C# + WPF + WPF-UI (Lepo.co) + LiveCharts2**

**Justificación técnica**:

1. **C# + WPF** sobre PowerShell+WPF:
   - PowerShell-WPF: rápido prototipar, ok para WinUtil-style scripts. **Techo bajo** en UI compleja (data binding sin MVVM clean, debugging XAML doloroso, perf con 1000+ items).
   - C# WPF: MVVM real, Visual Studio designer, async/await limpio, NuGet ecosystem completo, perf superior, distribución single-exe con `dotnet publish -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true`.
   - **Para app gaming con live graphs, 1000+ tweaks, per-game tabs, benchmark, C# WPF es la decisión correcta**.

2. **WPF-UI (Lepo.co)** sobre ModernWpf / MaterialDesign / MahApps / HandyControl:
   - WPF-UI trae **Fluent 2 + Mica backdrop nativo Win11** out of the box
   - ApplicationThemeManager auto-sigue tema Windows
   - NavigationView estilo WinUI 3 pero en WPF clásico
   - Fluent System Icons integrados
   - NumberBox, Dialog, Snackbar, ContentDialog incluidos
   - Actively maintained (9K stars, 650K downloads)
   - ModernWpf (Kinnara): valioso pero más "Windows 10 WinUI 2.x". WPF-UI está más alineado a 2026.
   - MaterialDesign: estética Google, desalineada con target Windows 11 gamer.
   - MahApps: Metro Win8 style, outdated.
   - HandyControl: componentes ricos, pero look menos premium que WPF-UI.

3. **LiveCharts2** para gráficos live (FPS/frametime):
   - MIT license (gratis, sin restricción)
   - SkiaSharp rendering, smooth 60fps
   - Data binding nativo WPF, MVVM friendly
   - Ejemplos real-time en docs oficiales
   - Alternativa **GLGraph** (OpenGL, máx perf) si necesitas 1B+ puntos — probablemente overkill
   - **SciChart** ($1499/dev) solo si vas comercial-enterprise. Para open/free → LiveCharts2.

4. **Windows Community Toolkit** (RadialGauge) para gauges circulares del "Health Score".

5. **Empaquetado**: MSIX (Win11 native, Store-ready) o single-exe con ClickOnce o Velopack (modern autoupdate). **No** recomiendo instalador InnoSetup clásico salvo que necesites admin install.

### Alternativas evaluadas y descartadas

| Stack | Pros | Contras | Veredicto |
|---|---|---|---|
| **PowerShell + WPF** | Rápido, familiar si pentester | Techo bajo UI compleja, debug XAML pobre, perf limitada | **Descartar** para app premium |
| **WinUI 3 + Windows App SDK** | Más moderno, Fluent nativo, futuro Microsoft | Windows App SDK packaging dolor, menos ejemplos, debugging inmaduro aún | Alternativa viable pero **riesgo** para ship en 2026 |
| **Avalonia 11** | Cross-platform (Linux/Mac si futuro), WPF-like XAML, CSS-like styling | Menos nativo Windows "look", community < WPF, curva aprendizaje si vienes WPF | **Si no necesitas cross-plat → WPF gana**; si sí → Avalonia primero |
| **Tauri + Rust + web UI** | Binario 2–10MB, perf excelente, UI web con React/Svelte | Requiere Rust + web stack, menos nativo Windows, menos control de Windows APIs | **Descartar** para app que hace registry/services tweaks (Windows-deep) |
| **Electron + React** | Ecosistema masivo, UI rápida iterar | 80–150MB bundle, RAM alta (gamers lo detestan) | **Descartar** irónicamente para una app que "optimiza gaming" |
| **Flutter desktop** | UI rica, dart | Non-native Windows feel, overhead runtime | **Descartar** |
| **.NET MAUI** | Cross-plat con C# | Desktop soporte todavía inmaduro vs WPF | **Descartar** para Windows-only |
| **C# WinForms** | Simple, familiar | UI se ve "2010", no modern | **Descartar** |

---

## 5. Componentes críticos — implementación

### Toggle switch
- **Style**: Fluent-ish (pill con circle), 40×20px, ease-out 200ms
- **Estados**: off (border gris, bg transparent), on (bg accent #7C5CFF), disabled (50% opacity), loading (spinner embedded tras click mientras aplica el tweak)
- WPF-UI `ui:ToggleSwitch` listo out of box
- **Siempre** con label a la izquierda (NO solo el switch sin contexto)

### Cards
- Border 1px `#2A3039`, radius 8px, padding 16–24
- Hover: border `#3D4551`, bg `#1C2027`→`#232933`
- Cards con header (icon + title + optional badge) + body + optional footer
- Card "severity" con border-left 3px color (safe/moderate/advanced/risky)

### Sliders (para valores numéricos: DPI, polling, etc.)
- Track 4px alto, thumb 16px círculo
- Valor live arriba del thumb o a la derecha en badge
- Optional step markers visibles
- WPF `Slider` estilizado o WPF-UI custom

### Progress bars
- Determinate: track 4px, fill accent, label % opcional
- Indeterminate: shimmer gradient loop
- Segmented (para multi-step apply): N bloques, cada uno toggle on cuando completa

### Gauges (Health Score, FPS counter)
- Radial gauge 160–200px diameter, semicircular
- Scale 0–100, needle animada ease-out
- Background tracks en `#2A3039`, fill con gradient por rango (rojo <30, ámbar 30–60, verde 60–100)
- Texto central grande (32–48px bold) con valor + label pequeño abajo
- Windows Community Toolkit `RadialGauge` (free) o LightningChart (paid)

### Notificaciones
- **Toast**: bottom-right, auto-dismiss 4s, slide+fade. Severidad por icono color
- **Banner**: inline top de la página para estados persistentes (ej: "You're running as non-admin, some tweaks disabled")
- **Inline validation**: dentro del form/slider con mensaje debajo
- **NO** popups modales salvo confirmación destructiva

### Search bar / command palette
- `Ctrl+K` abre overlay central (Raycast-style)
- Indexar: tweaks, settings, games, acciones
- Fuzzy match, keyboard navigation (arrows + enter)
- Resultados agrupados con iconos

### Tooltips
- Hover 300ms delay (no instantáneo, molesto)
- Ancho max 280px, padding 8, radius 6
- Contenido: 1 frase + opcionalmente "Learn more →" link a detail view
- **Never** ocultar info crítica (warnings) en tooltip. Warnings = visibles.

### Modals / confirmations
- Solo para acciones destructivas/irreversibles
- Backdrop smoke (blur 8px + 30% black)
- Body: clear copy + 2 botones (primary destructive / secondary cancel)
- Esc cierra, focus trap

### Empty states
- Icon 48–64px grises
- Título + descripción breve
- CTA si procede ("No hay juegos detectados aún → Scan now")

---

## 6. FPS/frametime graph live — integración

**Librería**: LiveCharts2 (MIT, gratuita).

**Arquitectura**:
- Background service que hook a PresentMon (API abierta Intel) para leer frametimes de cualquier DX11/12/Vulkan/OpenGL
- O MSI Afterburner shared memory (`MACMSharedMemory`) si ya está instalado
- Buffer circular de últimos 600 frames (~10s a 60fps)
- Update UI vía `Dispatcher.BeginInvoke` con throttle 16ms (60hz)

**Visualización**:
- **FPS line chart**: últimos 10s, escala Y auto 0–max+10%
- **Frametime bar chart**: ms por frame, línea roja en 16.7ms (60fps) / 8.3ms (120fps) / 4.2ms (240fps) según monitor
- **L-shape**: percentiles (P50/P99/P99.9) como CapFrameX
- **1% Low badge** grande con valor live

**Código ejemplo (pseudo-WPF + LiveCharts2)**:
```csharp
public ObservableCollection<double> FrameTimes = new();
var series = new LineSeries<double> { 
    Values = FrameTimes, Stroke = new SolidColorPaint(accent),
    GeometrySize = 0, LineSmoothness = 0.5
};
// background loop: FrameTimes.Add(newMs); if (count>600) RemoveAt(0);
```

**Alternativa**: Si ya tienes Afterburner del user → leer su shared memory y evitar implementar capture nativo.

---

## 7. Pro config import UX — flow ideal

**Inspiración**: Hone.gg + Logitech G HUB Games + WinUtil presets.

**Flow propuesto**:

1. **Página Pro Configs** (accesible desde sidebar principal o desde per-game tab)
2. Grid de cards — cada card es un pro player:
   - Avatar, nombre, team, game, ELO/rank, bandera país
   - "Last updated" badge (para que usuarios vean si es fresh)
   - Preview badges de qué toca: sensitivity / DPI / in-game video / Windows tweaks / FPS unlock
3. Click en card → **Detail view** (inspector slide-in o full page):
   - Lista diff "lo que va a cambiar en tu sistema"
   - Severidad por item (safe / advanced)
   - Toggle selectivo — el user puede deselect items que no quiera
   - Botón "Apply selected" + botón "Save as my preset"
4. **Aplicación**:
   - Restore point auto antes
   - Progress con log visible por item
   - Toast final: "Applied 12 of 14 changes. 2 skipped (required admin). Rollback available."
5. **Fuentes** (trust):
   - Cada pro config con link a fuente original (prosettings.net, settings.gg, video del jugador)
   - Hash verification contra fuente oficial si viable

**Data source**: scrape rutinario de prosettings.net, settings.gg, ProSettings.gg. JSON en repo GitHub de la app, actualizable sin update de app binary.

---

## 8. Onboarding first-run experience

**Pantalla 1 — Welcome (3s, auto-advance opcional)**:
- Logo grande, gradient sutil background
- "Make your PC gaming-ready, safely." (subtitle)
- Botón "Get started" + link "Import settings from another tool" (WinUtil/Optimizer/Hone)

**Pantalla 2 — System scan (loading + resultados)**:
- Progress auto durante 5–10s
- Resultados en cards:
  - Hardware detectado (CPU/GPU/RAM/Storage)
  - Monitor refresh rate detectado
  - Juegos detectados (Steam/Epic/Riot/Battle.net folders)
  - Anti-cheat detectado (Vanguard running? EAC installed?)
  - OS estado (Win11 version, powerplan actual)

**Pantalla 3 — Pick your profile**:
- 3 cards grandes lado a lado:
  - **Casual**: balanced. Aplica preset safe, no toca registry nivel profundo
  - **Competitive**: FPS/latency focused. Aplica tweaks moderate incl. timer resolution, HAGS check, etc.
  - **Esports Pro**: máximo. Requiere admin + full restore point. Warnings visibles
- Botón "I'll customize manually later" (skip)

**Pantalla 4 — Per-game setup (opcional)**:
- Si detectó CS2/Valorant/Apex → mostrar "Want pro configs for these?"
- Toggle per-game + preview de top pro player por default

**Pantalla 5 — Safety check**:
- Explicación en texto corto de: restore point creado automáticamente, qué es reversible
- Checkbox "I understand and want to proceed"
- Botón "Apply and go to dashboard"

**Principios**:
- Máximo 5 pantallas
- Skipable en cualquier momento (botón "Skip intro" top-right)
- Progress bar persistente top
- Decisiones reversibles desde Settings luego

**Onboarding "returning user"** (segunda vez que abre):
- Ninguno. Directo al dashboard. NO tutorial overlay intrusivo.
- Feature discovery via "What's new" modal solo en major updates.

---

## 9. Antipatterns a EVITAR (lista)

1. **Scareware fake errors**: "17 CRITICAL ERRORS DETECTED!!!" cuando son tweaks opcionales. Deslegitima.
2. **Fake stats inflados**: "+300% FPS!!" sin fuente. Usar lenguaje honesto ("up to X%" con fuente link).
3. **Banners publicitarios**: Razer Cortex mete "Deals" section (Steam keys). No incluir ads/upsell dentro de la UI.
4. **Upselling agresivo**: botón Premium bloqueando features core. Si freemium, premium debe ser features avanzadas, NO tweaks básicos.
5. **RGB vomit / gradientes rainbow**: el acento es uno. Cards no cambian de color random. Evitar efectos "gamer" cheesy.
6. **Fuentes gamer everywhere**: Audiowide/Aldrich/Orbitron en UI. Solo en logo máximo, no body text.
7. **Loading spinners innecesarios**: si acción toma <400ms, no mostrar spinner (parpadea molesto). Usar optimistic UI.
8. **Notificaciones popup constantes**: una por acción es mucho. Bundle notifications cuando aplicable.
9. **Modales para todo**: abrir modal para un toggle. Usar inspector side panel.
10. **Collapsibles ocultando primary actions**: WinUtil error. Primary actions visibles siempre.
11. **Iconos duplicados**: todos los apps con mismo icon genérico. Usar brand icons reales o colored letters.
12. **Copy marketing**: "Unleash your full potential!!" en descripción de tweak. Ser técnico sin ser seco.
13. **Claims sin fuente**: "aumenta +30% FPS" sin link a benchmark. Siempre citar o NO afirmar.
14. **Force login**: obligar cuenta para usar app (GeForce Experience errror). Login = opcional, solo para sync de configs.
15. **Tooltips con info crítica**: warnings deben ser visibles, no en hover.
16. **Too many clicks**: aplicar un preset no debería ser más de 2 clicks (selección + confirm). Evitar wizards de 7 pasos para acciones recurrentes.
17. **Dark pattern "Recommended" pre-checked con items invasivos**: pre-check solo items verdaderamente safe.
18. **Full-screen overlays in-game intrusivos**: preferir sidebar mini (tipo NVIDIA App 2024).

---

## 10. Microcopy guidelines

### Tone of voice
- **Clear, technical, respectful**. Pro-gamer lee docs, no es un grandma. Pero tampoco asume PhD.
- **Honest over hype**. "May improve FPS up to 10% on some hardware" > "BOOST FPS BY 300%!!".
- **Action-oriented**. Verbs activos en botones. "Apply", "Optimize", "Restore" > "OK".
- **Spanish (CL)** si target LATAM, o bilingüe toggle.

### Descripciones de tweaks

**Template**:
```
[Short action title - 3-5 palabras]
[1-2 sentences explaining what it does technically]
[Optional: Why it may help gaming]
Severity: [Safe|Moderate|Advanced|Risky]  Reversible: [Yes|No]  Anti-cheat: [Safe|Caution]
```

**Ejemplo bueno**:
> **Disable HAGS (Hardware-accelerated GPU Scheduling)**
> Turns off Windows feature that lets your GPU manage its own memory. Some esports titles show better frame consistency with HAGS off; modern AAA games may prefer it on.
> Severity: Safe · Reversible: Yes · Anti-cheat: Safe

**Ejemplo malo** (evitar):
> **TURBO FPS BOOST!!**
> Disables a stupid Windows thing that slows your games.

### Warnings por severidad

- **Safe**: sin warning, toggle directo.
- **Moderate**: badge ámbar + texto "Minor system change — easily reversible."
- **Advanced**: banner ámbar + "This modifies system services. Restart may be required."
- **Risky**: modal con full explanation + checkbox "I understand", requiere admin, auto restore point pre-aplicar.

### Tooltips
- Max 2 frases, 140 chars aprox.
- Si más detalle → "Learn more →" link abre inspector.

### Buttons
- Primary: acción concreta. "Apply all", "Run benchmark", "Import config".
- Secondary: "Cancel", "Skip", "Customize".
- Destructive: rojo, texto explícito. "Revert all tweaks", "Delete preset".

### Errors
- **Mal**: "Error: Something went wrong." / "Exception: 0x800700C1"
- **Bien**: "Couldn't apply DNS tweak — requires admin permissions. Try running as administrator." + button "Retry as admin".
- Incluir siempre: qué pasó + qué hacer + cómo reportar si persiste.

### Empty states
- **Mal**: "No items."
- **Bien**: "No games detected yet. Install Steam/Epic or scan custom folder." + botón.

### Onboarding copy
- Frases cortas (<15 palabras idealmente).
- Beneficio primero, mecánica después.
- "Make your PC gaming-ready, safely." > "A Windows optimization utility for gaming performance."

---

## 11. Wireframes descriptivos

### Pantalla A — Dashboard Home

```
┌─────────────────────────────────────────────────────────────────────────┐
│ [☰] GameTune        [Ctrl+K Search]              [🔔] [user]  [─][×]   │
├──────┬──────────────────────────────────────────────────────────────────┤
│ 🏠   │                                                                  │
│ Dash │   Welcome back, re_sound                                         │
│      │                                                                  │
│ ⚡   │   ╭──────────────╮  ╭──────╮ ╭──────╮ ╭──────╮ ╭──────╮          │
│ Opt  │   │              │  │ FPS  │ │ Temp │ │ RAM  │ │ Ping │          │
│      │   │    88 / 100  │  │ 244  │ │ 67°C │ │ 14GB │ │ 12ms │          │
│ 🎮   │   │  Health Score│  │ avg  │ │ CPU  │ │ free │ │ LoL  │          │
│ Games│   │  (↑ from 76) │  │      │ │      │ │      │ │      │          │
│  CS2 │   ╰──────────────╯  ╰──────╯ ╰──────╯ ╰──────╯ ╰──────╯          │
│  Val │                                                                  │
│  Apex│   ╭────────────────────────────────────────────╮                │
│ 📊   │   │  ⚡ Quick Optimize                           │                │
│ Mon  │   │  Apply the Gaming Baseline preset — 14      │                │
│      │   │  safe tweaks. Restore point created first.  │                │
│ 🧪   │   │                        [  Run optimization ]│                │
│ Bench│   ╰────────────────────────────────────────────╯                │
│      │                                                                  │
│ 🏆   │   Detected games                                                 │
│ Pro  │   ╭───────────╮ ╭───────────╮ ╭───────────╮                    │
│      │   │ CS2       │ │ Valorant  │ │ Apex      │                    │
│ ⚙️    │   │ ✓ Profile │ │ ⚠ Vanguard│ │ ○ Not set │                    │
│ Sett │   │ applied   │ │ running   │ │           │                    │
│      │   │           │ │ Caution   │ │           │                    │
├──────┤   ╰───────────╯ ╰───────────╯ ╰───────────╯                    │
│ ✅ OK│                                                                  │
│ restore│  Recent activity                                               │
│ point │  • 2h ago — Applied "Competitive Preset" (14 items)             │
│ ready │  • Yesterday — Ran benchmark: +8.2% FPS avg                     │
└──────┴──────────────────────────────────────────────────────────────────┘
```

Notas:
- Sidebar colapsable. Estado persistente.
- Health Score radial gauge con delta vs última vez.
- Metric cards top son live-updating (refresh cada 2s).
- CTA card "Quick Optimize" es acción primaria de la home.
- Per-game cards muestran estado profile + badge anti-cheat.
- Bottom sidebar: indicador restore point quick.

### Pantalla B — Per-game tab CS2

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Dashboard › Games › Counter-Strike 2                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🎯 Counter-Strike 2                        [Detected: Steam / C:/...]  │
│                                                                         │
│  ╭─────────────────────────────────────────────────────────────────╮   │
│  │ Active profile: apEX (Team Vitality)           [Change profile] │   │
│  │ Last applied: 3 days ago · 11 tweaks active                      │   │
│  ╰─────────────────────────────────────────────────────────────────╯   │
│                                                                         │
│  [ In-game settings ] [ System tweaks ] [ Launch options ] [ Network ]  │
│                                                                         │
│  In-game settings (apply via autoexec.cfg)                              │
│  ╭─────────────────────────────────────────────────────────────────╮   │
│  │  Sensitivity           1.70     [———•————]         · Safe       │   │
│  │  DPI                   800      [——•——————]        · Safe       │   │
│  │  Resolution            1280×960 stretched  [dropdown]            │   │
│  │  FOV (viewmodel)       60       [———•—————]        · Safe       │   │
│  │  MSAA                  2x       [toggle]                         │   │
│  │  Shadow quality        High     [dropdown]                       │   │
│  ╰─────────────────────────────────────────────────────────────────╯   │
│                                                                         │
│  System tweaks (CS2-specific)                                           │
│  ╭─────────────────────────────────────────────────────────────────╮   │
│  │  [•] High-priority process for cs2.exe          · Moderate      │   │
│  │  [•] Disable fullscreen optimizations            · Safe          │   │
│  │  [ ] Set affinity to P-cores only (Intel 12th+) · Advanced      │   │
│  │  [ ] Custom timer resolution 0.5ms              · Advanced ⚠    │   │
│  ╰─────────────────────────────────────────────────────────────────╯   │
│                                                                         │
│  [ Apply all selected (8)  ]  [ Reset to defaults ]                     │
└─────────────────────────────────────────────────────────────────────────┘
```

Notas:
- Top: active profile + last applied.
- Sub-tabs internas (In-game / System / Launch / Network).
- Sliders live con valor numérico.
- Cada tweak con badge severidad inline.
- Footer CTA: cuenta items seleccionados.

### Pantalla C — Tweak detail view (inspector slide-in)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Optimize                                          ╭───── Detail ─────╮  │
│                                                   │  [x] Close       │  │
│  [Search tweaks...]    [Severity ▾] [Category ▾]  │                  │  │
│                                                   │  Disable HAGS    │  │
│  Category: Performance                             │  Hardware-accel  │  │
│  ╭───────────────────────────────────────╮        │  GPU scheduling  │  │
│  │ [•] Disable HAGS            · Safe    │  ←──   │                  │  │
│  │     Improves frame consistency        │        │  Severity: Safe  │  │
│  ╰───────────────────────────────────────╯        │  Reversible: Yes │  │
│  ╭───────────────────────────────────────╮        │  Admin: No       │  │
│  │ [ ] Ultimate Performance    · Safe    │        │  Anti-cheat: ✓   │  │
│  │     Unlocks hidden power plan         │        │                  │  │
│  ╰───────────────────────────────────────╯        │  What it does    │  │
│  ╭───────────────────────────────────────╮        │  Registry edit:  │  │
│  │ [ ] Timer resolution 0.5ms  · Risky   │        │  HKLM\...\Graph… │  │
│  │     Advanced — can affect battery     │        │  HwSchMode = 1   │  │
│  ╰───────────────────────────────────────╯        │                  │  │
│                                                   │  Why it helps    │  │
│  Category: Network                                 │  Some esports    │  │
│  ╭───────────────────────────────────────╮        │  titles show     │  │
│  │ [•] Disable Nagle's Algorithm · Safe  │        │  better 1% low   │  │
│  ╰───────────────────────────────────────╯        │  with HAGS off.  │  │
│                                                   │                  │  │
│                                                   │  Source: [link]  │  │
│                                                   │                  │  │
│                                                   │  [ Apply tweak ] │  │
│                                                   │  [ Learn more ]  │  │
│                                                   ╰──────────────────╯  │
└─────────────────────────────────────────────────────────────────────────┘
```

Notas:
- Inspector slide-in 400px desde derecha (no modal).
- Click fuera cierra, tecla Esc cierra.
- Detalles: severidad, reversible, admin-required, anti-cheat compat, registry path concreto, racional, fuente.
- Siempre "Learn more" link a documentación externa.

### Pantalla D — Benchmark before/after

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Benchmark                                                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  [Run new benchmark]    Capture: [CS2 workshop_map ▾]   [   Start   ]   │
│                                                                         │
│  ╭─────────────── Before (2026-04-17 14:32) ───────────────╮            │
│  │                                                           │           │
│  │   FPS avg    312         1% Low    184     Temp 71°C      │           │
│  │                                                           │           │
│  │   ──────────────────────────────────────                  │           │
│  │   [L-shape frametime graph — 60s capture]                 │           │
│  │   ──────────────────────────────────────                  │           │
│  ╰──────────────────────────────────────────────────────────╯            │
│                                                                         │
│  ╭─────────────── After (2026-04-18 09:05) ────────────────╮            │
│  │                                                           │           │
│  │   FPS avg    338 (+8.3%)  1% Low  221 (+20%)  Temp 69°C   │           │
│  │                                                           │           │
│  │   ──────────────────────────────────────                  │           │
│  │   [L-shape frametime graph — 60s capture]                 │           │
│  │   ──────────────────────────────────────                  │           │
│  ╰──────────────────────────────────────────────────────────╯            │
│                                                                         │
│  ╭─── Comparison ──────────────────────────────────────────╮            │
│  │   [Overlay both L-shapes in same chart]                  │           │
│  │   Green line = After, Gray line = Before                 │           │
│  ╰──────────────────────────────────────────────────────────╯            │
│                                                                         │
│  [Export as image]  [Export CSV]  [Share comparison]                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

Notas:
- Inspirado en CapFrameX comparison tab.
- Cards stacked con valores + delta relativa.
- L-shape graphs (percentile distribution).
- Overlay para comparación directa.
- Export/share para compartir benchmarks (valor viral).

---

## Resumen ejecutivo de recomendaciones

1. **Stack técnico**: **C# + WPF + WPF-UI (Lepo.co) + LiveCharts2 + Windows Community Toolkit RadialGauge**. Empaquetar con MSIX o Velopack.
2. **Tema visual único**: dark mode sobrio, accent purpúreo eléctrico (#7C5CFF) diferenciando de competidores, Mica backdrop Win11 nativo.
3. **Layout**: sidebar colapsable + content area + inspector slide-in. NO modales excepto destructivas.
4. **Killer features a construir primero**: (1) Health Score radial en dashboard, (2) Pro Config import con diff visible, (3) Per-game tabs con anti-cheat awareness, (4) Benchmark before/after con L-shape graph, (5) Command palette `Ctrl+K`.
5. **Gamification y hype**: evitar. Tone técnico-honesto. Cada claim con fuente. Severity visible per tweak. Reversibilidad como valor central.
6. **Onboarding**: 5 pantallas max, skipable, con scan inicial y pick-profile. Returning user directo al dashboard.
7. **Microcopy**: acción + claridad + honestidad. "May improve" > "Will BOOST".
8. **Antipatterns a evitar explícitamente**: scareware, RGB vomit, fuentes gamer en body, collapsibles ocultando primaries, force login, ads internos.

**Esta UI puede competir con BoosterX/Razer Cortex/Hone sin copiarlos — basada en los mismos principios de los mejores (Linear, Raycast, Stripe, NVIDIA App 2024) aplicados al contexto gaming con respeto por el usuario pro.**

---

## Sources

- [BoosterX — Home](https://boosterx.org/en/)
- [Best PC Optimizer For Gaming in 2026 Guide — TechBre](https://www.techbre.com/best-pc-optimizer-for-gaming/)
- [BoosterX — xda-developers review](https://www.xda-developers.com/i-use-this-utility-to-boost-my-pcs-gaming-performance/)
- [Razer Cortex — Razer official](https://www.razer.com/cortex)
- [Razer Cortex Review 2026 — SoftOCoupon](https://www.softocoupon.com/review/pc-optimization/razer-cortex-game-booster-review/)
- [Razer Cortex review — TechRadar](https://www.techradar.com/reviews/razer-cortex)
- [Hone.gg — Optimize your PC for gaming](https://hone.gg/)
- [Hone Recommended Optimizations](https://support.hone.gg/hc/en-gb/articles/4758238924959-Recommended-Optimizations-to-Use)
- [NVIDIA App (Tom's Guide)](https://www.tomsguide.com/gaming/the-new-nvidia-app-is-here-to-replace-geforce-experience-heres-whats-new)
- [NVIDIA App officially replaces GeForce Experience — Tom's Hardware](https://www.tomshardware.com/pc-components/gpu-drivers/nvidia-bids-goodbye-to-geforce-experience-nvidia-app-officially-replaces-it-in-the-latest-driver-update)
- [I used the new Nvidia app for a week — XDA](https://www.xda-developers.com/nvidia-app-hands-on/)
- [Chris Titus WinUtil — GitHub](https://github.com/ChrisTitusTech/winutil)
- [Windows Utility in 2026 — Chris Titus](https://christitus.com/winutil-in-2026/)
- [WinUtil UI criticism — GitHub discussion](https://github.com/ChrisTitusTech/winutil/discussions/3240)
- [WinUtil UI improvements proposal — GitHub issue 2226](https://github.com/ChrisTitusTech/winutil/issues/2226)
- [hellzerg Optimizer — GitHub](https://github.com/hellzerg/optimizer)
- [AMD Adrenalin Release Notes 23.12.1](https://www.amd.com/en/resources/support-articles/release-notes/RN-RAD-WIN-23-12-1.html)
- [AMD Software: Adrenalin Edition 2024](https://www.thefpsreview.com/2023/12/05/amd-software-adrenalin-edition-23-30-13-01-adds-game-support-for-avatar-frontiers-of-pandora-a-software-ui-redesign-hardware-accelerated-gpu-scheduling-and-hypr-rx-eco-for-power-savings/)
- [Intel Graphics Software (replacing Arc Control) — Windows Central](https://www.windowscentral.com/hardware/cpu-gpu-components/intel-graphics-software-announcement)
- [Intel Graphics Software — VideoCardz](https://videocardz.com/newz/intel-introduces-new-graphics-software-with-expanded-oc-tools-replacing-arc-control-software)
- [Razer Synapse 4 — Windows Central](https://www.windowscentral.com/software-apps/razer-synapse-4-launch)
- [Razer Synapse 4 — TechPowerUp](https://www.techpowerup.com/327100/razer-synapse-4-brings-a-new-user-interface-and-up-to-30-performance-boost?cp=1)
- [Logitech G HUB Games — Logitech blog](https://www.logitech.com/blog/2025/09/17/personalized-gaming-perfectly-organized-introducing-logitech-g-hub-games/)
- [Logitech G HUB software](https://www.logitechg.com/en-us/software/ghub)
- [NZXT CAM — Official](https://nzxt.com/camapp)
- [NZXT CAM 3.0 Review — Modders Inc](https://www.modders-inc.com/nzxt-cam3/)
- [Corsair iCUE — Official](https://www.corsair.com/us/en/s/icue)
- [Corsair iCUE 5.36 update — Vortez](https://www.vortez.net/news_story/corsair_rolls_out_icue_5_36_update_with_new_widgets_and_major_stability_fixes.html)
- [SteelSeries GG 67.0.0 release notes](https://techblog.steelseries.com/2024/07/23/GG-notes-67.0.0.html)
- [ASUS Armoury Crate 6 — ROG](https://rog.asus.com/articles/guides/armoury-crate-version-6-is-here-optimizing-your-gaming-pc-easier-than-ever/)
- [MSI Afterburner + RTSS Overlay Guide — wccftech](https://wccftech.com/how-to-set-up-high-quality-performance-overlays-with-rtss/)
- [CapFrameX — Features](https://www.capframex.com/features)
- [CapFrameX — GitHub](https://github.com/CXWorld/CapFrameX)
- [Intel PresentMon — Intel Gaming Access](https://game.intel.com/us/intel-presentmon/)
- [PresentMon 2.5.0 — VideoCardz](https://videocardz.com/newz/presentmon-2-5-0-adds-multi-device-metrics-support-and-enhanced-metrics)
- [HWiNFO — Official](https://www.hwinfo.com/)
- [HWiNFO Jonitor dashboards](https://www.hwinfo.com/forum/threads/jonitor-v1-1-0-build-your-own-monitoring-dashboards.6511/)
- [ModernWpf (Kinnara) — GitHub](https://github.com/Kinnara/ModernWpf)
- [WPF-UI (Lepo) — GitHub](https://github.com/lepoco/wpfui)
- [WPF-UI documentation](https://wpfui.lepo.co/)
- [MaterialDesignInXamlToolkit — GitHub](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [MaterialDesignInXamlToolkit vs HandyControl — LibHunt](https://dotnet.libhunt.com/compare-materialdesigninxamltoolkit-vs-handycontrol)
- [WinUI vs WPF vs UWP — Avalonia UI Blog](https://avaloniaui.net/blog/winui-vs-wpf-vs-uwp)
- [WPF vs Avalonia — SciChart](https://www.scichart.com/blog/wpf-vs-avalonia/)
- [What UI Framework for .NET Desktop? — Claudio Bernasconi](https://claudiobernasconi.ch/blog/dotnet-user-interface-frameworks-selection/)
- [Tauri vs Electron 2026 — Tech-Insider](https://tech-insider.org/tauri-vs-electron-2026/)
- [Tauri vs Electron vs Wails vs Flutter — HN discussion](https://news.ycombinator.com/item?id=43983715)
- [Cross-Platform Dev Tools 2026](https://codenote.net/en/posts/cross-platform-dev-tools-comparison-2026/)
- [LiveCharts2 — Official site](https://livecharts.dev/)
- [LiveCharts2 Real Time Sample](https://livecharts.dev/docs/WPF/2.0.0-rc5/samples.general.realTime)
- [LiveCharts2 — GitHub](https://github.com/Live-Charts/LiveCharts2)
- [SciChart WPF — features](https://www.scichart.com/wpf-chart-features/)
- [GLGraph — GitHub](https://github.com/varon/GLGraph)
- [RealTimeGraphX — GitHub](https://github.com/royben/RealTimeGraphX)
- [Windows Community Toolkit RadialGauge](https://learn.microsoft.com/en-us/windows/communitytoolkit/controls/radialgauge)
- [Fluent Design System — Wikipedia](https://en.wikipedia.org/wiki/Fluent_Design_System)
- [Materials used in Windows apps — Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/materials)
- [Acrylic material — Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic)
- [Fluent 2 Design System — Material](https://fluent2.microsoft.design/material)
- [Linear — How we redesigned the UI](https://linear.app/now/how-we-redesigned-the-linear-ui)
- [Linear case study — Radix Primitives](https://www.radix-ui.com/primitives/case-studies/linear)
- [Which UI libraries support Linear aesthetic — LogRocket](https://blog.logrocket.com/ux-design/linear-design-ui-libraries-design-kits-layout-grid/)
- [Raycast — Fresh look and feel](https://www.raycast.com/blog/a-fresh-look-and-feel)
- [Raycast — UX Collective](https://uxdesign.cc/raycast-for-designers-649fdad43bf1?gi=57cead3c1950)
- [Stripe Dashboard — UIBakery templates](https://uibakery.io/templates/stripe-dashboard)
- [Stripe Apps — Design guidelines](https://docs.stripe.com/stripe-apps/design)
- [1Password 8 — The Story So Far](https://blog.1password.com/1password-8-the-story-so-far/)
- [1Password for Linux — Dave Teare](https://dteare.medium.com/behind-the-scenes-of-1password-for-linux-d59b19143a23)
- [EAC vs BattlEye vs Vanguard 2026 — TATEWARE](https://tateware.com/blog/anti-cheat-comparison-2026)
- [Anti-Cheat Systems Explained — InjectKings](https://injectkings.com/info/anti-cheat-explained)
- [apEX CS2 settings — ProSettings](https://prosettings.net/players/apex/)
- [SETTINGS.GG — Find pro configs](https://settings.gg/)
- [CS2 Best Settings 2026 — ProSettings](https://prosettings.net/guides/cs2-options/)
- [Sidebar Design for Web Apps 2026 — ALF Design Group](https://www.alfdesigngroup.com/post/improve-your-sidebar-design-for-web-apps)
- [Tabs UX Best Practices — Eleken](https://www.eleken.co/blog-posts/tabs-ux)
- [Dashboard UX Best Practices 2026 — DesignRush](https://www.designrush.com/agency/ui-ux-design/dashboard/trends/dashboard-ux)
- [Toggle UI design guide — Setproduct](https://www.setproduct.com/blog/toggle-switch-ui-design)
- [Fluent UI vs MUI — UXPin](https://www.uxpin.com/studio/blog/fluent-ui-vs-mui/)
- [Best Design Systems Examples 2026](https://designsystems.surf/articles/11-best-design-system-examples-in-2026)
- [Microcopy UX — Userpilot](https://userpilot.com/blog/microcopy-ux/)
- [UX Writing Tips — Smashing Magazine](https://www.smashingmagazine.com/2024/06/how-improve-microcopy-ux-writing-tips-non-ux-writers/)
- [User Onboarding Best Practices 2026 — Formbricks](https://formbricks.com/blog/user-onboarding-best-practices)
- [Onboarding UX — Appcues](https://www.appcues.com/blog/user-onboarding-ui-ux-patterns)
- [Semantic Colors in UI — Medium](https://medium.com/@zaimasri92/semantic-colors-in-ui-ux-design-a-beginners-guide-to-functional-color-systems-cc51cf79ac5a)
- [Don't Fall for Gamer Product Scam — GamesGrace](https://gamesgrace.com/gamer-product-scam/)
- [User Inyerface — Worst practice UI](https://userinyerface.com/)
- [UI Antipatterns — UI-Patterns](https://ui-patterns.com/blog/User-Interface-AntiPatterns)
- [Atlas OS — GitHub](https://github.com/Atlas-OS/Atlas)
- [JetBrains Toolbox App 2.4](https://blog.jetbrains.com/toolbox-app/2024/07/toolbox-app-2-4-gets-a-refreshed-ui-theme-right-click-context-menu-and-other-updates/)
- [JetBrains New UI](https://www.jetbrains.com/help/idea/new-ui.html)
- [WinUI 3 — Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [WinUI 3 performance optimization — Microsoft](https://learn.microsoft.com/en-us/windows/apps/develop/performance/winui-perf)
- [Dark Dashboard designs — Dribbble](https://dribbble.com/tags/dark-dashboard)
- [Gaming Dashboard UI Design — Dribbble](https://dribbble.com/search/gaming-dashboard-ui-design)
- [WPF-Circular-Gauge — GitHub](https://github.com/mesta1/WPF-Circular-Gauge)
- [WPFSpark — GitHub](https://github.com/ratishphilip/wpfspark)
- [MicaWPF — GitHub](https://github.com/Simnico99/MicaWPF)
