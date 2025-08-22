# DSAGrind - Production Setup Guide

## 🚀 Complete Setup Instructions

DSAGrind is a production-ready competitive programming platform with React frontend and .NET 8 microservices backend. This guide will help you set up the entire application.

## 📋 Prerequisites

### System Requirements
- **Operating System**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 18.04+)
- **Memory**: Minimum 8GB RAM (16GB recommended)
- **Storage**: 5GB free space
- **Network**: Internet connection for dependencies

### Required Software

#### 1. .NET 8 SDK
```bash
# Windows (via winget)
winget install Microsoft.DotNet.SDK.8

# macOS (via Homebrew)
brew install --cask dotnet

# Linux (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0
```

#### 2. Node.js (v18+ LTS)
```bash
# Windows (via winget)
winget install OpenJS.NodeJS

# macOS (via Homebrew)
brew install node

# Linux (via NodeSource)
curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
sudo apt-get install -y nodejs
```

#### 3. Git
```bash
# Windows (via winget)
winget install Git.Git

# macOS (via Homebrew)
brew install git

# Linux
sudo apt-get install git
```

## 🛠️ Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/dsagrind.git
cd dsagrind
```

### 2. Quick Start (Recommended)

#### For Windows:
```bash
# Run the complete application
start-all.bat
```

#### For Linux/macOS:
```bash
# Make scripts executable
chmod +x start-all.sh stop-all.sh

# Run the complete application
./start-all.sh
```

The startup script will:
- ✅ Check all prerequisites
- ✅ Install frontend dependencies
- ✅ Build backend services
- ✅ Start all 8 microservices
- ✅ Start React frontend
- ✅ Perform health checks
- ✅ Open application in browser

### 3. Manual Setup (Advanced)

#### Backend Setup
```bash
# Navigate to backend
cd backend

# Restore NuGet packages
dotnet restore

# Build all services
dotnet build --configuration Release

# Run individual services (in separate terminals)
cd src/Services/DSAGrind.Gateway.API && dotnet run --urls="http://0.0.0.0:5000"
cd src/Services/DSAGrind.Auth.API && dotnet run --urls="http://0.0.0.0:8080"
cd src/Services/DSAGrind.Problems.API && dotnet run --urls="http://0.0.0.0:5001"
# ... (continue for all 8 services)
```

#### Frontend Setup
```bash
# Navigate to frontend
cd client

# Install dependencies
npm install

# Start development server
npm run dev
```

## 🌐 Application URLs

After successful startup, access these URLs:

### Main Application
- **Frontend**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger

### Microservices (Direct Access)
- **Auth API**: http://localhost:8080
- **Problems API**: http://localhost:5001
- **Submissions API**: http://localhost:5002
- **AI API**: http://localhost:5003
- **Search API**: http://localhost:5004
- **Admin API**: http://localhost:5005
- **Payments API**: http://localhost:5006

### Health Checks
- Gateway: http://localhost:5000/health
- All services: http://localhost:{port}/api/health

## ⚙️ Configuration

### Environment Variables

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

#### Frontend (client/.env)
```bash
# API Configuration
VITE_GATEWAY_URL=http://localhost:5000
VITE_APP_NAME=DSAGrind

