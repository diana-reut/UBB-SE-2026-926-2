using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using HospitalManagement.Integration;
using HospitalManagement.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Integration;
using HospitalManagement.Proxy.AddictDetectionProxy;
using HospitalManagement.Proxy.PrescriptionProxy;

namespace HospitalManagement.ViewModel;

internal partial class PrescriptionViewModel : ObservableObject
{
    private readonly IPrescriptionProxy _prescriptionProxy;
    private readonly IAddictDetectionProxy _addictDetectionProxy;
    private int _loadVersion;

    public ObservableCollection<Prescription> Prescriptions { get; } = new();

    public ObservableCollection<Patient> AddictCandidates { get; } = new();

    public PrescriptionFilter ActiveFilter { get; private set; } = new();

    private const int PageSize = 9;

    [ObservableProperty]
    private int currentPage;

    [ObservableProperty]
    private string infoMessage = "";

    [ObservableProperty] private string? searchIdText;
    [ObservableProperty] private string? searchName;
    [ObservableProperty] private string? searchMedication;
    [ObservableProperty] private DateTimeOffset? dateFrom;
    [ObservableProperty] private DateTimeOffset? dateTo;

    public PrescriptionViewModel(
        IPrescriptionProxy prescriptionProxy,
        IAddictDetectionProxy addictDetectionProxy)
    {
        _prescriptionProxy = prescriptionProxy;
        _addictDetectionProxy = addictDetectionProxy;

        _ = LoadPrescriptionsAsync();
    }

    public async Task ShowPrescriptionAsync(int prescriptionId)
    {
        SearchIdText = prescriptionId.ToString(CultureInfo.InvariantCulture);
        SearchName = null;
        SearchMedication = null;
        DateFrom = null;
        DateTo = null;

        ActiveFilter = new PrescriptionFilter
        {
            PrescriptionId = prescriptionId
        };

        CurrentPage = 1;
        await UpdatePageDataAsync();

        if (Prescriptions.Count == 0)
        {
            InfoMessage = "No prescription found for the selected consultation.";
        }
    }



    [RelayCommand]
    private async Task ApplyFilter()
    {
        InfoMessage = "";
        CurrentPage = 1;

        ActiveFilter = new PrescriptionFilter
        {
            PrescriptionId = TryParseNullableInt(SearchIdText),
            MedName = Normalize(SearchMedication),
            DateFrom = DateFrom?.DateTime,
            DateTo = DateTo?.DateTime,
            PatientName = Normalize(SearchName),
            DoctorName = Normalize(SearchName)
        };

        try
        {
            await UpdatePageDataAsync();

            if (Prescriptions.Count == 0)
                InfoMessage = "No prescriptions found matching those criteria.";
        }
        catch (Exception ex)
        {
            InfoMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (Prescriptions.Count == PageSize)
        {
            CurrentPage++;
            await UpdatePageDataAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await UpdatePageDataAsync();
        }
    }

    [RelayCommand]
    private async Task SeeAllAddicts()
    {
        AddictCandidates.Clear();

        foreach (var patient in await _addictDetectionProxy.GetAddictCandidatesAsync())
            AddictCandidates.Add(patient);
    }

    private async Task LoadPrescriptionsAsync()
    {
        CurrentPage = 1;
        await UpdatePageDataAsync();
    }

    private async Task UpdatePageDataAsync()
    {
        int loadVersion = ++_loadVersion;
        InfoMessage = "";

        var fakeDoctors = MockDoctorProvider.FakeDoctors;

        bool hasFilter =
            ActiveFilter.PrescriptionId.HasValue ||
                !string.IsNullOrWhiteSpace(ActiveFilter.MedName) ||
                ActiveFilter.DateFrom.HasValue ||
                ActiveFilter.DateTo.HasValue ||
                !string.IsNullOrWhiteSpace(ActiveFilter.PatientName) ||
                !string.IsNullOrWhiteSpace(ActiveFilter.DoctorName);

        List<Prescription> targetList =
            hasFilter
            ? (await _prescriptionProxy.ApplyFilterAsync(ActiveFilter))
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList()
            : await _prescriptionProxy.GetLatestPrescriptionsAsync(PageSize, CurrentPage);

        if (loadVersion != _loadVersion)
        {
            return;
        }

        Prescriptions.Clear();

        foreach (var item in targetList)
        {
            if (string.IsNullOrEmpty(item.DoctorName) ||
                item.DoctorName.Contains("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                int index = RandomNumberGenerator.GetInt32(fakeDoctors.Count);
                var doc = fakeDoctors[index];
                item.DoctorName = $"Dr. {doc.FirstName} {doc.LastName}";
            }

            Prescriptions.Add(item);
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? TryParseNullableInt(string? value)
    {
        return int.TryParse(value, out int result) ? result : null;
    }
}
