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
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static string GetSelectedText()
        {
            try
            {
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

                // Clear clipboard
                Clipboard.Clear();
                Thread.Sleep(50);

                // Copy selected text (Ctrl+C)
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_C, 0, 0, UIntPtr.Zero);
                keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                Thread.Sleep(100);

                var selectedText = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        selectedText = Clipboard.GetText();
                    }
                }
                catch { }

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
        }

        public static string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
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
