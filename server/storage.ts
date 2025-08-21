import { drizzle } from "drizzle-orm/neon-http";
import { neon } from "@neondatabase/serverless";
import { eq, desc, asc, and, or, count, sql, ilike } from "drizzle-orm";
import type {
  User, InsertUser, Category, InsertCategory, Problem, InsertProblem,
  Submission, InsertSubmission, UserProgress, InsertUserProgress,
  Contest, InsertContest, AiChat, InsertAiChat, Setting, InsertSetting
} from "../shared/schema";
import {
  users, categories, problems, submissions, userProgress,
  contests, aiChats, settings
} from "../shared/schema";

// Database connection using environment variable
const connectionString = process.env.DATABASE_URL;
if (!connectionString) {
  throw new Error("DATABASE_URL environment variable is required");
}

const sql_client = neon(connectionString);
const db = drizzle(sql_client);

export interface IStorage {
  // User management
  createUser(user: InsertUser): Promise<User>;
  getUserById(id: string): Promise<User | undefined>;
  getUserByEmail(email: string): Promise<User | undefined>;
  getUserByUsername(username: string): Promise<User | undefined>;
  getUserByGithubId(githubId: string): Promise<User | undefined>;
  getUserByGoogleId(googleId: string): Promise<User | undefined>;
  updateUser(id: string, updates: Partial<User>): Promise<User>;
  updateStripeCustomerId(userId: string, customerId: string): Promise<User>;
  updateUserStripeInfo(userId: string, info: { customerId: string; subscriptionId: string }): Promise<User>;
  verifyUserEmail(token: string): Promise<User | undefined>;
  setResetPasswordToken(email: string, token: string, expires: Date): Promise<boolean>;
  resetPassword(token: string, passwordHash: string): Promise<User | undefined>;
  
  // Category management
  createCategory(category: InsertCategory): Promise<Category>;
  getCategories(): Promise<Category[]>;
  getCategoryById(id: string): Promise<Category | undefined>;
  getCategoryBySlug(slug: string): Promise<Category | undefined>;
  updateCategory(id: string, updates: Partial<Category>): Promise<Category>;
  deleteCategory(id: string): Promise<boolean>;
  updateCategoryQuestionCount(categoryId: string): Promise<void>;
  
  // Problem management
  createProblem(problem: InsertProblem): Promise<Problem>;
  getProblems(filters?: { categoryId?: string; difficulty?: string; isPremium?: boolean; isApproved?: boolean; search?: string; offset?: number; limit?: number }): Promise<{ problems: Problem[]; total: number }>;
  getProblemById(id: string): Promise<Problem | undefined>;
  getProblemBySlug(slug: string): Promise<Problem | undefined>;
  updateProblem(id: string, updates: Partial<Problem>): Promise<Problem>;
  deleteProblem(id: string): Promise<boolean>;
  approveProblem(id: string): Promise<Problem>;
  likeProblem(id: string, increment: boolean): Promise<void>;
  
  // Submission management
  createSubmission(submission: InsertSubmission): Promise<Submission>;
  getSubmissions(filters?: { userId?: string; problemId?: string; status?: string; offset?: number; limit?: number }): Promise<{ submissions: Submission[]; total: number }>;
  getSubmissionById(id: string): Promise<Submission | undefined>;
  getUserSubmissionsForProblem(userId: string, problemId: string): Promise<Submission[]>;
  
  // User progress tracking
  createOrUpdateProgress(progress: InsertUserProgress): Promise<UserProgress>;
  getUserProgress(userId: string, problemId: string): Promise<UserProgress | undefined>;
  getUserAllProgress(userId: string): Promise<UserProgress[]>;
  markProblemSolved(userId: string, problemId: string): Promise<void>;
  
  // Contest management
  createContest(contest: InsertContest): Promise<Contest>;
  getContests(active?: boolean): Promise<Contest[]>;
  getContestById(id: string): Promise<Contest | undefined>;
  updateContest(id: string, updates: Partial<Contest>): Promise<Contest>;
  
  // AI Chat management
  createAiChat(chat: InsertAiChat): Promise<AiChat>;
  getAiChatsBySession(sessionId: string): Promise<AiChat[]>;
  updateAiChat(id: string, messages: any[]): Promise<AiChat>;
  
  // Settings management
  createOrUpdateSetting(setting: InsertSetting): Promise<Setting>;
  getSetting(key: string): Promise<Setting | undefined>;
  getAllSettings(): Promise<Setting[]>;
  
  // Analytics and stats
  getUserStats(userId: string): Promise<{ totalSolved: number; rank: number; recentSubmissions: number }>;
  getProblemStats(): Promise<{ total: number; easy: number; medium: number; hard: number; premium: number }>;
  getCategoryStats(): Promise<Array<{ categoryId: string; categoryName: string; total: number; solved: number }>>;
}

