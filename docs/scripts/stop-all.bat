@echo off
echo Stopping DSAGrind Platform...

echo Killing Node.js processes...
taskkill /F /IM node.exe >nul 2>&1

echo Killing .NET processes...
taskkill /F /IM dotnet.exe >nul 2>&1

echo ✅ All services stopped.
pause