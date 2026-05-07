using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;



namespace HospitalManagement.View;
//ma plang
internal sealed partial class MedicalStaffView : Window
{
    public MedicalStaffViewModel ViewModel { get; }

    public MedicalStaffView()
    {
        InitializeComponent();
        ViewModel = ServiceRegistry.Services.GetRequiredService<MedicalStaffViewModel>();

        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = ViewModel;
        }

        ViewModel.OpenBloodDonorsAction = async selectedPatient =>
        {
            var donorsWindow = new Window
            {
                Title = $"Compatible Donors - {selectedPatient.FirstName} {selectedPatient.LastName}",
            };

            IServiceProvider scope = ServiceRegistry.Services;
            BloodDonorsView donorsPage = scope.GetRequiredService<BloodDonorsView>();

            await donorsPage.InitializeAsync(selectedPatient.Id);

            donorsWindow.Content = donorsPage;
            donorsWindow.Activate();
        };

        ViewModel.OpenTransplantRequestAction = selectedPatient =>
        {
            var requestWindow = new Window
            {
                Title = $"Organ Transplant Request - {selectedPatient.FirstName} {selectedPatient.LastName}",
            };

            var requestPage = new TransplantRequestView(selectedPatient.Id, requestWindow);

            requestWindow.Content = requestPage;
            requestWindow.Activate();
        };
    }

    private async void PatientList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.ListView listView
            && listView.SelectedItem is Entity.Patient selectedPatient)
        {
            var newWindow = new Window
            {
                Title = "Patient Medical Profile",
            };

            // 3. Instantiate your Page passing the actual Patient Id
            IServiceProvider scope = ServiceRegistry.Services;
            PatientProfileView profilePage = scope.GetRequiredService<PatientProfileView>();
            await profilePage.InitializeAsync(selectedPatient.Id);

            newWindow.Content = profilePage;
            newWindow.Activate();
        }
    }
}
