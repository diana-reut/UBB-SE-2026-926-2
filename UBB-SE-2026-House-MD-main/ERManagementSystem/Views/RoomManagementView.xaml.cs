using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class RoomManagementView : Page
    {
        public RoomManagementViewModel ViewModel { get; private set; }

        public RoomManagementView()
        {
            ViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<RoomManagementViewModel>(App.Services);
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is RoomManagementViewModel vm)
            {
                ViewModel = vm;
            }

            ViewModel.XamlRoot = this.Content?.XamlRoot;
            Bindings.Update();
            ViewModel.LoadRoomsCommand.Execute(null);
        }

        private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.XamlRoot = this.XamlRoot;
        }
    }
}
