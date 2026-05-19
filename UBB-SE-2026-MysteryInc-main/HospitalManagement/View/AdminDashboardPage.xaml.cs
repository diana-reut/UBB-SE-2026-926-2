using HospitalManagement.Infrastructure;
using HospitalManagement.View.DialogServiceAdmin;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace HospitalManagement.View;

internal sealed partial class AdminDashboardPage : Page
{
    private readonly AdminViewModel viewModel;
    private StatisticsView? statisticsView;

    public AdminDashboardPage()
    {
        InitializeComponent();

        IDialogService dialogService = ServiceRegistry.Services.GetRequiredService<IDialogService>();
        dialogService.SetWindow(ServiceRegistry.MainWindow);

        viewModel = ServiceRegistry.Services.GetRequiredService<AdminViewModel>();
        RootGrid.DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void PatientListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        viewModel.OpenPatientDetailsCommand.Execute(null);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AdminViewModel.IsStatisticsVisible) && viewModel.IsStatisticsVisible)
        {
            EnsureStatisticsViewLoaded();
        }
    }

    private void EnsureStatisticsViewLoaded()
    {
        if (statisticsView is not null)
        {
            return;
        }

        statisticsView = ServiceRegistry.Services.GetRequiredService<StatisticsView>();
        statisticsView.BackRequested += (_, _) => viewModel.IsStatisticsVisible = false;
        StatisticsContainer.Child = statisticsView;
    }
}
