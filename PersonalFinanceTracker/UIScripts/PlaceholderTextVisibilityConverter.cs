using System;
using System.Globalization;
using System.Security;
using System.Windows;
using System.Windows.Data;

namespace PersonalFinanceTracker.UIScripts
{
    public class PlaceholderTextVisibilityConverter : IValueConverter
    {
        // Converts a value to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle for text input like username, email, and password fields
            if (value is string text)
            {
                // Show placeholder text when the string is empty
                if (string.IsNullOrEmpty(text))
                {
                    return Visibility.Visible;
                }
                // Hide placeholder text when user has typed something
                return Visibility.Collapsed;
            }

            // Handle boolean values (error message visibility)
            if (value is bool isPassword)
            {
                // Show error message when there is an error
                return isPassword ? Visibility.Visible : Visibility.Collapsed;
            }

            // Default case: show if value is not handled by other conditions
            return Visibility.Visible;
        }

        // ConvertBack is not needed for one-way bindings, so we throw an exception
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
