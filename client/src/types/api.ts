// User Types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isEmailVerified: boolean;
  isActive: boolean;
  totalSolved: number;
  rank: number;
  subscriptionPlan: string;
  subscriptionStatus: string;
  subscriptionExpires?: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

// Problem Types
export interface Problem {
  id: string;
  title: string;
  description: string;
  difficulty: string;
  categoryId: string;
  tags: string[];
  hints: string[];
  solution?: string;
  solutionLanguage?: string;
  testCases: TestCase[];
  constraints?: string;
  examples: Example[];
  isPaid: boolean;
  isApproved: boolean;
  likes: number;
  dislikes: number;
  submissions: number;
  acceptedSubmissions: number;
  acceptanceRate: number;
  createdBy?: string;
  createdAt: string;
  updatedAt: string;
}

export interface TestCase {
  input: string;
  expectedOutput: string;
  isHidden: boolean;
}

export interface Example {
  input: string;
  output: string;
  explanation?: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  icon?: string;
  problemCount: number;
  isActive: boolean;
}

// Submission Types
export interface Submission {
  id: string;
  userId: string;
  problemId: string;
  code: string;
  language: string;
  status: string;
  runtime?: number;
  memory?: number;
  testCasesPassed: number;
  totalTestCases: number;
  errorMessage?: string;
  notes?: string;
  createdAt: string;
}

export interface CodeExecutionRequest {
  code: string;
  language: string;
  problemId: string;
}

export interface CodeExecutionResult {
  status: string;
  output?: string;
  error?: string;
  runtime?: number;
  memory?: number;
  testCasesPassed: number;
  totalTestCases: number;
  testResults: TestCaseResult[];
}

export interface TestCaseResult {
  input: string;
  expectedOutput: string;
  actualOutput: string;
  passed: boolean;
  runtime: number;
  memory: number;
}

// AI Types
export interface AIHintRequest {
  problemId: string;
  currentCode?: string;
  difficulty: string;
}

export interface AIExplanationRequest {
  problemId: string;
  code: string;
  language: string;
}

export interface AICodeAnalysisRequest {
  code: string;
  language: string;
  problemId: string;
}

export interface AIResponse {
  response: string;
  confidence: number;
  context: string;
  usage: TokenUsage;
}

export interface TokenUsage {
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  cost: number;
}

// Search Types
export interface SearchRequest {
  query: string;
  filters?: SearchFilters;
  page?: number;
  pageSize?: number;
}

export interface SearchFilters {
  difficulty?: string[];
  categories?: string[];
  tags?: string[];
  isPremium?: boolean;
}

export interface SearchResult {
  problems: Problem[];
  totalCount: number;
  suggestions: string[];
  aggregations: SearchAggregations;
}

export interface SearchAggregations {
  difficulties: { [key: string]: number };
  categories: { [key: string]: number };
  tags: { [key: string]: number };
}

// Payment Types
export interface PaymentIntent {
  id: string;
  clientSecret: string;
  amount: number;
  currency: string;
  status: string;
}

export interface CreatePaymentRequest {
  amount: number;
  currency: string;
  description: string;
  metadata?: { [key: string]: string };
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  interval: string;
  features: string[];
  isPopular: boolean;
  stripePriceId: string;
}

export interface Subscription {
  id: string;
  userId: string;
  planId: string;
  status: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
  stripeSubscriptionId: string;
  createdAt: string;
}

// Admin Types
export interface AdminDashboard {
  totalUsers: number;
  activeUsers: number;
  totalProblems: number;
  pendingProblems: number;
  totalSubmissions: number;
  todaySubmissions: number;
  revenue: number;
  premiumUsers: number;
  submissionsChart: ChartData[];
  usersChart: ChartData[];
}

export interface ChartData {
  label: string;
  value: number;
}

export interface AdminNotification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface SystemHealth {
  isHealthy: boolean;
  services: ServiceHealth[];
  database: DatabaseHealth;
  externalServices: ExternalServiceHealth;
}

export interface ServiceHealth {
  name: string;
  isHealthy: boolean;
  status: string;
  responseTime: number;
  lastCheck: string;
}

export interface DatabaseHealth {
  isHealthy: boolean;
  responseTime: number;
  connectionCount: number;
}

export interface ExternalServiceHealth {
  stripeHealthy: boolean;
  redisHealthy: boolean;
  kafkaHealthy: boolean;
  qdrantHealthy: boolean;
}

// Common Types
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}