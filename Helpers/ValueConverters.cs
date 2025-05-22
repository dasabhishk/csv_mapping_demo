using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CsvMapper.Helpers
{
    /// <summary>
    /// Converts boolean values to "Loaded" or "Not Loaded" strings
    /// </summary>
    public class BooleanYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Loaded" : "Not Loaded";
            }
            return "Not Started";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean values to "Valid" or "Invalid" strings
    /// </summary>
    public class BooleanValidInvalidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Valid" : "Invalid";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null values to Visibility.Collapsed, non-null to Visibility.Visible
    /// </summary>
    public class NullToInvisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}