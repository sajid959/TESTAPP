# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["src/Shared/DSAGrind.Models/DSAGrind.Models.csproj", "src/Shared/DSAGrind.Models/"]
COPY ["src/Shared/DSAGrind.Common/DSAGrind.Common.csproj", "src/Shared/DSAGrind.Common/"]
COPY ["src/Shared/DSAGrind.Events/DSAGrind.Events.csproj", "src/Shared/DSAGrind.Events/"]

# Copy Admin API project file
COPY ["src/Services/DSAGrind.Admin.API/DSAGrind.Admin.API.csproj", "src/Services/DSAGrind.Admin.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/DSAGrind.Admin.API/DSAGrind.Admin.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Services/DSAGrind.Admin.API"
RUN dotnet build "DSAGrind.Admin.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DSAGrind.Admin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5005

# Run the application
ENTRYPOINT ["dotnet", "DSAGrind.Admin.API.dll"]