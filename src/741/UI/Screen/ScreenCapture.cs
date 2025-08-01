using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TextCopy;
using Graphics = System.Drawing.Graphics;

namespace DarkAges.Library.UI.Screen;

public class ScreenCapture : IDisposable
{
    private string outputDirectory;
    private string watermarkText;

    public ScreenCapture()
    {
        outputDirectory = "screenshots";
        watermarkText = "Dark Ages";
        Directory.CreateDirectory(outputDirectory);
    }

    public void CaptureScreenToClipboard()
    {
        try
        {
            using var bitmap = new Bitmap(640, 480);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                ApplyWatermark(g, bitmap.Size);
            }
            ClipboardService.SetText(bitmap.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture screen to clipboard: {ex.Message}");
        }
    }

    private void ApplyWatermark(System.Drawing.Graphics g, Size size)
    {
        if (string.IsNullOrEmpty(watermarkText)) return;

        using var font = new Font("Arial", 12);
        var textSize = g.MeasureString(watermarkText, font);
        var position = new PointF(size.Width - textSize.Width - 10, size.Height - textSize.Height - 10);
            
        using var brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
        g.DrawString(watermarkText, font, brush, position);
    }

    public void Dispose()
    {
    }
}