using HospitalManagement.Infrastructure;
using HospitalManagement.View.DialogServiceAdmin;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement.View;

internal sealed partial class AdminDashboardPage : Page
{
    private readonly AdminViewModel viewModel;

    public AdminDashboardPage()
    {
        InitializeComponent();

        IDialogService dialogService = ServiceRegistry.Services.GetRequiredService<IDialogService>();
        dialogService.SetWindow(ServiceRegistry.MainWindow);

        StatisticsContainer.Child = ServiceRegistry.Services.GetRequiredService<StatisticsView>();

        viewModel = ServiceRegistry.Services.GetRequiredService<AdminViewModel>();
        RootGrid.DataContext = viewModel;
    }

    private void PatientListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        viewModel.OpenPatientDetailsCommand.Execute(null);
    }
}
