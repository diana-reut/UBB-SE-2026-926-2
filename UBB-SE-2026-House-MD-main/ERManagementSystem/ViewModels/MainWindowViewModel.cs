using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
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
            var vm = App.Services.GetRequiredService<PatientRegistrationViewModel>();
            navigationService.Navigate(typeof(PatientRegistrationView), vm);
        }

        [RelayCommand]
        private void ShowQueue()
        {
            var vm = App.Services.GetRequiredService<QueueViewModel>();
            navigationService.Navigate(typeof(QueueView), vm);
        }

        [RelayCommand]
        private void ShowTriage()
        {
            var vm = App.Services.GetRequiredService<TriageViewModel>();
            navigationService.Navigate(typeof(TriageView), vm);
        }

        [RelayCommand]
        private void ShowRoomAssignment()
        {
            var vm = App.Services.GetRequiredService<RoomAssignmentViewModel>();
            navigationService.Navigate(typeof(RoomAssignmentView), vm);
        }

        [RelayCommand]
        private void ShowExamination()
        {
            var vm = App.Services.GetRequiredService<ExaminationViewModel>();
            navigationService.Navigate(typeof(ExaminationView), vm);
        }

        [RelayCommand]
        private void ShowTransferLog()
        {
            var vm = App.Services.GetRequiredService<TransferLogViewModel>();
            navigationService.Navigate(typeof(TransferLogView), vm);
        }

        [RelayCommand]
        private void ShowRoomManagement()
        {
            var vm = App.Services.GetRequiredService<RoomManagementViewModel>();
            navigationService.Navigate(typeof(RoomManagementView), vm);
        }
    }
}