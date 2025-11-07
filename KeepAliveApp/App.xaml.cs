using Windows.Storage;

namespace KeepAliveApp;

public partial class App : Application
{
  private Window? _window;
  private StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

  public App()
  {
    InitializeComponent();

    this.UnhandledException += App_UnhandledException;
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new MainWindow();
    _window.Activate();
  }

  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
  {
    string _logPath = Path.Combine(_localFolder.Path, "AppCrashLog.txt");
    try
    {
      var text = 
        $"""
        [{DateTime.Now}]
        {e.Exception}


        """;
      File.AppendAllText(_logPath, text);
    }
    catch { }
  }
}
