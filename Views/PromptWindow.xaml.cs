using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AIAnywhere.Models;
using AIAnywhere.Services;

namespace AIAnywhere.Views
{    public partial class PromptWindow : Window
    {
        private readonly List<Operation> _operations;
        private readonly string _selectedText;
        private readonly Configuration _config;
        private readonly IntPtr _originalWindowHandle;

        public PromptWindow(string selectedText = "", IntPtr originalWindowHandle = default)        {
            InitializeComponent();
              _selectedText = selectedText;
            _originalWindowHandle = originalWindowHandle;
            _config = ConfigurationService.GetConfiguration();
            _operations = Operation.GetDefaultOperations(_config);
            
            InitializeOperations();
            DisplaySelectedText();
            PrefillPromptForTranslationAndRewrite();
            
            // Focus on prompt textbox
            PromptTextBox.Focus();
        }

        private void InitializeOperations()
        {
            OperationComboBox.ItemsSource = _operations;
            OperationComboBox.SelectedIndex = 0;
        }

        private void DisplaySelectedText()
        {
            if (!string.IsNullOrEmpty(_selectedText))
            {
                SelectedTextBlock.Text = _selectedText;
                SelectedTextGrid.Visibility = Visibility.Visible;
            }
        }        private void PrefillPromptForTranslationAndRewrite()
        {
            // This will be called after operations are initialized
            var selectedOperation = _operations.FirstOrDefault(); // Default to first operation
            
            // Check if this is a text translation or text rewrite operation
            if (selectedOperation != null && 
                (selectedOperation.Type == OperationType.TextTranslation || selectedOperation.Type == OperationType.TextRewrite))
            {
                string textToPopulate = "";
                
                // First, check if there's selected text
                if (!string.IsNullOrEmpty(_selectedText))
                {
                    textToPopulate = _selectedText;
                }
                else
                {
                    // If no selected text, check clipboard
                    textToPopulate = TextService.GetClipboardText();
                }
                
                // Populate the prompt text box if we have content
                if (!string.IsNullOrEmpty(textToPopulate))
                {
                    PromptTextBox.Text = textToPopulate;
                }
            }
        }

        private void OnOperationChanged(Operation operation)
        {
            // Check if this is a text translation or text rewrite operation
            if (operation.Type == OperationType.TextTranslation || operation.Type == OperationType.TextRewrite)
            {
                // Only prefill if the prompt text box is empty
                if (string.IsNullOrEmpty(PromptTextBox.Text))
                {
                    string textToPopulate = "";
                    
                    // First, check if there's selected text
                    if (!string.IsNullOrEmpty(_selectedText))
                    {
                        textToPopulate = _selectedText;
                    }
                    else
                    {
                        // If no selected text, check clipboard
                        textToPopulate = TextService.GetClipboardText();
                    }
                    
                    // Populate the prompt text box if we have content
                    if (!string.IsNullOrEmpty(textToPopulate))
                    {
                        PromptTextBox.Text = textToPopulate;
                    }
                }
            }
        }        private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    FontWeight = FontWeights.Bold
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
                            SelectedItem = option.DefaultValue
                        };
                        OptionsPanel.Children.Add(comboBox);
                        break;

                    case OptionType.Text:
                        var textBox = new TextBox
                        {
                            Name = $"Option_{option.Key}",
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 15),
                            Text = option.DefaultValue
                        };
                        OptionsPanel.Children.Add(textBox);
                        break;

                    case OptionType.Number:
                        var numberBox = new TextBox
                        {
                            Name = $"Option_{option.Key}",
                            Height = 30,
                            Margin = new Thickness(0, 0, 0, 15),
                            Text = option.DefaultValue
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
                    MessageBox.Show("Please enter a prompt.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OperationComboBox.SelectedItem is not Operation selectedOperation)
                {
                    MessageBox.Show("Please select an operation.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate API configuration
                if (string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    MessageBox.Show("API Key is not configured. Please open configuration first.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    Options = GetSelectedOptions()
                };

                var response = await llmService.ProcessRequestAsync(request);

                if (response.Success)
                {                    // Handle different response types
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
                    MessageBox.Show($"Error processing request:\n\n{response.Error}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error:\n\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable UI
                ProcessButton.IsEnabled = true;
                ProcessButton.Content = "Process";
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigWindow();
            configWindow.Owner = this;
            configWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (configWindow.ShowDialog() == true)
            {
                // Configuration was saved, you might want to refresh any settings here
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }        protected override void OnClosed(EventArgs e)
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
                Hide();
                
                // Small delay to ensure the window is hidden
                await System.Threading.Tasks.Task.Delay(200);
                
                // Copy to clipboard and paste
                Clipboard.SetText(responseContent);
                  // Restore focus and paste
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                    await System.Threading.Tasks.Task.Delay(100);
                }
                
                if (!string.IsNullOrEmpty(_selectedText))
                {
                    TextService.ReplaceSelectedText(responseContent);
                }
                else
                {
                    TextService.InsertText(responseContent);
                }
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during auto-paste: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HandleClipboardMode(string responseContent)
        {
            try
            {
                Clipboard.SetText(responseContent);
                
                MessageBox.Show("Text has been copied to clipboard!\n\nPress Ctrl+V to paste where you want it.", 
                    "Copied to Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);
                
                Close();
                  // Restore focus to original window
                if (_originalWindowHandle != IntPtr.Zero)
                {
                    TextService.RestoreFocusToWindow(_originalWindowHandle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleReviewMode(string responseContent, string operationType)
        {
            try
            {
                Hide();
                
                var reviewWindow = new ReviewWindow(responseContent, operationType, _originalWindowHandle);
                reviewWindow.ShowDialog();
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing review window: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error during auto-paste: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Image copied to clipboard successfully! You can now paste it anywhere.", 
                        "Image Copied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Fallback to copying URL as text
                    Clipboard.SetText(imageUrl);
                    MessageBox.Show("Image URL copied to clipboard as text.", 
                        "URL Copied", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Error copying image to clipboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleImageReviewMode(string imageUrl, string operationType)
        {
            try
            {
                Hide();
                
                var reviewWindow = new ReviewWindow(imageUrl, operationType, imageUrl, _originalWindowHandle);
                reviewWindow.ShowDialog();
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing image review window: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
