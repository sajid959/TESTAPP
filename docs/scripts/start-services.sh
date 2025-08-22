#!/bin/bash

echo "🚀 Starting DSAGrind Services..."

# Function to check if a port is available
check_port() {
    netstat -tuln | grep ":$1 " > /dev/null
    return $?
}

# Start Backend Services
echo "Starting Backend Services..."

cd backend/src/Services

# Start Gateway API (Port 5000)
if ! check_port 5000; then
    echo "Starting Gateway API on port 5000..."
    cd DSAGrind.Gateway.API
    dotnet run --urls="http://0.0.0.0:5000" &
    cd ..
    sleep 3
else
    echo "Port 5000 is already in use"
fi

# Start Auth API (Port 8080)
if ! check_port 8080; then
    echo "Starting Auth API on port 8080..."
    cd DSAGrind.Auth.API
    dotnet run --urls="http://0.0.0.0:8080" &
    cd ..
    sleep 2
else
    echo "Port 8080 is already in use"
fi

# Start Problems API (Port 5001)
if ! check_port 5001; then
    echo "Starting Problems API on port 5001..."
    cd DSAGrind.Problems.API
    dotnet run --urls="http://0.0.0.0:5001" &
    cd ..
    sleep 2
else
    echo "Port 5001 is already in use"
fi

# Start Submissions API (Port 5002)
if ! check_port 5002; then
    echo "Starting Submissions API on port 5002..."
    cd DSAGrind.Submissions.API
    dotnet run --urls="http://0.0.0.0:5002" &
    cd ..
    sleep 2
else
    echo "Port 5002 is already in use"
fi

# Start AI API (Port 5003)
if ! check_port 5003; then
    echo "Starting AI API on port 5003..."
    cd DSAGrind.AI.API
    dotnet run --urls="http://0.0.0.0:5003" &
    cd ..
    sleep 2
else
    echo "Port 5003 is already in use"
fi

# Start Search API (Port 5004)
if ! check_port 5004; then
    echo "Starting Search API on port 5004..."
    cd DSAGrind.Search.API
    dotnet run --urls="http://0.0.0.0:5004" &
    cd ..
    sleep 2
else
    echo "Port 5004 is already in use"
fi

# Start Admin API (Port 5005)
if ! check_port 5005; then
    echo "Starting Admin API on port 5005..."
    cd DSAGrind.Admin.API
    dotnet run --urls="http://0.0.0.0:5005" &
    cd ..
    sleep 2
else
    echo "Port 5005 is already in use"
fi

# Start Payments API (Port 5006)
if ! check_port 5006; then
    echo "Starting Payments API on port 5006..."
    cd DSAGrind.Payments.API
    dotnet run --urls="http://0.0.0.0:5006" &
    cd ..
    sleep 2
else
    echo "Port 5006 is already in use"
fi

cd ../../..

# Start Frontend
echo "Starting Frontend..."
cd client
if ! check_port 3000; then
    echo "Starting React frontend on port 3000..."
    npm run dev &
    sleep 3
else
    echo "Port 3000 is already in use"
fi

cd ..

echo "✅ All services are running!"
echo ""
echo "🌐 Application URLs:"
echo "Frontend: http://localhost:3000"
echo "Gateway: http://localhost:5000"
echo "Auth API: http://localhost:8080"
echo "Problems API: http://localhost:5001"
echo "Submissions API: http://localhost:5002"
echo "AI API: http://localhost:5003"
echo "Search API: http://localhost:5004"
echo "Admin API: http://localhost:5005"
echo "Payments API: http://localhost:5006"
echo ""
echo "Press Ctrl+C to stop all services"

# Keep the script running
wait