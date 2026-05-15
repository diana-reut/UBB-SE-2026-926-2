using System;
using ERManagementSystem.Infrastructure;
using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class RoomAssignmentView : Page
    {
        public RoomAssignmentViewModel ViewModel { get; private set; }

        public RoomAssignmentView()
        {
            ViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<RoomAssignmentViewModel>(ServiceRegistry.Services);
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is RoomAssignmentViewModel vm)
            {
                ViewModel = vm;
            }

            ViewModel.XamlRoot = Content?.XamlRoot;
            Bindings.Update();
            ViewModel.LoadDataCommand.Execute(null);
        }

        private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.XamlRoot = XamlRoot;
            UpdateListHeights();
        }

        private void Page_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            UpdateListHeights();
        }

        private void UpdateListHeights()
        {
            double availableHeight = LeftContentGrid.ActualHeight
                - HeaderPanel.ActualHeight
                - ActionButtonsPanel.ActualHeight
                - 48;

            double listHeight = Math.Max(220, availableHeight);

            WaitingVisitsList.Height = listHeight;
            AvailableRoomsList.Height = listHeight;
        }
    }
}
