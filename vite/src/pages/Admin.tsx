import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Header from '../components/Header';
import Sidebar from '../components/Sidebar';

const Admin: React.FC = () => {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

  const toggleMobileSidebar = () => {
    setIsMobileSidebarOpen(!isMobileSidebarOpen);
  };

  const handleSidebarToggle = (collapsed: boolean) => {
    setIsSidebarCollapsed(collapsed);
    console.log('Admin: Sidebar collapsed=', collapsed);
  };

  return (
    <div className="flex min-h-screen">
      <Sidebar
        isMobileOpen={isMobileSidebarOpen}
        setIsMobileOpen={setIsMobileSidebarOpen}
        onToggle={handleSidebarToggle}
      />
      <div className="flex flex-col flex-1">
        <Header
          toggleMobileSidebar={toggleMobileSidebar}
          isSidebarCollapsed={isSidebarCollapsed}
        />
        <main className="flex-1 p-6 bg-gray-100 dark:bg-gray-900 overflow-auto mt-16">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default Admin;