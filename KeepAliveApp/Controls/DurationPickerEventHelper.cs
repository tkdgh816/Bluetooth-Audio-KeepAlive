namespace KeepAliveApp.Controls;

public class DurationPickerEventHelper : IDisposable
{
  private bool _disposed;

  public event TypedEventHandler<DurationPicker, DurationChangedEventArgs>? PickerDurationChanged;
  public event TypedEventHandler<DurationPickerFlyoutPresenter, DurationChangedEventArgs>? PresenterDurationChanged;

  public void OnPickerDurationChanged(DurationPicker sender, DurationChangedEventArgs args) => PickerDurationChanged?.Invoke(sender, args);
  public void OnPresenterDurationChanged(DurationPickerFlyoutPresenter sender, DurationChangedEventArgs args) => PresenterDurationChanged?.Invoke(sender, args);

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        PickerDurationChanged = null;
        PresenterDurationChanged = null;
      }

      _disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}

public class DurationChangedEventArgs(TimeSpan oldValue, TimeSpan newValue) : EventArgs
{
  public TimeSpan OldValue => oldValue;
  public TimeSpan NewValue => newValue;
}
