import { createContext, useEffect, useState, ReactNode } from "react";
import { apiRequest } from "@/lib/queryClient";
import { API_CONFIG } from "@/lib/config";
import { User, RegisterRequest, AuthResponse } from "@/types/api";

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => void;
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
      }
    } catch (error: any) {
      // Only remove token if it's an authentication error (401/403), not network errors
      if (error.message?.includes('401') || error.message?.includes('403')) {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
      }
      // For network errors (500, connection failed), keep the token and retry later
      console.error('Auth check failed:', error);
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
    localStorage.setItem("token", data.token);
    localStorage.setItem("refreshToken", data.refreshToken);
    setUser(data.user);
  };

  const register = async (userData: RegisterRequest) => {
    const response = await apiRequest("POST", API_CONFIG.ENDPOINTS.AUTH.REGISTER, userData);

    const data: AuthResponse = await response.json();
    localStorage.setItem("token", data.token);
    localStorage.setItem("refreshToken", data.refreshToken);
    setUser(data.user);
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
    loading,
    isAuthenticated: !!user,
    isAdmin: user?.role?.toLowerCase() === "admin",
    isPremium: user?.subscriptionPlan === 'premium' && user?.subscriptionStatus === 'active',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

