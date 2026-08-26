param(
    [string]$SdkRoot,
    [switch]$RequireConsumerOutputs,
    [switch]$RequireInstalled
)

$ErrorActionPreference = "Stop"

$modRoot = (Get-Item $PSScriptRoot).Parent.FullName
if ([string]::IsNullOrWhiteSpace($SdkRoot)) {
    $SdkRoot = [System.IO.Path]::GetFullPath((Join-Path $modRoot "..\..\.."))
} else {
    $SdkRoot = [System.IO.Path]::GetFullPath($SdkRoot)
}

$modsRoot = Join-Path $SdkRoot "Assets\Mods"
$outputRoot = Join-Path $SdkRoot "Output"
$modId = "LIB_BaUnifiedUI"
$version = (Get-Content -LiteralPath (Join-Path $modRoot "VERSION") -Raw).Trim()
$manifestText = Get-Content -LiteralPath (Join-Path $modRoot "ModManifest.asset") -Raw
$versionSource = Get-Content -LiteralPath (Join-Path $modRoot "Scripts\Core\BaUiVersion.cs") -Raw

if ($manifestText -notmatch '(?m)^\s*Version:\s*(\S+)\s*$' -or $matches[1] -ne $version) {
    throw "ModManifest version does not match VERSION '$version'."
}
if ($versionSource -notmatch 'Version\s*=\s*"([^"]+)"' -or $matches[1] -ne $version) {
    throw "BaUiVersion.Version does not match VERSION '$version'."
}

$releaseDir = Join-Path $modRoot "releases\$version"
$requiredSourceFiles = @(
    (Join-Path $modRoot "LICENSE"),
    (Join-Path $modRoot "CHANGELOG.md"),
    (Join-Path $modRoot "README.md"),
    (Join-Path $modRoot "PUBLISHING.md"),
    (Join-Path $modRoot "Thumbnail.png"),
    (Join-Path $releaseDir "short-description.txt"),
    (Join-Path $releaseDir "full-description.md"),
    (Join-Path $releaseDir "Steam_ChangeLog.md")
)
foreach ($path in $requiredSourceFiles) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing Workshop release input: $path"
    }
}

$shortDescription = (Get-Content -LiteralPath (Join-Path $releaseDir "short-description.txt") -Raw).Trim()
if ($shortDescription.Length -eq 0 -or $shortDescription.Length -gt 300) {
    throw "Steam short description must contain 1-300 characters; found $($shortDescription.Length)."
}

$thumbnail = Get-Item -LiteralPath (Join-Path $modRoot "Thumbnail.png")
if ($thumbnail.Length -gt 1MB) {
    throw "Workshop preview exceeds 1 MiB: $($thumbnail.Length) bytes."
}
Add-Type -AssemblyName System.Drawing
$thumbnailImage = [System.Drawing.Image]::FromFile($thumbnail.FullName)
try {
    if ($thumbnailImage.Width -ne $thumbnailImage.Height -or $thumbnailImage.Width -lt 512) {
        throw "Workshop preview must be square and at least 512 px; found $($thumbnailImage.Width)x$($thumbnailImage.Height)."
    }
    $thumbnailWidth = $thumbnailImage.Width
    $thumbnailHeight = $thumbnailImage.Height
}
finally {
    $thumbnailImage.Dispose()
}

$outputDll = Join-Path $outputRoot "$modId\$modId.dll"
if (-not (Test-Path -LiteralPath $outputDll)) {
    throw "Missing library output: $outputDll"
}
$outputThumbnail = Join-Path $outputRoot "$modId\Thumbnail.png"
if (-not (Test-Path -LiteralPath $outputThumbnail)) {
    throw "Workshop output is missing Thumbnail.png: $outputThumbnail"
}
if ((Get-FileHash -LiteralPath $outputThumbnail -Algorithm SHA256).Hash -ne
    (Get-FileHash -LiteralPath $thumbnail.FullName -Algorithm SHA256).Hash) {
    throw "Workshop output Thumbnail.png does not match the validated source preview."
}

$assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($outputDll).Name
if ($assemblyName -ne $modId) {
    throw "Standalone assembly identity must be '$modId'; found '$assemblyName'."
}

. (Join-Path $SdkRoot "scripts\_project.ps1")
Assert-PlayerRuntimeAssembly -DllPath $outputDll

