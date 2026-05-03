using System;
using Microsoft.UI.Xaml.Data;

namespace HospitalManagement.Utils;

internal class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new MyNotImplementedException();
    }
}
