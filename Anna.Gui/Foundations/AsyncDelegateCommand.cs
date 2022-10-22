using System;
using System.Threading.Tasks;
using System.Windows.Input;

#pragma warning disable 0067

namespace Anna.Gui.Foundations;

public sealed class AsyncDelegateCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncDelegateCommand(Func<Task> execute)
    {
        _execute = execute;
    }

    public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    bool ICommand.CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    void ICommand.Execute(object? parameter) => _execute.Invoke();
}

public sealed class AsyncDelegateCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public AsyncDelegateCommand(Func<T?, Task> execute)
    {
        _execute = execute;
    }

    public AsyncDelegateCommand(Func<T?, Task> execute, Func<T?, bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    bool ICommand.CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    void ICommand.Execute(object? parameter) => _execute.Invoke((T?)parameter);
}