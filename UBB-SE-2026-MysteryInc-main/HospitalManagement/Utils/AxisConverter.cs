using System;
using Microsoft.UI.Xaml.Data;
using LiveChartsCore.SkiaSharpView;

namespace HospitalManagement.Utils;

internal class AxisConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        // Assumes value is Axis[] and needs to be IEnumerable<ICartesianAxis>
        if (value is Axis[] axes)
        {
            // Axis implements ICartesianAxis, so cast is safe
            return axes;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new MyNotImplementedException();
    }
}
