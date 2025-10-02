using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AIAnywhere.Models;
using AIAnywhere.Services;

namespace AIAnywhere.Views
{
    public partial class PromptWindow : Window
    {
        private readonly List<Operation> _operations;
        private readonly string _selectedText;
        private readonly Configuration _config;
        private readonly IntPtr _originalWindowHandle;
        private string? _selectedAudioFile;

        // Add field to reference the prompt content grid
        private Grid? _promptContentGrid;

        // Store original background brush for the AudioFileTextBox
        private Brush? _originalAudioFileTextBoxBackground;

        public PromptWindow(string selectedText = "", IntPtr originalWindowHandle = default)
        {
            InitializeComponent();
            _selectedText = selectedText;
            _originalWindowHandle = originalWindowHandle;

            // Initialize the prompt content grid reference
            _promptContentGrid = this.FindName("PromptContentGrid") as Grid;

            _config = ConfigurationService.GetConfiguration();
            _operations = Operation.GetDefaultOperations(_config);
            InitializeOperations(); // Defer the prefill operation to ensure UI is fully initialized
            // This fixes the timing issue where the initial population might not work
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    PrefillPromptForAllOperations();

                    // Initialize clear button state
                    ClearButton.IsEnabled = !string.IsNullOrEmpty(PromptTextBox.Text);
                    ClearButton.Opacity = ClearButton.IsEnabled ? 1.0 : 0.5;
                }),
                System.Windows.Threading.DispatcherPriority.Loaded
            );
            // Focus on prompt textbox
            PromptTextBox.Focus();

            // Add keyboard shortcut support
            KeyDown += PromptWindow_KeyDown;
        }

        private void InitializeOperations()
        {
            OperationComboBox.ItemsSource = _operations; // Set operation list
            // Default to "Custom Task" (GeneralChat) if available, else fallback to first item
            var defaultIndex = _operations.FindIndex(op => op.Type == OperationType.GeneralChat);
            OperationComboBox.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        }

        private void PrefillPromptForAllOperations()
        {
            // This function now works for ALL operations, not just translation and rewrite
            // NOTE: This method is called with Dispatcher.BeginInvoke to fix timing issues
            // where the PromptTextBox might not be properly initialized during constructor

            // Check if text selection is disabled in configuration
            if (_config.DisableTextSelection)
            {
                return;
            }

            string textToPopulate = "";

            // Get clipboard content for comparison (only if text selection is enabled)
            var clipboardText = TextService.GetClipboardText();

            // Priority order: 1) Selected text, 2) Clipboard content, 3) Empty
            if (!string.IsNullOrEmpty(_selectedText))
            {
                textToPopulate = _selectedText;
            }
            else
            {
                // If no selected text, check clipboard
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    textToPopulate = clipboardText;
                }
            }

            // Populate the prompt text box if we have content (works for all operations)
            if (!string.IsNullOrEmpty(textToPopulate))
            {
                PromptTextBox.Text = textToPopulate;
            }
        }

        private void OnOperationChanged(Operation operation)
        {
            // Show/hide audio upload panel based on operation type
            AudioUploadPanel.Visibility =
                operation.Type == OperationType.SpeechToText
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            // Show/hide prompt content panel - hide for Speech-to-Text (STT)
            if (_promptContentGrid != null)
            {
                _promptContentGrid.Visibility =
                    operation.Type == OperationType.SpeechToText
                        ? Visibility.Collapsed
                        : Visibility.Visible;
            }

            // Check if text selection is disabled in configuration
            if (_config.DisableTextSelection)
            {
                return; // Skip text prefilling if disabled
            }

            // For any operation change, if the prompt is empty, try to populate it
            // This ensures all operations can benefit from selected text or clipboard content
            // Skip for Speech-to-Text (STT) since prompt is hidden
            if (
                operation.Type != OperationType.SpeechToText
                && string.IsNullOrEmpty(PromptTextBox.Text)
            )
            {
                string textToPopulate = "";

                // Priority order: 1) Selected text, 2) Clipboard content, 3) Empty
                if (!string.IsNullOrEmpty(_selectedText))
                {
                    textToPopulate = _selectedText;
                }
                else
                {
                    // If no selected text, check clipboard
                    var clipboardText = TextService.GetClipboardText();
                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        textToPopulate = clipboardText;
                    }
                }

                // Populate the prompt text box if we have content
                if (!string.IsNullOrEmpty(textToPopulate))
                {
                    PromptTextBox.Text = textToPopulate;
                }
            }
        }

        private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationComboBox.SelectedItem is Operation operation)
            {
                BuildOptionsPanel(operation);
                OnOperationChanged(operation);
            }
        }

        private void BuildOptionsPanel(Operation operation)
        {
            OptionsPanel.Children.Clear();

            foreach (var option in operation.Options)
            {
                var label = new TextBlock
                {
                    Text = option.Name + (option.Required ? " *" : "") + ":",
                    Margin = new Thickness(0, 0, 0, 5),
                    FontWeight = FontWeights.Bold,
                };
                OptionsPanel.Children.Add(label);

                switch (option.Type)
                {
                    case OptionType.Select:
                        var comboBox = new ComboBox
                        {
                            Name = $"Option_{option.Key}",
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 15),
                            ItemsSource = option.Values,
                            SelectedItem = option.DefaultValue,
                        };
                        OptionsPanel.Children.Add(comboBox);
                        break;

                    case OptionType.Text:
                        var textBox = new TextBox
                        {
                            Name = $"Option_{option.Key}",
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 15),
                            Text = option.DefaultValue,
                        };
                        OptionsPanel.Children.Add(textBox);
                        break;

                    case OptionType.Number:
                        var numberBox = new TextBox
                        {
                            Name = $"Option_{option.Key}",
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 15),
                            Text = option.DefaultValue,
                        };
                        OptionsPanel.Children.Add(numberBox);
                        break;
                }
            }
        }

        private Dictionary<string, string> GetSelectedOptions()
        {
            var options = new Dictionary<string, string>();

            foreach (var child in OptionsPanel.Children)
            {
                if (child is ComboBox comboBox && comboBox.Name.StartsWith("Option_"))
                {
                    var key = comboBox.Name.Substring(7); // Remove "Option_" prefix
                    var value = comboBox.SelectedItem?.ToString() ?? "";

                    // For size parameter, extract just the dimensions (before parentheses)
                    // UI shows: "512x768 (2:3 Portrait)" → API gets: "512x768"
                    if (key == "size" && value.Contains(" ("))
                    {
                        value = value.Substring(0, value.IndexOf(" ("));
                    }

                    options[key] = value;
                }
                else if (child is TextBox textBox && textBox.Name.StartsWith("Option_"))
                {
                    var key = textBox.Name.Substring(7); // Remove "Option_" prefix
                    options[key] = textBox.Text;
                }
            }

            return options;
        }

        private async void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OperationComboBox.SelectedItem is not Operation selectedOperation)
                {
                    MessageBox.Show(
                        "Please select an operation.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // For Speech-to-Text (STT), check if file is selected
                if (selectedOperation.Type == OperationType.SpeechToText)
                {
                    if (
                        string.IsNullOrEmpty(_selectedAudioFile)
                        || !System.IO.File.Exists(_selectedAudioFile)
                    )
                    {
                        MessageBox.Show(
                            "Please select an audio file to transcribe.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }
                else
                {
                    // For non-audio operations, check if prompt is provided
                    if (string.IsNullOrWhiteSpace(PromptTextBox.Text))
                    {
                        MessageBox.Show(
                            "Please enter a prompt.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }

                // Validate API configuration
                if (string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    MessageBox.Show(
                        "API Key is not configured. Please open configuration first.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Disable UI
                ProcessButton.IsEnabled = false;
                ProcessButton.Content = "Processing...";
                Cursor = System.Windows.Input.Cursors.Wait;

                // Capture start time for image generation timing
                var requestStartTime = DateTime.Now;

                var llmService = new LLMService(_config);
                var request = new LLMRequest
                {
                    OperationType = selectedOperation.Type,
                    Prompt = PromptTextBox.Text,
                    SelectedText = _selectedText,
                    Options = GetSelectedOptions(),
                    AudioFilePath =
                        selectedOperation.Type == OperationType.SpeechToText
                            ? _selectedAudioFile
                            : null,
                };

                var response = await llmService.ProcessRequestAsync(request);

                // Add start time to response for timing calculation
                if (selectedOperation.Type == OperationType.ImageGeneration)
                {
                    response.GenerationStartTime = requestStartTime;
                }

                if (response.Success)
                { // Handle different response types
                    if (selectedOperation.Type == OperationType.ImageGeneration)
                    {
                        // Handle image responses based on user's paste behavior preference
                        if (response.IsImage && !string.IsNullOrEmpty(response.ImageUrl))
                        {
                            await HandleImageResponse(
                                response.ImageUrl,
                                selectedOperation.Type,
                                response.GenerationStartTime
                            );
                        }
                        else
                        {
                            // Fallback for legacy image responses or direct URL
                            await HandleImageResponse(
                                response.Content,
                                selectedOperation.Type,
                                response.GenerationStartTime
                            );
                        }
                    }
                    else if (selectedOperation.Type == OperationType.TextToSpeech)
                    {
                        // Handle Text to Speech audio response
                        if (response.IsAudio && response.AudioData != null)
                        {
                            await HandleAudioResponse(
                                response.AudioData,
                                response.AudioFormat ?? "mp3"
                            );
                        }
                        else
                        {
                            MessageBox.Show(
                                "No audio data received from the API.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                    }
                    else
                    {
                        // Handle text responses based on user's paste behavior preference
                        await HandleTextResponse(response.Content, selectedOperation.Type);
                    }

                    // Note: Window closing is now handled by individual response handlers
                    // to support the back button functionality in review mode
                }
                else
                {
                    MessageBox.Show(
                        $"Error processing request:\n\n{response.Error}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unexpected error:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                // Re-enable UI
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "✓ Send";
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigWindow();
            configWindow.Owner = this;
            configWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (configWindow.ShowDialog() == true)
            {
                // Configuration was saved, you might want to refresh any settings here
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PromptTextBox.Text = "";
            PromptTextBox.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private async Task HandleTextResponse(string responseContent, OperationType operationType)
        {
            switch (_config.PasteBehavior)
            {
                case PasteBehavior.AutoPaste:
                    await HandleAutoPaste(responseContent);
                    break;

                case PasteBehavior.ClipboardMode:
                    HandleClipboardMode(responseContent);
                    break;

                case PasteBehavior.ReviewMode:
                    HandleReviewMode(responseContent, operationType.ToString());
                    break;

                default:
                    // Default to review mode for safety
                    HandleReviewMode(responseContent, operationType.ToString());
                    break;
            }
        }

        private async Task HandleAutoPaste(string responseContent)
        {
            try
            {
                Hide(); // Small delay to ensure the window is hidden
                await System.Threading.Tasks.Task.Delay(200);

                // Format and copy to clipboard for pasting
                var formattedContent = TextProcessor.FormatForAutoPaste(responseContent);
                Clipboard.SetText(formattedContent);

                // Restore focus and paste
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                    await System.Threading.Tasks.Task.Delay(100);
                }

                if (!string.IsNullOrEmpty(_selectedText))
                {
                    TextService.ReplaceSelectedText(formattedContent);
                }
                else
                {
                    TextService.InsertText(formattedContent);
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during auto-paste: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void HandleClipboardMode(string responseContent)
        {
            try
            {
                // Format text for clipboard use
                var formattedContent = TextProcessor.FormatForClipboard(responseContent);
                Clipboard.SetText(formattedContent);

                MessageBox.Show(
                    "Text has been copied to clipboard!\n\nPress Ctrl+V to paste where you want it.",
                    "Copied to Clipboard",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Close();
                // Restore focus to original window
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error copying to clipboard: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void HandleReviewMode(string responseContent, string operationType)
        {
            try
            {
                Hide();

                var reviewWindow = new ReviewWindow(
                    responseContent,
                    operationType,
                    _originalWindowHandle
                );
                reviewWindow.ShowDialog();

                if (reviewWindow.ShouldGoBack)
                {
                    // User clicked back, restore the prompt window
                    Show();
                    Activate();
                    Focus();
                    Topmost = true;
                    Topmost = false; // Reset to allow normal window behavior

                    // Re-enable the process button
                    ProcessButton.IsEnabled = true;
                    ProcessButton.Content = "✓ Send";
                    Cursor = System.Windows.Input.Cursors.Arrow;
                }
                else
                {
                    // User completed the action (paste or cancel), close prompt window
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error showing review window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                // Re-enable UI on error
                Show();
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "✓ Send";
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async Task HandleImageResponse(
            string imageUrl,
            OperationType operationType,
            DateTime? generationStartTime = null
        )
        {
            switch (_config.PasteBehavior)
            {
                case PasteBehavior.AutoPaste:
                    await HandleImageAutoPaste(imageUrl);
                    break;

                case PasteBehavior.ClipboardMode:
                    await HandleImageClipboardMode(imageUrl);
                    break;

                case PasteBehavior.ReviewMode:
                    HandleImageReviewMode(imageUrl, operationType.ToString(), generationStartTime);
                    break;

                default:
                    // Default to review mode for safety
                    HandleImageReviewMode(imageUrl, operationType.ToString(), generationStartTime);
                    break;
            }
        }

        private async Task HandleImageAutoPaste(string imageUrl)
        {
            try
            {
                Hide();

                // Small delay to ensure the window is hidden
                await System.Threading.Tasks.Task.Delay(200);

                // Copy image to clipboard and paste
                var success = await ImageService.CopyImageFromUrlToClipboardAsync(imageUrl);
                if (!success)
                {
                    // Fallback to copying URL as text
                    Clipboard.SetText(imageUrl);
                }

                // Restore focus and paste
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                    await System.Threading.Tasks.Task.Delay(100);
                }

                // Send paste command
                TextService.SendPasteCommand();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during auto-paste: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task HandleImageClipboardMode(string imageUrl)
        {
            try
            {
                Hide();

                // Copy image to clipboard
                var success = await ImageService.CopyImageFromUrlToClipboardAsync(imageUrl);
                if (success)
                {
                    MessageBox.Show(
                        "Image copied to clipboard successfully! You can now paste it anywhere.",
                        "Image Copied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    // Fallback to copying URL as text
                    Clipboard.SetText(imageUrl);
                    MessageBox.Show(
                        "Image URL copied to clipboard as text.",
                        "URL Copied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }

                // Restore focus to original window
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error copying image to clipboard: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void HandleImageReviewMode(
            string imageUrl,
            string operationType,
            DateTime? generationStartTime = null
        )
        {
            try
            {
                Hide();

                var reviewWindow = new ReviewWindow(
                    imageUrl,
                    operationType,
                    imageUrl,
                    _originalWindowHandle,
                    generationStartTime
                );
                reviewWindow.ShowDialog();

                if (reviewWindow.ShouldGoBack)
                {
                    // User clicked back, restore the prompt window
                    Show();
                    Activate();
                    Focus();
                    Topmost = true;
                    Topmost = false; // Reset to allow normal window behavior

                    // Re-enable the process button
                    ProcessButton.IsEnabled = true;
                    ProcessButton.Content = "✓ Send";
                    Cursor = System.Windows.Input.Cursors.Arrow;
                }
                else
                {
                    // User completed the action (paste or cancel), close prompt window
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error showing image review window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                // Re-enable UI on error
                Show();
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "✓ Send";
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async Task HandleAudioResponse(byte[] audioData, string format)
        {
            try
            {
                // Show save file dialog with theme awareness
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Audio File",
                    Filter = format.ToLower() switch
                    {
                        "mp3" => "MP3 Files|*.mp3|All Files|*.*",
                        "opus" => "Opus Files|*.opus|All Files|*.*",
                        "aac" => "AAC Files|*.aac|All Files|*.*",
                        "flac" => "FLAC Files|*.flac|All Files|*.*",
                        _ => "Audio Files|*.mp3|All Files|*.*",
                    },
                    DefaultExt = format.ToLower(),
                    FileName = $"speech_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}",
                    AddExtension = true,
                    OverwritePrompt = true,
                    ValidateNames = true,
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the audio file
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, audioData);

                    // Show success message with option to open file location
                    var result = MessageBox.Show(
                        $"Audio file saved successfully!\n\nLocation: {saveFileDialog.FileName}\n\nWould you like to open the file location?",
                        "Success",
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
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving audio file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                // Enable/disable Cut and Copy based on text selection
                bool hasSelection = !string.IsNullOrEmpty(PromptTextBox.SelectedText);

                var cutMenuItem = contextMenu
                    .Items.OfType<MenuItem>()
                    .FirstOrDefault(m => m.Name == "CutMenuItem");
                var copyMenuItem = contextMenu
                    .Items.OfType<MenuItem>()
                    .FirstOrDefault(m => m.Name == "CopyMenuItem");
                var undoMenuItem = contextMenu
                    .Items.OfType<MenuItem>()
                    .FirstOrDefault(m => m.Name == "UndoMenuItem");
                var redoMenuItem = contextMenu
                    .Items.OfType<MenuItem>()
                    .FirstOrDefault(m => m.Name == "RedoMenuItem");
                var pasteMenuItem = contextMenu
                    .Items.OfType<MenuItem>()
                    .FirstOrDefault(m => m.Name == "PasteMenuItem");

                if (cutMenuItem != null)
                    cutMenuItem.IsEnabled = hasSelection;
                if (copyMenuItem != null)
                    copyMenuItem.IsEnabled = hasSelection;
                if (undoMenuItem != null)
                    undoMenuItem.IsEnabled = PromptTextBox.CanUndo;
                if (redoMenuItem != null)
                    redoMenuItem.IsEnabled = PromptTextBox.CanRedo;
                if (pasteMenuItem != null)
                    pasteMenuItem.IsEnabled = Clipboard.ContainsText();
            }
        }

        // Context menu handlers for enhanced prompt text editing
        private void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PromptTextBox.SelectedText))
            {
                try
                {
                    Clipboard.SetText(PromptTextBox.SelectedText);
                    int selectionStart = PromptTextBox.SelectionStart;
                    int selectionLength = PromptTextBox.SelectionLength;
                    PromptTextBox.Text = PromptTextBox.Text.Remove(selectionStart, selectionLength);
                    PromptTextBox.SelectionStart = selectionStart;
                }
                catch
                {
                    // Silently handle clipboard errors
                }
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PromptTextBox.SelectedText))
            {
                try
                {
                    Clipboard.SetText(PromptTextBox.SelectedText);
                }
                catch
                {
                    // Silently handle clipboard errors
                }
            }
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    int selectionStart = PromptTextBox.SelectionStart;

                    // If text is selected, replace it; otherwise, insert at cursor
                    if (PromptTextBox.SelectionLength > 0)
                    {
                        PromptTextBox.Text = PromptTextBox.Text.Remove(
                            selectionStart,
                            PromptTextBox.SelectionLength
                        );
                    }

                    PromptTextBox.Text = PromptTextBox.Text.Insert(selectionStart, clipboardText);
                    PromptTextBox.SelectionStart = selectionStart + clipboardText.Length;
                }
            }
            catch
            {
                // Silently handle clipboard errors
            }
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PromptTextBox.SelectAll();
            PromptTextBox.Focus();
        }

        private void ClearAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PromptTextBox.Text = "";
            PromptTextBox.Focus();
        }

        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PromptTextBox.CanUndo)
                {
                    PromptTextBox.Undo();
                }
            }
            catch
            {
                // Silently handle undo errors
            }
        }

        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PromptTextBox.CanRedo)
                {
                    PromptTextBox.Redo();
                }
            }
            catch
            {
                // Silently handle redo errors
            }
        }

        private void PromptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update clear button visibility and style based on text content
            ClearButton.IsEnabled = !string.IsNullOrEmpty(PromptTextBox.Text);
            ClearButton.Opacity = ClearButton.IsEnabled ? 1.0 : 0.5;
        }

        private void PromptWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle keyboard shortcuts when the PromptTextBox has focus
            if (PromptTextBox.IsFocused)
            {
                switch (e.Key)
                {
                    case Key.A when Keyboard.Modifiers == ModifierKeys.Control:
                        PromptTextBox.SelectAll();
                        e.Handled = true;
                        break;
                    case Key.Z when Keyboard.Modifiers == ModifierKeys.Control:
                        if (PromptTextBox.CanUndo)
                        {
                            PromptTextBox.Undo();
                            e.Handled = true;
                        }
                        break;
                    case Key.Y when Keyboard.Modifiers == ModifierKeys.Control:
                        if (PromptTextBox.CanRedo)
                        {
                            PromptTextBox.Redo();
                            e.Handled = true;
                        }
                        break;
                    // Note: Cut (Ctrl+X), Copy (Ctrl+C), and Paste (Ctrl+V) are handled automatically by TextBox
                }
            }
        }

        private void BrowseAudioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Audio File",
                    Filter =
                        "Audio Files|*.mp3;*.mp4;*.wav;*.m4a;*.ogg;*.aac;*.flac;*.wma;*.webm|"
                        + "MP3 Files|*.mp3|"
                        + "MP4 Files|*.mp4|"
                        + "WAV Files|*.wav|"
                        + "M4A Files|*.m4a|"
                        + "OGG Files|*.ogg|"
                        + "AAC Files|*.aac|"
                        + "FLAC Files|*.flac|"
                        + "WMA Files|*.wma|"
                        + "WebM Files|*.webm|"
                        + "All Files|*.*",
                    DefaultExt = "mp3",
                    Multiselect = false,
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    SetSelectedAudioFile(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error selecting audio file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void SetSelectedAudioFile(string filePath)
        {
            try
            {
                // Check file size (25MB limit for OpenAI Whisper)
                var fileInfo = new System.IO.FileInfo(filePath);
                const long maxSizeInBytes = 25 * 1024 * 1024; // 25MB

                if (fileInfo.Length > maxSizeInBytes)
                {
                    MessageBox.Show(
                        $"The selected file is too large ({fileInfo.Length / (1024 * 1024):F1} MB). "
                            + "Maximum file size is 25 MB.",
                        "File Too Large",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Check if file extension is supported
                var supportedExtensions = new[]
                {
                    ".mp3",
                    ".mp4",
                    ".wav",
                    ".m4a",
                    ".ogg",
                    ".aac",
                    ".flac",
                    ".wma",
                    ".webm",
                };
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

                if (!supportedExtensions.Contains(fileExtension))
                {
                    MessageBox.Show(
                        $"Unsupported file format: {fileExtension}\n\n"
                            + "Supported formats: MP3, MP4, WAV, M4A, OGG, AAC, FLAC, WMA, WebM",
                        "Unsupported Format",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                _selectedAudioFile = filePath;
                AudioFileTextBox.Text = System.IO.Path.GetFileName(filePath);
                AudioFileTextBox.ToolTip = filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error processing audio file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void AudioFileTextBox_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                // Save the original background color if needed
                if (_originalAudioFileTextBoxBackground == null)
                    _originalAudioFileTextBoxBackground = AudioFileTextBox.Background;

                // Check if the data is a file
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files.Length == 1 && File.Exists(files[0])) // We only accept one file at a time
                    {
                        // Get file extension
                        string fileExt = Path.GetExtension(files[0]).ToLowerInvariant();

                        // Supported file extensions
                        string[] supportedFormats =
                        {
                            ".mp3",
                            ".mp4",
                            ".wav",
                            ".m4a",
                            ".ogg",
                            ".aac",
                            ".flac",
                            ".wma",
                            ".webm",
                        };

                        // Check if file is supported
                        if (supportedFormats.Contains(fileExt))
                        {
                            // Set visual feedback
                            AudioFileTextBox.Background = new SolidColorBrush(
                                Color.FromArgb(40, 0, 120, 0)
                            );
                            e.Effects = DragDropEffects.Copy;
                            e.Handled = true;
                            return;
                        }
                    }
                }

                // If we got here, it's not a valid drag operation
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error in drag enter: {ex.Message}");
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void AudioFileTextBox_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                // Check if the data is a file
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files.Length == 1 && File.Exists(files[0])) // We only accept one file at a time
                    {
                        // Get file extension
                        string fileExt = Path.GetExtension(files[0]).ToLowerInvariant();

                        // Supported file extensions
                        string[] supportedFormats =
                        {
                            ".mp3",
                            ".mp4",
                            ".wav",
                            ".m4a",
                            ".ogg",
                            ".aac",
                            ".flac",
                            ".wma",
                            ".webm",
                        };

                        // Check if file is supported
                        if (supportedFormats.Contains(fileExt))
                        {
                            e.Effects = DragDropEffects.Copy;
                            e.Handled = true;
                            return;
                        }
                    }
                }

                // If we got here, it's not a valid drag operation
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error in drag over: {ex.Message}");
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void AudioFileTextBox_DragLeave(object sender, DragEventArgs e)
        {
            try
            {
                // Restore original background
                if (_originalAudioFileTextBoxBackground != null)
                    AudioFileTextBox.Background = _originalAudioFileTextBoxBackground;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error in drag leave: {ex.Message}");
            }
        }

        private void AudioFileTextBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                // Restore original background
                if (_originalAudioFileTextBoxBackground != null)
                    AudioFileTextBox.Background = _originalAudioFileTextBoxBackground;

                // Check if the dropped data contains file(s)
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the dropped files
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files.Length == 1 && File.Exists(files[0]))
                    {
                        // Get file extension
                        string fileExt = Path.GetExtension(files[0]).ToLowerInvariant();

                        // Supported file extensions
                        string[] supportedFormats =
                        {
                            ".mp3",
                            ".mp4",
                            ".wav",
                            ".m4a",
                            ".ogg",
                            ".aac",
                            ".flac",
                            ".wma",
                            ".webm",
                        };

                        // Check if file is supported
                        if (supportedFormats.Contains(fileExt))
                        {
                            // Check file size (25MB limit)
                            FileInfo fileInfo = new FileInfo(files[0]);
                            if (fileInfo.Length <= 25 * 1024 * 1024) // 25MB in bytes
                            {
                                // Set the selected audio file and update UI
                                _selectedAudioFile = files[0];
                                AudioFileTextBox.Text = Path.GetFileName(files[0]);
                                AudioFileTextBox.ToolTip = files[0];
                                e.Handled = true;
                                return;
                            }
                            else
                            {
                                MessageBox.Show(
                                    "File is too large. Maximum size is 25MB.",
                                    "File Size Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning
                                );
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                $"Unsupported file format. Please use: MP3, MP4, WAV, M4A, OGG, AAC, FLAC, WMA, WebM",
                                "Format Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                        }
                    }
                    else if (files.Length > 1)
                    {
                        MessageBox.Show(
                            "Please drag only one file at a time.",
                            "Multiple Files",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error processing file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                e.Handled = true;
            }
        }

        private void AudioFileTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Prevent all text input to keep the TextBox "read-only" for typing
            // but still allow drag and drop operations
            e.Handled = true;
        }

        private void AudioFileTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Prevent key input except for Ctrl+A (Select All), Ctrl+C (Copy), etc.
            // Allow navigation keys but prevent modification keys
            if (
                e.Key == Key.Delete
                || e.Key == Key.Back
                || e.Key == Key.Space
                || (e.Key >= Key.A && e.Key <= Key.Z && Keyboard.Modifiers == ModifierKeys.None)
                || (e.Key >= Key.D0 && e.Key <= Key.D9 && Keyboard.Modifiers == ModifierKeys.None)
            )
            {
                e.Handled = true;
            }
        }
    }
}
