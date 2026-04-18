# Windows 11 Optimization Research — Servicios, Registry, UWP, Telemetría

> Investigación exhaustiva para app de optimización Windows 11. Fuentes cruzadas: Raphire/Win11Debloat (2026.04.05), ChrisTitusTech/winutil, Atlas-OS/Atlas, meetrevision/playbook (ReviOS), ntdevlabs/tiny11builder, builtbybel/privatezilla, hellzerg/optimizer, O&O ShutUp10++, Disassembler0/Win10-Initial-Setup-Script, WindowsSpyBlocker, Microsoft Learn, elevenforum, The Register, gHacks, Microsoft-Q&A, CIS Benchmarks.

## Convención de riesgos usada en este documento

| Tag | Significado |
|---|---|
| SAFE | Sin efectos secundarios documentados en el uso típico |
| SAFE-IF | Seguro solo si **no** usas la feature indicada |
| CAUTION | Puede romper funcionalidad no obvia; probar |
| UNSAFE | No tocar salvo casos muy específicos; lista al final |

Todo el markdown usa rutas **reales** extraídas de `.reg` files y PowerShell scripts de los repos mencionados.

---

## 1. Servicios de Windows 11 — tabla de deshabilitación

### 1.1 Servicios de telemetría / diagnóstico (SAFE disable)

| Internal | Display Name | Qué hace | Recomendado | Riesgo | Rompe si deshabilitas |
|---|---|---|---|---|---|
| `DiagTrack` | Connected User Experiences and Telemetry | Recolecta y envía telemetría/diagnóstico a MS. ReviOS + Win11Debloat + todos lo deshabilitan | **Disabled** | SAFE | Nada para el usuario; Feedback Hub deja de funcionar |
| `dmwappushservice` | WAP Push Message Routing Svc | Routing de mensajes push device mgmt (MDM) | **Disabled** | SAFE-IF no usas Intune/MDM | Enrollment MDM |
| `diagnosticshub.standardcollector.service` | Diagnostics Hub Standard Collector | Recolecta traces para Visual Studio diagnostics | **Manual** | SAFE | VS perf profiler |
| `DPS` | Diagnostic Policy Service | Detección de problemas, solución automática | **Manual** | SAFE | Troubleshooters integrados |
| `WdiServiceHost` / `WdiSystemHost` | Diagnostic Service/System Host | Usados por DPS, hosts para diagnóstico | **Manual** | SAFE | Troubleshooters |
| `WerSvc` | Windows Error Reporting | Envía crash dumps a MS | **Disabled** | SAFE | Nada visible; devs pierden Watson reports |
| `PcaSvc` | Program Compatibility Assistant | Detecta apps con problemas de compat | **Disabled** | SAFE | Prompt auto de compat; juegos legacy aún funcionan |
| `wisvc` | Windows Insider Service | Inscripción en Insider Program | **Disabled** | SAFE-IF no eres Insider | Canal Insider |

**Comando PS (bulk):**
```powershell
$svc = @('DiagTrack','dmwappushservice','diagnosticshub.standardcollector.service','DPS','WdiServiceHost','WdiSystemHost','WerSvc','PcaSvc','wisvc')
$svc | % { Stop-Service $_ -Force -EA SilentlyContinue; Set-Service $_ -StartupType Disabled -EA SilentlyContinue }
```

### 1.2 Servicios de performance / cache (SAFE-IF SSD)

| Internal | Display | Función | Recomendado | Notas |
|---|---|---|---|---|
| `SysMain` | SysMain (ex-Superfetch) | Precarga apps usadas frecuentemente a RAM | **Disabled en SSD**, Auto en HDD | Libera RAM + I/O. XDA 2025 confirma smoother en SSDs |
| `WSearch` | Windows Search | Indexa archivos para búsquedas rápidas | **Manual** o Disabled si usas Everything | Search box menú Start sigue funcionando (resultados más lentos) |
| `SSDPSRV` | SSDP Discovery | Descubre UPnP (impresoras red, media) | **Manual** | SAFE si LAN sin UPnP |
| `upnphost` | UPnP Device Host | Host de UPnP | **Manual** | SAFE |
| `tcpipreg` | TCP/IP Registry Compat | Legacy TCP/IP registry | **Disabled** | ReviOS lo deshabilita; no rompe IPv4/v6 moderno |
| `NetBT` | NetBIOS over TCP/IP | Legacy resolución NetBIOS | **Disabled** | SAFE-IF no SMB1/nombres NetBIOS; ReviOS lo deshabilita |

### 1.3 Servicios dinosáuricos (obsoletos)

| Internal | Display | Recomendado |
|---|---|---|
| `Fax` | Fax | **Disabled** — nadie usa fax |
| `WbioSrvc` | Windows Biometric Service | **Disabled** si no usas Windows Hello biométrico |
| `RemoteRegistry` | Remote Registry | **Disabled** — superficie de ataque remota |
| `RetailDemo` | Retail Demo Service | **Disabled** — demo mode para tiendas |
| `MapsBroker` | Downloaded Maps Manager | **Disabled** si no usas Maps offline |
| `WMPNetworkSvc` | WMP Network Sharing | **Disabled** — WMP deprecated |
| `lfsvc` | Geolocation Service | **Disabled** si no necesitas location |
| `CscService` | Offline Files | **Disabled** salvo empresas con shares offline |
| `TrkWks` | Distributed Link Tracking | **Disabled** |
| `stisvc` | Windows Image Acquisition (WIA) | **Disabled** si no escaneas |
| `AJRouter` | AllJoyn Router Service | **Disabled** (IoT obsoleto) |
| `SNMPTRAP` | SNMP Trap | **Disabled** |

### 1.4 Xbox / Gaming services (SAFE-IF no usas Xbox)

| Internal | Display | Impacto si disable |
|---|---|---|
| `XblAuthManager` | Xbox Live Auth Manager | Login Xbox deja de funcionar |
| `XblGameSave` | Xbox Live Game Save | Cloud saves Xbox/Game Pass rotas |
| `XboxGipSvc` | Xbox Accessory Management | Controladores Xbox wireless (disable solo si uses Steam Input) |
| `XboxNetApiSvc` | Xbox Live Networking | Xbox Live features rotas |

**IMPORTANTE gaming:** Steam/Epic/GOG **no se afectan**. Solo Xbox app, Game Pass, MS Store games pierden funcionalidad. CIS Level 1 recomienda `XblGameSave` Disabled.

```powershell
# Disable Xbox suite (asumiendo no se usa Xbox/Game Pass)
'XblAuthManager','XblGameSave','XboxGipSvc','XboxNetApiSvc' | % {
  Stop-Service $_ -Force -EA SilentlyContinue; Set-Service $_ -StartupType Disabled -EA SilentlyContinue
}
```

### 1.5 Windows Update services (CAUTION)

| Internal | Display | Recomendado |
|---|---|---|
| `wuauserv` | Windows Update | **Manual** — dejar para que manual update funcione |
| `UsoSvc` | Update Orchestrator | **Manual** — si Disabled, WU UI se cae |
| `WaaSMedicSvc` | WaaS Medic | **Disabled vía registry** (está protegido por TrustedInstaller). Ver 1.7 |
| `DoSvc` | Delivery Optimization | **Manual** — el P2P share puede desactivarse en registry (ver 2.2) |
| `BITS` | Background Intelligent Transfer | **Manual** — necesario para WU y Store |

