using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AIAnywhere.Models;
using AIAnywhere.Services;

namespace AIAnywhere.Views
{
    public partial class ReviewWindow : Window
    {
        private readonly string _resultText;
        private readonly string _operationType;
        private readonly IntPtr _originalWindowHandle;
        private readonly bool _isImage;
        private readonly string? _imageUrl;
        private BitmapImage? _downloadedImage;

        // Windows API for focus management
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public bool ShouldPaste { get; private set; } = false;

        public ReviewWindow(
            string resultText,
            string operationType,
            IntPtr originalWindowHandle = default
        )
        {
            InitializeComponent();

            _resultText = resultText;
            _operationType = operationType;
            _originalWindowHandle = originalWindowHandle;
            _isImage = false;
            _imageUrl = null;

            InitializeWindow();

            // Handle keyboard shortcuts
            KeyDown += ReviewWindow_KeyDown;
        }

        public ReviewWindow(
            string resultText,
            string operationType,
            string imageUrl,
            IntPtr originalWindowHandle = default
        )
        {
            InitializeComponent();

            _resultText = resultText;
            _operationType = operationType;
            _originalWindowHandle = originalWindowHandle;
            _isImage = true;
            _imageUrl = imageUrl;

            InitializeWindow();

            // Handle keyboard shortcuts
            KeyDown += ReviewWindow_KeyDown;
        }

        private async void InitializeWindow()
        {
            if (_isImage && !string.IsNullOrEmpty(_imageUrl))
            {
                // Handle image content - show loading first
                ShowLoadingState();

                // Update UI for image
                OperationTypeTextBlock.Text = _operationType;
                CharacterCountTextBlock.Text = "Loading image...";

                // Hide text block, show loading
                ResultTextBox.Visibility = Visibility.Collapsed;
                LoadingPanel.Visibility = Visibility.Visible;
                ResultImage.Visibility = Visibility.Collapsed;

                // Start loading animation
                var loadingStoryboard = (Storyboard)FindResource("LoadingAnimation");
                loadingStoryboard.Begin();

                // Load image asynchronously
                await LoadAndDisplayImage();
            }
            else
            {
                // Handle text content - format for proper display
                var formattedText = TextProcessor.FormatForDisplay(_resultText);
                ResultTextBox.Text = formattedText;
                OperationTypeTextBlock.Text = _operationType;
                CharacterCountTextBlock.Text = $"{ResultTextBox.Text.Length} characters";

                // Add text changed event handler to update character count
                ResultTextBox.TextChanged += ResultTextBox_TextChanged;

                // Show text block, hide image and loading
                ResultTextBox.Visibility = Visibility.Visible;
                ResultImage.Visibility = Visibility.Collapsed;
                LoadingPanel.Visibility = Visibility.Collapsed;
            }

            // Start fade-in animation
            var fadeInStoryboard = (Storyboard)FindResource("FadeInAnimation");
            fadeInStoryboard.Begin(this);

            // Focus on the paste button
            PasteButton.Focus();
        }

        private void ShowLoadingState()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            ResultImage.Visibility = Visibility.Collapsed;
            ResultTextBox.Visibility = Visibility.Collapsed;
        }

