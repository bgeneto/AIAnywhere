param(
    [string]$Version = "1.1.8"
)

Write-Host "Building AI Anywhere v$Version..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "AIAnywhere-Distribution") { Remove-Item "AIAnywhere-Distribution" -Recurse -Force }
if (Test-Path "AIAnywhere-Optimized") { Remove-Item "AIAnywhere-Optimized" -Recurse -Force }
Remove-Item "AIAnywhere-v*.zip" -ErrorAction SilentlyContinue

# Build speed-optimized version (requires .NET runtime)
Write-Host "Building speed-optimized version..." -ForegroundColor Yellow
dotnet publish AIAnywhere.sln -c Release -r win-x64 --self-contained false `
    /p:PublishReadyToRun=true `
#    /p:OptimizationPreference=Speed `
#    /p:TieredCompilation=true `
#    /p:TieredPGO=true `
#    /p:ReadyToRunUseCrossgen2=true `
#    /p:DebuggerSupport=false `
#    /p:EnableEventPipeProfiler=false

# Create optimized distribution
New-Item -ItemType Directory -Name "AIAnywhere-Optimized" | Out-Null
Copy-Item "bin\Release\net10.0-windows\win-x64\publish\*.exe" "AIAnywhere-Optimized\"
Copy-Item "bin\Release\net10.0-windows\win-x64\publish\*.dll" "AIAnywhere-Optimized\"
Copy-Item "bin\Release\net10.0-windows\win-x64\publish\*.json" "AIAnywhere-Optimized\"
Copy-Item "bin\Release\net10.0-windows\win-x64\publish\*.ico" "AIAnywhere-Optimized\"
Copy-Item "bin\Release\net10.0-windows\win-x64\publish\*.png" "AIAnywhere-Optimized\"
Copy-Item "README.md" "AIAnywhere-Optimized\"

# Create installation files
@"
AI Anywhere - Installation Instructions
==========================================

- SYSTEM REQUIREMENTS:
  - Windows 10/11 (64-bit)
  - .NET 10.0 Runtime (Desktop) - Download link below if needed

- STEP 1: CHECK .NET INSTALLATION
  Try running "AIAnywhere.exe" first. If it works, you're all set!

  If you see an error about missing .NET:

    1. Download .NET 10.0 Desktop Runtime:
       https://dotnet.microsoft.com/download/dotnet/10.0
    2. Install the "Desktop Runtime" (not SDK)
    3. Restart and run AIAnywhere.exe

- QUICK START:
  1. Extract all files to your preferred location
  2. Run "AIAnywhere.exe" or use "Check and Start.bat"
  3. Configure your AI settings in the system tray

Package Size: ~6MB (Lightweight/Optimized - requires .NET 10.0)

© 2025 LABiA-FUP/UnB - AI Anywhere v$Version
"@ | Out-File "AIAnywhere-Optimized\INSTALL.txt" -Encoding UTF8

@"
@echo off
title AI Anywhere - .NET Check & Start
echo Checking for .NET 10.0 Runtime...
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 10." >nul
if %errorlevel% == 0 (
    echo ✅ .NET 10.0 is installed! Starting AI Anywhere...
    start "" "AIAnywhere.exe"
    echo AI Anywhere started in system tray.
) else (
    echo ❌ .NET 10.0 Desktop Runtime required.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/10.0
    set /p choice="Open download page? (y/n): "
    if /i "%choice%"=="y" start "" "https://dotnet.microsoft.com/download/dotnet/10.0"
)
pause
"@ | Out-File "AIAnywhere-Optimized\Check and Start.bat" -Encoding ASCII

# Create ZIP package
Write-Host "Creating ZIP package..." -ForegroundColor Yellow
$compress = @{
    Path = "AIAnywhere-Optimized\*"
    CompressionLevel = "Optimal"
    DestinationPath = "AIAnywhere-v$Version-Optimized.zip"
}
Compress-Archive @compress -Force

# Show results
$size = (Get-Item "AIAnywhere-v$Version-Optimized.zip").Length
Write-Host "✅ Build complete!" -ForegroundColor Green
Write-Host "📦 Package: AIAnywhere-v$Version-Optimized.zip" -ForegroundColor Cyan
Write-Host "📏 Size: $([math]::Round($size/1MB,2)) MB" -ForegroundColor Cyan
Write-Host "🎯 Requires: .NET 10.0 Desktop Runtime" -ForegroundColor Cyan
