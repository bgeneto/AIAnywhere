using System;
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
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const byte VK_A = 0x41;
        private const uint KEYEVENTF_KEYUP = 0x0002;        public static string GetSelectedText()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GetSelectedText() called - trying UI Automation first");
                
                // First, try UI Automation approach (non-intrusive)
                var uiAutomationText = UIAutomationTextService.GetSelectedTextViaUIAutomation();
                if (!string.IsNullOrEmpty(uiAutomationText))
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: ✓ UI Automation succeeded: '{uiAutomationText}'");
                    return uiAutomationText;
                }
                
                System.Diagnostics.Debug.WriteLine("DEBUG: UI Automation found no selection, trying keyboard simulation fallback");
                  // Fallback to keyboard simulation method for applications that don't support UI Automation
                var keyboardText = GetSelectedTextViaKeyboardSimulation();
                if (!string.IsNullOrEmpty(keyboardText))
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: ✓ Keyboard simulation succeeded: '{keyboardText}'");
                    return keyboardText;
                }
                
                System.Diagnostics.Debug.WriteLine("DEBUG: No selected text found via any method");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: GetSelectedText() exception: {ex.Message}");
                return "";
            }
        }

        private static string GetSelectedTextViaKeyboardSimulation()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GetSelectedTextViaKeyboardSimulation() called");
                
                // Get the currently focused window first
                var currentWindow = GetForegroundWindow();
                System.Diagnostics.Debug.WriteLine($"DEBUG: Current foreground window: {currentWindow}");
                
                // Save current clipboard content
                var originalClipboard = "";
                var originalClipboardFormat = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                        originalClipboardFormat = "text";
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Original clipboard content: '{originalClipboard}'");
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        originalClipboardFormat = "image";
                        System.Diagnostics.Debug.WriteLine("DEBUG: Original clipboard contains image");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Error reading original clipboard: {ex.Message}");
                }

                // Clear clipboard to detect if Ctrl+C works
                try
                {
                    Clipboard.Clear();
                    System.Diagnostics.Debug.WriteLine("DEBUG: Cleared clipboard");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Error clearing clipboard: {ex.Message}");
                }
                
                // Give some time for clipboard to clear
                Thread.Sleep(100);

                System.Diagnostics.Debug.WriteLine("DEBUG: Sending Ctrl+C to copy selected text");
                
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
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Retrieved selected text: '{selectedText}'");
                        
                        // Check if this is different from original clipboard content
                        if (selectedText == originalClipboard && originalClipboardFormat == "text")
                        {
                            System.Diagnostics.Debug.WriteLine("DEBUG: Selected text is same as original clipboard - likely no text was selected");
                            selectedText = ""; // No actual selection was made
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("DEBUG: No text found in clipboard after Ctrl+C");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Error reading clipboard after Ctrl+C: {ex.Message}");
                }

                // Restore original clipboard content
                try
                {
                    if (originalClipboardFormat == "text" && !string.IsNullOrEmpty(originalClipboard))
                    {
                        Clipboard.SetText(originalClipboard);
                        System.Diagnostics.Debug.WriteLine("DEBUG: Restored original clipboard text content");
                    }
                    else if (originalClipboardFormat == "image")
                    {
                        // Can't easily restore image content, so just clear
                        Clipboard.Clear();
                        System.Diagnostics.Debug.WriteLine("DEBUG: Cleared clipboard (had image content)");
                    }
                    else
                    {
                        Clipboard.Clear();
                        System.Diagnostics.Debug.WriteLine("DEBUG: Cleared clipboard (no original content)");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Error restoring clipboard: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"DEBUG: GetSelectedTextViaKeyboardSimulation() returning: '{selectedText}'");
                return selectedText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: GetSelectedTextViaKeyboardSimulation() exception: {ex.Message}");
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
                MessageBox.Show($"Error replacing text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error inserting text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        public static string GetClipboardText()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GetClipboardText() called");
                if (Clipboard.ContainsText())
                {
                    var clipboardText = Clipboard.GetText();
                    System.Diagnostics.Debug.WriteLine($"DEBUG: GetClipboardText() returning: '{clipboardText}'");
                    return clipboardText;
                }
                System.Diagnostics.Debug.WriteLine("DEBUG: GetClipboardText() - clipboard contains no text");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: GetClipboardText() exception: {ex.Message}");
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
        }        [DllImport("user32.dll")]
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