**No** pongas `wuauserv` en Disabled permanente: rompe Store, Defender signatures, WU; usa pausas vía registry (sección 2.7).

### 1.6 Print Spooler (SEGURIDAD — PrintNightmare)

`Spooler` (Print Spooler). **CVE-2021-34527 PrintNightmare** sigue siendo vector relevante. Si **no imprimes**:
```powershell
Stop-Service -Name Spooler -Force
Set-Service -Name Spooler -StartupType Disabled
```
Si imprimes, aplica hardening en vez de disable:
```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers\PointAndPrint]
"NoWarningNoElevationOnInstall"=dword:00000000
"UpdatePromptSettings"=dword:00000000
"RestrictDriverInstallationToAdministrators"=dword:00000001
```

### 1.7 Servicios protegidos por TrustedInstaller

`WaaSMedicSvc`, `DoSvc` en algunos builds, `gpsvc`, `DiagTrack` (en 24H2+) requieren modificar `Start` en registry:
```reg
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WaaSMedicSvc]
"Start"=dword:00000004   ; 2=Auto, 3=Manual, 4=Disabled
```
Recomendado: lanzar `reg.exe` desde un proceso `NT AUTHORITY\SYSTEM` (PsExec `-s` o Scheduled Task como SYSTEM).

### 1.8 Lo que NO tocar nunca (servicios core)

| Internal | Por qué |
|---|---|
| `bthserv` | Bluetooth entero |
| `CDPSvc` | Night Light depende |
| `cbdhsvc_*` | Clipboard, screenshots |
| `WpnService` | Notifications, Action Center |
| `TokenBroker` | Settings auth |
| `Wcmsvc` | Network settings UI |
| `EventLog`, `RpcSs`, `DcomLaunch`, `PlugPlay`, `nsi`, `Power` | Kernel-level — romperse garantizado |

---

## 2. Registry tweaks — por categoría

### 2.1 Privacy & Telemetría (extraído de `Win11Debloat/Regfiles/Disable_Telemetry.reg`)

```reg
Windows Registry Editor Version 5.00

; Advertising ID
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo]
"Enabled"=dword:00000000

; Tailored experiences
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy]
"TailoredExperiencesWithDiagnosticDataEnabled"=dword:00000000

; Online Speech Recognition
[HKEY_CURRENT_USER\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy]
"HasAccepted"=dword:00000000

; Ink/Typing
[HKEY_CURRENT_USER\Software\Microsoft\Input\TIPC]
"Enabled"=dword:00000000
[HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization]
"RestrictImplicitInkCollection"=dword:00000001
"RestrictImplicitTextCollection"=dword:00000001
[HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization\TrainedDataStore]
"HarvestContacts"=dword:00000000
[HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings]
"AcceptedPrivacyPolicy"=dword:00000000

; Telemetry level (0 = Security only, requires Enterprise; Pro respeta 1)
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection]
"AllowTelemetry"=dword:00000000
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection]
"AllowTelemetry"=dword:00000000
"MaxTelemetryAllowed"=dword:00000001
"DoNotShowFeedbackNotifications"=dword:00000001

; Activity History / Timeline
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System]
"PublishUserActivities"=dword:00000000
"UploadUserActivities"=dword:00000000
"EnableActivityFeed"=dword:00000000

; App launch tracking
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"Start_TrackProgs"=dword:00000000

; Feedback freq
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules]
"NumberOfSIUFInPeriod"=dword:00000000

; Location services (global policy)
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors]
"DisableLocation"=dword:00000001

; Find My Device
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\FindMyDevice]
"AllowFindMyDevice"=dword:00000000

; Clipboard history sync cloud
[HKEY_CURRENT_USER\Software\Microsoft\Clipboard]
"EnableClipboardHistory"=dword:00000000
"CloudClipboardAutomaticUpload"=dword:00000000

; Content Delivery Manager (todos los sugerencias/ads)
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager]
"ContentDeliveryAllowed"=dword:00000000
"OemPreInstalledAppsEnabled"=dword:00000000
"PreInstalledAppsEnabled"=dword:00000000
"PreInstalledAppsEverEnabled"=dword:00000000
"SilentInstalledAppsEnabled"=dword:00000000
"SubscribedContent-338387Enabled"=dword:00000000
"SubscribedContent-338388Enabled"=dword:00000000
"SubscribedContent-338389Enabled"=dword:00000000
"SubscribedContent-338393Enabled"=dword:00000000
"SubscribedContent-353694Enabled"=dword:00000000
"SubscribedContent-353696Enabled"=dword:00000000
"SystemPaneSuggestionsEnabled"=dword:00000000

; Edge telemetry
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge]
"PersonalizationReportingEnabled"=dword:00000000
"DiagnosticData"=dword:00000000
"MetricsReportingEnabled"=dword:00000000
"AutofillAddressEnabled"=dword:00000000

; Cortana / Bing in search
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search]
"AllowCortana"=dword:00000000
"CortanaConsent"=dword:00000000
"DisableWebSearch"=dword:00000001
"ConnectedSearchUseWeb"=dword:00000000
[HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer]
"DisableSearchBoxSuggestions"=dword:00000001
```

### 2.2 Windows Update & Delivery

```reg
; No reboot while logged in
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU]
"NoAutoRebootWithLoggedOnUsers"=dword:00000001
"AUOptions"=dword:00000002       ; 2=notify download, 3=auto download notify install

; Disable "get latest updates ASAP"
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings]
"IsContinuousInnovationOptedIn"=dword:00000000

; Delivery Optimization (no P2P seeding)
[HKEY_USERS\S-1-5-20\Software\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Settings]
"DownloadMode"=dword:00000000
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization]
"DODownloadMode"=dword:00000000

; Driver updates OFF via WU
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate]
"ExcludeWUDriversInQualityUpdate"=dword:00000001
```

### 2.3 UX — File Explorer / Taskbar Windows 11 specific

```reg
; Extensiones de archivos visibles
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"HideFileExt"=dword:00000000
"Hidden"=dword:00000001
"ShowSuperHidden"=dword:00000000   ; OS files still hidden by default
"LaunchTo"=dword:00000001          ; 1=This PC, 2=Quick access
"LastActiveClick"=dword:00000001   ; cycle ventanas al click taskbar
"TaskbarMn"=dword:00000000         ; hide Chat/Teams icon
"TaskbarDa"=dword:00000000         ; hide Widgets
"ShowCopilotButton"=dword:00000000 ; hide Copilot taskbar btn
"ShowTaskViewButton"=dword:00000000
"SearchboxTaskbarMode"=dword:00000000   ; 0=hide, 1=icon, 2=icon+label, 3=box
"TaskbarAl"=dword:00000000         ; 0=left align, 1=center (Win11 default)

; Enable "End Task" in right-click taskbar
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings]
"TaskbarEndTask"=dword:00000001

; Hide Gallery from Explorer sidebar Win11 23H2+
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}]
; delete the key to hide gallery

; Dark mode
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize]
"AppsUseLightTheme"=dword:00000000
"SystemUsesLightTheme"=dword:00000000

; Disable transparency
"EnableTransparency"=dword:00000000

; UserPreferencesMask - disable animations
[HKEY_CURRENT_USER\Control Panel\Desktop]
"UserPreferencesMask"=hex:90,12,07,80,10,00,00,00
"MenuShowDelay"="0"             ; faster submenu
"HungAppTimeout"="2000"         ; kill stalled apps faster
"WaitToKillAppTimeout"="2000"
"AutoEndTasks"="1"
"LowLevelHooksTimeout"="1000"
```

