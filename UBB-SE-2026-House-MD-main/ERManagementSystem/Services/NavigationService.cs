using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? frame;

        public void Initialize(Frame frame)
        {
            this.frame = frame;
        }

        public void Navigate(Type pageType)
        {
            frame?.Navigate(pageType);
        }

        public void Navigate(Type pageType, object parameter) // parameter - the ViewModel associated with the page
        {
            frame?.Navigate(pageType, parameter);
        }
    }
}
