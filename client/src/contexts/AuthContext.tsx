import { createContext, useEffect, useState, ReactNode } from "react";
import { apiRequest } from "@/lib/queryClient";
import { API_CONFIG } from "@/lib/config";
import { User, RegisterRequest, AuthResponse } from "@/types/api";

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => void;
  verifyEmail: (token: string) => Promise<boolean>;
  loading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isPremium: boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        setLoading(false);
        return;
      }

      const response = await apiRequest("GET", API_CONFIG.ENDPOINTS.AUTH.ME);
      const data = await response.json();

      if (data.user) {
        setUser(data.user);
      } else {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
      }
    } catch (error: any) {
      if (error.message?.includes("401") || error.message?.includes("403")) {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
      }
      console.error("Auth check failed:", error);
    } finally {
      setLoading(false);
    }
  };

  const login = async (email: string, password: string) => {
    const response = await apiRequest("POST", API_CONFIG.ENDPOINTS.AUTH.LOGIN, {
      email,
      password,
    });

    const data: AuthResponse = await response.json();
    localStorage.setItem("token", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    setUser(data.user);
  };

  const register = async (userData: RegisterRequest) => {
    await apiRequest("POST", API_CONFIG.ENDPOINTS.AUTH.REGISTER, userData);
    // ✅ No tokens returned. Show message like:
    // "Check your email to verify your account."
  };

  const verifyEmail = async (token: string): Promise<boolean> => {
    try {
      const response = await apiRequest(
        "POST",
        API_CONFIG.ENDPOINTS.AUTH.VERIFY_EMAIL,
        { token }
      );
      return response.ok;
    } catch {
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    setUser(null);
  };

  const value = {
    user,
    login,
    register,
    logout,
    verifyEmail,
    loading,
    isAuthenticated: !!user,
    isAdmin: user?.role?.toLowerCase() === "admin",
    isPremium:
      user?.subscriptionPlan === "premium" &&
      user?.subscriptionStatus === "active",
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
