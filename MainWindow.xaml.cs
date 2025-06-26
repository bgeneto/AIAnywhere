using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using AIAnywhere.Services;
using AIAnywhere.Views;

namespace AIAnywhere
{
    public partial class MainWindow : Window
    {
        private HotkeyService? _hotkeyService;
        private NotifyIcon? _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            // Hide the main window initially
            Hide();
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;

            // Initialize system tray
            InitializeSystemTray();

            // Initialize hotkey service
            InitializeHotkey();
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "AI Anywhere - Left click to open, right click for options"
            };

            // Try to load custom icon, fallback to system icon
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AIAnywhere.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open AI Anywhere", null, (s, e) => OpenPromptWindow("", IntPtr.Zero));
            contextMenu.Items.Add("Settings", null, (s, e) => OpenConfigWindow());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("About", null, (s, e) => OpenAboutWindow());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;

            // Left click to open prompt window
            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    OpenPromptWindow("", IntPtr.Zero);
                }
            };
        }

        private void InitializeHotkey()
        {
            try
            {
                var windowHandle = new WindowInteropHelper(this).EnsureHandle();
                _hotkeyService = new HotkeyService(windowHandle);
                _hotkeyService.HotkeyPressed += OnHotkeyPressed;

                var config = ConfigurationService.GetConfiguration();
                if (!_hotkeyService.RegisterHotkey(config.Hotkey))
                {
                    System.Windows.MessageBox.Show($"Failed to register hotkey: {config.Hotkey}\n\nPlease check the configuration.",
                        "Hotkey Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Add Windows message hook for hotkey processing
                var source = HwndSource.FromHwnd(windowHandle);
                source?.AddHook(WndProc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error initializing hotkey service: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                handled = _hotkeyService?.ProcessHotkey(wParam) ?? false;
            }
            return IntPtr.Zero;
        }        private void OnHotkeyPressed()
        {
            // Capture the currently active window before we show our app
            var originalWindowHandle = TextService.GetCurrentForegroundWindow();

            string selectedText = "";

            // Check if text selection is enabled in configuration
            var config = ConfigurationService.GetConfiguration();
            if (!config.DisableTextSelection)
            {
                // Small delay to ensure the hotkey processing doesn't interfere with text selection
                System.Threading.Thread.Sleep(50);

                // Get selected text from the active application (this should happen while original app still has focus)
                selectedText = TextService.GetSelectedText();
            }

            // Open prompt window with original window handle
            OpenPromptWindow(selectedText, originalWindowHandle);
        }

        private void OpenPromptWindow(string selectedText = "", IntPtr originalWindowHandle = default)
        {
            try
            {
                var promptWindow = new PromptWindow(selectedText, originalWindowHandle);
                promptWindow.Show();
                promptWindow.Activate();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening prompt window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }private void OpenConfigWindow()
        {
            try
            {
                var configWindow = new ConfigWindow();
                configWindow.Owner = this;
                configWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (configWindow.ShowDialog() == true)
                {
                    // Refresh hotkey registration if settings changed
                    RefreshHotkey();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening settings window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAboutWindow()
        {
            try
            {
                var aboutWindow = new Views.AboutWindow();
                aboutWindow.Owner = this;
                aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening about window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshHotkey()
        {
            try
            {
                if (_hotkeyService != null)
                {
                    var config = ConfigurationService.GetConfiguration();
                    if (!_hotkeyService.RegisterHotkey(config.Hotkey))
                    {
                        System.Windows.MessageBox.Show($"Failed to register new hotkey: {config.Hotkey}",
                            "Hotkey Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error refreshing hotkey: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Prevent closing, minimize to system tray instead
            e.Cancel = true;
            Hide();
            WindowState = WindowState.Minimized;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources
            _hotkeyService?.UnregisterHotkey();
            _notifyIcon?.Dispose();
            base.OnClosed(e);
        }
    }
}