### 2.4 Classic context menu restore (Windows 11)

```reg
Windows Registry Editor Version 5.00
[HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32]
@=""
```
O PowerShell:
```powershell
reg.exe add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
Stop-Process -Name explorer -Force
```

### 2.5 Performance — kernel/scheduler tweaks

```reg
; Win32PrioritySeparation - foreground boost
; 0x26 (decimal 38) = Short quantum, Variable, 3x foreground (DEFAULT Win desktop)
; 0x28 = Short, Fixed, No boost (equal timeslice - evita microstutters)
; 0x29 = Short, Fixed, Medium boost
; 0x2A = Short, Fixed, High boost (Atlas OS value)
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl]
"Win32PrioritySeparation"=dword:00000026

; Service shutdown timing
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control]
"WaitToKillServiceTimeout"="2000"

; Memory management (máquinas 16GB+)
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management]
"LargeSystemCache"=dword:00000000       ; 0 for desktop/gaming, 1 for server
"DisablePagingExecutive"=dword:00000001 ; keep kernel in RAM
"ClearPageFileAtShutdown"=dword:00000000 ; 1 = security overkill, slow shutdown

; Disable Memory Compression (solo sistemas 16GB+)
; Set-MMAgent -MemoryCompression $false   ; via powershell

; Fast startup OFF (previene bugs dual-boot y NTFS mount)
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power]
"HiberbootEnabled"=dword:00000000

; NVMe / disk
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem]
"NtfsDisableLastAccessUpdate"=dword:80000001   ; User managed, disabled
"NtfsDisable8dot3NameCreation"=dword:00000001  ; disable legacy 8.3
```

### 2.6 Gaming — GPU + Game Mode

```reg
; Game DVR off (nada de MS grabando background)
[HKEY_CURRENT_USER\System\GameConfigStore]
"GameDVR_Enabled"=dword:00000000
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR]
"AppCaptureEnabled"=dword:00000000
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR]
"AllowGameDVR"=dword:00000000

; Game Bar / FSE
[HKEY_CURRENT_USER\System\GameConfigStore]
"GameDVR_FSEBehaviorMode"=dword:00000002    ; 2=force disable FSO

; Hardware Accelerated GPU Scheduling (HAGS)
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers]
"HwSchMode"=dword:00000002    ; 1=off, 2=on (enable for DX12/VRR games)

; Games task scheduler category prio (from HKLM\...\Tasks\Games)
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games]
"Affinity"=dword:00000000
"Background Only"="False"
"Clock Rate"=dword:00002710
"GPU Priority"=dword:00000008
"Priority"=dword:00000006
"Scheduling Category"="High"
"SFIO Priority"="High"

; MMCSS SystemResponsiveness (multimedia vs otros)
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile]
"SystemResponsiveness"=dword:00000000   ; 0 = 100% to multimedia, 20 = default
"NetworkThrottlingIndex"=dword:ffffffff ; disable network throttling during multimedia
```

### 2.7 Networking — latencia / throughput

```reg
; Disable Nagle (TcpAckFrequency=1 + TcpNoDelay=1) — por-adaptador
; Ubicación: HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{adapter-GUID}
; DWORD TcpAckFrequency = 1
; DWORD TCPNoDelay = 1
; DWORD TcpDelAckTicks = 0

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSMQ\Parameters]
"TCPNoDelay"=dword:00000001

; Global network throttle (arriba en 2.6 también)
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile]
"NetworkThrottlingIndex"=dword:ffffffff

; Disable ICMP redirects (seguridad + latencia)
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters]
"EnableICMPRedirect"=dword:00000000
"DisableIPSourceRouting"=dword:00000002

; QoS Reserved Bandwidth (no reserva 20% para QoS)
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Psched]
"NonBestEffortLimit"=dword:00000000
```
**CLI complementario:**
```powershell
netsh int tcp set global autotuninglevel=normal
netsh int tcp set global rss=enabled
netsh int tcp set global chimney=disabled
netsh int tcp set global ecncapability=enabled
netsh int tcp set global timestamps=disabled
powercfg /h off           ; disable hibernation (libera ~RAM GB en disk)
```

### 2.8 Defender (¡cuidado!)

Aunque ReviOS lo desinstala, esto es **NO** recomendable por default. Si el usuario lo quiere:

**Exclusiones** — se ubican en `HKLM\SOFTWARE\Microsoft\Windows Defender\Exclusions\Paths` (bloqueado por Tamper Protection + TrustedInstaller ACL; escritura solo desde SYSTEM). Vía PowerShell elevada:
```powershell
Add-MpPreference -ExclusionPath 'C:\MyDevFolder'
Add-MpPreference -ExclusionProcess 'devenv.exe'
Add-MpPreference -ExclusionExtension '.vhdx'
```

**Cloud protection disable** (CAUTION — rompe MAPS):
```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet]
"SpynetReporting"=dword:00000000
"SubmitSamplesConsent"=dword:00000002
```

**Tamper Protection debe estar OFF** antes de cualquier tweak de Defender — solo vía Security Center GUI (no hay registry público).

### 2.9 Explorer — disable thumbnail/icon cache (low-end)

```reg
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"IconsOnly"=dword:00000001            ; no thumbnails (just icon)
"ListviewAlphaSelect"=dword:00000000
"ListviewShadow"=dword:00000000

[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer]
"NoThumbnailCache"=dword:00000001
"DisableThumbnails"=dword:00000001    ; only if really low on I/O
```

### 2.10 Edge AI features (Copilot sidebar)

Extraído de `Win11Debloat/Regfiles/Disable_Edge_AI_Features.reg`:
```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge]
"CopilotCDPPageContext"=dword:00000000
"CopilotPageContext"=dword:00000000
"HubsSidebarEnabled"=dword:00000000
"EdgeEntraCopilotPageContext"=dword:00000000
"EdgeHistoryAISearchEnabled"=dword:00000000
"ComposeInlineEnabled"=dword:00000000
"GenAILocalFoundationalModelSettings"=dword:00000001
"NewTabPageBingChatEnabled"=dword:00000000
```

### 2.11 Notepad / Paint AI

```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\WindowsNotepad]
"DisableAIFeatures"=dword:00000001

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint]
"DisableCocreator"=dword:00000001
"DisableGenerativeFill"=dword:00000001
"DisableImageCreator"=dword:00000001
"DisableGenerativeErase"=dword:00000001
"DisableRemoveBackground"=dword:00000001
```

---

## 3. UWP apps — tabla remove/keep

Basado en `Win11Debloat/Config/Apps.json` (145 apps, April 2026). Categorías: **safe** (93 SI remover), **optional** (46 depende), **unsafe** (6 NO remover).

### 3.1 SAFE a remover (bloatware obvio) — selección top

**Bing suite (todos deprecated excepto BingWeather):**
`Microsoft.BingNews`, `Microsoft.BingFinance`, `Microsoft.BingFoodAndDrink`, `Microsoft.BingHealthAndFitness`, `Microsoft.BingSports`, `Microsoft.BingTravel`, `Microsoft.BingTranslator`, `Microsoft.BingWeather`.

