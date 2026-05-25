using Common.Data.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement.View;

internal sealed partial class MedicalStaffDashboardPage : Page
{
    public MedicalStaffViewModel ViewModel { get; }

    public MedicalStaffDashboardPage()
    {
        ViewModel = ServiceRegistry.Services.GetRequiredService<MedicalStaffViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SearchCommand.CanExecute(null))
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private async void PatientList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is Patient selectedPatient)
        {
            await ViewModel.OpenPatientProfileAsync(selectedPatient);
        }
    }
}
