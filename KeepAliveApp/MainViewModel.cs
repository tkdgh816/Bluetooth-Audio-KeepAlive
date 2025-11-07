using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using KeepAliveApp.Commands;

using KeepAliveSettings;

using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace KeepAliveApp;

public class MainViewModel : ObservableObject, IDisposable
{
  private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
  private readonly DeviceWatcher _btDeviceWatcher;

  public Settings Settings { get; } = Program.Settings;
  public ObservableCollection<UserDevice> UserDevices { get; } = new();

  public MainViewModel()
  {
    _btDeviceWatcher = DeviceInformation.CreateWatcher(BluetoothDevice.GetDeviceSelector());
    Initialize();
    SetCommands();
  }

  private void Initialize()
  {
    Settings.PropertyChanged += Settings_PropertyChanged;

    _btDeviceWatcher.Added += BTDevice_Added;
    _btDeviceWatcher.Removed += BTDevice_Removed;
    _btDeviceWatcher.Updated += BTDevice_Updated;
    _btDeviceWatcher.Start();
  }

  public bool CanChangeRunningState
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(Settings.IsKeepAliveServiceActive):
        if (Settings.IsKeepAliveServiceActive)
          Program.LaunchKeepAliveService();
        else
          Program.QuitKeepAliveService();
        break;
    }
  }

  private async void AddNewDevice(DeviceInformation deviceInformation)
  {
    UserDevice userDevice = new(await BluetoothDevice.FromIdAsync(deviceInformation.Id))
    {
      Settings = Settings,
      DispatcherQueue = _dispatcherQueue,
      IsEditable = !Settings.IsKeepAliveServiceActive,
      IsEnabled = Settings.ApplyToSelectedDevices,
      IsSelected = Settings.SelectedDevices.Any(id => id == deviceInformation.Id)
    };

    Settings.PropertyChanged += (s, e) =>
    {
      switch (e.PropertyName)
      {
        case nameof(Settings.ApplyToSelectedDevices):
          userDevice.IsEnabled = Settings.ApplyToSelectedDevices;
          break;
        case nameof(Settings.IsKeepAliveServiceActive):
          userDevice.IsEditable = !Settings.IsKeepAliveServiceActive;
          break;
      }
    };
    _dispatcherQueue.TryEnqueue(() => UserDevices.Add(userDevice));
  }

  private async void BTDevice_Added(DeviceWatcher sender, DeviceInformation args)
  {
    try
    {
      if (!UserDevices.Any(userDevice => userDevice.BluetoothDevice.DeviceId == args.Id))
        AddNewDevice(args);
    }
    catch { }
  }

  private async void BTDevice_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
  {
    try
    {
      foreach (var userDevice in UserDevices)
      {
        if (userDevice.BluetoothDevice.DeviceId == args.Id)
        {
          _dispatcherQueue.TryEnqueue(() => UserDevices.Remove(userDevice));
          userDevice.Dispose();
        }
      }
    }
    catch { }
  }

  private async void BTDevice_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
  {
    try
    {
      UserDevice? device = UserDevices.FirstOrDefault(userDevice => userDevice.BluetoothDevice.DeviceId == args.Id);
      device?.SetDevice(await BluetoothDevice.FromIdAsync(args.Id));
    }
    catch { }
  }

  public bool IsDeviceSelectionVisible(bool applyToSelectedDevices) => applyToSelectedDevices && UserDevices.Count > 0;
  public bool NegateBool(bool value) => !value;

  private MediaPlayer? _testSoundPlayer;
  private bool _isTestSoundPlaying = false;
  private void Player_MediaEnded(MediaPlayer sender, object args) => DisposeTestSoundPlayer(sender);

  private void DisposeTestSoundPlayer(MediaPlayer player)
  {
    _isTestSoundPlaying = false;
    PlayTestSoundButtonClickCommand?.RaiseCanExecuteChanged();
    StopTestSoundButtonClickCommand?.RaiseCanExecuteChanged();
    player.MediaEnded -= Player_MediaEnded;
    player.Dispose();
  }

  public Command<int>? PlayTestSoundButtonClickCommand { get; private set; }
  public Command? StopTestSoundButtonClickCommand { get; private set; }

  private void SetCommands()
  {
    PlayTestSoundButtonClickCommand = new()
    {
      ActionToExecute = (index) =>
      {
        if (index < 0 || index >= Enum.GetNames<SoundKind>().Length)
          return;

        _isTestSoundPlaying = true;
        PlayTestSoundButtonClickCommand?.RaiseCanExecuteChanged();
        StopTestSoundButtonClickCommand?.RaiseCanExecuteChanged();

        string fileName = Enum.GetName((SoundKind)index) ?? SoundKind.Silent1.ToString();

        _testSoundPlayer = new MediaPlayer()
        {
          Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Media/{fileName}.wav")),
          AudioCategory = MediaPlayerAudioCategory.Media,
          AutoPlay = false,
        };
        _testSoundPlayer.MediaEnded += Player_MediaEnded;
        _testSoundPlayer.Play();
      },
      CanExecuteFunc = (_) => !_isTestSoundPlaying
    };

    StopTestSoundButtonClickCommand = new()
    {
      ActionToExecute = () =>
      {
        if (_testSoundPlayer is not null)
        {
          _testSoundPlayer.Pause();
          DisposeTestSoundPlayer(_testSoundPlayer);
        }
      },
      CanExecuteFunc = () => _isTestSoundPlaying
    };
  }

  // IDisposable Implementation
  private bool _disposed;

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        _btDeviceWatcher.Stop();
        _btDeviceWatcher.Added -= BTDevice_Added;
        _btDeviceWatcher.Removed -= BTDevice_Removed;
        _btDeviceWatcher.Updated -= BTDevice_Updated;
      }

      _disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}