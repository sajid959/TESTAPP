#!/bin/bash

# DSAGrind Microservices Startup Script
echo "🚀 Starting DSAGrind .NET Microservices..."

# Kill any existing processes on our ports
echo "🧹 Cleaning up existing processes..."
pkill -f "dotnet.*DSAGrind" || true

# Build all services
echo "🔨 Building all .NET services..."
cd backend
dotnet build --configuration Release

# Start all services in background
echo "🌟 Starting Gateway API (Port 5000)..."
cd src/Services/DSAGrind.Gateway.API
nohup dotnet run --urls="http://0.0.0.0:5000" > ../../../logs/gateway.log 2>&1 &
GATEWAY_PID=$!

echo "🔐 Starting Auth API (Port 8080)..."
cd ../DSAGrind.Auth.API
nohup dotnet run --urls="http://0.0.0.0:8080" > ../../../logs/auth.log 2>&1 &
AUTH_PID=$!

echo "📝 Starting Problems API (Port 5001)..."
cd ../DSAGrind.Problems.API
nohup dotnet run --urls="http://0.0.0.0:5001" > ../../../logs/problems.log 2>&1 &
PROBLEMS_PID=$!

echo "⚡ Starting Submissions API (Port 5002)..."
cd ../DSAGrind.Submissions.API
nohup dotnet run --urls="http://0.0.0.0:5002" > ../../../logs/submissions.log 2>&1 &
SUBMISSIONS_PID=$!

echo "🤖 Starting AI API (Port 5003)..."
cd ../DSAGrind.AI.API
nohup dotnet run --urls="http://0.0.0.0:5003" > ../../../logs/ai.log 2>&1 &
AI_PID=$!

echo "🔍 Starting Search API (Port 5004)..."
cd ../DSAGrind.Search.API
nohup dotnet run --urls="http://0.0.0.0:5004" > ../../../logs/search.log 2>&1 &
SEARCH_PID=$!

echo "👥 Starting Admin API (Port 5005)..."
cd ../DSAGrind.Admin.API
nohup dotnet run --urls="http://0.0.0.0:5005" > ../../../logs/admin.log 2>&1 &
ADMIN_PID=$!

echo "💳 Starting Payments API (Port 5006)..."
cd ../DSAGrind.Payments.API
nohup dotnet run --urls="http://0.0.0.0:5006" > ../../../logs/payments.log 2>&1 &
PAYMENTS_PID=$!

# Wait for services to start
echo "⏳ Waiting for services to start..."
sleep 10

# Health check all services
echo "🏥 Performing health checks..."
services=(
    "Gateway:http://localhost:5000/api/health"
    "Auth:http://localhost:8080/api/health"
    "Problems:http://localhost:5001/api/health"
    "Submissions:http://localhost:5002/api/health"
    "AI:http://localhost:5003/api/health"
    "Search:http://localhost:5004/api/health"
    "Admin:http://localhost:5005/api/health"
    "Payments:http://localhost:5006/api/health"
)

for service in "${services[@]}"; do
    name=$(echo $service | cut -d: -f1)
    url=$(echo $service | cut -d: -f2-3)
    
    if curl -f -s "$url" > /dev/null 2>&1; then
        echo "✅ $name API is healthy"
    else
        echo "❌ $name API is not responding"
    fi
done

echo ""
echo "🎉 All DSAGrind microservices started successfully!"
echo ""
echo "📚 Service URLs:"
echo "   Gateway API:    http://localhost:5000"
echo "   Auth API:       http://localhost:8080"
echo "   Problems API:   http://localhost:5001"
echo "   Submissions API: http://localhost:5002"
echo "   AI API:         http://localhost:5003"
echo "   Search API:     http://localhost:5004"
echo "   Admin API:      http://localhost:5005"
echo "   Payments API:   http://localhost:5006"
echo ""
echo "📝 Logs are available in backend/logs/"
echo "🔍 API Documentation: http://localhost:5000/swagger"
echo ""

# Store PIDs for later cleanup
cd ../../..
mkdir -p backend/logs
echo "$GATEWAY_PID $AUTH_PID $PROBLEMS_PID $SUBMISSIONS_PID $AI_PID $SEARCH_PID $ADMIN_PID $PAYMENTS_PID" > backend/pids.txt

# Keep the script running so services stay alive
echo "🔄 Services running... Press Ctrl+C to stop all services"
trap 'echo "🛑 Stopping all services..."; kill $(cat backend/pids.txt) 2>/dev/null; rm -f backend/pids.txt; exit 0' INT TERM

# Wait indefinitely
while true; do
    sleep 1
done