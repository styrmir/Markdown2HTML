param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "publish",
    [switch]$Clean,
    [string[]]$RuntimeIdentifiers
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "Markdown2Html.csproj"

$targets = @(
    @{ Rid = "win-x64"; Hosts = @("Windows") },
    @{ Rid = "win-arm64"; Hosts = @("Windows") },
    @{ Rid = "linux-x64"; Hosts = @("Linux") },
    @{ Rid = "osx-x64"; Hosts = @("MacOS") },
    @{ Rid = "osx-arm64"; Hosts = @("MacOS") }
)

function Get-HostPlatform {
    if ($IsWindows) { return "Windows" }
    if ($IsLinux) { return "Linux" }
    if ($IsMacOS) { return "MacOS" }
    throw "Unsupported host operating system."
}

function Publish-Target([string]$rid, [string]$configuration, [string]$outputRoot) {
    $targetOutput = Join-Path (Join-Path $PSScriptRoot $outputRoot) $rid

    if (-not (Test-Path $targetOutput)) {
        New-Item -ItemType Directory -Path $targetOutput -Force | Out-Null
    }

    Write-Host "Publishing $rid -> $targetOutput" -ForegroundColor Cyan

    dotnet publish $projectPath `
        -c $configuration `
        -r $rid `
        --self-contained true `
        -p:PublishAot=true `
        -p:PublishSingleFile=true `
        -o $targetOutput

    Get-ChildItem -Path $targetOutput -Filter *.pdb -File -ErrorAction SilentlyContinue | Remove-Item -Force
}

$hostPlatform = Get-HostPlatform

if ($Clean) {
    $resolvedOutputRoot = Join-Path $PSScriptRoot $OutputRoot
    if (Test-Path $resolvedOutputRoot) {
        Remove-Item $resolvedOutputRoot -Recurse -Force
    }
}

$selectedTargets = if ($RuntimeIdentifiers -and $RuntimeIdentifiers.Count -gt 0) {
    $targets | Where-Object { $RuntimeIdentifiers -contains $_.Rid }
}
else {
    $targets
}

if (-not $selectedTargets -or $selectedTargets.Count -eq 0) {
    throw "No matching runtime identifiers were selected."
}

Write-Host "Host platform: $hostPlatform" -ForegroundColor Yellow

foreach ($target in $selectedTargets) {
    if ($target.Hosts -notcontains $hostPlatform) {
        Write-Warning "Skipping $($target.Rid): Native AOT publishing for this RID is expected to run on $($target.Hosts -join ', ') hosts."
        continue
    }

    Publish-Target -rid $target.Rid -configuration $Configuration -outputRoot $OutputRoot
}

Write-Host "AOT publish step complete." -ForegroundColor Green