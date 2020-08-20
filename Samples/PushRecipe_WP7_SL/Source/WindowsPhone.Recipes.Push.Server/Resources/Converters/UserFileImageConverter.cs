using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.IO;

namespace WindowsPhone.Recipes.Push.Server.Resources.Converters
{
    /// <summary>
    /// Image file name to image file name with relevant path.
    /// </summary>
    public class UserFileImageConverter : IValueConverter
    {
        /// <value>Default tile image resource relative path.</value>
        private static readonly Uri DefaultTileImage = new Uri("/Resources/TileImages/Null.jpg", UriKind.Relative);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Uri imageUri = DefaultTileImage;
            string fileName = value as string;
            if (value != null)
            {
                string imageFullPath = Path.Combine(@"Resources\TileImages\Numbers", (string)value);
                if (File.Exists(imageFullPath))
                {
                    imageUri = new Uri(@"\" + imageFullPath, UriKind.Relative);
                }
            }

            return imageUri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
