using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WindowsPhone.Recipes.Push.Server.Resources.Converters
{
    /// <summary>
    /// Converts null to default tile image.
    /// </summary>
    public class NullTileImageConverter : IValueConverter
    {
        /// <value>Default tile image resource relative path.</value>
        private static readonly Uri DefaultTileImage = new Uri("/Resources/TileImages/Null.jpg", UriKind.Relative);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value ?? DefaultTileImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
