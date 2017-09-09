﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var threshold = parameter == null ? 0 : int.Parse((string) parameter);
            return (int)value > threshold ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}