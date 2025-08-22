import bcryptjs from "bcryptjs";
import { storage } from "./storage";

export async function createDefaultAdmin() {
  try {
    // Check if admin already exists
    const existingAdmin = await storage.getUserByEmail("admin@dsagrind.com");
    if (existingAdmin) {
      console.log("Admin user already exists");
      return existingAdmin;
    }

    // Create default admin user
    const adminData = {
      username: "admin",
      email: "admin@dsagrind.com", 
      passwordHash: await bcryptjs.hash("admin123", 12),
      firstName: "Admin",
      lastName: "User",
      role: "admin" as const,
      isEmailVerified: true,
      isPremium: true,
      emailVerificationToken: null
    };

    const admin = await storage.createUser(adminData);
    console.log("Default admin user created:", admin.email);
    return admin;

  } catch (error) {
    console.error("Error creating admin user:", error);
    throw error;
  }
}

export async function seedCategories() {
  try {
    // Check if categories already exist
    const existingCategories = await storage.getCategories();
    if (existingCategories.length > 0) {
      console.log("Categories already exist");
      return existingCategories;
    }

    // Create the three requested categories
    const categories = [
      {
        name: "LeetCode 350",
        slug: "leetcode-350",
        description: "Top 350 LeetCode interview questions for comprehensive preparation",
        icon: "🎯",
        freeQuestionLimit: 20
      },
      {
        name: "LeetCode 150", 
        slug: "leetcode-150",
        description: "Essential 150 questions covering all important patterns and algorithms",
        icon: "⚡", 
        freeQuestionLimit: 15
      },
      {
        name: "LeetCode 75",
        slug: "leetcode-75",
        description: "Must-know 75 questions for cracking coding interviews", 
        icon: "🔥",
        freeQuestionLimit: 10
      }
    ];

    const createdCategories = [];
    for (const categoryData of categories) {
      const category = await storage.createCategory(categoryData);
      createdCategories.push(category);
      console.log(`Created category: ${category.name}`);
    }

    return createdCategories;
  } catch (error) {
    console.error("Error seeding categories:", error);
    throw error;
  }
}

export async function seedProblems() {
  try {
    const categories = await storage.getCategories();
    if (categories.length === 0) {
      throw new Error("Categories must be created before seeding problems");
    }

    // Check if problems already exist
    const existingProblems = await storage.getProblems();
    if (existingProblems.problems.length > 0) {
      console.log("Problems already exist");
      return existingProblems;
    }

    const { LEETCODE_75_PROBLEMS, LEETCODE_150_PROBLEMS, LEETCODE_350_PROBLEMS } = await import('./leetcode-problems');

    // Find categories
    const leetcode75Category = categories.find(c => c.slug === "leetcode-75");
    const leetcode150Category = categories.find(c => c.slug === "leetcode-150");
    const leetcode350Category = categories.find(c => c.slug === "leetcode-350");

    const createdProblems = [];

    // Seed LeetCode 75 problems
    if (leetcode75Category) {
      for (const problemData of LEETCODE_75_PROBLEMS) {
        try {
          const problem = await storage.createProblem({
            ...problemData,
            categoryId: leetcode75Category.id
          });
          createdProblems.push(problem);
          console.log(`Created LC75 problem: ${problem.title}`);
        } catch (error: any) {
          if (!error.message.includes('duplicate key')) {
            throw error;
          }
        }
      }
    }

    // Seed LeetCode 150 problems (additional problems only)
    if (leetcode150Category) {
      const additional150Problems = LEETCODE_150_PROBLEMS.slice(LEETCODE_75_PROBLEMS.length);
      for (const problemData of additional150Problems) {
        try {
          const problem = await storage.createProblem({
            ...problemData,
            categoryId: leetcode150Category.id
          });
          createdProblems.push(problem);
          console.log(`Created LC150 problem: ${problem.title}`);
        } catch (error: any) {
          if (!error.message.includes('duplicate key')) {
            throw error;
          }
        }
      }
    }

    // Seed LeetCode 350 problems (additional problems only)
    if (leetcode350Category) {
      const additional350Problems = LEETCODE_350_PROBLEMS.slice(LEETCODE_150_PROBLEMS.length);
      for (const problemData of additional350Problems) {
        try {
          const problem = await storage.createProblem({
            ...problemData,
            categoryId: leetcode350Category.id
          });
          createdProblems.push(problem);
          console.log(`Created LC350 problem: ${problem.title}`);
        } catch (error: any) {
          if (!error.message.includes('duplicate key')) {
            throw error;
          }
        }
      }
    }

    return createdProblems;
  } catch (error) {
    console.error("Error seeding problems:", error);
    throw error;
  }
}