import { apiRequest } from '@/lib/queryClient';

export interface AIHintRequest {
  problemId: string;
  problemDescription: string;
  currentCode: string;
  language: string;
  difficulty: string;
  hints: string[];
}

export interface AIDebugRequest {
  code: string;
  language: string;
  error?: string;
  testCases?: Array<{
    input: string;
    expectedOutput: string;
    actualOutput: string;
    passed: boolean;
  }>;
}

export interface AIHintResponse {
  hint: string;
  level: 'basic' | 'intermediate' | 'advanced';
  codeExample?: string;
  nextSteps: string[];
}

export interface AIDebugResponse {
  issues: Array<{
    type: 'syntax' | 'logic' | 'performance' | 'edge_case';
    line?: number;
    description: string;
    suggestion: string;
    codeExample?: string;
  }>;
  suggestions: string[];
  optimizations?: string[];
}

export class AIService {
  /**
   * Get contextual hint based on user's current code and problem
   */
  static async getHint(request: AIHintRequest): Promise<AIHintResponse> {
    try {
      const response = await apiRequest('POST', '/api/ai/hint', request);
      return await response.json();
    } catch (error) {
      console.warn('AI service not available, generating contextual hint:', error);
      return this.generateContextualHint(request);
    }
  }

  /**
   * Debug user's code and provide specific suggestions
   */
  static async debugCode(request: AIDebugRequest): Promise<AIDebugResponse> {
    try {
      const response = await apiRequest('POST', '/api/ai/debug', request);
      return await response.json();
    } catch (error) {
      console.warn('AI service not available, analyzing code locally:', error);
      return this.analyzeCodeLocally(request);
    }
  }

  /**
   * Generate contextual hint based on code analysis
   */
  private static generateContextualHint(request: AIHintRequest): AIHintResponse {
    const { currentCode, problemDescription, language, difficulty } = request;
    
    // Analyze the current code to provide specific hints
    const codeAnalysis = this.analyzeCode(currentCode, language);
    
    let hint = '';
    let level: 'basic' | 'intermediate' | 'advanced' = 'basic';
    let codeExample = '';
    let nextSteps: string[] = [];

    // Check if user has started coding
    if (!currentCode.trim() || currentCode.trim().length < 10) {
      hint = this.getStarterHint(problemDescription, language, difficulty);
      level = 'basic';
      nextSteps = [
        'Start by understanding the problem requirements',
        'Think about the input and output format',
        'Consider edge cases',
        'Choose appropriate data structures'
      ];
    } else if (codeAnalysis.hasBasicStructure) {
      // User has basic structure, provide algorithm hints
      hint = this.getAlgorithmHint(problemDescription, codeAnalysis, difficulty);
      level = 'intermediate';
      nextSteps = [
        'Check your algorithm logic',
        'Consider time complexity',
        'Test with edge cases',
        'Optimize if needed'
      ];
    } else {
      // User has advanced code, provide optimization hints
      hint = this.getOptimizationHint(codeAnalysis, difficulty);
      level = 'advanced';
      nextSteps = [
        'Review time complexity',
        'Consider space optimization',
        'Check for edge cases',
        'Test with large inputs'
      ];
    }

    // Generate code example based on problem type
    if (problemDescription.toLowerCase().includes('array')) {
      codeExample = this.getArrayExample(language);
    } else if (problemDescription.toLowerCase().includes('string')) {
      codeExample = this.getStringExample(language);
    } else if (problemDescription.toLowerCase().includes('tree')) {
      codeExample = this.getTreeExample(language);
    }

    return {
      hint,
      level,
      codeExample,
      nextSteps
    };
  }

  /**
   * Analyze code locally to identify issues
   */
  private static analyzeCodeLocally(request: AIDebugRequest): AIDebugResponse {
    const { code, language, error, testCases } = request;
    const issues: AIDebugResponse['issues'] = [];
    const suggestions: string[] = [];
    const optimizations: string[] = [];

    // Basic syntax checks
    if (language === 'python') {
      if (code.includes('print(') && !code.includes('return')) {
        issues.push({
          type: 'logic',
          description: 'Using print() instead of return statement',
          suggestion: 'Make sure your function returns the result instead of printing it',
          codeExample: 'return result  # instead of print(result)'
        });
      }
      
      if (!code.includes('def ')) {
        issues.push({
          type: 'syntax',
          description: 'Missing function definition',
          suggestion: 'Define a function to solve the problem',
          codeExample: 'def solution(nums, target):\n    # your code here\n    return result'
        });
      }
    } else if (language === 'javascript') {
      if (code.includes('console.log') && !code.includes('return')) {
        issues.push({
          type: 'logic',
          description: 'Using console.log() instead of return statement',
          suggestion: 'Make sure your function returns the result instead of logging it',
          codeExample: 'return result; // instead of console.log(result)'
        });
      }
    }

    // Check for common logic errors based on test case failures
    if (testCases && testCases.length > 0) {
      const failedTests = testCases.filter(tc => !tc.passed);
      
      if (failedTests.length > 0) {
        // Analyze failed test patterns
        const hasEdgeCaseFailures = failedTests.some(tc => 
          tc.input.includes('[]') || tc.input.includes('""') || tc.input.includes('0')
        );
        
        if (hasEdgeCaseFailures) {
          issues.push({
            type: 'edge_case',
            description: 'Edge cases not handled properly',
            suggestion: 'Check for empty inputs, zero values, and boundary conditions',
            codeExample: 'if not nums or len(nums) == 0:\n    return []'
          });
        }

        // Check for off-by-one errors
        const hasIndexErrors = failedTests.some(tc => 
          tc.actualOutput.includes('IndexError') || tc.actualOutput.includes('out of bounds')
        );
        
        if (hasIndexErrors) {
          issues.push({
            type: 'logic',
            description: 'Potential index out of bounds error',
            suggestion: 'Check array bounds before accessing elements',
            codeExample: 'if i < len(nums) and j < len(nums):\n    # safe to access nums[i] and nums[j]'
          });
        }
      }
    }

    // Performance suggestions
    if (code.includes('for') && code.includes('for')) {
      optimizations.push('Consider if nested loops are necessary - might be optimizable to O(n)');
    }

    if (code.includes('.sort()') && !code.includes('# sorting needed')) {
      optimizations.push('Check if sorting is required - it adds O(n log n) complexity');
    }

    // General suggestions
    suggestions.push('Test your solution with the provided examples first');
    suggestions.push('Consider edge cases like empty inputs');
    suggestions.push('Check if your algorithm handles all required scenarios');

    if (error) {
      suggestions.push(`Debug the specific error: ${error}`);
    }

    return {
      issues,
      suggestions,
      optimizations: optimizations.length > 0 ? optimizations : undefined
    };
  }

