I now have enough research. Let me compile the deliverable.

# Plan de Ecosistema PRODUCCIÓN — Windows 11 Gaming Optimizer

Gap final 10/10. Markdown extenso con decisiones concretas, ejemplos REALES (Chris Titus, Atlas, Velopack, SignPath Foundation) y pricing 2026.

---

## 0. Contexto: de "script DIY" a producto

**Los 3 proyectos referencia con usuarios activos:**

| Proyecto | Stack | Modelo distribución | Monetización |
|---|---|---|---|
| Chris Titus WinUtil | PowerShell (compilado .ps1 monolítico) | `irm christitus.com/win \| iex` + `.exe` wrapper $10 | Store cttstore.com + GitHub Sponsors + YouTube |
| AME Wizard / Atlas | YAML playbooks + TrustedUninstaller CLI (MIT C#) | `.apbx` (zip renombrado, pw `malte`) + AME Wizard GUI (cerrada) | Donaciones + Patreon |
| Winhance / Privatezilla | .NET WPF (Winhance) / C# (Privatezilla) | GitHub Releases `.exe` | Donaciones |
| Velopack users (GitHub Desktop, Slack históricamente Squirrel) | .NET / Electron | Installer + background auto-update | Comercial |

**Lección:** el **one-liner `irm | iex` de Chris Titus** es el killer pattern de crecimiento (cero fricción, siempre latest), pero asume usuarios con PowerShell y admin. Para consolidar producto + construir reputación SmartScreen, necesitás **ADEMÁS** un binary signed estable.

---

## 1. Auto-update strategy — recomendación concreta

**Decisión: modelo híbrido tri-channel.**

1. **Canal "Quick Launch" (stable)** — one-liner PowerShell:
   ```powershell
   irm "https://tu-dominio.gg/run" | iex
   ```
   Sirve a usuarios que solo quieren "aplicar tweaks y cerrar". Internamente descarga el `.ps1` compilado desde GitHub raw (CDN con Cloudflare por delante para rate limiting). Pros: zero install, siempre latest, cero fricción. Cons: cada ejecución = download (~1-3MB), no hay persistencia UI, impacto SmartScreen nulo porque no se firma un binario.

2. **Canal "Installed" (WPF/.NET app)** — binary firmado con **Velopack** como framework auto-update.
   - Velopack es sucesor de Squirrel.Windows, escrito en Rust, delta updates, <2s relaunch sin UAC, 100% free, activo (3900+ commits).
   - Squirrel.Windows está deprecado (último release 2020), no usar.
   - Setup: `vpk pack` en CI, genera `.exe` installer + `.nupkg` deltas subidos a GitHub Releases. Cliente chequea vía Velopack SDK en background.
   - Ventaja clave vs Squirrel: Velopack mantiene el exe path estable entre updates (Squirrel lo cambiaba cada version → rompía shortcuts y anti-cheat fingerprints).

3. **Canal "Package manager"** — publicar manifest en **winget** + **Scoop bucket**.
   - winget: YAML manifest via `wingetcreate new`, submit PR a `microsoft/winget-pkgs`, auto-validated. Una vez aceptado, `winget upgrade` funciona gratis.
   - Scoop: crear bucket propio (repo git con JSON por app). Power users prefieren esto.
   - Chocolatey community: opcional, proceso más lento (moderation manual).

**NO recomiendo:**
- ClickOnce (legacy, UX pobre, roto en .NET Core sin workarounds).
- MSIX como primario (ver §2 — limitaciones con Task Scheduler/servicios son show-stopper para un gaming optimizer).
- AppInstaller `.msix` vía Microsoft Store como único canal (pierde usuarios sin Store).

**Para el canal PowerShell (1):** implementar manifest/checksum validation — antes de `iex` comparar SHA256 del script descargado contra manifest firmado. Ver [WinUtil como referencia](https://winutil.christitus.com/userguide/): su compile step genera `winutil.ps1` determinístico.

**Rollback:** Velopack mantiene `current/` y `packages/` directorios; si una versión crashea en boot, auto-rollback al anterior. Para la PowerShell variante, el `UndoScript` por tweak en el JSON schema (ver §3) permite revert granular.

---

## 2. Distribución / packaging — formato recomendado

**Decisión: tri-format.**

1. **Portable `.exe` (primary)** — .NET self-contained single-file (Avalonia o WPF), ~30-60MB, corre de USB sin install. Config en mismo dir (modo portable auto-detectado si existe `portable.txt` al lado del exe).

2. **Installer `.exe` (Inno Setup)** — gratis, Pascal scripting, producto profesional. Crea shortcuts, entrada en Add/Remove Programs, carpeta `%LOCALAPPDATA%\TuApp\`. **Velopack lo genera por defecto**, evita manejar Inno Setup directo.

3. **MSIX** — solo si querés publicar en Microsoft Store. **NO uses como primary**: no soporta todos los casos como remoting y ejecución por servicios a nivel sistema como Task Scheduler, lo cual rompe todo watch-mode que dependa de schedule tasks elevadas.

**Comparativa installers libres:**

| | Inno Setup | WiX (MSI) | NSIS |
|---|---|---|---|
| Learning curve | Baja (wizard IDE) | Alta | Media |
| Output | `.exe` | `.msi` nativo | `.exe` |
| Best for | Apps consumer | Enterprise GPO deploy | Max customization, min tamaño |
| 2026 state | Activo, "definitive 2026 developer review" positivo | MSI deprecando para consumer, vivo enterprise | 3.11 (marzo 2025), vivo |
| Recomendación | ✅ Sí (o Velopack envuelve) | Solo si targeteas corporate | Opcional |

**PowerShell wrapper:** si seguís stack PS+WPF, usar **PS2EXE 1.0.17** (agosto 2025, flag `-embedFiles` nuevo). Problema: PS2EXE empaqueta el script en .NET exe pero NO compila a native → AV a veces lo marca. Solución: firmar EV + construir reputación.

---

## 3. Code signing — provider + costos 2026

**MOMENTO CRÍTICO — las reglas cambiaron.**

### Cambios 2026:
- **Desde 1 marzo 2026:** certificados code signing duran MAX 460 días (~15 meses) por CA/B Forum.
- **EV certs ya NO bypassean SmartScreen inmediatamente** (cambio marzo 2024). Hay que construir reputación aún con EV.
- Al renovar, la reputación NO se transfiere al nuevo cert automáticamente. **Workaround:** dual-sign (firmar el instalador con el cert viejo + el nuevo) durante el período de transición.

### Opciones 2026:

| Provider | Tipo | Precio/año | Bypass SmartScreen |
|---|---|---|---|
| **SignPath Foundation** | OV via Foundation, HSM cloud | **$0** (OSS only) | No inmediato, pero con binario built-from-source verificable |
| **Azure Artifact Signing (ex-Trusted Signing) Basic** | Public Trust, cloud HSM | ~$120/año ($9.99/mo, 5000 sigs) | No EV, construye reputación |
| **Azure Artifact Signing Premium** | Public Trust, 10 profiles | ~$1200/año ($99.99/mo, 100k sigs) | No EV |
| **Sectigo EV** | EV | ~$279-296/año (signmycode, ssl2buy) | Casi inmediato pero 2024+ necesita reputación igual |
| **DigiCert EV** | EV | ~$409-576/año | Idem Sectigo, brand premium |

### Recomendación específica para tu caso (gaming OSS con app que modifica sistema):

**Stack de 2 caminos paralelos:**

1. **Primary: SignPath Foundation (gratis)** — Requisitos: proyecto 100% OSS en repo público, build reproducible, cada release requiere manual approval pero se automatiza con GitHub Actions. El certificado es emitido a "SignPath Foundation" como publisher (NO a vos personal), lo que es un **pro de trust** porque Foundation verifica binary == built-from-source. Ejemplo en producción: Stellarium, GitExtensions, Flameshot, Super Productivity. Link: [signpath.org/apply](https://signpath.org/).

2. **Secondary: Azure Artifact Signing Basic ($120/año)** — solo si SignPath no acepta o querés publisher name propio. Pro: integrado con GitHub Actions sin HSM físico, pay-per-signature escalable.

**NO recomiendo:**
- **DigiCert EV $400-600/año** — overkill para OSS con SignPath disponible y sin bypass instantáneo garantizado.
- **Self-signed** — SmartScreen SIEMPRE lo bloqueará, útil solo dev local.

### SmartScreen reputation building — plan realista:

- Meses 0-3: ~50-500 downloads → Warning "Unrecognized app" activo.
- Meses 3-6: 1k-10k downloads verificados + signature consistente → warning baja.
- Meses 6-12: 10k+ downloads → bypass consistente.

**Acelerador:** dual-sign tu primer release con el cert nuevo Y el cert viejo (si tenés uno previo que ya pasó reputación). Ver [Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/5584097/how-to-bypass-windows-defender-smartscreen-even-af) y [advancedinstaller guide](https://www.advancedinstaller.com/prevent-smartscreen-from-appearing.html).

### Authenticode basics:
- Mandatorio SHA256 (SHA1 deprecado desde 2016).
- **Timestamping obligatorio** — sin `/tr http://timestamp.digicert.com /td sha256` la firma expira cuando expira el cert. Con timestamp: binarios quedan válidos indefinidamente.
- Doble firma: `signtool sign /fd sha256 /as /tr ...` permite dos signatures en un mismo archivo.

---

## 4. Community profiles — schema + backend sugerido

### Schema JSON — basado en el formato real de WinUtil (`config/tweaks.json`):

```json
{
  "CS2_Pro_Baseline_v1": {
    "Content": "CS2 Pro Baseline",
    "Description": "Baseline tweaks applied by competitive CS2 pros (shroud/s1mple equivalent)",
    "Author": "community_handle",
    "Game": "cs2",
    "AntiCheatAware": true,
    "Category": "Competitive FPS",
    "Version": "1.2.0",
    "MinAppVersion": "2.5.0",
    "SchemaVersion": 2,
    "registry": [
      {
        "Path": "HKLM:\\SYSTEM\\CurrentControlSet\\Services\\mouclass\\Parameters",
        "Name": "MouseDataQueueSize",
        "Value": "100",
        "Type": "DWord",
        "OriginalValue": "<RemoveEntry>"
      }
    ],
    "service": [
      {
        "Name": "SysMain",
        "StartupType": "Disabled",
        "OriginalType": "Automatic"
      }
    ],
    "ScheduledTask": [],
    "InvokeScript": ["powercfg -setactive SCHEME_MIN"],
    "UndoScript": ["powercfg -setactive SCHEME_BALANCED"],
    "Benchmark": {
      "MeasuredDelta": "+8% avg FPS",
      "MeasuredOn": "RTX 4070 + 13600K",
      "Methodology": "PresentMon 5min dust2 aim_botz"
    },
    "Signature": "ed25519:abc123...",
    "SignedBy": "publisher_pubkey"
  }
}
```

### Backend — qué elegir:

| Opción | Modelo | Costo | Trust model |
|---|---|---|---|
| **GitHub como storage** (Chris Titus) | `raw.githubusercontent.com/org/profiles-repo/main/...` + PR flow | Gratis | Git history, firmas GPG del maintainer |
| Gist Discovery | Users comparten gist URL | Gratis | Author trust manual |
| **Supabase** (self-host free OSS) | Postgres + auth + CDN | $0-25/mo | Centralizado |
| PocketBase (self-host single binary) | SQLite + API | VPS $5/mo | Centralizado |
| IPFS | Pinata + CID | $20/mo pinning | Descentralizado, overkill |

**Recomendación: GitHub repo separado (`tu-org/community-profiles`)** con estructura:

```
/profiles
  /games
    /cs2/
      prosettings-niko-v1.json
      prosettings-s1mple-v2.json
  /use-cases
    /competitive-fps/
    /streaming/
    /productivity/
  /authors
    /_index.json         # pubkey registry
```

Pros: zero infra cost, version control nativo, diffs auditables, PR flow = peer review, GitHub Search funciona como discovery. Contras: scaling >10k profiles pide search API custom.

### Versionado: semver para profiles

- MAJOR: schema breaking (nuevo field requerido, cambio de Type).
- MINOR: nuevos tweaks añadidos a un profile existente.
- PATCH: fix de un value incorrecto.

### Signing de profiles (anti-malicious):

**Esquema Ed25519** (ligero, rápido):
- Cada author genera keypair local. Pubkey pushed a `_index.json`.
- Antes de aplicar un profile downloaded, la app valida:
  1. `signature` campo corresponde a `SignedBy` pubkey.
  2. `SignedBy` está en el index oficial verificado (o warn amarillo si es unknown).
  3. Diff visual: app muestra QUE TWEAKS APLICARÁ antes de ejecutar. Ningún tweak ciego.
- Librería: **libsodium** (C#: `NSec.Cryptography`, PS: via P/Invoke).

### Discovery UX:
- Tab "Community" en la app → query GitHub API contents → filter by Game, AntiCheatAware, rating.
- Stars contadas via GitHub Reactions en el archivo de profile.
- Reports: "Report this profile" → opens GitHub issue prefilled.

---

## 5. Telemetry opt-in — stack privacy-first

**Decisión: Aptabase.**

### Por qué Aptabase gana sobre alternativas:

| | PostHog | Plausible | Umami | **Aptabase** |
|---|---|---|---|---|
| Target | Web + product | Web solo | Web solo | **Mobile + desktop + web** |
| Self-host | Sí (complex, docker stack) | Sí | Sí (simple) | Sí (simple) |
| License | MIT (dual) | AGPLv3 | MIT | **AGPLv3** |
| Desktop SDK | Vía HTTP custom | Vía HTTP custom | Vía HTTP custom | **SDKs nativos: Tauri, Unity, Unreal, Swift, Kotlin, Flutter, RN** |
| Privacy default | Bad (PostHog tracks itself without opt-out por defecto) | Good | Good | **Excellent — no device IDs, no cookies, no fingerprint** |
| User-level analytics | Sí | No | No | **No** (by design) |
| GDPR/CCPA/PECR | Compliant con setup | Compliant default | Compliant default | **Compliant default** |
| Cloud pricing | $0 free → $450+/mo | €9/mo | Free self-host | Free <1k events/mo → $20/mo |

**Aptabase gana porque:** (a) SDK nativo para desktop apps (no HTTP crudo), (b) "privacy-first and simple" explícito sin user identifiers, (c) decide EU vs US datacenter o self-host, (d) licencia AGPLv3 asegura que forks del servidor se mantengan open.

### Qué medir (respetando privacy):

```csharp
// C# example con SDK Aptabase
var analytics = new AptabaseClient(new InitOptions(
    AppKey: "A-EU-XXXX",
    IsDebug: false
));

// ✅ OK
analytics.TrackEvent("tweak_applied", new Dictionary<string, object>{
    {"tweak_id", "cs2_mouse_queue"},  // sin PII
    {"cpu_family", "intel_13th_gen"},  // hardware agregado
    {"gpu_vendor", "nvidia_rtx_40"},
    {"os_build", "26100.2222"}
});

analytics.TrackEvent("benchmark_delta", new Dictionary<string, object>{
    {"game", "cs2"},
    {"fps_delta_pct", 8}
});
```

### Prohibido medir:
- IP address (Aptabase la hashea + drops, configurable).
- Usernames, paths personales (`C:\Users\IgnacioSoto\...` → normalizar a `%USERPROFILE%`).
- Lista exacta de juegos instalados (solo presence_of_game bool por game soportado).
- Network traffic.
- Contenido de configs custom del user.

### Opt-in explícito:
- First-run wizard: pantalla DEDICADA "Help us improve" con:
  - Toggle OFF por default.
  - Explicación plain-language de qué se manda con ejemplos reales.
  - Link a privacy policy (SECURITY.md style).
  - "What data will be sent" button expande lista completa.
- **Nunca pre-tickeado.** "Opt-out buried in settings" es el anti-patrón (PostHog autocapture controversy: [issue #790](https://github.com/PostHog/posthog/issues/790)).

### Compliance base:
- GDPR Art. 6(1)(a): consent explícito.
- CCPA: "Do Not Sell My Info" — no vendés, pero agregá la línea.
- LGPD (Brasil, similar a GDPR): mismo mecanismo cubre.
- DPA (UK): idem.

---

## 6. Error reporting — Sentry setup

**Sentry.io** sigue siendo gold standard. .NET SDK 4.0+ soporta crash reporting nativo en Windows/macOS/Linux.

### Setup WPF ejemplo:

```csharp
// App.xaml.cs
SentrySdk.Init(o => {
    o.Dsn = "https://key@sentry.io/project";
    o.SendDefaultPii = false;  // CRÍTICO: NO enviar machine name / user
    o.AttachStacktrace = true;
    o.TracesSampleRate = 0.1;
    o.AutoSessionTracking = true;
    o.Release = typeof(App).Assembly.GetName().Version.ToString();
    o.Environment = "production";
    o.BeforeSend = (sentryEvent) => {
        // Sanitize paths: redact usernames
        if (sentryEvent.Exception?.ToString().Contains(Environment.UserName) == true) {
            var sanitized = sentryEvent.Exception.ToString()
                .Replace(Environment.UserName, "<USER>");
            // ... reconstruct
        }
        return sentryEvent;
    };
});

// Global handler WPF
DispatcherUnhandledException += (s, e) => {
    SentrySdk.CaptureException(e.Exception);
    // Show crash dialog
};
```

### Self-host option:
- Free self-hosted via docker-compose en VPS Linux (4 cores, 16GB RAM, 20GB disk mínimo).
- Si costo es issue: $25-50/mo Hetzner servidor. Cloud Sentry free plan tiene 5k errors/mo, suficiente para OSS chico.

### UX dialog crash:

```
┌──────────────────────────────────────────┐
│ ⚠ Something went wrong                    │
│                                           │
│ TuApp encountered an unexpected error.    │
│ Your tweaks are safe — nothing was        │
│ applied partially.                        │
│                                           │
│ Help us fix this?                         │
│ [ ] Include stack trace (recommended)     │
│ [ ] Include app version & OS build        │
│ [ ] Include hardware summary (no serials) │
│ [ ] Include your description:             │
│ ┌─────────────────────────────────────┐   │
│ │ (optional)                          │   │
│ └─────────────────────────────────────┘   │
│                                           │
│ [Send Report]  [Don't Send]  [Details]    │
└──────────────────────────────────────────┘
```

**NUNCA incluir:** screenshots, game content, chat logs, opened files, clipboard.

### User feedback loop:
Sentry User Feedback module permite al user describir el crash libre texto — lo attacha al event. Útil para priorizar.

**Alternativas comerciales si Sentry no cuaja:** Rollbar ($17/mo 5k events), Bugsnag (free <7500 events/mo), Raygun ($13/mo). **NO uses** Microsoft AppCenter (deprecated 2025, shutting down).

---

## 7. CI/CD completo — GitHub Actions skeleton

### Workflow `release.yml` (stack .NET/WPF + Velopack + SignPath):

```yaml
name: Release
on:
  push:
    tags:
      - 'v*'

permissions:
  contents: write
  id-token: write   # for SignPath trusted publishing

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0   # for release-please history

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore & Build
        run: |
          dotnet restore
          dotnet build -c Release --no-restore -p:Deterministic=true

      - name: Test
        run: dotnet test -c Release --no-build --logger:trx

      - name: Lint (Roslyn analyzers)
        run: dotnet format --verify-no-changes --severity warn

      - name: Publish self-contained
        run: |
          dotnet publish src/TuApp/TuApp.csproj `
            -c Release -r win-x64 --self-contained true `
            -p:PublishSingleFile=true -p:Deterministic=true `
            -p:SourceRevisionId=${{ github.sha }} `
            -o publish/

      - name: SBOM (CycloneDX)
        run: |
          dotnet tool install --global CycloneDX
          dotnet CycloneDX . -o sbom/ -f sbom.cdx.json

      - name: Velopack pack (creates installer + delta)
        run: |
          dotnet tool install -g vpk
          vpk download github --repoUrl https://github.com/${{ github.repository }}
          vpk pack --packId TuApp --packVersion ${{ github.ref_name }} `
                   --packDir publish/ --mainExe TuApp.exe `
                   --icon assets/icon.ico

      - name: Upload unsigned artifacts for signing
        uses: actions/upload-artifact@v4
        with:
          name: unsigned
          path: Releases/

  sign:
    needs: build
    runs-on: windows-latest
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: unsigned
          path: Releases/

      - name: Sign with SignPath
        uses: signpath/github-action-submit-signing-request@v1
        with:
          api-token: '${{ secrets.SIGNPATH_API_TOKEN }}'
          organization-id: '${{ vars.SIGNPATH_ORG_ID }}'
          project-slug: 'tu-app'
          signing-policy-slug: 'release-signing'
          artifact-configuration-slug: 'main'
          github-artifact-id: '${{ steps.upload.outputs.artifact-id }}'
          wait-for-completion: true
          output-artifact-directory: 'Releases/'

      - name: Upload signed artifacts
        uses: actions/upload-artifact@v4
        with:
          name: signed
          path: Releases/

  publish:
    needs: sign
    runs-on: windows-latest
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: signed
          path: Releases/

      - name: Velopack upload to GitHub Releases
        run: |
          vpk upload github --repoUrl https://github.com/${{ github.repository }} `
                            --publish --releaseName "${{ github.ref_name }}" `
                            --token ${{ secrets.GITHUB_TOKEN }} `
                            --tag ${{ github.ref_name }}

  winget:
    needs: publish
    runs-on: windows-latest
    steps:
      - name: Update winget manifest
        uses: vedantmgoyal9/winget-releaser@main
        with:
          identifier: TuOrg.TuApp
          version: ${{ github.ref_name }}
          installers-regex: '\.exe$'
          token: ${{ secrets.WINGET_TOKEN }}
```

### `release-please.yml` (changelog + semver automáticos):

```yaml
name: Release Please
on:
  push:
    branches: [main]

jobs:
  release-please:
    runs-on: ubuntu-latest
    steps:
      - uses: googleapis/release-please-action@v4
        with:
          release-type: simple
          package-name: tu-app
          changelog-types: |
            [
              {"type":"feat","section":"Features","hidden":false},
              {"type":"fix","section":"Bug Fixes","hidden":false},
              {"type":"perf","section":"Performance","hidden":false},
              {"type":"tweak","section":"New Tweaks","hidden":false}
            ]
```

### Security scanning:
- **Dependabot** (`.github/dependabot.yml`) — weekly dep updates.
- **CodeQL** (`.github/workflows/codeql.yml`) — free para OSS.
- **Trivy** para scan del `.exe` final (aunque Trivy es más container-oriented).

### Testing:
- .NET: xUnit (no NUnit — más activo, mejor async support).
- PowerShell: Pester 5.x + PSScriptAnalyzer.

---

## 8. Portable vs installed — decisión

**Decisión: hybrid con detección automática.**

Lógica en `Main()`:
```csharp
var exeDir = AppContext.BaseDirectory;
var portableMarker = Path.Combine(exeDir, "portable.txt");
var isPortable = File.Exists(portableMarker);

var configRoot = isPortable
    ? Path.Combine(exeDir, "data")
    : Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "TuApp");
```

### Distribuir 2 artifacts:
1. `TuApp-portable-v2.5.0.zip` — extract anywhere, crea `portable.txt` pre-incluido, escribe logs en `./data/`.
2. `TuApp-Setup-v2.5.0.exe` — Velopack installer normal, crea shortcuts, usa `%LOCALAPPDATA%`.

### Pros portable:
- USB gaming rigs en LAN parties.
- Sin admin para extract (para APLICAR tweaks sí necesitás admin igual).
- Testing sin polución en Program Files.

### Pros installed:
- Shortcut Start Menu, taskbar pin.
- Add/Remove Programs entry (requerido por compliance enterprise).
- Auto-update via Velopack.
- Protected Programs Files (harder for other processes to tamper).

**Recomendación:** installed como default en sitio web, portable como secondary "for USB / testing" link más discreto. Chris Titus hace el contrario (one-liner first) porque su use case es diferente (script, no app persistente).

---

## 9. Uninstaller — checklist completa

**Stack: lista de items a revertir, ordenados por criticidad.**

### A. Pre-uninstall (UX):
- [ ] Dialog "Export your config?" → `.json` backup a Documents.
- [ ] Checkbox "Create System Restore Point before uninstall?" (default ON).
- [ ] Checkbox "Revert all tweaks to original state?" (default ON, CRITICAL).

### B. Durante uninstall (orden):
1. **Parar servicios/tareas propias:**
   - [ ] Stop + delete `TuAppWatcherService`.
   - [ ] Unregister scheduled tasks `\TuApp\*`.
2. **Revertir tweaks aplicados** (lee `%LOCALAPPDATA%\TuApp\applied_tweaks.jsonl` — append-only log):
   - [ ] Registry keys: restore desde `OriginalValue` o delete si `<RemoveEntry>`.
   - [ ] Services: restore `OriginalType` (Automatic/Manual/Disabled).
   - [ ] Power plans: `powercfg -delete GUID` + `powercfg -setactive BALANCED`.
   - [ ] Execute cada tweak's `UndoScript`.
3. **Borrar archivos app:**
   - [ ] `%LOCALAPPDATA%\TuApp\` (después de preguntar, el user puede optar por keep logs).
   - [ ] Shortcuts Start Menu + Desktop.
   - [ ] Registry entry Uninstall subkey.
4. **Cleanup:**
   - [ ] Firewall rules si añadiste.
   - [ ] Environment variables PATH si añadiste.
   - [ ] Event log source registrado.

### C. Append-only log format:
```jsonl
{"ts":"2026-04-18T10:23:00Z","tweak":"cs2_mouse_queue","action":"apply","registry":{"path":"HKLM:...","name":"MouseDataQueueSize","before":null,"after":100}}
{"ts":"2026-04-18T10:23:01Z","tweak":"cs2_mouse_queue","action":"apply","service":{"name":"SysMain","before":"Automatic","after":"Disabled"}}
```

### D. Safety net — AtlasOS model:
- Antes de aplicar cualquier tweak, **crear System Restore Point** (`Checkpoint-Computer -Description "TuApp pre-tweak" -RestorePointType MODIFY_SETTINGS`).
- Atlas usa [TrustedUninstaller](https://github.com/Ameliorated-LLC/trusted-uninstaller-cli) (MIT, C#) como backend que hace exactamente esto. Podés usarlo de referencia o incluso importar como librería.

### E. Test matrix:
Probar uninstall con:
- App corriendo (force close).
- App con servicios activos.
- Tweaks aplicados vs sin aplicar.
- Windows Update en medio.
- Reboot pendiente.

---

## 10. Watch mode / process detection — arquitectura

**Stack: Process Watcher via WMI + Scheduled Task startup + opcional Service.**

### Detección de procesos (Razer Cortex / Process Lasso approach):

```csharp
using System.Management;

// Watch Win32_ProcessStartTrace (root\cimv2) para process starts
var startQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
var startWatcher = new ManagementEventWatcher(startQuery);
startWatcher.EventArrived += (s, e) => {
    var name = e.NewEvent.Properties["ProcessName"].Value.ToString();
    if (GameProfiles.TryGetValue(name, out var profile)) {
        ApplyProfile(profile);  // cs2.exe → CS2 profile
    }
};
startWatcher.Start();

// Watch Win32_ProcessStopTrace para revert on close
var stopQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
```

### Limitaciones WMI:
- Requiere admin.
- Polling ~1Hz (no real-time nanosec).
- ETW (Event Tracing for Windows) es más performante pero MUCHO más complejo (kernel-level, requiere `Microsoft.Diagnostics.Tracing.TraceEvent` NuGet).

### Arquitectura recomendada:

```
┌─────────────────────────────────────────────────┐
│ User logs in                                    │
└────┬────────────────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────────────────────┐
│ Scheduled Task "TuApp_Watcher" runs at logon    │
│  → launches TuApp.Watcher.exe (no UI)           │
└────┬────────────────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────────────────────┐
│ TuApp.Watcher (headless .NET, runs as user     │
│  with highest priv via Task Scheduler RunAsAdmin│
└────┬────────────────────────────────────────────┘
     │
     ├─ WMI ManagementEventWatcher (process start)
     ├─ WMI ManagementEventWatcher (process stop)
     ├─ Named pipe IPC con TuApp.UI (si está running)
     └─ Systray icon opcional (show/hide state)
     │
     ▼
┌─────────────────────────────────────────────────┐
│ on cs2.exe start:                               │
│  1. Check if anti-cheat active (skip tweaks    │
│     that touch memory regions)                  │
│  2. Apply ActiveProfile                         │
│  3. Log to applied_tweaks.jsonl                 │
│                                                 │
│ on cs2.exe stop:                                │
│  1. Execute UndoScript                          │
│  2. Restore power plan Balanced                 │
└─────────────────────────────────────────────────┘
```

### ¿Service vs Scheduled Task?
- **Scheduled Task "At log on" + RunWithHighestPrivileges"** = mejor UX (no requiere install service elevated, per-user, sin UAC popup).
- **Windows Service** = overkill para un gaming optimizer, complica uninstall, user lo ve en services.msc y se asusta.

Chris Titus no usa watch mode (enfoque one-shot), Razer Cortex sí (kernel driver — NO imitar, demasiada fricción para un OSS).

### Anti-cheat awareness:
- Mantener lista estática de tweaks **seguros con EAC/BattlEye/Vanguard** vs **peligrosos** (cualquier cosa que inyecte en proceso, hook syscalls, modifique memoria de otro proceso, load kernel driver).
- **Gaming optimizer = tweaks de OS/registry/services ONLY.** Nada que toque el proceso del juego.
- Profile flag `AntiCheatAware: true` = profile auditado y solo usa tweaks OS-level.
- UI warning rojo si profile `AntiCheatAware: false` + game tiene anti-cheat detectado.

---

## 11. i18n — prioridad idiomas + framework

### Stack por tecnología:

**Si WPF (.NET):**
- `.resx` files + `ResourceManager`. Standard, Visual Studio support nativo.
- XAML `{x:Static local:Strings.AppTitle}` bindings.
- Cambio en runtime: LocBaml o `CultureInfo.CurrentUICulture` + re-render.

**Si Avalonia:**
- `.resx` idem o `.json` custom. Community uses [Projektanker/Icons.Avalonia](https://github.com/Projektanker/Icons.Avalonia) patterns.

**Si PowerShell:**
- `Data {}` sections + `Import-LocalizedData`.
- Estructura: `es-CL/tuapp.strings.psd1`, `en-US/tuapp.strings.psd1`.

### Idiomas prioritarios gaming 2026 (por revenue esports + PC install base):

| Prio | Idioma | Rationale |
|---|---|---|
| 1 | **en-US** | baseline, docs, 50%+ downloads |
| 2 | **es-ES + es-419** | Latam es territorio gaming masivo, user es Chile |
| 3 | **pt-BR** | Brasil CS/Valorant enormes |
| 4 | **zh-CN** | PC gaming market más grande mundial |
| 5 | **ru** | esports CS/Dota históricamente |
| 6 | **de, fr** | EU |
| 7 | **ja, ko** | FGC + MMO Asia |

### Crowdsourcing tool:

| | Crowdin | Weblate |
|---|---|---|
| License | Proprietary (AI-enhanced) | GPL |
| OSS hosting | Free tier | **Gratis for libre projects** |
| Integrations | 600+ tools | Git-centric, tight |
| Setup | 5 min | 30 min (hosted) |

**Recomendación: Weblate libre hosting** (hosted.weblate.org free for OSS) — git integration directa, se genera PR por cada translation, maintainers aprueban. Completamente OSS, ethos matchea. Si preferís menos fricción para translators: Crowdin free tier (proprietary).

### UX:
- Settings → Language: auto-detect Windows UI culture por default. Override dropdown persistente.
- Never hardcodear strings (usar `Strings.ApplyTweak`). ESLint/Roslyn analyzer para catch.

---

## 12. Accessibility — checklist WPF

**WPF Accessibility (WCAG 2.1 AA target):**

- [ ] **Every Button/Control tiene `AutomationProperties.Name`** — screen readers lo leen.
  ```xaml
  <Button AutomationProperties.Name="Apply CS2 Pro Profile"
          Content="Apply" />
  ```
- [ ] **`AutomationProperties.HelpText`** en controles complejos.
- [ ] **Keyboard navigation completa** — `TabIndex`, `KeyboardNavigation.TabNavigation=Cycle`.
- [ ] **Focus visible** — `FocusVisualStyle` custom con outline claro, no solo color (AA contrast).
- [ ] **No uses solo color para conveyar info** — íconos + labels + color.
- [ ] **High contrast theme** — test con `Settings → Accessibility → Contrast themes → Aquatic`. WPF debe respetar SystemColors.
- [ ] **Font scaling** — respetar `SystemParameters.WindowTitleFontSize` y DPI awareness (`<application manifest>` PerMonitorV2).
- [ ] **Reduced motion** — respect `SystemParameters.ClientAreaAnimation` y disable transitions si OFF.
- [ ] **Accessible color contrast** — AA 4.5:1 texto normal, 3:1 large text. Test con [WebAIM contrast checker](https://webaim.org/resources/contrastchecker/).
- [ ] **Screen reader testing** — NVDA (free, open source). Download y run `NVDA + numpad_plus` para test. Speech Viewer muestra exactamente qué lee.
- [ ] **Narrator (Windows built-in)** — test secondary.
- [ ] **Captions en videos tutoriales** (YouTube auto-captions + manual review).

### WPF-specific pitfalls conocidos:
- [Narrator no reconoce AutomationProperties en algunos controles Telerik/3rd party](https://www.telerik.com/forums/automationproperties-not-reqognized-by-narrator) — test early, no al final.
- LiveSettings: WPF tiene `SetLiveSetting`/`GetLiveSetting` API para notificaciones accesibles. Ver [Microsoft WPF accessibility improvements docs](https://github.com/microsoft/dotnet/blob/main/Documentation/compatibility/wpf-accessibility-improvements.MD).

---

## 13. Documentation site — recomendación

### Comparativa 2026:

| | Docusaurus | MkDocs Material | mdBook |
|---|---|---|---|
| Stack | React + JS | Python + MD | Rust + MD |
| Config | JS/TS | YAML | TOML |
| Custom components | React embed | limited | Rust preprocessors |
| 2026 state | Activo, dominante React cat | **Entró maintenance mode nov 2025** (no new features) | Activo, usado por Rust Book |
| Versioning | Nativo | Plugin | Manual |
| Search | Algolia DocSearch | Lunr built-in | Local search |

**Recomendación: Docusaurus.**
- Versionado nativo (crítico para una app con múltiples releases documentadas).
- i18n nativo (matchea tu §11).
- Componentes React embeds para demos interactivas (screenshots animados, diff viewers).
- Deploy gratis en GitHub Pages o Cloudflare Pages.
- Chris Titus usa MkDocs ([winutil.christitus.com](https://winutil.christitus.com/)) — funciona pero entró maintenance mode.

**Alternativa minimalista:** mdBook si querés solo docs técnicas sin fancy UI (Rust Book, Rustonomicon, Cargo docs todas mdBook). Deploy en un `docs/` folder dentro del repo.

### Estructura mínima sitio docs:
```
/docs
  /getting-started
    installation.md
    first-run.md
    uninstall.md
  /tweaks
    registry-tweaks.md
    service-tweaks.md
    power-plans.md
  /per-game
    cs2.md
    valorant.md
    ...
  /community
    submit-profile.md
    schema-reference.md
    signing-profiles.md
  /development
    architecture.md
    contributing.md
    build-from-source.md
  /security
    privacy-policy.md
    data-collection.md
    security-policy.md
  /changelog.md
```

### Community channels:
- **Discord** server — core community. Canales #support #dev #community-profiles #benchmarks.
- **Reddit r/TuApp** — async, searchable, indexed by Google. Crucial para SEO "TuApp review reddit".
- **GitHub Discussions** — Q&A técnicas, persistentes.
- **YouTube channel** — tutoriales, release walkthroughs. 10-15min videos cada major release.

---

## 14. Monetización ética — options

**Ordenadas por compatibilidad OSS + gaming audience:**

### Tier 1 — baseline (todos OSS usan):
- **GitHub Sponsors** — 0% fees, reach developers. Chris Titus usa.
- **Ko-fi** — setup en <5min, 0% fees donación. `donatr.ee` link aggregates ambos.
- **Buy Me a Coffee** — 5% fee, 0% one-time donations plan.
- **OpenCollective** — transparent budget, good for team splits.

### Tier 2 — premium cloud features:
- **"Pro" tier $3-5/mo** — sync profiles multi-device, benchmark history cloud, priority community support.
- **Free core mantiene TODAS las funcionalidades de tweaking.** Cloud sync es convenience, no core feature.
- Stack: Supabase auth + Postgres + Row Level Security. Costo <$25/mo hasta 10k usuarios.

### Tier 3 — sponsorships:
- Brand placements en splash screen "Powered by X" — OK si no interrumpen UX (NZXT, NVIDIA, peripheral brands).
- **NUNCA:** ads intrusivos, pop-ups, "upgrade to remove ads".

### Tier 4 — merch (Chris Titus model):
- T-shirts, stickers, mugs via Printful + Shopify ($29/mo).
- Margin bajo (~20%) pero genera trust + community signal.
- cttstore.com ejemplo.

### Tier 5 — enterprise support:
- Contracts para gaming cafés, esports orgs, LAN event organizers.
- $500-5000/año por licencia con SLA.

### Tier 6 — Microsoft Store published:
- Permite cobrar via MS Commerce (30% fee Microsoft, 15% después año 1 — o 12% si usa non-Microsoft commerce).
- Auto-update por Store.
- **Limitación MSIX:** no acceso a scheduled tasks para services/watch mode → requiere Sparse signing o workaround.

### Qué EVITAR:
- Scareware tactics ("Your PC has 1,528 issues!").
- Upselling constante intrusivo.
- Feature hostage (core es solo-demo).
- Telemetry forzada sin opt-out.
- Cryptominers escondidos (pasó con otras apps).
- Bundled PUPs.

### Stack recomendado sustainable:

```
Total monthly goal: $500-2000
├── GitHub Sponsors:     ~$100-400 (10-40 sponsors x $10)
├── Ko-fi one-time:      ~$50-200
├── Pro tier $3/mo:      ~$150-600 (50-200 users)
├── Enterprise contracts: ~$200-800 (1-2 SLAs)
└── Merch:               ~$50-200
```

Escala gradual — no esperar todo del día 1. Chris Titus tomó 6 años + 30M runs para monetización significativa.

---

## 15. License — recomendación específica

**Decisión: MIT.**

### Rationale para gaming utility OSS:

| License | Pros | Cons para este caso |
|---|---|---|
| **MIT** | Máxima adopción, corp-friendly, simple | Permite fork comercial cerrado (OK) |
| Apache 2.0 | Patent clause explicit, enterprise-grade | Más verbose, cláusulas legales para casual devs |
| GPLv3 | Fuerza OSS en forks (copyleft) | **Show-stopper:** scares away corp contributors, gaming peripheral brands no pueden usar bits en firmware cerrado, AtlasOS usa GPLv3 y esto limita integraciones |
| AGPLv3 | Cubre SaaS | Overkill para desktop utility |

**MIT gana porque:**
1. Máxima compatibility con ecosistema Windows (drivers/SDKs propietarios conviven).
2. Contributors no se preocupan por viral licensing.
3. Podés re-licenciar a commercial si algún día querés (AtlasOS no puede fácil).
4. Chris Titus WinUtil: MIT. Winhance: MIT. Ecosistema normalizado.

### Disclaimers esenciales en README + UI:

```
DISCLAIMER
This software modifies Windows system settings. While we take care to
ensure safety and reversibility, you accept full responsibility for
any consequences. No warranty is provided. Always create a restore
point before applying tweaks.

This project is not affiliated with, endorsed by, or sponsored by
Microsoft, Valve, Riot Games, Respawn Entertainment, NVIDIA, AMD,
Intel, or any other third party. All trademarks are property of
their respective owners.

Anti-cheat software may flag system modification tools. We maintain
a curated list of AntiCheatAware profiles that use only OS-level
tweaks without process injection or memory modification. You assume
the risk of using profiles not marked as AntiCheatAware.
```

### Trademarks — cuidado real:
- Usá **CS2 logo** solo si tenés permission Valve (no lo vas a tener casualmente).
- En su lugar: texto "CS2" o íconos genéricos crosshair/FPS que diseñes vos.
- ProSettings.net tiene permission explícito para scraping de pro configs — si integrás, pedí permission o linkeá sin republish.

### DMCA prep:
- Repo template `.github/DMCA.md` con procedimiento takedown si alguien reporta infringement de un profile.

---

## 16. Marketing plan — launch checklist

### Phase 1 — pre-launch (6 semanas antes):

- [ ] Landing page — 1-pager con screenshots, video demo 30s, clear USP, mailing list signup.
- [ ] GitHub README killer — animated GIFs de UI, badges (CI, downloads, discord, sponsors), quickstart one-liner, comparison table vs alternatives.
- [ ] Twitter/X/Bluesky accounts + dev log threads (build in public).
- [ ] Discord server abierto con 10-50 early testers.
- [ ] 3-5 beta testers con hardware diverso que generen first reviews.
- [ ] SEO: `best-windows-11-gaming-optimizer`, `cs2-tweaks-free`, `valorant-fps-boost-2026`, etc. Páginas /blog/ en docusaurus.

### Phase 2 — launch day:

**Orden cronológico (UTC):**

1. **00:01 PST** — **Product Hunt launch** (Tue/Wed). Post comment maker inmediato. [Strategy guide](https://smollaunch.com/guides/launching-on-product-hunt).
2. **07:00 UTC** — **Show HN** (Hacker News) post. Title: "Show HN: TuApp – Open source Windows gaming optimizer (replaces Razer Cortex)". NO pidas upvotes.
3. **13:00 UTC** — **Reddit posts** espaciados 2h:
   - r/optimizedgaming (explicit value, not self-promo)
   - r/Windows11 (debloat angle)
   - r/pcgaming (performance angle)
   - r/GlobalOffensive, r/VALORANT, r/apexlegends (game-specific post por profile)
   - r/opensource (OSS angle)
   - r/selfhosted (community profiles angle)
4. **16:00 UTC** — **Twitter/X thread** con clips del video demo.
5. **Todo el día** — Discord live support, respond comments.

### Phase 3 — weeks 1-4 post-launch:

- [ ] YouTube outreach a **Chris Titus Tech** (fit perfecto para review), **ThioJoe**, **TechLinked/LTT**, **CyberCPU Tech**, **BriTec**, **Cody's Lab**. Free trial + beta access + "no strings" review.
- [ ] Update Product Hunt / HN con roadmap + user milestones ("500 users, here's what we heard").
- [ ] Post-launch blog: "Launch stats: X downloads in week 1, lessons learned."
- [ ] Push first community profile contributions — show gratitude, feature contributors.
- [ ] Iterate fast on bugs from Sentry data (§6).

### Badges README recomendados:
```markdown
![Build](https://img.shields.io/github/actions/workflow/status/org/repo/release.yml)
![Downloads](https://img.shields.io/github/downloads/org/repo/total)
![License](https://img.shields.io/github/license/org/repo)
![Discord](https://img.shields.io/discord/XXX?logo=discord)
![Sponsors](https://img.shields.io/github/sponsors/orgname)
![Version](https://img.shields.io/github/v/release/org/repo)
![GitHub stars](https://img.shields.io/github/stars/org/repo)
```

---

## 17. Project governance — modelo propuesto

**Decisión: Benevolent Dictator + growing Core Team.**

### Fase 1 (Year 1): Benevolent Dictator
- Vos = final decision maker.
- Community contribute via PR, pero merge es tuyo.
- Modelo Chris Titus: explícitamente "extremely picky about contributions to keep project clean and efficient." Funciona porque evita scope creep.

### Fase 2 (Year 2+): Core Team
- Cuando 5-10 contributors recurrentes emerjan, crear core team (CODEOWNERS file).
- Áreas: @ui-maintainer, @tweaks-curator, @ci-cd-maintainer, @docs-maintainer, @translations-maintainer.
- PR requires 1 core approval (2 para changes en `tweaks.json` registry — higher stakes).

### Fase 3 (si escala >100k users): Foundation
- Overkill hasta punto. Linux Foundation, Apache Software Foundation = 5+ años de runway, $25k+ inicial.
- Más realista intermedio: **OpenCollective fiscal host** (Open Source Collective) — tax-deductible donations sin incorporación legal. Free.

### Files de governance:
- `GOVERNANCE.md` — explícito quién decide qué.
- `MAINTAINERS.md` — lista actual.
- `CODEOWNERS` — file-level automation.
- `CODE_OF_CONDUCT.md` — [Contributor Covenant v2.1](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). Adoptado por 9 de los 10 mayores OSS del mundo.
- `CONTRIBUTING.md` — cómo contribuir, style guide, commit convention.
- `SECURITY.md` — cómo reportar vulns (email dedicado, GPG key).
- `.github/ISSUE_TEMPLATE/` — bug_report.yml, feature_request.yml, new_tweak.yml, new_profile.yml.
- `.github/PULL_REQUEST_TEMPLATE.md` — checklist for PRs.

### Bus factor mitigation:
- **Credentials en password manager compartido** (Bitwarden Organization free OSS plan) con 2 core members acceso.
- **Domain renewal auto-pay** + expiry calendar alerts.
- **Backup ADMIN emails** (no solo uno personal).
- **Certificate private keys:** si SignPath Foundation, ellos custodian. Si self-managed: escrow entre 2 maintainers + Shamir Secret Sharing (split key en 3, need 2 to reconstruct).
- **Docs "If I disappear" file** (private in org repo) — step-by-step handoff.

---

## Bonus §18 — Auditability + trust

Para un tool que modifica el sistema profundamente:

- [ ] **100% open source** — MIT (§15).
- [ ] **Reproducible builds** — `-p:Deterministic=true` + `SOURCE_DATE_EPOCH` para timestamps PE. Ver [reproducible-builds.org](https://reproducible-builds.org/docs/source-date-epoch/).
- [ ] **Signed commits** — all maintainers con GPG key on GitHub, "Verified" badge. Settings → "Require signed commits on main".
- [ ] **SBOM generado per-release** — CycloneDX (`dotnet CycloneDX`) upload como asset GitHub Release.
- [ ] **No obfuscation** — excepto en edge cases muy justificados (anti-tamper del propio binary, no anti-RE).
- [ ] **Clear privacy policy** — dedicated page en docs site.
- [ ] **Security audits community** — `SECURITY.md` + bug bounty tier (no requiere cash, solo hall of fame).
- [ ] **Changelog transparente** — release-please auto-generated.
- [ ] **Build instructions reproducibles** — `BUILD.md` con exact versions (`.NET 9.0.100`, `vpk v0.0.XXX`).

Esto es diferenciador MASIVO contra herramientas como Razer Cortex (cerradas, con kernel drivers opacos).

---

## Resumen ejecutivo — decisiones top

| Área | Decisión | Costo/año |
|---|---|---|
| Auto-update | Velopack + tri-channel (PS one-liner + installer + winget) | $0 |
| Distribución | Portable `.exe` + Installer via Velopack + winget manifest | $0 |
| Code signing | **SignPath Foundation (free OSS)** primary, Azure Artifact Signing Basic ($120) backup | $0-120 |
| Community profiles | GitHub repo dedicated + Ed25519 signing + schema JSON (WinUtil-inspired) | $0 |
| Telemetry | **Aptabase opt-in** | $0-240 |
| Error reporting | **Sentry free tier** (5k errors/mo) | $0 |
| CI/CD | GitHub Actions + release-please + SignPath action | $0 |
| Portable/installed | Hybrid con detección `portable.txt` | - |
| Uninstaller | Full rollback con applied_tweaks.jsonl + restore point pre-apply | - |
| Watch mode | WMI ManagementEventWatcher + Scheduled Task at login | - |
| i18n | Weblate (free OSS hosting) + .resx / .psd1 | $0 |
| Accessibility | WPF AutomationProperties + NVDA testing + WCAG 2.1 AA | - |
| Docs | **Docusaurus** + Cloudflare Pages deploy | $0 |
| Monetización | GitHub Sponsors + Ko-fi + Pro tier $3/mo (cloud sync) + merch | revenue |
| License | **MIT** | - |
| Marketing launch | PH + Show HN + Reddit + YouTube outreach (6-week buildup) | ~$0-100 dominio+landing |
| Governance | Benevolent Dictator → Core Team → OpenCollective fiscal | $0 |

**Costo total mínimo year 1: $0-300** (solo domain + opcional Azure signing). Escalable con revenue.

**Timeline realista a producto con usuarios activos:**
- Month 1-3: MVP + SignPath application + CI/CD + docs site.
- Month 4: beta Discord (50 testers).
- Month 5: Product Hunt launch.
- Month 6-12: build reputation (SmartScreen + community), hit 10k users.
- Year 2+: Pro tier, enterprise contracts, core team.

---

## Sources

- [GitHub - ChrisTitusTech/winutil](https://github.com/ChrisTitusTech/winutil)
- [Winutil Documentation - Chris Titus Tech](https://winutil.christitus.com/)
- [Windows Utility in 2026 — Everything That's Changed | Chris Titus Tech](https://christitus.com/winutil-in-2026/)
- [winutil/config/tweaks.json - WinUtil schema](https://github.com/ChrisTitusTech/winutil/blob/main/config/tweaks.json)
- [GitHub - velopack/velopack](https://github.com/velopack/velopack)
- [Velopack - A next-gen software installer and update framework](https://velopack.io)
- [Delta Updates | Velopack](https://docs.velopack.io/packaging/deltas)
- [From Squirrel | Velopack migration](https://docs.velopack.io/migrating/squirrel)
- [Azure Artifact Signing (formerly Trusted Signing)](https://azure.microsoft.com/en-us/products/artifact-signing)
- [Artifact Signing - Pricing | Microsoft Azure](https://azure.microsoft.com/en-us/pricing/details/artifact-signing/)
- [Code Signing With Azure Trusted Signing on GitHub Actions | Hendrik Erz](https://www.hendrik-erz.de/post/code-signing-with-azure-trusted-signing-on-github-actions)
- [SignPath Foundation — Free Code Signing for OSS](https://signpath.org/)
- [SignPath DevSec360 - The free Code Signing & Software Integrity solution for Open Source Projects](https://signpath.io/solutions/open-source-community)
- [DigiCert Code Signing Certificates](https://www.digicert.com/signing/code-signing-certificates)
- [Sectigo EV Code Signing Certificate $279](https://signmycode.com/sectigo-ev-code-signing)
- [Defender SmartScreen + EV Code Signing - Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/5584097/how-to-bypass-windows-defender-smartscreen-even-af)
- [How to avoid the Windows Defender SmartScreen warning - Advanced Installer](https://www.advancedinstaller.com/prevent-smartscreen-from-appearing.html)
- [Authenticode in 2025 – Azure Trusted Signing - text/plain](https://textslashplain.com/2025/03/12/authenticode-in-2025-azure-trusted-signing/)
- [Submit packages to Windows Package Manager | Microsoft Learn](https://learn.microsoft.com/en-us/windows/package-manager/package/)
- [GitHub - microsoft/winget-create](https://github.com/microsoft/winget-create)
- [Scoop Installer](https://scoop.sh/)
- [GitHub - ScoopInstaller/Scoop](https://github.com/ScoopInstaller/Scoop)
- [Atlas Documentation - Playbook Architecture](https://docs.atlasos.net/contributing/playbook/)
- [GitHub - Atlas-OS/Atlas](https://github.com/Atlas-OS/Atlas)
- [AME Wizard - Ameliorated.io](https://ameliorated.io/)
- [TrustedUninstaller CLI (MIT)](https://github.com/Ameliorated-LLC/trusted-uninstaller-cli)
- [GitHub - builtbybel/privatezilla](https://github.com/builtbybel/privatezilla)
- [NVCleanstall - TechPowerUp](https://www.techpowerup.com/nvcleanstall/)
- [Aptabase GitHub - Open Source Privacy-First Analytics](https://github.com/aptabase/aptabase)
- [Aptabase.com](https://aptabase.com/)
- [PostHog vs Plausible in-depth tool comparison](https://posthog.com/blog/posthog-vs-plausible)
- [Sentry for WPF Documentation](https://docs.sentry.io/platforms/dotnet/guides/wpf/)
- [Sentry .NET SDK 4.0 improvements for .NET 8](https://blog.sentry.io/sentry-dotnet-sdk-4-for-dotnet-8/)
- [Sentry Self-Hosted Docs](https://develop.sentry.dev/self-hosted/)
- [NSIS vs WIX vs Other Installation Packages Guide 2026](https://copyprogramming.com/howto/nsis-vs-wix-vs-anyother-installation-package)
- [WiX Toolset vs Inno Setup - Advanced Installer](https://www.advancedinstaller.com/versus/wix-toolset/wix-toolset-vs-inno-setup-packaging-tool.html)
- [Is Inno Setup Good? The Definitive 2026 Developer Review](https://blog.thefix.it.com/is-inno-setup-good-the-definitive-2026-developer-review/)
- [What is MSIX? - Microsoft Learn](https://learn.microsoft.com/en-us/windows/msix/overview)
- [Modern App Packaging: Why MSIX Is Replacing MSI - Redmondmag](https://redmondmag.com/articles/2025/09/23/modern-app-packaging.aspx)
- [GitHub - MScholtes/PS2EXE](https://github.com/MScholtes/PS2EXE)
- [Microsoft GitHub Actions for Desktop Apps](https://github.com/microsoft/github-actions-for-desktop-apps)
- [GitHub - nixxquality/GitHubUpdate C# update checker](https://github.com/nixxquality/GitHubUpdate)
- [Win32_ProcessStartTrace class - Microsoft Learn](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/krnlprov/win32-processstarttrace)
- [Writing a simple process monitor with WMI and C# - DEV Community](https://dev.to/nicoriff/writing-a-simple-process-monitor-with-wmi-and-c-end)
- [Process Lasso vs Game Boosters 2026 - GeekMatrex](https://geekmatrex.com/process-lasso-vs-game-boosters-guide-2026/)
- [Conventional Commits Specification](https://www.conventionalcommits.org/en/v1.0.0/)
- [semantic-release](https://semantic-release.gitbook.io/)
- [release-please automation - Elixir School](https://elixirschool.com/blog/managing-releases-with-release-please)
- [CycloneDX BOM Standard](https://cyclonedx.org/)
- [SPDX](https://spdx.dev/)
- [SOURCE_DATE_EPOCH specification](https://reproducible-builds.org/docs/source-date-epoch/)
- [About commit signature verification - GitHub Docs](https://docs.github.com/en/authentication/managing-commit-signature-verification/about-commit-signature-verification)
- [Contributor Covenant v2.1](https://www.contributor-covenant.org/version/2/1/code_of_conduct/)
- [Contributor Covenant - Main Site](https://www.contributor-covenant.org/)
- [Open Source Licenses Guide 2026 - DEV Community](https://dev.to/juanisidoro/open-source-licenses-which-one-should-you-pick-mit-gpl-apache-agpl-and-more-2026-guide-p90)
- [Hone.gg - PC Optimizer](https://hone.gg/)
- [ProSettings.net - Pro Player Settings](https://prosettings.net/)
- [GitHub - Lindeneg/csgo-pro-settings](https://github.com/Lindeneg/csgo-pro-settings)
- [CapFrameX Benchmark Tool](https://www.capframex.com/)
- [PresentMon & Frame Time Analysis - BoringBoredom](https://github.com/BoringBoredom/Frame-Time-Analysis)
- [NVIDIA FrameView 1.7 Update - Tom's Hardware](https://www.tomshardware.com/pc-components/gpus/nvidia-updates-frameview-performance-measurement-tool-version-1-7-promises-accurate-results-even-at-800-fps)
- [BattlEye Anti-Cheat](https://www.battleye.com/)
- [Easy Anti-Cheat](https://www.easy.ac/)
- [Top Anti-Cheat Software 2026 - sync.top](https://sync.top/blog/top-anti-cheat-software)
- [Docusaurus Documentation](https://docusaurus.io/docs)
- [MkDocs vs Docusaurus for technical documentation - Damavis](https://blog.damavis.com/en/mkdocs-vs-docusaurus-for-technical-documentation/)
- [GitBook vs Docusaurus vs MkDocs - Unmarkdown](https://unmarkdown.com/blog/gitbook-vs-docusaurus-vs-mkdocs)
- [Avalonia UI vs MAUI](https://avaloniaui.net/maui-compare)
- [.NET Cross-Platform Showdown: MAUI vs Uno vs Avalonia - DEV Community](https://dev.to/biozal/the-net-cross-platform-showdown-maui-vs-uno-vs-avalonia-and-why-avalonia-won-ian)
- [NV Access - NVDA Screen Reader](https://www.nvaccess.org/)
- [dotnet - WPF Accessibility Improvements docs](https://github.com/microsoft/dotnet/blob/main/Documentation/compatibility/wpf-accessibility-improvements.MD)
- [Crowdin vs Weblate - Localizely](https://localizely.com/compare/weblate-vs-crowdin/)
- [Weblate - Open Source Translation](https://openalternative.co/weblate)
- [Import-LocalizedData - PowerShell Microsoft Learn](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/import-localizeddata?view=powershell-7.5)
- [GitHub Sponsors @ChrisTitusTech](https://github.com/sponsors/ChrisTitusTech)
- [10 Best GitHub Sponsors Alternatives 2026 - donatr.ee](https://donatr.ee/blog/github-sponsors-alternatives/)
- [Awesome OSS Funding](https://github.com/sustainers/awesome-oss-funding)
- [How to Launch on Product Hunt in 2026 - Smol Launch](https://smollaunch.com/guides/launching-on-product-hunt)
- [Reddit Gaming Community Marketing 2026 - Space Node](https://space-node.net/blog/reddit-growing-gaming-server-community-2026)
- [MSR Model-specific register - Wikipedia](https://en.wikipedia.org/wiki/Model-specific_register)
- [Deltas diffed - Hydraulic](https://hydraulic.dev/blog/20-deltas-diffed.html)
- [IPFS Documentation](https://ipfs.tech/)
