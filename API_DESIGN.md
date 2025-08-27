# DSAGrind Platform - API Design Documentation

## 📋 Document Information
- **Version**: 1.0
- **Created**: 2024
- **System**: DSAGrind Competitive Programming Platform
- **Focus**: Comprehensive RESTful API Design
- **API Style**: REST with GraphQL considerations for complex queries

---

## 🎯 API Architecture Overview

### API Design Principles
1. **RESTful Design**: Standard HTTP methods and status codes
2. **Resource-Oriented**: Clear resource hierarchies and relationships
3. **Stateless**: Each request contains all necessary information
4. **Versioned**: API versioning for backward compatibility
5. **Consistent**: Uniform response formats and error handling
6. **Secure**: Authentication, authorization, and rate limiting
7. **Developer-Friendly**: Comprehensive documentation and examples

### API Gateway Architecture
```
API Request Flow:
Client Request → API Gateway (Port 5000) → Authentication → Rate Limiting → Service Routing → Microservice
```

---

## 🔗 API Versioning Strategy

### URL-Based Versioning
```
Base URL Structure:
├── https://api.dsagrind.com/v1/auth/*
├── https://api.dsagrind.com/v1/problems/*
├── https://api.dsagrind.com/v1/submissions/*
├── https://api.dsagrind.com/v1/payments/*
├── https://api.dsagrind.com/v1/users/*
├── https://api.dsagrind.com/v1/contests/*
├── https://api.dsagrind.com/v1/search/*
└── https://api.dsagrind.com/v1/admin/*
```

### Version Compatibility Matrix
```
API Version Support:
├── v1 (Current): Full support, all features
├── v0 (Legacy): Deprecated, security updates only
└── v2 (Future): Beta features, limited availability
```

---

## 🔐 Authentication & Authorization API

### Base URL: `/v1/auth`

#### POST /v1/auth/register
**Register a new user account**

```http
POST /v1/auth/register
Content-Type: application/json
X-API-Version: 1.0
X-Client-Version: 1.2.3

{
  "email": "user@example.com",
  "username": "codingmaster",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "country": "US",
  "agreeToTerms": true,
  "marketingOptIn": false
}
```

**Success Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "usr_2Nq8LHnF9X7Y3Z4R5S6T",
      "email": "user@example.com",
      "username": "codingmaster",
      "firstName": "John",
      "lastName": "Doe",
      "role": "User",
      "subscription": {
        "type": "Free",
        "expiresAt": null,
        "features": ["basic_problems", "community_access"]
      },
      "profile": {
        "avatar": "https://dsagrind-assets.s3.amazonaws.com/avatars/default.png",
        "createdAt": "2024-01-20T10:30:00Z",
        "isEmailVerified": false
      }
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "rt_9X8Y7Z6W5V4U3T2S1R0Q",
      "expiresAt": "2024-01-20T11:30:00Z",
      "tokenType": "Bearer"
    }
  },
  "meta": {
    "requestId": "req_abc123def456",
    "timestamp": "2024-01-20T10:30:00Z",
    "processingTime": "245ms"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data provided",
    "details": [
      {
        "field": "email",
        "code": "INVALID_EMAIL",
        "message": "Please provide a valid email address"
      },
      {
        "field": "password",
        "code": "WEAK_PASSWORD", 
        "message": "Password must contain at least 8 characters with uppercase, lowercase, number, and special character"
      }
    ]
  },
  "meta": {
    "requestId": "req_abc123def456",
    "timestamp": "2024-01-20T10:30:00Z"
  }
}
```

#### POST /v1/auth/login
**Authenticate user and obtain access tokens**

```http
POST /v1/auth/login
Content-Type: application/json

