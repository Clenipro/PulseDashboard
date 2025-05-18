import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../services/api";
import type { User } from "../types";

export type AuthContextType = {
  user: User | null;
  setUser: (user: User | null) => void;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
  isAuthenticated: boolean;
};

export type AuthProviderProps = {
  children: ReactNode;
};

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: userData, error } = useQuery({
    queryKey: ["user"],
    queryFn: async () => {
      const token = localStorage.getItem("token");
      if (!token) return null;
      const response = await api.post("/auth/validate", { token });
      return response.data.user;
    },
    enabled: !!localStorage.getItem("token"),
    retry: 1, // Tenta uma vez antes de falhar
    retryDelay: 1000, // Aguarda 1s antes de tentar novamente
    refetchOnWindowFocus: false,
  });

  useEffect(() => {
    if (userData) {
      setUser(userData);
      setIsLoading(false);
    } else if (error || userData === null) {
      setUser(null);
      setIsLoading(false);
      // Atraso na limpeza do localStorage para permitir tentativas de refresh token
      setTimeout(() => {
        if (!localStorage.getItem("token")) {
          localStorage.removeItem("token");
          localStorage.removeItem("refreshToken");
          localStorage.removeItem("tokenCreatedAt");
        }
      }, 2000);
    }
  }, [userData, error]);

  const loginMutation = useMutation({
    mutationFn: async ({ username, password }: { username: string; password: string }) => {
      const response = await api.post("/auth/login", { username, password });
      return response.data;
    },
    onSuccess: (data) => {
      localStorage.setItem("token", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);
      localStorage.setItem("tokenCreatedAt", Date.now().toString());
      queryClient.invalidateQueries({ queryKey: ["user"] });
      navigate("/admin");
    },
    onError: () => {
      setIsLoading(false);
    },
  });

  const login = async (username: string, password: string) => {
    setIsLoading(true);
    await loginMutation.mutateAsync({ username, password });
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("tokenCreatedAt");
    setUser(null);
    queryClient.clear();
    navigate("/");
  };

  return (
    <AuthContext.Provider
      value={{ user, setUser, login, logout, isLoading, isAuthenticated: !!user }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};