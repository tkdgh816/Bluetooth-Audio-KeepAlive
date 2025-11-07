using System;

using Microsoft.UI.Xaml.Controls;

namespace CustomControls;

public sealed partial class DurationPickerFlyout(DurationPickerEventHelper? DurationPickerEventHelper, TimeSpan DefaultDuration = default) : Flyout
{
  protected override Control CreatePresenter()
  {
    this.Closed += (s, e) => Presenter.Close();
    Presenter.Closed += (s, e) => this.Hide();
    return Presenter;
  }

  public readonly DurationPickerFlyoutPresenter Presenter = new(DurationPickerEventHelper, DefaultDuration);
}

