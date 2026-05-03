using HospitalManagement.ViewModel;
using Microsoft.UI.Xaml.Controls;
using System;

namespace HospitalManagement.View;

internal sealed partial class PrescriptionView : UserControl
{
    public PrescriptionViewModel ViewModel { get; }

    public PrescriptionView(PrescriptionViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }
}