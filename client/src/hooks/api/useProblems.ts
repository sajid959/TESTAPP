import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { Problem, Category, SearchRequest, SearchResult } from "@/types/api";

export function useProblems(page = 1, pageSize = 20, difficulty?: string, categoryId?: string) {
  return useQuery({
    queryKey: ["/api/problems", { page, pageSize, difficulty, categoryId }],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        ...(difficulty && { difficulty }),
        ...(categoryId && { categoryId })
      });
      
      const response = await fetch(`/api/problems?${params}`);
      return response.json();
    }
  });
}

export function useProblem(id: string) {
  return useQuery({
    queryKey: ["/api/problems", id],
    enabled: !!id
  });
}

export function useCategories() {
  return useQuery<Category[]>({
    queryKey: ["/api/categories"]
  });
}

export function useSearchProblems() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (searchRequest: SearchRequest): Promise<SearchResult> => {
      const response = await apiRequest("POST", "/api/search/problems", searchRequest);
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/api/search"] });
    }
  });
}

export function useProblemRecommendations(userId: string) {
  return useQuery<Problem[]>({
    queryKey: ["/api/search/recommendations", userId],
    enabled: !!userId
  });
}

export function useLikeProblem() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ problemId, isLike }: { problemId: string; isLike: boolean }) => {
      const response = await apiRequest("POST", `/api/problems/${problemId}/like`, { isLike });
      return response.json();
    },
    onSuccess: (_, { problemId }) => {
      queryClient.invalidateQueries({ queryKey: ["/api/problems", problemId] });
      queryClient.invalidateQueries({ queryKey: ["/api/problems"] });
    }
  });
}