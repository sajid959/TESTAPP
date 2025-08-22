# DSAGrind User Flows

## Overview
This document outlines the key user flows for the DSAGrind competitive programming platform, covering different user types and their interactions with the system.

## User Types

### 1. Anonymous Users (Visitors)
- Can browse the landing page
- View problem categories and sample problems
- See pricing and features
- Cannot solve problems or access IDE

### 2. Free Users (Registered)
- Full access to free problems in each category
- Limited AI assistance
- Basic progress tracking
- Can upgrade to premium

### 3. Premium Users (Subscribers)
- Unlimited access to all problems
- Full AI assistance and hints
- Advanced analytics and insights
- Priority support

### 4. Admin Users
- All premium features
- Access to admin dashboard
- Content management capabilities
- User management and analytics

## Authentication Flows

### 1. User Registration Flow

```mermaid
graph TD
    A[Landing Page] --> B[Click Sign Up]
    B --> C[Registration Form]
    C --> D{Form Valid?}
    D -->|No| C
    D -->|Yes| E[Submit Registration]
    E --> F[Email Verification Sent]
    F --> G[Check Email]
    G --> H[Click Verification Link]
    H --> I[Email Verified]
    I --> J[Welcome Page]
    J --> K[Onboarding Tutorial]
```

**Steps:**
1. User clicks "Sign Up" on landing page
2. Fills registration form (email, username, password, name)
3. Form validation (client-side and server-side)
4. Account created with unverified status
5. Verification email sent via SendGrid
6. User clicks verification link in email
7. Account status updated to verified
8. Welcome email sent
9. User redirected to onboarding

**Technical Implementation:**
- React Hook Form with Zod validation
- JWT token generation for session
- Email verification token with expiration
- Welcome email template with SendGrid

### 2. OAuth Login Flow (Google/GitHub)

```mermaid
graph TD
    A[Login Page] --> B[Click OAuth Provider]
    B --> C[Redirect to Provider]
    C --> D[User Authorizes]
    D --> E[Callback with Auth Code]
    E --> F[Exchange for Access Token]
    F --> G[Fetch User Profile]
    G --> H{User Exists?}
    H -->|Yes| I[Login User]
    H -->|No| J[Create Account]
    J --> I
    I --> K[Generate JWT]
    K --> L[Redirect to Dashboard]
```

**Steps:**
1. User clicks "Continue with Google/GitHub"
2. Redirect to OAuth provider
3. User authorizes application
4. Callback with authorization code
5. Exchange code for access token
6. Fetch user profile from provider
7. Check if user exists in database
8. Create account if new user, login if existing
9. Generate JWT tokens
10. Redirect to appropriate page

### 3. Password Reset Flow

```mermaid
graph TD
    A[Forgot Password Link] --> B[Enter Email]
    B --> C{Email Exists?}
    C -->|No| D[Show Generic Success]
    C -->|Yes| E[Generate Reset Token]
    E --> F[Send Reset Email]
    F --> D
    D --> G[Check Email]
    G --> H[Click Reset Link]
    H --> I[Reset Password Form]
    I --> J[Submit New Password]
    J --> K[Password Updated]
    K --> L[Auto Login]
```

## Problem Solving Flows

### 1. Browse Problems Flow

```mermaid
graph TD
    A[Dashboard/Home] --> B[Browse Problems]
    B --> C[Select Category]
    C --> D[View Category Problems]
    D --> E[Filter/Search]
    E --> F[Select Problem]
    F --> G{User Access?}
    G -->|Free Problem| H[Open IDE]
    G -->|Premium Required| I[Upgrade Prompt]
    I --> J[Subscription Flow]
    J --> H
    H --> K[Start Solving]
```

**Steps:**
1. User navigates to Problems section
2. Views available categories (Arrays, Trees, etc.)
3. Selects a category
4. Sees list of problems with difficulty indicators
5. Uses filters (difficulty, tags, status)
6. Clicks on a problem
7. Access check based on user tier
8. Either opens IDE or shows upgrade prompt

### 2. Problem Solving Flow

