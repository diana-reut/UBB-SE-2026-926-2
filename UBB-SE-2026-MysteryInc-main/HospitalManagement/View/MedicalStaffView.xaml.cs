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
        ViewModel = ((App)Application.Current).Services.GetRequiredService<MedicalStaffViewModel>();

        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = ViewModel;
        }

        ViewModel.OpenBloodDonorsAction = selectedPatient =>
        {
            var donorsWindow = new Window
            {
                Title = $"Compatible Donors - {selectedPatient.FirstName} {selectedPatient.LastName}",
            };

            IServiceProvider scope = (Application.Current as App)!.Services;
            BloodDonorsView donorsPage = scope.GetRequiredService<BloodDonorsView>();

            donorsPage.Initialize(selectedPatient.Id);

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

    private void PatientList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.ListView listView
            && listView.SelectedItem is Entity.Patient selectedPatient)
        {
            var newWindow = new Window
            {
                Title = "Patient Medical Profile",
            };

            // 3. Instantiate your Page passing the actual Patient Id
            IServiceProvider scope = ((App)Application.Current).Services;
            PatientProfileView profilePage = scope.GetRequiredService<PatientProfileView>();
            profilePage.Initialize(selectedPatient.Id);

            newWindow.Content = profilePage;
            newWindow.Activate();
        }
    }
}
