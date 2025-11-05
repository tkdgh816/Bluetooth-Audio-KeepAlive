namespace KeepAliveApp.Controls;

public sealed partial class DurationPicker : Control
{
  public DurationPicker()
  {
    DefaultStyleKey = typeof(DurationPicker);
    DurationPickerEventHelper.PresenterDurationChanged += (s, e) =>
    {
      SuppressDurationChangedBubbling = true;
      Duration = e.NewValue;
      SuppressDurationChangedBubbling = false;
    };
    this.Unloaded += DurationPicker_Unloaded;
  }

  private void DurationPicker_Unloaded(object sender, RoutedEventArgs e)
  {
    DurationPickerEventHelper = null;
  }

  private DurationPickerEventHelper? DurationPickerEventHelper = new();
  public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(DurationPicker), new PropertyMetadata(TimeSpan.Zero, OnDurationChanged));
  public TimeSpan Duration
  {
    get => (TimeSpan)GetValue(DurationProperty);
    set => SetValue(DurationProperty, value);
  }

  bool SuppressDurationChangedBubbling;
  private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (!e.OldValue.Equals(e.NewValue))
    {
      var picker = (DurationPicker)d;
      picker.DurationChanged?.Invoke(picker, new DurationChangedEventArgs((TimeSpan)e.OldValue, (TimeSpan)e.NewValue));
      if (!picker.SuppressDurationChangedBubbling)
        picker.DurationPickerEventHelper?.OnPickerDurationChanged(picker, new DurationChangedEventArgs((TimeSpan)e.OldValue, (TimeSpan)e.NewValue));
    }
  }

  public event TypedEventHandler<DurationPicker, DurationChangedEventArgs>? DurationChanged;

  Button FlyoutButton = null!;
  DurationPickerFlyout? PickerFlyout;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    FlyoutButton = (Button)GetTemplateChild("FlyoutButton");
    FlyoutButton.Click += FlyoutButton_Click;
  }

  private void FlyoutButton_Click(object sender, RoutedEventArgs e)
  {
    PickerFlyout ??= new(DurationPickerEventHelper, Duration)
    {
      ShouldConstrainToRootBounds = false,
      AreOpenCloseAnimationsEnabled = false,
      Placement = FlyoutPlacementMode.Top
    };

    var options = new FlyoutShowOptions
    {
      Position = new Point(121.0, 235.0),
      ExclusionRect = Rect.Empty
    };

    PickerFlyout.ShowAt(this, options);
  }
}