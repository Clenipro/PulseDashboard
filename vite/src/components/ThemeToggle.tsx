import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';

const ThemeToggle: React.FC = () => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
  const { t } = useTranslation();

  useEffect(() => {
    console.log('Applying theme:', theme);
    document.documentElement.classList.toggle('dark', theme === 'dark');
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    console.log('Toggling to theme:', newTheme);
    setTheme(newTheme);
  };

  return (
    <button
      onClick={toggleTheme}
      className="p-2 bg-blue-900 text-white rounded-lg hover:bg-blue-800 transition"
      title={theme === 'light' ? t('theme.dark') : t('theme.light')}
    >
      {theme === 'light' ? '☀️' : '🌙'}
    </button>
  );
};

export default ThemeToggle;