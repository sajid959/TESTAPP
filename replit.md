# DSAGrind - Competitive Programming Platform

## Project Overview
DSAGrind is a comprehensive competitive programming platform with microservices architecture, featuring AI-powered features, multi-language IDE, admin management, OAuth authentication, and complete problem management system.

## Architecture
- **Frontend**: React + Vite + Tailwind CSS + Shadcn UI
- **Backend**: Node.js Express API with microservices pattern
- **Database**: MongoDB Atlas (free tier)
- **Queue**: Redis with BullMQ for async tasks
- **Search**: Vector search with embeddings
- **Authentication**: JWT + OAuth (Google, GitHub)
- **Email**: Email verification system
- **IDE**: Multi-language code execution environment
- **Admin**: Full-featured admin dashboard

## Key Features
1. **Authentication & Authorization**
   - JWT + Refresh tokens
   - OAuth integration (Google, GitHub)
   - Email verification
   - Role-based access control (User, Admin)

2. **Problem Management**
   - Category-based organization
   - Bulk Excel/CSV import
   - Admin UI for problem creation
   - Difficulty levels and tags
   - Test cases (visible/hidden)

3. **Multi-Language IDE**
   - Monaco editor integration
   - Support for multiple programming languages
   - Real-time code execution
   - Test case validation
   - Submission tracking

4. **Admin Dashboard**
   - User management
   - Problem approval/management
   - Category management
   - Excel upload for bulk problems
   - Analytics and insights

5. **Modern UI/UX**
   - Responsive design
   - Dark/light mode
   - Unique and modern interface
   - Professional styling

## User Preferences
- Use free tiers for all services (MongoDB Atlas, Confluent Kafka, etc.)
- Implement OAuth with Google and GitHub
- Email verification required
- Excel/CSV bulk upload for problems
- Multi-language IDE similar to LeetCode
- Reference AlgoMonster site features
- Separate key storage and environment variables
- Modern and unique UI design

## Recent Changes
- ✓ Initial project setup
- → Setting up authentication system
- → Building problem management
- → Creating admin dashboard
- → Implementing IDE