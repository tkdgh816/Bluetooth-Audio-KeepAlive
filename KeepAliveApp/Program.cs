using System.IO.MemoryMappedFiles;

using Interop;

using Microsoft.Windows.AppLifecycle;

namespace KeepAliveApp;

internal partial class Program
{
  private const string AppName = "BluetoothAudioKeepAlive";
  private static readonly string _appxInstalledPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

  private static readonly MemoryMappedFile mmf_ServiceProcessId = MemoryMappedFile.CreateOrOpen($"""Local\BluetoothAudioKeepAlive.MMF.ServiceProcessId""", 4);
  private static readonly Mutex mutex_ServiceProcessId = new(false, $"""Local\BluetoothAudioKeepAlive.MUTEX.ServiceProcessId""");

  [STAThread]
  private static int Main(string[] args)
  {
    WinRT.ComWrappersSupport.InitializeComWrappers();

    ExtendedActivationKind activationKind = AppInstance.GetCurrent().GetActivatedEventArgs().Kind;

    try
    {
      mutex_ServiceProcessId.WaitOne();
      int serviceProcessId;
      using (var accessor = mmf_ServiceProcessId.CreateViewAccessor())
        serviceProcessId = accessor.ReadInt32(0);

      if (serviceProcessId == 0)
      {
        ProcessStartInfo servicePSI = new()
        {
          FileName = Path.Combine(_appxInstalledPath, "KeepAliveService", "Bluetooth Audio KeepAlive.exe"),
          UseShellExecute = false,
        };
        Process.Start(servicePSI);
      }
    }
    finally
    {
      mutex_ServiceProcessId.ReleaseMutex();
    }

    if (!DecideRedirection() && activationKind == ExtendedActivationKind.Launch)
    {
      Application.Start((p) =>
      {
        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
        _ = new App();
      });
    }

    return 0;
  }

  private static bool DecideRedirection()
  {
    bool isRedirect = false;
    AppInstance keyInstance = AppInstance.FindOrRegisterForKey(AppName);

    if (keyInstance.IsCurrent)
    {
      keyInstance.Activated += OnActivated;
    }
    else
    {
      isRedirect = true;
      RedirectActivationTo(AppInstance.GetCurrent().GetActivatedEventArgs(), keyInstance);
    }

    return isRedirect;
  }

  private static void OnActivated(object? sender, AppActivationArguments args)
  {
    //ExtendedActivationKind kind = args.Kind;
  }

  public static IntPtr redirectEventHandle = IntPtr.Zero;

  // Do the redirection on another thread, and use a non-blocking
  // wait method to wait for the redirection to complete.
  public static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
  {
    redirectEventHandle = NativeMethods.CreateEvent(IntPtr.Zero, true, false, null);
    Task.Run(() =>
    {
      keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
      NativeMethods.SetEvent(redirectEventHandle);
    });

    uint CWMO_DEFAULT = 0;
    uint INFINITE = 0xFFFFFFFF;
    _ = NativeMethods.CoWaitForMultipleObjects(CWMO_DEFAULT, INFINITE, 1, [redirectEventHandle], out uint handleIndex);

    // Bring the window to the foreground
    Process process = Process.GetProcessById((int)keyInstance.ProcessId);
    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
  }
}

