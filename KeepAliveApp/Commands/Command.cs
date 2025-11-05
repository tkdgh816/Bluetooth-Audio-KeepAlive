using System.Windows.Input;

namespace KeepAliveApp.Commands;

public class Command : ICommand
{
  public Action? ActionToExecute { get; init; }
  public Func<bool>? CanExecuteFunc { get; init; }

  public Action<object>? ActionsToExecuteWithParam { get; init; }
  public Func<object, bool>? CanExecuteFuncWithParam { get; init; }

  public Command() { }

  public Command(Action actionToExecute, Func<bool>? canExecuteFunc = null)
  {
    ActionToExecute = actionToExecute;
    CanExecuteFunc = canExecuteFunc;
  }

  public Command(Action<object> actionToExecuteWithParam, Func<object, bool>? canExecuteFuncWithParam = null)
  {
    ActionsToExecuteWithParam = actionToExecuteWithParam;
    CanExecuteFuncWithParam = canExecuteFuncWithParam;
  }

  public event EventHandler? CanExecuteChanged;

  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter = null)
    => parameter is null
      ? CanExecuteFunc is null || CanExecuteFunc()
      : CanExecuteFuncWithParam is null || CanExecuteFuncWithParam(parameter);

  public void Execute(object? parameter = null)
  {
    if (!CanExecute(parameter))
      return;

    if (parameter is null)
    {
      if (ActionToExecute is not null)
        ActionToExecute();
    }
    else
    {
      if (ActionsToExecuteWithParam is not null)
        ActionsToExecuteWithParam(parameter);
    }
  }
}

public class Command<T> : ICommand
{
  public Action<T>? ActionToExecute { get; init; }
  public Func<T?, bool>? CanExecuteFunc { get; init; }

  public Command() { }

  public Command(Action<T> actionToExecute, Func<T?, bool>? canExecuteFunc = null)
  {
    ActionToExecute = actionToExecute;
    CanExecuteFunc = canExecuteFunc;
  }

  public event EventHandler? CanExecuteChanged;

  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter)
    => parameter is null || CanExecuteFunc is null || CanExecuteFunc((T)parameter);

  public void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
      return;

    if (ActionToExecute is not null && parameter is not null)
      ActionToExecute((T)parameter);
  }
}
