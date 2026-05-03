using HospitalManagement.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace HospitalManagement.View;

internal sealed partial class StatisticsView : UserControl
{
    private readonly StatisticsViewModel _statisticsViewModel;

    public StatisticsView(StatisticsViewModel statisticsViewModel)
    {
        InitializeComponent();
        _statisticsViewModel = statisticsViewModel ?? throw new ArgumentNullException(nameof(statisticsViewModel));
        DataContext = _statisticsViewModel;
    }

    private void StatisticsMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatisticsMenu.SelectedItem is not string selected)
        {
            return;
        }

        StatisticTitle.Text = selected;
        ErrorInfoBar.IsOpen = false;
        HideAllCharts();

        try
        {
            _statisticsViewModel.LoadDataForSelection(selected);
            ShowChartForSelection(selected);
        }
        catch (Exception error)
        {
            ShowError(error.Message);
        }
    }

    private void ShowChartForSelection(string selection)
    {
        StatisticsType type = StatisticsViewModel.GetByString(selection);
        switch (type)
        {
            case StatisticsType.PatientDistribution:
            case StatisticsType.ConsultationSource:
            {
                PieChartControl.Visibility = Visibility.Visible;
                break;
            }
            case StatisticsType.TopMedications:
            case StatisticsType.TopDiagnoses:
            {
                CartesianControl.Visibility = Visibility.Visible;
                break;
            }
            case StatisticsType.Demographics:
            {
                DemographicsGrid.Visibility = Visibility.Visible;
                break;
            }

            default:
            {
                ShowError("Requested unregistered statistics.");
                break;
            }
        }
    }

    private void HideAllCharts()
    {
        PieChartControl.Visibility = Visibility.Collapsed;
        CartesianControl.Visibility = Visibility.Collapsed;
        DemographicsGrid.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }
}
