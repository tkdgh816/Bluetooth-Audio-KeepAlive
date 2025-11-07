using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Windows.ApplicationModel;

namespace KeepAliveSettings;

public static class SettingsProvider
{
  private const string PFN = "ZeroFinchNeil.BluetoothAudioKeepAlive_trdr6c7cjqx0g";
  private static readonly MemoryMappedFile mmf_LocalFolderPath = MemoryMappedFile.CreateOrOpen($"""Local\BluetoothAudioKeepAlive.MMF.LocalFolderPath""", 1048);
  private static readonly Mutex mutex_LocalFolderPath = new(false, $"""Local\BluetoothAudioKeepAlive.MUTEX.LocalFolderPath""");

  private static readonly string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Packages", PFN, "LocalState", "settings.json");

  static SettingsProvider()
  {
    try
    {
      mutex_LocalFolderPath.WaitOne();
      using (var accessor = mmf_LocalFolderPath.CreateViewAccessor())
      {
        int length = accessor.ReadInt32(0);
        if (length > 0)
        {
          byte[] byteBuffer = new byte[length];
          accessor.ReadArray(4, byteBuffer, 0, length);
          settingsFilePath = Path.Combine(Encoding.UTF8.GetString(byteBuffer), "settings.json");
          Debug.WriteLine(settingsFilePath);
        }
      }
    }
    finally
    {
      mutex_LocalFolderPath.ReleaseMutex();
    }

    AppDomain.CurrentDomain.ProcessExit += (s, e) =>
    {
      mmf_LocalFolderPath.Dispose();
      mutex_LocalFolderPath.Dispose();
    };
  }

  private static readonly SettingsJsonContext jsonContext = new(new JsonSerializerOptions() { WriteIndented = true });
  public static bool Save(Settings settings)
  {
    try
    {
      mutex_LocalFolderPath.WaitOne();
      string jsonString = JsonSerializer.Serialize(settings, jsonContext.Settings);
      File.WriteAllText(settingsFilePath, jsonString);
      return true;
    }
    catch
    {
      return false;
    }
    finally
    {
      mutex_LocalFolderPath.ReleaseMutex();
    }
  }

  public static Settings Load()
  {
    Settings? settings = null;
    try
    {
      mutex_LocalFolderPath.WaitOne();
      if (File.Exists(settingsFilePath))
      {
        string jsonString = File.ReadAllText(settingsFilePath);
        settings = JsonSerializer.Deserialize(jsonString, jsonContext.Settings);
      }
    }
    catch { }
    finally
    {
      mutex_LocalFolderPath.ReleaseMutex();
    }

    return settings ?? new();
  }

  // StartupTask
  public static async Task<bool> GetStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync("StartupTaskId");
    return startupTask.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
  }

  public static async Task<bool> ToggleStartupTaskState()
  {
    StartupTask startupTask = await StartupTask.GetAsync("StartupTaskId");
    switch (startupTask.State)
    {
      case StartupTaskState.Enabled:
        startupTask.Disable();
        return false;
      case StartupTaskState.EnabledByPolicy:
        return true;
      default:
        await startupTask.RequestEnableAsync();
        return await GetStartupTaskState();
    }
  }
}

[JsonSerializable(typeof(Settings))]
internal partial class SettingsJsonContext : JsonSerializerContext { }