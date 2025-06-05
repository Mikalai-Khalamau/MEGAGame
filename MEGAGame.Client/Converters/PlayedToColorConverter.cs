using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MEGAGame.Client.Converters
{
    public class PlayedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"PlayedToColorConverter: value={value}, type={value?.GetType()?.Name}");
            if (value is bool isPlayed)
            {
                Debug.WriteLine($"PlayedToColorConverter: isPlayed={isPlayed}");
                return isPlayed ? Brushes.Crimson : Brushes.Green;
            }
            Debug.WriteLine("PlayedToColorConverter: Returning default Green");
            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}