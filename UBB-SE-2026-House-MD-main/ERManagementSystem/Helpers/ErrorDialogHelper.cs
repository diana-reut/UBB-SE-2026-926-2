using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.Helpers
{
    public static class ErrorDialogHelper
    {
        public static async Task ShowErrorAsync(string title, string message)
        {
            if (App.MainAppWindow?.Content is not FrameworkElement rootElement)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = rootElement.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
