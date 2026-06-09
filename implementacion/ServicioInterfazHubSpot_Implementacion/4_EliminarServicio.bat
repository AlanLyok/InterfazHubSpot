@echo off
setlocal

set SERVICE_NAME=MastersoftInterfazHubSpot

echo.
echo =====================================
echo Eliminar Servicio
echo =====================================

sc query "%SERVICE_NAME%" >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo El servicio no existe.
    pause
    exit /b 0
)

net stop "%SERVICE_NAME%" >nul 2>&1

sc delete "%SERVICE_NAME%"

echo.
echo Servicio eliminado.

pause