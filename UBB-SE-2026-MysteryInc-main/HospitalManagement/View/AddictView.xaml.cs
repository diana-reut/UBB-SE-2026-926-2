using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.Extensions.DependencyInjection;
using HospitalManagement.Infrastructure;


namespace HospitalManagement.View;

internal sealed partial class AddictView : UserControl
{
    public ViewModel.AddictViewModel ViewModel { get; }

    public AddictView()
    {
        ViewModel = ServiceRegistry.Services.GetRequiredService<ViewModel.AddictViewModel>();
        InitializeComponent();
    }

    private async void OnNotifyPoliceClickedAsync(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int patientId)
        {
            string reportText = await ViewModel.GetPoliceReportMessageAsync(patientId);


            var dialog = new PoliceAlertDialog(reportText)
            {
                XamlRoot = XamlRoot,
            };

            ContentDialogResult result = await dialog.ShowAsync();


            if (result == ContentDialogResult.Primary)
            {
                ViewModel.ConfirmPoliceAlert(patientId);
            }
        }
    }
}