**Office/Microsoft suite UWP:**
`Microsoft.MicrosoftOfficeHub`, `Microsoft.Office.OneNote` (UWP — NO la desktop .exe), `Microsoft.Office.Sway`, `Microsoft.MicrosoftPowerBIForWindows`, `Microsoft.PowerAutomateDesktop` (RPA), `Microsoft.Todos`, `Microsoft.MicrosoftStickyNotes`.

**Cortana/AI:**
- `Microsoft.549981C3F5F10` — Cortana legacy (discontinued)
- `Microsoft.Copilot` — AI asistente
- `Microsoft.Windows.AIHub` — Copilot+ AI Hub (Win11 24H2+)
- `Microsoft.WindowsFeedbackHub`

**3D legacy:**
`Microsoft.3DBuilder`, `Microsoft.Microsoft3DViewer`, `Microsoft.Print3D`, `Microsoft.MixedReality.Portal`.

**Zune legacy / Media:**
`Microsoft.ZuneVideo` (Movies & TV).

**Teams/Comunicación:**
`Microsoft.SkypeApp` (UWP — discontinued), `MicrosoftTeams` (old), `MSTeams` (new consumer), `Microsoft.OneConnect`, `Microsoft.Messaging`, `Microsoft.People` y `Microsoft.windowscommunicationsapps` (Mail & Calendar — discontinued abril 2026, reemplazado por Outlook).

**Xbox (todo si no usas):**
`Microsoft.XboxApp` (old Console Companion).

**Maps/Misc:**
`Microsoft.WindowsMaps`, `Microsoft.NetworkSpeedTest`, `Microsoft.WindowsSoundRecorder`, `Microsoft.WindowsAlarms`, `Microsoft.Getstarted` (protected but removable via `Get-AppxPackage -AllUsers | Where Name -like '*Getstarted*' | Remove-AppxPackage -AllUsers`).

**Gaming bloat (games específicos OEM):**
`Microsoft.MicrosoftSolitaireCollection`, `Microsoft.MicrosoftJournal`.

**OEM bloat (HP/Dell/Lenovo):**
```
AD2F1837.HPAIExperienceCenter, AD2F1837.HPConnectedMusic, AD2F1837.HPEasyClean,
AD2F1837.HPJumpStarts, AD2F1837.HPPrivacySettings, AD2F1837.HPSupportAssistant,
AD2F1837.HPSureShieldAI, AD2F1837.HPWelcome, AD2F1837.myHP,
E046963F.LenovoCompanion, LenovoCompanyLimited.LenovoVantageService,
DellInc.DellSupportAssistforPCs, DellInc.DellDigitalDelivery, DellInc.DellMobileConnect
```

**Third-party bloat (OEM preinstalled):**
`Clipchamp.Clipchamp`, `Disney`, `DisneyMagicKingdoms`, `Facebook`, `Instagram`, `LinkedInforWindows`, `Netflix`, `Spotify`, `TikTok`, `Twitter`, `AmazonVideo.PrimeVideo`, `Amazon.com.Amazon`, `king.com.CandyCrushSaga`, `king.com.CandyCrushSodaSaga`, `king.com.BubbleWitch3Saga`, `AdobeSystemsIncorporated.AdobePhotoshopExpress`, `CyberLinkMediaSuiteEssentials`, `Duolingo-LearnLanguagesforFree`, `Flipboard`, `fitbit`, `Hulu`, `iHeartRadio`, `Pandora`, `Plex`, `SlingTV`, `WinZipUniversal`, `XING`, `Shazam`, `ActiproSoftwareLLC`, `DrawboardPDF`, `Wunderlist`, `PandoraMediaInc`.

### 3.2 OPTIONAL (depende del uso)

| AppId | Consideración |
|---|---|
| `Microsoft.BingSearch` | Rompe búsqueda web en Start menu (eso puede ser lo que quieres) |
| `Microsoft.MSPaint` | Paint 3D (viejo) |
| `Microsoft.Paint` | Paint clásico (NUEVA versión 2026); la mayoría lo quiere |
| `Microsoft.WindowsNotepad` | Notepad moderno; si removes, regresas a legacy |
| `Microsoft.ScreenSketch` | Snipping Tool. **Usualmente se conserva** |
| `Microsoft.WindowsCalculator` | Calculadora |
| `Microsoft.WindowsCamera` | Cámara |
| `Microsoft.WindowsTerminal` | Terminal moderna (Win11 default). **Conservar** si usas PS |
| `Microsoft.Windows.Photos` | Visor fotos. Sin él abres con Paint |
| `Microsoft.YourPhone` | Phone Link |
| `MicrosoftWindows.CrossDevice` | Phone Link companion |
| `Microsoft.XboxGameOverlay` / `Microsoft.XboxGamingOverlay` | Game Bar — algunos juegos lo requieren |
| `Microsoft.GamingApp` | Xbox App (Game Pass installer) |
| `Microsoft.StartExperiencesApp` | Widgets engine |
| `Microsoft.RemoteDesktop` | RDP client nuevo |
| `Microsoft.OneDrive` | Cloud — remueve solo si no usas |
| `Microsoft.OutlookForWindows` | New Outlook (obliga remover si quieres classic) |
| `Microsoft.Whiteboard` | Whiteboard collab |
| `Microsoft.ZuneMusic` | Media Player (NUEVO, reemplazó Groove) |
| `Microsoft.Windows.DevHome` | Dev Home dashboard (discontinued) |

### 3.3 UNSAFE — NO remover

| AppId | Por qué NO |
|---|---|
| `Microsoft.Edge` / `XPFFTQ037JWMHS` | Único browser disponible por default. Removerlo deja PC sin navegador hasta que instales otro via CLI |
| `Microsoft.GetHelp` | Requerido por Troubleshooters Win11 |
| `Microsoft.WindowsStore` | Store — casi imposible reinstalar; varios apps dependen |
| `Microsoft.Xbox.TCUI` | Framework UI requerido por Store, Photos, algunos juegos |
| `Microsoft.XboxIdentityProvider` | Xbox sign-in framework — juegos y servicios dependen |
| `Microsoft.XboxSpeechToTextOverlay` | Accesibilidad; imposible reinstalar fácil |

### 3.4 Comandos PowerShell

```powershell
# Remove single (current user)
Get-AppxPackage -Name 'Microsoft.BingNews' | Remove-AppxPackage

# Remove all-users (requires admin)
Get-AppxPackage -AllUsers -Name 'Microsoft.BingNews' | Remove-AppxPackage -AllUsers

# Prevent provisioning for NEW users
Get-AppxProvisionedPackage -Online | Where DisplayName -EQ 'Microsoft.BingNews' |
    Remove-AppxProvisionedPackage -Online -AllUsers

# Bulk remove by pattern
$pkgs = @('*BingNews*','*BingWeather*','*BingFinance*','*BingSports*','*BingTravel*',
          '*OfficeHub*','*SkypeApp*','*ZuneVideo*','*3DBuilder*','*Microsoft3DViewer*',
          '*MixedReality*','*FeedbackHub*','*YourPhone*','*Teams*','*Getstarted*',
          '*GetHelp*','*Messaging*','*OneConnect*','*People*','*windowscommunicationsapps*',
          '*Solitaire*','*StickyNotes*','*Todos*','*Maps*','*SoundRecorder*','*Alarms*',
          '*WindowsFeedbackHub*','*549981C*','*Copilot*','*AIHub*','*Clipchamp*')
foreach ($p in $pkgs) {
    Get-AppxPackage -AllUsers $p | Remove-AppxPackage -AllUsers -EA SilentlyContinue
    Get-AppxProvisionedPackage -Online | Where DisplayName -like $p |
        Remove-AppxProvisionedPackage -Online -AllUsers -EA SilentlyContinue
}
```

