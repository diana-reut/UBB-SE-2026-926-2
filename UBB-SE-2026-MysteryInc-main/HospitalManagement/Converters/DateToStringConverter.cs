using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace HospitalManagement.Converters;

internal partial class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return ""; // Returns empty string if date is null/invalid
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new MyNotImplementedException();
    }
}
