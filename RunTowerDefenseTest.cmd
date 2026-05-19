@echo off
setlocal

set "ROOT=%~dp0"
set "APPDATA=%TEMP%\TowerDefenseTest\AppData\Roaming"
set "LOCALAPPDATA=%TEMP%\TowerDefenseTest\AppData\Local"
set "ARTIFACTS=%TEMP%\TowerDefenseTest\Artifacts"

if not exist "%APPDATA%" mkdir "%APPDATA%"
if not exist "%LOCALAPPDATA%" mkdir "%LOCALAPPDATA%"
if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"

dotnet run --project "%ROOT%src\TowerDefense\TowerDefense.csproj" -c Release --artifacts-path "%ARTIFACTS%"

if errorlevel 1 (
    echo.
    echo TowerDefense test run failed.
    pause
)
