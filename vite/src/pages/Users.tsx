import { useTranslation } from 'react-i18next';
import { useAuth } from '../contexts/AuthContext';

const Users: React.FC = () => {
  const { t } = useTranslation();
  const { user } = useAuth();

  return (
    <div>
      <h3 className="text-xl font-bold text-blue-900 dark:text-white mb-4">
        {t('users.title')}
      </h3>
      <p className="text-blue-900 dark:text-white">{t('users.content')}</p>
      {user?.role === 'ADMIN' && (
        <button className="mt-4 bg-blue-900 text-white px-4 py-2 rounded-lg hover:bg-blue-800 transition">
          {t('users.admin_action')}
        </button>
      )}
    </div>
  );
};

export default Users;