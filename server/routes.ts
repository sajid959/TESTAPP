import type { Express } from "express";
import { createServer, type Server } from "http";
import { WebSocketServer, WebSocket } from "ws";
import bcryptjs from "bcryptjs";
import jwt from "jsonwebtoken";
import Stripe from "stripe";
import multer from "multer";
import * as XLSX from "xlsx";
import { storage } from "./storage";
import { insertUserSchema, insertProblemSchema, insertCategorySchema, insertSubmissionSchema } from "../shared/schema";
import type { User } from "../shared/schema";

// Environment variables validation
const JWT_SECRET = process.env.JWT_SECRET || "dev_secret_key_change_in_production";
const PERPLEXITY_API_KEY = process.env.PERPLEXITY_API_KEY;
const STRIPE_SECRET_KEY = process.env.STRIPE_SECRET_KEY;

// Initialize Stripe if key is available
let stripe: Stripe | null = null;
if (STRIPE_SECRET_KEY) {
  stripe = new Stripe(STRIPE_SECRET_KEY, {
    apiVersion: "2023-10-16",
  });
}

// Setup multer for file uploads
const upload = multer({
  dest: "uploads/",
  fileFilter: (req, file, cb) => {
    const allowedTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/vnd.ms-excel',
      'text/csv'
    ];
    cb(null, allowedTypes.includes(file.mimetype));
  },
  limits: {
    fileSize: 10 * 1024 * 1024 // 10MB limit
  }
});

// JWT middleware
const authenticateToken = async (req: any, res: any, next: any) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];

  if (!token) {
    return res.sendStatus(401);
  }

  try {
    const decoded = jwt.verify(token, JWT_SECRET) as any;
    const user = await storage.getUserById(decoded.userId);
    if (!user) {
      return res.sendStatus(401);
    }
    req.user = user;
    next();
  } catch (error) {
    res.sendStatus(403);
  }
};

// Admin middleware
const requireAdmin = (req: any, res: any, next: any) => {
  if (req.user?.role !== 'admin') {
    return res.sendStatus(403);
  }
  next();
};

// Perplexity AI service
class PerplexityService {
  private apiKey: string;

  constructor(apiKey: string) {
    this.apiKey = apiKey;
  }

  async generateHint(problemDescription: string, userCode?: string): Promise<string> {
    const messages = [
      {
        role: "system",
        content: "You are an expert programming tutor. Provide helpful hints for coding problems without giving away the complete solution. Be encouraging and educational."
      },
      {
        role: "user",
        content: `Problem: ${problemDescription}${userCode ? `\n\nUser's current code: ${userCode}` : ''}\n\nProvide a helpful hint to guide the user towards the solution.`
      }
    ];

    return this.makeRequest(messages);
  }

  async generateTestCases(problemDescription: string): Promise<Array<{ input: any; output: any; explanation?: string }>> {
    const messages = [
      {
        role: "system",
        content: "You are an expert at creating comprehensive test cases for coding problems. Generate diverse test cases including edge cases. Return only valid JSON array."
      },
      {
        role: "user",
        content: `Problem: ${problemDescription}\n\nGenerate 5-8 test cases in JSON format: [{"input": {...}, "output": {...}, "explanation": "..."}]`
      }
    ];

    const response = await this.makeRequest(messages);
    try {
      return JSON.parse(response);
    } catch {
      // Fallback if parsing fails
      return [
        { input: "example_input", output: "example_output", explanation: "Basic test case" }
      ];
    }
  }

  async debugCode(problemDescription: string, userCode: string, error: string): Promise<string> {
    const messages = [
      {
        role: "system",
        content: "You are an expert debugging assistant. Help identify and explain coding errors clearly and provide guidance on how to fix them."
      },
      {
        role: "user",
        content: `Problem: ${problemDescription}\n\nCode: ${userCode}\n\nError: ${error}\n\nPlease explain what's wrong and suggest how to fix it.`
      }
    ];

    return this.makeRequest(messages);
  }

  async autoCompleteFromExcel(title: string, existingData: any): Promise<any> {
    const messages = [
      {
        role: "system",
        content: "You are an expert at completing coding problem data. Fill in missing fields based on the problem title and any existing data. Return only valid JSON."
      },
      {
        role: "user",
        content: `Problem title: "${title}"\nExisting data: ${JSON.stringify(existingData)}\n\nComplete missing fields like description, difficulty (Easy/Medium/Hard), tags, and test cases. Return as JSON.`
      }
    ];

    const response = await this.makeRequest(messages);
    try {
      return JSON.parse(response);
    } catch {
      return {
        description: `Solve the problem: ${title}`,
        difficulty: "Medium",
        tags: ["Algorithm"],
        testCases: [{ input: "example", output: "result" }]
      };
    }
  }

