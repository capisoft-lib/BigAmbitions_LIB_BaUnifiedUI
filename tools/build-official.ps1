param(
    [string]$SdkRoot,
    [switch]$RequireConsumerOutputs
)

$ErrorActionPreference = "Stop"

$modRoot = (Get-Item $PSScriptRoot).Parent.FullName
if ([string]::IsNullOrWhiteSpace($SdkRoot)) {
    $SdkRoot = [System.IO.Path]::GetFullPath((Join-Path $modRoot "..\..\.."))
} else {
    $SdkRoot = [System.IO.Path]::GetFullPath($SdkRoot)
}

. (Join-Path $SdkRoot "scripts\_project.ps1")

$lockFile = Join-Path $SdkRoot "Temp\UnityLockfile"
if (Test-Path -LiteralPath $lockFile) {
    throw "The SDK project is already open or locked. Close that Unity Editor before running the headless release build: $lockFile"
}

Assert-UnityProductVersion -BinaryPath $UnityEditor -Component "SDK Unity Editor"
$logDir = Join-Path $SdkRoot "Logs"
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$logPath = Join-Path $logDir "baui-official-modbuilder.log"

$previousModId = [Environment]::GetEnvironmentVariable("BA_MOD_BUILD_CLI", "Process")
try {
    [Environment]::SetEnvironmentVariable("BA_MOD_BUILD_CLI", "LIB_BaUnifiedUI", "Process")
    $process = Start-Process `
        -FilePath $UnityEditor `
        -ArgumentList @("-batchmode", "-projectPath", $SdkRoot, "-logFile", $logPath) `
        -WindowStyle Hidden `
        -PassThru `
        -Wait
}
finally {
    [Environment]::SetEnvironmentVariable("BA_MOD_BUILD_CLI", $previousModId, "Process")
}

if ($process.ExitCode -ne 0) {
    throw "Official Big Ambitions Mod Builder failed with exit code $($process.ExitCode). See $logPath"
}

$buildMarker = "[ModBuildCli] Build succeeded: " + (Join-Path $SdkRoot "Output\LIB_BaUnifiedUI")
if (-not (Select-String -LiteralPath $logPath -SimpleMatch $buildMarker -Quiet)) {
    throw "Unity exited successfully but the Mod Builder success marker is missing. See $logPath"
}

$validator = Join-Path $modRoot "tools\validate-workshop-release.ps1"
& $validator `
    -SdkRoot $SdkRoot `
    -RequireInstalled `
    -RequireConsumerOutputs:$RequireConsumerOutputs

Write-Host "[build] Official LIB_BaUnifiedUI release artifact is ready at $(Join-Path $SdkRoot 'Output\LIB_BaUnifiedUI')."
