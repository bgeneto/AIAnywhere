using System;
using System.Windows;
using AIAnywhere.Services;

namespace AIAnywhere
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Load configuration on startup
            ConfigurationService.LoadConfiguration();
            
            // Create and show main window (which will immediately hide itself and show in system tray)
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up resources
            base.OnExit(e);
        }
    }
}
