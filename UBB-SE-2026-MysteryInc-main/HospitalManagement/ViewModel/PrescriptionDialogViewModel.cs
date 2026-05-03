using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospitalManagement.Entity;

namespace HospitalManagement.ViewModel;

internal partial class PrescriptionDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string doctorNotes = "No notes provided";

    public ObservableCollection<PrescriptionItem> PrescriptionItems { get; } = new();

    [ObservableProperty]
    private Visibility noMedicationsVisibility = Visibility.Visible;

    public PrescriptionDialogViewModel()
    {
        PrescriptionItems.CollectionChanged += (_, __) =>
        {
            UpdateVisibility();
        };
    }

    private void UpdateVisibility()
    {
        NoMedicationsVisibility =
            PrescriptionItems.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    [RelayCommand]
    public void Initialize(Prescription? prescription)
    {
        PrescriptionItems.Clear();

        if (prescription == null)
        {
            DoctorNotes = "No prescription data available";
            UpdateVisibility();
            return;
        }

        DoctorNotes = prescription.DoctorNotes ?? "No notes provided";

        if (prescription.MedicationList != null)
        {
            foreach (var item in prescription.MedicationList)
            {
                PrescriptionItems.Add(item);
            }
        }

        UpdateVisibility();
    }
}