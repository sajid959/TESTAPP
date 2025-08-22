# DSAGrind Setup Guide

## Prerequisites

### System Requirements
- **Node.js 20+** - JavaScript runtime
- **.NET 8 SDK** - Backend framework
- **MongoDB** - Database (or MongoDB Atlas)
- **Redis** - Caching (optional for development)
- **Git** - Version control

### Frontend Technologies
- **React 18** - UI Library
- **TypeScript** - Type Safety
- **Vite** - Build Tool & Dev Server
- **TailwindCSS** - Utility-first CSS Framework
- **Shadcn/ui** - Component Library
- **Wouter** - Lightweight Router
- **TanStack Query** - Server State Management
- **Framer Motion** - Animations
- **Monaco Editor** - Code Editor
- **Recharts** - Charts and Analytics

### Backend Technologies
- **.NET 8** - Backend Framework
- **MongoDB** - Primary Database
- **Redis** - Caching Layer
- **Kafka** - Message Queue
- **YARP** - Reverse Proxy
- **Serilog** - Logging
- **AutoMapper** - Object Mapping
- **FluentValidation** - Input Validation

## Installation

### 1. Clone Repository
```bash
git clone <repository-url>
cd dsagrind
```

### 2. Frontend Setup
```bash
cd client
npm install
```

### 3. Backend Setup
```bash
cd backend
dotnet restore
```

### 4. Environment Configuration

Create a `.env` file in the root directory:

```env
# Database Configuration
DATABASE_URL="postgresql://username:password@localhost:5432/dsagrind"

# JWT Configuration
JWT_SECRET="your-super-secret-jwt-key-change-in-production"
JWT_REFRESH_SECRET="your-refresh-secret-key-change-in-production"

# Application URLs
FRONTEND_URL="http://localhost:5000"
BACKEND_URL="http://localhost:5000"

# External API Keys (Optional)
PERPLEXITY_API_KEY="your-perplexity-api-key"
STRIPE_SECRET_KEY="sk_test_your_stripe_secret_key"
STRIPE_PUBLISHABLE_KEY="pk_test_your_stripe_publishable_key"

# Email Configuration (Optional - using mock service)
SMTP_HOST="smtp.gmail.com"
SMTP_PORT="587"
SMTP_USER="your-email@gmail.com"
SMTP_PASS="your-app-password"

# Development
NODE_ENV="development"
PORT="5000"
```

## Database Setup

### Option 1: Local PostgreSQL (Docker)
```bash
docker run --name dsagrind-postgres \
  -e POSTGRES_DB=dsagrind \
  -e POSTGRES_USER=dsagrind_user \
  -e POSTGRES_PASSWORD=your_password \
  -p 5432:5432 \
  -d postgres:15

# Update .env file
DATABASE_URL="postgresql://dsagrind_user:your_password@localhost:5432/dsagrind"
```

### Option 2: MongoDB Atlas (Recommended)
1. Create account at [MongoDB Atlas](https://cloud.mongodb.com/)
2. Create a free cluster
3. Get connection string
4. Update `.env` file:
```env
MONGODB_CONNECTION_STRING="mongodb+srv://username:password@cluster.mongodb.net/dsagrind"
```

## Development

### Start All Services
```bash
# Option 1: Use the startup script
./start-all.sh

# Option 2: Manual startup
# Terminal 1: Frontend
cd client && npm run dev

# Terminal 2: Gateway API
cd backend/src/Services/DSAGrind.Gateway.API
dotnet run --urls="http://0.0.0.0:5000"

# Terminal 3: Auth API
cd backend/src/Services/DSAGrind.Auth.API
dotnet run --urls="http://0.0.0.0:8080"
```

### Application URLs
- **Frontend**: http://localhost:3000
- **Gateway API**: http://localhost:5000
- **Auth API**: http://localhost:8080
- **Swagger UI**: http://localhost:5000/swagger

## API Documentation

### Authentication Endpoints
```
POST /api/auth/register - User registration
POST /api/auth/login - User login
POST /api/auth/refresh - Refresh access token
POST /api/auth/logout - User logout
POST /api/auth/verify-email - Email verification
POST /api/auth/forgot-password - Password reset request
POST /api/auth/reset-password - Password reset
```

### Problems Endpoints
```
GET /api/problems - List problems
GET /api/problems/{id} - Get problem details
POST /api/problems - Create problem (admin)
PUT /api/problems/{id} - Update problem (admin)
DELETE /api/problems/{id} - Delete problem (admin)
POST /api/problems/bulk-import - Bulk import problems
```

### Categories Endpoints
```
GET /api/categories - List categories
GET /api/categories/{id} - Get category details
POST /api/categories - Create category (admin)
PUT /api/categories/{id} - Update category (admin)
```

## Testing

### Frontend Testing
```bash
cd client
npm test
npm run test:coverage
```

### Backend Testing
```bash
cd backend
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Testing
```bash
# Start test environment
./scripts/start-test-env.sh

# Run integration tests
npm run test:integration
```

## Production Deployment

### Environment Variables
Set these in production:
```env
NODE_ENV="production"
ASPNETCORE_ENVIRONMENT="Production"
JWT_SECRET="secure-production-secret-256-bits"
DATABASE_URL="production-database-url"
REDIS_URL="production-redis-url"
```

### Build Commands
```bash
# Frontend
cd client
npm run build

# Backend
cd backend
dotnet publish -c Release
```

### Docker Deployment
```bash
# Build containers
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f
```

## Troubleshooting

### Common Issues

**Frontend not starting:**
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

**Backend API errors:**
```bash
# Check .NET version
dotnet --version

# Restore packages
dotnet restore

# Clear build cache
dotnet clean
```

**Database connection issues:**
```bash
# Test MongoDB connection
mongosh "your-connection-string"

# Check environment variables
echo $DATABASE_URL
```

### Performance Optimization

**Frontend:**
- Enable code splitting
- Optimize bundle size
- Use lazy loading for routes
- Implement proper caching

**Backend:**
- Enable response compression
- Use connection pooling
- Implement caching strategies
- Optimize database queries

### Monitoring & Logging

**Application Logs:**
```bash
# View application logs
tail -f logs/dsagrind.log

# View specific service logs
docker logs dsagrind-api
```

**Health Checks:**
- Frontend: http://localhost:3000/health
- Gateway: http://localhost:5000/health
- Auth API: http://localhost:8080/health

## Support

### Documentation
- [Architecture Guide](./ARCHITECTURE.md)
- [API Documentation](./api/README.md)
- [Deployment Guide](./DEPLOYMENT.md)

### Getting Help
- Create an issue on GitHub
- Check existing documentation
- Review error logs and stack traces

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request