import { useI18n, Language } from '../i18n/index';

const languages: { code: Language; label: string; flag: string }[] = [
  { code: 'en', label: 'English', flag: '🇺🇸' },
  { code: 'pt-BR', label: 'Português', flag: '🇧🇷' },
];

export function LanguageToggle() {
  const { language, setLanguage } = useI18n();

  const toggleLanguage = () => {
    const currentIndex = languages.findIndex(l => l.code === language);
    const nextIndex = (currentIndex + 1) % languages.length;
    setLanguage(languages[nextIndex].code);
  };

  const currentLang = languages.find(l => l.code === language) || languages[0];

  return (
    <button
      onClick={toggleLanguage}
      className="p-2 rounded-lg text-slate-600 dark:text-slate-400 
                 hover:text-slate-900 dark:hover:text-white 
                 hover:bg-slate-100 dark:hover:bg-slate-800 
                 transition-colors"
      aria-label={`Current language: ${currentLang.label}. Click to change.`}
      title={`${currentLang.label} - Click to change language`}
    >
      <span className="text-lg">{currentLang.flag}</span>
    </button>
  );
}
