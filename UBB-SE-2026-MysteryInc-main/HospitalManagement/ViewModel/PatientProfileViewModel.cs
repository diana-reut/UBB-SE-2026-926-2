using CommunityToolkit.Mvvm.ComponentModel;
using HospitalManagement.Entity;
using HospitalManagement.Integration.Export;
using HospitalManagement.Service;
using HospitalManagement.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal partial class PatientProfileViewModel : ObservableObject
{
    private readonly IPatientService _patientService;
    private readonly IImportService _importService;
    private readonly IExportService _exportService;
    private readonly PrescriptionView _prescriptionView;

    private readonly Func<PrescriptionView> _prescriptionViewFactory;

    public Action<string, string>? ShowAlertAction { get; set; }

    public Action<string>? OpenFileAction { get; set; }

    public Func<int, Task>? ShowPrescriptionAction { get; set; }

    private Patient? _currentPatient;

    public Patient? CurrentPatient
    {
        get => _currentPatient;
        set
        {
            if (SetProperty(ref _currentPatient, value))
            {
                OnPropertyChanged(nameof(FormattedChronicConditions));
                OnPropertyChanged(nameof(FormattedAllergies));
            }
        }
    }

    private MedicalRecord? _selectedRecord;

    public MedicalRecord? SelectedRecord
    {
        get => _selectedRecord;

        set => SetProperty(ref _selectedRecord, value);
    }

    public string FormattedChronicConditions
    {
        get
        {
            List<string>? conditions = CurrentPatient?.MedicalHistory?.ChronicConditions;
            if (conditions is null || conditions.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", conditions);
        }
    }

    public string FormattedAllergies
    {
        get
        {
            List<(Allergy Allergy, string SeverityLevel)>? allergies = CurrentPatient?.MedicalHistory?.Allergies;
            if (allergies is null || allergies.Count == 0)
                return "None";

            var result = new List<string>();
            foreach (var item in allergies)
                result.Add($"{item.Allergy.AllergyName} ({item.SeverityLevel})");

            return string.Join(", ", result);
        }
    }

    public PatientProfileViewModel(IPatientService patientService, IExportService exportService, IImportService importService, PrescriptionView prescriptionView, Func<PrescriptionView> prescriptionViewFactory)
    {
        _patientService = patientService;
        _exportService = exportService;
        _importService = importService;
        _prescriptionView = prescriptionView;
        _prescriptionViewFactory = prescriptionViewFactory;


        CurrentPatient = new Patient
        {
            MedicalHistory = new MedicalHistory { MedicalRecords = [], },
        };
    }

    public void LoadFullPatientProfile(int id)
    {
        try
        {
            Patient? p = _patientService.GetPatientDetails(id);
            if (p is null)
            {
                return;
            }

            p.MedicalHistory ??= new MedicalHistory();
            p.MedicalHistory.MedicalRecords ??= [];
            CurrentPatient = p;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading patient {id}: {ex.Message}");
        }
    }

    public void CheckHighRiskStatus()
    {
        if (CurrentPatient is not null && _patientService.IsHighRiskPatient(CurrentPatient.Id))
            ShowAlertAction?.Invoke("High Risk Patient Alert", "Warning: This patient is flagged as High Risk.");
    }

    public void ExportSelectedRecord()
    {
        if (SelectedRecord is null)
        {
            return;
        }

        try
        {
            string path = _exportService.ExportRecordToPDF(SelectedRecord.Id);
            OpenFileAction?.Invoke(path);
        }
        catch (Exception ex)
        {
            ShowAlertAction?.Invoke("Export Failed", ex.Message);
        }
    }

    public async Task ViewPrescriptionAsync()
    {
        if (SelectedRecord is null)
            return;

        Prescription? prescription = _patientService.GetPrescriptionByRecordId(SelectedRecord.Id);

        if (prescription is null)
        {
            ShowAlertAction?.Invoke("No Prescription", "This consultation does not have an associated prescription.");
            return;
        }

        var prescriptionWindow = new Window { Title = "Prescription Details" };
        prescriptionWindow.Activate();

        bool enqueuedCommand = prescriptionWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var prescriptionPage = _prescriptionViewFactory();
            prescriptionPage
                .ViewModel
                .ApplyFilterCommand
                .Execute(null);
            var frame = new Frame();
            prescriptionWindow.Content = frame;
            frame.Content = prescriptionPage;
        });
    }

    public void ImportRecords(bool isER)
    {
        if (CurrentPatient is null)
        {
            return;
        }

        try
        {
            if (isER)
            {
                _importService.ImportFromER(CurrentPatient.Id, 1);
            }
            else
            {
                _importService.ImportFromAppointment(CurrentPatient.Id, 1);
            }

            LoadFullPatientProfile(CurrentPatient.Id);
            ShowAlertAction?.Invoke("Import Successful", "Records imported correctly.");
        }
        catch (Exception ex)
        {
            ShowAlertAction?.Invoke("Import Failed", ex.Message);
        }
    }
}