{
  "emailOrUsername": "user@example.com",
  "password": "SecurePass123!",
  "rememberMe": true,
  "deviceInfo": {
    "platform": "web",
    "userAgent": "Mozilla/5.0...",
    "ipAddress": "192.168.1.100"
  }
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "user": { /* User object */ },
    "tokens": { /* Token object */ },
    "session": {
      "sessionId": "sess_xyz789",
      "expiresAt": "2024-01-21T10:30:00Z",
      "isNewDevice": false,
      "location": "San Francisco, CA"
    }
  }
}
```

#### POST /v1/auth/refresh
**Refresh access token using refresh token**

```http
POST /v1/auth/refresh
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "refreshToken": "rt_9X8Y7Z6W5V4U3T2S1R0Q"
}
```

#### POST /v1/auth/logout
**Logout user and invalidate tokens**

```http
POST /v1/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "logoutAllDevices": false
}
```

#### GET /v1/auth/me
**Get current user profile**

```http
GET /v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "usr_2Nq8LHnF9X7Y3Z4R5S6T",
    "email": "user@example.com",
    "username": "codingmaster",
    "profile": {
      "firstName": "John",
      "lastName": "Doe",
      "displayName": "John Doe",
      "avatar": "https://dsagrind-assets.s3.amazonaws.com/avatars/usr_123.jpg",
      "bio": "Passionate coder and problem solver",
      "location": "San Francisco, CA",
      "website": "https://johndoe.dev",
      "socialLinks": {
        "github": "https://github.com/johndoe",
        "linkedin": "https://linkedin.com/in/johndoe"
      }
    },
    "statistics": {
      "totalSubmissions": 287,
      "acceptedSubmissions": 189,
      "acceptanceRate": 65.85,
      "problemsSolved": {
        "easy": 45,
        "medium": 78,
        "hard": 23,
        "total": 146
      },
      "currentStreak": 12,
      "longestStreak": 34,
      "ranking": {
        "global": 1567,
        "country": 123
      }
    },
    "subscription": {
      "type": "Premium",
      "status": "active",
      "expiresAt": "2024-12-31T23:59:59Z",
      "autoRenew": true,
      "features": ["premium_problems", "ai_hints", "detailed_analytics"]
    },
    "preferences": {
      "theme": "dark",
      "language": "en",
      "notifications": {
        "email": true,
        "push": false
      }
    }
  }
}
```

#### PUT /v1/auth/me
**Update current user profile**

```http
PUT /v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "profile": {
    "firstName": "John",
    "lastName": "Smith",
    "bio": "Updated bio text",
    "location": "New York, NY",
    "website": "https://johnsmith.dev"
  },
  "preferences": {
    "theme": "light",
    "notifications": {
      "email": false,
      "push": true
    }
  }
}
```

---

## 📚 Problems API

### Base URL: `/v1/problems`

#### GET /v1/problems
**Retrieve paginated list of problems with filtering and sorting**

```http
GET /v1/problems?page=1&limit=20&difficulty=Medium&tags=array,hash-table&search=two%20sum&sort=acceptance_rate&order=desc&premium=false
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters:**
```
Pagination:
├── page: Page number (default: 1)
├── limit: Items per page (default: 20, max: 100)

Filtering:
├── difficulty: Easy|Medium|Hard
├── tags: Comma-separated list of tags
├── search: Search term for title/description
├── premium: true|false (include premium problems)
├── status: not_attempted|attempted|completed
├── companies: Comma-separated company tags

Sorting:
├── sort: title|difficulty|acceptance_rate|created_at|updated_at
└── order: asc|desc (default: desc)
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "problems": [
      {
        "id": "prob_8M9N0O1P2Q3R4S5T6U7V",
        "title": "Two Sum",
        "slug": "two-sum",
        "difficulty": "Easy",
        "difficultyScore": 1200,
        "tags": [
          { "name": "array", "category": "data-structure" },
          { "name": "hash-table", "category": "data-structure" }
        ],
        "statistics": {
          "totalSubmissions": 15847,
          "acceptedSubmissions": 8956,
          "acceptanceRate": 56.52
        },
        "isPremium": false,
        "userProgress": {
          "status": "not_attempted", // not_attempted, attempted, completed
          "lastAttempt": null,
          "bestSubmission": null
        },
        "estimatedTime": "15-30 minutes",
        "companies": ["Google", "Amazon", "Apple"],
        "createdAt": "2023-08-15T10:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 45,
      "totalItems": 892,
      "itemsPerPage": 20,
      "hasNextPage": true,
      "hasPreviousPage": false
    },
    "filters": {
      "appliedFilters": {
        "difficulty": "Medium",
        "tags": ["array", "hash-table"],
        "premium": false
      },
      "availableFilters": {
        "difficulties": ["Easy", "Medium", "Hard"],
        "tags": ["array", "hash-table", "dynamic-programming", "..."],
        "companies": ["Google", "Amazon", "Microsoft", "..."]
      }
    }
  },
  "meta": {
    "requestId": "req_problems_list_123",
    "timestamp": "2024-01-20T10:30:00Z",
    "processingTime": "45ms",
    "cacheHit": true,
    "cacheExpiry": "2024-01-20T10:45:00Z"
  }
}
```

