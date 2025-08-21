import { sql } from "drizzle-orm";
import { pgTable, varchar, text, boolean, integer, timestamp, jsonb, decimal } from "drizzle-orm/pg-core";
import { createInsertSchema } from "drizzle-zod";
import { z } from "zod";

// Users table
export const users = pgTable("users", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  username: varchar("username", { length: 50 }).notNull().unique(),
  email: varchar("email", { length: 255 }).notNull().unique(),
  passwordHash: varchar("password_hash", { length: 255 }),
  firstName: varchar("first_name", { length: 100 }),
  lastName: varchar("last_name", { length: 100 }),
  avatar: varchar("avatar", { length: 500 }),
  role: varchar("role", { length: 20 }).notNull().default("user"), // user, admin
  isEmailVerified: boolean("is_email_verified").notNull().default(false),
  emailVerificationToken: varchar("email_verification_token", { length: 255 }),
  resetPasswordToken: varchar("reset_password_token", { length: 255 }),
  resetPasswordExpires: timestamp("reset_password_expires"),
  githubId: varchar("github_id", { length: 50 }),
  googleId: varchar("google_id", { length: 50 }),
  stripeCustomerId: varchar("stripe_customer_id", { length: 255 }),
  stripeSubscriptionId: varchar("stripe_subscription_id", { length: 255 }),
  subscriptionStatus: varchar("subscription_status", { length: 50 }).default("free"), // free, premium, cancelled
  subscriptionEnds: timestamp("subscription_ends"),
  isPremium: boolean("is_premium").notNull().default(false),
  totalSolved: integer("total_solved").notNull().default(0),
  rank: integer("rank").default(0),
  createdAt: timestamp("created_at").notNull().defaultNow(),
  updatedAt: timestamp("updated_at").notNull().defaultNow(),
});

// Categories table for organizing problems
export const categories = pgTable("categories", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  name: varchar("name", { length: 100 }).notNull(),
  slug: varchar("slug", { length: 100 }).notNull().unique(),
  description: text("description"),
  icon: varchar("icon", { length: 50 }),
  freeQuestionLimit: integer("free_question_limit").notNull().default(5), // Admin configurable
  totalQuestions: integer("total_questions").notNull().default(0),
  isActive: boolean("is_active").notNull().default(true),
  createdAt: timestamp("created_at").notNull().defaultNow(),
  updatedAt: timestamp("updated_at").notNull().defaultNow(),
});

// Problems table
export const problems = pgTable("problems", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  title: varchar("title", { length: 200 }).notNull(),
  slug: varchar("slug", { length: 200 }).notNull().unique(),
  description: text("description").notNull(),
  difficulty: varchar("difficulty", { length: 20 }).notNull(), // Easy, Medium, Hard
  categoryId: varchar("category_id").notNull(),
  tags: text("tags").array().notNull().default([]),
  hints: text("hints").array().notNull().default([]),
  solution: text("solution"),
  solutionLanguage: varchar("solution_language", { length: 50 }).default("python"),
  testCases: jsonb("test_cases").notNull().default([]),
  constraints: text("constraints"),
  examples: jsonb("examples").notNull().default([]),
  isPremium: boolean("is_premium").notNull().default(false),
  isApproved: boolean("is_approved").notNull().default(false),
  likes: integer("likes").notNull().default(0),
  dislikes: integer("dislikes").notNull().default(0),
  submissions: integer("submissions").notNull().default(0),
  acceptedSubmissions: integer("accepted_submissions").notNull().default(0),
  acceptanceRate: decimal("acceptance_rate", { precision: 5, scale: 2 }).default("0"),
  createdBy: varchar("created_by"),
  createdAt: timestamp("created_at").notNull().defaultNow(),
  updatedAt: timestamp("updated_at").notNull().defaultNow(),
});

// User submissions
export const submissions = pgTable("submissions", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  userId: varchar("user_id").notNull(),
  problemId: varchar("problem_id").notNull(),
  code: text("code").notNull(),
  language: varchar("language", { length: 50 }).notNull(),
  status: varchar("status", { length: 50 }).notNull(), // Accepted, Wrong Answer, Time Limit Exceeded, etc.
  runtime: integer("runtime"), // in milliseconds
  memory: integer("memory"), // in KB
  testCasesPassed: integer("test_cases_passed").notNull().default(0),
  totalTestCases: integer("total_test_cases").notNull().default(0),
  errorMessage: text("error_message"),
  createdAt: timestamp("created_at").notNull().defaultNow(),
});

