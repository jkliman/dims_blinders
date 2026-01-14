using System;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;

namespace StarCitizenBlinders
{
    public class Config
    {
        private static Config? _instance;
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DIMSBlinders",
            "config.json");

        public static Config Instance => _instance ??= Load();

        // Center viewport dimensions (the visible area when blinders are active)
        public int ViewportWidth { get; set; } = 1920;
        public int ViewportHeight { get; set; } = 1080;

        // Hotkey configuration
        public Key HotKey { get; set; } = Key.B;
        public ModifierKeys HotKeyModifiers { get; set; } = ModifierKeys.Control | ModifierKeys.Shift;

        // Blinder appearance
        public string BlinderColor { get; set; } = "#000000";
        public double BlinderOpacity { get; set; } = 1.0;

        // Mouse confinement
        public bool ConfineMouse { get; set; } = true;

        // Manual bounds override (use these instead of auto-calculation when set)
        public bool UseManualBounds { get; set; } = false;
        public int ManualLeft { get; set; } = 0;
        public int ManualTop { get; set; } = 0;
        public int ManualRight { get; set; } = 1920;
        public int ManualBottom { get; set; } = 1080;

        [JsonIgnore]
        public string HotkeyDisplay
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (HotKeyModifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
                if (HotKeyModifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
                if (HotKeyModifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
                if (HotKeyModifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
                parts.Add(HotKey.ToString());
                return string.Join("+", parts);
            }
        }

        public static Config Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
            }
            catch
            {
                // If load fails, return defaults
            }
            return new Config();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // Silently fail if we can't save
            }
        }

        public static void Reload()
        {
            _instance = Load();
        }
    }
}