        private void ShowImageState()
        {
            // Stop loading animation
            var loadingStoryboard = (Storyboard)FindResource("LoadingAnimation");
            loadingStoryboard.Stop();
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultImage.Visibility = Visibility.Visible;
            ResultTextBox.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorState(string errorMessage)
        {
            // Stop loading animation
            var loadingStoryboard = (Storyboard)FindResource("LoadingAnimation");
            loadingStoryboard.Stop();
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultImage.Visibility = Visibility.Collapsed;
            ResultTextBox.Visibility = Visibility.Visible;
            ResultTextBox.Text = errorMessage;
        }

        private async Task LoadAndDisplayImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_imageUrl))
                {
                    _downloadedImage = await ImageService.DownloadImageAsync(_imageUrl);
                    if (_downloadedImage != null)
                    {
                        ResultImage.Source = _downloadedImage;
                        ShowImageState();
                        CharacterCountTextBlock.Text = "Image loaded successfully";
                    }
                    else
                    {
                        ShowErrorState($"Failed to load image from: {_imageUrl}");
                        CharacterCountTextBlock.Text = "Image load failed";
                    }
                }
                else
                {
                    ShowErrorState("No image URL provided");
                    CharacterCountTextBlock.Text = "No image URL";
                }            }
            catch (Exception ex)
            {
                ShowErrorState($"Error loading image: {ex.Message}\n\nImage URL: {_imageUrl}");
                CharacterCountTextBlock.Text = "Image load error";
            }
        }

        private void ReviewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    PasteAndClose();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    CancelAndClose();
                    e.Handled = true;
                    break;
                case Key.C when Keyboard.Modifiers == ModifierKeys.Control:
                    // If text is selected in the TextBox, copy only selected text
                    if (
                        ResultTextBox.IsFocused && !string.IsNullOrEmpty(ResultTextBox.SelectedText)
                    )
                    {
                        // Let the TextBox handle Ctrl+C for selected text
                        break;
                    }
                    else
                    {
                        // Copy full content
                        CopyToClipboard();
                        e.Handled = true;
                    }
                    break;
                case Key.A when Keyboard.Modifiers == ModifierKeys.Control:
                    // Select all text in the TextBox if it's focused
                    if (ResultTextBox.IsFocused)
                    {
                        ResultTextBox.SelectAll();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            PasteAndClose();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelAndClose();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        // Context menu handlers for enhanced text selection
        private void CopySelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ResultTextBox.SelectedText))
            {                try
                {
                    Clipboard.SetText(ResultTextBox.SelectedText);
                }
                catch
                {
                    // Silently handle clipboard errors
                }
            }
        }

        private void CopyAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.SelectAll();
            ResultTextBox.Focus();
        }

        private void ResultTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Update character count when text changes
            if (CharacterCountTextBlock != null)
            {
                CharacterCountTextBlock.Text = $"{ResultTextBox.Text.Length} characters";
            }
        }

        private async void PasteAndClose()
        {
            ShouldPaste = true;

            // Copy result to clipboard first
            try
            {
                if (_isImage && _downloadedImage != null)
                {
                    ImageService.CopyImageToClipboard(_downloadedImage);
                }
                else if (_isImage && !string.IsNullOrEmpty(_imageUrl))
                {
                    var success = await ImageService.CopyImageFromUrlToClipboardAsync(_imageUrl);
                    if (!success)
                    {
                        // Fallback to copying URL as text
                        Clipboard.SetText(_imageUrl);
                    }
                }
                else
                {
                    // Format text for clipboard and paste - use current TextBox content
                    var formattedText = TextProcessor.FormatForClipboard(ResultTextBox.Text);
                    Clipboard.SetText(formattedText);
                }            }
            catch
            {
                // Silently handle clipboard errors
            }

            Close();

            // Brief delay to allow focus to shift after ReviewWindow closes, then send paste.
            // The OS will paste to the currently active/focused window.
            await System.Threading.Tasks.Task.Delay(100);
            TextService.SendPasteCommand();
        }

        private void CancelAndClose()
        {
            ShouldPaste = false;
            Close();
        }

        private async void CopyToClipboard()
        {
            try
            {
                if (_isImage && _downloadedImage != null)
                {
                    // Copy image to clipboard
                    ImageService.CopyImageToClipboard(_downloadedImage);
                }
                else if (_isImage && !string.IsNullOrEmpty(_imageUrl))
                {
                    // Download and copy image to clipboard
                    var success = await ImageService.CopyImageFromUrlToClipboardAsync(_imageUrl);
                    if (!success)
                    {
                        // Fallback to copying URL as text
                        Clipboard.SetText(_imageUrl);
                    }
                }
                else
                {
                    // Copy text to clipboard - format for clipboard use - use current TextBox content
                    var formattedText = TextProcessor.FormatForClipboard(ResultTextBox.Text);
                    Clipboard.SetText(formattedText);
                }

                // Briefly change button text to show feedback
                var originalContent = CopyButton.Content;
                CopyButton.Content = "✓ Copied!";
                CopyButton.IsEnabled = false;

                // Reset after 1 second
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                timer.Tick += (s, e) =>
                {
                    CopyButton.Content = originalContent;
                    CopyButton.IsEnabled = true;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to copy to clipboard: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Optionally, you can set the initial focus to a specific control
            // like this.PasteButton.Focus();
        }
    }
}
