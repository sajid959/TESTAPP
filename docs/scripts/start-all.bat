@echo off
echo Starting DSAGrind Platform...

echo.
echo Starting Backend Services...
cd backend
start "Gateway API" cmd /c "cd src\Services\DSAGrind.Gateway.API && dotnet run --urls=http://0.0.0.0:5000"
timeout /t 3 >nul
start "Auth API" cmd /c "cd src\Services\DSAGrind.Auth.API && dotnet run --urls=http://0.0.0.0:8080"
timeout /t 2 >nul
start "Problems API" cmd /c "cd src\Services\DSAGrind.Problems.API && dotnet run --urls=http://0.0.0.0:5001"
timeout /t 2 >nul
start "Submissions API" cmd /c "cd src\Services\DSAGrind.Submissions.API && dotnet run --urls=http://0.0.0.0:5002"
timeout /t 2 >nul
start "AI API" cmd /c "cd src\Services\DSAGrind.AI.API && dotnet run --urls=http://0.0.0.0:5003"
timeout /t 2 >nul
start "Search API" cmd /c "cd src\Services\DSAGrind.Search.API && dotnet run --urls=http://0.0.0.0:5004"
timeout /t 2 >nul
start "Admin API" cmd /c "cd src\Services\DSAGrind.Admin.API && dotnet run --urls=http://0.0.0.0:5005"
timeout /t 2 >nul
start "Payments API" cmd /c "cd src\Services\DSAGrind.Payments.API && dotnet run --urls=http://0.0.0.0:5006"

cd ..

echo.
echo Starting Frontend...
cd client
start "Frontend" cmd /c "npm run dev"

cd ..

echo.
echo ✅ All services are starting...
echo Frontend: http://localhost:3000
echo Gateway: http://localhost:5000
echo.
pause