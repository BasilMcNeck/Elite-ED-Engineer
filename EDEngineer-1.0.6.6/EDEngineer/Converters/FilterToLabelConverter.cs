using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models.Filters;

namespace EDEngineer.Converters
{
    public class FilterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var filter = (ILabelledFilter) value;
            return filter.Magic ? "All" : filter.Label;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}