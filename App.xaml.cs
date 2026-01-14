using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace StarCitizenBlinders
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;
        private BlinderManager? _blinderManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the blinder manager
            _blinderManager = new BlinderManager();

            // Create the system tray icon programmatically
            _notifyIcon = new TaskbarIcon
            {
                Icon = CreateIcon(),
                ToolTipText = "DIMS Blinders"
            };

            // Create context menu
            var contextMenu = new ContextMenu();

            var toggleItem = new MenuItem { Header = "Toggle Blinders", FontWeight = FontWeights.Bold };
            toggleItem.Click += (s, args) => _blinderManager.Toggle();
            contextMenu.Items.Add(toggleItem);

            contextMenu.Items.Add(new Separator());

            var settingsItem = new MenuItem { Header = "Settings..." };
            settingsItem.Click += (s, args) => new SettingsWindow(_blinderManager).ShowDialog();
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new Separator());

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (s, args) => Shutdown();
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenu = contextMenu;
            _notifyIcon.TrayMouseDoubleClick += (s, args) => _blinderManager.Toggle();

            // Update tooltip when state changes
            _blinderManager.StateChanged += (s, active) =>
            {
                _notifyIcon.ToolTipText = active
                    ? "DIMS Blinders - ACTIVE (Single Monitor)"
                    : "DIMS Blinders - Inactive (Triple Monitor)";
            };

            // Register global hotkey
            HotkeyManager.Instance.Initialize(_blinderManager);

            // Show initial notification
            _notifyIcon.ShowBalloonTip(
                "DIMS Blinders",
                $"Running in background. Press {Config.Instance.HotkeyDisplay} to toggle blinders.",
                BalloonIcon.Info);
        }

        private static Icon CreateIcon()
        {
            // Create a simple icon programmatically
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);

                // Draw three monitors representation
                int monitorWidth = 8;
                int monitorHeight = 20;
                int gap = 2;
                int startX = (32 - (3 * monitorWidth + 2 * gap)) / 2;
                int startY = (32 - monitorHeight) / 2;

                // Left monitor (dark)
                g.FillRectangle(Brushes.DarkGray, startX, startY, monitorWidth, monitorHeight);

                // Center monitor (bright - active)
                g.FillRectangle(Brushes.Cyan, startX + monitorWidth + gap, startY, monitorWidth, monitorHeight);

                // Right monitor (dark)
                g.FillRectangle(Brushes.DarkGray, startX + 2 * (monitorWidth + gap), startY, monitorWidth, monitorHeight);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _blinderManager?.Dispose();
            HotkeyManager.Instance.Dispose();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
