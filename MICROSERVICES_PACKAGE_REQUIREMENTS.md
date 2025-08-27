# DSAGrind Microservices Package Requirements

## Overview
This document provides a comprehensive list of packages required for each microservice and shared library in the DSAGrind platform after the migration from Kafka to RabbitMQ.

## Shared Libraries

### DSAGrind.Common
**Core infrastructure and shared services**
```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="8.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.MongoDb" Version="7.0.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.3.1" />
<PackageReference Include="Qdrant.Client" Version="1.15.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.3.1" />
<PackageReference Include="MongoDB.Bson" Version="3.0.0" />
<PackageReference Include="MongoDB.Driver" Version="3.0.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

### DSAGrind.Models
**Domain models and DTOs**
```xml
<PackageReference Include="MongoDB.Bson" Version="3.0.0" />
<PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
```

### DSAGrind.Events
**Event definitions for RabbitMQ messaging**
```xml
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

## Microservices

### DSAGrind.Gateway.API
**API Gateway using YARP reverse proxy**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="YARP.ReverseProxy" Version="2.1.0" />
```

### DSAGrind.Auth.API
**Authentication and authorization service**
```xml
<PackageReference Include="AspNetCore.HealthChecks.MongoDb" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="MongoDB.Driver" Version="3.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
```

### DSAGrind.Problems.API
**Problem management service**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="MongoDB.Bson" Version="3.0.0" />
<PackageReference Include="MongoDB.Driver" Version="3.0.0" />
```

### DSAGrind.Submissions.API
**Code submission and execution service**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Docker.DotNet" Version="3.125.15" />
```

### DSAGrind.Payments.API
**Payment processing with Stripe integration**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Stripe.net" Version="43.0.0" />
```

### DSAGrind.Search.API
**Vector search using Qdrant**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Qdrant.Client" Version="1.15.1" />
```

### DSAGrind.AI.API
**AI integration with OpenAI and Semantic Kernel**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.0.1" />
<PackageReference Include="OpenAI" Version="1.11.0" />
```

