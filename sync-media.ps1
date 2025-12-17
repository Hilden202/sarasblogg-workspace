# DEV-verktyg – används endast lokalt
# Synkar bilder från GitHub sarasblogg-media repo till lokal SarasBlogg-Media mapp
# Kör detta när du vill hämta de senaste bilderna från GitHub

param(
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"

$localMediaPath = Join-Path $PSScriptRoot "SarasBloggAPI\SarasBlogg-Media"
$tempClonePath = Join-Path $env:TEMP "sarasblogg-media-sync"

Write-Host "Synkar media från GitHub till lokal mapp..." -ForegroundColor Cyan
Write-Host "Lokal mapp: $localMediaPath" -ForegroundColor Gray

# Ta bort temp om den finns
if (Test-Path $tempClonePath) {
    Remove-Item -Recurse -Force $tempClonePath
}

# Klona GitHub-repot
Write-Host "`nKlonar från GitHub..." -ForegroundColor Yellow
git clone --depth 1 --branch $Branch https://github.com/Hilden202/sarasblogg-media.git $tempClonePath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Fel vid kloning från GitHub" -ForegroundColor Red
    exit 1
}

# Kopiera filer (exkludera .git)
Write-Host "`nKopierar filer..." -ForegroundColor Yellow
Get-ChildItem -Path $tempClonePath -Exclude ".git" | ForEach-Object {
    $dest = Join-Path $localMediaPath $_.Name
    if ($_.PSIsContainer) {
        Copy-Item -Path $_.FullName -Destination $dest -Recurse -Force
    } else {
        Copy-Item -Path $_.FullName -Destination $dest -Force
    }
}

# Städa upp
Remove-Item -Recurse -Force $tempClonePath

Write-Host "`n✓ Klar! Bilder synkade till lokal mapp." -ForegroundColor Green
Write-Host "Antal filer:" -ForegroundColor Gray
Get-ChildItem -Recurse -File $localMediaPath | Measure-Object | Select-Object -ExpandProperty Count
