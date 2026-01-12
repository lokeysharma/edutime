import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authApi } from '../services/api';

interface AuthUser {
  email: string;
  role: string;
  token: string;
}

interface AuthContextType {
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  register: (data: any) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isTeacher: boolean;
  isStudent: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    if (token && savedUser) {
      setUser(JSON.parse(savedUser));
    }
  }, []);

  const login = async (email: string, password: string) => {
    const response = await authApi.login(email, password);
    const { token, role } = response.data;
    const authUser: AuthUser = { email, role, token };
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(authUser));
    setUser(authUser);
  };

  const register = async (data: any) => {
    const response = await authApi.register(data);
    const { token, role, email } = response.data;
    const authUser: AuthUser = { email, role, token };
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(authUser));
    setUser(authUser);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    login,
    register,
    logout,
    isAuthenticated: !!user,
    isAdmin: user?.role === 'Admin',
    isTeacher: user?.role === 'Teacher',
    isStudent: user?.role === 'Student',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
