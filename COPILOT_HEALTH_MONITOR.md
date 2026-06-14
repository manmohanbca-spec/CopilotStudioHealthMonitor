# Copilot Studio Agent Health Monitor — XrmToolBox Plugin

**Author:** Manmohan (EY, Pune) — Solution Architect, Microsoft Power Platform  
**Status:** Phase 1 Complete ✅ | Phases 2–5 Planned  
**Last Updated:** 2026-06-14

---

## What This Plugin Does

An XrmToolBox plugin that audits, secures, and validates Microsoft Copilot Studio agents across Power Platform / Dataverse environments. Gives admins and architects a single pane of glass to:

- Inventory all agents in an org
- Run security checks (auth mode, orphaned owners, HTTP actions, DLP gaps)
- Validate deployment readiness before promoting to UAT/PROD
- Diff agents across environments (ALM)
- Dashboard with risk-ranked agent list

---

## Project Location

| Item | Path |
|---|---|
| Solution | `C:\Project\XRMToolbox Tools\CopilotStudioHealthMonitor.sln` |
| Project | `C:\Project\XRMToolbox Tools\CopilotStudioHealthMonitor\` |
| Local DLL references | `...\CopilotStudioHealthMonitor\lib\` |
| Installed plugin | `%APPDATA%\MscrmTools\XrmToolBox\Plugins\CopilotStudioHealthMonitor.dll` |
| XrmToolBox exe | `D:\old download\XrmToolbox\XrmToolBox.exe` |

---

## Build & Deploy Commands

```powershell
# Full rebuild
cd "C:\Project\XRMToolbox Tools\CopilotStudioHealthMonitor"
dotnet msbuild /p:Configuration=Release /t:Rebuild /v:m

# Deploy (XrmToolBox must be CLOSED first)
Copy-Item "bin\Release\CopilotStudioHealthMonitor.dll" "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\" -Force
```

---

## Architecture

### Tech Stack
- **Framework:** .NET Framework 4.7.2 (old-style csproj, not SDK-style)
- **UI:** WinForms only (no WPF, no XAML)
- **Dataverse SDK:** Microsoft.Xrm.Sdk 9.2.49 (from XrmToolBox install)
- **XrmToolBox:** v1.2025.10.74
- **JSON:** Newtonsoft.Json 13.0.3
- **Build tool:** `dotnet msbuild` (MSBuild 16.4 via .NET Core 3.1 SDK)

### DLL References (lib\ folder — copied from XrmToolBox install)
All DLLs are sourced directly from `D:\old download\XrmToolbox\` to ensure exact version match with the running host:

| DLL | Version |
|---|---|
| XrmToolBox.Extensibility.dll | 1.2025.10.74 |
| XrmToolBox.ToolLibrary.dll | 1.2025.10.74 |
| McTools.Xrm.Connection.dll | 1.2025.9.64 |
| McTools.Xrm.Connection.WinForms.dll | 1.2025.9.64 |
| Microsoft.Xrm.Sdk.dll | 9.2.49.14828 |
| Microsoft.Crm.Sdk.Proxy.dll | 9.2.49.14828 |
| Newtonsoft.Json.dll | 13.0.3 |

### Plugin Discovery Pattern (Critical)
XrmToolBox uses MEF (Managed Extensibility Framework) to discover plugins. The entry point class **must** have these attributes or it will never appear in the tool list:

```csharp
[Export(typeof(IXrmToolBoxPlugin))]
[ExportMetadata("Name", "Copilot Studio Agent Health Monitor")]
[ExportMetadata("Description", "...")]
[ExportMetadata("BackgroundColor", "#0078D4")]
[ExportMetadata("PrimaryFontColor", "White")]
[ExportMetadata("SecondaryFontColor", "LightBlue")]
[ExportMetadata("SmallImageBase64", "")]
[ExportMetadata("BigImageBase64", "")]
public class CopilotStudioHealthMonitorPlugin : PluginBase
```

Required namespaces: `System.ComponentModel.Composition`, `XrmToolBox.Extensibility`, `XrmToolBox.Extensibility.Interfaces`  
Required csproj reference: `<Reference Include="System.ComponentModel.Composition" />`

### Tab ↔ PluginControl Event Pattern
Tabs never call services directly. They raise events; PluginControl handles them with WorkAsync:

```
TabControl (UI only)
  └── raises: LoadAgentsRequested, LoadComponentsRequested
        └── PluginControl handles via WorkAsync (background thread)
              └── calls AgentInventoryService (Dataverse SDK)
