#!/bin/bash

# DSA Grind - Start All Microservices
echo "🚀 Starting DSA Grind Microservices..."

# Check if MongoDB connection string is set
if [ -z "$MONGODB_CONNECTION_STRING" ]; then
    echo "⚠️  Warning: MONGODB_CONNECTION_STRING environment variable not set"
    echo "Please set it in your .env file or environment"
fi

# Function to start a service in background
start_service() {
    local service_name=$1
    local port=$2
    local project_path=$3
    
    echo "🌱 Starting $service_name on port $port..."
    cd "$project_path"
    dotnet run --urls="http://0.0.0.0:$port" &
    echo $! > "/tmp/dsagrind_${service_name}.pid"
    cd - > /dev/null
}

# Kill any existing processes
echo "🧹 Cleaning up existing processes..."
pkill -f "dotnet.*DSAGrind" || true
rm -f /tmp/dsagrind_*.pid

# Start all services
start_service "gateway" 5000 "src/Services/DSAGrind.Gateway.API"
sleep 2

start_service "auth" 8080 "src/Services/DSAGrind.Auth.API"
sleep 2

start_service "problems" 5001 "src/Services/DSAGrind.Problems.API"
sleep 2

start_service "submissions" 5002 "src/Services/DSAGrind.Submissions.API"
sleep 2

start_service "ai" 5003 "src/Services/DSAGrind.AI.API"
sleep 2

start_service "search" 5004 "src/Services/DSAGrind.Search.API"
sleep 2

start_service "admin" 5005 "src/Services/DSAGrind.Admin.API"
sleep 2

start_service "payments" 5006 "src/Services/DSAGrind.Payments.API"

echo ""
echo "✅ All services started!"
echo ""
echo "📊 Service Status:"
echo "🌐 Gateway API:    http://localhost:5000"
echo "🔐 Auth API:       http://localhost:8080"
echo "📝 Problems API:   http://localhost:5001"
echo "⚡ Submissions API: http://localhost:5002"
echo "🤖 AI API:         http://localhost:5003"
echo "🔍 Search API:     http://localhost:5004"
echo "👑 Admin API:      http://localhost:5005"
echo "💳 Payments API:   http://localhost:5006"
echo ""
echo "🛑 To stop all services, run: ./stop-all-services.sh"
echo "📋 To view logs: docker logs <service_name>"
echo "🩺 Health checks: curl http://localhost:<port>/health"

# Wait for user input to keep script running
echo ""
echo "Press Ctrl+C to stop all services..."
trap 'echo "🛑 Stopping all services..."; pkill -f "dotnet.*DSAGrind"; rm -f /tmp/dsagrind_*.pid; echo "✅ All services stopped!"; exit 0' INT

# Keep script running
while true; do
    sleep 1
done