#### GET /v1/problems/{problemId}
**Get detailed problem information**

```http
GET /v1/problems/prob_8M9N0O1P2Q3R4S5T6U7V
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "prob_8M9N0O1P2Q3R4S5T6U7V",
    "title": "Two Sum",
    "slug": "two-sum",
    "description": {
      "statement": "Given an array of integers `nums` and an integer `target`, return indices of the two numbers such that they add up to target...",
      "inputFormat": "The first line contains an integer n, the size of the array...",
      "outputFormat": "Return an array of two integers representing the indices...",
      "constraints": [
        "2 ≤ nums.length ≤ 10^4",
        "-10^9 ≤ nums[i] ≤ 10^9",
        "-10^9 ≤ target ≤ 10^9"
      ]
    },
    "difficulty": "Easy",
    "difficultyScore": 1200,
    "tags": [
      { "name": "array", "category": "data-structure" },
      { "name": "hash-table", "category": "data-structure" }
    ],
    "examples": [
      {
        "input": "nums = [2,7,11,15], target = 9",
        "output": "[0,1]",
        "explanation": "Because nums[0] + nums[1] == 9, we return [0, 1]."
      }
    ],
    "hints": [
      {
        "level": 1,
        "content": "A really brute force way would be to search for all possible pairs...",
        "isPremium": false
      },
      {
        "level": 2, 
        "content": "Try using a hash map to improve the time complexity...",
        "isPremium": true
      }
    ],
    "starterCode": {
      "csharp": "public class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        \n    }\n}",
      "python": "class Solution:\n    def twoSum(self, nums: List[int], target: int) -> List[int]:\n        ",
      "java": "class Solution {\n    public int[] twoSum(int[] nums, int target) {\n        \n    }\n}"
    },
    "supportedLanguages": ["csharp", "python", "java", "cpp", "javascript"],
    "statistics": {
      "totalSubmissions": 15847,
      "acceptedSubmissions": 8956,
      "acceptanceRate": 56.52,
      "averageRuntime": 85,
      "languageStats": {
        "csharp": { "submissions": 4521, "accepted": 2789, "acceptanceRate": 61.70 },
        "python": { "submissions": 5890, "accepted": 3245, "acceptanceRate": 55.11 }
      }
    },
    "userProgress": {
      "status": "attempted",
      "attempts": 3,
      "lastAttempt": "2024-01-19T15:30:00Z",
      "bestSubmission": {
        "id": "sub_best_attempt_123",
        "status": "WrongAnswer",
        "runtime": 95,
        "memory": 42.3,
        "submittedAt": "2024-01-19T15:30:00Z"
      },
      "timeSpent": 2847 // seconds
    },
    "relatedProblems": [
      {
        "id": "prob_3sum_challenge",
        "title": "3Sum",
        "difficulty": "Medium",
        "similarity": 0.85
      }
    ],
    "editorial": {
      "available": true,
      "isPremium": true,
      "preview": "This problem can be solved using multiple approaches..."
    },
    "discussionCount": 1247,
    "isPremium": false,
    "estimatedTime": "15-30 minutes",
    "companies": ["Google", "Amazon", "Apple"],
    "createdAt": "2023-08-15T10:00:00Z",
    "updatedAt": "2024-01-18T14:22:00Z"
  }
}
```

