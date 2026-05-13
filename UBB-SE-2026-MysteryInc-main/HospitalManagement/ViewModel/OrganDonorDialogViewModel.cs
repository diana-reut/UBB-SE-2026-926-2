using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Common.Data.Entity;
using HospitalManagement.Proxy.TransplantProxy;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;

internal partial class OrganDonorDialogViewModel : ObservableObject
{
    private readonly ITransplantProxy _transplantProxy;

    [ObservableProperty]
    private Patient? _deceasedPatient;

    [ObservableProperty]
    private string? _selectedOrgan;

    [ObservableProperty]
    private TransplantMatch? _selectedMatch;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _loadingMessage;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<string> Organs { get; } =
    [
        "Heart", "Kidney", "Liver", "Pancreas", "Lung", "Cornea"
    ];

    public ObservableCollection<TransplantMatch> TopMatches { get; } = [];

    public Action<int, int, float>? OnAssignmentConfirmed { get; set; }

    public OrganDonorDialogViewModel(ITransplantProxy transplantProxy)
    {
        _transplantProxy = transplantProxy ?? throw new ArgumentNullException(nameof(transplantProxy));
    }

    partial void OnSelectedOrganChanged(string? value) => _ = LoadTopMatchesAsync();

    private void LoadTopMatches()
    {
        LoadTopMatchesAsync().GetAwaiter().GetResult();
    }

    private async Task LoadTopMatchesAsync()
    {
        if (DeceasedPatient is null || string.IsNullOrEmpty(SelectedOrgan))
        {
            TopMatches.Clear();
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Finding compatible recipients for {SelectedOrgan}...";

        try
        {
            List<TransplantMatch> matches =
                await _transplantProxy.GetTopMatchesAsDisplayModelsAsync(DeceasedPatient.Id, SelectedOrgan);

            TopMatches.Clear();
            foreach (TransplantMatch match in matches)
            {
                TopMatches.Add(match);
            }

            LoadingMessage = TopMatches.Count == 0
                ? $"No compatible recipients found for {SelectedOrgan}."
                : "";
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error loading matches: {ex.Message}";
            TopMatches.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<(bool Success, string? Error)> TryConfirmAssignmentAsync()
    {
        if (SelectedMatch is null)
        {
            return (false, "Please select a recipient from the list before confirming.");
        }

        if (string.IsNullOrEmpty(SelectedOrgan))
        {
            return (false, "Please select an organ before confirming.");
        }

        try
        {
            await _transplantProxy.AssignDonorAsync(SelectedMatch.TransplantId, DeceasedPatient!.Id, SelectedMatch.CompatibilityScore);
            OnAssignmentConfirmed?.Invoke(SelectedMatch.TransplantId, DeceasedPatient.Id, SelectedMatch.CompatibilityScore);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
