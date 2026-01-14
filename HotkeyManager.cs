using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace StarCitizenBlinders
{
    public class HotkeyManager : IDisposable
    {
        private static HotkeyManager? _instance;
        public static HotkeyManager Instance => _instance ??= new HotkeyManager();

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        private HwndSource? _source;
        private IntPtr _windowHandle;
        private BlinderManager? _blinderManager;
        private bool _disposed;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifier key constants for RegisterHotKey
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        private HotkeyManager() { }

        public void Initialize(BlinderManager blinderManager)
        {
            _blinderManager = blinderManager;

            // Create a hidden window to receive hotkey messages
            var helper = new WindowInteropHelper(new Window());
            helper.EnsureHandle();
            _windowHandle = helper.Handle;

            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(HwndHook);

            RegisterConfiguredHotkey();
        }

        private void RegisterConfiguredHotkey()
        {
            var config = Config.Instance;

            uint modifiers = MOD_NOREPEAT; // Prevent repeat when holding
            if (config.HotKeyModifiers.HasFlag(ModifierKeys.Alt)) modifiers |= MOD_ALT;
            if (config.HotKeyModifiers.HasFlag(ModifierKeys.Control)) modifiers |= MOD_CONTROL;
            if (config.HotKeyModifiers.HasFlag(ModifierKeys.Shift)) modifiers |= MOD_SHIFT;
            if (config.HotKeyModifiers.HasFlag(ModifierKeys.Windows)) modifiers |= MOD_WIN;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(config.HotKey);

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, vk))
            {
                MessageBox.Show(
                    $"Failed to register hotkey {config.HotkeyDisplay}. It may already be in use by another application.",
                    "Hotkey Registration Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _blinderManager?.Toggle();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void ReregisterHotkey()
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            RegisterConfiguredHotkey();
        }

        public void Dispose()
        {
            if (_disposed) return;

            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            _source?.RemoveHook(HwndHook);
            _disposed = true;
        }
    }
}
