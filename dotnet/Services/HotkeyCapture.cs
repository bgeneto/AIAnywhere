using System;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace AIAnywhere.Services
{
    public static class HotkeyCapture
    {
        public static string CaptureKeyCombo(System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.DeadCharProcessed || key == Key.None)
                return string.Empty;

            var modifiers = new StringBuilder();

            // Capture modifier keys
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                modifiers.Append("Ctrl+");

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                modifiers.Append("Alt+");

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                modifiers.Append("Shift+");

            if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                modifiers.Append("Win+");

            // Convert Key to readable string
            var keyText = KeyToString(key);

            if (string.IsNullOrEmpty(keyText))
                return string.Empty;

            // Require at least one modifier for most keys (except function keys)
            bool hasModifier = modifiers.Length > 0;
            bool isFunctionKey = key >= Key.F1 && key <= Key.F12;

            if (!hasModifier && !isFunctionKey)
                return string.Empty;

            return modifiers + keyText;
        }

        private static string KeyToString(Key key)
        {
            switch (key)
            {
                case Key.Space:
                    return "Space";
                case Key.Escape:
                    return "Esc";
                case Key.Enter:
                    return "Enter";
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
                // Handle special keys that don't have good ToString() representations
                case Key.OemPlus:
                    return "+";
                case Key.OemMinus:
                    return "-";
                case Key.OemComma:
                    return ",";
                case Key.OemPeriod:
                    return ".";
                case Key.OemQuestion:
                    return "/";
                case Key.OemSemicolon:
                    return ";";
                case Key.OemQuotes:
                    return "'";
                case Key.OemOpenBrackets:
                    return "[";
                case Key.OemCloseBrackets:
                    return "]";
                case Key.OemPipe:
                    return "\\";
                case Key.OemTilde:
                    return "`";
                // Ignore modifier keys by themselves
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LWin:
                case Key.RWin:
                case Key.System: // Alt key events
                    return string.Empty;
                default:
                    // Handle function keys
                    if (key >= Key.F1 && key <= Key.F24)
                        return key.ToString();

                    // Handle alphanumeric keys
                    var keyString = key.ToString();
                    if (keyString.Length == 1 && (char.IsLetterOrDigit(keyString[0])))
                        return keyString.ToUpper();

                    // Handle numeric keypad
                    if (keyString.StartsWith("NumPad"))
                        return keyString.Replace("NumPad", "Num");

                    return keyString;
            }
        }
    }
}
