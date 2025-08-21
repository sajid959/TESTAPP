interface PerplexityResponse {
  choices: Array<{
    message: {
      content: string;
    };
  }>;
  citations?: string[];
}

interface AIRequest {
  prompt: string;
  context?: string;
}

export class AIService {
  private static apiKey = process.env.PERPLEXITY_API_KEY;
  private static baseURL = 'https://api.perplexity.ai/chat/completions';

  static async generateTestCases(problem: {
    title: string;
    description: string;
    constraints?: string;
    examples?: any[];
  }): Promise<any[]> {
    const prompt = `Generate comprehensive test cases for this coding problem:

Title: ${problem.title}
Description: ${problem.description}
${problem.constraints ? `Constraints: ${problem.constraints}` : ''}
${problem.examples?.length ? `Examples: ${JSON.stringify(problem.examples)}` : ''}

Please generate 8-10 test cases including:
1. Basic examples from the problem
2. Edge cases (empty inputs, single elements, maximum constraints)
3. Corner cases (boundary conditions)
4. Normal cases with different sizes

Return as JSON array with format: [{"input": {...}, "output": ..., "explanation": "..."}]`;

    try {
      const response = await this.makeRequest(prompt);
      const content = response.choices[0]?.message?.content || '[]';
      
      // Extract JSON from the response
      const jsonMatch = content.match(/\[[\s\S]*\]/);
      if (jsonMatch) {
        return JSON.parse(jsonMatch[0]);
      }
      return [];
    } catch (error) {
      console.error('Error generating test cases:', error);
      return [];
    }
  }

  static async getHint(problem: {
    title: string;
    description: string;
    code?: string;
    error?: string;
  }): Promise<string> {
    let prompt = `Provide a helpful hint for this coding problem:

Title: ${problem.title}
Description: ${problem.description}`;

    if (problem.code && problem.error) {
      prompt += `

User's current code:
${problem.code}

Error encountered:
${problem.error}

Please provide a specific debugging hint.`;
    } else {
      prompt += `

Please provide a general algorithmic hint without giving away the complete solution.`;
    }

    try {
      const response = await this.makeRequest(prompt);
      return response.choices[0]?.message?.content || 'Sorry, I could not generate a hint at this time.';
    } catch (error) {
      console.error('Error generating hint:', error);
      return 'Sorry, I could not generate a hint at this time.';
    }
  }

  static async fillMissingProblemData(problem: {
    title?: string;
    description?: string;
    difficulty?: string;
    tags?: string[];
    constraints?: string;
  }): Promise<Partial<typeof problem>> {
    const prompt = `Complete the missing information for this coding problem:

${Object.entries(problem).map(([key, value]) => 
  value ? `${key}: ${Array.isArray(value) ? value.join(', ') : value}` : `${key}: [MISSING]`
).join('\n')}

Please fill in any missing fields and return as JSON with fields: title, description, difficulty (Easy/Medium/Hard), tags (array), constraints.
Make sure the content is accurate and appropriate for a competitive programming problem.`;

    try {
      const response = await this.makeRequest(prompt);
      const content = response.choices[0]?.message?.content || '{}';
      
      // Extract JSON from the response
      const jsonMatch = content.match(/\{[\s\S]*\}/);
      if (jsonMatch) {
        return JSON.parse(jsonMatch[0]);
      }
      return {};
    } catch (error) {
      console.error('Error filling missing data:', error);
      return {};
    }
  }

  private static async makeRequest(prompt: string): Promise<PerplexityResponse> {
    if (!this.apiKey) {
      throw new Error('Perplexity API key not configured');
    }

    const response = await fetch(this.baseURL, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.apiKey}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        model: 'llama-3.1-sonar-small-128k-online',
        messages: [
          {
            role: 'system',
            content: 'You are an expert competitive programming coach. Be precise and helpful.'
          },
          {
            role: 'user',
            content: prompt
          }
        ],
        max_tokens: 2000,
        temperature: 0.2,
        top_p: 0.9,
        stream: false,
      }),
    });

    if (!response.ok) {
      throw new Error(`Perplexity API error: ${response.statusText}`);
    }

    return response.json();
  }
}
