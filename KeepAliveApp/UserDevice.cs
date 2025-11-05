using CommunityToolkit.Mvvm.ComponentModel;

using KeepAliveSettings;

using Windows.Devices.Bluetooth;

namespace KeepAliveApp;

public class UserDevice : ObservableObject, IDisposable
{
  public BluetoothDevice BluetoothDevice { get; private set; } = null!;
  public required DispatcherQueue DispatcherQueue { get; init; }
  public required Settings Settings { get; init; }

  public UserDevice(BluetoothDevice bluetoothDevice)
  {
    SetDevice(bluetoothDevice);
  }

  public void SetDevice(BluetoothDevice bluetoothDevice)
  {
    BluetoothDevice?.ConnectionStatusChanged -= Device_ConnectionStatusChanged;

    BluetoothDevice = bluetoothDevice;

    IsConnected = BluetoothDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
    BluetoothDevice.ConnectionStatusChanged += Device_ConnectionStatusChanged;
  }

  private void Device_ConnectionStatusChanged(BluetoothDevice sender, object args)
  {
    DispatcherQueue.TryEnqueue(() => IsConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected);
  }

  public bool IsConnected
  {
    get;
    set => SetProperty(ref field, value);
  }

  public bool IsEditable
  {
    get;
    set => SetProperty(ref field, value);
  }

  public bool IsEnabled
  {
    get;
    set => SetProperty(ref field, value);
  }

  public bool IsSelected
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      _ = ChangeSelectionState(value);
    }
  }

  private async Task ChangeSelectionState(bool state)
  {
    var selectedDevices = Settings.SelectedDevices;
    var id = BluetoothDevice.DeviceId;
    if (state)
    {
      if (!selectedDevices.Contains(id))
        Settings.SelectedDevices.Add(id);
    }
    else
      Settings.SelectedDevices.Remove(id);
  }

  private bool _disposed;

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        BluetoothDevice?.ConnectionStatusChanged -= Device_ConnectionStatusChanged;
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