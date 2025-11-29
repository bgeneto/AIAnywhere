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
                 transition-colors flex items-center gap-1"
      aria-label={`Current language: ${currentLang.label}. Click to change.`}
      title={`${currentLang.label} - Click to change language`}
    >
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5h12M9 3v2m1.048 9.5A18.022 18.022 0 016.412 9m6.088 9h7M11 21l5-10 5 10M12.751 5C11.783 10.77 8.07 15.61 3 18.129"></path>
      </svg>
      <span className="text-sm font-medium">{currentLang.label}</span>
    </button>
  );
}
