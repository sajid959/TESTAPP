import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { apiRequest } from "@/lib/queryClient";
import { User, LoginRequest, RegisterRequest, AuthResponse } from "@/types/api";

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => void;
  loading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
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

      const response = await apiRequest("GET", "/api/auth/me");
      const data = await response.json();
      
      if (data.user) {
        setUser(data.user);
      } else {
        localStorage.removeItem("token");
      }
    } catch (error) {
      localStorage.removeItem("token");
    } finally {
      setLoading(false);
    }
  };

  const login = async (email: string, password: string) => {
    const response = await apiRequest("POST", "/api/auth/login", {
      email,
      password,
    });

    const data: AuthResponse = await response.json();
    localStorage.setItem("token", data.token);
    localStorage.setItem("refreshToken", data.refreshToken);
    setUser(data.user);
  };

  const register = async (userData: RegisterRequest) => {
    const response = await apiRequest("POST", "/api/auth/register", userData);

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
    isAdmin: user?.role === "admin",
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

