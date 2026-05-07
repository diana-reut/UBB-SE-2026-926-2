using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Infrastructure;
using ERManagementSystem.Services;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ERManagementSystem.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;

        public MainWindowViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void ShowPatientRegistration()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<PatientRegistrationViewModel>();
            navigationService.Navigate(typeof(PatientRegistrationView), vm);
        }

        [RelayCommand]
        private void ShowQueue()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<QueueViewModel>();
            navigationService.Navigate(typeof(QueueView), vm);
        }

        [RelayCommand]
        private void ShowTriage()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<TriageViewModel>();
            navigationService.Navigate(typeof(TriageView), vm);
        }

        [RelayCommand]
        private void ShowRoomAssignment()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<RoomAssignmentViewModel>();
            navigationService.Navigate(typeof(RoomAssignmentView), vm);
        }

        [RelayCommand]
        private void ShowExamination()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<ExaminationViewModel>();
            navigationService.Navigate(typeof(ExaminationView), vm);
        }

        [RelayCommand]
        private void ShowTransferLog()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<TransferLogViewModel>();
            navigationService.Navigate(typeof(TransferLogView), vm);
        }

        [RelayCommand]
        private void ShowRoomManagement()
        {
            var vm = ServiceRegistry.Services.GetRequiredService<RoomManagementViewModel>();
            navigationService.Navigate(typeof(RoomManagementView), vm);
        }
    }
}