#### GET /v1/problems/{problemId}/editorial
**Get problem editorial (Premium feature)**

```http
GET /v1/problems/prob_8M9N0O1P2Q3R4S5T6U7V/editorial
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### GET /v1/problems/{problemId}/solutions
**Get community solutions**

```http
GET /v1/problems/prob_8M9N0O1P2Q3R4S5T6U7V/solutions?language=csharp&sort=votes&page=1&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### GET /v1/problems/{problemId}/submissions
**Get user's submissions for a specific problem**

```http
GET /v1/problems/prob_8M9N0O1P2Q3R4S5T6U7V/submissions?page=1&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 💻 Submissions API

### Base URL: `/v1/submissions`

#### POST /v1/submissions
**Submit code solution for a problem**

```http
POST /v1/submissions
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "problemId": "prob_8M9N0O1P2Q3R4S5T6U7V",
  "language": "csharp",
  "code": "public class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        Dictionary<int, int> map = new Dictionary<int, int>();\n        \n        for (int i = 0; i < nums.Length; i++) {\n            int complement = target - nums[i];\n            if (map.ContainsKey(complement)) {\n                return new int[] { map[complement], i };\n            }\n            map[nums[i]] = i;\n        }\n        \n        return new int[] { };\n    }\n}",
  "contestId": null, // Optional: if submitted during contest
  "isPrivate": false,
  "metadata": {
    "editorSettings": {
      "theme": "vs-dark",
      "fontSize": 14
    },
    "codingTime": 1847000, // milliseconds
    "keystrokes": 547
  }
}
```

**Success Response (202 Accepted):**
```json
{
  "success": true,
  "data": {
    "submissionId": "sub_5K6L7M8N9O0P1Q2R3S4T",
    "status": "pending",
    "estimatedProcessingTime": "3-8 seconds",
    "queuePosition": 3,
    "message": "Your submission has been received and is being processed"
  },
  "meta": {
    "requestId": "req_submission_123",
    "timestamp": "2024-01-20T14:30:00Z"
  }
}
```

#### GET /v1/submissions/{submissionId}
**Get submission details and results**

```http
GET /v1/submissions/sub_5K6L7M8N9O0P1Q2R3S4T
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "sub_5K6L7M8N9O0P1Q2R3S4T",
    "problemId": "prob_8M9N0O1P2Q3R4S5T6U7V",
    "problemTitle": "Two Sum",
    "userId": "usr_2Nq8LHnF9X7Y3Z4R5S6T",
    "code": {
      "source": "public class Solution {...}",
      "language": "csharp",
      "codeLength": 456,
      "linesOfCode": 14
    },
    "execution": {
      "status": "Accepted", // Pending, Running, Accepted, WrongAnswer, RuntimeError, TimeLimitExceeded, etc.
      "statusMessage": "All test cases passed successfully",
      "submittedAt": "2024-01-20T14:30:00Z",
      "completedAt": "2024-01-20T14:30:08Z",
      "totalExecutionTime": 6234
    },
    "results": {
      "passedTestCases": 15,
      "totalTestCases": 15,
      "passRate": 100.0,
      "score": 100,
      "testCaseResults": [
        {
          "testCaseId": "tc_1",
          "status": "Passed",
          "isPublic": true,
          "input": "[2,7,11,15]\\n9",
          "expectedOutput": "[0,1]",
          "actualOutput": "[0,1]",
          "executionTime": 45,
          "memoryUsed": 2147483
        }
        // Additional test cases...
      ],
      "performanceMetrics": {
        "averageExecutionTime": 48.7,
        "maxMemoryUsed": 2267834,
        "timeEfficiency": 97.6,
        "memoryEfficiency": 84.5,
        "percentileRank": 85.7
      }
    },
    "analysis": {
      "timeComplexity": {
        "estimated": "O(n)",
        "confidence": 0.95
      },
      "spaceComplexity": {
        "estimated": "O(n)", 
        "confidence": 0.92
      },
      "codeQuality": {
        "score": 8.5,
        "feedback": [
          "Excellent use of hash table for optimization",
          "Clean and readable code structure",
          "Good variable naming conventions"
        ]
      }
    },
    "feedback": {
      "aiGeneratedTips": [
        "Great job achieving O(n) time complexity!",
        "Consider adding input validation for edge cases"
      ],
      "similarityCheck": {
        "hasSimilarSubmissions": false,
        "suspiciousLevel": "low"
      },
      "nextRecommendations": [
        {
          "problemId": "prob_3sum_challenge",
          "title": "3Sum",
          "reason": "Similar hash table concepts"
        }
      ]
    },
    "isPrivate": false,
    "contestSubmission": null,
    "shareableUrl": "https://dsagrind.com/submissions/sub_5K6L7M8N9O0P1Q2R3S4T"
  }
}
```

#### GET /v1/submissions
**Get user's submission history**

```http
GET /v1/submissions?page=1&limit=20&status=Accepted&language=csharp&problemId=prob_123&dateFrom=2024-01-01&dateTo=2024-01-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### POST /v1/submissions/{submissionId}/run
**Run code with custom input (testing)**

