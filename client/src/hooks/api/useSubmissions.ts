import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { Submission, CodeExecutionRequest, CodeExecutionResult } from "@/types/api";

export function useSubmissions(userId?: string, problemId?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ["/api/submissions", { userId, problemId, page, pageSize }],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        ...(userId && { userId }),
        ...(problemId && { problemId })
      });
      
      const response = await fetch(`/api/submissions?${params}`);
      return response.json();
    }
  });
}

export function useSubmission(id: string) {
  return useQuery<Submission>({
    queryKey: ["/api/submissions", id],
    enabled: !!id
  });
}

export function useUserSubmissions(userId: string, page = 1, pageSize = 20) {
  return useQuery<{ submissions: Submission[]; totalCount: number }>({
    queryKey: ["/api/submissions/user", userId, { page, pageSize }],
    enabled: !!userId
  });
}

export function useSubmitCode() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (request: CodeExecutionRequest): Promise<CodeExecutionResult> => {
      const response = await apiRequest("POST", "/api/submissions/execute", request);
      return response.json();
    },
    onSuccess: (_, { problemId }) => {
      queryClient.invalidateQueries({ queryKey: ["/api/submissions"] });
      queryClient.invalidateQueries({ queryKey: ["/api/submissions", "user"] });
      queryClient.invalidateQueries({ queryKey: ["/api/problems", problemId] });
    }
  });
}

export function useValidateCode() {
  return useMutation({
    mutationFn: async (request: CodeExecutionRequest): Promise<CodeExecutionResult> => {
      const response = await apiRequest("POST", "/api/submissions/validate", request);
      return response.json();
    }
  });
}

export function useSubmissionStats(userId: string) {
  return useQuery({
    queryKey: ["/api/submissions/stats", userId],
    enabled: !!userId,
    queryFn: async () => {
      const response = await fetch(`/api/submissions/stats/${userId}`);
      return response.json();
    }
  });
}