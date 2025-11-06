using Microsoft.UI.Windowing;

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
}