```http
POST /v1/submissions/sub_5K6L7M8N9O0P1Q2R3S4T/run
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "customInput": "[1,2,3,4]\\n5",
  "expectedOutput": "[]"
}
```

---

## 💳 Payments API

### Base URL: `/v1/payments`

#### GET /v1/payments/plans
**Get available subscription plans**

```http
GET /v1/payments/plans
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "plans": [
      {
        "id": "plan_free",
        "name": "Free",
        "price": {
          "amount": 0,
          "currency": "USD",
          "interval": null
        },
        "features": [
          "Access to 100+ basic problems",
          "Community discussion access",
          "Basic code execution",
          "Limited submissions per hour"
        ],
        "limits": {
          "problemsPerMonth": 100,
          "submissionsPerHour": 10,
          "aiHintsPerMonth": 0
        },
        "isPopular": false
      },
      {
        "id": "plan_premium_monthly",
        "name": "Premium",
        "price": {
          "amount": 9.99,
          "currency": "USD", 
          "interval": "month"
        },
        "features": [
          "Access to 500+ problems including premium",
          "AI-powered hints and explanations",
          "Detailed performance analytics",
          "Unlimited submissions",
          "Contest participation",
          "Code review and optimization tips"
        ],
        "limits": {
          "problemsPerMonth": "unlimited",
          "submissionsPerHour": "unlimited",
          "aiHintsPerMonth": 100
        },
        "isPopular": true,
        "discount": {
          "percentage": 20,
          "validUntil": "2024-02-01T00:00:00Z",
          "promoCode": "NEWUSER20"
        }
      },
      {
        "id": "plan_premium_yearly",
        "name": "Premium Annual",
        "price": {
          "amount": 99.99,
          "currency": "USD",
          "interval": "year"
        },
        "features": [
          "All Premium features",
          "Priority customer support",
          "Early access to new features",
          "Advanced analytics dashboard"
        ],
        "limits": {
          "problemsPerMonth": "unlimited",
          "submissionsPerHour": "unlimited", 
          "aiHintsPerMonth": "unlimited"
        },
        "savings": {
          "compared_to": "plan_premium_monthly",
          "amount": 19.89,
          "percentage": 16.6
        }
      }
    ]
  }
}
```

#### POST /v1/payments/create-intent
**Create payment intent for subscription**

