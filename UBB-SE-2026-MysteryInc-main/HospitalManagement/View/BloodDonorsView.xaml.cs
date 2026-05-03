using Microsoft.UI.Xaml.Controls;
using HospitalManagement.ViewModel;

namespace HospitalManagement.View;

internal sealed partial class BloodDonorsView : Page
{
    public BloodDonorsViewModel ViewModel { get; }

    public BloodDonorsView(BloodDonorsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    public void Initialize(int patientId)
    {
        ViewModel.LoadCompatibleDonors(patientId);
    }
}
