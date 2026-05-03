using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<MainWindowViewModel>();

            var navigationService = App.Services.GetRequiredService<NavigationService>();
            navigationService.Initialize(ContentFrame);

            // FIX: Navigate properly so OnNavigatedTo fires and ViewModel is set.
            // Previously used ContentFrame.Content = ... which bypasses navigation
            // entirely — OnNavigatedTo never fired, ViewModel was always null.
            ViewModel.ShowPatientRegistrationCommand.Execute(null);
            AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
        }

        private void AppNavigationView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer?.Tag is not string tag)
            {
                return;
            }

            switch (tag)
            {
                case "PatientRegistration":
                    ViewModel.ShowPatientRegistrationCommand.Execute(null);
                    break;
                case "Queue":
                    ViewModel.ShowQueueCommand.Execute(null);
                    break;
                case "Triage":
                    ViewModel.ShowTriageCommand.Execute(null);
                    break;
                case "RoomAssignment":
                    ViewModel.ShowRoomAssignmentCommand.Execute(null);
                    break;
                case "Examination":
                    ViewModel.ShowExaminationCommand.Execute(null);
                    break;
                case "TransferLog":
                    ViewModel.ShowTransferLogCommand.Execute(null);
                    break;
                case "RoomManagement":
                    ViewModel.ShowRoomManagementCommand.Execute(null);
                    break;
            }
        }
    }
}