```

This keeps the XrmToolBox threading contract intact — service calls never happen on the UI thread.

---

## File Structure

```
CopilotStudioHealthMonitor/
├── CopilotStudioHealthMonitor.csproj
├── MyPlugin.cs                    ← Plugin entry point (Export attributes here)
├── PluginControl.cs               ← Main control (WorkAsync orchestration)
├── PluginControl.Designer.cs
├── lib/                           ← Local DLL copies (from XrmToolBox install dir)
│   ├── XrmToolBox.Extensibility.dll
│   ├── McTools.Xrm.Connection.dll
│   ├── Microsoft.Xrm.Sdk.dll
│   └── ... (7 DLLs total)
├── Models/
│   ├── AgentModel.cs
│   ├── AgentSecurityResult.cs
│   ├── BotComponentModel.cs
│   └── DeploymentCheckResult.cs
├── Services/
│   └── AgentInventoryService.cs   ← FetchXML on bot table
└── Controls/
    ├── InventoryTab.cs
    └── InventoryTab.Designer.cs
```

---

## Dataverse Key Facts

| Thing | Detail |
|---|---|
| Agent table | `bot` |
| Agent components | `botcomponent` (linked via `parentbotid`) |
| Solution component type for bot | `481` (in `solutioncomponent` table) |
| botcomponent types | `0` = Topic, `1` = Action, `9` = Knowledge Source |
| Auth mode option values | `0` = No Auth, `1` = Azure AD, `2` = External |
| HTTP action detection | `botcomponent.content` JSON contains `"type":"OpenApiConnection"` or `"httpRequest"` |

---

## Phase Roadmap

### ✅ Phase 1 — Agent Inventory (DONE)
- FetchXML query on `bot` table with owner join
- DataGridView showing all agents
- Color coding: red = No Auth, yellow = disabled owner
- Detail panel with agent properties
- Bot components list (Topics / Actions / Knowledge Sources)
- CSV export
- Search/filter

### 🔲 Phase 2 — Security Scanner
**File to create:** `Services\SecurityScannerService.cs`, `Controls\SecurityTab.cs`

Security checks:
| ID | Check | Score Impact |
|---|---|---|
| SEC-01 | No Authentication | -30 |
| SEC-03 | HTTP Request actions (bypasses DLP) | -10 |
| SEC-05 | Orphaned agent (owner disabled) | -20 |
| SEC-07 | Not in solution (no ALM) | -5 |

Score labels: `≥85` = 🟢 Healthy, `≥60` = 🟡 Needs Attention, `<60` = 🔴 Critical

UI: DataGridView with color-coded score column + expandable failed checks list

### 🔲 Phase 3 — Deployment Readiness
**File to create:** `Services\DeploymentReadinessService.cs`, `Controls\DeploymentTab.cs`

Checks:
1. Agent is in a solution
2. Auth mode is not "No Auth"
3. Environment variables have values in target
4. Connection references mapped in target

UI: Checklist with ✅/❌ per check, remediation text, second org connection picker

### 🔲 Phase 4 — ALM Diff
**File to create:** `Services\AlmDiffService.cs`, `Controls\AlmDiffTab.cs`

- Side-by-side diff of agent components between source and target env
- Diff statuses: ✅ Match, ⚠️ Content Differs, ❌ Missing in Target, Only in Target
- Compares Topics (type 0), Actions (type 1), Knowledge Sources (type 9)
- Uses `botcomponent.content` hash for content comparison

### 🔲 Phase 5 — Dashboard
**File to create:** `Controls\DashboardTab.cs`

- Summary cards: total agents, critical agents, agents with no auth, orphaned agents
- Risk-ranked agent list (sorted by security score ascending)
- Quick-launch buttons to jump to Security/Deployment tabs for selected agent

---

## Build Challenges & Resolutions (Session Learnings)

### 1. Wrong NuGet package name
- ❌ `McD1982.XrmToolBox` — does not exist on NuGet.org
- ✅ `XrmToolBoxPackage` — correct package ID

### 2. XrmToolBoxPackage 2025 requires net48, causes WebView2 targets error
- `XrmToolBoxPackage 1.2025.x` needs `net48`
- WebView2 (transitive dep) has `.targets` file using `[MSBuild]::IsTargetFrameworkCompatible` — only in MSBuild 16.9+
- Our environment has .NET Core SDK 3.1 = MSBuild 16.4 → crashes during restore
- ✅ **Fix:** Skip NuGet restore entirely. Copy DLLs directly from `D:\old download\XrmToolbox\` into `lib\` folder. Use HintPaths.

### 3. MSBuild drops references targeting higher TFM (MSB3274)
- DLLs claim `net48` at assembly level even when in `net462` NuGet folder
- MSBuild 16.4 with `TargetFrameworkVersion=v4.7.2` refuses to compile against net48 DLLs
- ✅ **Fix:** Use `<ReferencePath>` items instead of `<Reference>` for XrmToolBox/McTools DLLs. `ReferencePath` bypasses `ResolveAssemblyReference` task entirely and goes straight to the compiler.

### 4. `IXrmToolBoxPluginControl` namespace
- Interface is in `XrmToolBox.Extensibility.Interfaces`, not `XrmToolBox.Extensibility`
- ✅ **Fix:** Add `using XrmToolBox.Extensibility.Interfaces;`

### 5. Plugin not visible in XrmToolBox tool list (biggest blocker)
- XrmToolBox uses **MEF (Managed Extensibility Framework)** for plugin discovery
- Without `[Export(typeof(IXrmToolBoxPlugin))]` and `[ExportMetadata(...)]` attributes, the plugin is silently skipped
- XrmToolBox logs it in `ScannedAssemblies` in manifest.json but never adds it to `PluginMetadata`
- ✅ **Fix:** Add `[Export]` + 7× `[ExportMetadata]` attributes to the plugin class + `System.ComponentModel.Composition` reference

### 6. Always copy DLLs from actual XrmToolBox install, not NuGet
- Version mismatch between compile-time DLLs and runtime DLLs causes silent load failures
- ✅ **Rule:** Always source `lib\` DLLs from `D:\old download\XrmToolbox\*.dll`

---

## XrmToolBox Plugin Checklist (for future phases)

- [ ] Entry class decorated with `[Export(typeof(IXrmToolBoxPlugin))]`
- [ ] All 7 `[ExportMetadata]` attributes present (Name, Description, BackgroundColor, PrimaryFontColor, SecondaryFontColor, SmallImageBase64, BigImageBase64)
- [ ] `System.ComponentModel.Composition` in csproj references
- [ ] `using XrmToolBox.Extensibility.Interfaces;` wherever `IXrmToolBoxPluginControl` is used
- [ ] All service calls inside `WorkAsync` — never on UI thread
- [ ] DLLs in `lib\` sourced from `D:\old download\XrmToolbox\`
- [ ] XrmToolBox closed before deploying new DLL
- [ ] Build: `dotnet msbuild /p:Configuration=Release /t:Rebuild /v:m`
- [ ] Deploy: copy `bin\Release\CopilotStudioHealthMonitor.dll` to Plugins folder
