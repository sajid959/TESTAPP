# DSA Grind - Complete Setup Guide

## 🚀 Project Overview

DSA Grind is a comprehensive coding practice platform similar to LeetCode, built with modern web technologies. It features:

- **Frontend**: React 18 + TypeScript + Vite + TailwindCSS + Shadcn/ui
- **Backend**: Node.js + Express + TypeScript
- **Database**: PostgreSQL with Drizzle ORM
- **Real-time**: WebSocket support for live collaboration
- **Authentication**: JWT-based auth with OAuth integration
- **Payments**: Stripe integration for premium subscriptions
- **AI Integration**: Perplexity API for hints and code assistance
- **Code Editor**: Monaco Editor (VS Code editor)

## 📋 Tech Stack Summary

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
- **Node.js 20** - Runtime Environment
- **Express.js** - Web Framework
- **TypeScript** - Type Safety
- **Drizzle ORM** - Database ORM
- **PostgreSQL** - Primary Database
- **JWT** - Authentication Tokens
- **bcryptjs** - Password Hashing
- **WebSocket (ws)** - Real-time Communication
- **Multer** - File Upload Handling
- **XLSX** - Excel File Processing

### External Services
- **PostgreSQL Database** - Primary data storage
- **Stripe** - Payment processing (optional)
- **Perplexity API** - AI-powered hints and assistance (optional)
- **Email Service** - User notifications (mock implementation included)

## 🛠️ Local Development Setup

### Prerequisites

Ensure you have these installed:
- **Node.js 20+** (LTS recommended)
- **npm** or **yarn** package manager
- **PostgreSQL 12+** database
- **Git** version control

### 1. Project Setup

```bash
# Clone the repository
git clone <your-repo-url>
cd dsagrind

# Install dependencies
npm install

# Alternative with yarn
yarn install
```

### 2. Environment Variables

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

### 3. Database Setup

#### Option A: Local PostgreSQL Installation

1. **Install PostgreSQL**:
   ```bash
   # Ubuntu/Debian
   sudo apt update
   sudo apt install postgresql postgresql-contrib
   
   # macOS with Homebrew
   brew install postgresql
   brew services start postgresql
   
   # Windows: Download from postgresql.org
   ```

2. **Create Database**:
   ```bash
   # Connect to PostgreSQL
   sudo -u postgres psql
   
   # Create database and user
   CREATE DATABASE dsagrind;
   CREATE USER dsagrind_user WITH PASSWORD 'your_password';
   GRANT ALL PRIVILEGES ON DATABASE dsagrind TO dsagrind_user;
   \q
   ```

3. **Update DATABASE_URL** in `.env`:
   ```env
   DATABASE_URL="postgresql://dsagrind_user:your_password@localhost:5432/dsagrind"
   ```

#### Option B: Docker PostgreSQL (Easier)

1. **Create docker-compose.yml**:
   ```yaml
   version: '3.8'
   services:
     postgres:
       image: postgres:15
       container_name: dsagrind_db
       environment:
         POSTGRES_DB: dsagrind
         POSTGRES_USER: dsagrind_user
         POSTGRES_PASSWORD: your_password
       ports:
         - "5432:5432"
       volumes:
         - postgres_data:/var/lib/postgresql/data
   
   volumes:
     postgres_data:
   ```

2. **Start Database**:
   ```bash
   docker-compose up -d
   ```

#### Option C: Cloud Database (Neon, Supabase, etc.)

1. **Sign up for a cloud PostgreSQL provider**:
   - **Neon** (recommended): https://neon.tech
   - **Supabase**: https://supabase.com
   - **Railway**: https://railway.app
   - **PlanetScale**: https://planetscale.com

2. **Create database and copy connection string to `.env`**

### 4. Database Schema Setup

```bash
# Push schema to database (creates all tables)
npm run db:push

# This will create all the necessary tables:
# - users, categories, problems, submissions
# - user_progress, contests, ai_chats, settings
```

### 5. Start Development Server

```bash
# Start both frontend and backend
npm run dev

# The application will be available at:
# - Frontend: http://localhost:5000
# - Backend API: http://localhost:5000/api
# - WebSocket: ws://localhost:5000/ws
```

### 6. Default Admin Account

The application automatically creates a default admin account:
- **Email**: admin@dsagrind.com
- **Password**: admin123

**Important**: Change this password immediately in production!

## 🔧 Optional Integrations Setup

### Stripe Payment Integration

1. **Create Stripe Account**: https://stripe.com
2. **Get API Keys** from Stripe Dashboard
3. **Add to .env**:
   ```env
   STRIPE_SECRET_KEY="sk_test_..."
   STRIPE_PUBLISHABLE_KEY="pk_test_..."
   ```