if ($RequireInstalled) {
    $installedDll = Join-Path $ModsLocalRoot "$modId\$modId.dll"
    $installedThumbnail = Join-Path $ModsLocalRoot "$modId\Thumbnail.png"
    if (-not (Test-Path -LiteralPath $installedDll)) {
        throw "Official install is missing: $installedDll"
    }
    if ((Get-FileHash -LiteralPath $outputDll -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $installedDll -Algorithm SHA256).Hash) {
        throw "Official output and ModsLocal install hashes do not match."
    }
    if (-not (Test-Path -LiteralPath $installedThumbnail)) {
        throw "Installed Workshop mod is missing Thumbnail.png: $installedThumbnail"
    }
    if ((Get-FileHash -LiteralPath $installedThumbnail -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $thumbnail.FullName -Algorithm SHA256).Hash) {
        throw "Installed Thumbnail.png does not match the validated source preview."
    }
}

$consumerIds = @(
    "AutoShopping",
    "BaChat",
    "BetterFines",
    "CasinoAutoPlay",
    "FirstPersonMode",
    "Gagazon",
    "OffshoreFactory",
    "RealComputerGame",
    "VoogleRoute"
)

$sourceBundledCopies = @(
    foreach ($consumerId in $consumerIds) {
        $dependencies = Join-Path $modsRoot "$consumerId\Dependencies"
        if (Test-Path -LiteralPath $dependencies) {
            Get-ChildItem -LiteralPath $dependencies -Filter "LIB_BaUnifiedUI*.dll" -File -ErrorAction SilentlyContinue
        }
    }
)
if ($sourceBundledCopies.Count -gt 0) {
    throw "Consumer source folders still bundle BAUI: $($sourceBundledCopies.FullName -join ', ')"
}

$verifiedConsumerOutputs = 0
foreach ($consumerId in $consumerIds) {
    $consumerRoot = Join-Path $modsRoot $consumerId
    $requiredMods = Join-Path $consumerRoot "REQUIRED_MODS.txt"
    if (-not (Test-Path -LiteralPath $requiredMods)) {
        throw "$consumerId is missing REQUIRED_MODS.txt."
    }
    $requiredText = Get-Content -LiteralPath $requiredMods -Raw
    if ($requiredText -notmatch 'LIB_BaUnifiedUI\s+([0-9]+\.[0-9]+\.[0-9]+)\+') {
        throw "$consumerId does not declare a minimum LIB_BaUnifiedUI version in REQUIRED_MODS.txt."
    }
    $declaredUiMinimum = [version]$matches[1]
    if ($declaredUiMinimum -lt [version]'0.2.0') {
        throw "$consumerId declares LIB_BaUnifiedUI $declaredUiMinimum+, below the supported 0.2.0 minimum."
    }

    $consumerOutput = Join-Path $outputRoot $consumerId
    $consumerDll = Join-Path $consumerOutput "$consumerId.dll"
    if (-not (Test-Path -LiteralPath $consumerDll)) {
        if ($RequireConsumerOutputs) {
            throw "Missing consumer output: $consumerDll"
        }
        continue
    }

    $bundledCopies = @(Get-ChildItem -LiteralPath $consumerOutput -Filter "LIB_BaUnifiedUI*.dll" -File -Recurse -ErrorAction SilentlyContinue)
    if ($bundledCopies.Count -gt 0) {
        throw "$consumerId output bundles BAUI: $($bundledCopies.FullName -join ', ')"
    }

    $consumerAssembly = [System.Reflection.Assembly]::LoadFile($consumerDll)
    $uiReferences = @($consumerAssembly.GetReferencedAssemblies() | Where-Object { $_.Name -like 'LIB_BaUnifiedUI*' })
    if ($uiReferences.Count -ne 1 -or $uiReferences[0].Name -ne $modId) {
        $found = @($uiReferences | ForEach-Object { $_.Name }) -join ", "
        throw "$consumerId must reference exactly '$modId'; found '$found'."
    }

    $verifiedConsumerOutputs++
}

$dll = Get-Item -LiteralPath $outputDll
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $outputDll).Hash
Write-Host "[verify] version=$version assembly=$assemblyName"
Write-Host "[verify] library=$($dll.Length) bytes sha256=$hash"
Write-Host "[verify] preview=${thumbnailWidth}x${thumbnailHeight} bytes=$($thumbnail.Length) packaged=True installed=$RequireInstalled"
Write-Host "[verify] consumers=$verifiedConsumerOutputs/$($consumerIds.Count) source_bundles=0 output_bundles=0"
Write-Host "[verify] Workshop release inputs and standalone dependency contract passed."
