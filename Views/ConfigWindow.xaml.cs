using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIAnywhere.Models;
using AIAnywhere.Services;
using Newtonsoft.Json;

namespace AIAnywhere.Views
{
    public partial class ConfigWindow : Window
    {
        private Configuration _config;
        private bool _isApiKeyModified = false;

        public ConfigWindow()
        {
            InitializeComponent();
            _config = ConfigurationService.GetConfiguration();
            LoadConfiguration();

            // Track when API key is modified
            ApiKeyPasswordBox.PasswordChanged += (s, e) => _isApiKeyModified = true;
        }

        private void LoadConfiguration()
        {
            HotkeyTextBox.Text = _config.Hotkey;
            ApiBaseUrlTextBox.Text = _config.ApiBaseUrl;
            // Show masked API key if it exists, but don't show the actual key
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                ApiKeyPasswordBox.Password = PortableEncryptionService.MaskSensitiveTextFully(
                    _config.ApiKey
                ); // Show all dots
            }
            else
            {
                ApiKeyPasswordBox.Password = "";
            }

            // Set LLM Model - find matching item or add custom one
            SetLlmModelSelection(_config.LlmModel);

            // Set paste behavior
            try
            {
                var pasteBehaviorIndex = (int)_config.PasteBehavior;
                if (
                    pasteBehaviorIndex >= 0
                    && pasteBehaviorIndex < PasteBehaviorComboBox.Items.Count
                )
                {
                    PasteBehaviorComboBox.SelectedIndex = pasteBehaviorIndex;
                }
                else
                {
                    PasteBehaviorComboBox.SelectedIndex = 0; // Default to Auto-paste
                }
            }
            catch
            {
                PasteBehaviorComboBox.SelectedIndex = 0; // Default to Auto-paste
            }

            UpdatePasteBehaviorDescription();

            // Set text selection preference
            EnableTextSelectionCheckBox.IsChecked = _config.EnableTextSelection;

            // Handle selection change event
            PasteBehaviorComboBox.SelectionChanged += PasteBehaviorComboBox_SelectionChanged;

            _isApiKeyModified = false; // Reset flag after loading
        }

        private void SaveConfiguration()
        {
            // Only save UI-editable properties, preserve SystemPrompts
            _config.Hotkey = HotkeyTextBox.Text;
            _config.ApiBaseUrl = ApiBaseUrlTextBox.Text;
            // Use the same logic as validation for consistency
            _config.ApiKey = GetActualApiKey();

            // Save LLM Model - get from selected item content, not Text property
            if (
                LlmModelComboBox.SelectedItem is ComboBoxItem selectedItem
                && selectedItem.IsEnabled
                && // Don't save the placeholder item
                selectedItem.Content != null
            )
            {
                _config.LlmModel = selectedItem.Content.ToString() ?? "";
            }
            else if (!string.IsNullOrEmpty(LlmModelComboBox.Text))
            {
                // Fallback to Text property if available
                _config.LlmModel = LlmModelComboBox.Text;
            }

            // Save paste behavior based on selected index
            if (
                PasteBehaviorComboBox.SelectedIndex >= 0
                && PasteBehaviorComboBox.SelectedIndex <= 2
            )
            {
                _config.PasteBehavior = (PasteBehavior)PasteBehaviorComboBox.SelectedIndex;
            }

            // Save text selection preference
            _config.EnableTextSelection = EnableTextSelectionCheckBox.IsChecked ?? true;

            // NOTE: SystemPrompts are intentionally NOT modified here
            // This allows users to customize them directly in config.json
            // without losing their changes when saving UI settings
        }

        private void UpdatePasteBehaviorDescription()
        {
            try
            {
                switch (PasteBehaviorComboBox.SelectedIndex)
                {
                    case 0: // AutoPaste
                        PasteBehaviorDescriptionTextBlock.Text =
                            "Auto-paste provides the smoothest workflow but requires trust in AI results. Press Ctrl+Z to undo if needed.";
                        break;
                    case 1: // ClipboardMode
                        PasteBehaviorDescriptionTextBlock.Text =
                            "Copy to clipboard mode gives you full control. You manually paste (Ctrl+V) where you want the result.";
                        break;
                    case 2: // ReviewMode
                        PasteBehaviorDescriptionTextBlock.Text =
                            "Review mode shows a beautiful preview window where you can review and confirm before pasting.";
                        break;
                    default:
                        PasteBehaviorDescriptionTextBlock.Text =
                            "Choose how AI results are handled after processing.";
                        break;
                }
            }
            catch { }
        }

