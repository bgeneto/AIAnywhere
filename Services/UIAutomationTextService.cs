using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Linq;

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
                System.Diagnostics.Debug.WriteLine("DEBUG: Starting UI Automation text selection");

                // Get the currently focused element
                var focusedElement = AutomationElement.FocusedElement;
                if (focusedElement == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: No focused element found via UI Automation");
                    return "";
                }

                System.Diagnostics.Debug.WriteLine($"DEBUG: Focused element found: {focusedElement.Current.Name} ({focusedElement.Current.ControlType.LocalizedControlType})");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Element AutomationId: {focusedElement.Current.AutomationId}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Element ClassName: {focusedElement.Current.ClassName}");

                // Try to get selected text using TextPattern
                var selectedText = GetSelectedTextFromTextPattern(focusedElement);
                if (!string.IsNullOrEmpty(selectedText))
                {
                    return selectedText;
                }

                // If no selection found in focused element, try to find text controls in the foreground window
                return GetSelectedTextFromWindow();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: UI Automation error: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine("DEBUG: TextPattern found on focused element");

                    // Get the selection
                    var selection = textPattern.GetSelection();
                    if (selection != null && selection.Length > 0)
                    {
                        var selectedText = selection[0].GetText(-1);
                        System.Diagnostics.Debug.WriteLine($"DEBUG: UI Automation found selected text: '{selectedText}'");
                        
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            return selectedText;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("DEBUG: No text selection found in TextPattern");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Element doesn't support TextPattern");
                }

                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error getting text from TextPattern: {ex.Message}");
                return "";
            }
        }

        private static string GetSelectedTextFromWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Attempting to get selected text from foreground window");

                // Get the foreground window
                var foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: No foreground window found");
                    return "";
                }

                // Get the automation element for the window
                var windowElement = AutomationElement.FromHandle(foregroundWindow);
                if (windowElement == null)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Could not get AutomationElement from window handle");
                    return "";
                }

                System.Diagnostics.Debug.WriteLine($"DEBUG: Window element: {windowElement.Current.Name}");

                // Search for text controls that might have selections
                return SearchForSelectedTextInElement(windowElement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error getting selected text from window: {ex.Message}");
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
                var condition = new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, true);
                var textElements = element.FindAll(TreeScope.Descendants, condition);

                System.Diagnostics.Debug.WriteLine($"DEBUG: Found {textElements.Count} elements with TextPattern");

                foreach (AutomationElement textElement in textElements)
                {
                    try
                    {
                        selectedText = GetSelectedTextFromTextPattern(textElement);
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            System.Diagnostics.Debug.WriteLine($"DEBUG: Found selected text in descendant element: '{selectedText}'");
                            return selectedText;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Error checking text element: {ex.Message}");
                        continue;
                    }
                }

                // Also try looking for focused elements within the window
                var focusCondition = new PropertyCondition(AutomationElement.HasKeyboardFocusProperty, true);
                var focusedElements = element.FindAll(TreeScope.Descendants, focusCondition);

                System.Diagnostics.Debug.WriteLine($"DEBUG: Found {focusedElements.Count} focused elements");

                foreach (AutomationElement focusedElement in focusedElements)
                {
                    try
                    {
                        selectedText = GetSelectedTextFromTextPattern(focusedElement);
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            System.Diagnostics.Debug.WriteLine($"DEBUG: Found selected text in focused element: '{selectedText}'");
                            return selectedText;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Error checking focused element: {ex.Message}");
                        continue;
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error searching for selected text: {ex.Message}");
                return "";
            }
        }

        public static string GetSelectedTextWithFallback()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GetSelectedTextWithFallback() called");

                // First try UI Automation
                var uiAutomationText = GetSelectedTextViaUIAutomation();
                if (!string.IsNullOrEmpty(uiAutomationText))
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: UI Automation succeeded: '{uiAutomationText}'");
                    return uiAutomationText;
                }

                System.Diagnostics.Debug.WriteLine("DEBUG: UI Automation found no selection, checking if there might be clipboard-accessible selection");

                // If UI Automation didn't find anything, it could mean:
                // 1. There's actually no selection
                // 2. The application doesn't expose selection via UI Automation
                // 3. The text is in a control that doesn't support TextPattern

                // For applications that don't support UI Automation properly (like some web browsers),
                // we can still try the clipboard method as a fallback
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: GetSelectedTextWithFallback() exception: {ex.Message}");
                return "";
            }
        }
    }
}