4. **Configure Webhooks** (for production):
   - Webhook URL: `https://yourdomain.com/api/webhooks/stripe`
   - Events: `customer.subscription.created`, `customer.subscription.updated`, `customer.subscription.deleted`

### Perplexity AI Integration

1. **Get API Key**: https://www.perplexity.ai/settings/api
2. **Add to .env**:
   ```env
   PERPLEXITY_API_KEY="pplx-..."
   ```

### OAuth Integration (GitHub, Google)

The application supports OAuth but requires additional setup:

1. **GitHub OAuth**:
   - Create GitHub App: https://github.com/settings/developers
   - Add callback URL: `http://localhost:5000/auth/github/callback`
   - Add CLIENT_ID and CLIENT_SECRET to .env

2. **Google OAuth**:
   - Create Google Cloud Project: https://console.cloud.google.com
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Add callback URL: `http://localhost:5000/auth/google/callback`

## 📁 Project Structure

```
dsagrind/
├── client/                 # Frontend React application
│   ├── src/
│   │   ├── components/     # Reusable UI components
│   │   ├── pages/          # Page components
│   │   ├── contexts/       # React contexts
│   │   ├── hooks/          # Custom React hooks
│   │   ├── lib/            # Utility functions
│   │   └── types/          # TypeScript type definitions
│   └── index.html          # HTML entry point
├── server/                 # Backend Express application
│   ├── services/           # Business logic services
│   ├── routes.ts           # API route definitions
│   ├── storage.ts          # Database operations
│   ├── index.ts            # Server entry point
│   └── seeds.ts            # Database seed data
├── shared/                 # Shared code between frontend/backend
│   └── schema.ts           # Database schema & types
├── package.json            # Dependencies and scripts
├── vite.config.ts          # Vite configuration
├── drizzle.config.ts       # Database configuration
├── tailwind.config.ts      # TailwindCSS configuration
└── tsconfig.json           # TypeScript configuration
```

## 🚀 Production Deployment

### Environment Variables for Production

```env
NODE_ENV="production"
DATABASE_URL="your-production-database-url"
JWT_SECRET="your-secure-production-jwt-secret"
JWT_REFRESH_SECRET="your-secure-production-refresh-secret"
FRONTEND_URL="https://yourdomain.com"
```

### Build Process

```bash
# Build the application
npm run build

# Start production server
npm start
```

### Deployment Options

1. **Replit** (Current): Already configured
2. **Vercel**: Add `vercel.json` configuration
3. **Netlify**: Configure build settings
4. **Railway**: Connect GitHub repository
5. **DigitalOcean App Platform**: Use App Spec

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Errors**:
   ```bash
   # Check if PostgreSQL is running
   sudo service postgresql status
   
   # Test connection
   psql $DATABASE_URL
   ```

2. **Port Already in Use**:
   ```bash
   # Kill process on port 5000
   lsof -ti:5000 | xargs kill -9
   ```

3. **TypeScript Errors**:
   ```bash
   # Type check
   npm run check
   
   # Clear node_modules and reinstall
   rm -rf node_modules package-lock.json
   npm install
   ```

4. **Database Schema Issues**:
   ```bash
   # Reset database schema
   npm run db:push --force
   ```

### Development Commands

```bash
# Development
npm run dev              # Start development server
npm run check           # Type checking
npm run db:push         # Push schema to database

# Production
npm run build           # Build for production
npm start               # Start production server

# Database
npm run db:push         # Sync database schema
```

## 📊 Database Schema Overview

The application uses the following main tables:

- **users** - User accounts and profiles
- **categories** - Problem categories (Arrays, Trees, etc.)
- **problems** - Coding problems with test cases
- **submissions** - User code submissions
- **user_progress** - Track user solving progress
- **contests** - Coding contests/challenges
- **ai_chats** - AI conversation history
- **settings** - Application configuration

## 🔐 Security Features

- JWT-based authentication
- Password hashing with bcrypt
- SQL injection prevention with Drizzle ORM
- CORS configuration
- Rate limiting (can be added)
- Input validation with Zod schemas

## 🧪 Testing

The application includes basic error handling and logging. You can add testing with:

```bash
# Add testing dependencies
npm install --save-dev jest @testing-library/react @testing-library/jest-dom
```

## 📝 API Documentation

Main API endpoints:

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/problems` - List problems
- `POST /api/submissions` - Submit solution
- `GET /api/admin/users` - Admin: List users
- `POST /api/admin/problems` - Admin: Create problem

WebSocket events:
- `ping/pong` - Keep connection alive
- `join_problem` - Join problem room
- `code_update` - Real-time code sharing
- `submission_status` - Live submission updates

This setup guide covers everything you need to run the DSA Grind application locally or in production. The application is already working on Replit with the database properly configured.