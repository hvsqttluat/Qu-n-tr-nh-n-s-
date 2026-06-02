param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputRoot = "artifacts"
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\QuanLyNhanSuWpf\QuanLyNhanSuWpf.csproj"
$outputRootPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\$OutputRoot"))
$publishDir = [System.IO.Path]::GetFullPath((Join-Path $outputRootPath "QuanLyNhanSuWpf"))
$zipPath = [System.IO.Path]::GetFullPath((Join-Path $outputRootPath "QuanLyNhanSuWpf-$Runtime.zip"))

dotnet test (Join-Path $PSScriptRoot "..\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln") -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Automated tests failed. Release package was not created."
}

if (-not $publishDir.StartsWith($outputRootPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Publish directory must remain inside the configured artifacts folder."
}

if (Test-Path $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

dotnet publish $project -c $Configuration -r $Runtime --self-contained true -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "Application publish failed. Release package was not created."
}

Copy-Item -Path (Join-Path $PSScriptRoot "..\README.md") -Destination (Join-Path $publishDir "README.md") -Force

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force
Write-Host "Created package: $zipPath"
