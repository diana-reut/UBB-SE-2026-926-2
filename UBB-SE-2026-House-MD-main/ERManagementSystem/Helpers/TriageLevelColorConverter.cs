using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ERManagementSystem.Helpers
{
    /// <summary>
    /// Task 3.14 – IValueConverter that maps triage levels to SolidColorBrush:
    ///   Level 1 → Red
    ///   Level 2 → Orange
    ///   Level 3 → Yellow
    ///   Level 4 → Green
    ///   Level 5 → Blue
    ///
    /// Applied to QueueView DataGrid rows.
    /// </summary>
    public class TriageLevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int level)
            {
                return level switch
                {
                    1 => new SolidColorBrush(Color.FromArgb(255, 220, 38, 38)),   // Red
                    2 => new SolidColorBrush(Color.FromArgb(255, 217, 119, 6)),   // Orange
                    3 => new SolidColorBrush(Color.FromArgb(255, 234, 179, 8)),   // Yellow
                    4 => new SolidColorBrush(Color.FromArgb(255, 22, 163, 74)),   // Green
                    5 => new SolidColorBrush(Color.FromArgb(255, 37, 99, 235)),   // Blue
                    _ => new SolidColorBrush(Color.FromArgb(255, 156, 163, 175)) // Gray
                };
            }
            return new SolidColorBrush(Color.FromArgb(255, 156, 163, 175)); // Gray fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
