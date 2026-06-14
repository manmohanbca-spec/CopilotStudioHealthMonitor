# Publishing to the XrmToolBox Community Store

This document walks you through the complete process of registering your plugin with the XrmToolBox Tool Library so other users can discover and install it with one click.

---

## Overview of the Process

```
1. Prepare the plugin
      ↓
2. Create a GitHub repository (public)
      ↓
3. Build and pack the NuGet package
      ↓
4. Publish to NuGet.org
      ↓
5. Submit to the XrmToolBox website
      ↓
6. Users install via Tool Library
```

---

## Prerequisites

### 1 — NuGet.org Account

1. Go to [nuget.org](https://www.nuget.org) → **Sign in** → Register with your Microsoft or GitHub account.
2. Note your NuGet.org **API key** (Profile → API Keys → Create) — you'll need it for the `nuget push` command.

### 2 — NuGet CLI

Download `nuget.exe` from [nuget.org/downloads](https://www.nuget.org/downloads) and place it anywhere on your `PATH` (e.g., `C:\Tools\nuget.exe`).

Verify:
```powershell
nuget help
```

### 3 — Public GitHub Repository

The XrmToolBox team and community expect a public repository with:
- Source code
- `README.md` with screenshot or feature description
- MIT (or compatible) open-source `LICENSE`
- Releases page (GitHub Releases, tagged `v1.0.0`)

### 4 — Plugin Metadata (already done)

Your plugin's `[ExportMetadata]` attributes must include all of:

```csharp
[ExportMetadata("Name", "Copilot Studio Agent Health Monitor")]
[ExportMetadata("Description", "...")]
[ExportMetadata("BackgroundColor", "#0078D4")]
[ExportMetadata("PrimaryFontColor", "White")]
[ExportMetadata("SecondaryFontColor", "LightBlue")]
[ExportMetadata("SmallImageBase64", "...")]   // 32×32 PNG, base64
[ExportMetadata("BigImageBase64", "...")]     // 80×80 PNG, base64
```

All of these are already in place in `MyPlugin.cs`.

---

## Step-by-Step

### Step 1 — Create a GitHub Repository

1. Go to [github.com/new](https://github.com/new).
2. Repository name: `CopilotStudioHealthMonitor`
3. Visibility: **Public**
4. Initialize with: a README (you'll replace it with the one from this project)
5. License: **MIT**

```powershell
# From the project root
git init
git add .
git commit -m "Initial release v1.0.0"
git remote add origin https://github.com/YOUR_USERNAME/CopilotStudioHealthMonitor.git
git push -u origin main
```

6. Create a GitHub Release:
   - Tag: `v1.0.0`
   - Title: `v1.0.0 — Initial Release`
   - Attach: `CopilotStudioHealthMonitor.dll` from `bin\Release\`

### Step 2 — Build the Release DLL

```powershell
& "D:\VS\MSBuild\Current\Bin\MSBuild.exe" `
    "CopilotStudioHealthMonitor\CopilotStudioHealthMonitor.csproj" `
    /p:Configuration=Release /t:Rebuild /nologo
```

The DLL will be at: `CopilotStudioHealthMonitor\bin\Release\CopilotStudioHealthMonitor.dll`

### Step 3 — Pack the NuGet Package

From `C:\Project\XRMToolbox Tools\CopilotStudioHealthMonitor\`:

```powershell
nuget pack CopilotStudioHealthMonitor.nuspec -OutputDirectory ..\NuGetPackages\
```

This produces `CopilotStudioHealthMonitor.1.0.0.nupkg`.

Inspect the package to verify it contains the DLL in `lib\net462\`:
```powershell
nuget verify ..\NuGetPackages\CopilotStudioHealthMonitor.1.0.0.nupkg
```

Or unzip the `.nupkg` (it's a zip) and inspect the folder structure.

### Step 4 — Publish to NuGet.org

```powershell
nuget push ..\NuGetPackages\CopilotStudioHealthMonitor.1.0.0.nupkg `
    -ApiKey YOUR_NUGET_API_KEY `
    -Source https://api.nuget.org/v3/index.json
```

- The package will be listed on nuget.org within a few minutes.
- Full indexing (searchable) takes up to 30 minutes.
- Verify at: `https://www.nuget.org/packages/CopilotStudioHealthMonitor`

### Step 5 — Submit to xrmtoolbox.com

1. Go to [xrmtoolbox.com](https://www.xrmtoolbox.com) and sign in (GitHub or Microsoft account).
2. Click **Submit a Tool** (top menu or community section).
3. Fill in the submission form:

| Field | Value |
|---|---|
| **NuGet Package ID** | `CopilotStudioHealthMonitor` |
| **Tool Name** | Copilot Studio Agent Health Monitor |
| **Short Description** | Audit, secure and validate Copilot Studio agents across Power Platform environments |
| **GitHub URL** | `https://github.com/YOUR_USERNAME/CopilotStudioHealthMonitor` |
| **Compatible Versions** | XrmToolBox 1.2024.9.69+ |
| **Category** | Administration |
| **Tags** | Copilot Studio, Security, ALM, Bot, Deployment |

4. Submit. The XrmToolBox maintainers review submissions; approval typically takes 1–5 business days.

### Step 6 — Once Approved

Users will find your plugin via:
- **XrmToolBox → Tool Library → search "Copilot Studio"**
- One-click install, auto-update on new versions

---

## Releasing Updates

When you have a new version:

1. Bump the version in two places:
   - `Properties\AssemblyInfo.cs` → `AssemblyVersion` and `AssemblyFileVersion`
   - `CopilotStudioHealthMonitor.nuspec` → `<version>`

2. Rebuild:
   ```powershell
   & "D:\VS\MSBuild\Current\Bin\MSBuild.exe" ... /t:Rebuild
   ```

3. Pack and push:
   ```powershell
   nuget pack CopilotStudioHealthMonitor.nuspec -OutputDirectory ..\NuGetPackages\
   nuget push ..\NuGetPackages\CopilotStudioHealthMonitor.1.0.1.nupkg -ApiKey ... -Source https://api.nuget.org/v3/index.json
   ```

4. XrmToolBox auto-detects the new version on nuget.org — no additional store submission needed for updates.

---

## Important Notes

### Do NOT include in the NuGet package

XrmToolBox ships these DLLs itself — including them causes version conflicts:

- `Microsoft.Xrm.Sdk.dll`
- `Microsoft.Crm.Sdk.Proxy.dll`
- `Microsoft.Xrm.Tooling.Connector.dll`
- `XrmToolBox.dll`
- `Newtonsoft.Json.dll`

The `.nuspec` already only packages `CopilotStudioHealthMonitor.dll`, so this is handled correctly.

### Versioning convention

Follow semantic versioning (`MAJOR.MINOR.PATCH`):

| Change | Version bump |
|---|---|
| Breaking change / major redesign | MAJOR (2.0.0) |
| New feature, backwards compatible | MINOR (1.1.0) |
| Bug fix | PATCH (1.0.1) |

### NuGet package ID uniqueness

The ID `CopilotStudioHealthMonitor` must be unique on nuget.org. If it is already taken (by a prior test push or another author), choose an alternative such as `EY.CopilotStudioHealthMonitor` or `Manmohan.CopilotStudioHealthMonitor` and update both the `.nuspec` `<id>` and the `packages.config` dependency reference.

---

## Checklist Before Submission

- [ ] `bin\Release\CopilotStudioHealthMonitor.dll` builds cleanly with no warnings
- [ ] Plugin icon appears correctly in XrmToolBox (SmallImageBase64 + BigImageBase64)
- [ ] `AssemblyVersion` and `AssemblyFileVersion` in `AssemblyInfo.cs` match `.nuspec` `<version>`
- [ ] GitHub repository is public and has `README.md`, `LICENSE`, and a tagged release
- [ ] `NuGetPackages\CopilotStudioHealthMonitor.1.0.0.nupkg` contains only `lib\net462\CopilotStudioHealthMonitor.dll` (no Xrm*.dll)
- [ ] Package published to nuget.org and visible at `https://www.nuget.org/packages/CopilotStudioHealthMonitor`
- [ ] xrmtoolbox.com submission form completed and submitted
