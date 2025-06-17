using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

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
        }        private (uint modifiers, Keys key) ParseHotkey(string hotkey)
        {
            uint modifiers = 0;
            Keys key = Keys.None;

            // Clean up the hotkey string (handle escaped characters)
            var cleanHotkey = hotkey.Replace("\\u002B", "+").Replace("\\u002b", "+");
            var parts = cleanHotkey.Split('+');

            foreach (var part in parts)
            {
                var trimmed = part.Trim().ToLower();
                switch (trimmed)
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
                    default:
                        // Try to parse as a key
                        if (trimmed.Length == 1)
                        {
                            // Single character key
                            var upperChar = char.ToUpper(trimmed[0]);
                            if (char.IsLetter(upperChar))
                            {
                                key = (Keys)upperChar;
                            }
                            else if (char.IsDigit(upperChar))
                            {
                                key = (Keys)('0' + (upperChar - '0'));
                            }
                        }
                        else if (Enum.TryParse<Keys>(trimmed, true, out var parsedKey))
                        {
                            key = parsedKey;
                        }
                        break;
                }
            }

            return (modifiers, key);
        }
    }
}
