import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { setUser } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");
    try {
      const response = await api.post("/auth/login", { username, password });
      const { token, user } = response.data;
      localStorage.setItem("token", token);
      localStorage.setItem("tokenCreatedAt", Date.now().toString());
      setUser(user);
      navigate("/admin");
    } catch (err) {
      console.error("Erro ao fazer login", err);
      setError(
        t("login.error") || "Falha ao fazer login. Verifique suas credenciais."
      );
    } finally {
      setIsLoading(false);
    }
  };

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-gray-900">
      <div className="bg-white dark:bg-gray-800 p-8 rounded-lg shadow-lg w-full max-w-md">
        <h2 className="text-2xl font-bold text-blue-900 dark:text-white mb-6 text-center">
          {t("login.title")}
        </h2>
        {error && <p className="text-red-500 mb-4 text-center">{error}</p>}
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label
              className="block text-blue-900 dark:text-white mb-2"
              htmlFor="username"
            >
              {t("login.username")}
            </label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full p-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-900 dark:bg-gray-700 dark:text-white"
              placeholder={t("login.username_placeholder")}
              required
              disabled={isLoading}
            />
          </div>
          <div className="mb-6">
            <label
              className="block text-blue-900 dark:text-white mb-2"
              htmlFor="password"
            >
              {t("login.password")}
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full p-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-900 dark:bg-gray-700 dark:text-white"
              placeholder={t("login.password_placeholder")}
              required
              disabled={isLoading}
            />
          </div>
          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-blue-900 text-white p-3 rounded-lg hover:bg-blue-800 transition disabled:bg-blue-400 flex items-center justify-center space-x-2"
          >
            {isLoading ? (
              <>
                <div className="relative inline-flex">
                  <div className="w-5 h-5 border-2 border-white dark:border-gray-900 border-t-transparent rounded-full animate-spin"></div>
                  <div className="absolute inset-0 w-5 h-5 bg-white dark:bg-gray-900 opacity-30 rounded-full animate-pulse"></div>
                </div>
                <span>{t("loading")}</span>
              </>
            ) : (
              <span>{t("login.submit")}</span>
            )}
          </button>
        </form>
        <div className="mt-4 flex justify-end">
          <select
            onChange={(e) => changeLanguage(e.target.value)}
            className="p-2 border rounded-lg dark:bg-gray-700 dark:text-white"
            disabled={isLoading}
          >
            <option value="pt">{t("language.pt")}</option>
            <option value="en">{t("language.en")}</option>
          </select>
        </div>
      </div>
    </div>
  );
};

export default Login;
