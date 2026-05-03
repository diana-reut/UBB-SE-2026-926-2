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
    /// <summary>
    /// Tasks 6.7, 6.9, 6.10, 6.11, 6.13
    ///
    /// Properties and methods match the class diagram exactly:
    ///   SelectedVisit  [ObservableProperty]
    ///   SendPatientData(): void  [RelayCommand]
    ///   LoadLogs(): void  [RelayCommand]
    ///
    /// Task 6.9: validation errors shown via WinUI 3 ContentDialog.
    /// </summary>
    public partial class TransferLogViewModel : BaseViewModel
    {
        public Action? ClearGridSelection { get; set; }
        public Action? RefreshGrid { get; set; }
        private readonly ITransferService transferService;

        // XamlRoot needed to show ContentDialogs — set by the View
        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        // Observable Properties
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedVisit))]
        private VisitSummary? selectedVisit;

        [ObservableProperty]
        private ObservableCollection<VisitSummary> eligibleVisits = new ObservableCollection<VisitSummary>();

        [ObservableProperty]
        private ObservableCollection<Transfer_Log> transferLogs = new ObservableCollection<Transfer_Log>();

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool canRetry = false;

        public bool HasSelectedVisit => SelectedVisit != null;

        // Constructor
        public TransferLogViewModel(ITransferService transferService)
        {
            this.transferService = transferService;
        }

        // LoadLogs(): void  (Task 6.13)
        [RelayCommand]
        public void LoadLogs()
        {
            TransferLogs.Clear();
            CanRetry = false;

            if (SelectedVisit == null)
            {
                return;
            }

            var logs = transferService.GetLogs(SelectedVisit.Visit_ID);
            foreach (var log in logs)
            {
                TransferLogs.Add(log);
            }

            // Task 6.11: enable Retry if most recent attempt was FAILED
            var latest = TransferLogs.FirstOrDefault();
            if (latest != null && latest.Status == "FAILED")
            {
                CanRetry = true;
            }
        }

        partial void OnSelectedVisitChanged(VisitSummary? value)
        {
            LoadLogs();
        }

        // Task 6.7 — Load visits in IN_EXAMINATION status
        [RelayCommand]
        public void LoadData()
        {
            SelectedVisit = null;
            TransferLogs.Clear();
            StatusMessage = string.Empty;
            CanRetry = false;

            var freshList = new ObservableCollection<VisitSummary>();

            var eligibleVisits = transferService.GetEligibleVisitsForTransfer();
            foreach (var eligibleVisit in eligibleVisits)
            {
                freshList.Add(new VisitSummary
                {
                    Visit_ID = eligibleVisit.VisitId,
                    Chief_Complaint = eligibleVisit.ChiefComplaint,
                    Status = eligibleVisit.Status,
                    PatientName = $"{eligibleVisit.PatientFirstName} {eligibleVisit.PatientLastName}",
                    Transferred = eligibleVisit.IsTransferred
                });
            }

            EligibleVisits = freshList;
        }

        // SendPatientData(): void  (Tasks 6.7, 6.9, 6.10)
        [RelayCommand]
        public async Task SendPatientData()
        {
            // Task 6.9: Validation with WinUI 3 ContentDialog
            if (SelectedVisit == null)
            {
                await ShowDialog("Validation Error", "Please select a visit before sending.");
                return;
            }
            if (SelectedVisit.Status != ER_Visit.VisitStatus.IN_EXAMINATION)
            {
                await ShowDialog("Validation Error",
                    "Transfer is only allowed for visits with status IN_EXAMINATION.");
                return;
            }
            if (SelectedVisit.Transferred)
            {
                await ShowDialog("Validation Error",
                    "This patient already has a successful transfer (Transferred = true).");
                return;
            }

            try
            {
                // Task 6.4 & 6.5: send data, save JSON, log attempt
                transferService.SendPatientData(SelectedVisit.Visit_ID);

                // Task 6.10: transition visit + mark patient as transferred
                transferService.TransitionVisitToTransferred(SelectedVisit.Visit_ID);
                transferService.MarkPatientAsTransferred(SelectedVisit.Visit_ID);

                SelectedVisit.Status = ER_Visit.VisitStatus.TRANSFERRED;
                SelectedVisit.Transferred = true;

                StatusMessage = "SUCCESS";
                CanRetry = false;

                // Task 6.10: show WinUI 3 ContentDialog success message + refresh data
                await ShowDialog("Transfer Successful",
                    $"Patient data for Visit {SelectedVisit.Visit_ID} has been successfully sent to Patient Management.");
                // LoadData();
            }
            catch (Exception ex)
            {
                StatusMessage = $"FAILED — {ex.Message}";
                CanRetry = true;
                await ShowDialog("Transfer Failed",
                    $"Transfer failed: {ex.Message}\nYou can retry using the Retry button.");
            }
            finally
            {
                // Task 6.13: always refresh after every attempt
                LoadLogs();
            }
        }

        // Task 6.11 — Retry failed transfer
        [RelayCommand]
        public async Task RetryTransfer()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                var result = transferService.RetryTransfer(SelectedVisit.Visit_ID);

                if (result.Status == "SUCCESS")
                {
                    transferService.TransitionVisitToTransferred(SelectedVisit.Visit_ID);
                    transferService.MarkPatientAsTransferred(SelectedVisit.Visit_ID);

                    SelectedVisit.Status = ER_Visit.VisitStatus.TRANSFERRED;
                    SelectedVisit.Transferred = true;

                    StatusMessage = "Retry SUCCESS";
                    CanRetry = false;

                    await ShowDialog("Retry Successful",
                        $"Patient data for Visit {SelectedVisit.Visit_ID} was successfully sent on retry.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Retry FAILED — {ex.Message}";
                await ShowDialog("Retry Failed", $"Retry failed: {ex.Message}");
            }
            finally
            {
                LoadLogs();
            }
        }

        [RelayCommand]
        public async Task CloseVisit()
        {
            if (SelectedVisit == null)
            {
                await ShowDialog("Validation Error", "Please select a visit before closing.");
                return;
            }
            if (SelectedVisit.Status != ER_Visit.VisitStatus.IN_EXAMINATION)
            {
                await ShowDialog("Validation Error",
                    "Closing is only allowed for visits with status IN_EXAMINATION.");
                return;
            }

            var visitId = SelectedVisit.Visit_ID;
            var patientName = SelectedVisit.PatientName;

            try
            {
                transferService.CloseVisit(visitId);

                SelectedVisit.Status = ER_Visit.VisitStatus.CLOSED;

                await ShowDialog("Visit Closed",
                    $"Visit {visitId} for {patientName} has been closed successfully.");
            }
            catch (Exception ex)
            {
                await ShowDialog("Close Failed", $"Could not close visit: {ex.Message}");
            }
        }

        // Task 6.9 helper — WinUI 3 ContentDialog
        private async Task ShowDialog(string title, string message)
        {
            if (XamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    // VisitSummary — lightweight helper for the eligible visits DataGrid
    public partial class VisitSummary : ObservableObject
    {
        [ObservableProperty]
        private int visit_ID;

        [ObservableProperty]
        private string patientName = string.Empty;

        [ObservableProperty]
        private string chief_Complaint = string.Empty;

        [ObservableProperty]
        private string status = string.Empty;

        [ObservableProperty]
        private bool transferred;
    }
}