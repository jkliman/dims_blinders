using System;
using System.Windows;
using System.Windows.Input;

namespace StarCitizenBlinders
{
    public class NotifyIconViewModel
    {
        private readonly BlinderManager _blinderManager;

        public NotifyIconViewModel(BlinderManager blinderManager)
        {
            _blinderManager = blinderManager;
        }

        public string ToolTipText => _blinderManager.IsActive
            ? "Star Citizen Blinders - ACTIVE (Single Monitor Mode)"
            : "Star Citizen Blinders - Inactive (Triple Monitor Mode)";

        public ICommand ToggleCommand => new RelayCommand(_ =>
        {
            _blinderManager.Toggle();
        });

        public ICommand SettingsCommand => new RelayCommand(_ =>
        {
            var settingsWindow = new SettingsWindow(_blinderManager);
            settingsWindow.ShowDialog();
        });

        public ICommand ExitCommand => new RelayCommand(_ =>
        {
            Application.Current.Shutdown();
        });
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
