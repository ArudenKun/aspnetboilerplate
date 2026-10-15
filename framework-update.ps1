<#
.SYNOPSIS
    Upgrades all .csproj files to target .NET 10.

.DESCRIPTION
    Recursively finds all .csproj files in the target folder (or current folder
    if none is specified) and updates their TargetFramework(s) to net10.0.
    Also updates common related properties like LangVersion if desired.

.PARAMETER TargetDir
    The root folder to search. Defaults to the current directory.

.PARAMETER WhatIf
    Preview changes without writing to disk.

.PARAMETER LangVersion
    Optional C# language version to set (e.g., 'latest', '14.0'). Omit to leave as-is.

.EXAMPLE
    .\Upgrade-ToNet10.ps1
    .\Upgrade-ToNet10.ps1 -TargetDir C:\src\MySolution
    .\Upgrade-ToNet10.ps1 -TargetDir .\src -WhatIf
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Position = 0)]
    [string]$TargetDir = (Get-Location).Path,

    [Parameter()]
    [string]$LangVersion
)

$ErrorActionPreference = 'Stop'

# --- Resolve and validate target directory -------------------------------
if ([string]::IsNullOrWhiteSpace($TargetDir)) {
    $TargetDir = (Get-Location).Path
}

if (-not (Test-Path -LiteralPath $TargetDir -PathType Container)) {
    throw "Target directory not found: $TargetDir"
}

$root = (Resolve-Path -LiteralPath $TargetDir).Path
Write-Host "Scanning for .csproj files under: $root" -ForegroundColor Cyan

# --- Find all csproj files (exclude bin/obj) -----------------------------
$csprojFiles = Get-ChildItem -LiteralPath $root -Filter *.csproj -Recurse -File |
    Where-Object {
        $_.FullName -notmatch '[\\/](bin|obj)[\\/]'
    }

if (-not $csprojFiles) {
    Write-Warning "No .csproj files found under $root"
    return
}

Write-Host "Found $($csprojFiles.Count) .csproj file(s)." -ForegroundColor Cyan

$updatedCount = 0

foreach ($file in $csprojFiles) {
    $path = $file.FullName
    try {
        [xml]$xml = Get-Content -LiteralPath $path -Raw
    }
    catch {
        Write-Warning "Skipping (not valid XML): $path -- $_"
        continue
    }

    # Locate the first PropertyGroup that has a TargetFramework(s) element
    $propertyGroups = @($xml.Project.PropertyGroup)
    $changed = $false

    foreach ($pg in $propertyGroups) {
        # Handle single TargetFramework
        if ($pg.TargetFramework) {
            $current = $pg.TargetFramework
            if ($current -ne 'net10.0') {
                if ($PSCmdlet.ShouldProcess($path, "TargetFramework '$current' -> 'net10.0'")) {
                    $pg.TargetFramework = 'net10.0'
                    $changed = $true
                }
            }
        }

        # Handle multi-targeting TargetFrameworks (semicolon-separated)
        if ($pg.TargetFrameworks) {
            $current = $pg.TargetFrameworks
            $parts = $current -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
            $newParts = $parts | ForEach-Object {
                if ($_ -match '^net\d') { 'net10.0' } else { $_ }
            } | Select-Object -Unique

            $newValue = ($newParts -join ';')
            if ($newValue -ne $current) {
                if ($PSCmdlet.ShouldProcess($path, "TargetFrameworks '$current' -> '$newValue'")) {
                    $pg.TargetFrameworks = $newValue
                    $changed = $true
                }
            }
        }

        # Optionally set LangVersion
        if ($LangVersion -and $pg.LangVersion) {
            if ($pg.LangVersion -ne $LangVersion) {
                if ($PSCmdlet.ShouldProcess($path, "LangVersion '$($pg.LangVersion)' -> '$LangVersion'")) {
                    $pg.LangVersion = $LangVersion
                    $changed = $true
                }
            }
        }
    }

    if ($changed) {
        if ($PSCmdlet.ShouldProcess($path, 'Save changes')) {
            $xml.Save($path)
            Write-Host "  [UPDATED] $path" -ForegroundColor Green
            $updatedCount++
        }
    }
    else {
        Write-Host "  [SKIP]    $path (already net10.0)" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "Done. Updated $updatedCount of $($csprojFiles.Count) file(s)." -ForegroundColor Cyan