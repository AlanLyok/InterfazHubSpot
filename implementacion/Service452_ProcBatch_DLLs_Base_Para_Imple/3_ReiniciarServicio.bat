@echo off
setlocal

set SERVICE_NAME=MastersoftServicioNuevo

echo.
echo =====================================
echo Reiniciar Servicio
echo =====================================

sc query "%SERVICE_NAME%" >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: El servicio no existe.
    pause
    exit /b 1
)

net stop "%SERVICE_NAME%"

timeout /t 3 /nobreak >nul

net start "%SERVICE_NAME%"

timeout /t 2 /nobreak >nul

echo.
echo Estado actual:
sc query "%SERVICE_NAME%"

pause