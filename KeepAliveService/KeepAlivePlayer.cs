using CommunityToolkit.Mvvm.ComponentModel;

using KeepAliveSettings;

using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace KeepAliveService;

public class KeepAlivePlayer : ObservableObject, IDisposable
{
  private Timer? _timer;
  public MediaPlayer? Player { get; private set; }
  private readonly MediaSource _mediaSource;
  private readonly TimeSpan _mediaDuration;
  public Settings Settings { get; }

  public KeepAlivePlayer(Settings settings)
  {
    Settings = settings;
    _mediaSource = MediaSource.CreateFromUri(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Media", $"{Settings.SoundKind}.wav"), UriKind.Absolute));
    _mediaDuration = GetAuidoDuration(Settings.SoundKind);
    if (Settings.ApplyToSelectedDevices)
      SelectDevices();
    else 
      Start();
  }

  private void Player_MediaEnded(MediaPlayer sender, object args) => DisposeTestSoundPlayer(sender);

  private void DisposeTestSoundPlayer(MediaPlayer player)
  {
    player.MediaEnded -= Player_MediaEnded;
    player.Dispose();
  }

  private Dictionary<BluetoothDevice, bool>? SelectedDevices;

  public bool IsAnySelectedDeviceConnected
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);

      if (value)
        Start();
      else
        Stop();
    }
  } = false;

  private async void SelectDevices()
  {
    SelectedDevices = new();
    foreach (var id in Settings.SelectedDevices)
    {
      var device = await BluetoothDevice.FromIdAsync(id);
      if (device is not null)
      {
        device.ConnectionStatusChanged += BtDevice_ConnectionStatusChanged;
        SelectedDevices.Add(device, device.ConnectionStatus == BluetoothConnectionStatus.Connected);
      }
    }

    CheckAnySelectedDeviceConnected();
  }

  private void BtDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
  {
    SelectedDevices?[sender] = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
    CheckAnySelectedDeviceConnected();
  }

  private void CheckAnySelectedDeviceConnected() => IsAnySelectedDeviceConnected = SelectedDevices?.Any(pair => pair.Value == true) ?? false;

  public void Start()
  {
    if (_timer is null)
    {
      _timer = new Timer((_) =>
      {
        Player = new MediaPlayer()
        {
          Source = _mediaSource,
          AudioCategory = MediaPlayerAudioCategory.Media,
          AutoPlay = false
        };
        Player.MediaEnded += Player_MediaEnded;
        Player.Play();
      }, null, TimeSpan.Zero, Settings.KeepAliveInterval.Add(_mediaDuration));
    }
    else
    {
      _timer.Change(TimeSpan.Zero, Settings.KeepAliveInterval.Add(_mediaDuration));
    }
  }

  public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

  private TimeSpan GetAuidoDuration(SoundKind kind) => kind switch
  {
    SoundKind.Silent1 => TimeSpan.FromSeconds(1),
    SoundKind.Silent2 => TimeSpan.FromSeconds(5),
    SoundKind.Beep1 => TimeSpan.FromMilliseconds(300),
    SoundKind.Beep2 => TimeSpan.FromMilliseconds(500),
    SoundKind.Beep3 => TimeSpan.FromMilliseconds(300),
    SoundKind.Beep4 => TimeSpan.FromMilliseconds(500),
    _ => TimeSpan.FromSeconds(1)
  };

  private bool disposedValue;

  protected virtual void Dispose(bool disposing)
  {
    if (!disposedValue)
    {
      if (disposing)
      {
        _timer?.Dispose();
        Player?.Dispose();
      }

      disposedValue = true;
      Player?.Dispose();
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}