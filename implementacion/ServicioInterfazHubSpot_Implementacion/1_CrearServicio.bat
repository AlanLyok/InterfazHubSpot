@echo off
setlocal

set SERVICE_NAME=MastersoftInterfazHubSpot
set EXE_NAME=MSScheduler452Service.exe

set SCRIPT_DIR=%~dp0
set EXE_PATH=%SCRIPT_DIR%%EXE_NAME%

echo.
echo =====================================
echo Crear Servicio
echo =====================================

if not exist "%EXE_PATH%" (
    echo ERROR: No existe:
    echo %EXE_PATH%
    pause
    exit /b 1
)

sc query "%SERVICE_NAME%" >nul 2>&1

if %ERRORLEVEL% EQU 0 (
    echo El servicio ya existe.
    sc query "%SERVICE_NAME%"
    pause
    exit /b 0
)

sc create "%SERVICE_NAME%" binPath= "\"%EXE_PATH%\"" start= auto

if %ERRORLEVEL% NEQ 0 (
    echo ERROR al crear el servicio.
    pause
    exit /b 1
)

echo.
echo Servicio creado correctamente.
sc query "%SERVICE_NAME%"

pause