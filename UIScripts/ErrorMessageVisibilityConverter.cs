using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PersonalFinanceTracker.UIScripts
{
    public class ErrorMessageVisibilityConverter: IValueConverter
    {
        // Converts a value to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string errorMessage)
            {
                return string.IsNullOrEmpty(errorMessage) ? Visibility.Collapsed : Visibility.Visible;
            }

            // Default case: show if value is not handled by other conditions
            return Visibility.Collapsed;
        }

        // ConvertBack is not needed for one-way bindings, so we throw an exception
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