# Development
VITE_ENABLE_DEV_TOOLS=true
VITE_LOG_LEVEL=debug
```

### Production Configuration

For production deployment, update:

1. **Database**: Use MongoDB Atlas or production MongoDB instance
2. **Redis**: Use Redis Cloud or production Redis instance
3. **AI API**: Add real Perplexity API key
4. **JWT**: Use cryptographically secure secret
5. **OAuth**: Configure real Google/GitHub OAuth apps
6. **CORS**: Set specific production origins
7. **HTTPS**: Enable SSL/TLS certificates

## 🧪 Testing

### Health Checks
```bash
# Check all services are running
curl http://localhost:5000/health
curl http://localhost:8080/api/health
curl http://localhost:5001/api/health
# ... (test all 8 services)
```

### Frontend Tests
```bash
cd client
npm test
```

### Backend Tests
```bash
cd backend
dotnet test
```

## 🏗️ Architecture Overview

### Frontend (React + TypeScript)
- **Framework**: React 18 + TypeScript
- **Styling**: Tailwind CSS + Shadcn UI
- **Routing**: Wouter
- **State Management**: TanStack Query
- **Build Tool**: Vite
- **Package Manager**: npm

### Backend (.NET 8 Microservices)
- **API Gateway**: YARP Reverse Proxy (Port 5000)
- **Authentication**: JWT + OAuth (Port 8080)
- **Problems**: CRUD + Categories (Port 5001)
- **Submissions**: Code execution (Port 5002)
- **AI**: Test AI service (Port 5003)
- **Search**: Vector search (Port 5004)
- **Admin**: Management dashboard (Port 5005)
- **Payments**: Stripe integration (Port 5006)

### Key Features
- ✅ Microservices architecture
- ✅ API Gateway with routing
- ✅ JWT authentication
- ✅ OAuth integration (Google, GitHub)
- ✅ Multi-language IDE (Monaco Editor)
- ✅ AI-powered assistance
- ✅ Real-time submissions
- ✅ Admin dashboard
- ✅ Payment processing
- ✅ Cross-platform support

## 🛑 Stopping the Application

### Windows:
```bash
stop-all.bat
```

### Linux/macOS:
```bash
./stop-all.sh
```

Or press `Ctrl+C` in the terminal running `start-all` script.

## 📝 Logs

Logs are stored in:
- **Backend logs**: `backend/logs/`
- **Frontend logs**: `logs/frontend.log`
- **Individual service logs**: `backend/logs/{service}.log`

## 🚨 Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Check what's using a port
lsof -i :5000  # Linux/macOS
netstat -ano | findstr :5000  # Windows

# Kill process using port
kill -9 PID  # Linux/macOS
taskkill /PID PID /F  # Windows
```

#### Frontend Build Issues
```bash
cd client
rm -rf node_modules package-lock.json
npm install
```

#### Backend Build Issues
```bash
cd backend
dotnet clean
dotnet restore
dotnet build
```

#### Services Not Starting
1. Check logs in `backend/logs/`
2. Verify all prerequisites are installed
3. Ensure ports are available
4. Check environment variables

### Performance Optimization

#### Development
- Use `npm run dev` for frontend hot reload
- Backend services auto-reload on file changes
- Enable detailed error messages

#### Production
- Use `npm run build` for optimized frontend
- Configure reverse proxy (nginx/IIS)
- Enable compression and caching
- Use production databases
- Configure monitoring and logging

## 📚 Additional Resources

### Documentation
- [Architecture Guide](./ARCHITECTURE.md)
- [Features Documentation](./FEATURES_DOCUMENTATION.md)
- [User Flows](./USER_FLOWS.md)
- [Setup Guide](./SETUP_GUIDE.md)

### Development
- **Frontend**: React + TypeScript best practices
- **Backend**: .NET 8 microservices patterns
- **Database**: MongoDB with proper indexing
- **Caching**: Redis for performance
- **Real-time**: SignalR for live updates

### Production Deployment
- **Cloud**: Azure, AWS, or Google Cloud
- **Containers**: Docker + Kubernetes
- **CI/CD**: GitHub Actions or Azure DevOps
- **Monitoring**: Application Insights or equivalent
- **Security**: HTTPS, security headers, input validation

## 🎯 Next Steps

1. **Customize**: Modify branding and themes
2. **Configure**: Set up production databases
3. **Deploy**: Choose cloud provider and deploy
4. **Monitor**: Set up logging and monitoring
5. **Scale**: Configure auto-scaling and load balancing

---

**🎉 Congratulations!** You now have a fully functional, production-ready competitive programming platform. The application includes modern frontend, scalable backend, and comprehensive tooling for development and deployment.