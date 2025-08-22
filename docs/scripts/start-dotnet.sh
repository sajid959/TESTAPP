#!/bin/bash

echo "🚀 Starting DSAGrind .NET Services..."

cd backend

echo "Starting Gateway API (Port 5000)..."
gnome-terminal --tab --title="Gateway API" -- bash -c "cd src/Services/DSAGrind.Gateway.API && dotnet run --urls=http://0.0.0.0:5000; exec bash"

sleep 3

echo "Starting Auth API (Port 8080)..."
gnome-terminal --tab --title="Auth API" -- bash -c "cd src/Services/DSAGrind.Auth.API && dotnet run --urls=http://0.0.0.0:8080; exec bash"

sleep 2

echo "Starting Problems API (Port 5001)..."
gnome-terminal --tab --title="Problems API" -- bash -c "cd src/Services/DSAGrind.Problems.API && dotnet run --urls=http://0.0.0.0:5001; exec bash"

sleep 2

echo "Starting Submissions API (Port 5002)..."
gnome-terminal --tab --title="Submissions API" -- bash -c "cd src/Services/DSAGrind.Submissions.API && dotnet run --urls=http://0.0.0.0:5002; exec bash"

sleep 2

echo "Starting AI API (Port 5003)..."
gnome-terminal --tab --title="AI API" -- bash -c "cd src/Services/DSAGrind.AI.API && dotnet run --urls=http://0.0.0.0:5003; exec bash"

sleep 2

echo "Starting Search API (Port 5004)..."
gnome-terminal --tab --title="Search API" -- bash -c "cd src/Services/DSAGrind.Search.API && dotnet run --urls=http://0.0.0.0:5004; exec bash"

sleep 2

echo "Starting Admin API (Port 5005)..."
gnome-terminal --tab --title="Admin API" -- bash -c "cd src/Services/DSAGrind.Admin.API && dotnet run --urls=http://0.0.0.0:5005; exec bash"

sleep 2

echo "Starting Payments API (Port 5006)..."
gnome-terminal --tab --title="Payments API" -- bash -c "cd src/Services/DSAGrind.Payments.API && dotnet run --urls=http://0.0.0.0:5006; exec bash"

echo "✅ All .NET services are starting..."
echo "Gateway API: http://localhost:5000"
echo "Auth API: http://localhost:8080"
echo "Problems API: http://localhost:5001"
echo "Submissions API: http://localhost:5002"
echo "AI API: http://localhost:5003"
echo "Search API: http://localhost:5004"
echo "Admin API: http://localhost:5005"
echo "Payments API: http://localhost:5006"