  /**
   * Analyze code structure and patterns
   */
  private static analyzeCode(code: string, language: string) {
    const hasFunction = language === 'python' ? code.includes('def ') : 
                      language === 'javascript' ? code.includes('function ') || code.includes('=>') :
                      language === 'java' ? code.includes('public ') : true;
    
    const hasLoops = code.includes('for ') || code.includes('while ');
    const hasConditionals = code.includes('if ');
    const hasReturnStatement = code.includes('return ');
    const lineCount = code.split('\n').length;
    
    return {
      hasBasicStructure: hasFunction && (hasLoops || hasConditionals) && hasReturnStatement,
      hasFunction,
      hasLoops,
      hasConditionals,
      hasReturnStatement,
      complexity: lineCount > 20 ? 'high' : lineCount > 10 ? 'medium' : 'low'
    };
  }

  /**
   * Get starter hint for beginners
   */
  private static getStarterHint(problemDescription: string, language: string, difficulty: string): string {
    if (problemDescription.toLowerCase().includes('two sum')) {
      return 'Think about using a hash map/dictionary to store numbers you\'ve seen and their indices. For each number, check if its complement (target - current number) exists in the hash map.';
    }
    
    if (problemDescription.toLowerCase().includes('array')) {
      return `Start by iterating through the array. Consider what data structure might help you solve this efficiently. ${difficulty === 'Easy' ? 'A simple loop might be sufficient.' : 'You might need hash maps or two pointers.'}`;
    }
    
    if (problemDescription.toLowerCase().includes('string')) {
      return 'Think about string manipulation techniques. Consider if you need to check characters, substrings, or patterns. Two pointers or hash maps are often useful for string problems.';
    }
    
    return `Break down the problem step by step:\n1. Understand the input and output\n2. Think of a simple approach first\n3. Consider edge cases\n4. Implement and test`;
  }

  /**
   * Get algorithm-specific hint
   */
  private static getAlgorithmHint(problemDescription: string, codeAnalysis: any, difficulty: string): string {
    if (codeAnalysis.hasLoops && !codeAnalysis.hasConditionals) {
      return 'You have loops but missing conditional checks. Consider what conditions you need to check inside your loops.';
    }
    
    if (codeAnalysis.hasConditionals && !codeAnalysis.hasReturnStatement) {
      return 'You have conditional logic but no return statement. Make sure your function returns the expected result.';
    }
    
    if (difficulty === 'Hard') {
      return 'For hard problems, consider advanced techniques like dynamic programming, graph algorithms, or complex data structures. Break the problem into smaller subproblems.';
    }
    
    return 'Your code structure looks good. Focus on the core algorithm logic and make sure you\'re handling all the required cases correctly.';
  }

  /**
   * Get optimization hint
   */
  private static getOptimizationHint(codeAnalysis: any, difficulty: string): string {
    if (codeAnalysis.complexity === 'high') {
      return 'Your solution looks comprehensive. Consider optimizing for time complexity. Can you reduce nested loops or redundant operations?';
    }
    
    return 'Your solution is well-structured. Focus on edge cases and ensure optimal time/space complexity. Test with larger inputs to verify performance.';
  }

  /**
   * Generate language-specific code examples
   */
  private static getArrayExample(language: string): string {
    switch (language) {
      case 'python':
        return `# Array iteration example
for i, num in enumerate(nums):
    # process each element
    pass`;
      case 'javascript':
        return `// Array iteration example
for (let i = 0; i < nums.length; i++) {
    // process each element
}`;
      case 'java':
        return `// Array iteration example
for (int i = 0; i < nums.length; i++) {
    // process each element
}`;
      default:
        return 'Consider iterating through the array efficiently';
    }
  }

  private static getStringExample(language: string): string {
    switch (language) {
      case 'python':
        return `# String processing example
for char in s:
    # process each character
    pass`;
      case 'javascript':
        return `// String processing example
for (let i = 0; i < s.length; i++) {
    // process each character
}`;
      default:
        return 'Consider processing string character by character';
    }
  }

  private static getTreeExample(language: string): string {
    switch (language) {
      case 'python':
        return `# Tree traversal example
def traverse(node):
    if not node:
        return
    # process current node
    traverse(node.left)
    traverse(node.right)`;
      case 'javascript':
        return `// Tree traversal example
function traverse(node) {
    if (!node) return;
    // process current node
    traverse(node.left);
    traverse(node.right);
}`;
      default:
        return 'Consider recursive tree traversal';
    }
  }
}