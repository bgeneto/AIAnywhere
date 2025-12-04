import { ReactNode, useState } from 'react';
import { useI18n } from '../i18n/index';
import { ThemeToggle } from './ThemeToggle';
import { LanguageToggle } from './LanguageToggle';
import appIcon from '../assets/icon.png';

export type PageId = 'home' | 'history' | 'custom-tasks' | 'api-settings' | 'about';

interface NavItem {
  id: PageId;
  icon: string;
  labelKey: keyof typeof import('../i18n/translations').translations['en']['nav'];
  section?: 'main' | 'settings' | 'bottom';
}

const navItems: NavItem[] = [
  { id: 'home', icon: '🏠', labelKey: 'home', section: 'main' },
  { id: 'history', icon: '📜', labelKey: 'history', section: 'main' },
  { id: 'custom-tasks', icon: '✨', labelKey: 'customTasks', section: 'main' },
  { id: 'api-settings', icon: '⚙', labelKey: 'appSettings', section: 'settings' },
  { id: 'about', icon: 'ℹ️', labelKey: 'about', section: 'bottom' },
];

interface MainLayoutProps {
  activePage: PageId;
  onPageChange: (page: PageId) => void;
  children: ReactNode;
}

export function MainLayout({ activePage, onPageChange, children }: MainLayoutProps) {
  const { t } = useI18n();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  const mainItems = navItems.filter(item => item.section === 'main');
  const settingsItems = navItems.filter(item => item.section === 'settings');
  const bottomItems = navItems.filter(item => item.section === 'bottom');

  const renderNavItem = (item: NavItem) => {
    const isActive = activePage === item.id;
    const label = t.nav[item.labelKey];

    return (
      <button
        key={item.id}
        onClick={() => onPageChange(item.id)}
        title={sidebarCollapsed ? label : undefined}
        className={`
          w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-all duration-200
          ${sidebarCollapsed ? 'px-3 justify-center' : 'justify-start'}
          ${isActive
            ? 'bg-slate-700 dark:bg-slate-700 text-white shadow-md'
            : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-900 dark:hover:text-white'
          }
        `}
      >
        <span className="text-xl shrink-0">{item.icon}</span>
        {!sidebarCollapsed && <span className="font-medium text-sm truncate">{label}</span>}
      </button>
    );
  };

  return (
    <div className="flex h-screen bg-slate-100 dark:bg-slate-950 transition-colors duration-200">
      {/* Sidebar */}
      <aside className={`bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 flex flex-col transition-all duration-300 ${sidebarCollapsed ? 'w-20' : 'w-64'}`}>
        {/* App Header */}
        <div className="p-4 border-b border-slate-200 dark:border-slate-800">
          <div className={`flex items-center ${sidebarCollapsed ? 'justify-center' : 'justify-between'}`}>
            {sidebarCollapsed ? (
              /* Collapsed: hamburger icon replaces logo */
              <button
                onClick={() => setSidebarCollapsed(false)}
                title="Expand sidebar"
                className="w-10 h-10 flex items-center justify-center rounded-lg text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-xl"
              >
                ☰
              </button>
            ) : (
              /* Expanded: logo + text + hamburger on right */
              <>
                <div className="flex items-center gap-3">
                  <img
                    src={appIcon}
                    alt="AI Anywhere"
                    className="w-10 h-10 rounded-lg shadow-lg shrink-0"
                  />
                  <div>
                    <h1 className="font-bold text-slate-900 dark:text-white">{t.appName}</h1>
                    <p className="text-xs text-slate-500 dark:text-slate-400">v1.3.5</p>
                  </div>
                </div>
                <button
                  onClick={() => setSidebarCollapsed(true)}
                  title="Collapse sidebar"
                  className="p-2 rounded-lg text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-lg"
                >
                  ☰
                </button>
              </>
            )}
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto p-3 space-y-1">
          {/* Main Items */}
          <div className="space-y-1">
            {mainItems.map(renderNavItem)}
          </div>

          {/* Settings Section */}
          {!sidebarCollapsed && (
            <div className="pt-4">
              <p className="px-4 py-2 text-xs font-semibold text-slate-400 dark:text-slate-500 uppercase tracking-wider">
                {t.nav.settings}
              </p>
              <div className="space-y-1">
                {settingsItems.map(renderNavItem)}
              </div>
            </div>
          )}

          {/* Settings Item (shown as icon when collapsed) */}
          {sidebarCollapsed && (
            <div className="space-y-1">
              {settingsItems.map(renderNavItem)}
            </div>
          )}
        </nav>

        {/* Bottom Section */}
        <div className={`p-3 border-t border-slate-200 dark:border-slate-800 space-y-1`}>
          {bottomItems.map(renderNavItem)}

          {/* Theme and Language Toggles */}
          <div className={`flex items-center gap-2 pt-2 ${sidebarCollapsed ? 'flex-col' : 'justify-center'}`}>
            <ThemeToggle />
            <LanguageToggle />
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-hidden flex flex-col bg-slate-50 dark:bg-slate-900">
        {children}
      </main>
    </div>
  );
}
