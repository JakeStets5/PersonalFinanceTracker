namespace PersonalFinanceTracker.UIScripts
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class InvertedBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible; // Inverts visibility

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility visibility) && visibility == Visibility.Collapsed;
        }
    }
}
