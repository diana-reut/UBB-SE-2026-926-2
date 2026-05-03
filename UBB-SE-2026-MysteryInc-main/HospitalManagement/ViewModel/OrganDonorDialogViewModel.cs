using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HospitalManagement.Entity;
using HospitalManagement.Service;

namespace HospitalManagement.ViewModel;

internal partial class OrganDonorDialogViewModel : ObservableObject
{
    private readonly ITransplantService _transplantService;

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

    public OrganDonorDialogViewModel(ITransplantService transplantService)
    {
        _transplantService = transplantService ?? throw new ArgumentNullException(nameof(transplantService));
    }

    partial void OnSelectedOrganChanged(string? value) => LoadTopMatches();

    private void LoadTopMatches()
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
                _transplantService.GetTopMatchesAsDisplayModels(DeceasedPatient.Id, SelectedOrgan);

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

    public bool TryConfirmAssignment(out string? error)
    {
        error = null;

        if (SelectedMatch is null)
        {
            error = "Please select a recipient from the list before confirming.";
            return false;
        }

        if (string.IsNullOrEmpty(SelectedOrgan))
        {
            error = "Please select an organ before confirming.";
            return false;
        }

        try
        {
            _transplantService.AssignDonor(SelectedMatch.TransplantId, DeceasedPatient!.Id, SelectedMatch.CompatibilityScore);
            OnAssignmentConfirmed?.Invoke(SelectedMatch.TransplantId, DeceasedPatient.Id, SelectedMatch.CompatibilityScore);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Error: {ex.Message}";
            return false;
        }
    }
}
