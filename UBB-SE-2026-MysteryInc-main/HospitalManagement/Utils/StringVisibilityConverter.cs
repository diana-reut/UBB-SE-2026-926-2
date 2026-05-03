using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace HospitalManagement.Utils;

internal class StringVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string? selected = value as string;
        string? target = parameter as string;

        // IF THE UI WANTS A BOOLEAN (for InfoBar.IsOpen)
        if (targetType == typeof(bool))
        {
            return !string.IsNullOrEmpty(selected);
        }

        // IF THE UI WANTS VISIBILITY (for Charts)
        if (string.IsNullOrEmpty(selected))
        {
            return Visibility.Collapsed;
        }

        // Default logic for chart visibility
        bool isMatch = string.IsNullOrEmpty(target) || target.Contains(selected, StringComparison.OrdinalIgnoreCase);
        return isMatch ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new MyNotImplementedException();
    }
}

