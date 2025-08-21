import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { PaymentIntent, CreatePaymentRequest, SubscriptionPlan, Subscription } from "@/types/api";

export function useSubscriptionPlans() {
  return useQuery<SubscriptionPlan[]>({
    queryKey: ["/api/payments/plans"]
  });
}

export function useUserSubscription(userId: string) {
  return useQuery<Subscription>({
    queryKey: ["/api/payments/subscription", userId],
    enabled: !!userId
  });
}

export function useCreatePaymentIntent() {
  return useMutation({
    mutationFn: async (request: CreatePaymentRequest): Promise<PaymentIntent> => {
      const response = await apiRequest("POST", "/api/payments/intent", request);
      return response.json();
    }
  });
}

export function useCreateSubscription() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (request: { planId: string; paymentMethodId: string }): Promise<Subscription> => {
      const response = await apiRequest("POST", "/api/payments/subscription", request);
      return response.json();
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["/api/payments/subscription"] });
      queryClient.invalidateQueries({ queryKey: ["/api/auth/me"] });
    }
  });
}

export function useCancelSubscription() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (subscriptionId: string) => {
      const response = await apiRequest("DELETE", `/api/payments/subscription/${subscriptionId}`);
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/api/payments/subscription"] });
      queryClient.invalidateQueries({ queryKey: ["/api/auth/me"] });
    }
  });
}

export function useUserPayments(userId: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ["/api/payments/history", userId, { page, pageSize }],
    enabled: !!userId,
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString()
      });
      
      const response = await fetch(`/api/payments/history?${params}`);
      return response.json();
    }
  });
}