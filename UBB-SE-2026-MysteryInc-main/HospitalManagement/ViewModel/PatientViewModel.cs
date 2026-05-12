using CommunityToolkit.Mvvm.Input;
using Common.Data.Entity;
using HospitalManagement.Integration.Export;
using HospitalManagement.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Proxy.BillingProxy;
namespace HospitalManagement.ViewModel;

// this refactoring took years from my life

// hope and prayers that it's ok

internal class PatientViewModel : INotifyPropertyChanged
{
    private readonly IPatientProxy _patientService;
    private readonly IExportService? _exportService;
    private readonly IBillingProxy? _billingProxy;

    private Patient? _selectedPatient;

    public Patient? SelectedPatient
    {
        get => _selectedPatient;

        set
        {
            _selectedPatient = value;
            OnPropertyChanged();
            if (_selectedPatient is not null)
            {
                MedicalHistory = _selectedPatient.MedicalHistory;
                _ = LoadMedicalRecordsAsync();
            }
        }
    }

    private MedicalHistory? _medicalHistory;

    public MedicalHistory? MedicalHistory
    {
        get => _medicalHistory;

        set
        {
            _medicalHistory = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ChronicConditionsFormatted));
        }
    }

    public string ChronicConditionsFormatted
    {
        get
        {
            if (MedicalHistory?.ChronicConditions is null || MedicalHistory.ChronicConditions.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", MedicalHistory.ChronicConditions);
        }
    }

    private ObservableCollection<MedicalRecord>? _medicalRecords;

    public ObservableCollection<MedicalRecord>? MedicalRecords
    {
        get => _medicalRecords;

        set
        {
            _medicalRecords = value;
            OnPropertyChanged();
        }
    }

    private MedicalRecord? _selectedMedicalRecord;

    public MedicalRecord? SelectedMedicalRecord
    {
        get => _selectedMedicalRecord;

        set
        {
            _selectedMedicalRecord = value;
            OnPropertyChanged();
            RefreshCommandStates();
            if (_selectedMedicalRecord is not null)
            {
                _ = LoadBillingForRecordAsync(_selectedMedicalRecord);
            }
        }
    }

    private ObservableCollection<string>? _allergies;

    public ObservableCollection<string>? Allergies
    {
        get => _allergies;

        set
        {
            _allergies = value;
            OnPropertyChanged();
        }
    }

    private Prescription? _selectedPrescription;

    public Prescription? SelectedPrescription
    {
        get => _selectedPrescription;

        set
        {
            _selectedPrescription = value;
            OnPropertyChanged();
        }
    }

    private decimal _basePrice;

    public decimal BasePrice
    {
        get => _basePrice;

        set
        {
            _basePrice = value;
            OnPropertyChanged();
        }
    }

    private decimal _finalPrice;

    public decimal FinalPrice
    {
        get => _finalPrice;

        set
        {
            _finalPrice = value;
            OnPropertyChanged();
        }
    }

    private bool _discountApplied;

    public bool DiscountApplied
    {
        get => _discountApplied;

        set
        {
            _discountApplied = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public ICommand BackCommand { get; }

    public ICommand ExportRecordCommand { get; }

    public ICommand ViewPrescriptionCommand { get; }

    public ICommand ApplyDiscountCommand { get; }

    public Action? GoBackAction { get; set; }

    public Func<decimal, Task>? OpenRouletteAction { get; set; }

    public Func<Prescription, Task>? OpenPrescriptionDialogAction { get; set; }

    public Action<string, string>? ShowAlertAction { get; set; }

    public PatientViewModel(IPatientProxy patientService, IExportService exportService, IBillingProxy billingProxy)
    {
        _patientService = patientService;
        _exportService = exportService;
        _billingProxy = billingProxy;
        MedicalRecords = [];
        Allergies = [];
        BackCommand = new RelayCommand(GoBack);
        ExportRecordCommand = new AsyncRelayCommand(ExportSelectedRecordAsync, CanExportRecord);
        ApplyDiscountCommand = new AsyncRelayCommand(ApplyDiscountAsync, CanApplyDiscount);
        ViewPrescriptionCommand = new AsyncRelayCommand(ViewSelectedPrescriptionAsync, CanViewPrescription);
    }

    public async Task LoadFullPatientProfileAsync(int id)
    {
        try
        {
            Patient p = await _patientService.GetPatientDetailsAsync(id);
            if (p is null)
            {
                return;
            }

            p.MedicalHistory ??= new MedicalHistory();
            p.MedicalHistory.MedicalRecords ??= [];
            SelectedPatient = p;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task HandleRouletteResultAsync(int discount, decimal finalPrice)
    {
        if (SelectedMedicalRecord is null || _billingProxy is null)
        {
            return;
        }

        try
        {
            decimal calculatedFinalPrice = await _billingProxy.ApplyDiscountAsync(BasePrice, discount);

            SelectedMedicalRecord.DiscountApplied = discount;
            SelectedMedicalRecord.FinalPrice = calculatedFinalPrice;

            FinalPrice = calculatedFinalPrice;
            DiscountApplied = true;
            OnPropertyChanged(nameof(SelectedMedicalRecord));

            System.Diagnostics.Debug.WriteLine($"Discount applied: {discount}% | Final Price: {calculatedFinalPrice}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying discount: {ex.Message}");
        }
    }

    private async Task LoadBillingForRecordAsync(MedicalRecord record)
    {
        if (_billingProxy is null || SelectedPatient is null)
        {
            return;
        }

        try
        {
            BasePrice = await _billingProxy.ComputeBasePriceAsync(SelectedPatient.Id, record.Id);
            FinalPrice = record.FinalPrice > 0 ? record.FinalPrice : BasePrice;
            DiscountApplied = record.DiscountApplied.HasValue;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating base price: {ex.Message}");
        }
    }


    private async Task LoadMedicalRecordsAsync()
    {
        if (SelectedPatient is null || MedicalHistory is null)
        {
            MedicalRecords?.Clear();
            Allergies?.Clear();
            return;
        }

        try
        {
            List<MedicalRecord> records = await _patientService.GetMedicalRecordsAsync(MedicalHistory.Id);
            MedicalRecords?.Clear();
            foreach (MedicalRecord record in records.OrderByDescending(r => r.ConsultationDate))
            {
                MedicalRecords?.Add(record);
            }

            List<string> allergies = await _patientService.GetPatientAllergiesAsync(SelectedPatient.Id);
            Allergies?.Clear();
            foreach (string allergy in allergies)
            {
                Allergies?.Add(allergy);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading medical records: {ex.Message}");
        }
    }

    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    private bool CanExportRecord()
    {
        return SelectedMedicalRecord is not null && _exportService is not null;
    }

    private bool CanViewPrescription()
    {
        return SelectedMedicalRecord is not null;
    }

    private bool CanApplyDiscount()
    {
        return SelectedMedicalRecord is not null && !DiscountApplied && _billingProxy is not null;
    }

    private async Task ViewSelectedPrescriptionAsync()
    {
        if (SelectedMedicalRecord is null)
        {
            return;
        }

        try
        {
            Prescription? prescription = await _patientService.GetPrescriptionByRecordIdAsync(SelectedMedicalRecord.Id);
            if (prescription is null)
            {
                ShowAlertAction?.Invoke("No Prescription", "This consultation does not have an associated prescription.");
                return;
            }

            if (OpenPrescriptionDialogAction is not null)
            {
                await OpenPrescriptionDialogAction.Invoke(prescription);
            }
        }
        catch (Exception ex)
        {
            ShowAlertAction?.Invoke("Prescription Error", ex.Message);
            System.Diagnostics.Debug.WriteLine($"Error loading prescription: {ex.Message}");
        }
    }

    private async Task ApplyDiscountAsync()
    {
        if (SelectedMedicalRecord is null || _billingProxy is null)
        {
            return;
        }

        if (OpenRouletteAction is not null)
        {
            await OpenRouletteAction.Invoke(BasePrice);
        }
    }

    private async Task ExportSelectedRecordAsync()
    {
        if (SelectedMedicalRecord is null || _exportService is null)
        {
            return;
        }

        try
        {
            _ = await _exportService.ExportRecordToPDFAsync(SelectedMedicalRecord.Id);
            System.Diagnostics.Debug.WriteLine($"Successfully exported record {SelectedMedicalRecord.Id} to PDF");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting record: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RefreshCommandStates()
    {
        if (ExportRecordCommand is AsyncRelayCommand exportCommand)
        {
            exportCommand.NotifyCanExecuteChanged();
        }

        if (ViewPrescriptionCommand is AsyncRelayCommand viewPrescriptionCommand)
        {
            viewPrescriptionCommand.NotifyCanExecuteChanged();
        }

        if (ApplyDiscountCommand is AsyncRelayCommand applyDiscountCommand)
        {
            applyDiscountCommand.NotifyCanExecuteChanged();
        }
    }
}