### 3.5 Teams consumer — remove definitivo

```powershell
# 1. App package
Get-AppxPackage -AllUsers MicrosoftTeams | Remove-AppxPackage -AllUsers
Get-AppxPackage -AllUsers MSTeams | Remove-AppxPackage -AllUsers
Get-AppxProvisionedPackage -Online | Where DisplayName -like '*Teams*' |
    Remove-AppxProvisionedPackage -Online -AllUsers

# 2. Hide Chat icon + block auto-install
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Chat" /v ChatIcon /t REG_DWORD /d 3 /f
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Communications" /v ConfigureChatAutoInstall /t REG_DWORD /d 0 /f
```

### 3.6 OneDrive — uninstall completo

```powershell
# 1. Kill processes
taskkill /f /im OneDrive.exe 2>$null
taskkill /f /im explorer.exe

# 2. Uninstall 64/32 bit
if (Test-Path "$env:systemroot\System32\OneDriveSetup.exe") {
    Start-Process "$env:systemroot\System32\OneDriveSetup.exe" -ArgumentList '/uninstall' -Wait
}
if (Test-Path "$env:systemroot\SysWOW64\OneDriveSetup.exe") {
    Start-Process "$env:systemroot\SysWOW64\OneDriveSetup.exe" -ArgumentList '/uninstall' -Wait
}

# 3. Remove leftovers
Remove-Item -Recurse -Force "$env:LOCALAPPDATA\Microsoft\OneDrive" -EA SilentlyContinue
Remove-Item -Recurse -Force "$env:PROGRAMDATA\Microsoft OneDrive" -EA SilentlyContinue
Remove-Item -Recurse -Force "$env:SYSTEMDRIVE\OneDriveTemp" -EA SilentlyContinue

# 4. Registry — hide from Explorer + block reinstall
reg delete "HKEY_CLASSES_ROOT\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}" /f 2>$null
reg delete "HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}" /f 2>$null
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive" /v DisableFileSyncNGSC /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive" /v DisableFileSync /t REG_DWORD /d 1 /f

# 5. Restart Explorer
Start-Process explorer
```

---

## 4. Scheduled Tasks a deshabilitar

Extraído de tiny11builder, ReviOS, Disassembler0, Raphire — lista consolidada.

```powershell
$tasks = @(
  'Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser',
  'Microsoft\Windows\Application Experience\ProgramDataUpdater',
  'Microsoft\Windows\Application Experience\StartupAppTask',
  'Microsoft\Windows\Application Experience\PcaPatchDbTask',
  'Microsoft\Windows\Application Experience\MareBackup',

  'Microsoft\Windows\Autochk\Proxy',

  'Microsoft\Windows\Customer Experience Improvement Program\Consolidator',
  'Microsoft\Windows\Customer Experience Improvement Program\UsbCeip',
  'Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask',

  'Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector',
  'Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver',

  'Microsoft\Windows\Feedback\Siuf\DmClient',
  'Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload',

  'Microsoft\Windows\Maintenance\WinSAT',

  'Microsoft\Windows\Media Center\ActivateWindowsSearch',
  'Microsoft\Windows\Media Center\ConfigureInternetTimeService',
  'Microsoft\Windows\Media Center\DispatchRecoveryTasks',
  'Microsoft\Windows\Media Center\ehDRMInit',
  'Microsoft\Windows\Media Center\InstallPlayReady',
  'Microsoft\Windows\Media Center\mcupdate',
  'Microsoft\Windows\Media Center\MediaCenterRecoveryTask',
  'Microsoft\Windows\Media Center\ObjectStoreRecoveryTask',
  'Microsoft\Windows\Media Center\OCURActivate',
  'Microsoft\Windows\Media Center\OCURDiscovery',
  'Microsoft\Windows\Media Center\PBDADiscovery',
  'Microsoft\Windows\Media Center\PBDADiscoveryW1',
  'Microsoft\Windows\Media Center\PBDADiscoveryW2',
  'Microsoft\Windows\Media Center\PvrRecoveryTask',
  'Microsoft\Windows\Media Center\PvrScheduleTask',
  'Microsoft\Windows\Media Center\RegisterSearch',
  'Microsoft\Windows\Media Center\ReindexSearchRoot',
  'Microsoft\Windows\Media Center\SqlLiteRecoveryTask',
  'Microsoft\Windows\Media Center\UpdateRecordPath',

  'Microsoft\Windows\Windows Error Reporting\QueueReporting',

  'Microsoft\Office\Office ClickToRun Service Monitor',
  'Microsoft\Office\OfficeTelemetryAgentLogOn2016',
  'Microsoft\Office\OfficeTelemetryAgentFallBack2016',
  'Microsoft\Office\OfficeBackgroundTaskHandlerRegistration',
  'Microsoft\Office\OfficeBackgroundTaskHandlerLogon'
)
foreach ($t in $tasks) {
    Disable-ScheduledTask -TaskPath ('\' + ($t -replace '\\[^\\]+$','\')) `
                          -TaskName ($t -split '\\')[-1] -EA SilentlyContinue | Out-Null
}
```

---

## 5. Copilot + Recall — erradicación específica Win11

### 5.1 Copilot

```reg
Windows Registry Editor Version 5.00

; Hide taskbar button
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"ShowCopilotButton"=dword:00000000

; Policy disable (legacy deprecating, but still honored Pro/Ent pre-24H2)
[HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot]
"TurnOffWindowsCopilot"=dword:00000001
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot]
"TurnOffWindowsCopilot"=dword:00000001
```
```powershell
# Remove Copilot app
Get-AppxPackage -AllUsers *Copilot* | Remove-AppxPackage -AllUsers
Get-AppxProvisionedPackage -Online | Where DisplayName -like '*Copilot*' |
    Remove-AppxProvisionedPackage -Online -AllUsers
```

**Nota crítica (2026):** MS deprecó `TurnOffWindowsCopilot` en builds 25H2. Para versiones modernas hace falta **AppLocker** rule bloqueando `CopilotNativeClient.exe` + remoción AppxPackage + disable del servicio `WSAIFabricSvc`:
```reg
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSAIFabricSvc]
"Start"=dword:00000003    ; manual; 4=disabled
```

### 5.2 Recall (AI screen recording, Copilot+ PCs)

```reg
; User-level
[HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI]
"DisableAIDataAnalysis"=dword:00000001
"DisableClickToDo"=dword:00000001

; Machine-level — más efectivo
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI]
"DisableAIDataAnalysis"=dword:00000001
"AllowRecallEnablement"=dword:00000000
"TurnOffSavingSnapshots"=dword:00000001
"DisableClickToDo"=dword:00000001
```
```powershell
# Disable via DISM si el "optional feature" aparece
Disable-WindowsOptionalFeature -Online -FeatureName 'Recall' -NoRestart