// User problem progress
export const userProgress = pgTable("user_progress", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  userId: varchar("user_id").notNull(),
  problemId: varchar("problem_id").notNull(),
  status: varchar("status", { length: 50 }).notNull(), // not_started, in_progress, solved
  attempts: integer("attempts").notNull().default(0),
  timeSpent: integer("time_spent").notNull().default(0), // in seconds
  lastAttemptAt: timestamp("last_attempt_at"),
  solvedAt: timestamp("solved_at"),
  hintsUsed: integer("hints_used").notNull().default(0),
  createdAt: timestamp("created_at").notNull().defaultNow(),
  updatedAt: timestamp("updated_at").notNull().defaultNow(),
});

// Contest/Challenge events
export const contests = pgTable("contests", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  title: varchar("title", { length: 200 }).notNull(),
  description: text("description"),
  startTime: timestamp("start_time").notNull(),
  endTime: timestamp("end_time").notNull(),
  problemIds: text("problem_ids").array().notNull().default([]),
  participants: integer("participants").notNull().default(0),
  prizes: jsonb("prizes").default([]),
  isActive: boolean("is_active").notNull().default(true),
  createdBy: varchar("created_by").notNull(),
  createdAt: timestamp("created_at").notNull().defaultNow(),
});

// AI Chat history for Perplexity integration
export const aiChats = pgTable("ai_chats", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  userId: varchar("user_id").notNull(),
  problemId: varchar("problem_id"),
  sessionId: varchar("session_id").notNull(),
  messages: jsonb("messages").notNull().default([]),
  context: varchar("context", { length: 100 }).notNull(), // hint, debug, explanation
  createdAt: timestamp("created_at").notNull().defaultNow(),
});

// System settings for admin
export const settings = pgTable("settings", {
  id: varchar("id").primaryKey().default(sql`gen_random_uuid()`),
  key: varchar("key", { length: 100 }).notNull().unique(),
  value: text("value"),
  description: text("description"),
  updatedBy: varchar("updated_by"),
  updatedAt: timestamp("updated_at").notNull().defaultNow(),
});

// Insert schemas for forms
export const insertUserSchema = createInsertSchema(users).omit({
  id: true,
  createdAt: true,
  updatedAt: true,
}).extend({
  password: z.string().min(6, "Password must be at least 6 characters")
});

// For direct database creation without password validation
export const createUserSchema = createInsertSchema(users).omit({
  id: true,
  createdAt: true,
  updatedAt: true,
});

export const insertCategorySchema = createInsertSchema(categories).omit({
  id: true,
  totalQuestions: true,
  createdAt: true,
  updatedAt: true,
});

export const insertProblemSchema = createInsertSchema(problems).omit({
  id: true,
  submissions: true,
  acceptedSubmissions: true,
  acceptanceRate: true,
  likes: true,
  dislikes: true,
  createdAt: true,
  updatedAt: true,
});

export const insertSubmissionSchema = createInsertSchema(submissions).omit({
  id: true,
  createdAt: true,
});

export const insertUserProgressSchema = createInsertSchema(userProgress).omit({
  id: true,
  createdAt: true,
  updatedAt: true,
});

export const insertContestSchema = createInsertSchema(contests).omit({
  id: true,
  participants: true,
  createdAt: true,
});

export const insertAiChatSchema = createInsertSchema(aiChats).omit({
  id: true,
  createdAt: true,
});

export const insertSettingSchema = createInsertSchema(settings).omit({
  id: true,
  updatedAt: true,
});

// Types
export type User = typeof users.$inferSelect;
export type InsertUser = z.infer<typeof insertUserSchema>;
export type CreateUser = z.infer<typeof createUserSchema>;

export type Category = typeof categories.$inferSelect;
export type InsertCategory = z.infer<typeof insertCategorySchema>;

export type Problem = typeof problems.$inferSelect;
export type InsertProblem = z.infer<typeof insertProblemSchema>;

export type Submission = typeof submissions.$inferSelect;
export type InsertSubmission = z.infer<typeof insertSubmissionSchema>;

export type UserProgress = typeof userProgress.$inferSelect;
export type InsertUserProgress = z.infer<typeof insertUserProgressSchema>;

export type Contest = typeof contests.$inferSelect;
export type InsertContest = z.infer<typeof insertContestSchema>;

export type AiChat = typeof aiChats.$inferSelect;
export type InsertAiChat = z.infer<typeof insertAiChatSchema>;

export type Setting = typeof settings.$inferSelect;
export type InsertSetting = z.infer<typeof insertSettingSchema>;