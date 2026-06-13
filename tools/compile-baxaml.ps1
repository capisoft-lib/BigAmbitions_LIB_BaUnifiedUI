# BaXaml compiler (phase 2) — generates Scripts/BaXaml/Generated/*.g.cs from Panels/*.baxaml
# Pilot: GpsHud.baxaml is hand-synced with GpsHud.g.cs until the compiler is wired into build.
param(
    [string]$LibRoot = (Split-Path $PSScriptRoot -Parent)
)

$panels = Join-Path $LibRoot "Panels"
$generated = Join-Path $LibRoot "Scripts\BaXaml\Generated"
Write-Host "[baxaml] panels=$panels generated=$generated"
Write-Host "[baxaml] Pilot documents are maintained in sync manually; full XML codegen TBD."
