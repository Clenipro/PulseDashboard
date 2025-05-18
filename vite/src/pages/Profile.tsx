import { useState, useContext } from "react";
import { useTranslation } from "react-i18next";
import { AuthContext } from "../contexts/AuthContext";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../services/api";
import toast from "react-hot-toast";

const Profile: React.FC = () => {
  const { t } = useTranslation();
  const { user, setUser } = useContext(AuthContext);
  const [name, setName] = useState(user?.nome || "");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const queryClient = useQueryClient();

  const updateNameMutation = useMutation({
    mutationFn: async (name: string) => {
      const response = await api.post(
        "/auth/update-name",
        { name },
        {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }
      );
      return response.data;
    },
    onSuccess: () => {
      if (user) setUser({ ...user, nome: name });
      queryClient.invalidateQueries({ queryKey: ["user"] });
      toast.success(t("profile.name_updated"));
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || t("profile.error"));
    },
  });

  const updatePasswordMutation = useMutation({
    mutationFn: async (password: string) => {
      const response = await api.post(
        "/auth/update-password",
        { password },
        {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }
      );
      return response.data;
    },
    onSuccess: () => {
      setPassword("");
      setConfirmPassword("");
      toast.success(t("profile.password_updated"));
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || t("profile.error"));
    },
  });

  const handleNameSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      toast.error(t("profile.name_empty"));
      return;
    }
    updateNameMutation.mutate(name);
  };

  const handlePasswordSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!password) {
      toast.error(t("profile.password_empty"));
      return;
    }
    if (password !== confirmPassword) {
      toast.error(t("profile.password_mismatch"));
      return;
    }
    updatePasswordMutation.mutate(password);
  };

  return (
    <div>
      <h3 className="text-xl font-bold text-blue-900 dark:text-white mb-4">
        {t("profile.title")}
      </h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <h4 className="text-lg font-semibold text-blue-900 dark:text-white mb-2">
            {t("profile.update_name")}
          </h4>
          <form onSubmit={handleNameSubmit}>
            <div className="mb-4">
              <label
                htmlFor="name"
                className="block text-blue-900 dark:text-white mb-2"
              >
                {t("profile.name")}
              </label>
              <input
                type="text"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full p-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-900 dark:bg-gray-700 dark:text-white"
                placeholder={t("profile.name_placeholder")}
                required
              />
            </div>
            <button
              type="submit"
              disabled={updateNameMutation.isPending}
              className="bg-blue-900 text-white p-3 rounded-lg hover:bg-blue-800 transition disabled:bg-blue-400"
            >
              {updateNameMutation.isPending
                ? t("profile.updating")
                : t("profile.submit_name")}
            </button>
          </form>
        </div>

        <div>
          <h4 className="text-lg font-semibold text-blue-900 dark:text-white mb-2">
            {t("profile.update_password")}
          </h4>
          <form onSubmit={handlePasswordSubmit}>
            <div className="mb-4">
              <label
                htmlFor="password"
                className="block text-blue-900 dark:text-white mb-2"
              >
                {t("profile.password")}
              </label>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full p-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-900 dark:bg-gray-700 dark:text-white"
                placeholder={t("profile.password_placeholder")}
                required
              />
            </div>
            <div className="mb-4">
              <label
                htmlFor="confirmPassword"
                className="block text-blue-900 dark:text-white mb-2"
              >
                {t("profile.confirm_password")}
              </label>
              <input
                type="password"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full p-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-900 dark:bg-gray-700 dark:text-white"
                placeholder={t("profile.confirm_password_placeholder")}
                required
              />
            </div>
            <button
              type="submit"
              disabled={updatePasswordMutation.isPending}
              className="bg-blue-900 text-white p-3 rounded-lg hover:bg-blue-800 transition disabled:bg-blue-400"
            >
              {updatePasswordMutation.isPending
                ? t("profile.updating")
                : t("profile.submit_password")}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Profile;
