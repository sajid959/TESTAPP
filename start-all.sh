#!/bin/bash
# DSAGrind - Complete Application Startup Script (Linux/macOS)
# Starts both backend microservices and frontend React application

set -e

echo "🚀 Starting DSAGrind Complete Application..."
echo "============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if a port is available
check_port() {
    local port=$1
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null; then
        return 1  # Port is in use
    else
        return 0  # Port is available
    fi
}

# Function to wait for service to be ready
wait_for_service() {
    local name=$1
    local port=$2
    local max_attempts=30
    local attempt=1
    
    print_status "Waiting for $name to be ready on port $port..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -f -s "http://localhost:$port/health" > /dev/null 2>&1 || \
           curl -f -s "http://localhost:$port/api/health" > /dev/null 2>&1 || \
           curl -f -s "http://localhost:$port/" > /dev/null 2>&1; then
            print_success "$name is ready!"
            return 0
        fi
        
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    print_warning "$name didn't respond after $((max_attempts * 2)) seconds"
    return 1
}

# Cleanup function
cleanup() {
    print_status "Stopping all services..."
    
    # Kill backend services
    pkill -f "dotnet.*DSAGrind" || true
    
    # Kill frontend
    pkill -f "vite.*client" || true
    pkill -f "npm.*dev" || true
    
    # Remove PID files
    rm -f backend/pids.txt
    rm -f client/vite.pid
    
    print_success "All services stopped"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Check prerequisites
print_status "Checking prerequisites..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    print_error ".NET 8 SDK is required but not installed"
    exit 1
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    print_error "Node.js is required but not installed"
    exit 1
fi

# Check npm
if ! command -v npm &> /dev/null; then
    print_error "npm is required but not installed"
    exit 1
fi

print_success "All prerequisites are available"

# Check ports
print_status "Checking port availability..."
ports=(3000 5000 5001 5002 5003 5004 5005 5006 8080)
for port in "${ports[@]}"; do
    if ! check_port $port; then
        print_error "Port $port is already in use. Please stop the service using this port."
        exit 1
    fi
done
print_success "All required ports are available"

# Create logs directory
mkdir -p backend/logs
mkdir -p logs

# Install frontend dependencies if needed
if [ ! -d "client/node_modules" ]; then
    print_status "Installing frontend dependencies..."
    cd client
    npm install
    cd ..
    print_success "Frontend dependencies installed"
fi

# Build backend services
print_status "Building backend services..."
cd backend
dotnet build --configuration Release --no-restore
if [ $? -ne 0 ]; then
    print_error "Backend build failed"
    exit 1
fi
cd ..
print_success "Backend services built successfully"

# Start backend services
print_status "Starting backend microservices..."

# Start Gateway API (Port 5000)
print_status "Starting Gateway API on port 5000..."
cd backend/src/Services/DSAGrind.Gateway.API
nohup dotnet run --urls="http://0.0.0.0:5000" > ../../logs/gateway.log 2>&1 &
GATEWAY_PID=$!
cd ../../..

# Start Auth API (Port 8080)
print_status "Starting Auth API on port 8080..."
cd backend/src/Services/DSAGrind.Auth.API
nohup dotnet run --urls="http://0.0.0.0:8080" > ../../logs/auth.log 2>&1 &
AUTH_PID=$!
cd ../../..

# Start Problems API (Port 5001)
print_status "Starting Problems API on port 5001..."
cd backend/src/Services/DSAGrind.Problems.API
nohup dotnet run --urls="http://0.0.0.0:5001" > ../../logs/problems.log 2>&1 &
PROBLEMS_PID=$!
cd ../../..

# Start Submissions API (Port 5002)
print_status "Starting Submissions API on port 5002..."
cd backend/src/Services/DSAGrind.Submissions.API
nohup dotnet run --urls="http://0.0.0.0:5002" > ../../logs/submissions.log 2>&1 &
SUBMISSIONS_PID=$!
cd ../../..

# Start AI API (Port 5003)
print_status "Starting AI API on port 5003..."
cd backend/src/Services/DSAGrind.AI.API
nohup dotnet run --urls="http://0.0.0.0:5003" > ../../logs/ai.log 2>&1 &
AI_PID=$!
cd ../../..

# Start Search API (Port 5004)
print_status "Starting Search API on port 5004..."
cd backend/src/Services/DSAGrind.Search.API
nohup dotnet run --urls="http://0.0.0.0:5004" > ../../logs/search.log 2>&1 &
SEARCH_PID=$!
cd ../../..

# Start Admin API (Port 5005)
print_status "Starting Admin API on port 5005..."
cd backend/src/Services/DSAGrind.Admin.API
nohup dotnet run --urls="http://0.0.0.0:5005" > ../../logs/admin.log 2>&1 &
ADMIN_PID=$!
cd ../../..

# Start Payments API (Port 5006)
print_status "Starting Payments API on port 5006..."
cd backend/src/Services/DSAGrind.Payments.API
nohup dotnet run --urls="http://0.0.0.0:5006" > ../../logs/payments.log 2>&1 &
PAYMENTS_PID=$!
cd ../../..

# Store PIDs
echo "$GATEWAY_PID $AUTH_PID $PROBLEMS_PID $SUBMISSIONS_PID $AI_PID $SEARCH_PID $ADMIN_PID $PAYMENTS_PID" > backend/pids.txt

# Wait for backend services to start
print_status "Waiting for backend services to initialize..."
sleep 10

# Check backend services health
print_status "Performing health checks on backend services..."
services=(
    "Gateway:5000"
    "Auth:8080"
    "Problems:5001"
    "Submissions:5002"
    "AI:5003"
    "Search:5004"
    "Admin:5005"
    "Payments:5006"
)

backend_ready=true
for service in "${services[@]}"; do
    name=$(echo $service | cut -d: -f1)
    port=$(echo $service | cut -d: -f2)
    
    if wait_for_service "$name" "$port"; then
        print_success "$name API is ready"
    else
        print_warning "$name API is not responding (will continue anyway)"
        backend_ready=false
    fi
done

# Start frontend
print_status "Starting React frontend on port 3000..."
cd client
nohup npm run dev > ../logs/frontend.log 2>&1 &
FRONTEND_PID=$!
echo $FRONTEND_PID > vite.pid
cd ..

# Wait for frontend to start
wait_for_service "Frontend" "3000"

print_success "🎉 DSAGrind application started successfully!"
echo ""
echo "📚 Service URLs:"
echo "   🌐 Frontend:        http://localhost:3000"
echo "   🔧 Gateway API:     http://localhost:5000"
echo "   🔐 Auth API:        http://localhost:8080"
echo "   📝 Problems API:    http://localhost:5001"
echo "   ⚡ Submissions API:  http://localhost:5002"
echo "   🤖 AI API:          http://localhost:5003"
echo "   🔍 Search API:      http://localhost:5004"
echo "   👑 Admin API:       http://localhost:5005"
echo "   💳 Payments API:    http://localhost:5006"
echo ""
echo "📖 Additional Resources:"
echo "   📋 API Documentation: http://localhost:5000/swagger"
echo "   📊 Admin Dashboard:   http://localhost:3000/admin"
echo "   📝 Logs Directory:    ./logs/"
echo ""
echo "🛑 To stop all services: Press Ctrl+C or run ./stop-all.sh"
echo ""

if [ "$backend_ready" = true ]; then
    print_success "All services are running and healthy!"
else
    print_warning "Some services may not be fully ready. Check logs in ./logs/ for details."
fi

# Keep script running and monitor services
print_status "Monitoring services... Press Ctrl+C to stop all services"

while true; do
    sleep 30
    
    # Check if main processes are still running
    if ! kill -0 $GATEWAY_PID 2>/dev/null; then
        print_error "Gateway API stopped unexpectedly!"
        cleanup
    fi
    
    if ! kill -0 $FRONTEND_PID 2>/dev/null; then
        print_error "Frontend stopped unexpectedly!"
        cleanup
    fi
done