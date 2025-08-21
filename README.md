# DSAGrind - Competitive Programming Platform

A comprehensive competitive programming platform built with .NET 8 microservices architecture, featuring AI-powered assistance, multi-language IDE, admin management, OAuth authentication, and complete problem management system.

## 🚀 Features

### Core Features
- **Multi-Language IDE**: Support for C#, Python, JavaScript, Java, C++ with real-time code execution
- **AI-Powered Assistance**: OpenAI integration for hints, explanations, code analysis, and optimization suggestions  
- **Vector Search**: Semantic problem discovery using Qdrant vector database
- **OAuth Authentication**: Google and GitHub OAuth integration with JWT tokens
- **Payment Processing**: Stripe integration for premium subscriptions
- **Admin Dashboard**: Comprehensive admin panel for user and content management
- **Real-time Execution**: Docker-based sandboxing for secure code execution

### Technical Architecture
- **Microservices**: 8 independent services (Auth, Problems, Submissions, AI, Search, Admin, Payments, Gateway)
- **Event-Driven**: Apache Kafka for async messaging between services
- **Database**: MongoDB for data persistence, Redis for caching
- **API Gateway**: YARP reverse proxy with rate limiting and load balancing
- **Containerization**: Full Docker support with docker-compose

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   API Gateway   │    │  Auth Service   │
│   (React)       │◄──►│   (YARP)        │◄──►│   (JWT/OAuth)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                       ┌────────┼────────┐
                       │                 │
            ┌─────────────────┐ ┌─────────────────┐
            │ Problems API    │ │ Submissions API │
            │ (CRUD/Search)   │ │ (Code Exec)     │
            └─────────────────┘ └─────────────────┘
                       │                 │
            ┌─────────────────┐ ┌─────────────────┐
            │   AI Service    │ │  Search Service │
            │   (OpenAI)      │ │   (Qdrant)      │
            └─────────────────┘ └─────────────────┘
                       │                 │
            ┌─────────────────┐ ┌─────────────────┐
            │  Admin API      │ │ Payments API    │
            │ (Management)    │ │   (Stripe)      │
            └─────────────────┘ └─────────────────┘
```

## 🛠️ Technology Stack

### Backend (.NET 8)
- **Framework**: ASP.NET Core Web API
- **Authentication**: JWT Bearer tokens + OAuth2 (Google, GitHub)
- **Database**: MongoDB with Repository pattern
- **Caching**: Redis for session management and performance
- **Messaging**: Apache Kafka for event-driven architecture
- **Vector DB**: Qdrant for semantic search
- **Payments**: Stripe for subscription management
- **Logging**: Serilog with structured logging
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **API Documentation**: Swagger/OpenAPI

### Frontend (React + TypeScript)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite for fast development
- **Styling**: Tailwind CSS + Shadcn UI components
- **State Management**: TanStack Query for server state
- **Routing**: Wouter for client-side routing
- **Forms**: React Hook Form with Zod validation
- **Code Editor**: Monaco Editor for multi-language support
- **Authentication**: JWT with refresh token rotation

### Infrastructure
- **Containerization**: Docker + Docker Compose
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **Rate Limiting**: Built-in ASP.NET Core rate limiting
- **Health Checks**: Comprehensive service health monitoring
- **CORS**: Configured for secure cross-origin requests

## 🚀 Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for development)
- Node.js 18+ (for frontend development)

### Environment Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd dsagrind
```

2. **Set up environment variables**
```bash
# Copy the example environment file
cp backend/.env.example backend/.env

# Edit the .env file with your API keys:
# - OPENAI_API_KEY: Your OpenAI API key
# - STRIPE_SECRET_KEY: Your Stripe secret key
# - MONGODB_CONNECTION_STRING: MongoDB connection string
# - JWT_SECRET_KEY: Your JWT secret (min 32 characters)
```

3. **Start the application**
```bash
# Start all services with Docker Compose
cd backend
docker-compose up -d

# Or run individual services for development
dotnet run --project src/Services/DSAGrind.Gateway.API
```

4. **Frontend Development**
```bash
# Install dependencies and start frontend
npm install
npm run dev
```

