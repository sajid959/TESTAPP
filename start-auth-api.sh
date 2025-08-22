#!/bin/bash

# Change to Auth API directory
cd backend/src/Services/DSAGrind.Auth.API

# Set environment variables from our test config
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://0.0.0.0:8080"

# Run the Auth API
dotnet run --project . --urls "http://0.0.0.0:8080"