# Windows Sandbox donde probar
dism /online /disable-feature /featurename:Recall /norestart
```

### 5.3 Click-to-Do (AI feature 24H2)

Ver `DisableClickToDo=1` arriba. Servicio asociado:
```reg
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSAIFabricSvc]
"Start"=dword:00000004
```

---

## 6. Context menu / Start menu / Explorer Win11 específico

### 6.1 Classic context menu (revertir al Win10)

Ver sección 2.4. Comando:
```powershell
reg.exe add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
Stop-Process -Name explorer -Force; Start-Process explorer
```

### 6.2 Start menu — remove Recommended section

```reg
; GPO equivalent (Win11 22H2+)
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer]
"HideRecommendedSection"=dword:00000001
"HideRecentJumplists"=dword:00000001

; Disable Bing/Store suggestions in Start
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search]
"BingSearchEnabled"=dword:00000000
"CortanaConsent"=dword:00000000

; Start All Apps — hide
[HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer]
"DisableStartMenuAllApps"=dword:00000001
```

### 6.3 Widgets — remove completo

```powershell
# Web Experience Pack
Get-AppxPackage -AllUsers *WebExperience* | Remove-AppxPackage -AllUsers
Get-AppxProvisionedPackage -Online | Where DisplayName -like '*WebExperience*' |
    Remove-AppxProvisionedPackage -Online -AllUsers
winget uninstall "9MSSGKG348SP"   # Windows Web Experience Pack
```
```reg
; Hide icon
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"TaskbarDa"=dword:00000000

; Policy
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh]
"AllowNewsAndInterests"=dword:00000000
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\default\NewsAndInterests\AllowNewsAndInterests]
"value"=dword:00000000
```

### 6.4 File Explorer — abrir en This PC (no Home)

```reg
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced]
"LaunchTo"=dword:00000001   ; 1=This PC, 2=Quick access (default), 3=Downloads

; Hide Home from sidebar (requires ACL modification)
; Hide Gallery from sidebar (requires delete CLSID key)
```

### 6.5 File Explorer tabs (Win11 22H2+)

No hay registry toggle oficial — están siempre activos en 22H2+. Si quieres comportamiento single-window, usa apps como ExplorerPatcher o StartAllBack.

---

## 7. Telemetría — enfoque completo (4 capas)

### 7.1 Capa 1: Registry (ver 2.1)

### 7.2 Capa 2: Services (ver 1.1)

### 7.3 Capa 3: Scheduled Tasks (ver sección 4)

### 7.4 Capa 4: Hosts file + Firewall

**Hosts file telemetry block** (WindowsSpyBlocker `spy.txt` — 353 entries):

Archivo ubicado en `C:\Windows\System32\drivers\etc\hosts`. Script de instalación:
```powershell
# Warning: Defender puede flaggear como HostsFileHijack; add exclusion first
Add-MpPreference -ExclusionPath 'C:\Windows\System32\drivers\etc\hosts'

$hosts = @'
# Microsoft Telemetry
0.0.0.0 alpha.telemetry.microsoft.com
0.0.0.0 v10.events.data.microsoft.com
0.0.0.0 v10c.events.data.microsoft.com
0.0.0.0 v10.vortex-win.data.microsoft.com
0.0.0.0 v20.events.data.microsoft.com
0.0.0.0 vortex.data.microsoft.com
0.0.0.0 vortex-win.data.microsoft.com
0.0.0.0 telecommand.telemetry.microsoft.com
0.0.0.0 telemetry.microsoft.com
0.0.0.0 watson.telemetry.microsoft.com
0.0.0.0 umwatson.events.data.microsoft.com
0.0.0.0 settings-win.data.microsoft.com
0.0.0.0 browser.events.data.msn.com
0.0.0.0 self.events.data.microsoft.com
0.0.0.0 oca.telemetry.microsoft.com
0.0.0.0 sqm.telemetry.microsoft.com
0.0.0.0 sqm.microsoft.com
# Ads / CDM
0.0.0.0 a.ads1.msn.com
0.0.0.0 a.ads2.msads.net
0.0.0.0 a.ads2.msn.com
0.0.0.0 ads.msn.com
0.0.0.0 ads1.msads.net
0.0.0.0 ads1.msn.com
0.0.0.0 bingads.microsoft.com
0.0.0.0 ac3.msn.com
0.0.0.0 c.msn.com
0.0.0.0 dub1.vortex.data.microsoft.com.akadns.net
0.0.0.0 flex.msn.com
# Cortana
0.0.0.0 api.cortana.ai
0.0.0.0 cortana.ai
0.0.0.0 pandoragatewayprod.azurewebsites.net
# Copilot
0.0.0.0 copilot.microsoft.com
0.0.0.0 substrate.office.com
# SmartScreen (security; cuidado disable)
# 0.0.0.0 urs.smartscreen.microsoft.com
'@

Add-Content -Path 'C:\Windows\System32\drivers\etc\hosts' -Value $hosts
```

Referencia completa (300+ endpoints): `https://github.com/crazy-max/WindowsSpyBlocker/blob/master/data/hosts/spy.txt`

**Firewall rules (IFEO approach):**
```powershell
# Block outbound of telemetry processes
$bins = @(
  "$env:windir\System32\CompatTelRunner.exe",
  "$env:windir\System32\DeviceCensus.exe",
  "$env:windir\System32\diagtrack.dll",
  "$env:windir\System32\MusNotification.exe",
  "$env:windir\System32\dmclient.exe"
)
foreach ($b in $bins) {
    New-NetFirewallRule -DisplayName "Block $([IO.Path]::GetFileName($b))" `
        -Direction Outbound -Program $b -Action Block -Profile Any -EA SilentlyContinue
}

# Block DiagTrack service
New-NetFirewallRule -DisplayName 'Block DiagTrack' -Direction Outbound `
    -Service DiagTrack -Action Block -EA SilentlyContinue
```

**WMI Autologger disable (ReviOS approach):**
```reg
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\AutoLogger-Diagtrack-Listener]
"Start"=dword:00000000
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\SQMLogger]
"Start"=dword:00000000
[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WMI\Autologger\SetupPlatformTel]
"Start"=dword:00000000
```

**IFEO kill telemetry binaries (agresivo, ReviOS):**
```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\CompatTelRunner.exe]
"Debugger"="%windir%\\System32\\systray.exe"

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DeviceCensus.exe]
"Debugger"="%windir%\\System32\\systray.exe"
```

### 7.5 Environment variables .NET / PS telemetry

```powershell
[Environment]::SetEnvironmentVariable('DOTNET_CLI_TELEMETRY_OPTOUT','1','Machine')
[Environment]::SetEnvironmentVariable('POWERSHELL_TELEMETRY_OPTOUT','1','Machine')
```

---

## 8. Group Policy equivalents (Pro/Enterprise)

Todas estas rutas GPO son mapeables a las del regedit arriba; útil si tu app soporta empresas.

