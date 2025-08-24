# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["src/Shared/DSAGrind.Models/DSAGrind.Models.csproj", "src/Shared/DSAGrind.Models/"]
COPY ["src/Shared/DSAGrind.Common/DSAGrind.Common.csproj", "src/Shared/DSAGrind.Common/"]
COPY ["src/Shared/DSAGrind.Events/DSAGrind.Events.csproj", "src/Shared/DSAGrind.Events/"]

# Copy Auth API project file
COPY ["src/Services/DSAGrind.Auth.API/DSAGrind.Auth.API.csproj", "src/Services/DSAGrind.Auth.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/DSAGrind.Auth.API/DSAGrind.Auth.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Services/DSAGrind.Auth.API"
RUN dotnet build "DSAGrind.Auth.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DSAGrind.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "DSAGrind.Auth.API.dll"]