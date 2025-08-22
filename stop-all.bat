@echo off
:: DSAGrind - Stop All Services Script (Windows)

echo.
echo 🛑 Stopping DSAGrind Application...
echo ==================================

echo [INFO] Stopping all DSAGrind services...

:: Stop .NET processes
echo [INFO] Stopping backend microservices...
taskkill /f /im dotnet.exe 2>nul
if %errorlevel% equ 0 (
    echo [SUCCESS] Backend services stopped
) else (
    echo [WARNING] No backend services were running or failed to stop
)

:: Stop Node.js processes (frontend)
echo [INFO] Stopping frontend services...
taskkill /f /im node.exe 2>nul
if %errorlevel% equ 0 (
    echo [SUCCESS] Frontend services stopped
) else (
    echo [WARNING] No frontend services were running or failed to stop
)

:: Stop npm processes
echo [INFO] Stopping npm processes...
taskkill /f /im npm.cmd 2>nul
taskkill /f /im npm.exe 2>nul

:: Clean up temporary files
echo [INFO] Cleaning up temporary files...
if exist "backend\pids.txt" del "backend\pids.txt"
if exist "client\vite.pid" del "client\vite.pid"

echo.
echo [SUCCESS] ✅ All DSAGrind services have been stopped!
echo.
echo [INFO] All application windows should now be closed.
echo [INFO] You can now restart the application using start-all.bat
echo.
pause