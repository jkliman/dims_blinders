using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace StarCitizenBlinders
{
    public partial class SettingsWindow : Window
    {
        private readonly BlinderManager _blinderManager;
        private Key _capturedKey = Key.None;
        private ModifierKeys _capturedModifiers = ModifierKeys.None;

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(System.IntPtr lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public SettingsWindow(BlinderManager blinderManager)
        {
            InitializeComponent();
            _blinderManager = blinderManager;
            LoadSettings();
            LoadDebugInfo();
        }

        private void LoadSettings()
        {
            var config = Config.Instance;

            TxtViewportWidth.Text = config.ViewportWidth.ToString();
            TxtViewportHeight.Text = config.ViewportHeight.ToString();

            _capturedKey = config.HotKey;
            _capturedModifiers = config.HotKeyModifiers;
            TxtHotkey.Text = config.HotkeyDisplay;

            TxtBlinderColor.Text = config.BlinderColor;
            TxtOpacity.Text = config.BlinderOpacity.ToString("F2");

            ChkConfineMouse.IsChecked = config.ConfineMouse;

            // Manual bounds
            ChkUseManualBounds.IsChecked = config.UseManualBounds;
            TxtMouseLeft.Text = config.ManualLeft.ToString();
            TxtMouseTop.Text = config.ManualTop.ToString();
            TxtMouseRight.Text = config.ManualRight.ToString();
            TxtMouseBottom.Text = config.ManualBottom.ToString();
        }

        private void LoadDebugInfo()
        {
            // Get all the values used in calculations
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            TxtDebug.Text = $"Virtual Screen: L={virtualLeft}, T={virtualTop}, W={virtualWidth}, H={virtualHeight}";
        }

        private void BtnApplyMouseBounds_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtMouseLeft.Text, out int left) ||
                !int.TryParse(TxtMouseTop.Text, out int top) ||
                !int.TryParse(TxtMouseRight.Text, out int right) ||
                !int.TryParse(TxtMouseBottom.Text, out int bottom))
            {
                ShowError("Invalid mouse bounds values");
                return;
            }

            var rect = new RECT { Left = left, Top = top, Right = right, Bottom = bottom };
            ClipCursor(ref rect);
            TxtStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
            TxtStatus.Text = $"Applied mouse bounds: ({left}, {top}) to ({right}, {bottom})";
        }

        private void TxtHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Ignore modifier keys alone
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            _capturedKey = key;
            _capturedModifiers = Keyboard.Modifiers;

            // Build display string
            var parts = new System.Collections.Generic.List<string>();
            if (_capturedModifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (_capturedModifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (_capturedModifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (_capturedModifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            parts.Add(_capturedKey.ToString());

            TxtHotkey.Text = string.Join("+", parts);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate and save settings
            if (!int.TryParse(TxtViewportWidth.Text, out int viewportWidth) || viewportWidth <= 0)
            {
                ShowError("Invalid viewport width");
                return;
            }
            if (!int.TryParse(TxtViewportHeight.Text, out int viewportHeight) || viewportHeight <= 0)
            {
                ShowError("Invalid viewport height");
                return;
            }
            if (!double.TryParse(TxtOpacity.Text, out double opacity) || opacity < 0 || opacity > 1)
            {
                ShowError("Opacity must be between 0 and 1");
                return;
            }
            if (_capturedKey == Key.None)
            {
                ShowError("Please set a hotkey");
                return;
            }

            // Always validate and save manual bounds (so they're preserved even when disabled)
            bool useManualBounds = ChkUseManualBounds.IsChecked ?? false;
            if (!int.TryParse(TxtMouseLeft.Text, out int manualLeft) ||
                !int.TryParse(TxtMouseTop.Text, out int manualTop) ||
                !int.TryParse(TxtMouseRight.Text, out int manualRight) ||
                !int.TryParse(TxtMouseBottom.Text, out int manualBottom))
            {
                ShowError("Invalid manual bounds values");
                return;
            }

            var config = Config.Instance;
            config.ViewportWidth = viewportWidth;
            config.ViewportHeight = viewportHeight;
            config.HotKey = _capturedKey;
            config.HotKeyModifiers = _capturedModifiers;
            config.BlinderColor = TxtBlinderColor.Text;
            config.BlinderOpacity = opacity;
            config.ConfineMouse = ChkConfineMouse.IsChecked ?? true;
            config.UseManualBounds = useManualBounds;
            config.ManualLeft = manualLeft;
            config.ManualTop = manualTop;
            config.ManualRight = manualRight;
            config.ManualBottom = manualBottom;

            config.Save();

            // Refresh the blinder manager with new settings
            _blinderManager.RefreshConfig();
            HotkeyManager.Instance.ReregisterHotkey();

            TxtStatus.Text = "Settings saved successfully!";

            // Close after brief delay
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string message)
        {
            TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
            TxtStatus.Text = message;
        }
    }
}