```http
POST /v1/payments/create-intent
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "planId": "plan_premium_monthly",
  "paymentMethodId": "pm_1NvKTGLkdIwHu7ix0KqhrYnK",
  "promoCode": "NEWUSER20",
  "billingAddress": {
    "line1": "123 Main St",
    "city": "San Francisco",
    "state": "CA",
    "postalCode": "94105",
    "country": "US"
  }
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "paymentIntentId": "pi_1NvKTGLkdIwHu7ix0KqhrYnK",
    "clientSecret": "pi_1NvKTGLkdIwHu7ix0KqhrYnK_secret_xyz",
    "amount": {
      "subtotal": 9.99,
      "discount": -2.00,
      "tax": 0.85,
      "total": 8.84,
      "currency": "USD"
    },
    "subscription": {
      "id": "sub_premium_monthly_123",
      "startDate": "2024-01-20T00:00:00Z",
      "endDate": "2024-02-20T00:00:00Z",
      "autoRenew": true
    },
    "requiresAction": false,
    "nextAction": null
  }
}
```

#### GET /v1/payments/history
**Get payment history**

```http
GET /v1/payments/history?page=1&limit=10&dateFrom=2024-01-01&dateTo=2024-01-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### GET /v1/payments/subscription
**Get current subscription details**

```http
GET /v1/payments/subscription
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### PUT /v1/payments/subscription
**Update subscription (upgrade/downgrade)**

```http
PUT /v1/payments/subscription
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "newPlanId": "plan_premium_yearly",
  "prorationBehavior": "create_prorations" // create_prorations, none
}
```

#### DELETE /v1/payments/subscription
**Cancel subscription**

```http
DELETE /v1/payments/subscription
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "cancelAtPeriodEnd": true,
  "reason": "switching_service",
  "feedback": "Found a better alternative"
}
```

---

## 🔍 Search API

### Base URL: `/v1/search`

#### GET /v1/search/problems
**Advanced problem search with semantic similarity**

```http
GET /v1/search/problems?q=find%20duplicates%20in%20array&semantic=true&limit=10&filters[difficulty]=Medium&filters[tags]=array
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "problem": {
          "id": "prob_find_duplicates",
          "title": "Find All Duplicates in an Array",
          "difficulty": "Medium",
          "tags": ["array", "hash-table"],
          "acceptanceRate": 67.8
        },
        "relevanceScore": 0.94,
        "matchedFields": ["title", "description"],
        "semanticSimilarity": 0.89
      }
    ],
    "searchMetadata": {
      "query": "find duplicates in array",
      "semanticSearchEnabled": true,
      "totalResults": 45,
      "searchTime": "23ms",
      "suggestions": [
        "duplicate numbers",
        "array duplicates",
        "remove duplicates"
      ]
    }
  }
}
```

#### GET /v1/search/suggestions
**Get search suggestions and autocomplete**

```http
GET /v1/search/suggestions?q=two%20sum&type=problems&limit=5
```

#### POST /v1/search/similar-problems
**Find problems similar to given problem**

```http
POST /v1/search/similar-problems
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "problemId": "prob_8M9N0O1P2Q3R4S5T6U7V",
  "limit": 10,
  "minSimilarity": 0.7
}
```

---

## 🏆 Contests API

### Base URL: `/v1/contests`

#### GET /v1/contests
**Get list of contests**

```http
GET /v1/contests?status=upcoming&type=weekly&page=1&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "contests": [
      {
        "id": "contest_weekly_67",
        "title": "Weekly Contest 67",
        "type": "weekly",
        "status": "upcoming",
        "schedule": {
          "startTime": "2024-02-01T18:00:00Z",
          "endTime": "2024-02-01T19:30:00Z",
          "duration": 5400,
          "timezone": "UTC"
        },
        "problems": [
          {
            "order": 1,
            "title": "Array Manipulation",
            "points": 100,
            "difficulty": "Easy"
          },
          {
            "order": 2,
            "title": "String Algorithms",
            "points": 200,
            "difficulty": "Medium"
          }
        ],
        "participants": {
          "registered": 1247,
          "capacity": 5000
        },
        "prizes": {
          "hasFirstPrize": true,
          "description": "Winner gets premium subscription"
        },
        "isRegistered": false,
        "registrationDeadline": "2024-02-01T17:30:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalItems": 48
    }
  }
}
```

