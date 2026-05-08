using Common.Data.Entity;
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

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
    {
        ContentDialogButtonClickDeferral deferral = e.GetDeferral();
        try
        {
            (bool success, string? error) = await ViewModel.TryConfirmAssignmentAsync();
            if (!success)
            {
                e.Cancel = true;
                ViewModel.ErrorMessage = error;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }

    public void Initialize(Patient donor, Action<int, int, float> onAssigned)
    {
        ViewModel.DeceasedPatient = donor;
        ViewModel.OnAssignmentConfirmed = onAssigned;
    }
}
