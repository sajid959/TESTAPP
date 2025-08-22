# DSAGrind - Competitive Programming Platform Requirements

## Project Overview
DSAGrind is a comprehensive competitive programming platform designed to help developers practice coding problems with AI-powered assistance, similar to LeetCode but with enhanced features.

## Core Features

### 1. Authentication System
- **JWT Authentication** with refresh tokens
- **OAuth Integration** (Google, GitHub)
- **Email Verification** required for account activation
- **Password Reset** functionality
- **Role-based Access Control** (User, Admin)

### 2. Problem Management
- **Category-based Organization** (Arrays, Trees, Dynamic Programming, etc.)
- **Difficulty Levels** (Easy, Medium, Hard)
- **Tags System** for better discoverability
- **Premium Problems** with subscription model
- **Test Cases** (visible and hidden)
- **Solution Templates** in multiple languages

### 3. Admin Dashboard
- **Problem Creation** via UI form
- **Category Management** with free problem limits
- **Excel/CSV Bulk Upload** for problems
  - Required columns: title, description, examples, constraints, tags, difficulty, testcases, isPaid, categorySlug
  - AI-powered data validation and filling
  - Import summary reports
- **User Management** and analytics
- **Content Moderation** tools

### 4. Multi-Language IDE
- **Monaco Editor** integration (VS Code engine)
- **Language Support**: Python, JavaScript, Java, C++, C#, Go, Rust
- **Real-time Code Execution** with Judge0 API
- **Test Case Validation**
- **Submission History** tracking
- **Code Templates** for each language

### 5. AI-Powered Features
- **Perplexity AI Integration** for hints and explanations
- **Smart Hints** without giving away solutions
- **Code Review** and optimization suggestions
- **Auto-generated Examples** for problems
- **Difficulty Assessment** for uploaded problems

### 6. Search & Discovery
- **Vector Search** with Qdrant for semantic problem matching
- **Advanced Filters** (difficulty, tags, acceptance rate)
- **Recommendation Engine** based on solving patterns
- **Progress Tracking** across categories

### 7. Subscription & Payments
- **Stripe Integration** for payments
- **Freemium Model** with limited free problems per category
- **Monthly/Annual Plans** for premium access
- **Progress Analytics** for subscribers

### 8. Email System
- **SendGrid Integration** for transactional emails
- **Welcome Emails** after verification
- **Password Reset** emails
- **Progress Reports** and notifications

## Technical Architecture

### Frontend
- **Framework**: React 18 + TypeScript
- **Styling**: Tailwind CSS + Shadcn UI components
- **State Management**: TanStack Query + Context API
- **Routing**: Wouter (lightweight routing)
- **Editor**: Monaco Editor
- **Forms**: React Hook Form + Zod validation

### Backend
- **Architecture**: .NET 8 Microservices
- **API Gateway**: YARP Reverse Proxy
- **Services**:
  - Auth API (Port 8080)
  - Problems API (Port 5001)
  - Submissions API (Port 5002)
  - AI API (Port 5003)
  - Search API (Port 5004)
  - Admin API (Port 5005)
  - Payments API (Port 5006)

### Databases & Infrastructure
- **Primary Database**: MongoDB Atlas (free tier)
- **Caching**: Redis Cloud (free tier)
- **Message Queue**: Kafka (Confluent Cloud free tier)
- **Vector Database**: Qdrant Cloud (free tier)
- **Email Service**: SendGrid (free tier)
- **Payment Processing**: Stripe (test mode)

### AI & External APIs
- **Primary AI**: Perplexity API
- **Backup AI**: OpenAI GPT-3.5
- **Code Execution**: Judge0 API
- **OAuth Providers**: Google, GitHub

## Development Requirements

### Excel/CSV Upload Format
```csv
title,description,difficulty,categorySlug,tags,isPremium,constraints,examples,testCases,solution,hints
"Two Sum","Given array find two numbers that add up to target","Easy","arrays-hashing","Array,Hash Table","false","2 ≤ nums.length ≤ 10⁴","[{""input"":""nums = [2,7,11,15], target = 9"",""output"":""[0,1]""}]","[{""input"":{""nums"":[2,7,11,15],""target"":9},""output"":[0,1]}]","Python solution code","Hint about using hash map"
```

### Environment Variables
- MongoDB connection string
- Redis connection string
- Kafka configuration
- API keys (Perplexity, OpenAI, Stripe, SendGrid)
- OAuth credentials (Google, GitHub)
- JWT secrets

### Security Requirements
- HTTPS enforcement in production
- Rate limiting on all APIs
- Input validation and sanitization
- Secure secret management
- CORS configuration
- JWT token expiration and refresh

### Performance Requirements
- Page load times < 2 seconds
- Code execution results < 5 seconds
- API response times < 500ms
- Real-time updates for submissions
- Efficient caching strategies

## User Experience

### Free Users
- Access to limited problems per category
- Basic IDE functionality
- Community solutions after solving
- Progress tracking

### Premium Users
- Unlimited access to all problems
- Advanced analytics and insights
- Priority AI assistance
- Interview preparation tracks
- Export progress reports

### Admin Users
- Full dashboard access
- Content management capabilities
- User analytics and management
- Bulk operations and imports
- System monitoring tools

## Success Metrics
- User engagement and retention
- Problem solving completion rates
- Subscription conversion rates
- Code execution success rates
- AI assistance effectiveness
- Platform performance metrics