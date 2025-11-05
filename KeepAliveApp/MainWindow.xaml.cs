using Microsoft.UI.Windowing;

namespace KeepAliveApp;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(View_TitleBar);

    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = 600;
    presenter?.PreferredMinimumHeight = 300;

    //AppWindow.ResizeClient(new(600, 800));
  }
}
