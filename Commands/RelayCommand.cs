using System;
using System.Windows.Input;

namespace PersonalFinanceTracker.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;                     // Action to execute
        private readonly Func<bool>? _canExecute;             // Predicate for enabling/disabling

        public event EventHandler? CanExecuteChanged;         // Raised when executability changes

        // Constructor without condition
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ICommand.CanExecute Implementation
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // ICommand.Execute Implementation
        public void Execute(object? parameter) => _execute();

        // Notify UI to re-check CanExecute
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Generic Version for Commands with Parameters
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            parameter is T value && (_canExecute?.Invoke(value) ?? true);

        public void Execute(object? parameter)
        {
            if (parameter is T value) _execute(value);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