| Feature | GPO Path | Registry |
|---|---|---|
| Telemetría | Computer > Admin Templates > Windows Components > Data Collection and Preview Builds | `HKLM\...\DataCollection\AllowTelemetry` |
| Cortana | Computer > Admin Templates > Windows Components > Search > "Allow Cortana" | `HKLM\...\Windows Search\AllowCortana` |
| Copilot | User > Admin Templates > Windows Components > Windows Copilot > "Turn off Windows Copilot" | `HKCU\...\WindowsCopilot\TurnOffWindowsCopilot` |
| OneDrive | Computer > Admin Templates > Windows Components > OneDrive | `HKLM\...\OneDrive\DisableFileSyncNGSC` |
| Windows Update | Computer > Admin Templates > Windows Components > Windows Update | Varias `HKLM\...\WindowsUpdate` |
| Recall (AI) | Computer > Admin Templates > Windows Components > Windows AI | `HKLM\...\WindowsAI\*` |
| App Package Deployment (Win11 25H2+) | Computer > Admin Templates > Windows Components > App Package Deployment > "Remove default Microsoft Store packages" | NUEVO en 25H2, policy CSP |

---

## 9. Lo que NO se debe tocar (errores comunes)

### 9.1 Servicios core (garantizado romper)

- **RPC (RpcSs)**, **DCOM Server (DcomLaunch)**, **PlugPlay**, **Power**, **nsi**, **EventLog**, **GpSvc (Group Policy Client)**, **CryptSvc**, **LSASS helpers** — kernel-level. Tocarlos corrompe sesión/arranque.

### 9.2 Tweaks peligrosos que circulan en foros

- `ClearPageFileAtShutdown=1` — **hace shutdown extremadamente lento** y aporta cero security si usas BitLocker.
- `DisableDefrag` en SSD — Windows ya detecta SSD y ejecuta TRIM, no fragmentación. Dejar servicio `defragsvc` en Manual.
- Disable `SecHealthUI` / `wscsvc` — rompe Security Center sin beneficio. Defender sigue activo.
- Deshabilitar `BITS` permanente — rompe Windows Update, Store, **Defender signature updates**.
- Tocar `NetworkList` / `LanmanServer` — rompe uso compartido LAN.
- `NoLMHash=1` — solo si controlas todo el dominio; rompe legacy devices.
- Deshabilitar `Schedule` (Task Scheduler) — rompe casi todo el mantenimiento del sistema.

### 9.3 Gaming snake-oil

- "Ultimate Performance" power plan — en desktops AC-powered modernos, diferencia vs High Performance es estadísticamente cero. Puede aumentar temp sin FPS gain.
- `DisablePagingExecutive=1` sin verificar RAM — en máquinas con <16GB causa BSOD bajo carga.
- `LargeSystemCache=1` en desktop — es setting para **server workloads**; en gaming causa stalls.
- `MmPagedPoolSize` / `SystemPages` overrides — Win10+ los calcula dinámico. Hardcodear causa kernel crashes.
- "TCP optimizer" extremos: `TcpWindowSize` fijo, MTU modificado — ISPs modernos negocian óptimo; hardcodear causa retransmits.

### 9.4 "Ghost" tweaks de Win7/8 que NO aplican Win11

- `ForegroundLockTimeout=0` — obsoleto desde Win10
- Disable `Superfetch` service — **ya no existe**; se llama `SysMain`
- `EnablePrefetcher=0` — rompe startup; SSD ya detecta
- `DisableLastAccess` via fsutil — Win10/11 ya tiene `NtfsDisableLastAccessUpdate=0x80000001` por default en installs post-2018

### 9.5 Defender / Security

**Nunca** deshabilites Defender via registry si **no** vas a instalar AV alternativo. Sin AV real (no MS Security Essentials legacy), el sistema es completamente vulnerable. ReviOS hace remoción agresiva solo porque asume usuario avanzado con comportamiento seguro.

- `DisableAntiSpyware=1` en `HKLM\...\Windows Defender` — **Windows 11 22H2+ lo ignora** si Tamper Protection está ON. Para deshabilitar real: Safe Mode → quitar servicios vía PsExec SYSTEM.

### 9.6 Nuevas trampas Win11 24H2/25H2

- Removing `AppLocker` service (`AppIDSvc`) — rompe el mecanismo del propio Copilot block policy moderno.
- Tocar `UCPD` (UserChoice Protection Driver) — **romper esto permite cambiar default apps más fácil, pero** también habilita malware que cambia default browser a voluntad. Lo deshabilita ReviOS; evalúa según threat model.
- `DiagTrack` en 24H2 está **protegido por TrustedInstaller**. Set-Service fallará. Necesitas PsExec SYSTEM o registry directo.

---

## 10. Recursos adicionales para tu app

### 10.1 Fuentes primarias (validar siempre)

- `Raphire/Win11Debloat` — `Regfiles/` es referencia canónica para registry tweaks validados community
- `ChrisTitusTech/winutil` — tiene JSON de features con categorización Essential/Advanced
- `meetrevision/playbook` (ReviOS) — hardcore mode, ver features docs
- `Atlas-OS/Atlas` — extrae YAML tweaks en `src/playbook`
- `ntdevlabs/tiny11builder` — `tiny11maker.ps1` para lógica DISM removal pre-install
- `builtbybel/privatezilla` — presets de privacy por categoría
- `crazy-max/WindowsSpyBlocker` — hosts file + firewall rules + trace analysis
- `Disassembler0/Win10-Initial-Setup-Script` — (obsoleto pero referencia PS module style)

### 10.2 Patrones implementables en tu app

1. **Categorización 3-tier** (Safe/Optional/Risky) con filtros visuales — copia de WinUtil
2. **Undo folder con .reg originales** — Win11Debloat pattern; restaura 1-click
3. **System Restore Point pre-apply** — default en WinUtil y Win11Debloat
4. **Progress callbacks** por feature — Win11Debloat `ExecuteChanges.ps1` line 174-195
5. **JSON-driven feature manifest** — Apps.json pattern. Permite updates sin redeploy
6. **Sysprep variants de regfiles** — Win11Debloat duplica cada `.reg` en `Sysprep/` con rutas `HKEY_USERS\DEFAULT` para aplicar a NEW users provisionados

### 10.3 Políticas clave (resumen 24H2/25H2)

- `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection\AllowTelemetry` — **1** mínimo en Pro, **0** solo Enterprise/Education
- `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI\*` — AI/Recall/Click-to-Do controls
- `HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent\*` — sugerencias/ads
- `HKLM\SOFTWARE\Policies\Microsoft\Windows\System\*` — Activity History, User Activities
- `HKLM\SOFTWARE\Policies\Microsoft\WindowsStore\*` — auto-updates, private store only

### 10.4 Comandos de diagnóstico útiles (para la app)

```powershell
# Verificar telemetría level actual
Get-ItemProperty 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' AllowTelemetry

# Verificar apps provisioned (affects new users)
Get-AppxProvisionedPackage -Online | Select DisplayName

# Verificar services startup
Get-Service | Where StartType -EQ 'Automatic' | Select Name, StartType, Status

# Verificar scheduled tasks enabled
Get-ScheduledTask | Where {$_.State -EQ 'Ready' -and $_.TaskPath -like '*Application Experience*'}

# Audit all Defender exclusions
Get-MpPreference | Select ExclusionPath, ExclusionProcess, ExclusionExtension

# Check if Copilot/Recall policy applied
Get-ItemProperty 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -EA SilentlyContinue
```

### 10.5 Inventario Win11Debloat completo (regfiles disponibles)

