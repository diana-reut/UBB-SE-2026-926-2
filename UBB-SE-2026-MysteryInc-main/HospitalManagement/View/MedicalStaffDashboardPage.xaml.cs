using HospitalManagement.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement.View;

internal sealed partial class MedicalStaffDashboardPage : Page
{
    public MedicalStaffViewModel ViewModel { get; }

    public MedicalStaffDashboardPage()
    {
        InitializeComponent();
        ViewModel = ServiceRegistry.Services.GetRequiredService<MedicalStaffViewModel>();
        DataContext = ViewModel;

        ViewModel.OpenBloodDonorsAction = async selectedPatient =>
        {
            BloodDonorsView donorsPage = ServiceRegistry.Services.GetRequiredService<BloodDonorsView>();
            await donorsPage.InitializeAsync(selectedPatient.Id);

            var donorsWindow = new Window
            {
                Title = $"Compatible Donors - {selectedPatient.FirstName} {selectedPatient.LastName}",
                Content = donorsPage,
            };

            donorsWindow.Activate();
        };

        ViewModel.OpenTransplantRequestAction = selectedPatient =>
        {
            var requestWindow = new Window
            {
                Title = $"Organ Transplant Request - {selectedPatient.FirstName} {selectedPatient.LastName}",
            };

            requestWindow.Content = new TransplantRequestView(selectedPatient.Id, requestWindow);
            requestWindow.Activate();
        };
    }

    private async void PatientList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is Patient selectedPatient)
        {
            var newWindow = new Window
            {
                Title = "Patient Medical Profile",
            };

            PatientProfileView profilePage = ServiceRegistry.Services.GetRequiredService<PatientProfileView>();
            await profilePage.InitializeAsync(selectedPatient.Id);

            newWindow.Content = profilePage;
            newWindow.Activate();
        }
    }
}
