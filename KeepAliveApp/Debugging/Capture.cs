using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;

namespace KeepAliveApp.Debugging;

internal static class Capture
{
  public static async Task CaptureHighResWindowAsync(Window window, UIElement element, string fileName, double scaleFactor = 1.0)
  {
    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
    var dpi = Interop.NativeMethods.GetDpiForWindow(hWnd);

    if (scaleFactor <= 0)
      scaleFactor = 1.0;

    // 기존 RenderTransform 백업
    var oldTransform = element.RenderTransform;
    var oldOrigin = element.RenderTransformOrigin;

    // 스케일 업 적용
    element.RenderTransform = new ScaleTransform
    {
      ScaleX = scaleFactor,
      ScaleY = scaleFactor
    };
    element.RenderTransformOrigin = new Point(0, 0);

    Vector2 scaledSize = new((float)(element.ActualSize.X * scaleFactor), (float)(element.ActualSize.Y * scaleFactor));

    Debug.WriteLine(dpi);
    Debug.WriteLine(scaledSize.X + " X " + scaledSize.Y);
    try
    {
      var renderTarget = new RenderTargetBitmap();
      await renderTarget.RenderAsync(element, (int)scaledSize.X, (int)scaledSize.Y);

      var pixelBuffer = await renderTarget.GetPixelsAsync();
      var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

      using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
      {
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetPixelData(
          BitmapPixelFormat.Bgra8,
          BitmapAlphaMode.Premultiplied,
          (uint)scaledSize.X,
          (uint)scaledSize.Y,
          dpi, dpi,  // DPI
          pixelBuffer.ToArray());
        await encoder.FlushAsync();
      }
    }
    catch(Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      await Task.Delay(1000);
      element.RenderTransform = oldTransform;
      element.RenderTransformOrigin = oldOrigin;
    }
  }
}
