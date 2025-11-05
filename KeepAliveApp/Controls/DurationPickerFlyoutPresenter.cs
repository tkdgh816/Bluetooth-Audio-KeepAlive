using Microsoft.UI.Xaml.Media.Animation;

namespace KeepAliveApp.Controls;

public sealed partial class DurationPickerFlyoutPresenter : FlyoutPresenter
{
  public DurationPickerFlyoutPresenter(DurationPickerEventHelper? durationPickerEventHelper, TimeSpan DefaultDuration = default)
  {
    DefaultStyleKey = typeof(DurationPickerFlyoutPresenter);
    DurationPickerEventHelper = durationPickerEventHelper;

    SetDurationWithoutEventInvoking(DefaultDuration);

    DurationPickerEventHelper?.PickerDurationChanged += (s, e) =>
    {
      SetDurationWithoutEventInvoking(e.NewValue);
      SetClock();
    };

    this.Loaded += (s, e) => VisualStateManager.GoToState(this, "FlyoutOpen", false);
    this.Closed += (s, e) => VisualStateManager.GoToState(this, "FlyoutClosed", false);
  }

  private readonly DurationPickerEventHelper? DurationPickerEventHelper;

  public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(DurationPickerFlyoutPresenter), new PropertyMetadata(TimeSpan.Zero, OnDurationChanged));
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
      var presenter = (DurationPickerFlyoutPresenter)d;
      if (!presenter.SuppressDurationChangedBubbling)
        presenter.DurationPickerEventHelper?.OnPresenterDurationChanged(presenter, new DurationChangedEventArgs((TimeSpan)e.OldValue, (TimeSpan)e.NewValue));
    }
  }

  private static readonly double _interval = 40.0;

  public event EventHandler<object, EventArgs>? Closed;

  private Grid PickerHostGrid = null!;

  private Grid FirstPickerHost = null!;
  private Grid SecondPickerHost = null!;
  private Grid ThirdPickerHost = null!;

  private ScrollPresenter FirstPickerHostScrollPresenter = null!;
  private ScrollPresenter SecondPickerHostScrollPresenter = null!;
  private ScrollPresenter ThirdPickerHostScrollPresenter = null!;

  private ItemsRepeater FirstPickerHostItemsRepeater = null!;
  private ItemsRepeater SecondPickerHostItemsRepeater = null!;
  private ItemsRepeater ThirdPickerHostItemsRepeater = null!;

  private RepeatButton FirstPickerHostUpButton = null!;
  private RepeatButton FirstPickerHostDownButton = null!;
  private RepeatButton SecondPickerHostUpButton = null!;
  private RepeatButton SecondPickerHostDownButton = null!;
  private RepeatButton ThirdPickerHostUpButton = null!;
  private RepeatButton ThirdPickerHostDownButton = null!;

  private Button AcceptButton = null!;
  private Button DismissButton = null!;

  private Storyboard FlyoutOpenStoryBoard = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    PickerHostGrid = (Grid)GetTemplateChild("PickerHostGrid");

    FirstPickerHost = (Grid)GetTemplateChild("FirstPickerHost");
    FirstPickerHost.PointerEntered += (s, e) => VisualStateManager.GoToState(this, "FirstHostPointerOver", false);
    FirstPickerHost.PointerExited += PickerHost_PointerExited;

    FirstPickerHostScrollPresenter = (ScrollPresenter)GetTemplateChild("FirstPickerHostScrollPresenter");
    FirstPickerHostScrollPresenter.VerticalSnapPoints.Add(new RepeatedScrollSnapPoint(0.0, _interval, 0.0, 1040.0, ScrollSnapPointsAlignment.Near));
    FirstPickerHostScrollPresenter.ViewChanged += CreateViewChangedEventHandler(_interval, 0.0, 1000.0);
    FirstPickerHostScrollPresenter.ViewChanged += (s, e) => Hour = Math.Max((int)Math.Round(s.VerticalOffset / _interval) - 1, 0) % 24;

    FirstPickerHostItemsRepeater = (ItemsRepeater)GetTemplateChild("FirstPickerHostItemsRepeater");
    FirstPickerHostItemsRepeater.ItemsSource = CreateNumbers(24, 5);
    FirstPickerHostItemsRepeater.ElementPrepared += CreateElementPreparedEventHandler(FirstPickerHostScrollPresenter);

    FirstPickerHostUpButton = (RepeatButton)GetTemplateChild("FirstPickerHostUpButton");
    FirstPickerHostDownButton = (RepeatButton)GetTemplateChild("FirstPickerHostDownButton");
    FirstPickerHostUpButton.Click += (s, e) => FirstPickerHostScrollPresenter.ScrollBy(0, -_interval);
    FirstPickerHostDownButton.Click += (s, e) => FirstPickerHostScrollPresenter.ScrollBy(0, _interval);

    SecondPickerHost = (Grid)GetTemplateChild("SecondPickerHost");
    SecondPickerHost.PointerEntered += (s, e) => VisualStateManager.GoToState(this, "SecondHostPointerOver", false);
    SecondPickerHost.PointerExited += PickerHost_PointerExited;

    SecondPickerHostScrollPresenter = (ScrollPresenter)GetTemplateChild("SecondPickerHostScrollPresenter");
    SecondPickerHostScrollPresenter.VerticalSnapPoints.Add(new RepeatedScrollSnapPoint(0.0, _interval, 0.0, 2480.0, ScrollSnapPointsAlignment.Near));
    SecondPickerHostScrollPresenter.ViewChanged += CreateViewChangedEventHandler(_interval, 0.0, 2440.0);
    SecondPickerHostScrollPresenter.ViewChanged += (s, e) => Minute = Math.Max((int)Math.Round(s.VerticalOffset / _interval) - 1, 0) % 60;

    SecondPickerHostItemsRepeater = (ItemsRepeater)GetTemplateChild("SecondPickerHostItemsRepeater");
    SecondPickerHostItemsRepeater.ItemsSource = CreateNumbers(60, 5);
    SecondPickerHostItemsRepeater.ElementPrepared += CreateElementPreparedEventHandler(SecondPickerHostScrollPresenter);

    SecondPickerHostUpButton = (RepeatButton)GetTemplateChild("SecondPickerHostUpButton");
    SecondPickerHostDownButton = (RepeatButton)GetTemplateChild("SecondPickerHostDownButton");
    SecondPickerHostUpButton.Click += (s, e) => SecondPickerHostScrollPresenter.ScrollBy(0, -_interval);
    SecondPickerHostDownButton.Click += (s, e) => SecondPickerHostScrollPresenter.ScrollBy(0, _interval);

    ThirdPickerHost = (Grid)GetTemplateChild("ThirdPickerHost");
    ThirdPickerHost.PointerEntered += (s, e) => VisualStateManager.GoToState(this, "ThirdHostPointerOver", false);
    ThirdPickerHost.PointerExited += PickerHost_PointerExited;

    ThirdPickerHostScrollPresenter = (ScrollPresenter)GetTemplateChild("ThirdPickerHostScrollPresenter");
    ThirdPickerHostScrollPresenter.VerticalSnapPoints.Add(new RepeatedScrollSnapPoint(0.0, _interval, 0.0, 2480.0, ScrollSnapPointsAlignment.Near));
    ThirdPickerHostScrollPresenter.ViewChanged += CreateViewChangedEventHandler(_interval, 0.0, 2440.0);
    ThirdPickerHostScrollPresenter.ViewChanged += (s, e) => Second = Math.Max((int)Math.Round(s.VerticalOffset / _interval) - 1, 0) % 60;

    ThirdPickerHostItemsRepeater = (ItemsRepeater)GetTemplateChild("ThirdPickerHostItemsRepeater");
    ThirdPickerHostItemsRepeater.ItemsSource = CreateNumbers(60, 5);
    ThirdPickerHostItemsRepeater.ElementPrepared += CreateElementPreparedEventHandler(ThirdPickerHostScrollPresenter);

    ThirdPickerHostUpButton = (RepeatButton)GetTemplateChild("ThirdPickerHostUpButton");
    ThirdPickerHostDownButton = (RepeatButton)GetTemplateChild("ThirdPickerHostDownButton");
    ThirdPickerHostUpButton.Click += (s, e) => ThirdPickerHostScrollPresenter.ScrollBy(0, -_interval);
    ThirdPickerHostDownButton.Click += (s, e) => ThirdPickerHostScrollPresenter.ScrollBy(0, _interval);

    FlyoutOpenStoryBoard = (Storyboard)GetTemplateChild("FlyoutOpenStoryBoard");
    FlyoutOpenStoryBoard.Completed += (s, e) =>
    {
      SetClock();
      PickerHostGrid.Opacity = 1.0;
    };

    AcceptButton = (Button)GetTemplateChild("AcceptButton");
    AcceptButton.Click += (s, e) =>
    {
      SetDurationFromClock();
      Closed?.Invoke(this, EventArgs.Empty);
    };
    DismissButton = (Button)GetTemplateChild("DismissButton");
    DismissButton.Click += (s, e) =>
    {
      Closed?.Invoke(this, EventArgs.Empty);
    };
  }

  private TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> CreateElementPreparedEventHandler(ScrollPresenter presenter) =>
    (sender, args) =>
    {
      var btn = (Button)VisualTreeHelper.GetChild((Grid)args.Element, 0);
      btn.Click += (s, e) => presenter.ScrollTo(0, ((int)btn.Content + 1) * _interval);
    };

  private void SetDurationFromClock()
  {
    var ts = TimeSpan.Zero;
    ts = ts.Add(TimeSpan.FromHours(Hour));
    ts = ts.Add(TimeSpan.FromMinutes(Minute));
    ts = ts.Add(TimeSpan.FromSeconds(Second));
    Duration = ts;
  }

  private void SetDurationWithoutEventInvoking(TimeSpan duration)
  {
    SuppressDurationChangedBubbling = true;
    Duration = duration;
    SuppressDurationChangedBubbling = false;
  }

  private void SetClock()
  {
    ScrollingScrollOptions scrollOptions = new(ScrollingAnimationMode.Disabled);
    FirstPickerHostScrollPresenter.ScrollTo(0, (Duration.Hours + 1) * _interval, scrollOptions);
    SecondPickerHostScrollPresenter.ScrollTo(0, (Duration.Minutes + 1) * _interval, scrollOptions);
    ThirdPickerHostScrollPresenter.ScrollTo(0, (Duration.Seconds + 1) * _interval, scrollOptions);
  }

  private void PickerHost_PointerExited(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "HostPointerOverNone", false);

  private TypedEventHandler<ScrollPresenter, object> CreateViewChangedEventHandler(double interval, double startOffset, double endOffset) =>
    (sender, args) =>
    {
      double offset = sender.VerticalOffset;
      if (offset <= startOffset)
        sender.ScrollTo(0, endOffset - interval, new ScrollingScrollOptions(ScrollingAnimationMode.Disabled));
      if (offset >= endOffset)
        sender.ScrollTo(0, startOffset + interval, new ScrollingScrollOptions(ScrollingAnimationMode.Disabled));
    };

  private int Hour = 0;
  private int Minute = 0;
  private int Second = 0;

  private static List<int> CreateNumbers(int count, int padding) =>
    [
      ..Enumerable.Range(count - padding, padding),
      ..Enumerable.Range(0, count),
      ..Enumerable.Range(0, padding),
    ];
}
