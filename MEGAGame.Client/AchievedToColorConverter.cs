using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MEGAGame.Client.Converters
{
    public class AchievedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAchieved)
            {
                return isAchieved ? Brushes.Gold : Brushes.Gray;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}