Referencia del repo `Raphire/Win11Debloat/Regfiles/`:
- Disable: AI_Recall, AI_Service_Auto_Start, Animations, Bing_Cortana_In_Search, Bitlocker_Auto_Encryption, Brave_Bloat, Chat_Taskbar, Click_to_Do, Copilot, Delivery_Optimization, Desktop_Spotlight, DVR, Edge_AI_Features, Edge_Ads_And_Suggestions, Enhance_Pointer_Precision, Fast_Startup, Find_My_Device, Game_Bar_Integration, Give_access_to_context_menu, Include_in_library_from_context_menu, Location_Services, Lockscreen_Tips, Modern_Standby_Networking, Notepad_AI_Features, Paint_AI_Features, Phone_Link_In_Start, Search_Highlights, Search_History, Settings_365_Ads, Settings_Home, Share_Drag_Tray, Share_from_context_menu, Show_More_Options_Context_Menu, Snap_Assist, Snap_Layouts, Start_All_Apps, Start_Recommended, Sticky_Keys_Shortcut, Storage_Sense, Telemetry, Transparency, Update_ASAP, Widgets_Service, Window_Snapping, Windows_Suggestions
- Enable: Dark_Mode, End_Task, Last_Active_Click
- Hide: 3D_Objects_Folder, Drive_Letters, Gallery_from_Explorer, Home_from_Explorer, Music_Folder, Onedrive_Folder, Search_Taskbar, Tabs_In_Alt_Tab, Taskview_Taskbar, duplicate_removable_drives_from_navigation_pane_of_File_Explorer
- Launch_File_Explorer_To: Downloads / Home / OneDrive / This_PC
- MMTaskbarMode: Active / All / Main_Active
- Combine_Taskbar / Combine_MMTaskbar: Always / Never / When_Full
- Show: 20/5/3 Tabs_In_Alt_Tab, Drive_Letters_First/Last, Extensions_For_Known_File_Types, Hidden_Folders, Network_Drive_Letters_First, Search_Box, Search_Icon, Search_Icon_And_Label
- Prevent_Auto_Reboot
- Add_All_Folders_Under_This_PC
- Align_Taskbar_Left

Cada uno tiene gemelo en `Regfiles/Sysprep/` (para aplicar a default user profile) y en `Regfiles/Undo/` (reversión).

---

## Sources

- [Raphire/Win11Debloat GitHub](https://github.com/Raphire/Win11Debloat) — main debloater, Release 2026.04.05
- [Win11Debloat Regfiles directory](https://github.com/Raphire/Win11Debloat/tree/master/Regfiles)
- [Win11Debloat Apps.json (145 apps)](https://github.com/Raphire/Win11Debloat/blob/master/Config/Apps.json)
- [ChrisTitusTech/winutil](https://github.com/ChrisTitusTech/winutil) — WinUtil tweaks
- [Winutil Documentation - Tweaks](https://winutil.christitus.com/userguide/tweaks/)
- [Atlas-OS/Atlas](https://github.com/Atlas-OS/Atlas) — Atlas playbook
- [Atlas OS Documentation](https://docs.atlasos.net/)
- [meetrevision/playbook (ReviOS)](https://github.com/meetrevision/playbook)
- [ReviOS Features Overview](https://revi.cc/docs/features) — full system changes list
- [ntdevlabs/tiny11builder](https://github.com/ntdevlabs/tiny11builder)
- [Tiny11 Removed Components](https://deepwiki.com/ntdevlabs/tiny11builder/7-removed-components)
- [builtbybel/privatezilla](https://github.com/builtbybel/privatezilla)
- [hellzerg/optimizer](https://github.com/hellzerg/optimizer) — archived Jan 2026
- [Disassembler0/Win10-Initial-Setup-Script](https://github.com/Disassembler0/Win10-Initial-Setup-Script)
- [crazy-max/WindowsSpyBlocker](https://github.com/crazy-max/WindowsSpyBlocker) — hosts + firewall + traces
- [O&O ShutUp10++](https://www.oo-software.com/en/shutup10)
- [Windows 11 Copilot disable - techcommunity.microsoft.com](https://techcommunity.microsoft.com/discussions/windows11/how-to-turn-off-or-disable-copilot-in-windows-11-permanently/4504962)
- [Manage Recall for Windows clients - Microsoft Learn](https://learn.microsoft.com/en-us/windows/client-management/manage-recall)
- [WindowsAI Policy CSP - Microsoft Learn](https://learn.microsoft.com/en-us/windows/client-management/mdm/policy-csp-windowsai)
- [PrintNightmare CVE-2021-34527 - Microsoft MSRC](https://msrc.microsoft.com/update-guide/vulnerability/CVE-2021-34527)
- [CISA PrintNightmare alert](https://www.cisa.gov/news-events/alerts/2021/06/30/printnightmare-critical-windows-print-spooler-vulnerability)
- [Safety of disabling services in Windows 10 and 11 - GitHub Gist (Aldaviva)](https://gist.github.com/Aldaviva/0eb62993639da319dc456cc01efa3fe5)
- [Elevenforum: Which services do you recommend to disable](https://www.elevenforum.com/t/which-services-do-you-recommend-to-disable-here-is-my-list.36437/)
- [XDA: I disabled these 5 Windows 11 background services](https://www.xda-developers.com/i-disabled-these-5-windows-11-background-services-and-saw-zero-downsides/)
- [XDA: Disabling SysMain to improve performance](https://www.xda-developers.com/i-make-this-one-change-to-make-windows-faster/)
- [The Register: 11 Windows 11 Registry tweaks](https://www.theregister.com/2025/09/21/windows_11_registry_hacks_regedit/)
- [Microsoft Q&A: Completely uninstall preinstalled apps on Windows 11](https://learn.microsoft.com/en-us/answers/questions/789733/completely-uninstall-preinstalled-apps-on-windows)
- [Remove-AppxPackage Microsoft Learn](https://learn.microsoft.com/en-us/powershell/module/appx/remove-appxpackage?view=windowsserver2025-ps)
- [Configure custom exclusions for Microsoft Defender - Microsoft Learn](https://learn.microsoft.com/en-us/defender-endpoint/configure-exclusions-microsoft-defender-antivirus)
- [Win32PrioritySeparation - XbitLabs](https://www.xbitlabs.com/blog/win32priorityseparation-performance/)
- [Geekflare: 8 Windows Registry Hacks for Gaming](https://geekflare.com/gaming/windows-registry-hacks-to-improve-gaming/)
- [Windows 11 25H2 debloat tool - TechSpot](https://www.techspot.com/news/108600-windows-11-25h2-adds-tool-debloat-os-remove.html)
- [PatchMyPC: Windows 11 25H2 Remove Default MS Store Packages](https://patchmypc.com/blog/remove-default-microsoft-store-app-packages-windows11-25h2/)
- [Restore classic context menu - Winhelponline](https://www.winhelponline.com/blog/get-classic-full-context-menu-windows-11/)
- [TeamLabs Blog: Remove Teams chat via PowerShell](https://techlabs.blog/categories/office-365/remove-teams-chat-personal-windows-11-using-powershell-endpoint-manager-intune)
- [Woshub: Removing Built-in Teams Chat](https://woshub.com/remove-teams-chat-windows/)
- [GitHub asheroto/UninstallOneDrive](https://github.com/asheroto/UninstallOneDrive)
