using System;
using System.Windows.Input;

#pragma warning disable 0067

namespace Anna.Gui.Foundations
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        bool ICommand.CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        void ICommand.Execute(object? parameter) => _execute.Invoke();
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public DelegateCommand(Action<T?> execute)
        {
            _execute = execute;
        }

        public DelegateCommand(Action<T?> execute, Func<T?, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        bool ICommand.CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        void ICommand.Execute(object? parameter) => _execute.Invoke((T?)parameter);
    }
}