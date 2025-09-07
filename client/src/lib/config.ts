// API Configuration for DSAGrind .NET Microservices
export const API_CONFIG = {
  // Gateway API (main entry point for all microservices)
  GATEWAY_URL: import.meta.env.VITE_GATEWAY_URL || 'http://localhost:5000',
  
  // Individual service URLs (for direct access if needed)
  SERVICES: {
    AUTH: import.meta.env.VITE_AUTH_URL || 'http://localhost:8080',
    PROBLEMS: import.meta.env.VITE_PROBLEMS_URL || 'http://localhost:5001',
    SUBMISSIONS: import.meta.env.VITE_SUBMISSIONS_URL || 'http://localhost:5002',
    AI: import.meta.env.VITE_AI_URL || 'http://localhost:5003',
    SEARCH: import.meta.env.VITE_SEARCH_URL || 'http://localhost:5004',
    ADMIN: import.meta.env.VITE_ADMIN_URL || 'http://localhost:5005',
    PAYMENTS: import.meta.env.VITE_PAYMENTS_URL || 'http://localhost:5006',
  },
  
  // API endpoints (routed through Gateway)
  ENDPOINTS: {
    // Auth endpoints
    AUTH: {
      LOGIN: '/api/auth/login',
      REGISTER: '/api/auth/register',
      REFRESH: '/api/auth/refresh',
      LOGOUT: '/api/auth/revoke',
      ME: '/api/auth/me',
      VERIFY_EMAIL: '/api/auth/verify-email',
      FORGOT_PASSWORD: '/api/auth/forgot-password',
      RESET_PASSWORD: '/api/auth/reset-password',
      CHANGE_PASSWORD: '/api/auth/change-password',
    },
    
    // Problems endpoints
    PROBLEMS: {
      LIST: '/api/problems',
      SEARCH: '/api/problems/search',
      GET: (id: string) => `/api/problems/${id}`,
      CREATE: '/api/problems',
      UPDATE: (id: string) => `/api/problems/${id}`,
      DELETE: (id: string) => `/api/problems/${id}`,
      RANDOM: '/api/problems/random',
      SIMILAR: (id: string) => `/api/problems/${id}/similar`,
      LIKE: (id: string) => `/api/problems/${id}/like`,
    },
    
    // Categories endpoints
    CATEGORIES: {
      LIST: '/api/categories',
      GET: (id: string) => `/api/categories/${id}`,
      CREATE: '/api/categories',
      UPDATE: (id: string) => `/api/categories/${id}`,
      DELETE: (id: string) => `/api/categories/${id}`,
    },
    
    // Submissions endpoints
    SUBMISSIONS: {
      LIST: '/api/submissions',
      GET: (id: string) => `/api/submissions/${id}`,
      CREATE: '/api/submissions',
      EXECUTE: '/api/submissions/execute',
      TEST: '/api/submissions/test',
      LANGUAGES: '/api/submissions/languages',
      LEADERBOARD: (problemId: string) => `/api/submissions/leaderboard/${problemId}`,
    },
    
    // AI endpoints
    AI: {
      HINT: '/api/ai/hint',
      EXPLAIN: '/api/ai/explain',
      ANALYZE: '/api/ai/analyze',
      OPTIMIZE: '/api/ai/optimize',
      DEBUG: '/api/ai/debug',
    },
    
    // Search endpoints
    SEARCH: {
      SEARCH: '/api/search',
      SUGGESTIONS: '/api/search/suggestions',
      SIMILAR: (problemId: string) => `/api/search/similar/${problemId}`,
      RECOMMENDATIONS: '/api/search/recommendations',
      BY_TAGS: '/api/search/by-tags',
    },
    
    // Admin endpoints
    ADMIN: {
      DASHBOARD: '/api/admin/dashboard',
      USERS: '/api/admin/users',
      USER: (id: string) => `/api/admin/users/${id}`,
      BAN_USER: (id: string) => `/api/admin/users/${id}/ban`,
      UNBAN_USER: (id: string) => `/api/admin/users/${id}/unban`,
      PENDING_PROBLEMS: '/api/admin/problems/pending',
      APPROVE_PROBLEM: (id: string) => `/api/admin/problems/${id}/approve`,
      REJECT_PROBLEM: (id: string) => `/api/admin/problems/${id}/reject`,
      ANALYTICS: '/api/admin/analytics',
      SYSTEM_HEALTH: '/api/admin/system/health',
    },
    
    // Payments endpoints
    PAYMENTS: {
      CREATE_INTENT: '/api/payments/create-intent',
      CONFIRM: '/api/payments/confirm',
      HISTORY: '/api/payments/history',
      GET: (id: string) => `/api/payments/${id}`,
      PLANS: '/api/payments/plans',
      SUBSCRIPTIONS: {
        CREATE: '/api/payments/subscriptions/create',
        CURRENT: '/api/payments/subscriptions/current',
        CANCEL: (id: string) => `/api/payments/subscriptions/${id}/cancel`,
      },
    },
  },
} as const;

// Helper function to build full API URLs
export const buildApiUrl = (endpoint: string) => {
  return `${API_CONFIG.GATEWAY_URL}${endpoint}`;
};

// Helper function for direct service access
export const buildServiceUrl = (service: keyof typeof API_CONFIG.SERVICES, endpoint: string) => {
  return `${API_CONFIG.SERVICES[service]}${endpoint}`;
};