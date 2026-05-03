using Microsoft.UI.Xaml.Controls;

namespace HospitalManagement.View;

internal sealed partial class PoliceAlertDialog : ContentDialog
{
    public PoliceAlertDialog(string reportText)
    {
        InitializeComponent();
        ReportTextBlock.Text = reportText;
    }
}

