using System.IO.MemoryMappedFiles;

using Interop;

using KeepAliveSettings;
namespace KeepAliveService;

internal class Program
{
  private static readonly MemoryMappedFile mmf_ServiceProcessId = MemoryMappedFile.CreateOrOpen($"""Local\BluetoothAudioKeepAlive.MMF.ServiceProcessId""", 4);
  private static readonly Mutex mutex_ServiceProcessId = new(false, $"""Local\BluetoothAudioKeepAlive.MUTEX.ServiceProcessId""");

  private static void Main(string[] args)
  {
    try
    {
      mutex_ServiceProcessId.WaitOne();
      using (var accessor = mmf_ServiceProcessId.CreateViewAccessor())
        accessor.Write(0, Environment.ProcessId);
    }
    finally
    {
      mutex_ServiceProcessId.ReleaseMutex();
    }

    _ = new KeepAlivePlayer(SettingsProvider.Load());
    _ = WaitForQuitMessage(NativeMethods.GetCurrentThreadId());

    while (NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0))
    {

    }

    Dispose();
  }

  private static readonly EventWaitHandle ewh_QuitServiceProcess = new(false, EventResetMode.AutoReset, $"""Local\BluetoothAudioKeepAlive.EWH.QuitServiceProcess""");

  private static Task WaitForQuitMessage(uint mainThreadId) =>
    Task.Run(() =>
    {
      ewh_QuitServiceProcess.WaitOne();
      NativeMethods.PostThreadMessage(mainThreadId, (uint)NativeMethods.WindowMessage.WM_QUIT, 0, 0);
    });

  private static void Dispose()
  {
    try
    {
      mutex_ServiceProcessId.WaitOne();
      using (var accessor = mmf_ServiceProcessId.CreateViewAccessor())
        accessor.Write(0, 0);
    }
    finally
    {
      mutex_ServiceProcessId.ReleaseMutex();
    }

    // Dispose EventWaitHandle
    ewh_QuitServiceProcess.Dispose();

    // Dispose MemoryMappedFile
    mmf_ServiceProcessId.Dispose();

    // Dispose Mutex
    mutex_ServiceProcessId.Dispose();
  }
}
