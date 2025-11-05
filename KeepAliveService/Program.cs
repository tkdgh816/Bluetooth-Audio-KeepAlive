using System.Diagnostics;
using System.IO.MemoryMappedFiles;

using Interop;
namespace KeepAliveService;

internal class Program
{
  private static readonly MemoryMappedFile mmf_ServiceProcessId = MemoryMappedFile.CreateOrOpen($"""Local\BluetoothAudioKeepAlive.MMF.ServiceProcessId""", 4);
  private static readonly Mutex mutex_ServiceProcessId = new(false, $"""Local\BluetoothAudioKeepAlive.MUTEX.ServiceProcessId""");

  static void Main(string[] args)
  {
    try
    {
      mutex_ServiceProcessId.WaitOne();
      using (var accessor = mmf_ServiceProcessId.CreateViewAccessor())
        accessor.Write(0, Process.GetCurrentProcess().Id);
    }
    finally
    {
      mutex_ServiceProcessId.ReleaseMutex();
    }

    while (NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0))
    {

    }
  }
}
