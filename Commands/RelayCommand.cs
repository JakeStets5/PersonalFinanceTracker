using System;
using System.Windows.Input;

namespace PersonalFinanceTracker.Commands
{
    // RelayCommand allows the ViewModel to handle UI commands without needing code in the code-behind.
    // It provides a way to execute actions and control whether they are enabled, supporting the MVVM pattern.
    public class RelayCommand : ICommand
    {
        // Stores the action to execute when the command is triggered.
        private readonly Action _execute;
        // Optional condition to determine whether the command can execute.
        private readonly Func<bool>? _canExecute;

        // Event to notify the UI to re-check whether the command can execute, enabling/disabling buttons dynamically.
        public event EventHandler? CanExecuteChanged;

        // Constructor assigns the action and optional condition for execution.
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            // Ensures the execute action is valid and prevents errors if null is passed.
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determines if the command can execute by evaluating the condition or defaulting to true.
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // Executes the action associated with the command.
        public void Execute(object? parameter) => _execute();

        // Triggers UI updates to re-check the command's availability.
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Generic version of RelayCommand supports commands that require parameters.
    public class RelayCommand<T> : ICommand
    {
        // Stores the action with a parameter.
        private readonly Action<T> _execute;
        // Optional condition to check executability based on the parameter.
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        // Constructor assigns the action and optional condition for execution.
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determines if the command can execute based on the parameter and condition.
        public bool CanExecute(object? parameter) =>
            parameter is T value && (_canExecute?.Invoke(value) ?? true);

        // Executes the action using the provided parameter.
        public void Execute(object? parameter)
        {
            if (parameter is T value) _execute(value);
        }

        // Triggers UI updates to re-check the command's availability.
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
