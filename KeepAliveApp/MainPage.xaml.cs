using KeepAliveSettings;

namespace KeepAliveApp;

public sealed partial class MainPage : Page
{
  private DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
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
    this.Unloaded += MainPage_Unloaded;
  }

  private void MainPage_Unloaded(object sender, RoutedEventArgs e)
  {
    this.Bindings.StopTracking();
  }

  private bool _preventToggleChanging = false;

  private async void View_StartupTaskToggleSwitch_Toggled(object sender, RoutedEventArgs e)
  {
    if (_preventToggleChanging)
      return;

    View_StartupTaskInfoBar.Visibility = Visibility.Collapsed;
    bool changedState = await SettingsProvider.ToggleStartupTaskState();

    if (View_StartupTaskToggleSwitch.IsOn != changedState)
    {
      _preventToggleChanging = true;
      View_StartupTaskToggleSwitch.IsOn = changedState;
      View_StartupTaskInfoBar.Visibility = Visibility.Visible;
      _preventToggleChanging = false;
    }
  }

  private async void View_ViewStartupTaskSettingsButton_Click(object sender, RoutedEventArgs e)
  {
    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app?PFN=ZeroFinchNeil.BluetoothAudioKeepAlive_trdr6c7cjqx0g"));
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    Application.Current.Exit();
    //Program.QuitMainThread();
  }
}
