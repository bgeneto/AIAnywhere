using System;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace AIAnywhere.Services
{
    public static class UIAutomationTextService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static string GetSelectedTextViaUIAutomation()
        {
            try
            {
                // Get the currently focused element
                var focusedElement = AutomationElement.FocusedElement;
                if (focusedElement == null)
                {
                    return "";
                }

                // Try to get selected text using TextPattern
                var selectedText = GetSelectedTextFromTextPattern(focusedElement);
                if (!string.IsNullOrEmpty(selectedText))
                {
                    return selectedText;
                }

                // If no selection found in focused element, try to find text controls in the foreground window
                return GetSelectedTextFromWindow();
            }
            catch
            {
                return "";
            }
        }

        private static string GetSelectedTextFromTextPattern(AutomationElement element)
        {
            try
            {
                // Try to get TextPattern from the element
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out object textPatternObj))
                {
                    var textPattern = (TextPattern)textPatternObj;

                    // Get the selection
                    var selection = textPattern.GetSelection();
                    if (selection != null && selection.Length > 0)
                    {
                        var selectedText = selection[0].GetText(-1);
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            return selectedText;
                        }
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private static string GetSelectedTextFromWindow()
        {
            try
            {
                var foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    return "";
                }

                // Get the automation element for the window
                var windowElement = AutomationElement.FromHandle(foregroundWindow);
                if (windowElement == null)
                {
                    return "";
                }

                // Search for text controls that might have selections
                return SearchForSelectedTextInElement(windowElement);
            }
            catch
            {
                return "";
            }
        }

        private static string SearchForSelectedTextInElement(AutomationElement element)
        {
            try
            {
                // First, check the current element for text selection
                var selectedText = GetSelectedTextFromTextPattern(element);
                if (!string.IsNullOrEmpty(selectedText))
                {
                    return selectedText;
                }

                // Find all descendant elements that support TextPattern
                var condition = new PropertyCondition(
                    AutomationElement.IsTextPatternAvailableProperty,
                    true
                );
                var textElements = element.FindAll(TreeScope.Descendants, condition);

                foreach (AutomationElement textElement in textElements)
                {
                    try
                    {
                        selectedText = GetSelectedTextFromTextPattern(textElement);
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            return selectedText;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // Also try looking for focused elements within the window
                var focusCondition = new PropertyCondition(
                    AutomationElement.HasKeyboardFocusProperty,
                    true
                );
                var focusedElements = element.FindAll(TreeScope.Descendants, focusCondition);

                foreach (AutomationElement focusedElement in focusedElements)
                {
                    try
                    {
                        selectedText = GetSelectedTextFromTextPattern(focusedElement);
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            return selectedText;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        public static string GetSelectedTextWithFallback()
        {
            try
            {
                // First try UI Automation
                var uiAutomationText = GetSelectedTextViaUIAutomation();
                if (!string.IsNullOrEmpty(uiAutomationText))
                {
                    return uiAutomationText;
                }

                // If UI Automation didn't find anything, it could mean:
                // 1. There's actually no selection
                // 2. The application doesn't expose selection via UI Automation
                // 3. The text is in a control that doesn't support TextPattern

                // For applications that don't support UI Automation properly (like some web browsers),
                // we can still try the clipboard method as a fallback
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}
