@echo off
setlocal

set SERVICE_NAME=MastersoftInterfazHubSpot

echo.
echo =====================================
echo Iniciar Servicio
echo =====================================

sc query "%SERVICE_NAME%" >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: El servicio no existe.
    pause
    exit /b 1
)

net start "%SERVICE_NAME%"

echo.
echo Estado actual:
sc query "%SERVICE_NAME%"

sc query "%SERVICE_NAME%" | find "RUNNING" >nul

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SERVICIO INICIADO CORRECTAMENTE.
) else (
    echo.
    echo ADVERTENCIA: El servicio no quedo en RUNNING.
)

pause