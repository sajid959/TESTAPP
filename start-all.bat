@echo off
:: DSAGrind - Complete Application Startup Script (Windows)
:: Starts both backend microservices and frontend React application

setlocal enabledelayedexpansion

echo.
echo 🚀 Starting DSAGrind Complete Application...
echo =============================================

:: Check prerequisites
echo [INFO] Checking prerequisites...

:: Check .NET
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET 8 SDK is required but not installed
    pause
    exit /b 1
)

:: Check Node.js
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Node.js is required but not installed
    pause
    exit /b 1
)

:: Check npm
npm --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] npm is required but not installed
    pause
    exit /b 1
)

echo [SUCCESS] All prerequisites are available

:: Create logs directory
if not exist "backend\logs" mkdir "backend\logs"
if not exist "logs" mkdir "logs"

:: Install frontend dependencies if needed
if not exist "client\node_modules" (
    echo [INFO] Installing frontend dependencies...
    cd client
    call npm install
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to install frontend dependencies
        pause
        exit /b 1
    )
    cd ..
    echo [SUCCESS] Frontend dependencies installed
)

:: Build backend services
echo [INFO] Building backend services...
cd backend
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo [ERROR] Backend build failed
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] Backend services built successfully

:: Start backend services
echo [INFO] Starting backend microservices...

:: Start Gateway API (Port 5000)
echo [INFO] Starting Gateway API on port 5000...
cd backend\src\Services\DSAGrind.Gateway.API
start "Gateway API" dotnet run --urls="http://0.0.0.0:5000"
cd ..\..\..

:: Start Auth API (Port 8080)
echo [INFO] Starting Auth API on port 8080...
cd backend\src\Services\DSAGrind.Auth.API
start "Auth API" dotnet run --urls="http://0.0.0.0:8080"
cd ..\..\..

:: Start Problems API (Port 5001)
echo [INFO] Starting Problems API on port 5001...
cd backend\src\Services\DSAGrind.Problems.API
start "Problems API" dotnet run --urls="http://0.0.0.0:5001"
cd ..\..\..

:: Start Submissions API (Port 5002)
echo [INFO] Starting Submissions API on port 5002...
cd backend\src\Services\DSAGrind.Submissions.API
start "Submissions API" dotnet run --urls="http://0.0.0.0:5002"
cd ..\..\..

:: Start AI API (Port 5003)
echo [INFO] Starting AI API on port 5003...
cd backend\src\Services\DSAGrind.AI.API
start "AI API" dotnet run --urls="http://0.0.0.0:5003"
cd ..\..\..

:: Start Search API (Port 5004)
echo [INFO] Starting Search API on port 5004...
cd backend\src\Services\DSAGrind.Search.API
start "Search API" dotnet run --urls="http://0.0.0.0:5004"
cd ..\..\..

:: Start Admin API (Port 5005)
echo [INFO] Starting Admin API on port 5005...
cd backend\src\Services\DSAGrind.Admin.API
start "Admin API" dotnet run --urls="http://0.0.0.0:5005"
cd ..\..\..

:: Start Payments API (Port 5006)
echo [INFO] Starting Payments API on port 5006...
cd backend\src\Services\DSAGrind.Payments.API
start "Payments API" dotnet run --urls="http://0.0.0.0:5006"
cd ..\..\..

:: Wait for backend services to initialize
echo [INFO] Waiting for backend services to initialize...
timeout /t 15 /nobreak >nul

:: Start frontend
echo [INFO] Starting React frontend on port 3000...
cd client
start "React Frontend" npm run dev
cd ..

:: Wait for frontend to start
echo [INFO] Waiting for frontend to start...
timeout /t 10 /nobreak >nul

echo.
echo [SUCCESS] 🎉 DSAGrind application started successfully!
echo.
echo 📚 Service URLs:
echo    🌐 Frontend:        http://localhost:3000
echo    🔧 Gateway API:     http://localhost:5000
echo    🔐 Auth API:        http://localhost:8080
echo    📝 Problems API:    http://localhost:5001
echo    ⚡ Submissions API:  http://localhost:5002
echo    🤖 AI API:          http://localhost:5003
echo    🔍 Search API:      http://localhost:5004
echo    👑 Admin API:       http://localhost:5005
echo    💳 Payments API:    http://localhost:5006
echo.
echo 📖 Additional Resources:
echo    📋 API Documentation: http://localhost:5000/swagger
echo    📊 Admin Dashboard:   http://localhost:3000/admin
echo    📝 Logs Directory:    .\logs\
echo.
echo 🛑 To stop all services: Run stop-all.bat or close all console windows
echo.
echo [INFO] All services are starting in separate windows.
echo [INFO] Please wait a few moments for all services to be fully ready.
echo.

:: Open the application in default browser
timeout /t 5 /nobreak >nul
start http://localhost:3000

echo [SUCCESS] Application is ready! Check the browser for the frontend.
echo.
pause