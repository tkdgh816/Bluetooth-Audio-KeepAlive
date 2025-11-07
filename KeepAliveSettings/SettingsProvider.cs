using System.Text.Json;
using System.Text.Json.Serialization;

using Windows.ApplicationModel;

namespace KeepAliveSettings;

public static class SettingsProvider
{
  private static readonly string settingsFilePath = Path.Combine(AppContext.BaseDirectory, "..", "settings.json");
  private static readonly Mutex mutex = new(false, """Local\BlutoothAudioKeepAlive.MUTEX.Settings""");

  static SettingsProvider()
  {
    AppDomain.CurrentDomain.ProcessExit += (s, e) => mutex.Dispose();
  }

  private static readonly SettingsJsonContext jsonContext = new(new JsonSerializerOptions(){ WriteIndented = true });
  public static bool Save(Settings settings)
  {
    try
    {
      mutex.WaitOne();
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
      mutex.ReleaseMutex();
    }
  }

  public static Settings Load()
  {
    Settings? settings = null;
    try
    {
      mutex.WaitOne();
      if (File.Exists(settingsFilePath))
      {
        string jsonString = File.ReadAllText(settingsFilePath);
        settings = JsonSerializer.Deserialize(jsonString, jsonContext.Settings);
      }
    }
    finally
    {
      mutex.ReleaseMutex();
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