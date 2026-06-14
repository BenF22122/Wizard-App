@echo off
echo ============================================
echo   🧙 Restoring Wizard Powers...
echo   Installing required magical components...
echo ============================================
echo.

REM ============================================================
REM  REMOVE OLD / UNUSED PACKAGES
REM ============================================================
echo 🧹 Cleaning unused packages...
dotnet remove package Microsoft.VisualBasic
dotnet remove package System.Diagnostics.PerformanceCounter

echo.

REM ============================================================
REM  CORE WIZARD DEPENDENCIES
REM ============================================================

REM System.Management (still needed for hardware info)
echo 🔧 Installing System.Management...
dotnet add package System.Management

REM NAudio (your audio spell)
echo 🔊 Installing NAudio...
dotnet add package NAudio

REM ============================================================
REM  METADATA SCRYER DEPENDENCIES
REM ============================================================

REM Office document metadata
echo 📄 Installing DocumentFormat.OpenXml...
dotnet add package DocumentFormat.OpenXml

REM Image EXIF metadata
echo 🖼️ Installing MetadataExtractor...
dotnet add package MetadataExtractor

REM Audio/Video metadata
echo 🎵 Installing TagLibSharp...
dotnet add package TagLibSharp

REM MSI metadata reader (WiX Toolset)
echo 📦 Installing WixToolset.Dtf.WindowsInstaller...

echo.
echo ============================================
echo   🔮 Restoring NuGet packages...
echo ============================================
dotnet restore

echo.
echo ============================================
echo   🛠️ Building the Wizard...
echo ============================================
dotnet build

echo.
echo ============================================
echo   ✨ Wizard fully empowered!
echo   You may now run: dotnet run
echo ============================================
pause
