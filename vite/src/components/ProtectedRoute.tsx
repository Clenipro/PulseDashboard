import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { useTranslation } from "react-i18next";

interface ProtectedRouteProps {
  children: JSX.Element;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { user, isLoading, isAuthenticated } = useAuth();
  const { t } = useTranslation();
  const token = localStorage.getItem("token");

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-gray-900">
        <div className="flex flex-col items-center space-y-4">
          <div className="relative">
            <div className="w-16 h-16 border-4 border-blue-900 dark:border-white border-t-transparent rounded-full animate-spin"></div>
            <div className="absolute inset-0 w-16 h-16 bg-blue-900 dark:bg-white opacity-20 rounded-full animate-pulse"></div>
          </div>
          <span className="text-blue-900 dark:text-white text-lg font-semibold">
            {t("theme.loading")}...
          </span>
        </div>
      </div>
    );
  }

  if (!token || !isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return children;
};

export default ProtectedRoute;
