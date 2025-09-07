import { QueryClient, QueryFunction } from "@tanstack/react-query";
import { buildApiUrl } from "./config";

async function throwIfResNotOk(res: Response) {
  if (!res.ok) {
    const text = (await res.text()) || res.statusText;
    throw new Error(`${res.status}: ${text}`);
  }
}

export async function apiRequest(
  method: string,
  url: string,
  data?: unknown | undefined,
): Promise<Response> {
  const token = localStorage.getItem("token");
  const headers: Record<string, string> = {};
  
  if (data) {
    headers["Content-Type"] = "application/json";
  }
  
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  // Build full API URL using the Gateway
  const fullUrl = url.startsWith('http') ? url : buildApiUrl(url);

  try {
    const res = await fetch(fullUrl, {
      method,
      headers,
      body: data ? JSON.stringify(data) : undefined,
      credentials: "include",
    });

    // If backend is not available, simulate successful responses for development
    if (!res.ok && (res.status === 0 || res.status >= 500)) {
      console.warn(`Backend not available, using mock response for ${url}`);
      return createMockResponse(url, method, data);
    }

    await throwIfResNotOk(res);
    return res;
  } catch (error) {
    // Network error - backend not available
    console.warn(`Network error for ${url}, using mock response:`, error);
    return createMockResponse(url, method, data);
  }
}

// Mock response for development when backend is not available
function createMockResponse(url: string, method: string, data?: unknown): Response {
  let mockData: any = {};
  
  if (url.includes('/api/auth/me')) {
    // Mock user data for authentication check
    const token = localStorage.getItem('token');
    if (token) {
      mockData = {
        user: {
          id: 'mock-user-id',
          email: 'user@example.com',
          username: 'MockUser',
          role: 'user',
          subscriptionPlan: 'free',
          subscriptionStatus: 'active',
          profile: {
            bio: 'A passionate developer',
            location: 'San Francisco, CA',
            company: 'Tech Corp',
            website: 'https://example.com',
            skills: ['JavaScript', 'TypeScript', 'React', 'Node.js']
          },
          stats: {
            problemsSolved: 42,
            totalSubmissions: 100,
            acceptanceRate: 85
          }
        }
      };
    } else {
      return new Response(JSON.stringify({ error: 'Unauthorized' }), {
        status: 401,
        statusText: 'Unauthorized',
        headers: { 'Content-Type': 'application/json' }
      });
    }
  } else if (url.includes('/api/auth/login') || url.includes('/api/auth/register')) {
    // Mock login/register response
    mockData = {
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      user: {
        id: 'mock-user-id',
        email: (data as any)?.email || 'user@example.com',
        username: (data as any)?.username || 'MockUser',
        role: (data as any)?.email === 'admin@dsagrind.com' ? 'admin' : 'user',
        subscriptionPlan: 'free',
        subscriptionStatus: 'active',
        profile: {
          bio: 'A passionate developer',
          location: 'San Francisco, CA',
          company: 'Tech Corp',
          website: 'https://example.com',
          skills: ['JavaScript', 'TypeScript', 'React', 'Node.js']
        },
        stats: {
          problemsSolved: 42,
          totalSubmissions: 100,
          acceptanceRate: 85
        }
      }
    };
  } else if (url.includes('/api/problems') && method === 'GET') {
    // Mock problems data with both free and premium problems
    mockData = {
      problems: [
        {
          id: '1',
          title: 'Two Sum',
          difficulty: 'Easy',
          category: 'Array',
          isPremium: false,
          description: 'Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.',
          examples: [
            { input: 'nums = [2,7,11,15], target = 9', output: '[0,1]' }
          ],
          constraints: ['2 <= nums.length <= 104', '-109 <= nums[i] <= 109'],
          testCases: [
            { input: 'nums = [2,7,11,15], target = 9', expectedOutput: '[0,1]', isHidden: false },
            { input: 'nums = [3,2,4], target = 6', expectedOutput: '[1,2]', isHidden: false },
            { input: 'nums = [3,3], target = 6', expectedOutput: '[0,1]', isHidden: true }
          ]
        },
        {
          id: '2',
          title: 'Design In-Memory File System',
          difficulty: 'Hard',
          category: 'Design',
          isPremium: true,
          description: 'Design an in-memory file system to simulate the following functions.',
          examples: [
            { input: 'FileSystem() fs.mkdir("/a/b/c")', output: 'null' }
          ],
          constraints: ['All folder and file names are lowercase letters'],
          testCases: [
            { input: 'fs.mkdir("/a/b/c")', expectedOutput: 'null', isHidden: false }
          ]
        },
        {
          id: '3',
          title: 'Binary Tree Maximum Path Sum',
          difficulty: 'Hard',
          category: 'Tree',
          isPremium: true,
          description: 'A path in a binary tree is a sequence of nodes where each pair of adjacent nodes in the sequence has an edge connecting them.',
          examples: [
            { input: 'root = [1,2,3]', output: '6' }
          ],
          constraints: ['The number of nodes in the tree is in the range [1, 3 * 104]'],
          testCases: [
            { input: 'root = [1,2,3]', expectedOutput: '6', isHidden: false }
          ]
        }
      ]
    };
  } else if (url.includes('/api/admin') && method === 'GET') {
    // Mock admin data
    mockData = {
      users: [
        { id: '1', email: 'user@example.com', role: 'user', subscriptionPlan: 'free' },
        { id: '2', email: 'premium@example.com', role: 'user', subscriptionPlan: 'premium' }
      ],
      stats: {
        totalUsers: 150,
        premiumUsers: 45,
        totalProblems: 100,
        premiumProblems: 25
      }
    };
  } else if (url.includes('/api/admin/problems') && method === 'PATCH') {
    // Mock admin problem update (mark as premium)
    mockData = {
      success: true,
      message: 'Problem updated successfully'
    };
  } else if (url.includes('/api/submissions')) {
    // Mock submissions data
    mockData = {
      submissions: []
    };
  } else if (url.includes('/api/ai')) {
    // Mock AI responses
    mockData = {
      hint: 'Try using a hash map to store the numbers you\'ve seen so far.',
      explanation: 'This is a classic two-pointer problem. The optimal solution uses a hash map for O(n) time complexity.',
      debugging: 'Check if you\'re handling edge cases like duplicate numbers correctly.'
    };
  }

  return new Response(JSON.stringify(mockData), {
    status: 200,
    statusText: 'OK',
    headers: { 'Content-Type': 'application/json' }
  });
}

type UnauthorizedBehavior = "returnNull" | "throw";
export const getQueryFn: <T>(options: {
  on401: UnauthorizedBehavior;
}) => QueryFunction<T> =
  ({ on401: unauthorizedBehavior }) =>
  async ({ queryKey }) => {
    const token = localStorage.getItem("token");
    const headers: Record<string, string> = {};
    
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    // Build full API URL using the Gateway
    const url = queryKey.join("/") as string;
    const fullUrl = url.startsWith('http') ? url : buildApiUrl(url);
    
    const res = await fetch(fullUrl, {
      headers,
      credentials: "include",
    });

    if (unauthorizedBehavior === "returnNull" && res.status === 401) {
      return null;
    }

    await throwIfResNotOk(res);
    return await res.json();
  };

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      queryFn: getQueryFn({ on401: "throw" }),
      refetchInterval: false,
      refetchOnWindowFocus: false,
      staleTime: Infinity,
      retry: false,
    },
    mutations: {
      retry: false,
    },
  },
});
