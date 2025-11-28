using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace AIAnywhere.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            try
            {
                // Get version from assembly
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;

                if (version != null)
                {
                    VersionTextBlock.Text =
                        $"Version {version.Major}.{version.Minor}.{version.Build}";
                }

                // Update copyright year
                var currentYear = DateTime.Now.Year;
                CopyrightTextBlock.Text = $"© {currentYear} LABiA-FUP/UnB. All rights reserved.";            }
            catch
            {
                // If version info can't be loaded, keep default values
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open GitHub repository in default browser
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/bgeneto/AIAnywhere",
                    UseShellExecute = true,
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open GitHub link: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void IssuesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open GitHub issues page in default browser
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/bgeneto/AIAnywhere/issues",
                    UseShellExecute = true,
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open issues link: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}