### Service Ports
- **Frontend**: http://localhost:5000
- **API Gateway**: http://localhost:5000/api
- **Auth API**: http://localhost:8080
- **Problems API**: http://localhost:5001
- **Submissions API**: http://localhost:5002
- **AI API**: http://localhost:5003
- **Search API**: http://localhost:5004
- **Admin API**: http://localhost:5005
- **Payments API**: http://localhost:5006

## 🔧 Development

### Project Structure
```
backend/
├── src/
│   ├── Services/           # Microservices
│   │   ├── DSAGrind.Auth.API/
│   │   ├── DSAGrind.Problems.API/
│   │   ├── DSAGrind.Submissions.API/
│   │   ├── DSAGrind.AI.API/
│   │   ├── DSAGrind.Search.API/
│   │   ├── DSAGrind.Admin.API/
│   │   ├── DSAGrind.Payments.API/
│   │   └── DSAGrind.Gateway.API/
│   └── Shared/             # Shared libraries
│       ├── DSAGrind.Common/
│       ├── DSAGrind.Models/
│       └── DSAGrind.Events/
├── docker-compose.yml
└── .env.example

client/
├── src/
│   ├── components/         # Reusable UI components
│   ├── pages/             # Page components
│   ├── hooks/             # Custom React hooks
│   ├── contexts/          # React contexts
│   ├── types/             # TypeScript type definitions
│   └── lib/               # Utility libraries
└── package.json
```

### Adding New Features

1. **Backend Service**
   - Create new service in `src/Services/`
   - Implement repository pattern with MongoDB
   - Add service registration in DI container
   - Configure routing in API Gateway

2. **Frontend Integration**
   - Add API types in `src/types/api.ts`
   - Create custom hooks in `src/hooks/api/`
   - Implement UI components with Shadcn

### Database Schema

The application uses MongoDB with the following main collections:
- **Users**: User authentication and profile data
- **Problems**: Coding problems with test cases and solutions
- **Categories**: Problem categorization
- **Submissions**: User code submissions and results
- **Subscriptions**: Payment and subscription data
- **AuditLogs**: Admin action tracking

## 🔐 Security Features

- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **OAuth2 Integration**: Google and GitHub social login
- **Rate Limiting**: API rate limiting to prevent abuse
- **Input Validation**: Comprehensive input validation with FluentValidation
- **Code Sandboxing**: Docker-based isolation for code execution
- **HTTPS Enforcement**: SSL/TLS encryption for all communications
- **CORS Configuration**: Secure cross-origin resource sharing

## 📊 Monitoring & Observability

- **Health Checks**: Built-in health check endpoints for all services
- **Structured Logging**: Serilog with structured logging to files and console
- **Error Tracking**: Comprehensive error handling and logging
- **Performance Metrics**: Service response time monitoring
- **Admin Dashboard**: Real-time system health and metrics

## 🚀 Deployment

### Production Deployment

1. **Environment Configuration**
```bash
# Set production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export MONGODB_CONNECTION_STRING=<production-mongodb-url>
export OPENAI_API_KEY=<production-openai-key>
export STRIPE_SECRET_KEY=<production-stripe-key>
```

2. **Docker Production Build**
```bash
# Build for production
docker-compose -f docker-compose.prod.yml up -d
```

3. **Database Migration**
```bash
# MongoDB indexes and initial data
./scripts/setup-database.sh
```

### Scaling Considerations

- **Horizontal Scaling**: All services are stateless and can be scaled horizontally
- **Database Sharding**: MongoDB can be sharded for large datasets
- **Caching Strategy**: Redis cluster for high-availability caching
- **Load Balancing**: API Gateway handles load balancing across service instances

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Comprehensive API documentation available at `/swagger`
- **Issues**: Report bugs and feature requests via GitHub Issues
- **Discord**: Join our community Discord server for real-time support

## 🚧 Roadmap

- [ ] WebSocket integration for real-time collaboration
- [ ] Contest/Tournament management system
- [ ] Advanced analytics and insights
- [ ] Mobile app development
- [ ] Integration with popular coding platforms
- [ ] Enhanced AI features with custom models

---

Built with ❤️ using modern technologies for the next generation of competitive programming education.