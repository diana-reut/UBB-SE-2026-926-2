using Microsoft.UI.Xaml.Controls;
using HospitalManagement.ViewModel;

namespace HospitalManagement.View;

internal sealed partial class PrescriptionDialog : ContentDialog
{
    public PrescriptionDialogViewModel ViewModel { get; }

    public PrescriptionDialog(PrescriptionDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

}