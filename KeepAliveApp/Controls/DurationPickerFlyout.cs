namespace KeepAliveApp.Controls;

public class DurationPickerFlyout(DurationPickerEventHelper? DurationPickerEventHelper, TimeSpan DefaultDuration = default) : Flyout
{
  protected override Control CreatePresenter()
  {
    Presenter.Closed += (s, e) => this.Hide();
    return Presenter;
  }

  public readonly DurationPickerFlyoutPresenter Presenter = new(DurationPickerEventHelper, DefaultDuration);
}

