using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIAnywhere.Services
{
    public class HotkeyService
    {
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CTRL = 0x0002;
        private const int MOD_ALT = 0x0001;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly IntPtr _windowHandle;
        private int _hotkeyId = 1;
        private bool _isRegistered = false;

        public event Action? HotkeyPressed;

        public HotkeyService(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public bool RegisterHotkey(string hotkey)
        {
            try
            {
                if (_isRegistered)
                {
                    UnregisterHotKey(_windowHandle, _hotkeyId);
                    _isRegistered = false;
                }

                var (modifiers, key) = ParseHotkey(hotkey);
                _isRegistered = RegisterHotKey(_windowHandle, _hotkeyId, modifiers, (uint)key);

                return _isRegistered;
            }
            catch
            {
                return false;
            }
        }

        public void UnregisterHotkey()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, _hotkeyId);
                _isRegistered = false;
            }
        }

        public bool ProcessHotkey(IntPtr wParam)
        {
            if (wParam.ToInt32() == _hotkeyId)
            {
                HotkeyPressed?.Invoke();
                return true;
            }
            return false;
        }

        private (uint modifiers, Keys key) ParseHotkey(string hotkey)
        {
            uint modifiers = 0;
            Keys key = Keys.None;

            if (string.IsNullOrWhiteSpace(hotkey))
                return (modifiers, key);

            var parts = hotkey.Split('+');

            // Process all parts except the last one as modifiers
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i].Trim();
                switch (part.ToLower())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= MOD_CTRL;
                        break;
                    case "alt":
                        modifiers |= MOD_ALT;
                        break;
                    case "shift":
                        modifiers |= MOD_SHIFT;
                        break;
                    case "win":
                    case "windows":
                        modifiers |= MOD_WIN;
                        break;
                }
            }

            // The last part is the key itself
            var keyPart = parts[parts.Length - 1].Trim();

            // Handle special cases
            switch (keyPart.ToLower())
            {
                case "space":
                    key = Keys.Space;
                    break;
                case "esc":
                case "escape":
                    key = Keys.Escape;
                    break;
                case "enter":
                    key = Keys.Enter;
                    break;
                case "tab":
                    key = Keys.Tab;
                    break;
                case "backspace":
                    key = Keys.Back;
                    break;
                case "delete":
                    key = Keys.Delete;
                    break;
                case "insert":
                    key = Keys.Insert;
                    break;
                case "home":
                    key = Keys.Home;
                    break;
                case "end":
                    key = Keys.End;
                    break;
                case "pageup":
                    key = Keys.PageUp;
                    break;
                case "pagedown":
                    key = Keys.PageDown;
                    break;
                case "up":
                    key = Keys.Up;
                    break;
                case "down":
                    key = Keys.Down;
                    break;
                case "left":
                    key = Keys.Left;
                    break;
                case "right":
                    key = Keys.Right;
                    break;
                case "+":
                    key = Keys.Oemplus;
                    break;
                case "-":
                    key = Keys.OemMinus;
                    break;
                case ",":
                    key = Keys.Oemcomma;
                    break;
                case ".":
                    key = Keys.OemPeriod;
                    break;
                case "/":
                    key = Keys.OemQuestion;
                    break;
                case ";":
                    key = Keys.OemSemicolon;
                    break;
                case "'":
                    key = Keys.OemQuotes;
                    break;
                case "[":
                    key = Keys.OemOpenBrackets;
                    break;
                case "]":
                    key = Keys.OemCloseBrackets;
                    break;
                case "\\":
                    key = Keys.OemPipe;
                    break;
                case "`":
                    key = Keys.Oemtilde;
                    break;
                default:
                    // Try to parse as a function key
                    if (
                        keyPart.StartsWith("f", StringComparison.OrdinalIgnoreCase)
                        && int.TryParse(keyPart.Substring(1), out int fNum)
                        && fNum >= 1
                        && fNum <= 24
                    )
                    {
                        key = Keys.F1 + (fNum - 1);
                    }
                    // Try to parse as a letter or number
                    else if (keyPart.Length == 1)
                    {
                        char c = keyPart.ToUpper()[0];
                        if (char.IsLetter(c) || char.IsDigit(c))
                        {
                            key = (Keys)c;
                        }
                    }
                    // Handle numeric keypad
                    else if (keyPart.StartsWith("num", StringComparison.OrdinalIgnoreCase))
                    {
                        var numKey = keyPart.Replace(
                            "num",
                            "numpad",
                            StringComparison.OrdinalIgnoreCase
                        );
                        if (Enum.TryParse(numKey, true, out Keys numPadKey))
                        {
                            key = numPadKey;
                        }
                    }
                    // Try to parse as an enum
                    else if (Enum.TryParse(keyPart, true, out Keys parsedKey))
                    {
                        key = parsedKey;
                    }
                    break;
            }

            return (modifiers, key);
        }
    }
}
