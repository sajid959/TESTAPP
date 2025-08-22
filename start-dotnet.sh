#!/bin/bash

# Start DSAGrind .NET Microservices
echo "🚀 Starting DSAGrind .NET + MongoDB Architecture..."

# Navigate to backend directory
cd backend

# Build all services
echo "📦 Building .NET microservices..."
dotnet build --configuration Release

# Start the Gateway API (main entry point)
echo "🌐 Starting Gateway API on port 8000..."
export ASPNETCORE_URLS="http://0.0.0.0:8000"
export ASPNETCORE_ENVIRONMENT="Development"

# Run the Gateway API
dotnet run --project src/Services/DSAGrind.Gateway.API/DSAGrind.Gateway.API.csproj --configuration Release