# DSAGrind - Competitive Programming Platform

## Overview

DSAGrind is a comprehensive competitive programming platform designed to help developers practice coding problems with AI-powered assistance. The platform features a microservices architecture built with .NET 8 backend and React TypeScript frontend, providing multi-language IDE support, real-time code execution, AI-powered hints, admin management capabilities, OAuth authentication, and premium subscription features.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Frontend Architecture
- **Framework**: React 18 with TypeScript for type safety
- **Build System**: Vite for fast development and optimized production builds
- **Routing**: Wouter for lightweight client-side routing
- **State Management**: TanStack Query for server state management with Context API for global application state
- **UI Framework**: Tailwind CSS with Shadcn UI component library for consistent design system
- **Code Editor**: Monaco Editor (VS Code engine) for multi-language code editing
- **Authentication**: JWT-based authentication with refresh token rotation
- **Forms**: React Hook Form with Zod validation for type-safe form handling

### Backend Architecture
- **Microservices Design**: 8 independent services communicating through an API Gateway
- **API Gateway**: YARP reverse proxy handling routing, rate limiting, and load balancing
- **Authentication Service**: JWT + OAuth (Google, GitHub) with refresh token management
- **Problems Service**: CRUD operations for coding problems and categories
- **Submissions Service**: Code execution engine with Docker sandboxing
- **AI Service**: OpenAI integration for hints, explanations, and problem analysis
- **Search Service**: Vector search using Qdrant for semantic problem discovery
- **Admin Service**: Content management and user administration
- **Payments Service**: Stripe integration for subscription management

### Data Layer
- **Primary Database**: MongoDB Atlas for document storage with flexible schema
- **Caching**: Redis for session management, query caching, and real-time data
- **Message Queue**: Apache Kafka for event-driven communication between services
- **Vector Database**: Qdrant for semantic search and problem recommendations

### Authentication & Authorization
- **JWT Tokens**: Access tokens (15-60 min) with refresh tokens (7-30 days)
- **OAuth Integration**: Google and GitHub OAuth with PKCE flow
- **Role-based Access**: User, Premium, Admin roles with granular permissions
- **Session Management**: Distributed sessions with Redis backing

### Code Execution Engine
- **Sandboxing**: Docker containers for secure code execution
- **Multi-language Support**: Python, JavaScript, Java, C++, C#, Go, Rust, PHP
- **Test Case Validation**: Hidden and visible test cases with performance metrics
- **Real-time Results**: WebSocket-based live execution feedback

## External Dependencies

### Third-Party Services
- **MongoDB Atlas**: Cloud NoSQL database for primary data storage
- **Redis Cloud**: In-memory caching and session store
- **Stripe**: Payment processing for premium subscriptions
- **OpenAI API**: AI-powered hints, explanations, and code analysis
- **Qdrant**: Vector database for semantic search capabilities
- **SendGrid**: Email service for user verification and notifications

### Development & Infrastructure
- **Docker**: Containerization for consistent deployment environments
- **Apache Kafka**: Message broker for microservices communication
- **YARP**: .NET reverse proxy for API Gateway functionality
- **Monaco Editor**: Web-based code editor with syntax highlighting
- **Font Awesome**: Icon library for UI components

### Authentication Providers
- **Google OAuth 2.0**: Social login integration
- **GitHub OAuth**: Developer-focused authentication
- **JWT**: Self-contained token-based authentication

### Monitoring & Logging
- **Serilog**: Structured logging across all .NET services
- **Application Insights**: Performance monitoring and telemetry (production)

### Frontend Dependencies
- **Radix UI**: Headless UI components for accessibility
- **Framer Motion**: Animation library for smooth interactions
- **React Hook Form**: Form state management with validation
- **Recharts**: Data visualization for analytics dashboards