```mermaid
graph TD
    A[Open Problem] --> B[Read Description]
    B --> C[Understand Examples]
    C --> D[Choose Language]
    D --> E[Write Code]
    E --> F[Test with Examples]
    F --> G{Tests Pass?}
    G -->|No| H[Debug/Revise]
    H --> E
    G -->|Yes| I[Submit Solution]
    I --> J[Run All Test Cases]
    J --> K{All Pass?}
    K -->|No| L[Show Failed Cases]
    L --> H
    K -->|Yes| M[Solution Accepted]
    M --> N[Update Progress]
    N --> O[Show Statistics]
```

**Steps:**
1. Problem description loaded with examples
2. User selects programming language
3. Monaco editor initialized with template
4. User writes solution
5. Test with visible examples
6. Submit for full evaluation
7. Server runs against all test cases
8. Results displayed with performance metrics
9. Progress updated if successful

### 3. AI Assistance Flow

```mermaid
graph TD
    A[Stuck on Problem] --> B[Click AI Hint]
    B --> C{User Tier?}
    C -->|Free| D[Limited Hints]
    C -->|Premium| E[Full AI Access]
    D --> F[Show Basic Hint]
    E --> G[Perplexity API Call]
    G --> H[Generate Contextual Hint]
    H --> I[Display Hint]
    I --> J[Continue Solving]
    F --> J
    J --> K{Want More Help?}
    K -->|Yes| L[Next Hint Level]
    L --> G
    K -->|No| M[Back to Code]
```

**Steps:**
1. User clicks "Get Hint" button
2. System checks user's subscription tier
3. For premium users, calls Perplexity API
4. AI generates hint based on problem context
5. Hint displayed without giving away solution
6. User can request progressive hints
7. Each hint level becomes more specific

## Admin Flows

### 1. Problem Creation Flow

```mermaid
graph TD
    A[Admin Dashboard] --> B[Click Add Problem]
    B --> C[Problem Form]
    C --> D[Fill Basic Info]
    D --> E[Add Test Cases]
    E --> F[Write Solution]
    F --> G[Preview Problem]
    G --> H{Looks Good?}
    H -->|No| C
    H -->|Yes| I[Submit Problem]
    I --> J[Validation]
    J --> K{Valid?}
    K -->|No| L[Show Errors]
    L --> C
    K -->|Yes| M[Save to Database]
    M --> N[Publish/Draft]
```

**Steps:**
1. Admin navigates to problem management
2. Clicks "Add New Problem"
3. Fills form: title, description, difficulty, category
4. Adds examples and constraints
5. Creates test cases (visible and hidden)
6. Writes reference solution
7. Previews how problem appears to users
8. Submits for validation
9. Server validates all fields and test cases
10. Problem saved and published

### 2. Bulk Import Flow

```mermaid
graph TD
    A[Admin Dashboard] --> B[Click Bulk Import]
    B --> C[Download Template]
    C --> D[Fill Excel/CSV]
    D --> E[Upload File]
    E --> F[File Validation]
    F --> G{Valid Format?}
    G -->|No| H[Show Format Errors]
    H --> E
    G -->|Yes| I[Parse Problems]
    I --> J[AI Data Enhancement]
    J --> K[Preview Import]
    K --> L{Approve?}
    L -->|No| M[Edit Problems]
    M --> K
    L -->|Yes| N[Bulk Insert]
    N --> O[Import Summary]
```

**Steps:**
1. Admin clicks "Bulk Import"
2. Downloads CSV template with required columns
3. Fills template with problem data
4. Uploads completed file
5. System validates file format and structure
6. Parses each row into problem objects
7. AI fills missing data (Perplexity API)
8. Shows preview of all problems to import
9. Admin reviews and approves
10. Bulk insert into database
11. Summary report with success/failure counts

### 3. User Management Flow

```mermaid
graph TD
    A[Admin Dashboard] --> B[User Management]
    B --> C[Search/Filter Users]
    C --> D[Select User]
    D --> E[View User Profile]
    E --> F[Available Actions]
    F --> G[Edit Profile]
    F --> H[Change Subscription]
    F --> I[Ban/Suspend]
    F --> J[View Activity]
    G --> K[Save Changes]
    H --> K
    I --> K
    J --> L[Activity Report]
```

## Subscription Flows

### 1. Upgrade to Premium Flow

