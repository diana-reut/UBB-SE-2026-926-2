using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.Extensions.DependencyInjection;


namespace HospitalManagement.View;

internal sealed partial class AddictView : UserControl
{
    public ViewModel.AddictViewModel ViewModel { get; }

    public AddictView()
    {
        ViewModel = ((App)Application.Current).Services.GetRequiredService<ViewModel.AddictViewModel>();
        InitializeComponent();
    }

    private async void OnNotifyPoliceClickedAsync(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int patientId)
        {
            string reportText = ViewModel.GetPoliceReportMessage(patientId);


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
