using HospitalManagement.Entity;
using HospitalManagement.ViewModel;
using Microsoft.UI.Xaml.Controls;
using System;

namespace HospitalManagement.View;

internal sealed partial class OrganDonorDialog : ContentDialog
{
    public OrganDonorDialogViewModel ViewModel { get; set; }

    public OrganDonorDialog(OrganDonorDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
    {
        if (!ViewModel.TryConfirmAssignment(out string? error))
        {
            e.Cancel = true;
            ViewModel.ErrorMessage = error;
        }
    }

    public void Initialize(Patient donor, Action<int, int, float> onAssigned)
    {
        ViewModel.DeceasedPatient = donor;
        ViewModel.OnAssignmentConfirmed = onAssigned;
    }
}
