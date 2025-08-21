// Real LeetCode problems data for our categories

export const LEETCODE_75_PROBLEMS = [
  // Array / String
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
    constraints: "2 <= nums.length <= 10^4\n-10^9 <= nums[i] <= 10^9\n-10^9 <= target <= 10^9\nOnly one valid answer exists.",
    examples: [
      {
        input: "nums = [2,7,11,15], target = 9",
        output: "[0,1]", 
        explanation: "Because nums[0] + nums[1] == 9, we return [0, 1]."
      },
      {
        input: "nums = [3,2,4], target = 6",
        output: "[1,2]"
      },
      {
        input: "nums = [3,3], target = 6", 
        output: "[0,1]"
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
      },
      {
        input: "prices = [7,6,4,3,1]",
        output: "0",
        explanation: "No profitable transaction possible."
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
      },
      {
        input: "nums = [1,2,3,4]", 
        output: "false"
      },
      {
        input: "nums = [1,1,1,3,3,4,3,2,4,2]",
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
      },
      {
        input: 's = "rat", t = "car"',
        output: "false"
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
    constraints: "1 <= strs.length <= 10^4\n0 <= strs[i].length <= 100\nstrs[i] consists of lowercase English letters.",
    examples: [
      {
        input: 'strs = ["eat","tea","tan","ate","nat","bat"]',
        output: '[["bat"],["nat","tan"],["ate","eat","tea"]]'
      },
      {
        input: 'strs = [""]',
        output: '[[""]]'
      },
      {
        input: 'strs = ["a"]',
        output: '[["a"]]'
      }
    ],
    testCases: [
      { 
        input: { strs: ["eat", "tea", "tan", "ate", "nat", "bat"] }, 
        output: [["bat"], ["nat", "tan"], ["ate", "eat", "tea"]] 
      },
      { input: { strs: [""] }, output: [[""]] },
      { input: { strs: ["a"] }, output: [["a"]] }
    ],
    isPremium: false,
    isApproved: true
  }
];

export const LEETCODE_150_PROBLEMS = [
  // This will be extended with the 150 interview questions
  ...LEETCODE_75_PROBLEMS,
  {
    title: "3Sum",
    slug: "3sum",
    description: `Given an integer array nums, return all the triplets [nums[i], nums[j], nums[k]] such that i != j, i != k, and j != k, and nums[i] + nums[j] + nums[k] == 0.

Notice that the solution set must not contain duplicate triplets.`,
    difficulty: "Medium" as const,
    tags: ["Array", "Two Pointers", "Sorting"],
    hints: [
      "Sort the array first to handle duplicates easier.",
      "Use two pointers technique for each fixed element.",
      "Skip duplicates to avoid duplicate triplets."
    ],
    constraints: "3 <= nums.length <= 3000\n-10^5 <= nums[i] <= 10^5",
    examples: [
      {
        input: "nums = [-1,0,1,2,-1,-4]",
        output: "[[-1,-1,2],[-1,0,1]]",
        explanation: "nums[0] + nums[1] + nums[2] = (-1) + 0 + 1 = 0."
      },
      {
        input: "nums = [0,1,1]",
        output: "[]",
        explanation: "The only possible triplet does not sum up to 0."
      },
      {
        input: "nums = [0,0,0]",
        output: "[[0,0,0]]",
        explanation: "The only possible triplet sums up to 0."
      }
    ],
    testCases: [
      { input: { nums: [-1, 0, 1, 2, -1, -4] }, output: [[-1, -1, 2], [-1, 0, 1]] },
      { input: { nums: [0, 1, 1] }, output: [] },
      { input: { nums: [0, 0, 0] }, output: [[0, 0, 0]] }
    ],
    isPremium: false,
    isApproved: true
  },
  {
    title: "Container With Most Water",
    slug: "container-with-most-water",
    description: `You are given an integer array height of length n. There are n vertical lines drawn such that the two endpoints of the ith line are (i, 0) and (i, height[i]).

Find two lines that together with the x-axis form a container, such that the container contains the most water.

Return the maximum amount of water a container can store.

Notice that you may not slant the container.`,
    difficulty: "Medium" as const,
    tags: ["Array", "Two Pointers", "Greedy"],
    hints: [
      "Start with the widest container and gradually narrow it.",
      "Always move the pointer with smaller height.",
      "The area is limited by the shorter line."
    ],
    constraints: "n == height.length\n2 <= n <= 10^5\n0 <= height[i] <= 10^4",
    examples: [
      {
        input: "height = [1,8,6,2,5,4,8,3,7]",
        output: "49",
        explanation: "The vertical lines are at indices 1 and 8 with heights 8 and 7."
      },
      {
        input: "height = [1,1]",
        output: "1",
        explanation: "The container can store at most 1 unit of water."
      }
    ],
    testCases: [
      { input: { height: [1, 8, 6, 2, 5, 4, 8, 3, 7] }, output: 49 },
      { input: { height: [1, 1] }, output: 1 },
      { input: { height: [4, 3, 2, 1, 4] }, output: 16 }
    ],
    isPremium: false,
    isApproved: true
  }
];

export const LEETCODE_350_PROBLEMS = [
  // This will include all 150 problems plus additional 200 problems
  ...LEETCODE_150_PROBLEMS,
  {
    title: "Maximum Subarray",
    slug: "maximum-subarray",
    description: `Given an integer array nums, find the subarray with the largest sum, and return its sum.`,
    difficulty: "Medium" as const,
    tags: ["Array", "Dynamic Programming", "Divide and Conquer"],
    hints: [
      "Try using Kadane's algorithm.",
      "Keep track of the current sum and maximum sum seen so far.",
      "Reset current sum to 0 if it becomes negative."
    ],
    constraints: "1 <= nums.length <= 10^5\n-10^4 <= nums[i] <= 10^4",
    examples: [
      {
        input: "nums = [-2,1,-3,4,-1,2,1,-5,4]",
        output: "6",
        explanation: "The subarray [4,-1,2,1] has the largest sum 6."
      },
      {
        input: "nums = [1]",
        output: "1",
        explanation: "The subarray [1] has the largest sum 1."
      },
      {
        input: "nums = [5,4,-1,7,8]",
        output: "23",
        explanation: "The subarray [5,4,-1,7,8] has the largest sum 23."
      }
    ],
    testCases: [
      { input: { nums: [-2, 1, -3, 4, -1, 2, 1, -5, 4] }, output: 6 },
      { input: { nums: [1] }, output: 1 },
      { input: { nums: [5, 4, -1, 7, 8] }, output: 23 }
    ],
    isPremium: false,
    isApproved: true
  }
];