        private void PasteBehaviorComboBox_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e
        )
        {
            try
            {
                UpdatePasteBehaviorDescription();
            }
            catch { }
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestButton.IsEnabled = false;
                TestButton.Content = "Testing...";

                // Save configuration first, but we need to get the actual API key
                var tempConfig = new Configuration
                {
                    Hotkey = HotkeyTextBox.Text,
                    ApiBaseUrl = ApiBaseUrlTextBox.Text,
                    LlmModel = GetSelectedLlmModel(),
                    ApiKey = GetActualApiKey(),
                };

                var llmService = new LLMService(tempConfig);

                var testRequest = new LLMRequest
                {
                    OperationType = OperationType.GeneralChat,
                    Prompt =
                        "Hello, this is a test message. Please respond with 'Connection successful!'",
                };

                var response = await llmService.ProcessRequestAsync(testRequest);

                if (response.Success)
                {
                    MessageBox.Show(
                        "Connection test successful!\n\nResponse: " + response.Content,
                        "Test Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Connection test failed!\n\nError: " + response.Error,
                        "Test Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Connection test failed!\n\nError: " + ex.Message,
                    "Test Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                TestButton.IsEnabled = true;
                TestButton.Content = "Test Connection";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate API base URL is required
                if (string.IsNullOrWhiteSpace(ApiBaseUrlTextBox.Text))
                {
                    MessageBox.Show(
                        "API Base URL is required. Please enter the base URL for your API.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ApiBaseUrlTextBox.Focus();
                    return;
                }

                // Validate API key is required
                string actualApiKey = GetActualApiKey();

                if (string.IsNullOrWhiteSpace(actualApiKey))
                {
                    MessageBox.Show(
                        "API Key is required. Please enter your API key before saving.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ApiKeyPasswordBox.Focus();
                    return;
                }

                // Validate LLM model is required
                string selectedModel = GetSelectedLlmModel();
                if (string.IsNullOrWhiteSpace(selectedModel))
                {
                    MessageBox.Show(
                        "LLM Model is required. Please select a model or click 'Get Models' to retrieve available options.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    LlmModelComboBox.Focus();
                    return;
                }
                SaveConfiguration();

                // Preserve SystemPrompts from the current configuration file
                // in case user has customized them directly in config.json
                var currentConfig = ConfigurationService.GetConfiguration();
                _config.SystemPrompts = currentConfig.SystemPrompts;

                ConfigurationService.UpdateConfiguration(_config);

                MessageBox.Show(
                    "Configuration saved successfully!\n\nRestart the application for hotkey changes to take effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Error saving configuration: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void RetrieveModelsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get actual API key value
            string actualApiKey = GetActualApiKey();

            // Validate API key and base URL
            if (string.IsNullOrWhiteSpace(actualApiKey))
            {
                ModelStatusTextBlock.Text = "API key is required to retrieve models";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Red
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(ApiBaseUrlTextBox.Text))
            {
                ModelStatusTextBlock.Text = "API base URL is required to retrieve models";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Red
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                RetrieveModelsButton.IsEnabled = false;
                RetrieveModelsButton.Content = "Retrieving...";
                ModelStatusTextBlock.Text = "Fetching available models...";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Blue
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;

                var models = await FetchAvailableModelsAsync(ApiBaseUrlTextBox.Text, actualApiKey);

                if (models.Any())
                {
                    // Store current selection
                    var currentSelection = GetSelectedLlmModel();

                    // Clear and populate ComboBox
                    LlmModelComboBox.Items.Clear();
                    foreach (var model in models.OrderBy(m => m))
                    {
                        LlmModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                    }

                    // Restore selection if it exists in the new list
                    if (models.Contains(currentSelection))
                    {
                        SetLlmModelSelection(currentSelection);
                    }
                    else if (models.Any())
                    {
                        LlmModelComboBox.SelectedIndex = 0;
                    }

                    ModelStatusTextBlock.Text = $"Retrieved {models.Count} models successfully";
                    ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.Green
                    );
                }
                else
                {
                    ModelStatusTextBlock.Text = "No models found or API returned empty list";
                    ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.Orange
                    );
                }
            }
            catch (System.Exception ex)
            {
                ModelStatusTextBlock.Text = $"Failed to retrieve models: {ex.Message}";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Red
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                RetrieveModelsButton.IsEnabled = true;
                RetrieveModelsButton.Content = "Retrieve Models";
            }
        }

        private async Task<List<string>> FetchAvailableModelsAsync(string apiBaseUrl, string apiKey)
        {
            var models = new List<string>();

            using (var httpClient = new HttpClient())
            {
                // Set authorization header
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                // Ensure URL ends with /models
                var modelsUrl = apiBaseUrl.TrimEnd('/') + "/models";

                var response = await httpClient.GetAsync(modelsUrl);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonConvert.DeserializeObject<ModelsResponse>(jsonContent);

                if (modelsResponse?.Data != null)
                {
                    models.AddRange(modelsResponse.Data.Select(m => m.Id));
                }
            }

            return models;
        }

        private bool _isCapturingHotkey = false;
        private string _originalHotkeyValue = "";

        private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _isCapturingHotkey = true;
            _originalHotkeyValue = HotkeyTextBox.Text;
            HotkeyTextBox.Text = "Press your desired key combination...";
            HotkeyTextBox.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 255, 224)
            ); // Light yellow
            HotkeyTextBox.Foreground = System.Windows.Media.Brushes.Black; // Ensure text is visible
        }

        private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isCapturingHotkey)
            {
                // If we lost focus without capturing a key, restore the original value
                HotkeyTextBox.Text = _originalHotkeyValue;
                _isCapturingHotkey = false;
                HotkeyTextBox.Background = System.Windows.SystemColors.WindowBrush; // Use system default background
                HotkeyTextBox.Foreground = System.Windows.SystemColors.ControlTextBrush; // Ensure visible text
            }
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isCapturingHotkey)
                return;

            e.Handled = true; // Prevent default text input behavior            // Allow Escape to cancel capture
            if (e.Key == Key.Escape)
            {
                HotkeyTextBox.Text = _originalHotkeyValue;
                _isCapturingHotkey = false;
                HotkeyTextBox.Background = System.Windows.SystemColors.WindowBrush; // Use system default background
                HotkeyTextBox.Foreground = System.Windows.SystemColors.ControlTextBrush; // Ensure visible text
                return;
            }

            // Ignore standalone modifier keys
            if (
                e.Key == Key.LeftCtrl
                || e.Key == Key.RightCtrl
                || e.Key == Key.LeftAlt
                || e.Key == Key.RightAlt
                || e.Key == Key.LeftShift
                || e.Key == Key.RightShift
                || e.Key == Key.LWin
                || e.Key == Key.RWin
                || e.Key == Key.System
            ) // Alt key variations
            {
                return;
            }

            // Require at least one modifier key for hotkeys (except function keys)
            bool hasModifier =
                (
                    Keyboard.Modifiers
                    & (
                        ModifierKeys.Control
                        | ModifierKeys.Alt
                        | ModifierKeys.Shift
                        | ModifierKeys.Windows
                    )
                ) != 0;
            bool isFunctionKey = e.Key >= Key.F1 && e.Key <= Key.F12;

            if (!hasModifier && !isFunctionKey)
            {
                // For non-function keys, require at least one modifier
                return;
            }

            // Build the hotkey string
            var hotkeyBuilder = new StringBuilder();

            // Add modifiers in consistent order
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                hotkeyBuilder.Append("Ctrl+");
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                hotkeyBuilder.Append("Alt+");
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                hotkeyBuilder.Append("Shift+");
            if ((Keyboard.Modifiers & ModifierKeys.Windows) != 0)
                hotkeyBuilder.Append("Win+");

            // Add the main key
            string keyName = GetKeyName(e.Key);
            hotkeyBuilder.Append(keyName); // Set the text and finish capturing
            HotkeyTextBox.Text = hotkeyBuilder.ToString();
            _isCapturingHotkey = false;
            HotkeyTextBox.Background = System.Windows.SystemColors.WindowBrush; // Use system default background
            HotkeyTextBox.Foreground = System.Windows.SystemColors.ControlTextBrush; // Ensure visible text

            // Move focus away to indicate we're done
            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    ApiBaseUrlTextBox.Focus();
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }

        private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // This is handled by PreviewKeyDown, but we need to suppress normal key handling
            if (_isCapturingHotkey)
            {
                e.Handled = true;
            }
        }

        private void HotkeyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Always prevent normal text input - we only want hotkey combinations
            e.Handled = true;
        }

        private string GetKeyName(Key key)
        {
            // Handle special keys
            switch (key)
            {
                case Key.Space:
                    return "Space";
                case Key.Enter:
                    return "Enter";
                case Key.Escape:
                    return "Escape";
                case Key.Tab:
                    return "Tab";
                case Key.Back:
                    return "Backspace";
                case Key.Delete:
                    return "Delete";
                case Key.Insert:
                    return "Insert";
                case Key.Home:
                    return "Home";
                case Key.End:
                    return "End";
                case Key.PageUp:
                    return "PageUp";
                case Key.PageDown:
                    return "PageDown";
                case Key.Up:
                    return "Up";
                case Key.Down:
                    return "Down";
                case Key.Left:
                    return "Left";
                case Key.Right:
                    return "Right";
                case Key.F1:
                case Key.F2:
                case Key.F3:
                case Key.F4:
                case Key.F5:
                case Key.F6:
                case Key.F7:
                case Key.F8:
                case Key.F9:
                case Key.F10:
                case Key.F11:
                case Key.F12:
                    return key.ToString();
                default:
                    // For alphanumeric keys, convert to uppercase
                    return key.ToString().ToUpper();
            }
        }

        private string GetActualApiKey()
        {
            string result = "";

            System.Diagnostics.Debug.WriteLine(
                $"GetActualApiKey: _isApiKeyModified={_isApiKeyModified}"
            );
            System.Diagnostics.Debug.WriteLine(
                $"GetActualApiKey: ApiKeyPasswordBox.Password='{ApiKeyPasswordBox.Password}' (length={ApiKeyPasswordBox.Password?.Length ?? 0})"
            );
            System.Diagnostics.Debug.WriteLine(
                $"GetActualApiKey: _config.ApiKey='{_config.ApiKey}' (length={_config.ApiKey?.Length ?? 0})"
            );

            if (_isApiKeyModified)
            {
                // If user modified the field, use what they entered
                if (string.IsNullOrEmpty(ApiKeyPasswordBox.Password))
                {
                    // User cleared the field
                    result = "";
                    System.Diagnostics.Debug.WriteLine(
                        "GetActualApiKey: User cleared the field -> empty string"
                    );
                }
                else if (ApiKeyPasswordBox.Password.All(c => c == '•'))
                {
                    // User didn't change the masked key, use existing key from config
                    result = _config.ApiKey ?? "";
                    System.Diagnostics.Debug.WriteLine(
                        "GetActualApiKey: Masked key unchanged -> using config key"
                    );
                }
                else
                {
                    // User entered a new key
                    result = ApiKeyPasswordBox.Password;
                    System.Diagnostics.Debug.WriteLine("GetActualApiKey: User entered new key");
                }
            }
            else
            {
                // User didn't modify the field, use existing key from config
                result = _config.ApiKey ?? "";
                System.Diagnostics.Debug.WriteLine(
                    "GetActualApiKey: Field not modified -> using config key"
                );
            }
            System.Diagnostics.Debug.WriteLine(
                $"GetActualApiKey: returning '{result}' (length={result?.Length ?? 0})"
            );
            return result ?? "";
        }

        private string GetSelectedLlmModel()
        {
            if (
                LlmModelComboBox.SelectedItem is ComboBoxItem selectedItem
                && selectedItem.IsEnabled
                && selectedItem.Content != null
            )
            {
                return selectedItem.Content.ToString() ?? "";
            }
            return LlmModelComboBox.Text ?? "";
        }

        private void SetLlmModelSelection(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                LlmModelComboBox.SelectedIndex = -1; // No selection for empty ComboBox
                return;
            }

            // Try to find an existing item that matches the model
            for (int i = 0; i < LlmModelComboBox.Items.Count; i++)
            {
                if (
                    LlmModelComboBox.Items[i] is ComboBoxItem item
                    && item.Content.ToString() == modelName
                )
                {
                    LlmModelComboBox.SelectedIndex = i;
                    return;
                }
            }

            // If not found, add the custom model as a new item
            var customItem = new ComboBoxItem { Content = modelName };
            LlmModelComboBox.Items.Add(customItem);
            LlmModelComboBox.SelectedItem = customItem;
        }
    }
}