  private async makeRequest(messages: any[]): Promise<string> {
    if (!this.apiKey) {
      throw new Error("Perplexity API key not configured");
    }

    const response = await fetch('https://api.perplexity.ai/chat/completions', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.apiKey}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        model: 'llama-3.1-sonar-small-128k-online',
        messages,
        max_tokens: 1000,
        temperature: 0.2,
        top_p: 0.9,
        stream: false
      })
    });

    if (!response.ok) {
      throw new Error(`Perplexity API error: ${response.statusText}`);
    }

    const data = await response.json();
    return data.choices[0]?.message?.content || "No response available";
  }
}

const perplexityService = PERPLEXITY_API_KEY ? new PerplexityService(PERPLEXITY_API_KEY) : null;

// Code execution service (mock for security)
class CodeExecutionService {
  async executeCode(code: string, language: string, testCases: any[]): Promise<{
    status: string;
    runtime?: number;
    memory?: number;
    testCasesPassed: number;
    totalTestCases: number;
    errorMessage?: string;
  }> {
    // In a real implementation, this would run code in a secure sandbox
    // For demo purposes, we'll simulate execution
    await new Promise(resolve => setTimeout(resolve, Math.random() * 2000 + 500));
    
    const passed = Math.floor(Math.random() * (testCases.length + 1));
    const statuses = ['Accepted', 'Wrong Answer', 'Time Limit Exceeded', 'Runtime Error'];
    const status = passed === testCases.length ? 'Accepted' : statuses[Math.floor(Math.random() * (statuses.length - 1)) + 1];
    
    return {
      status,
      runtime: Math.floor(Math.random() * 1000 + 100),
      memory: Math.floor(Math.random() * 50000 + 10000),
      testCasesPassed: passed,
      totalTestCases: testCases.length,
      errorMessage: status !== 'Accepted' ? 'Simulated error for demo' : undefined
    };
  }
}

const codeExecutionService = new CodeExecutionService();

