using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace StarCitizenBlinders
{
    public partial class BlinderWindow : Window
    {
        // Window styles for click-through behavior
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public BlinderWindow()
        {
            InitializeComponent();

            // Apply configuration
            var config = Config.Instance;
            var color = (Color)ColorConverter.ConvertFromString(config.BlinderColor);
            BlinderRect.Fill = new SolidColorBrush(color);
            BlinderRect.Opacity = config.BlinderOpacity;

            Loaded += BlinderWindow_Loaded;
        }

        private void BlinderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Make the window click-through so it doesn't interfere with the game
            MakeClickThrough();
        }

        private void MakeClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            // Add transparent, layered, toolwindow (hide from alt-tab), and noactivate styles
            SetWindowLong(hwnd, GWL_EXSTYLE,
                extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        }

        public void SetPosition(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }
}
