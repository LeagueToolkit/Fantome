using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Fantome.ModManagement.IO;

namespace Fantome.ValueConverters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Image image = (value as ModFile).Image;
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bitmap = new BitmapImage();

            image.Save(memoryStream, ImageFormat.Png);

            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
