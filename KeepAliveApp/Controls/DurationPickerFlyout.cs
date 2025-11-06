namespace KeepAliveApp.Controls;

public class DurationPickerFlyout(DurationPickerEventHelper? DurationPickerEventHelper, TimeSpan DefaultDuration = default) : Flyout
{
  protected override Control CreatePresenter()
  {
    this.Closed += (s, e) => Presenter.Close();
    Presenter.Closed += (s, e) => this.Hide();
    return Presenter;
  }

  public readonly DurationPickerFlyoutPresenter Presenter = new(DurationPickerEventHelper, DefaultDuration);
}

