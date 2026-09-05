# redeploy-flexifit-adminpanel.ps1
# Run this as Administrator whenever you make changes to the FlexiFit Admin Panel source code
# and want to update the version running on your local IIS.

# --- Check for Administrator privileges ---
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator." -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as administrator', then try again." -ForegroundColor Red
    exit 1
}

# --- Load IIS management module ---
Import-Module WebAdministration

# --- Variables Configuration for FlexiFit Admin Panel ---
$appPoolName = "FlexiFitAdminPanelPool"
$projectPath = "C:\FlexiFitAdminPanel\FlexiFit_AdminPanel.csproj"
$outputPath  = "C:\inetpub\wwwroot\FlexiFitAdminPanel"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "     FLEXIFIT ADMIN PANEL - IIS REDEPLOYMENT" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/4] Stopping App Pool: $appPoolName ..." -ForegroundColor Yellow
Stop-WebAppPool -Name $appPoolName

Write-Host "[2/4] Waiting for worker process to release files..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host "[3/4] Publishing Admin Panel project..." -ForegroundColor Yellow
dotnet publish $projectPath -c Release -o $outputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publishing failed. Please check the project path and build errors." -ForegroundColor Red
    Start-WebAppPool -Name $appPoolName
    exit 1
}

Write-Host "[4/4] Starting App Pool: $appPoolName ..." -ForegroundColor Yellow
Start-WebAppPool -Name $appPoolName

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "  Admin Panel: http://localhost:8070" -ForegroundColor Green
Write-Host "  API: http://localhost:8090" -ForegroundColor Green
Write-Host "  Public API: https://flexifitapinet.shares.zrok.io" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""