#### POST /v1/contests/{contestId}/register
**Register for a contest**

```http
POST /v1/contests/contest_weekly_67/register
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### GET /v1/contests/{contestId}/leaderboard
**Get contest leaderboard**

```http
GET /v1/contests/contest_weekly_67/leaderboard?page=1&limit=50
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 Analytics API

### Base URL: `/v1/analytics`

#### GET /v1/analytics/user/progress
**Get user progress analytics**

```http
GET /v1/analytics/user/progress?period=30d&include=submissions,performance,streaks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "period": {
      "from": "2023-12-21T00:00:00Z",
      "to": "2024-01-20T23:59:59Z",
      "days": 30
    },
    "submissions": {
      "total": 89,
      "accepted": 67,
      "acceptanceRate": 75.28,
      "dailyBreakdown": [
        { "date": "2024-01-20", "submitted": 3, "accepted": 2 },
        { "date": "2024-01-19", "submitted": 5, "accepted": 4 }
      ],
      "languageBreakdown": {
        "csharp": { "submitted": 34, "accepted": 28 },
        "python": { "submitted": 32, "accepted": 24 },
        "java": { "submitted": 23, "accepted": 15 }
      }
    },
    "performance": {
      "averageRuntime": 127, // milliseconds
      "averageMemory": 45.2, // MB
      "runtimeTrend": "improving", // improving, stable, declining
      "memoryTrend": "stable",
      "percentileRank": {
        "runtime": 78.5,
        "memory": 82.1
      }
    },
    "streaks": {
      "current": 12,
      "longest": 34,
      "history": [
        { "startDate": "2024-01-08", "endDate": "2024-01-20", "length": 12 }
      ]
    },
    "problemCategories": {
      "arrays": { "attempted": 25, "completed": 19, "successRate": 76.0 },
      "strings": { "attempted": 18, "completed": 14, "successRate": 77.8 },
      "trees": { "attempted": 12, "completed": 8, "successRate": 66.7 }
    },
    "insights": [
      {
        "type": "improvement",
        "message": "Your acceptance rate has improved by 12% this month!"
      },
      {
        "type": "suggestion",
        "message": "Try more dynamic programming problems to strengthen weak areas"
      }
    ]
  }
}
```

#### GET /v1/analytics/user/comparison
**Compare performance with peers**

```http
GET /v1/analytics/user/comparison?metric=acceptance_rate&timeframe=all_time&cohort=similar_experience
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🔧 Admin API (Premium/Admin Only)

### Base URL: `/v1/admin`

#### GET /v1/admin/users
**Get user management dashboard**

```http
GET /v1/admin/users?page=1&limit=50&search=john&status=active&subscription=premium
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Admin-Role: admin
```

#### POST /v1/admin/problems
**Create new problem**

```http
POST /v1/admin/problems
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Admin-Role: content_creator
Content-Type: application/json

{
  "title": "New Algorithm Challenge",
  "description": {
    "statement": "Given an array...",
    "inputFormat": "First line contains...",
    "outputFormat": "Return a single integer...",
    "constraints": ["1 ≤ n ≤ 10^5"]
  },
  "difficulty": "Medium",
  "tags": ["array", "sorting"],
  "testCases": [
    {
      "input": "3\n[1,2,3]",
      "expectedOutput": "6",
      "isPublic": true
    }
  ],
  "hints": [
    {
      "level": 1,
      "content": "Think about the sum of elements",
      "isPremium": false
    }
  ],
  "isPremium": false
}
```

#### GET /v1/admin/analytics/platform
**Get platform-wide analytics**

```http
GET /v1/admin/analytics/platform?period=7d&metrics=users,submissions,revenue
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Admin-Role: admin
```

---

## 🚨 Error Handling

### Standard Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": "Additional error context or validation errors",
    "timestamp": "2024-01-20T10:30:00Z"
  },
  "meta": {
    "requestId": "req_error_123",
    "endpoint": "/v1/problems/invalid-id",
    "method": "GET"
  }
}
```

