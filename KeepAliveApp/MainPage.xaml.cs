using KeepAliveSettings;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
namespace KeepAliveApp;

public sealed partial class MainPage : Page
{
  private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
  private readonly DispatcherTimer _startupTaskTimer = new() { Interval = TimeSpan.FromSeconds(2) };
  private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

  public MainViewModel ViewModel { get; } = new();

  public MainPage()
  {
    InitializeComponent();
    _dispatcherQueue.TryEnqueue(async () =>
    {
      _preventToggleChanging = true;
      View_StartupTaskToggleSwitch.IsOn = await SettingsProvider.GetStartupTaskState();
      _preventToggleChanging = false;
    });

    _startupTaskTimer.Tick += _startupTaskTimer_Tick;
    _startupTaskTimer.Start();
    this.Unloaded += MainPage_Unloaded;
  }

  private void MainPage_Unloaded(object sender, RoutedEventArgs e)
  {
    _startupTaskTimer.Stop();
    _startupTaskTimer.Tick -= _startupTaskTimer_Tick;
    this.Bindings.StopTracking();
  }

  private async void _startupTaskTimer_Tick(object? sender, object e)
  {
    _preventToggleChanging = true;

    bool state = await SettingsProvider.GetStartupTaskState();
    View_StartupTaskToggleSwitch.IsOn = state;

    if (state)
      VisualStateManager.GoToState(this, "View_StartupTaskNormalState", false);

    _preventToggleChanging = false;
  }

  private bool _preventToggleChanging = false;

  private async void View_StartupTaskToggleSwitch_Toggled(object sender, RoutedEventArgs e)
  {
    if (_preventToggleChanging)
      return;

    bool changedState = await SettingsProvider.ToggleStartupTaskState();

    if (View_StartupTaskToggleSwitch.IsOn == changedState)
      VisualStateManager.GoToState(this, "View_StartupTaskNormalState", false);
    else
    {
      _preventToggleChanging = true;
      View_StartupTaskToggleSwitch.IsOn = changedState;
      if (!changedState)
        VisualStateManager.GoToState(this, "View_StartupTaskWarningState", false);
      _preventToggleChanging = false;
    }
  }

  private readonly string _pfn = Package.Current.Id.FamilyName;
  private async void View_ViewStartupTaskSettingsButton_Click(object sender, RoutedEventArgs e)
  {
    await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-settings:appsfeatures-app?{Uri.EscapeDataString(_pfn)}"));
  }

  private async void View_QuitButton_Click(object sender, RoutedEventArgs e)
  {
    ContentDialog dialog = ViewModel.Settings.IsKeepAliveServiceRunning
      ? new()
      {
        XamlRoot = this.XamlRoot,
        Title = _resourceLoader.GetString("View_QuitOrCloseContentDialog_Title"),
        Content = _resourceLoader.GetString("View_QuitOrCloseContentDialog_Content"),
        PrimaryButtonText = _resourceLoader.GetString("View_QuitOrCloseContentDialog_PrimaryButtonText"),
        SecondaryButtonText = _resourceLoader.GetString("View_QuitOrCloseContentDialog_SecondaryButtonText"),
        CloseButtonText = _resourceLoader.GetString("View_QuitOrCloseContentDialog_CloseButtonText"),
        DefaultButton = ContentDialogButton.Close
      }
      : new()
      {
        XamlRoot = this.XamlRoot,
        Content= _resourceLoader.GetString("View_QuitContentDialog_Content"),
        Title = _resourceLoader.GetString("View_QuitContentDialog_Title"),
        PrimaryButtonText = _resourceLoader.GetString("View_QuitContentDialog_PrimaryButtonText"),
        CloseButtonText = _resourceLoader.GetString("View_QuitContentDialog_CloseButtonText"),
        DefaultButton = ContentDialogButton.Close
      };

    var result = await dialog.ShowAsync();
    switch (result)
    {
      case ContentDialogResult.Primary:
        Program.QuitKeepAliveService();
        Application.Current.Exit();
        break;
      case ContentDialogResult.Secondary:
        Application.Current.Exit();
        break;

    }
  }
}
