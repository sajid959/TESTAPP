#!/bin/bash

# Start DSAGrind Backend Services
echo "Starting DSAGrind Backend Services..."

# Load environment variables
export $(cat .env | grep -v '^#' | xargs)

# Start Gateway API (API Gateway)
echo "Starting Gateway API on port 8000..."
cd src/Services/DSAGrind.Gateway.API
dotnet run --urls=http://0.0.0.0:8000 &
GATEWAY_PID=$!
cd ../../..

# Start Auth API
echo "Starting Auth API on port 8080..."
cd src/Services/DSAGrind.Auth.API
dotnet run --urls=http://0.0.0.0:8080 &
AUTH_PID=$!
cd ../../..

# Start Problems API
echo "Starting Problems API on port 5001..."
cd src/Services/DSAGrind.Problems.API
dotnet run --urls=http://0.0.0.0:5001 &
PROBLEMS_PID=$!
cd ../../..

# Start Admin API
echo "Starting Admin API on port 8081..."
cd src/Services/DSAGrind.Admin.API
dotnet run --urls=http://0.0.0.0:8081 &
ADMIN_PID=$!
cd ../../..

# Start AI API
echo "Starting AI API on port 8082..."
cd src/Services/DSAGrind.AI.API
dotnet run --urls=http://0.0.0.0:8082 &
AI_PID=$!
cd ../../..

echo "All services starting..."
echo "Gateway: http://localhost:8000"
echo "Auth: http://localhost:8080"
echo "Problems: http://localhost:5001"
echo "Admin: http://localhost:8081"
echo "AI: http://localhost:8082"

# Wait for services
wait