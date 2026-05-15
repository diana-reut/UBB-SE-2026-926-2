using CommunityToolkit.Mvvm.ComponentModel;
using Common.Data.Entity;
using HospitalManagement.Integration.Export;
using HospitalManagement.Service;
using HospitalManagement.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagement.Proxy.PatientProxy;

namespace HospitalManagement.ViewModel;

internal partial class PatientProfileViewModel : ObservableObject
{
    private readonly IPatientProxy _patientService;
    private readonly IImportService _importService;
    private readonly IExportService _exportService;
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

    public PatientProfileViewModel(IPatientProxy patientService, IExportService exportService, IImportService importService, Func<PrescriptionView> prescriptionViewFactory)
    {
        _patientService = patientService;
        _exportService = exportService;
        _importService = importService;
        _prescriptionViewFactory = prescriptionViewFactory;
    }

    public async Task LoadFullPatientProfileAsync(int patientId)
    {
        try
        {
            Patient? patient = await _patientService.GetPatientDetailsAsync(patientId);
            if (patient is null)
            {
                return;
            }

            patient.MedicalHistory ??= new MedicalHistory
            {
                PatientId = patient.Id,
            };
            patient.MedicalHistory.MedicalRecords ??= [];

            for (int i = 0; i < patient.MedicalHistory.MedicalRecords.Count; i++)
            {
                MedicalRecord record = patient.MedicalHistory.MedicalRecords[i];
                try
                {
                    record.Prescription = await _patientService.GetPrescriptionByRecordIdAsync(record.Id);
                }
                catch (Exception ex)
                {
                    record.Prescription = null;
                    System.Diagnostics.Debug.WriteLine($"No prescription loaded for record {record.Id}: {ex.Message}");
                }
            }

            CurrentPatient = patient;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading patient: {ex.Message}");
        }
    }

    public void CheckHighRiskStatus()
    {
        CheckHighRiskStatusAsync().GetAwaiter().GetResult();
    }

    public async Task CheckHighRiskStatusAsync()
    {
        if (CurrentPatient is not null && await _patientService.IsHighRiskPatientAsync(CurrentPatient.Id))
            ShowAlertAction?.Invoke("High Risk Patient Alert", "Warning: This patient is flagged as High Risk.");
    }

    public async Task ExportSelectedRecordAsync()
    {
        if (SelectedRecord is null)
        {
            return;
        }

        try
        {
            string path = await _exportService.ExportRecordToPDFAsync(SelectedRecord.Id);
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

        Prescription? prescription = await _patientService.GetPrescriptionByRecordIdAsync(SelectedRecord.Id);

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
            var frame = new Frame();
            prescriptionWindow.Content = frame;
            frame.Content = prescriptionPage;
            _ = prescriptionPage.ViewModel.ShowPrescriptionAsync(prescription.Id);
        });
    }

    public void ImportRecords(bool isER)
    {
        ImportRecordsAsync(isER).GetAwaiter().GetResult();
    }

    public async Task ImportRecordsAsync(bool isER)
    {
        if (CurrentPatient is null)
        {
            return;
        }

        try
        {
            if (isER)
            {
                await _importService.ImportFromERAsync(CurrentPatient.Id, 1);
            }
            else
            {
                await _importService.ImportFromAppointmentAsync(CurrentPatient.Id, 1);
            }

            await LoadFullPatientProfileAsync(CurrentPatient.Id);
            ShowAlertAction?.Invoke("Import Successful", "Records imported correctly.");
        }
        catch (Exception ex)
        {
            ShowAlertAction?.Invoke("Import Failed", ex.Message);
        }
    }
}
