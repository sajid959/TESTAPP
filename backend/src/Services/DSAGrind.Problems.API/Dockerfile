# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["src/Shared/DSAGrind.Models/DSAGrind.Models.csproj", "src/Shared/DSAGrind.Models/"]
COPY ["src/Shared/DSAGrind.Common/DSAGrind.Common.csproj", "src/Shared/DSAGrind.Common/"]
COPY ["src/Shared/DSAGrind.Events/DSAGrind.Events.csproj", "src/Shared/DSAGrind.Events/"]

# Copy Problems API project file
COPY ["src/Services/DSAGrind.Problems.API/DSAGrind.Problems.API.csproj", "src/Services/DSAGrind.Problems.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/DSAGrind.Problems.API/DSAGrind.Problems.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Services/DSAGrind.Problems.API"
RUN dotnet build "DSAGrind.Problems.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DSAGrind.Problems.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001

# Run the application
ENTRYPOINT ["dotnet", "DSAGrind.Problems.API.dll"]