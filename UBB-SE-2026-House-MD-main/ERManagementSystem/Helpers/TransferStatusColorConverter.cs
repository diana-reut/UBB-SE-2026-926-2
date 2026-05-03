using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ERManagementSystem.Helpers
{
    /// <summary>
    /// Task 6.14 - IValueConverter that maps Transfer_Log.Status to a SolidColorBrush:
    ///   SUCCESS  → Green
    ///   FAILED   → Red
    ///   RETRYING → Orange
    ///
    /// Applied to the Status column in TransferLogView.xaml DataGrid.
    /// </summary>
    public class TransferStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string status)
            {
                return status switch
                {
                    "SUCCESS" => new SolidColorBrush(Color.FromArgb(255, 34, 139, 34)),  // Green
                    "FAILED" => new SolidColorBrush(Color.FromArgb(255, 220, 20, 60)),  // Red
                    "RETRYING" => new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)),   // Orange
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}