class PostgresStorage implements IStorage {
  // User methods
  async createUser(user: InsertUser): Promise<User> {
    const [newUser] = await db.insert(users).values(user).returning();
    return newUser;
  }

  async getUserById(id: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.id, id));
    return user;
  }

  async getUserByEmail(email: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.email, email));
    return user;
  }

  async getUserByUsername(username: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.username, username));
    return user;
  }

  async getUserByGithubId(githubId: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.githubId, githubId));
    return user;
  }

  async getUserByGoogleId(googleId: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.googleId, googleId));
    return user;
  }

  async updateUser(id: string, updates: Partial<User>): Promise<User> {
    const [updatedUser] = await db.update(users)
      .set({ ...updates, updatedAt: new Date() })
      .where(eq(users.id, id))
      .returning();
    return updatedUser;
  }

  async updateStripeCustomerId(userId: string, customerId: string): Promise<User> {
    return this.updateUser(userId, { stripeCustomerId: customerId });
  }

  async updateUserStripeInfo(userId: string, info: { customerId: string; subscriptionId: string }): Promise<User> {
    return this.updateUser(userId, { 
      stripeCustomerId: info.customerId, 
      stripeSubscriptionId: info.subscriptionId 
    });
  }

  async verifyUserEmail(token: string): Promise<User | undefined> {
    const [user] = await db.select().from(users).where(eq(users.emailVerificationToken, token));
    if (user) {
      await this.updateUser(user.id, { 
        isEmailVerified: true, 
        emailVerificationToken: null 
      });
      return user;
    }
    return undefined;
  }

  async setResetPasswordToken(email: string, token: string, expires: Date): Promise<boolean> {
    const result = await db.update(users)
      .set({ resetPasswordToken: token, resetPasswordExpires: expires })
      .where(eq(users.email, email));
    return true;
  }

  async resetPassword(token: string, passwordHash: string): Promise<User | undefined> {
    const [user] = await db.select().from(users)
      .where(and(
        eq(users.resetPasswordToken, token),
        sql`${users.resetPasswordExpires} > NOW()`
      ));
    
    if (user) {
      return this.updateUser(user.id, {
        passwordHash,
        resetPasswordToken: null,
        resetPasswordExpires: null
      });
    }
    return undefined;
  }

  // Category methods
  async createCategory(category: InsertCategory): Promise<Category> {
    const [newCategory] = await db.insert(categories).values(category).returning();
    return newCategory;
  }

  async getCategories(): Promise<Category[]> {
    return db.select().from(categories).where(eq(categories.isActive, true)).orderBy(asc(categories.name));
  }

  async getCategoryById(id: string): Promise<Category | undefined> {
    const [category] = await db.select().from(categories).where(eq(categories.id, id));
    return category;
  }

  async getCategoryBySlug(slug: string): Promise<Category | undefined> {
    const [category] = await db.select().from(categories).where(eq(categories.slug, slug));
    return category;
  }

  async updateCategory(id: string, updates: Partial<Category>): Promise<Category> {
    const [updatedCategory] = await db.update(categories)
      .set({ ...updates, updatedAt: new Date() })
      .where(eq(categories.id, id))
      .returning();
    return updatedCategory;
  }

  async deleteCategory(id: string): Promise<boolean> {
    await db.update(categories)
      .set({ isActive: false })
      .where(eq(categories.id, id));
    return true;
  }

  async updateCategoryQuestionCount(categoryId: string): Promise<void> {
    const [result] = await db.select({ count: count() })
      .from(problems)
      .where(and(eq(problems.categoryId, categoryId), eq(problems.isApproved, true)));
    
    await db.update(categories)
      .set({ totalQuestions: result.count })
      .where(eq(categories.id, categoryId));
  }

  // Problem methods
  async createProblem(problem: InsertProblem): Promise<Problem> {
    const [newProblem] = await db.insert(problems).values(problem).returning();
    await this.updateCategoryQuestionCount(newProblem.categoryId);
    return newProblem;
  }

  async getProblems(filters: { 
    categoryId?: string; 
    difficulty?: string; 
    isPremium?: boolean; 
    isApproved?: boolean; 
    search?: string; 
    offset?: number; 
    limit?: number;
  } = {}): Promise<{ problems: Problem[]; total: number }> {
    const conditions = [];
    
    if (filters.categoryId) conditions.push(eq(problems.categoryId, filters.categoryId));
    if (filters.difficulty) conditions.push(eq(problems.difficulty, filters.difficulty));
    if (filters.isPremium !== undefined) conditions.push(eq(problems.isPremium, filters.isPremium));
    if (filters.isApproved !== undefined) conditions.push(eq(problems.isApproved, filters.isApproved));
    if (filters.search) {
      conditions.push(or(
        ilike(problems.title, `%${filters.search}%`),
        ilike(problems.description, `%${filters.search}%`)
      ));
    }

    const whereClause = conditions.length > 0 ? and(...conditions) : undefined;
    
    const [problemsResult, totalResult] = await Promise.all([
      db.select().from(problems)
        .where(whereClause)
        .orderBy(asc(problems.title))
        .limit(filters.limit || 50)
        .offset(filters.offset || 0),
      db.select({ count: count() }).from(problems).where(whereClause)
    ]);

    return {
      problems: problemsResult,
      total: totalResult[0].count
    };
  }

  async getProblemById(id: string): Promise<Problem | undefined> {
    const [problem] = await db.select().from(problems).where(eq(problems.id, id));
    return problem;
  }

  async getProblemBySlug(slug: string): Promise<Problem | undefined> {
    const [problem] = await db.select().from(problems).where(eq(problems.slug, slug));
    return problem;
  }

  async updateProblem(id: string, updates: Partial<Problem>): Promise<Problem> {
    const [updatedProblem] = await db.update(problems)
      .set({ ...updates, updatedAt: new Date() })
      .where(eq(problems.id, id))
      .returning();
    return updatedProblem;
  }

  async deleteProblem(id: string): Promise<boolean> {
    const [problem] = await db.delete(problems).where(eq(problems.id, id)).returning();
    if (problem) {
      await this.updateCategoryQuestionCount(problem.categoryId);
    }
    return !!problem;
  }

  async approveProblem(id: string): Promise<Problem> {
    return this.updateProblem(id, { isApproved: true });
  }

  async likeProblem(id: string, increment: boolean): Promise<void> {
    if (increment) {
      await db.update(problems)
        .set({ likes: sql`${problems.likes} + 1` })
        .where(eq(problems.id, id));
    } else {
      await db.update(problems)
        .set({ dislikes: sql`${problems.dislikes} + 1` })
        .where(eq(problems.id, id));
    }
  }

  // Submission methods
  async createSubmission(submission: InsertSubmission): Promise<Submission> {
    const [newSubmission] = await db.insert(submissions).values(submission).returning();
    
    // Update problem statistics
    await db.update(problems)
      .set({ submissions: sql`${problems.submissions} + 1` })
      .where(eq(problems.id, submission.problemId));
    
    if (newSubmission.status === 'Accepted') {
      await db.update(problems)
        .set({ acceptedSubmissions: sql`${problems.acceptedSubmissions} + 1` })
        .where(eq(problems.id, submission.problemId));
    }
    
    return newSubmission;
  }

  async getSubmissions(filters: { 
    userId?: string; 
    problemId?: string; 
    status?: string; 
    offset?: number; 
    limit?: number;
  } = {}): Promise<{ submissions: Submission[]; total: number }> {
    const conditions = [];
    
    if (filters.userId) conditions.push(eq(submissions.userId, filters.userId));
    if (filters.problemId) conditions.push(eq(submissions.problemId, filters.problemId));
    if (filters.status) conditions.push(eq(submissions.status, filters.status));

    const whereClause = conditions.length > 0 ? and(...conditions) : undefined;
    
    const [submissionsResult, totalResult] = await Promise.all([
      db.select().from(submissions)
        .where(whereClause)
        .orderBy(desc(submissions.createdAt))
        .limit(filters.limit || 50)
        .offset(filters.offset || 0),
      db.select({ count: count() }).from(submissions).where(whereClause)
    ]);

    return {
      submissions: submissionsResult,
      total: totalResult[0].count
    };
  }

  async getSubmissionById(id: string): Promise<Submission | undefined> {
    const [submission] = await db.select().from(submissions).where(eq(submissions.id, id));
    return submission;
  }

  async getUserSubmissionsForProblem(userId: string, problemId: string): Promise<Submission[]> {
    return db.select().from(submissions)
      .where(and(eq(submissions.userId, userId), eq(submissions.problemId, problemId)))
      .orderBy(desc(submissions.createdAt));
  }

  // User progress methods
  async createOrUpdateProgress(progress: InsertUserProgress): Promise<UserProgress> {
    const existing = await this.getUserProgress(progress.userId, progress.problemId);
    
    if (existing) {
      const [updated] = await db.update(userProgress)
        .set({ ...progress, updatedAt: new Date() })
        .where(and(eq(userProgress.userId, progress.userId), eq(userProgress.problemId, progress.problemId)))
        .returning();
      return updated;
    } else {
      const [created] = await db.insert(userProgress).values(progress).returning();
      return created;
    }
  }

  async getUserProgress(userId: string, problemId: string): Promise<UserProgress | undefined> {
    const [progress] = await db.select().from(userProgress)
      .where(and(eq(userProgress.userId, userId), eq(userProgress.problemId, problemId)));
    return progress;
  }

  async getUserAllProgress(userId: string): Promise<UserProgress[]> {
    return db.select().from(userProgress).where(eq(userProgress.userId, userId));
  }

  async markProblemSolved(userId: string, problemId: string): Promise<void> {
    await this.createOrUpdateProgress({
      userId,
      problemId,
      status: 'solved',
      solvedAt: new Date()
    });

    // Update user's total solved count
    const solvedCount = await db.select({ count: count() })
      .from(userProgress)
      .where(and(eq(userProgress.userId, userId), eq(userProgress.status, 'solved')));
    
    await db.update(users)
      .set({ totalSolved: solvedCount[0].count })
      .where(eq(users.id, userId));
  }

  // Contest methods
  async createContest(contest: InsertContest): Promise<Contest> {
    const [newContest] = await db.insert(contests).values(contest).returning();
    return newContest;
  }

  async getContests(active?: boolean): Promise<Contest[]> {
    const query = db.select().from(contests);
    if (active !== undefined) {
      query.where(eq(contests.isActive, active));
    }
    return query.orderBy(desc(contests.startTime));
  }

  async getContestById(id: string): Promise<Contest | undefined> {
    const [contest] = await db.select().from(contests).where(eq(contests.id, id));
    return contest;
  }

  async updateContest(id: string, updates: Partial<Contest>): Promise<Contest> {
    const [updatedContest] = await db.update(contests)
      .set(updates)
      .where(eq(contests.id, id))
      .returning();
    return updatedContest;
  }

  // AI Chat methods
  async createAiChat(chat: InsertAiChat): Promise<AiChat> {
    const [newChat] = await db.insert(aiChats).values(chat).returning();
    return newChat;
  }

  async getAiChatsBySession(sessionId: string): Promise<AiChat[]> {
    return db.select().from(aiChats)
      .where(eq(aiChats.sessionId, sessionId))
      .orderBy(asc(aiChats.createdAt));
  }

  async updateAiChat(id: string, messages: any[]): Promise<AiChat> {
    const [updated] = await db.update(aiChats)
      .set({ messages })
      .where(eq(aiChats.id, id))
      .returning();
    return updated;
  }

  // Settings methods
  async createOrUpdateSetting(setting: InsertSetting): Promise<Setting> {
    const existing = await this.getSetting(setting.key);
    
    if (existing) {
      const [updated] = await db.update(settings)
        .set({ ...setting, updatedAt: new Date() })
        .where(eq(settings.key, setting.key))
        .returning();
      return updated;
    } else {
      const [created] = await db.insert(settings).values(setting).returning();
      return created;
    }
  }

  async getSetting(key: string): Promise<Setting | undefined> {
    const [setting] = await db.select().from(settings).where(eq(settings.key, key));
    return setting;
  }

  async getAllSettings(): Promise<Setting[]> {
    return db.select().from(settings);
  }

  // Analytics methods
  async getUserStats(userId: string): Promise<{ totalSolved: number; rank: number; recentSubmissions: number }> {
    const [user] = await db.select().from(users).where(eq(users.id, userId));
    const recentSubmissions = await db.select({ count: count() })
      .from(submissions)
      .where(and(
        eq(submissions.userId, userId),
        sql`${submissions.createdAt} > NOW() - INTERVAL '7 days'`
      ));

    return {
      totalSolved: user?.totalSolved || 0,
      rank: user?.rank || 0,
      recentSubmissions: recentSubmissions[0].count
    };
  }

  async getProblemStats(): Promise<{ total: number; easy: number; medium: number; hard: number; premium: number }> {
    const stats = await db.select({
      total: count(),
      easy: count(sql`CASE WHEN ${problems.difficulty} = 'Easy' THEN 1 END`),
      medium: count(sql`CASE WHEN ${problems.difficulty} = 'Medium' THEN 1 END`),
      hard: count(sql`CASE WHEN ${problems.difficulty} = 'Hard' THEN 1 END`),
      premium: count(sql`CASE WHEN ${problems.isPremium} = true THEN 1 END`)
    }).from(problems).where(eq(problems.isApproved, true));

    return stats[0];
  }

  async getCategoryStats(): Promise<Array<{ categoryId: string; categoryName: string; total: number; solved: number }>> {
    // This would need a more complex query in a real implementation
    return [];
  }
}

export const storage = new PostgresStorage();