using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;

namespace KeepAliveApp.Debugging;

internal static class Capture
{
  public static async Task CaptureHighResWindowAsync(UIElement element, string fileName)
  {
    double originalWidth = element.RenderSize.Width;
    double originalHeight = element.RenderSize.Height;

    var renderTarget = new RenderTargetBitmap();
    await renderTarget.RenderAsync(element, (int)originalWidth, (int)originalHeight);

    var pixelBuffer = await renderTarget.GetPixelsAsync();
    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
    {
      var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
      encoder.SetPixelData(
        BitmapPixelFormat.Bgra8,
        BitmapAlphaMode.Premultiplied,
        (uint)originalWidth,
        (uint)originalHeight,
        96, 96,  // DPI
        pixelBuffer.ToArray());
      await encoder.FlushAsync();
    }
  }
}
