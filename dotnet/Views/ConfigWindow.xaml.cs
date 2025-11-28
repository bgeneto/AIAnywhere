using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
                );
            }
            else
            {
                ApiKeyPasswordBox.Password = "";
            }

            // Load available models from config and populate ComboBoxes
            LoadModelsIntoComboBox();

            // Set model selections - find matching item or add custom one
            SetLlmModelSelection(_config.LlmModel);
            SetImageModelSelection(_config.ImageModel);
            SetAudioModelSelection(_config.AudioModel);

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
            DisableTextSelectionCheckBox.IsChecked = _config.DisableTextSelection;

            // Set disable thinking preference
            DisableThinkingCheckBox.IsChecked = _config.DisableThinking;

            // Set debug logging preference
            EnableDebugLoggingCheckBox.IsChecked = _config.EnableDebugLogging;

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

            // Save Image Model
            if (
                ImageModelComboBox.SelectedItem is ComboBoxItem selectedImageItem
                && selectedImageItem.IsEnabled
                && selectedImageItem.Content != null
            )
            {
                _config.ImageModel = selectedImageItem.Content.ToString() ?? "";
            }
            else if (!string.IsNullOrEmpty(ImageModelComboBox.Text))
            {
                _config.ImageModel = ImageModelComboBox.Text;
            }

            // Save Audio Model
            if (
                AudioModelComboBox.SelectedItem is ComboBoxItem selectedAudioItem
                && selectedAudioItem.IsEnabled
                && selectedAudioItem.Content != null
            )
            {
                _config.AudioModel = selectedAudioItem.Content.ToString() ?? "";
            }
            else if (!string.IsNullOrEmpty(AudioModelComboBox.Text))
            {
                _config.AudioModel = AudioModelComboBox.Text;
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
            _config.DisableTextSelection = DisableTextSelectionCheckBox.IsChecked ?? false;

            // Save disable thinking preference
            _config.DisableThinking = DisableThinkingCheckBox.IsChecked ?? false;

            // Save debug logging preference
            _config.EnableDebugLogging = EnableDebugLoggingCheckBox.IsChecked ?? false;

            // Save current models lists from ComboBoxes
            var currentModels = new List<string>();
            foreach (var item in LlmModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content != null)
                {
                    var modelName = comboItem.Content.ToString();
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        currentModels.Add(modelName);
                    }
                }
            }
            _config.Models = currentModels;

            var currentImageModels = new List<string>();
            foreach (var item in ImageModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content != null)
                {
                    var modelName = comboItem.Content.ToString();
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        currentImageModels.Add(modelName);
                    }
                }
            }
            _config.ImageModels = currentImageModels;

            var currentAudioModels = new List<string>();
            foreach (var item in AudioModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content != null)
                {
                    var modelName = comboItem.Content.ToString();
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        currentAudioModels.Add(modelName);
                    }
                }
            }
            _config.AudioModels = currentAudioModels;

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
                            "(Auto-paste provides smoothest workflow)";
                        break;
                    case 1: // ClipboardMode
                        PasteBehaviorDescriptionTextBlock.Text = "(Copy to clipboard, paste manually)";
                        break;
                    case 2: // ReviewMode
                        PasteBehaviorDescriptionTextBlock.Text =
                            "(Show preview window for confirmation)";
                        break;
                    default:
                        PasteBehaviorDescriptionTextBlock.Text =
                            "(Choose how AI results are handled)";
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
                TestButton.Content = "📡 Connection Test";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                        "LLM Model is required. Please select a model or click '⤓ Get Models' to retrieve available options.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    LlmModelComboBox.Focus();
                    return;
                }

                // Try to refresh models if API settings are valid
                await TryRefreshModelsAsync(actualApiKey, ApiBaseUrlTextBox.Text);

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

                var allModels = await FetchAvailableModelsAsync(
                    ApiBaseUrlTextBox.Text,
                    actualApiKey
                );

                if (allModels.Any())
                {
                    // Store current selections
                    var currentTextSelection = GetSelectedLlmModel();
                    var currentImageSelection = GetSelectedImageModel();
                    var currentAudioSelection = GetSelectedAudioModel();

                    // Categorize models using the new filter methods
                    var textModels = FilterTextModels(allModels);
                    var imageModels = FilterImageModels(allModels);
                    var audioModels = FilterAudioModels(allModels);

                    // Populate Text Models
                    LlmModelComboBox.Items.Clear();
                    foreach (var model in textModels.OrderBy(m => m))
                    {
                        LlmModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                    }
                    _config.Models = textModels.ToList();

                    // Populate Image Models
                    ImageModelComboBox.Items.Clear();
                    foreach (var model in imageModels.OrderBy(m => m))
                    {
                        ImageModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                    }
                    _config.ImageModels = imageModels.ToList();

                    // Populate Audio Models
                    AudioModelComboBox.Items.Clear();
                    foreach (var model in audioModels.OrderBy(m => m))
                    {
                        AudioModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                    }
                    _config.AudioModels = audioModels.ToList();

                    // Restore selections if they exist in the new lists
                    if (textModels.Contains(currentTextSelection))
                    {
                        SetLlmModelSelection(currentTextSelection);
                    }
                    else if (textModels.Any())
                    {
                        LlmModelComboBox.SelectedIndex = 0;
                    }

                    if (imageModels.Contains(currentImageSelection))
                    {
                        SetImageModelSelection(currentImageSelection);
                    }
                    else if (imageModels.Any())
                    {
                        ImageModelComboBox.SelectedIndex = 0;
                    }

                    if (audioModels.Contains(currentAudioSelection))
                    {
                        SetAudioModelSelection(currentAudioSelection);
                    }
                    else if (audioModels.Any())
                    {
                        AudioModelComboBox.SelectedIndex = 0;
                    }

                    ModelStatusTextBlock.Text =
                        $"Retrieved {allModels.Count} models successfully (Text: {textModels.Count}, Image: {imageModels.Count}, Audio: {audioModels.Count})";
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
                RetrieveModelsButton.Content = "⤓ Get Models";
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

            e.Handled = true;

            // Allow Escape to cancel capture
            if (e.Key == Key.Escape)
            {
                HotkeyTextBox.Text = _originalHotkeyValue;
                _isCapturingHotkey = false;
                HotkeyTextBox.Background = System.Windows.SystemColors.WindowBrush;
                HotkeyTextBox.Foreground = System.Windows.SystemColors.ControlTextBrush;
                return;
            }

            // Determine actual key and modifiers
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;

            // Skip standalone modifier keys
            if (
                key == Key.LeftCtrl
                || key == Key.RightCtrl
                || key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LWin
                || key == Key.RWin
                || key == Key.LeftShift
                || key == Key.RightShift
            )
            {
                return;
            }

            // Require at least one modifier or function key
            if (modifiers == ModifierKeys.None && (key < Key.F1 || key > Key.F24))
                return;

            try
            {
                var gesture = new KeyGesture(key, modifiers);
                HotkeyTextBox.Text = gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
            }
            catch
            {
                // Fallback to original value on error
                HotkeyTextBox.Text = _originalHotkeyValue;
            }

            // Complete capture and restore appearance
            _isCapturingHotkey = false;
            HotkeyTextBox.Background = System.Windows.SystemColors.WindowBrush;
            HotkeyTextBox.Foreground = System.Windows.SystemColors.ControlTextBrush;

            // Move focus away to finish capture
            SaveButton.Focus();
        }

        private void HotkeyTextBox_PreviewTextInput(
            object sender,
            System.Windows.Input.TextCompositionEventArgs e
        )
        {
            // Prevent text input when capturing hotkeys
            if (_isCapturingHotkey)
            {
                e.Handled = true;
            }
        }

        private string GetActualApiKey()
        {
            // If password was modified, use the current value
            if (_isApiKeyModified)
            {
                return ApiKeyPasswordBox.Password;
            }

            // If password wasn't modified, use the original value from config
            return _config.ApiKey ?? "";
        }

        private void LoadModelsIntoComboBox()
        {
            // Clear existing items
            LlmModelComboBox.Items.Clear();
            ImageModelComboBox.Items.Clear();
            AudioModelComboBox.Items.Clear();

            int totalModelsLoaded = 0;

            // Add text models from config if any exist - apply filtering to ensure clean categorization
            if (_config.Models != null && _config.Models.Any())
            {
                var filteredTextModels = FilterTextModels(_config.Models);
                foreach (var model in filteredTextModels.OrderBy(m => m))
                {
                    LlmModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                }
                totalModelsLoaded += filteredTextModels.Count;

                // Update config with filtered models
                _config.Models = filteredTextModels.ToList();
            }

            // Add image models from config if any exist
            if (_config.ImageModels != null && _config.ImageModels.Any())
            {
                var filteredImageModels = FilterImageModels(_config.ImageModels);
                foreach (var model in filteredImageModels.OrderBy(m => m))
                {
                    ImageModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                }
                totalModelsLoaded += filteredImageModels.Count;

                // Update config with filtered models
                _config.ImageModels = filteredImageModels.ToList();
            }

            // Add audio models from config if any exist
            if (_config.AudioModels != null && _config.AudioModels.Any())
            {
                var filteredAudioModels = FilterAudioModels(_config.AudioModels);
                foreach (var model in filteredAudioModels.OrderBy(m => m))
                {
                    AudioModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                }
                totalModelsLoaded += filteredAudioModels.Count;

                // Update config with filtered models
                _config.AudioModels = filteredAudioModels.ToList();
            }

            // Update unified status message
            if (totalModelsLoaded > 0)
            {
                ModelStatusTextBlock.Text = $"Loaded {totalModelsLoaded} models from configuration";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Green
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // No models in config, show instruction
                ModelStatusTextBlock.Text =
                    "Enter API key and base URL first, then click '⤓ Get Models'";
                ModelStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.Gray
                );
                ModelStatusTextBlock.Visibility = Visibility.Visible;
            }
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

        private string GetSelectedImageModel()
        {
            if (
                ImageModelComboBox.SelectedItem is ComboBoxItem selectedItem
                && selectedItem.IsEnabled
                && selectedItem.Content != null
            )
            {
                return selectedItem.Content.ToString() ?? "";
            }
            return ImageModelComboBox.Text ?? "";
        }

        private string GetSelectedAudioModel()
        {
            if (
                AudioModelComboBox.SelectedItem is ComboBoxItem selectedItem
                && selectedItem.IsEnabled
                && selectedItem.Content != null
            )
            {
                return selectedItem.Content.ToString() ?? "";
            }
            return AudioModelComboBox.Text ?? "";
        }

        private void SetLlmModelSelection(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return;

            foreach (var item in LlmModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == modelName)
                {
                    LlmModelComboBox.SelectedItem = comboItem;
                    return;
                }
            }

            // If model not found in list, add it as a custom item
            var customItem = new ComboBoxItem { Content = modelName };
            LlmModelComboBox.Items.Add(customItem);
            LlmModelComboBox.SelectedItem = customItem;
        }

        private void SetImageModelSelection(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return;

            foreach (var item in ImageModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == modelName)
                {
                    ImageModelComboBox.SelectedItem = comboItem;
                    return;
                }
            }

            // If model not found in list, add it as a custom item
            var customItem = new ComboBoxItem { Content = modelName };
            ImageModelComboBox.Items.Add(customItem);
            ImageModelComboBox.SelectedItem = customItem;
        }

        private void SetAudioModelSelection(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return;

            foreach (var item in AudioModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == modelName)
                {
                    AudioModelComboBox.SelectedItem = comboItem;
                    return;
                }
            }

            // If model not found in list, add it as a custom item
            var customItem = new ComboBoxItem { Content = modelName };
            AudioModelComboBox.Items.Add(customItem);
            AudioModelComboBox.SelectedItem = customItem;
        }

        private async Task TryRefreshModelsAsync(string apiKey, string apiBaseUrl)
        {
            try
            {
                // Only attempt to refresh if we have the required parameters
                if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiBaseUrl))
                {
                    var models = await FetchAvailableModelsAsync(apiBaseUrl, apiKey);
                    if (models.Any())
                    {
                        // Store current selection
                        var currentSelection = GetSelectedLlmModel();

                        // Update the ComboBox with new models
                        LlmModelComboBox.Items.Clear();
                        foreach (var model in models.OrderBy(m => m))
                        {
                            LlmModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                        }

                        // Update the configuration's Models list
                        _config.Models = models.ToList();

                        // Restore selection if it exists in the new list
                        if (models.Contains(currentSelection))
                        {
                            SetLlmModelSelection(currentSelection);
                        }
                        else if (models.Any())
                        {
                            LlmModelComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch
            {
                // Silently fail for model refresh during save - not critical
                // The user can always manually refresh models if needed
            }
        }

        /// <summary>
        /// Filter models to include only text/chat models, excluding image, audio, and special models
        /// </summary>
        private List<string> FilterTextModels(IEnumerable<string> models)
        {
            return models.Where(m =>
            {
                var modelLower = m.ToLower();
                var isImageModel = modelLower.Contains("dall") || modelLower.Contains("flux") ||
                                   modelLower.Contains("image") || modelLower.Contains("midjourney") ||
                                   modelLower.Contains("stable") || modelLower.Contains("dream") ||
                                   modelLower.Contains("shuttle") || modelLower.Contains("kandinsky") ||
                                   modelLower.Contains("playground");

                var isAudioModel = modelLower.Contains("whisper") || modelLower.Contains("speech") ||
                                   modelLower.Contains("transcrib") || modelLower.Contains("tts") ||
                                   modelLower.Contains("audio") || modelLower.Contains("voice");

                var isSpecialModel = modelLower.Contains("embed") || modelLower.Contains("clip") ||
                                     modelLower.Contains("moderation") || modelLower.Contains("fallback");

                return !(isImageModel || isAudioModel || isSpecialModel);
            }).ToList();
        }

        /// <summary>
        /// Filter models to include only image generation models
        /// </summary>
        private List<string> FilterImageModels(IEnumerable<string> models)
        {
            return models.Where(m =>
            {
                var modelLower = m.ToLower();
                return modelLower.Contains("dall") || modelLower.Contains("flux") ||
                       modelLower.Contains("image") || modelLower.Contains("midjourney") ||
                       modelLower.Contains("stable") || modelLower.Contains("dream") ||
                       modelLower.Contains("shuttle") || modelLower.Contains("kandinsky") ||
                       modelLower.Contains("playground");
            }).ToList();
        }

        /// <summary>
        /// Filter models to include only audio models
        /// </summary>
        private List<string> FilterAudioModels(IEnumerable<string> models)
        {
            return models.Where(m =>
            {
                var modelLower = m.ToLower();
                return modelLower.Contains("whisper") || modelLower.Contains("audio") ||
                       modelLower.Contains("speech") || modelLower.Contains("transcrib") ||
                       modelLower.Contains("stt") || modelLower.Contains("voice");
            }).ToList();
        }
    }
}
