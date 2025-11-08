using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;

namespace KeepAliveApp;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    this.Title = "Bluetooth Audio KeepAlive";
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(View_TitleBar);
    //AppWindow.SetTitleBarIcon("Assets/BluetoothAudioKeepAlive_24.ico");

    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = 600;
    presenter?.PreferredMinimumHeight = 300;

    AppWindow.ResizeClient(new(600, 800));
  }

  private async void View_CaptureButton_Click(object sender, RoutedEventArgs e)
  {
    await Task.Delay(1000);
    await Debugging.Capture.CaptureHighResWindowAsync(this.Content, "capture.png");
  }
}
