using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement.View;

internal sealed partial class PharmacistDashboardPage : Page
{
    public PharmacistViewModel ViewModel { get; }

    public PharmacistDashboardPage()
    {
        ViewModel = ServiceRegistry.Services.GetRequiredService<PharmacistViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
