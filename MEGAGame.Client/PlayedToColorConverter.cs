using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MEGAGame.Client.Converters
{
    public class PlayedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlayed)
            {
                return isPlayed ? Brushes.Red : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}