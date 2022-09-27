using System;
using System.Windows.Input;

namespace RayTracingInOneWeekend.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _action;
    private readonly Func<object?, bool>? _canExecuteChangedFunc;

    public RelayCommand(Action<object?> action, Func<object?, bool>? canExecuteChangedFunc = null)
    {
        _action = action;
        _canExecuteChangedFunc = canExecuteChangedFunc;
    }
    
    public bool CanExecute(object? parameter)
    {
        return _canExecuteChangedFunc?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        _action.Invoke(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? CanExecuteChanged;
}