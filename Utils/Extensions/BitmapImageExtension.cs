using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Utils.Extensions;

public static class BitmapImageExtension
{
    public static void CreateFrom(this BitmapImage image, byte[] imageBytes)
    {
        using var  ms = new MemoryStream(imageBytes);
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
    }
}
