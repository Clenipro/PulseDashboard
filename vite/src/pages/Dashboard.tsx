import { useTranslation } from 'react-i18next';

const Dashboard: React.FC = () => {
  const { t } = useTranslation();

  return (
    <div>
      <h3 className="text-xl font-bold text-blue-900 dark:text-white mb-4">
        {t('dashboard.title')}
      </h3>
      <p className="text-blue-900 dark:text-white">{t('dashboard.content')}</p>
    </div>
  );
};

export default Dashboard;