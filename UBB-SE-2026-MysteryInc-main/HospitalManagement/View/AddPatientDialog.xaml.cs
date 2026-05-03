using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagement.View;

internal sealed partial class AddPatientDialog : ContentDialog
{
    public Entity.Patient NewPatient { get; private set; } = null!;

    private readonly ViewModel.AddPatientDialogViewModel _viewModel;

    public AddPatientDialog()
    {
        _viewModel = ((App)Application.Current).Services.GetRequiredService<ViewModel.AddPatientDialogViewModel>();
        InitializeComponent();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        string sex = (SexEntry.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "M";

        ViewModel.AddPatientDialogViewModel.FormValidationResult result = ViewModel.AddPatientDialogViewModel.ValidateForm(
            FirstNameEntry.Text,
            LastNameEntry.Text,
            CnpEntry.Text,
            PhoneEntry.Text,
            EmergencyEntry.Text
        );

        FirstNameError.Visibility = result.FirstNameValid ? Visibility.Collapsed : Visibility.Visible;
        LastNameError.Visibility = result.LastNameValid ? Visibility.Collapsed : Visibility.Visible;
        CnpError.Visibility = result.CnpValid ? Visibility.Collapsed : Visibility.Visible;
        PhoneError.Visibility = result.PhoneValid ? Visibility.Collapsed : Visibility.Visible;
        EmergencyError.Visibility = result.EmergencyValid ? Visibility.Collapsed : Visibility.Visible;

        if (!result.IsValid)
        {
            args.Cancel = true;
            return;
        }

        (bool success, string? errorMessage, Entity.Patient? patient) = _viewModel.SubmitPatient(
            FirstNameEntry.Text,
            LastNameEntry.Text,
            sex,
            DobEntry.Date,
            CnpEntry.Text,
            PhoneEntry.Text,
            EmergencyEntry.Text
        );

        if (!success)
        {
            ErrorLabel.Text = errorMessage;
            ErrorLabel.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        ErrorLabel.Visibility = Visibility.Collapsed;
        NewPatient = patient!;
    }
}
