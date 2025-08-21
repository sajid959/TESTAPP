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

    // Sample problems for each category
    const leetcode75Problems = [
      {
        title: "Two Sum",
        slug: "two-sum",
        description: `Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.

You may assume that each input would have exactly one solution, and you may not use the same element twice.

You can return the answer in any order.`,
        difficulty: "Easy" as const,
        tags: ["Array", "Hash Table"],
        hints: [
          "A brute force approach is to check every combination of 2 numbers.",
          "Use a hash map to store complements and their indices."
        ],
        constraints: "2 <= nums.length <= 10^4\n-10^9 <= nums[i] <= 10^9\n-10^9 <= target <= 10^9",
        examples: [
          {
            input: "nums = [2,7,11,15], target = 9",
            output: "[0,1]",
            explanation: "Because nums[0] + nums[1] == 9, we return [0, 1]."
          }
        ],
        testCases: [
          { input: { nums: [2, 7, 11, 15], target: 9 }, output: [0, 1] },
          { input: { nums: [3, 2, 4], target: 6 }, output: [1, 2] },
          { input: { nums: [3, 3], target: 6 }, output: [0, 1] }
        ],
        isPremium: false,
        isApproved: true
      },
      {
        title: "Best Time to Buy and Sell Stock",
        slug: "best-time-to-buy-and-sell-stock",
        description: `You are given an array prices where prices[i] is the price of a given stock on the ith day.

You want to maximize your profit by choosing a single day to buy one stock and choosing a different day in the future to sell that stock.

Return the maximum profit you can achieve from this transaction. If you cannot achieve any profit, return 0.`,
        difficulty: "Easy" as const,
        tags: ["Array", "Dynamic Programming"],
        hints: [
          "Think about tracking the minimum price seen so far.",
          "For each day, calculate profit if we sell on that day."
        ],
        constraints: "1 <= prices.length <= 10^5\n0 <= prices[i] <= 10^4",
        examples: [
          {
            input: "prices = [7,1,5,3,6,4]", 
            output: "5",
            explanation: "Buy on day 2 (price = 1) and sell on day 5 (price = 6), profit = 6-1 = 5."
          }
        ],
        testCases: [
          { input: { prices: [7, 1, 5, 3, 6, 4] }, output: 5 },
          { input: { prices: [7, 6, 4, 3, 1] }, output: 0 },
          { input: { prices: [1, 2] }, output: 1 }
        ],
        isPremium: false,
        isApproved: true
      },
      {
        title: "Contains Duplicate",
        slug: "contains-duplicate",
        description: `Given an integer array nums, return true if any value appears at least twice in the array, and return false if every element is distinct.`,
        difficulty: "Easy" as const,
        tags: ["Array", "Hash Table", "Sorting"],
        hints: [
          "Use a hash set to track seen elements.",
          "Alternatively, sort the array and check adjacent elements."
        ],
        constraints: "1 <= nums.length <= 10^5\n-10^9 <= nums[i] <= 10^9",
        examples: [
          {
            input: "nums = [1,2,3,1]",
            output: "true"
          }
        ],
        testCases: [
          { input: { nums: [1, 2, 3, 1] }, output: true },
          { input: { nums: [1, 2, 3, 4] }, output: false },
          { input: { nums: [1, 1, 1, 3, 3, 4, 3, 2, 4, 2] }, output: true }
        ],
        isPremium: false,
        isApproved: true
      },
      {
        title: "Valid Anagram",
        slug: "valid-anagram",
        description: `Given two strings s and t, return true if t is an anagram of s, and false otherwise.

An Anagram is a word or phrase formed by rearranging the letters of a different word or phrase, typically using all the original letters exactly once.`,
        difficulty: "Easy" as const,
        tags: ["Hash Table", "String", "Sorting"],
        hints: [
          "Count frequency of each character in both strings.",
          "Alternatively, sort both strings and compare."
        ],
        constraints: "1 <= s.length, t.length <= 5 * 10^4\ns and t consist of lowercase English letters.",
        examples: [
          {
            input: 's = "anagram", t = "nagaram"',
            output: "true"
          }
        ],
        testCases: [
          { input: { s: "anagram", t: "nagaram" }, output: true },
          { input: { s: "rat", t: "car" }, output: false }
        ],
        isPremium: false,
        isApproved: true
      },
      {
        title: "Group Anagrams",
        slug: "group-anagrams", 
        description: `Given an array of strings strs, group the anagrams together. You can return the answer in any order.

An Anagram is a word or phrase formed by rearranging the letters of a different word or phrase, typically using all the original letters exactly once.`,
        difficulty: "Medium" as const,
        tags: ["Array", "Hash Table", "String", "Sorting"],
        hints: [
          "Use sorted string as a key to group anagrams.",
          "Alternatively, use character frequency as a key."
        ],
        constraints: '1 <= strs.length <= 10^4\n0 <= strs[i].length <= 100\nstrs[i] consists of lowercase English letters.',
        examples: [
          {
            input: 'strs = ["eat","tea","tan","ate","nat","bat"]',
            output: '[["bat"],["nat","tan"],["ate","eat","tea"]]'
          }
        ],
        testCases: [
          { 
            input: { strs: ["eat", "tea", "tan", "ate", "nat", "bat"] }, 
            output: [["bat"], ["nat", "tan"], ["ate", "eat", "tea"]] 
          }
        ],
        isPremium: false,
        isApproved: true
      }
    ];

    const leetcode75Category = categories.find(c => c.slug === "leetcode-75");
    if (!leetcode75Category) {
      throw new Error("LeetCode 75 category not found");
    }

    const createdProblems = [];
    for (const problemData of leetcode75Problems) {
      const problem = await storage.createProblem({
        ...problemData,
        categoryId: leetcode75Category.id
      });
      createdProblems.push(problem);
      console.log(`Created problem: ${problem.title}`);
    }

    return createdProblems;
  } catch (error) {
    console.error("Error seeding problems:", error);
    throw error;
  }
}