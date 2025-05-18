import { useTranslation } from "react-i18next";
import { Menu } from "@headlessui/react";
import { FaUserCircle, FaBars } from "react-icons/fa";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import ThemeToggle from "./ThemeToggle";

interface HeaderProps {
  toggleMobileSidebar: () => void;
  isSidebarCollapsed: boolean;
}

const Header: React.FC<HeaderProps> = ({
  toggleMobileSidebar,
  isSidebarCollapsed,
}) => {
  const { t, i18n } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  console.log('Header: isSidebarCollapsed=', isSidebarCollapsed, 'user=', user);

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng);
  };

  return (
    <header
      className={`fixed top-0 left-0 z-40 bg-white dark:bg-gray-800 text-blue-900 dark:text-white flex justify-between items-center shadow-md w-full ${
        isSidebarCollapsed ? 'md:left-16 md:w-[calc(100%-4rem)]' : 'md:left-64 md:w-[calc(100%-16rem)]'
      }`}
    >
      <div className="flex items-center gap-4 p-4">
        <button
          onClick={toggleMobileSidebar}
          className="md:hidden text-blue-900 dark:text-white hover:bg-blue-100 dark:hover:bg-gray-700 p-2 rounded"
        >
          <FaBars className="text-xl" />
        </button>
        <h1 className="text-xl font-bold">{t("header.title")}</h1>
      </div>
      <div className="flex items-center gap-4 p-4">
        <ThemeToggle />
        <select
          onChange={(e) => changeLanguage(e.target.value)}
          value={i18n.language}
          className="bg-blue-100 dark:bg-gray-700 text-blue-900 dark:text-white p-2 rounded-lg"
        >
          <option value="pt">{t("language.pt")}</option>
          <option value="en">{t("language.en")}</option>
        </select>
        {user && (
          <Menu as="div" className="relative">
            <Menu.Button className="flex items-center">
              <FaUserCircle className="text-2xl text-blue-900 dark:text-white" />
              <span className="ml-2 hidden md:inline">
                {user.nome || user.username}
              </span>
            </Menu.Button>
            <Menu.Items className="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-800 rounded-lg shadow-lg py-1 z-10">
              <Menu.Item>
                {({ active }) => (
                  <button
                    onClick={() => navigate("/admin/profile")}
                    className={`${
                      active
                        ? "bg-blue-100 text-blue-900 dark:bg-gray-700 dark:text-white"
                        : "text-blue-900 dark:text-white"
                    } w-full text-left px-4 py-2 text-sm`}
                  >
                    {t("header.profile")}
                  </button>
                )}
              </Menu.Item>
              <Menu.Item>
                {({ active }) => (
                  <button
                    onClick={logout}
                    className={`${
                      active
                        ? "bg-blue-100 text-blue-900 dark:bg-gray-700 dark:text-white"
                        : "text-blue-900 dark:text-white"
                    } w-full text-left px-4 py-2 text-sm`}
                  >
                    {t("header.logout")}
                  </button>
                )}
              </Menu.Item>
            </Menu.Items>
          </Menu>
        )}
      </div>
    </header>
  );
};

export default Header;