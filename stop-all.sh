#!/bin/bash
# DSAGrind - Stop All Services Script (Linux/macOS)

echo "🛑 Stopping DSAGrind Application..."
echo "=================================="

# Function to print colored output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Stop backend services using PIDs if available
if [ -f "backend/pids.txt" ]; then
    print_status "Stopping backend services using stored PIDs..."
    while read -r pid; do
        if [ ! -z "$pid" ] && kill -0 "$pid" 2>/dev/null; then
            kill "$pid" 2>/dev/null
            print_status "Stopped process $pid"
        fi
    done < backend/pids.txt
    rm -f backend/pids.txt
    print_success "Backend services stopped using PIDs"
else
    print_status "PID file not found, using process name matching..."
fi

# Stop frontend using PID if available
if [ -f "client/vite.pid" ]; then
    print_status "Stopping frontend using stored PID..."
    FRONTEND_PID=$(cat client/vite.pid)
    if kill -0 "$FRONTEND_PID" 2>/dev/null; then
        kill "$FRONTEND_PID" 2>/dev/null
        print_status "Stopped frontend process $FRONTEND_PID"
    fi
    rm -f client/vite.pid
fi

# Forcefully stop any remaining processes
print_status "Stopping any remaining DSAGrind processes..."

# Stop backend services by process name
pkill -f "dotnet.*DSAGrind" && print_status "Stopped .NET services" || true

# Stop frontend processes
pkill -f "vite.*client" && print_status "Stopped Vite dev server" || true
pkill -f "npm.*dev" && print_status "Stopped npm dev processes" || true

# Wait a moment for processes to terminate
sleep 2

# Check for any remaining processes
remaining_dotnet=$(pgrep -f "dotnet.*DSAGrind" | wc -l)
remaining_vite=$(pgrep -f "vite.*client" | wc -l)
remaining_npm=$(pgrep -f "npm.*dev" | wc -l)

if [ "$remaining_dotnet" -gt 0 ] || [ "$remaining_vite" -gt 0 ] || [ "$remaining_npm" -gt 0 ]; then
    print_warning "Some processes may still be running. Force killing..."
    pkill -9 -f "dotnet.*DSAGrind" 2>/dev/null || true
    pkill -9 -f "vite.*client" 2>/dev/null || true
    pkill -9 -f "npm.*dev" 2>/dev/null || true
fi

# Clean up temporary files
print_status "Cleaning up temporary files..."
rm -f backend/pids.txt
rm -f client/vite.pid
rm -f logs/*.tmp

print_success "✅ All DSAGrind services have been stopped!"

# Optional: Show port status
print_status "Checking port status..."
ports=(3000 5000 5001 5002 5003 5004 5005 5006 8080)
for port in "${ports[@]}"; do
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        print_warning "Port $port is still in use"
    fi
done

echo ""
print_success "DSAGrind application shutdown complete!"
echo ""