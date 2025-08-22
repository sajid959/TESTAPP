# DSAGrind Production Setup Guide

## Overview
This guide covers the complete production deployment of DSAGrind, a competitive programming platform with microservices architecture.

## Architecture Overview

### Technology Stack

#### Frontend (React + TypeScript)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite for fast development
- **Styling**: Tailwind CSS + Shadcn UI components
- **State Management**: TanStack Query for server state
- **Routing**: Wouter for client-side routing
- **Forms**: React Hook Form with Zod validation
- **Code Editor**: Monaco Editor for multi-language support
- **Authentication**: JWT with refresh token rotation

#### Backend (.NET 8 Microservices)
- **API Gateway**: YARP Reverse Proxy (Port 5000)
- **Authentication**: JWT + OAuth (Port 8080)
- **Problems**: CRUD + Categories (Port 5001)
- **Submissions**: Code execution (Port 5002)
- **AI**: Test AI service (Port 5003)
- **Search**: Vector search (Port 5004)
- **Admin**: Management dashboard (Port 5005)
- **Payments**: Stripe integration (Port 5006)

#### Infrastructure
- **Database**: MongoDB Atlas (Primary) + Redis (Cache)
- **Message Queue**: Kafka for async processing
- **Vector Search**: Qdrant for problem recommendations
- **Email**: SendGrid for transactional emails
- **Payments**: Stripe for subscription handling
- **Monitoring**: Serilog + Application Insights
- **Hosting**: Docker containers on cloud platforms

## Pre-Deployment Checklist

### 1. Environment Configuration

#### Backend (.env in root)
```bash
# Database (Development - uses local/test values)
MONGODB_CONNECTION_STRING=mongodb://localhost:27017/dsagrind_test
REDIS_CONNECTION_STRING=redis://localhost:6379

# AI Configuration (Test mode enabled)
AI_DEFAULT_PROVIDER=Test
PERPLEXITY_API_KEY=test-key-for-development

# JWT Settings
JWT_SECRET_KEY=super-secret-development-key-that-is-at-least-256-bits-long

# OAuth (Development)
GOOGLE_CLIENT_ID=test-google-client-id
GITHUB_CLIENT_ID=test-github-client-id

# Application
ASPNETCORE_ENVIRONMENT=Development
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5000
```

#### Production Environment Variables
```bash
# Database (Production - MongoDB Atlas)
MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/dsagrind_prod?retryWrites=true&w=majority
MONGODB_DATABASE_NAME=dsagrind_prod
REDIS_CONNECTION_STRING=redis://username:password@redis-prod-host:6379

# Kafka (Confluent Cloud)
KAFKA_BOOTSTRAP_SERVERS=pkc-xxxxx.region.provider.confluent.cloud:9092
KAFKA_USERNAME=kafka_username
KAFKA_PASSWORD=kafka_password
KAFKA_SECURITY_PROTOCOL=SASL_SSL
KAFKA_SASL_MECHANISM=PLAIN

# AI Services
AI_DEFAULT_PROVIDER=Perplexity
PERPLEXITY_API_KEY=pplx-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
OPENAI_API_KEY=sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Vector Database (Qdrant Cloud)
QDRANT_URL=https://xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.us-east-1-0.aws.cloud.qdrant.io:6333
QDRANT_API_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Authentication & Security
JWT_SECRET_KEY=production-secret-key-256-bits-minimum-length-required
JWT_ISSUER=DSAGrind
JWT_AUDIENCE=DSAGrind.Users
JWT_ACCESS_TOKEN_EXPIRY_MINUTES=15
JWT_REFRESH_TOKEN_EXPIRY_DAYS=30

# OAuth (Production)
GOOGLE_CLIENT_ID=xxxxxxxx-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=GOCSPXxxxxxxxxxxxxxxxxxxxxxxxx
GITHUB_CLIENT_ID=xxxxxxxxxxxxxxxxxxxxxx
GITHUB_CLIENT_SECRET=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Payment Processing (Stripe Production)
STRIPE_PUBLISHABLE_KEY=pk_live_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
STRIPE_SECRET_KEY=sk_live_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
STRIPE_WEBHOOK_SECRET=whsec_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Email Services (SendGrid)
SENDGRID_API_KEY=SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
SENDGRID_FROM_EMAIL=noreply@dsagrind.com
SENDGRID_FROM_NAME=DSAGrind

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://0.0.0.0:443;http://0.0.0.0:80

# CORS (Production domains)
CORS_ALLOWED_ORIGINS=https://dsagrind.com,https://www.dsagrind.com,https://app.dsagrind.com

# Logging & Monitoring
LOG_LEVEL=Information
LOG_TO_FILE=true
ENABLE_DETAILED_ERRORS=false
ENABLE_SENSITIVE_DATA_LOGGING=false

# External APIs
CODE_EXECUTION_API_URL=https://api.judge0.com
CODE_EXECUTION_API_KEY=production_judge0_api_key
```

### 2. Database Setup

#### MongoDB Atlas Configuration
1. **Create Production Cluster**
   - Navigate to MongoDB Atlas
   - Create new cluster in production region
   - Configure VPC peering if needed
   - Set up database user with strong password

