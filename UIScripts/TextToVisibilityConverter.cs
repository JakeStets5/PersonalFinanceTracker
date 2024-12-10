using System;
using System.Globalization;
using System.Security;
using System.Windows;
using System.Windows.Data;

namespace PersonalFinanceTracker.UIScripts
{
    public class TextToVisibilityConverter : IValueConverter
    {

        // Converts a value to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a string or SecureString
            if (value is string text)
            {
                return string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is SecureString secureText)
            {
                return secureText.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible; // default case, when the value is neither string nor SecureString
        }

        // ConvertBack is not needed for one-way bindings, so we throw an exception
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
