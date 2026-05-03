using HospitalManagement.Entity;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using HospitalManagement.ViewModel;

namespace HospitalManagement.View;
//m
internal sealed partial class MedicalHistoryDialog : ContentDialog
{
    private readonly MedicalHistoryDialogViewModel _viewModel;
    private const string DefaultAllergySeverity = "Mild";
    private const string DefaultBloodType = "A";
    private const string DefaultRhFactor = "Positive";
    private const string AllergyDisplayMemberPath = "AllergyName";
    private const string AllergySelectedValuePath = "AllergyId";

    public MedicalHistory MedicalHistory => _viewModel.MedicalHistory!;
    public bool WasSkipped { get; private set; }
    public MedicalHistoryDialog(MedicalHistoryDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;

        Closing += MedicalHistoryDialog_Closing;
        AllergiesList.ItemsSource = _viewModel.AllergyList;
    }

    public void Initialize()
    {
        _viewModel.LoadAllergies();
        AllergyNameEntry.ItemsSource = _viewModel.AvailableAllergies;
        AllergyNameEntry.DisplayMemberPath = AllergyDisplayMemberPath;
        AllergyNameEntry.SelectedValuePath = AllergySelectedValuePath;

    }

    private void MedicalHistoryDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        if (args.Result != ContentDialogResult.Primary)
        {
            WasSkipped = true;
        }
    }

    private void AddAllergyButton_Click(object sender, RoutedEventArgs e)
    {
        string severity =
            (AllergySeverityEntry.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DefaultAllergySeverity;

        bool added = _viewModel.TryAddAllergy(
            AllergyNameEntry.SelectedItem as Allergy,
            severity);

        if (added)
        {
            AllergyNameEntry.SelectedIndex = -1;
            AllergySeverityEntry.SelectedIndex = 0;
        }
    }

    private void RemoveAllergyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is AllergyEntry allergyEntry)
        {
            _viewModel.RemoveAllergy(allergyEntry);
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        string bloodType =
            (BloodTypeEntry.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DefaultBloodType;

        string rh =
            (RhFactorEntry.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DefaultRhFactor;

        bool created = _viewModel.TryCreateMedicalHistory(
            bloodType,
            rh,
            ChronicConditionsEntry.Text);

        if (!created)
        {
            args.Cancel = true;
            return;
        }
        WasSkipped = false;
    }
}
