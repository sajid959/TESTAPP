#!/bin/bash

echo "🛑 Stopping DSAGrind Platform..."

echo "Stopping Node.js processes..."
pkill -f "npm run dev" 2>/dev/null || true
pkill -f "vite" 2>/dev/null || true

echo "Stopping .NET processes..."
pkill -f "dotnet run" 2>/dev/null || true
pkill -f "DSAGrind" 2>/dev/null || true

echo "Stopping any remaining processes on application ports..."
lsof -ti:3000 | xargs kill -9 2>/dev/null || true
lsof -ti:5000 | xargs kill -9 2>/dev/null || true
lsof -ti:5001 | xargs kill -9 2>/dev/null || true
lsof -ti:5002 | xargs kill -9 2>/dev/null || true
lsof -ti:5003 | xargs kill -9 2>/dev/null || true
lsof -ti:5004 | xargs kill -9 2>/dev/null || true
lsof -ti:5005 | xargs kill -9 2>/dev/null || true
lsof -ti:5006 | xargs kill -9 2>/dev/null || true
lsof -ti:8080 | xargs kill -9 2>/dev/null || true

echo "✅ All services stopped."