2. **Database Schema**
   ```javascript
   // Collections
   db.users.createIndex({ "email": 1 }, { unique: true })
   db.users.createIndex({ "username": 1 }, { unique: true })
   db.problems.createIndex({ "slug": 1 }, { unique: true })
   db.problems.createIndex({ "categoryId": 1, "difficulty": 1 })
   db.categories.createIndex({ "slug": 1 }, { unique: true })
   db.submissions.createIndex({ "userId": 1, "problemId": 1 })
   ```

3. **Backup Strategy**
   - Enable automated backups
   - Configure backup retention policy
   - Set up cross-region backup replication

#### Redis Setup
1. **Redis Cloud Configuration**
   - Create Redis instance
   - Configure memory optimization
   - Set up persistence options
   - Enable clustering if needed

### 3. Container Configuration

#### Dockerfile (Backend Services)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DSAGrind.Gateway.API/DSAGrind.Gateway.API.csproj", "DSAGrind.Gateway.API/"]
COPY ["DSAGrind.Common/DSAGrind.Common.csproj", "DSAGrind.Common/"]
RUN dotnet restore "DSAGrind.Gateway.API/DSAGrind.Gateway.API.csproj"
COPY . .
WORKDIR "/src/DSAGrind.Gateway.API"
RUN dotnet build "DSAGrind.Gateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DSAGrind.Gateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DSAGrind.Gateway.API.dll"]
```

#### Docker Compose (Production)
```yaml
version: '3.8'

services:
  gateway:
    build:
      context: ./backend/src/Services/DSAGrind.Gateway.API
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__MongoDB=${MONGODB_CONNECTION_STRING}
      - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
    depends_on:
      - auth-api
      - problems-api
    networks:
      - dsagrind-network

  auth-api:
    build:
      context: ./backend/src/Services/DSAGrind.Auth.API
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__MongoDB=${MONGODB_CONNECTION_STRING}
      - JwtSettings__Secret=${JWT_SECRET_KEY}
    networks:
      - dsagrind-network

  problems-api:
    build:
      context: ./backend/src/Services/DSAGrind.Problems.API
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__MongoDB=${MONGODB_CONNECTION_STRING}
    networks:
      - dsagrind-network

  frontend:
    build:
      context: ./client
      dockerfile: Dockerfile.prod
    ports:
      - "3000:80"
    environment:
      - VITE_API_URL=https://api.dsagrind.com
    networks:
      - dsagrind-network

networks:
  dsagrind-network:
    driver: bridge
```

## Cloud Deployment Options

### 1. Azure Container Instances

#### Resource Group Setup
```bash
# Create resource group
az group create --name dsagrind-prod --location eastus

# Create container registry
az acr create --resource-group dsagrind-prod --name dsagrindregistry --sku Basic

# Build and push images
az acr build --registry dsagrindregistry --image dsagrind/gateway:latest ./backend/src/Services/DSAGrind.Gateway.API
az acr build --registry dsagrindregistry --image dsagrind/auth:latest ./backend/src/Services/DSAGrind.Auth.API
```

#### Container Deployment
```bash
# Deploy container group
az container create \
  --resource-group dsagrind-prod \
  --name dsagrind-app \
  --image dsagrindregistry.azurecr.io/dsagrind/gateway:latest \
  --cpu 2 \
  --memory 4 \
  --ports 5000 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    MONGODB_CONNECTION_STRING=$MONGODB_CONNECTION_STRING
