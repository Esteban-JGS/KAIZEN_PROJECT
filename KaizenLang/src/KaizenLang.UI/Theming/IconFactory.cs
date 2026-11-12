using System.Drawing;
using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace KaizenLang.UI.Theming;

public static class IconFactory
{
    private static readonly string IconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icons");

    public static Image? GetIcon(string iconName, int width = 16, int height = 16)
    {
        try
        {
            var filePath = Path.Combine(IconPath, $"{iconName}.svg");
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (var svg = new SKSvg())
            {
                if (svg.Load(filePath) is null)
                {
                    return null;
                }

                // Create a SkiaSharp bitmap and canvas
                using (var skBitmap = new SKBitmap(width, height))
                {
                    using (var canvas = new SKCanvas(skBitmap))
                    {
                        canvas.Clear(SKColors.Transparent);

                        // Check if picture is valid
                        if (svg.Picture == null)
                        {
                            return null;
                        }

                        // Calculate scaling to fit the icon
                        float scaleX = (float)width / svg.Picture.CullRect.Width;
                        float scaleY = (float)height / svg.Picture.CullRect.Height;
                        var matrix = SKMatrix.CreateScale(scaleX, scaleY);

                        // Draw the SVG onto the canvas
                        canvas.DrawPicture(svg.Picture, in matrix);
                    }

                    // Convert SkiaSharp bitmap to System.Drawing.Bitmap
                    using var image = SKImage.FromBitmap(skBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = new MemoryStream(data.ToArray());
                    return new Bitmap(stream);
                }
            }
        }
        catch
        {
            // Log error, return null or a default icon
            return null;
        }
    }
}