export async function registerRoutes(app: Express): Promise<Server> {
  // Health check
  app.get("/api/health", (req, res) => {
    res.json({ 
      status: "ok", 
      timestamp: new Date().toISOString(),
      features: {
        database: !!process.env.DATABASE_URL,
        ai: !!PERPLEXITY_API_KEY,
        payments: !!STRIPE_SECRET_KEY
      }
    });
  });

  // ============ Authentication Routes ============
  app.post("/api/auth/register", async (req, res) => {
    try {
      const userData = insertUserSchema.parse(req.body);
      
      // Check if user already exists
      const existingUser = await storage.getUserByEmail(userData.email);
      if (existingUser) {
        return res.status(400).json({ message: "Email already registered" });
      }

      const existingUsername = await storage.getUserByUsername(userData.username);
      if (existingUsername) {
        return res.status(400).json({ message: "Username already taken" });
      }

      // Hash password
      const passwordHash = await bcryptjs.hash(userData.password, 12);
      
      // Create user
      const user = await storage.createUser({
        ...userData,
        passwordHash,
        emailVerificationToken: jwt.sign({ email: userData.email }, JWT_SECRET)
      });

      // Generate JWT
      const token = jwt.sign({ userId: user.id }, JWT_SECRET, { expiresIn: '7d' });
      
      res.status(201).json({
        user: { ...user, passwordHash: undefined },
        token
      });
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  app.post("/api/auth/login", async (req, res) => {
    try {
      const { email, password } = req.body;
      
      // Find user
      const user = await storage.getUserByEmail(email);
      if (!user || !user.passwordHash) {
        return res.status(401).json({ message: "Invalid credentials" });
      }

      // Verify password
      const isValid = await bcryptjs.compare(password, user.passwordHash);
      if (!isValid) {
        return res.status(401).json({ message: "Invalid credentials" });
      }

      // Generate JWT
      const token = jwt.sign({ userId: user.id }, JWT_SECRET, { expiresIn: '7d' });
      
      res.json({
        user: { ...user, passwordHash: undefined },
        token
      });
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  app.get("/api/auth/me", authenticateToken, async (req: any, res) => {
    res.json({ user: { ...req.user, passwordHash: undefined } });
  });

  // ============ Category Routes ============
  app.get("/api/categories", async (req, res) => {
    try {
      const categories = await storage.getCategories();
      res.json(categories);
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  app.post("/api/categories", authenticateToken, requireAdmin, async (req: any, res) => {
    try {
      const categoryData = insertCategorySchema.parse(req.body);
      const category = await storage.createCategory(categoryData);
      res.status(201).json(category);
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  app.put("/api/categories/:id", authenticateToken, requireAdmin, async (req: any, res) => {
    try {
      const { id } = req.params;
      const updates = req.body;
      const category = await storage.updateCategory(id, updates);
      res.json(category);
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  // ============ Problem Routes ============
  app.get("/api/problems", async (req, res) => {
    try {
      const filters = {
        categoryId: req.query.categoryId as string,
        difficulty: req.query.difficulty as string,
        isPremium: req.query.isPremium === 'true' ? true : req.query.isPremium === 'false' ? false : undefined,
        isApproved: req.query.isApproved === 'true' ? true : req.query.isApproved === 'false' ? false : undefined,
        search: req.query.search as string,
        offset: parseInt(req.query.offset as string) || 0,
        limit: parseInt(req.query.limit as string) || 50
      };

      const result = await storage.getProblems(filters);
      res.json(result);
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  app.get("/api/problems/:slug", async (req, res) => {
    try {
      const { slug } = req.params;
      const problem = await storage.getProblemBySlug(slug);
      
      if (!problem) {
        return res.status(404).json({ message: "Problem not found" });
      }

      res.json(problem);
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  app.post("/api/problems", authenticateToken, async (req: any, res) => {
    try {
      const problemData = insertProblemSchema.parse({
        ...req.body,
        createdBy: req.user.id,
        isApproved: req.user.role === 'admin'
      });

      const problem = await storage.createProblem(problemData);
      res.status(201).json(problem);
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  app.put("/api/problems/:id/approve", authenticateToken, requireAdmin, async (req: any, res) => {
    try {
      const { id } = req.params;
      const problem = await storage.approveProblem(id);
      res.json(problem);
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  // ============ Submission Routes ============
  app.post("/api/submissions", authenticateToken, async (req: any, res) => {
    try {
      const submissionData = insertSubmissionSchema.parse({
        ...req.body,
        userId: req.user.id
      });

      // Get problem to access test cases
      const problem = await storage.getProblemById(submissionData.problemId);
      if (!problem) {
        return res.status(404).json({ message: "Problem not found" });
      }

      // Execute code
      const result = await codeExecutionService.executeCode(
        submissionData.code,
        submissionData.language,
        problem.testCases as any[]
      );

      // Create submission with results
      const submission = await storage.createSubmission({
        ...submissionData,
        ...result
      });

      // Update user progress
      if (result.status === 'Accepted') {
        await storage.markProblemSolved(req.user.id, submissionData.problemId);
      } else {
        await storage.createOrUpdateProgress({
          userId: req.user.id,
          problemId: submissionData.problemId,
          status: 'in_progress',
          attempts: 1,
          lastAttemptAt: new Date()
        });
      }

      res.status(201).json(submission);
    } catch (error: any) {
      res.status(400).json({ message: error.message });
    }
  });

  app.get("/api/submissions", authenticateToken, async (req: any, res) => {
    try {
      const filters = {
        userId: req.query.userId as string || req.user.id,
        problemId: req.query.problemId as string,
        status: req.query.status as string,
        offset: parseInt(req.query.offset as string) || 0,
        limit: parseInt(req.query.limit as string) || 50
      };

      const result = await storage.getSubmissions(filters);
      res.json(result);
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  // ============ AI Routes ============
  app.post("/api/ai/hint", authenticateToken, async (req: any, res) => {
    try {
      if (!perplexityService) {
        return res.status(503).json({ message: "AI service not available" });
      }

      const { problemDescription, userCode } = req.body;
      const hint = await perplexityService.generateHint(problemDescription, userCode);
      
      res.json({ hint });
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  app.post("/api/ai/debug", authenticateToken, async (req: any, res) => {
    try {
      if (!perplexityService) {
        return res.status(503).json({ message: "AI service not available" });
      }

      const { problemDescription, userCode, error } = req.body;
      const debugInfo = await perplexityService.debugCode(problemDescription, userCode, error);
      
      res.json({ debugInfo });
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  app.post("/api/ai/generate-test-cases", authenticateToken, requireAdmin, async (req: any, res) => {
    try {
      if (!perplexityService) {
        return res.status(503).json({ message: "AI service not available" });
      }

      const { problemDescription } = req.body;
      const testCases = await perplexityService.generateTestCases(problemDescription);
      
      res.json({ testCases });
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  // ============ Bulk Import Routes ============
  app.post("/api/admin/bulk-import", authenticateToken, requireAdmin, upload.single('file'), async (req: any, res) => {
    try {
      if (!req.file) {
        return res.status(400).json({ message: "No file uploaded" });
      }

      // Read Excel/CSV file
      const workbook = XLSX.readFile(req.file.path);
      const sheetName = workbook.SheetNames[0];
      const data = XLSX.utils.sheet_to_json(workbook.Sheets[sheetName]);

      const results = [];
      const errors = [];

      for (const [index, row] of data.entries()) {
        try {
          let problemData = row as any;

          // AI auto-completion for missing data
          if (perplexityService && (!problemData.description || !problemData.testCases)) {
            const completed = await perplexityService.autoCompleteFromExcel(
              problemData.title || `Problem ${index + 1}`,
              problemData
            );
            problemData = { ...problemData, ...completed };
          }

          // Process and create problem
          const processed = insertProblemSchema.parse({
            title: problemData.title,
            slug: problemData.slug || problemData.title?.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
            description: problemData.description || `Solve: ${problemData.title}`,
            difficulty: problemData.difficulty || 'Medium',
            categoryId: problemData.categoryId,
            tags: typeof problemData.tags === 'string' ? problemData.tags.split(',').map((t: string) => t.trim()) : [],
            testCases: typeof problemData.testCases === 'string' ? JSON.parse(problemData.testCases) : (problemData.testCases || []),
            isPremium: problemData.isPremium === 'true' || problemData.isPremium === true,
            isApproved: true,
            createdBy: req.user.id
          });

          const problem = await storage.createProblem(processed);
          results.push(problem);
        } catch (error: any) {
          errors.push({ row: index + 1, error: error.message });
        }
      }

      res.json({
        success: results.length,
        errors: errors.length,
        results,
        errors
      });
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  // ============ Stripe Payment Routes ============
  if (stripe) {
    app.post("/api/create-payment-intent", authenticateToken, async (req: any, res) => {
      try {
        const { amount } = req.body;
        const paymentIntent = await stripe!.paymentIntents.create({
          amount: Math.round(amount * 100),
          currency: "usd",
          metadata: { userId: req.user.id }
        });
        res.json({ clientSecret: paymentIntent.client_secret });
      } catch (error: any) {
        res.status(500).json({ message: "Error creating payment intent: " + error.message });
      }
    });

    app.post('/api/get-or-create-subscription', authenticateToken, async (req: any, res) => {
      try {
        let user = req.user;

        if (user.stripeSubscriptionId) {
          const subscription = await stripe!.subscriptions.retrieve(user.stripeSubscriptionId);
          res.json({
            subscriptionId: subscription.id,
            clientSecret: subscription.latest_invoice?.payment_intent?.client_secret,
          });
          return;
        }
        
        if (!user.email) {
          throw new Error('No user email on file');
        }

        const customer = await stripe!.customers.create({
          email: user.email,
          name: user.username,
        });

        user = await storage.updateStripeCustomerId(user.id, customer.id);

        const subscription = await stripe!.subscriptions.create({
          customer: customer.id,
          items: [{
            price: process.env.STRIPE_PRICE_ID || 'price_test',
          }],
          payment_behavior: 'default_incomplete',
          expand: ['latest_invoice.payment_intent'],
        });

        await storage.updateUserStripeInfo(user.id, {
          customerId: customer.id, 
          subscriptionId: subscription.id
        });
    
        res.json({
          subscriptionId: subscription.id,
          clientSecret: subscription.latest_invoice?.payment_intent?.client_secret,
        });
      } catch (error: any) {
        res.status(400).json({ error: { message: error.message } });
      }
    });
  }

  // ============ Analytics Routes ============
  app.get("/api/analytics/stats", authenticateToken, async (req: any, res) => {
    try {
      const [userStats, problemStats] = await Promise.all([
        storage.getUserStats(req.user.id),
        storage.getProblemStats()
      ]);

      res.json({
        user: userStats,
        problems: problemStats
      });
    } catch (error: any) {
      res.status(500).json({ message: error.message });
    }
  });

  const httpServer = createServer(app);

  // WebSocket setup for real-time features
  const wss = new WebSocketServer({ 
    server: httpServer, 
    path: '/ws' 
  });

  const activeConnections = new Map<string, Set<WebSocket>>();

  wss.on('connection', (ws: WebSocket, req) => {
    const url = new URL(req.url!, `http://${req.headers.host}`);
    const token = url.searchParams.get('token');
    
    if (!token) {
      ws.close(1008, 'No token provided');
      return;
    }

    try {
      const decoded = jwt.verify(token, JWT_SECRET) as any;
      const userId = decoded.userId;
      
      if (!activeConnections.has(userId)) {
        activeConnections.set(userId, new Set());
      }
      activeConnections.get(userId)!.add(ws);

      ws.on('message', async (data) => {
        try {
          const message = JSON.parse(data.toString());
          
          // Handle different message types
          switch (message.type) {
            case 'code_update':
              // Broadcast code updates to other sessions
              break;
            case 'submission_update':
              // Real-time submission status updates
              break;
          }
        } catch (error) {
          console.error('WebSocket message error:', error);
        }
      });

      ws.on('close', () => {
        const userConnections = activeConnections.get(userId);
        if (userConnections) {
          userConnections.delete(ws);
          if (userConnections.size === 0) {
            activeConnections.delete(userId);
          }
        }
      });

    } catch (error) {
      ws.close(1008, 'Invalid token');
    }
  });

  return httpServer;
}