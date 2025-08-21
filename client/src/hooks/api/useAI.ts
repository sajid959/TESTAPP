import { useMutation } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { AIHintRequest, AIExplanationRequest, AICodeAnalysisRequest, AIResponse } from "@/types/api";

export function useAIHint() {
  return useMutation({
    mutationFn: async (request: AIHintRequest): Promise<AIResponse> => {
      const response = await apiRequest("POST", "/api/ai/hint", request);
      return response.json();
    }
  });
}

export function useAIExplanation() {
  return useMutation({
    mutationFn: async (request: AIExplanationRequest): Promise<AIResponse> => {
      const response = await apiRequest("POST", "/api/ai/explanation", request);
      return response.json();
    }
  });
}

export function useAICodeAnalysis() {
  return useMutation({
    mutationFn: async (request: AICodeAnalysisRequest): Promise<AIResponse> => {
      const response = await apiRequest("POST", "/api/ai/analysis", request);
      return response.json();
    }
  });
}

export function useAICodeReview() {
  return useMutation({
    mutationFn: async (request: AICodeAnalysisRequest): Promise<AIResponse> => {
      const response = await apiRequest("POST", "/api/ai/review", request);
      return response.json();
    }
  });
}

export function useAIOptimization() {
  return useMutation({
    mutationFn: async (request: AICodeAnalysisRequest): Promise<AIResponse> => {
      const response = await apiRequest("POST", "/api/ai/optimize", request);
      return response.json();
    }
  });
}