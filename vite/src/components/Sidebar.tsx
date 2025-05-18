import { useState } from "react";
import { useTranslation } from "react-i18next";
import { NavLink } from "react-router-dom";
import {
  FaTachometerAlt,
  FaUsers,
  FaChartBar,
  FaCog,
} from "react-icons/fa";

interface SidebarProps {
  isMobileOpen: boolean;
  setIsMobileOpen: (open: boolean) => void;
  onToggle: (collapsed: boolean) => void;
}

const Sidebar: React.FC<SidebarProps> = ({
  isMobileOpen,
  setIsMobileOpen,
  onToggle,
}) => {
  const { t } = useTranslation();
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggleSidebar = () => {
    const newCollapsed = !isCollapsed;
    setIsCollapsed(newCollapsed);
    onToggle(newCollapsed);
    console.log("Sidebar width:", newCollapsed ? "64px" : "256px");
  };

  const closeMobileSidebar = () => {
    setIsMobileOpen(false);
  };

  return (
    <>
      <aside
        className={`bg-white dark:bg-gray-800 text-blue-900 dark:text-white fixed top-0 left-0 min-h-full z-50 transition-all duration-300 ${
          isCollapsed ? "w-16" : "w-64"
        } ${
          isMobileOpen ? "sidebar-mobile open" : "sidebar-mobile"
        } md:static md:flex md:flex-col`}
      >
        <div className="flex items-center justify-between p-4">
          <img
            src={isCollapsed ? "/assets/vera.png" : "/assets/vera.png"}
            alt="Logo"
            className={`h-10 ${
              isCollapsed ? "w-10" : "w-32"
            } transition-all duration-300`}
          />
          <button
            onClick={toggleSidebar}
            className="p-2 hover:bg-blue-100 dark:hover:bg-gray-700 rounded hidden md:block"
          >
            {isCollapsed ? "➡️" : "⬅️"}
          </button>
          <button
            onClick={closeMobileSidebar}
            className="p-2 hover:bg-blue-100 dark:hover:bg-gray-700 rounded md:hidden"
          >
            ✕
          </button>
        </div>
        <nav className="flex-1 min-h-0 bg-white dark:bg-gray-800">
          <ul className="space-y-2 p-4">
            <li>
              <NavLink
                to="/admin/dashboard"
                className={({ isActive }) =>
                  `flex items-center p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-gray-700 ${
                    isActive ? "bg-blue-100 dark:bg-gray-700" : ""
                  }`
                }
                onClick={closeMobileSidebar}
              >
                <FaTachometerAlt className="text-xl" />
                {!isCollapsed && (
                  <span className="ml-3">{t("sidebar.dashboard")}</span>
                )}
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/admin/users"
                className={({ isActive }) =>
                  `flex items-center p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-gray-700 ${
                    isActive ? "bg-blue-100 dark:bg-gray-700" : ""
                  }`
                }
                onClick={closeMobileSidebar}
              >
                <FaUsers className="text-xl" />
                {!isCollapsed && (
                  <span className="ml-3">{t("sidebar.users")}</span>
                )}
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/admin/reports"
                className={({ isActive }) =>
                  `flex items-center p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-gray-700 ${
                    isActive ? "bg-blue-100 dark:bg-gray-700" : ""
                  }`
                }
                onClick={closeMobileSidebar}
              >
                <FaChartBar className="text-xl" />
                {!isCollapsed && (
                  <span className="ml-3">{t("sidebar.reports")}</span>
                )}
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/admin/settings"
                className={({ isActive }) =>
                  `flex items-center p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-gray-700 ${
                    isActive ? "bg-blue-100 dark:bg-gray-700" : ""
                  }`
                }
                onClick={closeMobileSidebar}
              >
                <FaCog className="text-xl" />
                {!isCollapsed && (
                  <span className="ml-3">{t("sidebar.settings")}</span>
                )}
              </NavLink>
            </li>
          </ul>
        </nav>
      </aside>
      {isMobileOpen && (
        <div
          className="sidebar-overlay open md:hidden"
          onClick={closeMobileSidebar}
        ></div>
      )}
    </>
  );
};

export default Sidebar;