```mermaid
graph TD
    A[Hit Premium Limit] --> B[Upgrade Prompt]
    B --> C[View Plans]
    C --> D[Select Plan]
    D --> E[Stripe Checkout]
    E --> F[Payment Processing]
    F --> G{Payment Success?}
    G -->|No| H[Payment Failed]
    H --> E
    G -->|Yes| I[Update Subscription]
    I --> J[Send Confirmation]
    J --> K[Unlock Premium Features]
```

**Steps:**
1. User encounters premium problem or feature
2. Upgrade prompt displayed
3. User views available plans
4. Selects monthly or annual plan
5. Redirected to Stripe Checkout
6. Payment processed securely
7. Webhook confirms payment
8. User subscription upgraded in database
9. Confirmation email sent
10. Premium features unlocked

### 2. Subscription Management Flow

```mermaid
graph TD
    A[User Settings] --> B[Subscription Tab]
    B --> C[Current Plan Details]
    C --> D[Available Actions]
    D --> E[Change Plan]
    D --> F[Cancel Subscription]
    D --> G[Update Payment Method]
    E --> H[Stripe Portal]
    F --> I[Cancellation Survey]
    G --> H
    H --> J[Changes Processed]
    J --> K[Update Database]
    K --> L[Send Notification]
```

## Error Handling Flows

### 1. Network Error Flow

```mermaid
graph TD
    A[User Action] --> B[API Request]
    B --> C{Network Available?}
    C -->|No| D[Show Offline Message]
    D --> E[Queue Request]
    E --> F[Wait for Connection]
    F --> G[Retry Request]
    G --> C
    C -->|Yes| H[Process Request]
```

### 2. Authentication Error Flow

```mermaid
graph TD
    A[Protected Action] --> B[Check Token]
    B --> C{Token Valid?}
    C -->|No| D[Try Refresh Token]
    D --> E{Refresh Success?}
    E -->|No| F[Redirect to Login]
    E -->|Yes| G[Retry Original Request]
    C -->|Yes| H[Continue Action]
    G --> H
```

## Notification Flows

### 1. Email Notification Flow

```mermaid
graph TD
    A[Trigger Event] --> B[Check User Preferences]
    B --> C{Email Enabled?}
    C -->|No| D[Skip Email]
    C -->|Yes| E[Generate Email Content]
    E --> F[SendGrid API]
    F --> G{Send Success?}
    G -->|No| H[Log Error]
    G -->|Yes| I[Log Success]
    H --> J[Retry Later]
    I --> K[Update Delivery Status]
```

### 2. In-App Notification Flow

```mermaid
graph TD
    A[Event Occurs] --> B[Create Notification]
    B --> C[Store in Database]
    C --> D[Real-time Push]
    D --> E[Update UI Badge]
    E --> F[User Views Notifications]
    F --> G[Mark as Read]
    G --> H[Update Database]
```

## Analytics Flows

### 1. User Progress Tracking

```mermaid
graph TD
    A[User Solves Problem] --> B[Record Submission]
    B --> C[Update Statistics]
    C --> D[Calculate Progress]
    D --> E[Update Leaderboards]
    E --> F[Generate Insights]
    F --> G[Cache Results]
    G --> H[Update Dashboard]
```

### 2. Admin Analytics Flow

```mermaid
graph TD
    A[Admin Dashboard] --> B[Select Date Range]
    B --> C[Choose Metrics]
    C --> D[Query Database]
    D --> E[Aggregate Data]
    E --> F[Generate Charts]
    F --> G[Cache Results]
    G --> H[Display Analytics]
```

## Mobile Responsiveness

All flows are designed to work seamlessly across devices:

- **Desktop**: Full-featured experience with multi-panel layouts
- **Tablet**: Optimized layouts with collapsible sidebars
- **Mobile**: Touch-friendly interfaces with bottom navigation

## Performance Considerations

### 1. Loading States
- Skeleton screens during data fetching
- Progressive loading for large datasets
- Optimistic updates for better UX

### 2. Caching Strategy
- Redis caching for frequently accessed data
- Browser caching for static assets
- Service worker for offline functionality

### 3. Real-time Updates
- WebSocket connections for live updates
- Server-sent events for notifications
- Optimistic UI updates

This comprehensive user flow documentation ensures consistent and intuitive user experiences across all features of the DSAGrind platform.