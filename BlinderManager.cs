using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace StarCitizenBlinders
{
    public class BlinderManager : IDisposable
    {
        private BlinderWindow? _leftBlinder;
        private BlinderWindow? _rightBlinder;
        private bool _isActive;
        private bool _disposed;

        // Store calculated viewport bounds for mouse confinement
        private int _viewportLeft;
        private int _viewportTop;
        private int _viewportRight;
        private int _viewportBottom;

        public event EventHandler<bool>? StateChanged;

        public bool IsActive => _isActive;

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public BlinderManager()
        {
            CreateBlinderWindows();
        }

        private void CreateBlinderWindows()
        {
            var config = Config.Instance;

            // Get the virtual screen bounds (all monitors combined)
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            // Always calculate blinder positions based on viewport dimensions centered on virtual screen
            double blinderViewportLeft = (virtualWidth - config.ViewportWidth) / 2.0 + virtualLeft;
            double blinderViewportRight = blinderViewportLeft + config.ViewportWidth;

            // For mouse confinement, use manual bounds if enabled, otherwise use auto-calculated
            if (config.UseManualBounds)
            {
                _viewportLeft = config.ManualLeft;
                _viewportTop = config.ManualTop;
                _viewportRight = config.ManualRight;
                _viewportBottom = config.ManualBottom;
            }
            else
            {
                _viewportLeft = (int)blinderViewportLeft;
                _viewportTop = (int)((virtualHeight - config.ViewportHeight) / 2.0 + virtualTop);
                _viewportRight = (int)blinderViewportRight;
                _viewportBottom = (int)(_viewportTop + config.ViewportHeight);
            }

            // Left blinder: from virtual screen left to viewport left, full height
            double leftBlinderWidth = blinderViewportLeft - virtualLeft;

            // Right blinder: from viewport right to virtual screen right, full height
            double rightBlinderWidth = (virtualLeft + virtualWidth) - blinderViewportRight;

            // Create left blinder (covers from top to bottom of screen)
            _leftBlinder = new BlinderWindow();
            _leftBlinder.SetPosition(virtualLeft, virtualTop, leftBlinderWidth, virtualHeight);

            // Create right blinder (covers from top to bottom of screen)
            _rightBlinder = new BlinderWindow();
            _rightBlinder.SetPosition(blinderViewportRight, virtualTop, rightBlinderWidth, virtualHeight);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int LOGPIXELSX = 88;

        private static double GetDpiScale()
        {
            // Get system DPI using Windows API (works without a window)
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc != IntPtr.Zero)
            {
                try
                {
                    int dpi = GetDeviceCaps(hdc, LOGPIXELSX);
                    return dpi / 96.0;
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }
            return 1.0;
        }

        public void Toggle()
        {
            if (_isActive)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (_isActive) return;

            // Show blinder windows
            _leftBlinder?.Show();
            _rightBlinder?.Show();

            // Confine mouse if enabled
            if (Config.Instance.ConfineMouse)
            {
                ConfineMouse(true);
            }

            _isActive = true;
            StateChanged?.Invoke(this, true);
        }

        public void Deactivate()
        {
            if (!_isActive) return;

            // Hide blinder windows
            _leftBlinder?.Hide();
            _rightBlinder?.Hide();

            // Release mouse confinement
            ConfineMouse(false);

            _isActive = false;
            StateChanged?.Invoke(this, false);
        }

        private void ConfineMouse(bool confine)
        {
            if (confine)
            {
                var rect = new RECT
                {
                    Left = _viewportLeft,
                    Top = _viewportTop,
                    Right = _viewportRight,
                    Bottom = _viewportBottom
                };
                ClipCursor(ref rect);
            }
            else
            {
                // Pass null to release the cursor
                ClipCursor(IntPtr.Zero);
            }
        }

        public void RefreshConfig()
        {
            bool wasActive = _isActive;

            if (wasActive)
            {
                Deactivate();
            }

            // Recreate windows with new config
            _leftBlinder?.Close();
            _rightBlinder?.Close();

            Config.Reload();
            CreateBlinderWindows();

            if (wasActive)
            {
                Activate();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            Deactivate();
            _leftBlinder?.Close();
            _rightBlinder?.Close();
            _disposed = true;
        }
    }
}
