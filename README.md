# DSAGrind - Competitive Programming Platform

A comprehensive competitive programming platform built with .NET 8 microservices architecture and React TypeScript frontend, featuring AI-powered assistance, multi-language IDE, admin management, OAuth authentication, and premium subscription features.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Setup Scenarios](#setup-scenarios)
  - [1. Running Individual Services (Without Docker)](#1-running-individual-services-without-docker)
  - [2. Running with Docker Compose (Recommended)](#2-running-with-docker-compose-recommended)
  - [3. Production Deployment](#3-production-deployment)
- [Environment Variables & API Keys](#environment-variables--api-keys)
- [Service Endpoints](#service-endpoints)
- [Troubleshooting](#troubleshooting)

## Architecture Overview

### Frontend
- **React 18** with TypeScript
- **Vite** for fast development
- **Tailwind CSS + Shadcn UI** for styling
- **Monaco Editor** for code editing
- **TanStack Query** for state management

### Backend Microservices
- **Gateway API** (Port 5000) - YARP reverse proxy
- **Authentication API** (Port 8080) - JWT + OAuth
- **Problems API** (Port 5001) - Problem management
- **Submissions API** (Port 5002) - Code execution
- **AI API** (Port 5003) - OpenAI integration
- **Search API** (Port 5004) - Vector search with Qdrant
- **Admin API** (Port 5005) - Admin dashboard
- **Payments API** (Port 5006) - Stripe integration
- **MCP API** (Port 5007) - Model Context Protocol

### External Dependencies
- **MongoDB** - Primary database
- **Redis** - Caching and sessions
- **Apache Kafka** - Message broker
- **Qdrant** - Vector database
- **OpenAI API** - AI assistance
- **Stripe** - Payments
- **Google/GitHub OAuth** - Authentication

## Prerequisites

### For All Scenarios
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Git** - [Download](https://git-scm.com/)

### For Docker Scenario
- **Docker** - [Download](https://www.docker.com/get-started)
- **Docker Compose** - Included with Docker Desktop

### For Individual Services
- **MongoDB** - [Download](https://www.mongodb.com/try/download/community)
- **Redis** - [Download](https://redis.io/download)
- **Apache Kafka** - [Download](https://kafka.apache.org/downloads)
- **Qdrant** - [Download](https://qdrant.tech/documentation/quick-start/)

## Setup Scenarios

## 1. Running Individual Services (Without Docker)

### Step 1: Clone and Setup
```bash
git clone <your-repo-url>
cd dsagrind
```

### Step 2: Install Dependencies
```bash
# Frontend dependencies
cd client
npm install
cd ..

# Restore .NET packages
cd backend
dotnet restore
cd ..
```

### Step 3: Setup External Services

#### MongoDB
```bash
# Install and start MongoDB
mongod --dbpath /path/to/your/data/directory

# Create database (optional, auto-created)
mongosh
use dsagrind
```

#### Redis
```bash
# Install and start Redis
redis-server

# Test connection
redis-cli ping
```

#### Apache Kafka & Zookeeper
```bash
# Start Zookeeper
bin/zookeeper-server-start.sh config/zookeeper.properties

# Start Kafka
bin/kafka-server-start.sh config/server.properties
```

#### Qdrant
```bash
# Using Docker (recommended for Qdrant)
docker run -p 6333:6333 qdrant/qdrant

# Or install locally - see Qdrant documentation
```

### Step 4: Configure Environment Variables
Create `appsettings.Development.json` in each service directory:

**Authentication API** (`backend/src/Services/DSAGrind.Auth.API/appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017",
    "Redis": "localhost:6379"
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "dsagrind"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-jwt-key-minimum-32-characters",
    "Issuer": "DSAGrind",
    "Audience": "DSAGrind-Users",
    "ExpiryMinutes": 60
  },
  "OAuthSettings": {
    "Google": {
      "ClientId": "your-google-oauth-client-id",
      "ClientSecret": "your-google-oauth-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-oauth-client-id",
      "ClientSecret": "your-github-oauth-client-secret"
    }
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "auth-service"
  }
}
```

**Problems API** (`backend/src/Services/DSAGrind.Problems.API/appsettings.Development.json`):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "dsagrind"
  },
  "JwtSettings": {
    "Audience": "DSAGrind-Users"
  },
  "Auth": {
    "Authority": "http://localhost:8080"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "problems-service"
  }
}
```

**AI API** (`backend/src/Services/DSAGrind.AI.API/appsettings.Development.json`):
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  },
  "JwtSettings": {
    "Audience": "DSAGrind-Users"
  },
  "Auth": {
    "Authority": "http://localhost:8080"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "ai-service"
  }
}
```

**Payments API** (`backend/src/Services/DSAGrind.Payments.API/appsettings.Development.json`):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "dsagrind"
  },
  "Stripe": {
    "SecretKey": "your-stripe-secret-key-here",
    "PublishableKey": "your-stripe-publishable-key-here"
  },
  "Payments": {
    "UseHttps": false
  },
  "PaymentSettings": {
    "UseMockData": true,
    "EnableStripeIntegration": false
  }
}
```

### Step 5: Start Services Individually

#### Terminal 1 - Authentication API
```bash
cd backend/src/Services/DSAGrind.Auth.API
dotnet run
```

#### Terminal 2 - Problems API
```bash
cd backend/src/Services/DSAGrind.Problems.API
dotnet run
```

#### Terminal 3 - Submissions API
```bash
cd backend/src/Services/DSAGrind.Submissions.API
dotnet run
```

#### Terminal 4 - AI API
```bash
cd backend/src/Services/DSAGrind.AI.API
dotnet run
```

#### Terminal 5 - Search API
```bash
cd backend/src/Services/DSAGrind.Search.API
dotnet run
```

#### Terminal 6 - Admin API
```bash
cd backend/src/Services/DSAGrind.Admin.API
dotnet run
```

#### Terminal 7 - Payments API
```bash
cd backend/src/Services/DSAGrind.Payments.API
dotnet run
```

#### Terminal 8 - MCP API
```bash
cd backend/src/Services/DSAGrind.MCP.API
dotnet run
```

#### Terminal 9 - Gateway API
```bash
cd backend/src/Services/DSAGrind.Gateway.API
dotnet run
```

#### Terminal 10 - Frontend
```bash
cd client
npm run dev
```

## 2. Running with Docker Compose (Recommended)

### Step 1: Clone Repository
```bash
git clone <your-repo-url>
cd dsagrind
```

### Step 2: Create Environment File
Create `backend/.env` file:
```bash
# Required API Keys
OPENAI_API_KEY=your-openai-api-key-here
STRIPE_SECRET_KEY=your-stripe-secret-key-here
JWT_SECRET_KEY=your-super-secret-jwt-key-minimum-32-characters

# OAuth Configuration
GOOGLE_CLIENT_ID=your-google-oauth-client-id
GOOGLE_CLIENT_SECRET=your-google-oauth-client-secret
GITHUB_CLIENT_ID=your-github-oauth-client-id
GITHUB_CLIENT_SECRET=your-github-oauth-client-secret

# Optional: Email Configuration
SENDGRID_API_KEY=your-sendgrid-api-key
EMAIL_FROM=noreply@your-domain.com
```

### Step 3: Build and Start All Services
```bash
cd backend
docker-compose up --build
```

This will start:
- ✅ MongoDB (Port 27017)
- ✅ Redis (Port 6379)
- ✅ Apache Kafka (Port 9092)
- ✅ Zookeeper (Port 2181)
- ✅ Qdrant (Port 6333)
- ✅ All 9 Backend APIs
- ⚠️ Frontend needs to be started separately

### Step 4: Start Frontend
```bash
cd client
npm install
npm run dev
```

### Step 5: Access Application
- **Frontend**: http://localhost:3000
- **Gateway API**: http://localhost:5000
- **MongoDB**: mongodb://localhost:27017
- **Redis**: redis://localhost:6379

### Useful Docker Commands
```bash
# View logs
docker-compose logs -f [service-name]

# Stop all services
docker-compose down

# Remove volumes (reset data)
docker-compose down -v

# Rebuild specific service
docker-compose up --build [service-name]
```

## 3. Production Deployment

### Option A: Cloud Platform (Azure/AWS/GCP)

#### Prerequisites
- Container registry (Docker Hub, Azure Container Registry, etc.)
- Kubernetes cluster or container service
- Managed MongoDB (Atlas, Cosmos DB, etc.)
- Managed Redis (Redis Cloud, ElastiCache, etc.)

#### Steps
1. **Build and Push Images**
```bash
# Build all images
docker-compose build

# Tag and push to registry
docker tag dsagrind-gateway your-registry/dsagrind-gateway:latest
docker push your-registry/dsagrind-gateway:latest
# Repeat for all services
```

2. **Deploy Database Services**
```bash
# MongoDB Atlas
# Create cluster at https://cloud.mongodb.com
# Get connection string

# Redis Cloud
# Create instance at https://redis.com
# Get connection string

# Qdrant Cloud
# Create cluster at https://cloud.qdrant.io
# Get API endpoint and key
```

3. **Configure Production Environment Variables**
```bash
# In your cloud deployment platform
ASPNETCORE_ENVIRONMENT=Production
MONGODB_CONNECTION_STRING=mongodb+srv://user:pass@cluster.mongodb.net/dsagrind
REDIS_CONNECTION_STRING=redis://your-redis-cloud-endpoint:port
QDRANT_HOST=your-qdrant-cloud-endpoint
QDRANT_API_KEY=your-qdrant-api-key

# API Keys (same as development but production keys)
OPENAI_API_KEY=your-production-openai-key
STRIPE_SECRET_KEY=your-production-stripe-key
JWT_SECRET_KEY=your-production-jwt-secret

# OAuth (configure production URLs)
GOOGLE_CLIENT_ID=your-production-google-client-id
GOOGLE_CLIENT_SECRET=your-production-google-client-secret
GITHUB_CLIENT_ID=your-production-github-client-id
GITHUB_CLIENT_SECRET=your-production-github-client-secret

# Domain configuration
CORS_ALLOWED_ORIGINS=https://your-domain.com,https://www.your-domain.com
```

### Option B: VPS/Dedicated Server

#### Prerequisites
- Ubuntu/CentOS server with Docker installed
- Domain name and SSL certificate
- Nginx for reverse proxy

#### Steps
1. **Setup Server**
```bash
# Install Docker and Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

2. **Configure Nginx**
```nginx
# /etc/nginx/sites-available/dsagrind
server {
    listen 80;
    server_name your-domain.com www.your-domain.com;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /api/ {
        proxy_pass http://localhost:5000/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

3. **Setup SSL with Certbot**
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com -d www.your-domain.com
```

4. **Deploy Application**
```bash
# Clone repo on server
git clone <your-repo-url>
cd dsagrind

# Set environment variables
cp backend/.env.example backend/.env
# Edit with your production values

# Start services
cd backend
docker-compose up -d
```

### Option C: Replit Deployment

#### Prerequisites
- Replit account
- All API keys configured

#### Steps
1. **Configure Environment Variables in Replit**
```bash
# In Replit Secrets tab, add:
OPENAI_API_KEY=your-openai-api-key
STRIPE_SECRET_KEY=your-stripe-secret-key  
JWT_SECRET_KEY=your-jwt-secret
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
GITHUB_CLIENT_ID=your-github-client-id
GITHUB_CLIENT_SECRET=your-github-client-secret
```

2. **Use Replit Database**
```bash
# MongoDB Atlas connection string
MONGODB_CONNECTION_STRING=mongodb+srv://user:pass@cluster.mongodb.net/dsagrind

# Or use Replit PostgreSQL if preferred (requires code changes)
```

3. **Deploy**
```bash
# Frontend runs automatically on port 3000
# Gateway API runs on port 5000
# Configure production URLs in OAuth providers
```

## Environment Variables & API Keys

### Required API Keys Setup

#### 1. OpenAI API Key
```bash
# Get from: https://platform.openai.com/api-keys
# Steps:
# 1. Sign up/login to OpenAI
# 2. Go to API Keys section
# 3. Create new secret key
# 4. Copy the key (starts with sk-)

OPENAI_API_KEY=sk-...your-key-here
```

#### 2. Stripe Keys
```bash
# Get from: https://dashboard.stripe.com/apikeys
# Steps:
# 1. Create Stripe account
# 2. Go to Developers > API Keys
# 3. Get both secret and publishable keys

# For Development
STRIPE_SECRET_KEY=sk_test_...your-test-key
STRIPE_PUBLISHABLE_KEY=pk_test_...your-test-publishable-key

# For Production  
STRIPE_SECRET_KEY=sk_live_...your-live-key
STRIPE_PUBLISHABLE_KEY=pk_live_...your-live-publishable-key
```

#### 3. OAuth Applications

**Google OAuth:**
```bash
# Setup at: https://console.developers.google.com
# Steps:
# 1. Create new project or select existing
# 2. Enable Google+ API
# 3. Go to Credentials > Create Credentials > OAuth 2.0 Client ID
# 4. Set application type to "Web application"
# 5. Add authorized redirect URIs:

# Development
http://localhost:5000/auth/google/callback
http://localhost:8080/auth/google/callback

# Production
https://your-domain.com/auth/google/callback

GOOGLE_CLIENT_ID=your-app-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=GOCSPX-your-secret
```

**GitHub OAuth:**
```bash
# Setup at: https://github.com/settings/applications/new
# Steps:
# 1. Go to Settings > Developer settings > OAuth Apps
# 2. Click "New OAuth App"
# 3. Fill in details:
#    - Application name: DSAGrind
#    - Homepage URL: http://localhost:3000 (dev) or https://your-domain.com (prod)
#    - Authorization callback URL: http://localhost:5000/auth/github/callback

GITHUB_CLIENT_ID=your-github-client-id
GITHUB_CLIENT_SECRET=your-github-client-secret
```

#### 4. JWT Secret
```bash
# Generate a secure random key (minimum 32 characters)
# You can use online generators or:
openssl rand -base64 32

JWT_SECRET_KEY=your-super-secure-jwt-secret-key-minimum-32-characters-long
```

#### 5. Email Service (Optional)
```bash
# SendGrid setup: https://sendgrid.com
# Steps:
# 1. Create SendGrid account
# 2. Go to Settings > API Keys
# 3. Create new API key with Mail Send permissions

SENDGRID_API_KEY=SG.your-sendgrid-api-key
EMAIL_FROM=noreply@your-domain.com
```

### Environment Configuration Examples

#### Development (.env)
```bash
# Database connections (local)
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
REDIS_CONNECTION_STRING=localhost:6379
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
QDRANT_HOST=localhost
QDRANT_PORT=6333

# API Keys
OPENAI_API_KEY=sk-your-dev-key
STRIPE_SECRET_KEY=sk_test_your-test-key
JWT_SECRET_KEY=your-dev-jwt-secret

# OAuth (use test apps)
GOOGLE_CLIENT_ID=your-dev-google-id
GOOGLE_CLIENT_SECRET=your-dev-google-secret
GITHUB_CLIENT_ID=your-dev-github-id
GITHUB_CLIENT_SECRET=your-dev-github-secret

# CORS
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5000
```

#### Production (.env)
```bash
# Managed database services
MONGODB_CONNECTION_STRING=mongodb+srv://user:pass@cluster.mongodb.net/dsagrind
REDIS_CONNECTION_STRING=rediss://user:pass@redis-cluster.cloud.redislabs.com:port
KAFKA_BOOTSTRAP_SERVERS=your-kafka-cluster:9092
QDRANT_HOST=your-qdrant-cluster.qdrant.cloud
QDRANT_API_KEY=your-qdrant-api-key

# Production API Keys
OPENAI_API_KEY=sk-your-production-key
STRIPE_SECRET_KEY=sk_live_your-live-key
JWT_SECRET_KEY=your-super-secure-production-jwt-secret

# Production OAuth
GOOGLE_CLIENT_ID=your-prod-google-id
GOOGLE_CLIENT_SECRET=your-prod-google-secret
GITHUB_CLIENT_ID=your-prod-github-id
GITHUB_CLIENT_SECRET=your-prod-github-secret

# Production CORS
CORS_ALLOWED_ORIGINS=https://your-domain.com,https://www.your-domain.com
```

## Service Endpoints

### Frontend
- **Development**: http://localhost:3000
- **Production**: https://your-domain.com

### Backend APIs
| Service | Port | Endpoint | Purpose |
|---------|------|----------|---------|
| Gateway | 5000 | http://localhost:5000 | Main API entry point |
| Auth | 8080 | http://localhost:8080 | Authentication & OAuth |
| Problems | 5001 | http://localhost:5001 | Problem management |
| Submissions | 5002 | http://localhost:5002 | Code execution |
| AI | 5003 | http://localhost:5003 | AI assistance |
| Search | 5004 | http://localhost:5004 | Search & recommendations |
| Admin | 5005 | http://localhost:5005 | Admin dashboard |
| Payments | 5006 | http://localhost:5006 | Stripe integration |
| MCP | 5007 | http://localhost:5007 | Model Context Protocol |

### Health Checks
- Gateway: http://localhost:5000/health
- Auth: http://localhost:8080/health
- Each service: http://localhost:{port}/health

### API Documentation
- Gateway Swagger: http://localhost:5000/swagger
- Auth Swagger: http://localhost:8080/swagger
- Each service: http://localhost:{port}/swagger

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
```bash
# Find process using port
lsof -i :5000
netstat -tulpn | grep :5000

# Kill process
kill -9 <process-id>
```

#### 2. MongoDB Connection Issues
```bash
# Check if MongoDB is running
sudo systemctl status mongod

# Check connection
mongosh "mongodb://localhost:27017/dsagrind"
```

#### 3. Redis Connection Issues
```bash
# Check if Redis is running
redis-cli ping

# Should return "PONG"
```

#### 4. Docker Issues
```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs -f [service-name]

# Restart services
docker-compose restart [service-name]

# Clean rebuild
docker-compose down -v
docker-compose up --build
```

#### 5. HTTPS/Certificate Issues
- Development: All services use HTTP (certificate issues are disabled)
- Production: Use proper SSL certificates with nginx or load balancer

#### 6. CORS Issues
```bash
# Update CORS settings in appsettings.json
"Cors": {
  "AllowedOrigins": ["http://localhost:3000", "https://your-domain.com"]
}
```

### Performance Optimization

#### Production Settings
```bash
# In appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "MongoDB": "your-optimized-connection-string-with-connection-pooling"
  }
}
```

#### Database Indexing
```javascript
// MongoDB indexes for better performance
db.problems.createIndex({ "difficulty": 1, "category": 1 })
db.submissions.createIndex({ "userId": 1, "createdAt": -1 })
db.users.createIndex({ "email": 1 }, { unique: true })
```

### Deployment Checklist

#### Before Deployment
- [ ] All API keys configured
- [ ] OAuth redirect URIs updated for production domain
- [ ] Database connection strings updated
- [ ] CORS origins configured for production domain
- [ ] SSL certificates configured
- [ ] Environment variables set to Production
- [ ] Database indexes created
- [ ] Monitoring and logging configured

#### Production Environment Variables
```bash
# Essential production settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000

# Security
JWT_SECRET_KEY=production-secret-minimum-32-chars
STRIPE_SECRET_KEY=sk_live_production-stripe-key

# Databases (use managed services)
MONGODB_CONNECTION_STRING=mongodb+srv://production-connection
REDIS_CONNECTION_STRING=production-redis-connection

# OAuth production apps
GOOGLE_CLIENT_ID=production-google-client-id
GITHUB_CLIENT_ID=production-github-client-id

# Domain configuration
CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

### Support
For issues and questions:
1. Check this README first
2. Review service logs: `docker-compose logs -f [service-name]`
3. Check GitHub issues
4. Contact support team

---

**DSAGrind** - Empowering developers through competitive programming! 🚀