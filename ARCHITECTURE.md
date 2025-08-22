# DSAGrind - System Architecture Documentation

## 🏗️ Overview
DSAGrind is a modern competitive programming platform built with a microservices architecture, featuring AI-powered assistance, real-time collaboration, and comprehensive administrative tools.

## 🔧 Technical Stack

### Frontend
- **Framework**: React 18 + Vite
- **UI Library**: Tailwind CSS + Shadcn UI components
- **State Management**: TanStack Query + Context API
- **Routing**: Wouter (lightweight routing)
- **Editor**: Monaco Editor (VS Code engine)
- **Real-time**: WebSocket API
- **Authentication**: JWT with refresh tokens

### Backend
- **Runtime**: .NET 8 (Latest LTS)
- **Architecture**: Microservices with API Gateway
- **Database**: MongoDB Atlas (Cloud NoSQL)
- **Messaging**: Apache Kafka (Event-driven)
- **Caching**: Redis (In-memory cache)
- **AI**: OpenAI GPT-3.5 Turbo + Semantic Kernel
- **Search**: Vector embeddings + Qdrant
- **Authentication**: JWT + OAuth (Google, GitHub)
- **Payments**: Stripe integration

## 🏛️ Microservices Architecture

```
┌─────────────────────┐
│   Frontend (React)  │
│     Port: 5000      │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   Node.js Proxy     │
│     Port: 5000      │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│  .NET API Gateway   │
│     Port: 8000      │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│    Microservices    │
├─────────────────────┤
│ Auth API     :8080  │
│ Problems API :5001  │
│ Submit API   :5002  │
│ AI API       :5003  │
│ Search API   :5004  │
│ Admin API    :5005  │
│ Payments API :5006  │
└─────────────────────┘
           │
┌──────────▼──────────┐
│   External Services │
├─────────────────────┤
│ MongoDB Atlas       │
│ Apache Kafka        │
│ Redis Cache         │
│ OpenAI API          │
│ Stripe Payments     │
│ OAuth Providers     │
└─────────────────────┘
```

## 🚀 Service Details

### 1. Gateway API (Port 8000)
**Purpose**: Main entry point and reverse proxy
- Routes requests to appropriate microservices
- Handles CORS and security policies
- Load balancing and health checks
- Rate limiting (configurable)

### 2. Auth API (Port 8080)
**Purpose**: Authentication and user management
- JWT token generation/validation
- OAuth integration (Google, GitHub)
- User registration/login
- Password reset functionality
- Role-based access control

### 3. Problems API (Port 5001)
**Purpose**: Problem and category management
- CRUD operations for problems
- Category management
- Test case handling
- Difficulty estimation
- Problem recommendations

### 4. Submissions API (Port 5002)
**Purpose**: Code execution and submissions
- Multi-language code execution
- Test case validation
- Submission tracking
- Performance metrics
- Execution history

### 5. AI API (Port 5003)
**Purpose**: AI-powered assistance
- Code analysis and review
- Hint generation
- Solution explanations
- Test case generation
- Debugging assistance
- Code optimization suggestions

### 6. Search API (Port 5004)
**Purpose**: Advanced search and recommendations
- Vector search with embeddings
- Problem discovery
- Personalized recommendations
- Tag-based filtering
- Semantic search

### 7. Admin API (Port 5005)
**Purpose**: Administrative functions
- User management
- Content moderation
- System analytics
- Bulk operations
- Configuration management

### 8. Payments API (Port 5006)
**Purpose**: Subscription and payment handling
- Stripe integration
- Subscription management
- Payment processing
- Billing history
- Plan upgrades/downgrades

## 📊 Data Flow Architecture

### Event-Driven Communication
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Service   │───▶│    Kafka    │───▶│   Service   │
│  Producer   │    │   Message   │    │  Consumer   │
│             │    │    Queue    │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
```

### Event Types
- **UserEvents**: Registration, login, logout, profile updates
- **ProblemEvents**: Creation, updates, submissions
- **SubmissionEvents**: Code execution, results, scoring
- **PaymentEvents**: Subscriptions, renewals, cancellations

## 🔐 Security Architecture

### Authentication Flow
```
1. User Login Request
   ↓
2. Verify Credentials
   ↓
3. Generate JWT + Refresh Token
   ↓
4. Store in HTTP-only Cookie
   ↓
5. API Requests with Bearer Token
   ↓
6. JWT Validation on Each Request
```

### Authorization Matrix
| Role     | Problems | Submissions | Admin | Payments |
|----------|----------|-------------|-------|----------|
| Guest    | Read     | -           | -     | -        |
| User     | Read     | CRUD        | -     | Read     |
| Premium  | Full     | CRUD        | -     | CRUD     |
| Admin    | Full     | Full        | Full  | Full     |

## 🧠 AI Integration Architecture

### AI Service Pipeline
```
User Request
    ↓
AI Service
    ↓
OpenAI API (GPT-3.5)
    ↓
Response Processing
    ↓
Structured Output
    ↓
Cache in Redis
    ↓
Return to User
```

### AI Features
- **Code Analysis**: Complexity, quality, best practices
- **Debugging**: Error detection and suggestions
- **Hints**: Progressive difficulty hints
- **Test Generation**: Edge cases and boundary conditions
- **Optimization**: Performance and readability improvements

## 📈 Scalability Considerations

### Horizontal Scaling
- Each microservice can scale independently
- Load balancing at Gateway level
- Database sharding strategies
- Kafka topic partitioning

### Performance Optimizations
- Redis caching for frequent queries
- CDN for static assets
- Database indexing strategies
- Lazy loading and pagination

### Monitoring & Observability
- Structured logging with Serilog
- Health checks for all services
- Metrics collection and alerting
- Distributed tracing

## 🔄 Development Workflow

### Local Development
1. Start infrastructure (MongoDB, Kafka, Redis)
2. Run microservices in parallel
3. Frontend development server
4. Hot reload for rapid iteration

### Deployment Pipeline
1. Code commit triggers CI/CD
2. Automated testing suite
3. Docker containerization
4. Kubernetes deployment
5. Health check validation

## 📝 API Documentation

### RESTful Endpoints
- **GET /api/problems** - List problems
- **POST /api/submissions** - Submit code
- **GET /api/ai/hints** - Get AI hints
- **POST /api/admin/problems** - Admin operations

### WebSocket Events
- **code:update** - Real-time code changes
- **submission:result** - Execution results
- **hint:available** - AI hint ready

## 🛠️ Configuration Management

### Environment Variables
```bash
# Database
MONGODB_CONNECTION_STRING=mongodb+srv://...
REDIS_CONNECTION_STRING=redis://...

# External APIs
OPENAI_API_KEY=sk-...
STRIPE_SECRET_KEY=sk_test_...

# Kafka
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
```

### Service Configuration
Each service has its own `appsettings.json` with:
- Database connections
- API endpoints
- Security settings
- Feature flags