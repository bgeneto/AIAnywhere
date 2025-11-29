import { useState, useEffect, useCallback } from 'react';
import { listen } from '@tauri-apps/api/event';
import { ask } from '@tauri-apps/plugin-dialog';
import { relaunch } from '@tauri-apps/plugin-process';
import { AppProvider, useApp } from '../context/AppContext';
import { I18nProvider } from '../i18n/index';
import { useGlobalShortcut } from '../hooks/useGlobalShortcut';
import { MainLayout, PageId } from './MainLayout';
import { HomePage, HistoryPage, CustomTasksPage, SettingsPage, AboutPage } from './pages';
import { ReviewModal } from './ReviewModal';
import Toast, { ToastMessage } from './Toast';

function AppContent() {
  const { 
    activeModal, 
    loadConfig, 
    configLoading,
    config
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

  // Handle hotkey registration failure - offer to restart the app
  const handleHotkeyError = useCallback(async (error: string) => {
    console.error('Global shortcut error:', error);
    
    // Show initial error toast
    showToast('error', 'Hotkey Error', `Failed to register hotkey: ${error}`);
    
    // Ask user if they want to restart the app
    const shouldRestart = await ask(
      'The hotkey could not be registered. This might happen if it\'s being used by another application.\n\nWould you like to restart the app to try again?',
      {
        title: 'Hotkey Registration Failed',
        kind: 'warning',
        okLabel: 'Restart App',
        cancelLabel: 'Later',
      }
    );
    
    if (shouldRestart) {
      await relaunch();
    }
  }, [showToast]);

  // Register global shortcut when config is loaded
  useGlobalShortcut({
    hotkey: config?.hotkey || '',
    enabled: !!config?.hotkey,
    onTrigger: () => {
      // When hotkey is triggered, navigate to home page and focus the prompt
      setActivePage('home');
    },
    onError: handleHotkeyError,
    onRegistered: (hotkey) => {
      console.log('Global shortcut registered successfully:', hotkey);
    },
    onHotkeyChanged: (oldHotkey, newHotkey) => {
      console.log(`Hotkey changed from "${oldHotkey}" to "${newHotkey}"`);
      showToast('success', 'Hotkey Updated', `New hotkey "${newHotkey}" is now active.`);
    },
  });

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

  // Only show loading on initial load (when config is null)
  if (configLoading && !config) {
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
      case 'history':
        return <HistoryPage onNavigateToHome={() => setActivePage('home')} />;
      case 'custom-tasks':
        return <CustomTasksPage showToast={showToast} />;
      case 'api-settings':
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
