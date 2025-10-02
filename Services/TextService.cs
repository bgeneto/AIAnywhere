using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace AIAnywhere.Services
{
    public class TextService
    {
        [DllImport("user32.dll")]
        private static extern bool GetClipboardData();

        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte bVk,
            byte bScan,
            uint dwFlags,
            UIntPtr dwExtraInfo
        );

        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const byte VK_A = 0x41;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static string GetSelectedText()
        {
            try
            {
                // First, try UI Automation approach (non-intrusive)
                var uiAutomationText = UIAutomationTextService.GetSelectedTextViaUIAutomation();
                if (!string.IsNullOrEmpty(uiAutomationText))
                {
                    LogDebugTextCapture("UI Automation", uiAutomationText.Length);
                    return uiAutomationText;
                }

                // Fallback to keyboard simulation method for applications that don't support UI Automation
                var keyboardText = GetSelectedTextViaKeyboardSimulation();
                if (!string.IsNullOrEmpty(keyboardText))
                {
                    LogDebugTextCapture("Keyboard Simulation", keyboardText.Length);
                    return keyboardText;
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private static void LogDebugTextCapture(string method, int length)
        {
            try
            {
                var config = ConfigurationService.GetConfiguration();
                if (config.EnableDebugLogging)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                    var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_debug");
                    Directory.CreateDirectory(debugDir);

                    var filename = $"text_capture_{timestamp}.log";
                    var filePath = Path.Combine(debugDir, filename);

                    var logContent = $"[{timestamp}] Text Capture - Method: {method}, Length: {length} chars{Environment.NewLine}";
                    File.AppendAllText(filePath, logContent);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static string GetSelectedTextViaKeyboardSimulation()
        {
            try
            {
                // Get the currently focused window first
                var currentWindow = GetForegroundWindow();

                // Save current clipboard content
                var originalClipboard = "";
                var originalClipboardFormat = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                        originalClipboardFormat = "text";
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        originalClipboardFormat = "image";
                    }
                }
                catch
                {
                    // Error reading original clipboard
                }

                // Clear clipboard to detect if Ctrl+C works
                try
                {
                    Clipboard.Clear();
                }
                catch
                {
                    // Error clearing clipboard
                }

                // Give some time for clipboard to clear
                Thread.Sleep(100);

                // Send Ctrl+C with more reliable key event handling
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                Thread.Sleep(10); // Small delay between key press and key combination
                keybd_event(VK_C, 0, 0, UIntPtr.Zero);
                Thread.Sleep(10); // Small delay before key release
                keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Thread.Sleep(10);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Wait longer for applications to respond
                Thread.Sleep(250); // Increased wait time

                var selectedText = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        selectedText = Clipboard.GetText();

                        // Check if this is different from original clipboard content
                        if (selectedText == originalClipboard && originalClipboardFormat == "text")
                        {
                            selectedText = ""; // No actual selection was made
                        }
                    }
                    else
                    {
                        // No text in clipboard after Ctrl+C
                    }
                }
                catch
                {
                    // Error reading clipboard after Ctrl+C
                }

                // Restore original clipboard content
                try
                {
                    if (
                        originalClipboardFormat == "text"
                        && !string.IsNullOrEmpty(originalClipboard)
                    )
                    {
                        Clipboard.SetText(originalClipboard);
                    }
                    else if (originalClipboardFormat == "image")
                    {
                        // Can't easily restore image content, so just clear
                        Clipboard.Clear();
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                }
                catch
                {
                    // Error restoring clipboard
                }

                return selectedText;
            }
            catch
            {
                return "";
            }
        }

        public static void ReplaceSelectedText(string newText)
        {
            try
            {
                if (string.IsNullOrEmpty(newText))
                    return;

                // Save current clipboard content
                var originalClipboard = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                    }
                }
                catch { }

                // Set new text to clipboard
                Clipboard.SetText(newText);
                Thread.Sleep(50);

                // Paste new text (Ctrl+V)
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                Thread.Sleep(100);

                // Restore original clipboard content
                try
                {
                    if (!string.IsNullOrEmpty(originalClipboard))
                    {
                        Clipboard.SetText(originalClipboard);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error replacing text: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public static void InsertText(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return;

                // Save current clipboard content
                var originalClipboard = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                    }
                }
                catch { }

                // Set text to clipboard
                Clipboard.SetText(text);
                Thread.Sleep(50);

                // Paste text (Ctrl+V)
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                Thread.Sleep(100);

                // Restore original clipboard content
                try
                {
                    if (!string.IsNullOrEmpty(originalClipboard))
                    {
                        Clipboard.SetText(originalClipboard);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inserting text: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public static string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var clipboardText = Clipboard.GetText();
                    return clipboardText;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public static void SendPasteCommand()
        {
            try
            {
                // Send Ctrl+V keystroke
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch { }
        }

        public static IntPtr GetCurrentForegroundWindow()
        {
            return GetForegroundWindow();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void RestoreFocusToWindow(IntPtr windowHandle)
        {
            try
            {
                if (windowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(windowHandle);
                }
            }
            catch { }
        }
    }
}