```

### 2. AWS ECS Fargate

#### Task Definition
```json
{
  "family": "dsagrind-gateway",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "1024",
  "memory": "2048",
  "containerDefinitions": [
    {
      "name": "gateway",
      "image": "dsagrind/gateway:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/dsagrind-gateway",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

### 3. Google Cloud Run

#### Deployment Script
```bash
# Build and push to Container Registry
docker build -t gcr.io/dsagrind-prod/gateway ./backend/src/Services/DSAGrind.Gateway.API
docker push gcr.io/dsagrind-prod/gateway

# Deploy to Cloud Run
gcloud run deploy dsagrind-gateway \
  --image gcr.io/dsagrind-prod/gateway \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production \
  --set-env-vars MONGODB_CONNECTION_STRING=$MONGODB_CONNECTION_STRING
```

## Security Configuration

### 1. SSL/TLS Setup
```bash
# Generate SSL certificate (Let's Encrypt)
certbot certonly --webroot -w /var/www/html -d dsagrind.com -d www.dsagrind.com

# Configure HTTPS redirect
server {
    listen 80;
    server_name dsagrind.com www.dsagrind.com;
    return 301 https://$server_name$request_uri;
}
```

### 2. Security Headers
```csharp
// Configure security headers in Startup.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});
```

### 3. Rate Limiting
```csharp
// Configure rate limiting
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", configure =>
    {
        configure.PermitLimit = 100;
        configure.Window = TimeSpan.FromMinutes(1);
        configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        configure.QueueLimit = 50;
    });
});
```

## Monitoring & Logging

### 1. Application Insights Configuration
```csharp
// Configure Application Insights
services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

// Custom telemetry
services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
```

### 2. Health Checks
```csharp
// Configure health checks
services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString)
    .AddRedis(redisConnectionString)
    .AddUrlGroup(new Uri("https://api.perplexity.ai/health"), "perplexity-api");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 3. Structured Logging
```csharp
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
    .CreateLogger();
```

## Performance Optimization

### 1. Caching Strategy
```csharp
// Configure Redis caching
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "DSAGrind";
});

// Implement caching in services
public async Task<ProblemDto> GetProblemAsync(string id)
{
    var cacheKey = $"problem:{id}";
    var cached = await _cache.GetStringAsync(cacheKey);
    
    if (cached != null)
        return JsonSerializer.Deserialize<ProblemDto>(cached);
    
    var problem = await _repository.GetByIdAsync(id);
    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(problem), 
        new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
    
    return problem;
}
```

### 2. Database Optimization
```csharp
// Configure MongoDB connection pooling
services.Configure<MongoDbSettings>(options =>
{
    options.MaxConnectionPoolSize = 100;
    options.MinConnectionPoolSize = 10;
    options.MaxConnectionIdleTime = TimeSpan.FromMinutes(30);
    options.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
});
```

## Backup & Disaster Recovery

### 1. Database Backup
```bash
# Automated MongoDB backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_NAME="dsagrind_backup_$DATE"

mongodump --uri="$MONGODB_CONNECTION_STRING" --out="/backups/$BACKUP_NAME"
tar -czf "/backups/$BACKUP_NAME.tar.gz" "/backups/$BACKUP_NAME"
aws s3 cp "/backups/$BACKUP_NAME.tar.gz" "s3://dsagrind-backups/"
rm -rf "/backups/$BACKUP_NAME" "/backups/$BACKUP_NAME.tar.gz"
```

### 2. Application State Backup
```bash
# Backup application configuration
kubectl get configmaps -o yaml > configmaps-backup.yaml
kubectl get secrets -o yaml > secrets-backup.yaml
```

## Continuous Deployment

### 1. GitHub Actions Workflow
```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build and Test
        run: |
          dotnet restore
          dotnet build --configuration Release
          dotnet test --no-build --verbosity normal
      
      - name: Build Docker Images
        run: |
          docker build -t dsagrind/gateway:${{ github.sha }} ./backend/src/Services/DSAGrind.Gateway.API
          docker build -t dsagrind/auth:${{ github.sha }} ./backend/src/Services/DSAGrind.Auth.API
      
      - name: Deploy to Cloud Run
        run: |
          echo ${{ secrets.GCP_SA_KEY }} | base64 -d > gcp-key.json
          gcloud auth activate-service-account --key-file gcp-key.json
          gcloud config set project dsagrind-prod
          
          gcloud run deploy dsagrind-gateway \
            --image gcr.io/dsagrind-prod/gateway:${{ github.sha }} \
            --region us-central1
```

## Post-Deployment Verification

### 1. Health Check Verification
```bash
# Verify all services are healthy
curl https://dsagrind.com/health
curl https://api.dsagrind.com/auth/health
curl https://api.dsagrind.com/problems/health
```

### 2. Functional Testing
```bash
# Test user registration
curl -X POST https://api.dsagrind.com/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPass123!"}'

# Test problem retrieval
curl https://api.dsagrind.com/problems?page=1&limit=10
```

### 3. Performance Testing
```bash
# Load testing with Apache Bench
ab -n 1000 -c 50 https://dsagrind.com/
ab -n 500 -c 25 https://api.dsagrind.com/problems
```

## Maintenance & Updates

### 1. Zero-Downtime Deployment
```bash
# Rolling update strategy
kubectl set image deployment/dsagrind-gateway gateway=dsagrind/gateway:v2.0.0
kubectl rollout status deployment/dsagrind-gateway
```

### 2. Database Migration
```csharp
// Run database migrations
public class MigrationService
{
    public async Task MigrateAsync()
    {
        // Add new indexes
        await _database.GetCollection<Problem>("problems")
            .Indexes.CreateOneAsync(new CreateIndexModel<Problem>(
                Builders<Problem>.IndexKeys.Ascending(x => x.Difficulty)));
        
        // Data migration scripts
        await MigrateUserProfiles();
        await UpdateProblemSchema();
    }
}
```

## Troubleshooting

### Common Production Issues

1. **High Memory Usage**
   - Monitor garbage collection
   - Check for memory leaks
   - Optimize caching strategies

2. **Database Connection Issues**
   - Verify connection string
   - Check network connectivity
   - Monitor connection pool usage

3. **Slow API Responses**
   - Enable query profiling
   - Check database indexes
   - Optimize expensive operations

### Monitoring Commands
```bash
# Check application logs
kubectl logs -f deployment/dsagrind-gateway

# Monitor resource usage
kubectl top pods

# Check database performance
mongostat --uri="$MONGODB_CONNECTION_STRING"
```

This production setup ensures high availability, security, and scalability for the DSAGrind platform while maintaining optimal performance and monitoring capabilities.