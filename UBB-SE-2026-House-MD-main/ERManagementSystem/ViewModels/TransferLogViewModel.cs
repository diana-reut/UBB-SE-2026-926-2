using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.TransferLogProxy;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class TransferLogViewModel : BaseViewModel
    {
        private readonly ITransferLogProxy transferLogProxy;
        private readonly IERVisitProxy erVisitProxy;

        public Action? ClearGridSelection { get; set; }
        public Action? RefreshGrid { get; set; }

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

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

        public bool HasSelectedVisit => selectedVisit != null;

        public TransferLogViewModel(
            ITransferLogProxy transferLogProxy,
            IERVisitProxy erVisitProxy)
        {
            this.transferLogProxy = transferLogProxy;
            this.erVisitProxy = erVisitProxy;
        }

        [RelayCommand]
        public async Task LoadLogs()
        {
            TransferLogs.Clear();
            CanRetry = false;

            if (SelectedVisit == null)
            {
                return;
            }

            var logs = await transferLogProxy.GetByVisitIdAsync(SelectedVisit.Visit_ID);
            foreach (var log in logs)
            {
                TransferLogs.Add(log);
            }

            var latest = TransferLogs.FirstOrDefault();
            if (latest != null && latest.Status == "FAILED")
            {
                CanRetry = true;
            }
        }

        partial void OnSelectedVisitChanged(VisitSummary? value)
        {
            _ = LoadLogs();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            SelectedVisit = null;
            TransferLogs.Clear();
            StatusMessage = string.Empty;
            CanRetry = false;

            var freshList = new ObservableCollection<VisitSummary>();
            var eligibleVisits = await transferLogProxy.GetEligibleVisitsAsync();

            foreach (ERTransferEligibleVisitDto eligibleVisit in eligibleVisits)
            {
                freshList.Add(new VisitSummary
                {
                    Visit_ID = eligibleVisit.Visit_ID,
                    Chief_Complaint = eligibleVisit.Chief_Complaint,
                    Status = eligibleVisit.Status,
                    PatientName = eligibleVisit.PatientName,
                    Transferred = eligibleVisit.Transferred
                });
            }

            EligibleVisits = freshList;
        }

        [RelayCommand]
        public async Task SendPatientData()
        {
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
                await erVisitProxy.TransferVisitAsync(SelectedVisit.Visit_ID);

                SelectedVisit.Status = ER_Visit.VisitStatus.TRANSFERRED;
                SelectedVisit.Transferred = true;

                StatusMessage = "SUCCESS";
                CanRetry = false;

                await ShowDialog("Transfer Successful",
                    $"Patient data for Visit {SelectedVisit.Visit_ID} has been successfully sent to Patient Management.");
            }
            catch (Exception ex)
            {
                StatusMessage = $"FAILED - {ex.Message}";
                CanRetry = true;
                await ShowDialog("Transfer Failed",
                    $"Transfer failed: {ex.Message}\nYou can retry using the Retry button.");
            }
            finally
            {
                await LoadLogs();
            }
        }

        [RelayCommand]
        public async Task RetryTransfer()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                await erVisitProxy.RetryTransferAsync(SelectedVisit.Visit_ID);

                SelectedVisit.Status = ER_Visit.VisitStatus.TRANSFERRED;
                SelectedVisit.Transferred = true;

                StatusMessage = "Retry SUCCESS";
                CanRetry = false;

                await ShowDialog("Retry Successful",
                    $"Patient data for Visit {SelectedVisit.Visit_ID} was successfully sent on retry.");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Retry FAILED - {ex.Message}";
                await ShowDialog("Retry Failed", $"Retry failed: {ex.Message}");
            }
            finally
            {
                await LoadLogs();
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
                await erVisitProxy.CloseVisitAsync(visitId);
                SelectedVisit.Status = ER_Visit.VisitStatus.CLOSED;

                await ShowDialog("Visit Closed",
                    $"Visit {visitId} for {patientName} has been closed successfully.");
            }
            catch (Exception ex)
            {
                await ShowDialog("Close Failed", $"Could not close visit: {ex.Message}");
            }
        }

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
