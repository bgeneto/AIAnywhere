import { useState, useEffect, useCallback } from 'react';
import { listen } from '@tauri-apps/api/event';
import { AppProvider, useApp } from '../context/AppContext';
import { I18nProvider } from '../i18n/index';
import { MainLayout, PageId } from './MainLayout';
import { HomePage, SettingsPage, AboutPage } from './pages';
import { ReviewModal } from './ReviewModal';
import Toast, { ToastMessage } from './Toast';

function AppContent() {
  const { 
    activeModal, 
    loadConfig, 
    configLoading 
  } = useApp();
  
  const [activePage, setActivePage] = useState<PageId>('home');
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const showToast = useCallback((type: ToastMessage['type'], title: string, message?: string) => {
    const id = Date.now().toString();
    setToasts(prev => [...prev, { id, type, title, message }]);
  }, []);

  const removeToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  // Load configuration on mount
  useEffect(() => {
    loadConfig();
  }, [loadConfig]);

  // Listen for tray menu events
  useEffect(() => {
    const unlistenSettings = listen('open-settings', () => {
      setActivePage('api-settings');
    });

    const unlistenAbout = listen('open-about', () => {
      setActivePage('about');
    });

    return () => {
      unlistenSettings.then(fn => fn());
      unlistenAbout.then(fn => fn());
    };
  }, []);

  if (configLoading) {
    return (
      <div className="flex items-center justify-center h-screen bg-slate-50 dark:bg-slate-950">
        <div className="text-center">
          <div className="animate-spin text-4xl mb-4">⏳</div>
          <p className="text-slate-600 dark:text-slate-400">Loading...</p>
        </div>
      </div>
    );
  }

  // Render page content based on active page
  const renderPage = () => {
    switch (activePage) {
      case 'home':
        return <HomePage onShowToast={showToast} />;
      case 'api-settings':
      case 'language-settings':
      case 'audio-settings':
        return <SettingsPage onShowToast={showToast} />;
      case 'about':
        return <AboutPage />;
      default:
        return <HomePage onShowToast={showToast} />;
    }
  };

  return (
    <>
      {/* Toast Container */}
      <div className="fixed top-4 right-4 z-[100] space-y-2 max-w-md">
        {toasts.map(toast => (
          <Toast
            key={toast.id}
            toast={toast}
            onClose={removeToast}
          />
        ))}
      </div>

      {/* Main Layout with Sidebar */}
      <MainLayout activePage={activePage} onPageChange={setActivePage}>
        {renderPage()}
      </MainLayout>

      {/* Modals */}
      {activeModal === 'review' && <ReviewModal onShowToast={showToast} />}
    </>
  );
}

export function App() {
  return (
    <I18nProvider>
      <AppProvider>
        <AppContent />
      </AppProvider>
    </I18nProvider>
  );
}