### HTTP Status Codes
```
Success Codes:
├── 200 OK: Request successful
├── 201 Created: Resource created successfully
├── 202 Accepted: Request accepted for processing
└── 204 No Content: Successful request with no response body

Client Error Codes:
├── 400 Bad Request: Invalid request data
├── 401 Unauthorized: Authentication required
├── 403 Forbidden: Insufficient permissions
├── 404 Not Found: Resource not found
├── 409 Conflict: Resource already exists
├── 422 Unprocessable Entity: Validation errors
└── 429 Too Many Requests: Rate limit exceeded

Server Error Codes:
├── 500 Internal Server Error: Unexpected server error
├── 502 Bad Gateway: Upstream service error
├── 503 Service Unavailable: Service temporarily unavailable
└── 504 Gateway Timeout: Request timeout
```

### Error Code Categories
```
Authentication Errors:
├── AUTH_TOKEN_INVALID
├── AUTH_TOKEN_EXPIRED
├── AUTH_CREDENTIALS_INVALID
└── AUTH_PERMISSION_DENIED

Validation Errors:
├── VALIDATION_REQUIRED_FIELD
├── VALIDATION_INVALID_FORMAT
├── VALIDATION_OUT_OF_RANGE
└── VALIDATION_DUPLICATE_VALUE

Business Logic Errors:
├── RESOURCE_NOT_FOUND
├── RESOURCE_ALREADY_EXISTS
├── OPERATION_NOT_ALLOWED
└── QUOTA_EXCEEDED

System Errors:
├── INTERNAL_ERROR
├── SERVICE_UNAVAILABLE
├── DATABASE_ERROR
└── EXTERNAL_SERVICE_ERROR
```

---

## 📡 Real-time APIs (WebSocket)

### WebSocket Endpoints
```
WebSocket Connections:
├── wss://api.dsagrind.com/v1/ws/submissions/{submissionId}
├── wss://api.dsagrind.com/v1/ws/contests/{contestId}
└── wss://api.dsagrind.com/v1/ws/notifications
```

### Submission Status Updates
```json
{
  "type": "submission_status_update",
  "data": {
    "submissionId": "sub_5K6L7M8N9O0P1Q2R3S4T",
    "status": "Running",
    "progress": {
      "currentTestCase": 5,
      "totalTestCases": 15,
      "percentage": 33.3
    },
    "timestamp": "2024-01-20T14:30:05Z"
  }
}
```

### Contest Updates
```json
{
  "type": "contest_leaderboard_update",
  "data": {
    "contestId": "contest_weekly_67",
    "leaderboard": [
      {
        "rank": 1,
        "userId": "usr_champion",
        "score": 400,
        "penalty": 1247
      }
    ],
    "timestamp": "2024-02-01T18:30:00Z"
  }
}
```

---

## 🔒 Rate Limiting

### Rate Limit Configuration
```
API Endpoint Rate Limits:
├── Authentication: 5 requests/minute per IP
├── Problem Listing: 100 requests/minute per user
├── Problem Details: 200 requests/minute per user
├── Code Submission: 10 submissions/minute per user
├── Search API: 50 requests/minute per user
└── Analytics: 20 requests/minute per user

Rate Limit Headers:
├── X-RateLimit-Limit: Maximum requests allowed
├── X-RateLimit-Remaining: Requests remaining
├── X-RateLimit-Reset: Unix timestamp when limit resets
└── Retry-After: Seconds to wait before retrying (when limited)
```

### Rate Limit Response
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please wait before trying again.",
    "retryAfter": 60
  },
  "meta": {
    "requestId": "req_rate_limited_123",
    "rateLimit": {
      "limit": 10,
      "remaining": 0,
      "reset": 1642781294
    }
  }
}
```

This comprehensive API design provides a robust, scalable, and developer-friendly interface for the DSAGrind platform, supporting all features while maintaining security, performance, and usability standards.