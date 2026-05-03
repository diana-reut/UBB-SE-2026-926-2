using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using ERManagementSystem.Models;

namespace ERManagementSystem.Helpers
{
    public class RoomTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string roomType)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            return roomType switch
            {
                ER_Room.RoomType.OperatingRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220,  38,  38)),
                ER_Room.RoomType.TraumaBay => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 234, 108,  10)),
                ER_Room.RoomType.RespiratoryRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255,  37, 100, 235)),
                ER_Room.RoomType.NeurologyRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 124,  58, 237)),
                ER_Room.RoomType.OrthopedicRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255,  22, 163,  74)),
                ER_Room.RoomType.GeneralRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255,  13, 148, 136)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public class RoomStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string status)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            return status switch
            {
                ER_Room.RoomStatus.Available => new SolidColorBrush(Windows.UI.Color.FromArgb(255,  22, 163,  74)),
                ER_Room.RoomStatus.Occupied => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220,  38,  38)),
                ER_Room.RoomStatus.Cleaning => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 217, 119,   6)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
