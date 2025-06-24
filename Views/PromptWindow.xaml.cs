using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public PromptWindow(string selectedText = "", IntPtr originalWindowHandle = default)
        {
            InitializeComponent();
            _selectedText = selectedText;
            _originalWindowHandle = originalWindowHandle;            _config = ConfigurationService.GetConfiguration();
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
            OperationComboBox.ItemsSource = _operations;
            OperationComboBox.SelectedIndex = 0;
        }

        private void PrefillPromptForAllOperations()
        {
            // This function now works for ALL operations, not just translation and rewrite
            // NOTE: This method is called with Dispatcher.BeginInvoke to fix timing issues
            // where the PromptTextBox might not be properly initialized during constructor

            // Check if text selection is enabled in configuration
            if (!_config.EnableTextSelection)            {
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
            // Check if text selection is enabled in configuration
            if (!_config.EnableTextSelection)
            {
                return; // Skip text prefilling if disabled
            }

            // For any operation change, if the prompt is empty, try to populate it
            // This ensures all operations can benefit from selected text or clipboard content
            if (string.IsNullOrEmpty(PromptTextBox.Text))
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
                    options[key] = comboBox.SelectedItem?.ToString() ?? "";
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

                var llmService = new LLMService(_config);
                var request = new LLMRequest
                {
                    OperationType = selectedOperation.Type,
                    Prompt = PromptTextBox.Text,
                    SelectedText = _selectedText,
                    Options = GetSelectedOptions(),
                };

                var response = await llmService.ProcessRequestAsync(request);

                if (response.Success)
                { // Handle different response types
                    if (selectedOperation.Type == OperationType.ImageGeneration)
                    {
                        // Handle image responses based on user's paste behavior preference
                        if (response.IsImage && !string.IsNullOrEmpty(response.ImageUrl))
                        {
                            await HandleImageResponse(response.ImageUrl, selectedOperation.Type);
                        }
                        else
                        {
                            // Fallback for legacy image responses or direct URL
                            await HandleImageResponse(response.Content, selectedOperation.Type);
                        }
                    }
                    else
                    {
                        // Handle text responses based on user's paste behavior preference
                        await HandleTextResponse(response.Content, selectedOperation.Type);
                    }

                    Close();
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
                ProcessButton.Content = "Process";
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
        }        private void ClearButton_Click(object sender, RoutedEventArgs e)
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

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error showing review window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task HandleImageResponse(string imageUrl, OperationType operationType)
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
                    HandleImageReviewMode(imageUrl, operationType.ToString());
                    break;

                default:
                    // Default to review mode for safety
                    HandleImageReviewMode(imageUrl, operationType.ToString());
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

        private void HandleImageReviewMode(string imageUrl, string operationType)
        {
            try
            {
                Hide();

                var reviewWindow = new ReviewWindow(
                    imageUrl,
                    operationType,
                    imageUrl,
                    _originalWindowHandle
                );
                reviewWindow.ShowDialog();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error showing image review window: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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
            {                try
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
        }        private void ClearAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PromptTextBox.Text = "";
            PromptTextBox.Focus();
        }        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
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
    }
}
