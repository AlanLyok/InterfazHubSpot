@echo off
setlocal

set SERVICE_NAME=MastersoftInterfazHubSpot

echo.
echo =====================================
echo Detener Servicio
echo =====================================

sc query "%SERVICE_NAME%" >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: El servicio no existe.
    pause
    exit /b 1
)

echo.
echo Estado antes de detener:
sc query "%SERVICE_NAME%"

echo.
echo Deteniendo servicio...
sc stop "%SERVICE_NAME%"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR al enviar la orden de detencion.
    pause
    exit /b 1
)

echo.
echo Esperando que el servicio se detenga...

:WAIT_STOP
timeout /t 1 /nobreak >nul

sc query "%SERVICE_NAME%" | find "STOPPED" >nul
if %ERRORLEVEL% NEQ 0 goto WAIT_STOP

echo.
echo SERVICIO DETENIDO CORRECTAMENTE.
echo.

sc query "%SERVICE_NAME%"

pause