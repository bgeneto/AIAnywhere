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
        private readonly bool _isImageGeneration;
        private readonly string? _imageUrl;
        private readonly DateTime? _generationStartTime;
        private BitmapImage? _downloadedImage;

        // Windows API for focus management
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public bool ShouldPaste { get; private set; } = false;
        public bool ShouldGoBack { get; private set; } = false;

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
            _isImageGeneration = false;
            _imageUrl = null;
            _generationStartTime = null;

            InitializeWindow();

            // Handle keyboard shortcuts
            KeyDown += ReviewWindow_KeyDown;
        }

        public ReviewWindow(
            string resultText,
            string operationType,
            string imageUrl,
            IntPtr originalWindowHandle = default,
            DateTime? generationStartTime = null
        )
        {
            InitializeComponent();

            _resultText = resultText;
            _operationType = operationType;
            _originalWindowHandle = originalWindowHandle;
            _isImage = true;
            _isImageGeneration =
                operationType.Equals("ImageGeneration", StringComparison.OrdinalIgnoreCase)
                || operationType.Equals("Image Generation", StringComparison.OrdinalIgnoreCase);
            _imageUrl = imageUrl;
            _generationStartTime = generationStartTime;

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

            // Show Save Image button only for Image Generation operations
            if (_isImageGeneration)
            {
                SaveImageButton.Visibility = Visibility.Visible;
            }

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

                        // Calculate elapsed time if start time is available
                        if (_generationStartTime.HasValue)
                        {
                            var elapsed = DateTime.Now - _generationStartTime.Value;
                            var seconds = elapsed.TotalSeconds;
                            CharacterCountTextBlock.Text =
                                $"Image loaded successfully in {seconds:F1} seconds";
                        }
                        else
                        {
                            CharacterCountTextBlock.Text = "Image loaded successfully";
                        }
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
                }
            }
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
                case Key.B when Keyboard.Modifiers == ModifierKeys.None:
                    BackButton_Click(sender, e);
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

        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveImageToFile();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldGoBack = true;
            Close();
        }

        // Context menu handlers for enhanced text selection
        private void CopySelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ResultTextBox.SelectedText))
            {
                try
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

        private void ResultTextBox_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        )
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
                }
            }
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

        private async Task SaveImageToFile()
        {
            try
            {
                if (!_isImage || (_downloadedImage == null && string.IsNullOrEmpty(_imageUrl)))
                {
                    MessageBox.Show(
                        "No image available to save.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Determine file extension from URL or default to PNG
                string extension = "png";
                if (!string.IsNullOrEmpty(_imageUrl))
                {
                    var uri = new Uri(_imageUrl);
                    var path = uri.AbsolutePath.ToLowerInvariant();
                    if (path.Contains(".jpg") || path.Contains(".jpeg"))
                        extension = "jpg";
                    else if (path.Contains(".png"))
                        extension = "png";
                    else if (path.Contains(".gif"))
                        extension = "gif";
                    else if (path.Contains(".bmp"))
                        extension = "bmp";
                    else if (path.Contains(".webp"))
                        extension = "webp";
                }

                // Generate random filename
                string randomFileName =
                    $"ai_image_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}.{extension}";

                // Show save file dialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Generated Image",
                    Filter = extension.ToLower() switch
                    {
                        "jpg" or "jpeg" =>
                            "JPEG Images|*.jpg;*.jpeg|PNG Images|*.png|All Files|*.*",
                        "png" => "PNG Images|*.png|JPEG Images|*.jpg;*.jpeg|All Files|*.*",
                        "gif" => "GIF Images|*.gif|PNG Images|*.png|All Files|*.*",
                        "bmp" => "Bitmap Images|*.bmp|PNG Images|*.png|All Files|*.*",
                        "webp" => "WebP Images|*.webp|PNG Images|*.png|All Files|*.*",
                        _ => "PNG Images|*.png|JPEG Images|*.jpg;*.jpeg|All Files|*.*",
                    },
                    DefaultExt = extension,
                    FileName = randomFileName,
                    AddExtension = true,
                    OverwritePrompt = true,
                    ValidateNames = true,
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the image
                    bool saved = false;

                    if (_downloadedImage != null)
                    {
                        // Save from BitmapImage
                        saved = await SaveBitmapImageToFile(
                            _downloadedImage,
                            saveFileDialog.FileName
                        );
                    }
                    else if (!string.IsNullOrEmpty(_imageUrl))
                    {
                        // Download and save directly from URL
                        saved = await SaveImageFromUrlToFile(_imageUrl, saveFileDialog.FileName);
                    }

                    if (saved)
                    {
                        // Show success message with option to open file location
                        var result = MessageBox.Show(
                            $"Image saved successfully!\n\nLocation: {saveFileDialog.FileName}\n\nWould you like to open the file location?",
                            "Image Saved",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            // Open file location in Windows Explorer
                            System.Diagnostics.Process.Start(
                                "explorer.exe",
                                $"/select,\"{saveFileDialog.FileName}\""
                            );
                        }

                        // Briefly change button text to show feedback
                        var originalContent = SaveImageButton.Content;
                        SaveImageButton.Content = "✓ Saved!";
                        SaveImageButton.IsEnabled = false;

                        // Reset after 2 seconds
                        var timer = new System.Windows.Threading.DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(2),
                        };
                        timer.Tick += (s, e) =>
                        {
                            SaveImageButton.Content = originalContent;
                            SaveImageButton.IsEnabled = true;
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to save image. Please try again.",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving image: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private Task<bool> SaveBitmapImageToFile(BitmapImage bitmapImage, string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Create encoder based on file extension
                    BitmapEncoder encoder = System
                        .IO.Path.GetExtension(filePath)
                        .ToLowerInvariant() switch
                    {
                        ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                        ".png" => new PngBitmapEncoder(),
                        ".gif" => new GifBitmapEncoder(),
                        ".bmp" => new BmpBitmapEncoder(),
                        _ => new PngBitmapEncoder(), // Default to PNG
                    };

                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    // Ensure directory exists
                    var directory = System.IO.Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }

                    using var fileStream = new System.IO.FileStream(
                        filePath,
                        System.IO.FileMode.Create,
                        System.IO.FileAccess.Write
                    );
                    encoder.Save(fileStream);

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving bitmap image: {ex.Message}");
                    return false;
                }
            });
        }

        private async Task<bool> SaveImageFromUrlToFile(string imageUrl, string filePath)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Ensure directory exists
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image from URL: {ex.Message}");
                return false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Optionally, you can set the initial focus to a specific control
            // like this.PasteButton.Focus();
        }
    }
}