### DSAGrind.Admin.API
**Administrative management service**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="MongoDB.Bson" Version="3.0.0" />
<PackageReference Include="MongoDB.Driver" Version="3.0.0" />
```

## Frontend (React TypeScript)

### Client Dependencies
```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "wouter": "^3.0.0",
    "@tanstack/react-query": "^5.17.0",
    "@radix-ui/react-*": "Latest compatible versions",
    "class-variance-authority": "^0.7.0",
    "clsx": "^2.0.0",
    "tailwind-merge": "^2.2.0",
    "tailwindcss-animate": "^1.0.7",
    "lucide-react": "^0.307.0",
    "embla-carousel-react": "^8.0.0",
    "embla-carousel-autoplay": "^8.0.0",
    "vaul": "^0.8.0",
    "cmdk": "^0.2.0",
    "input-otp": "^1.2.4",
    "next-themes": "^0.2.1",
    "recharts": "^2.10.3",
    "monaco-editor": "^0.45.0",
    "@monaco-editor/react": "^4.6.0",
    "axios": "^1.6.5",
    "react-hook-form": "^7.48.2",
    "@hookform/resolvers": "^3.3.2",
    "zod": "^3.22.4",
    "date-fns": "^3.2.0",
    "react-day-picker": "^8.10.0",
    "sonner": "^1.3.1"
  },
  "devDependencies": {
    "@types/react": "^18.2.43",
    "@types/react-dom": "^18.2.17",
    "@typescript-eslint/eslint-plugin": "^6.14.0",
    "@typescript-eslint/parser": "^6.14.0",
    "@vitejs/plugin-react": "^4.2.1",
    "typescript": "^5.2.2",
    "vite": "^5.0.8",
    "eslint": "^8.55.0",
    "eslint-plugin-react-hooks": "^4.6.0",
    "eslint-plugin-react-refresh": "^0.4.5",
    "tailwindcss": "^3.4.0",
    "postcss": "^8.4.32",
    "autoprefixer": "^10.4.16",
    "vitest": "^1.1.0",
    "@vitest/ui": "^1.1.0"
  }
}
```

## Database and Infrastructure

### MongoDB Atlas
- **Primary Database**: Document storage with flexible schema
- **Connection**: MongoDB.Driver Version 3.0.0
- **Health Checks**: AspNetCore.HealthChecks.MongoDb

### RabbitMQ (CloudAMQP)
- **Message Broker**: Event-driven communication
- **Connection**: RabbitMQ.Client Version 6.8.1
- **Configuration**: CloudAMQP connection string

### Redis Cloud
- **Caching**: Session management and query caching
- **Connection**: StackExchange.Redis Version 2.7.10
- **Health Checks**: AspNetCore.HealthChecks.Redis

### Qdrant
- **Vector Database**: Semantic search capabilities
- **Connection**: Qdrant.Client Version 1.15.1
- **Usage**: DSAGrind.Search.API and DSAGrind.Common

## External Services

### Stripe
- **Payment Processing**: Subscription management
- **Package**: Stripe.net Version 43.0.0
- **Service**: DSAGrind.Payments.API

### OpenAI
- **AI Integration**: Code analysis and hints
- **Package**: OpenAI Version 1.11.0
- **Service**: DSAGrind.AI.API

### Microsoft Semantic Kernel
- **AI Orchestration**: Advanced AI capabilities
- **Package**: Microsoft.SemanticKernel Version 1.0.1
- **Service**: DSAGrind.AI.API

### Docker
- **Code Execution**: Sandboxed environment
- **Package**: Docker.DotNet Version 3.125.15
- **Service**: DSAGrind.Submissions.API

## Installation Commands

### Backend (.NET 8)
```bash
# Install all packages for each microservice
dotnet restore backend/src/Shared/DSAGrind.Common/DSAGrind.Common.csproj
dotnet restore backend/src/Shared/DSAGrind.Models/DSAGrind.Models.csproj
dotnet restore backend/src/Shared/DSAGrind.Events/DSAGrind.Events.csproj
dotnet restore backend/src/Services/DSAGrind.Gateway.API/DSAGrind.Gateway.API.csproj
dotnet restore backend/src/Services/DSAGrind.Auth.API/DSAGrind.Auth.API.csproj
dotnet restore backend/src/Services/DSAGrind.Problems.API/DSAGrind.Problems.API.csproj
dotnet restore backend/src/Services/DSAGrind.Submissions.API/DSAGrind.Submissions.API.csproj
dotnet restore backend/src/Services/DSAGrind.Payments.API/DSAGrind.Payments.API.csproj
dotnet restore backend/src/Services/DSAGrind.Search.API/DSAGrind.Search.API.csproj
dotnet restore backend/src/Services/DSAGrind.AI.API/DSAGrind.AI.API.csproj
dotnet restore backend/src/Services/DSAGrind.Admin.API/DSAGrind.Admin.API.csproj
```

### Frontend (Node.js 20)
```bash
# Install root dependencies
npm install

# Install client dependencies  
cd client && npm install
```

## Migration Notes

### Kafka to RabbitMQ Migration
- **Original Kafka implementation**: Preserved as commented code
- **New RabbitMQ implementation**: Drop-in replacement maintaining same interfaces
- **Topics/Queues**: Same topic names maintained for compatibility
- **Event Types**: All existing event types preserved
- **Configuration**: New RabbitMQSettings replaces KafkaSettings

### Preserved Technologies
- **MongoDB**: Primary database unchanged
- **Redis**: Caching layer unchanged  
- **Qdrant**: Vector search capabilities unchanged
- **All microservice functionality**: Preserved and unmodified
- **All external integrations**: Stripe, OpenAI, Docker execution unchanged
- **Frontend stack**: React, TypeScript, Vite unchanged

This migration ensures zero functional changes while moving from Kafka to RabbitMQ for improved cloud compatibility and simplified deployment.