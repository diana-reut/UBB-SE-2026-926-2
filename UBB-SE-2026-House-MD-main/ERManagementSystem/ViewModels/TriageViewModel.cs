using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class TriageViewModel : BaseViewModel
    {
        private readonly ITriageService triageService;

        public TriageViewModel(ITriageService triageService)
        {
            this.triageService = triageService;
        }

        // ── Observable collections ──────────────────────────────────────────
        public ObservableCollection<ER_Visit> RegisteredVisits { get; } = new ObservableCollection<ER_Visit>();

        // ── Selected visit ──────────────────────────────────────────────────
        [ObservableProperty]
        private ER_Visit? selectedVisit;

        // ── Triage parameters (1-3 each) ────────────────────────────────────
        [ObservableProperty]
        private int consciousness;

        [ObservableProperty]
        private int breathing;

        [ObservableProperty]
        private int bleeding;

        [ObservableProperty]
        private int injuryType;

        [ObservableProperty]
        private int painLevel;

        // ── Calculated results ──────────────────────────────────────────────
        [ObservableProperty]
        private int calculatedSeverity;

        [ObservableProperty]
        private string calculatedSpecialization = string.Empty;

        // ── UI state: controls visibility of "Move to Queue" / "Cancel" ─────
        [ObservableProperty]
        private bool isTriaged;

        public bool IsNotTriaged => !IsTriaged;

        // ── Last triage result (needed for display) ─────────────────────────
        private Triage? lastTriageResult;

        partial void OnSelectedVisitChanged(ER_Visit? value)
        {
            if (value == null)
            {
                IsTriaged = false;
                return;
            }

            IsTriaged = value.Status == ER_Visit.VisitStatus.TRIAGED;

            if (IsTriaged)
            {
                var triage = triageService.GetByVisitId(value.Visit_ID);

                if (triage != null)
                {
                    CalculatedSeverity = triage.Triage_Level;
                    CalculatedSpecialization = triage.Specialization;
                }
            }
        }

        // ── Commands ────────────────────────────────────────────────────────
        [RelayCommand]
        private void LoadVisitsForTriage()
        {
            RegisteredVisits.Clear();

            var visits = triageService.GetVisitsForTriage();

            foreach (var v in visits)
            {
                RegisteredVisits.Add(v);
            }
        }

        [RelayCommand]
        private async Task PerformTriage()
        {
            if (SelectedVisit == null)
            {
                await ShowDialog("No Visit Selected",
                    "Please select a visit from the list before performing triage.");
                return;
            }

            if (SelectedVisit.Status == ER_Visit.VisitStatus.TRIAGED)
            {
                await ShowDialog("Triage already performed",
                    "The triage for this visit has already been performed.");
                return;
            }

            if (Consciousness == 0 || Breathing == 0 || Bleeding == 0 ||
                InjuryType == 0 || PainLevel == 0)
            {
                await ShowDialog("Incomplete Parameters",
                    "Please select a value for all 5 triage parameters.");
                return;
            }

            try
            {
                var parameters = new Triage_Parameters
                {
                    Consciousness = Consciousness,
                    Breathing = Breathing,
                    Bleeding = Bleeding,
                    Injury_Type = InjuryType,
                    Pain_Level = PainLevel
                };

                lastTriageResult = triageService.CreateTriage(SelectedVisit.Visit_ID, parameters);

                CalculatedSeverity = lastTriageResult.Triage_Level;
                CalculatedSpecialization = lastTriageResult.Specialization;
                IsTriaged = true;

                await ShowDialog("Triage Complete",
                    $"Visit {SelectedVisit.Visit_ID} triaged successfully.\n" +
                    $"Severity Level: {CalculatedSeverity}\n" +
                    $"Specialization: {CalculatedSpecialization}");

                // Remember the previously selected visit ID
                var previousVisitId = SelectedVisit.Visit_ID;

                // Reload the whole list
                LoadVisitsForTriage();

                // Reselect the same visit by ID
                SelectedVisit = RegisteredVisits.FirstOrDefault(v => v.Visit_ID == previousVisitId);
            }
            catch (Exception ex)
            {
                await ShowDialog("Triage Failed", ex.Message);
            }
        }

        [RelayCommand]
        private async Task MoveToQueue()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                triageService.MoveVisitToQueue(SelectedVisit.Visit_ID);

                await ShowDialog("Moved to Queue",
                    $"Visit {SelectedVisit.Visit_ID} is now WAITING_FOR_ROOM.");

                ResetForm();
                LoadVisitsForTriage();
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task CancelTriage()
        {
            if (SelectedVisit == null)
            {
                await ShowDialog("No Visit Selected",
                    "Please select a visit from the list before performing triage.");
                return;
            }

            triageService.CloseVisit(SelectedVisit.Visit_ID);

            await ShowDialog("Visit closed",
                $"The visit {SelectedVisit.Visit_ID} has been closed!");

            LoadVisitsForTriage();
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private void ResetForm()
        {
            SelectedVisit = null;
            Consciousness = 0;
            Breathing = 0;
            Bleeding = 0;
            InjuryType = 0;
            PainLevel = 0;
            CalculatedSeverity = 0;
            CalculatedSpecialization = string.Empty;
            IsTriaged = false;
            lastTriageResult = null;
        }

        private Microsoft.UI.Xaml.XamlRoot? GetXamlRoot()
            => App.MainAppWindow?.Content?.XamlRoot;

        private async Task ShowDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = GetXamlRoot()
            };
            await dialog.ShowAsync();
        }
    }
}
