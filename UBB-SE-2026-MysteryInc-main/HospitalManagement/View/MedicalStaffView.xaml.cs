using Common.Data.Entity;
using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace HospitalManagement.View;

internal sealed partial class MedicalStaffView : Window
{
    public MedicalStaffViewModel ViewModel { get; }

    public MedicalStaffView()
    {
        ViewModel = ServiceRegistry.Services.GetRequiredService<MedicalStaffViewModel>();
        InitializeComponent();

        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = ViewModel;
        }
    }

    private async void PatientList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.ListView listView
            && listView.SelectedItem is Patient selectedPatient)
        {
            await ViewModel.OpenPatientProfileAsync(selectedPatient);
        }
    }
}
