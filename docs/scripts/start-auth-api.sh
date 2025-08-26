#!/bin/bash

echo "🚀 Starting DSAGrind Auth API..."

cd backend/src/Services/DSAGrind.Auth.API

echo "Installing dependencies..."
dotnet restore

echo "Starting Auth API on port 8080..."
dotnet run --urls="http://0.0.0.0:8080"