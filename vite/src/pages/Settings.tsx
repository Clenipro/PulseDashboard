import { useTranslation } from 'react-i18next';

const Settings: React.FC = () => {
  const { t } = useTranslation();

  return (
    <div>
      <h3 className="text-xl font-bold text-blue-900 dark:text-white mb-4">
        {t('settings.title')}
      </h3>
      <p className="text-blue-900 dark:text-white">{t('settings.content')}</p>
    </div>
  );
};

export default Settings;