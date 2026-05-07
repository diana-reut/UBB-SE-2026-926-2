using Microsoft.UI.Xaml.Controls;
using HospitalManagement.ViewModel;
using System.Threading.Tasks;

namespace HospitalManagement.View;

internal sealed partial class BloodDonorsView : Page
{
    public BloodDonorsViewModel ViewModel { get; }

    public BloodDonorsView(BloodDonorsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    public void Initialize(int patientId)
    {
        _ = InitializeAsync(patientId);
    }

    public async Task InitializeAsync(int patientId)
    {
        await ViewModel.LoadCompatibleDonorsAsync(patientId);
    }
}
