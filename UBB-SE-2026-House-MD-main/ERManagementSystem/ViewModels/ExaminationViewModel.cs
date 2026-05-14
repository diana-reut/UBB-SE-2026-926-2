using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Proxy.ERRoomProxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.ExaminationProxy;
using ERManagementSystem.Proxy.TriageParametersProxy;
using ERManagementSystem.Proxy.TriageProxy;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class ExaminationViewModel : ObservableObject
    {
        private readonly IExaminationProxy examinationProxy;
        private readonly MockStaffService mockStaffService;
        private readonly IERVisitProxy erVisitProxyApi;
        private readonly IERRoomProxy erRoomProxy;
        private readonly ITriageProxy triageProxyApi;
        private readonly ITriageParametersProxy triageParametersProxy;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RequestDoctorCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        [NotifyCanExecuteChangedFor(nameof(ViewSummaryCommand))]
        private ER_Visit? selectedVisit;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        private int doctorId;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        private string notes = string.Empty;

        [ObservableProperty]
        private string doctorName = string.Empty;

        [ObservableProperty]
        private string doctorSpecialty = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ER_Visit> eligibleVisits = new ObservableCollection<ER_Visit>();

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Examination> examinationHistory = new ObservableCollection<Examination>();

        [ObservableProperty]
        private string triageLevelDisplay = string.Empty;

        [ObservableProperty]
        private string triageSpecialization = string.Empty;

        [ObservableProperty]
        private string triageNurseId = string.Empty;

        [ObservableProperty]
        private string savedTimeDisplay = string.Empty;

        private readonly Microsoft.UI.Xaml.DispatcherTimer autoSaveTimer;
        private string lastSavedNotes = string.Empty;

        public ExaminationViewModel(
            IExaminationProxy examinationProxy,
            MockStaffService mockStaffService,
            IERVisitProxy erVisitProxyApi,
            IERRoomProxy erRoomProxy,
            ITriageProxy triageProxyApi,
            ITriageParametersProxy triageParametersProxy)
        {
            this.examinationProxy = examinationProxy;
            this.mockStaffService = mockStaffService;
            this.erVisitProxyApi = erVisitProxyApi;
            this.erRoomProxy = erRoomProxy;
            this.triageProxyApi = triageProxyApi;
            this.triageParametersProxy = triageParametersProxy;

            autoSaveTimer = new Microsoft.UI.Xaml.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10),
            };
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();
        }

        private async void AutoSaveTimer_Tick(object? sender, object e)
        {
            if (SelectedVisit == null || Notes == lastSavedNotes)
            {
                return;
            }

            Examination? existingExam = ExaminationHistory.FirstOrDefault(examination => examination.Visit_ID == SelectedVisit.Visit_ID);
            if (existingExam == null)
            {
                return;
            }

            await examinationProxy.UpdateNotesAsync(existingExam.Exam_ID, Notes);
            existingExam.Notes = Notes;
            lastSavedNotes = Notes;
            await ShowSavedIndicatorAsync();
        }

        private async Task ShowSavedIndicatorAsync()
        {
            SavedTimeDisplay = $"Auto-saved at {DateTime.Now:HH:mm:ss}";
            await Task.Delay(3000);
            SavedTimeDisplay = string.Empty;
        }

        private bool CanRequestDoctor()
            => SelectedVisit != null && SelectedVisit.Status == ER_Visit.VisitStatus.IN_ROOM;

        private bool CanSaveExamination()
        {
            return SelectedVisit != null &&
                   DoctorId != 0 &&
                   !string.IsNullOrWhiteSpace(Notes) &&
                   (SelectedVisit.Status == ER_Visit.VisitStatus.WAITING_FOR_DOCTOR ||
                    SelectedVisit.Status == ER_Visit.VisitStatus.IN_EXAMINATION);
        }

        private bool CanViewSummary()
        {
            return SelectedVisit != null && SelectedVisit.Status == ER_Visit.VisitStatus.IN_EXAMINATION;
        }

        [RelayCommand]
        public async Task LoadData()
        {
            EligibleVisits.Clear();
            SelectedVisit = null;
            DoctorId = 0;
            DoctorName = string.Empty;
            DoctorSpecialty = string.Empty;
            Notes = string.Empty;
            StatusMessage = string.Empty;
            ClearTriageDetails();

            List<ER_Visit> eligibleVisits = await examinationProxy.GetEligibleVisitsAsync();

            foreach (ER_Visit visit in eligibleVisits)
            {
                EligibleVisits.Add(visit);
            }
        }

        partial void OnSelectedVisitChanged(ER_Visit? value)
            => _ = HandleSelectedVisitChangedAsync(value);

        private async Task HandleSelectedVisitChangedAsync(ER_Visit? value)
        {
            if (value == null)
            {
                DoctorId = 0;
                DoctorName = string.Empty;
                DoctorSpecialty = string.Empty;
                Notes = string.Empty;
                ClearTriageDetails();
                ExaminationHistory.Clear();
                return;
            }

            ExaminationHistory.Clear();
            List<Examination> history = await examinationProxy.GetPatientHistoryAsync(value.Patient_ID);
            foreach (Examination exam in history)
            {
                ExaminationHistory.Add(exam);
            }

            try
            {
                await LoadTriageDetailsAsync(value.Visit_ID);
            }
            catch
            {
                ClearTriageDetails();
            }

            if (value.Status == ER_Visit.VisitStatus.WAITING_FOR_DOCTOR ||
                value.Status == ER_Visit.VisitStatus.IN_EXAMINATION)
            {
                Examination? existingExam = history.FirstOrDefault(examination => examination.Visit_ID == value.Visit_ID);
                if (existingExam != null)
                {
                    DoctorId = existingExam.Doctor_ID;
                    Doctor doctor = mockStaffService.GetDoctorByID(DoctorId);
                    DoctorName = doctor.Name;
                    DoctorSpecialty = doctor.Specialty;
                    Notes = existingExam.Notes;
                }
                else
                {
                    Triage? triage = await triageProxyApi.GetByVisitIdAsync(value.Visit_ID);
                    if (triage != null && !string.IsNullOrEmpty(triage.Specialization))
                    {
                        Triage_Parameters? triageParams = await triageParametersProxy.GetByTriageIdAsync(triage.Triage_ID);
                        int recoveredDoctorId = mockStaffService.RequestDoctor(triage.Specialization, triageParams ?? new Triage_Parameters());

                        DoctorId = recoveredDoctorId;
                        Doctor doctor = mockStaffService.GetDoctorByID(recoveredDoctorId);
                        DoctorName = doctor.Name;
                        DoctorSpecialty = doctor.Specialty;
                    }
                    else
                    {
                        DoctorId = 0;
                        DoctorName = string.Empty;
                        DoctorSpecialty = string.Empty;
                    }
                }
            }
            else
            {
                DoctorId = 0;
                DoctorName = string.Empty;
                DoctorSpecialty = string.Empty;
            }

            if (!history.Any(examination => examination.Visit_ID == value.Visit_ID))
            {
                Notes = string.Empty;
            }

            lastSavedNotes = Notes;
        }

        private async Task LoadTriageDetailsAsync(int visitId)
        {
            Triage? triage = await triageProxyApi.GetByVisitIdAsync(visitId);
            if (triage == null)
            {
                ClearTriageDetails();
                return;
            }

            Triage_Parameters? triageParams = await triageParametersProxy.GetByTriageIdAsync(triage.Triage_ID);
            if (triageParams == null)
            {
                ClearTriageDetails();
                return;
            }

            TriageLevelDisplay = $"Level {triage.Triage_Level}";
            TriageSpecialization = string.IsNullOrEmpty(triage.Specialization) ? "N/A" : triage.Specialization;
            TriageNurseId = $"Nurse #{triage.Nurse_ID}";
        }

        private void ClearTriageDetails()
        {
            TriageLevelDisplay = string.Empty;
            TriageSpecialization = string.Empty;
            TriageNurseId = string.Empty;
        }

        [RelayCommand(CanExecute = nameof(CanRequestDoctor))]
        public async Task RequestDoctor()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                int visitId = SelectedVisit.Visit_ID;
                Triage triage = await triageProxyApi.GetByVisitIdAsync(visitId)
                    ?? throw new InvalidOperationException($"Triage record not found for visit {visitId}");

                Triage_Parameters triageParameters = await triageParametersProxy.GetByTriageIdAsync(triage.Triage_ID)
                    ?? throw new InvalidOperationException($"Triage parameters not found for triage {triage.Triage_ID}");

                int assignedDoctorId = mockStaffService.RequestDoctor(triage.Specialization, triageParameters);
                await erVisitProxyApi.UpdateStatusAsync(visitId, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR);

                Doctor doctor = mockStaffService.GetDoctorByID(assignedDoctorId);
                StatusMessage = $"Doctor {doctor.Name} ({doctor.Specialty}) assigned.";

                await ShowDialog(
                    "Doctor Assigned",
                    $"Doctor {doctor.Name} (ID: {assignedDoctorId}, Specialty: {doctor.Specialty})\nAssigned to Visit {visitId}.");

                await LoadData();

                ER_Visit? reloadedVisit = EligibleVisits.FirstOrDefault(visit => visit.Visit_ID == visitId);
                if (reloadedVisit != null)
                {
                    SelectedVisit = reloadedVisit;
                }

                DoctorId = assignedDoctorId;
                DoctorName = doctor.Name;
                DoctorSpecialty = doctor.Specialty;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to request doctor: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveExamination))]
        public async Task SaveExamination()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                int assignedRoomId = await ResolveAssignedRoomIdAsync(SelectedVisit.Visit_ID);

                Examination examination = new Examination()
                {
                    Visit_ID = SelectedVisit.Visit_ID,
                    Doctor_ID = DoctorId,
                    Exam_Time = DateTime.Now,
                    Room_ID = assignedRoomId,
                    Notes = Notes,
                };

                await examinationProxy.CreateAsync(examination);
                await erVisitProxyApi.UpdateStatusAsync(examination.Visit_ID, ER_Visit.VisitStatus.IN_EXAMINATION);

                await ShowDialog(
                    "Examination Saved",
                    $"Examination for Visit {SelectedVisit.Visit_ID} has been saved.\nDoctor: {DoctorName} ({DoctorSpecialty})\nStatus transitioned to IN_EXAMINATION.");

                ER_Visit updatedVisit = new ER_Visit()
                {
                    Visit_ID = SelectedVisit.Visit_ID,
                    Patient_ID = SelectedVisit.Patient_ID,
                    Arrival_date_time = SelectedVisit.Arrival_date_time,
                    Chief_Complaint = SelectedVisit.Chief_Complaint,
                    Status = ER_Visit.VisitStatus.IN_EXAMINATION,
                };

                int index = EligibleVisits.IndexOf(SelectedVisit);
                if (index != -1)
                {
                    EligibleVisits.RemoveAt(index);
                    EligibleVisits.Insert(index, updatedVisit);
                    SelectedVisit = updatedVisit;
                }

                lastSavedNotes = Notes;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to save examination: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanViewSummary))]
        public async Task ViewSummary()
        {
            if (SelectedVisit == null || XamlRoot == null)
            {
                return;
            }

            Examination? existingExam = ExaminationHistory.FirstOrDefault(examination => examination.Visit_ID == SelectedVisit.Visit_ID);
            if (existingExam == null)
            {
                await ShowDialog("Notice", "No existing examination found for this current visit to summarize.");
                return;
            }

            ERExaminationSummaryDto? summary = await examinationProxy.GetSummaryByVisitIdAsync(SelectedVisit.Visit_ID);
            if (summary == null)
            {
                await ShowDialog("Error", "Could not aggregate summary data to display.");
                return;
            }

            Doctor doctor = mockStaffService.GetDoctorByID(summary.DoctorId);
            summary.AssignedDoctorName = $"{doctor.Name} ({doctor.Specialty})";

            var contentPanel = new Microsoft.UI.Xaml.Controls.StackPanel { Spacing = 10 };

            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Patient: {summary.FirstName} {summary.LastName}",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                FontSize = 16,
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Arrival: {summary.ArrivalDateTime}  |  Chief Complaint: {summary.ChiefComplaint}",
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = "\n--- TRIAGE DETAILS ---",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Level {summary.TriageLevel} ({summary.Specialization})\nSeverity Score: {summary.SeverityScore}\nVitals: C:{summary.Consciousness} Br:{summary.Breathing} Bl:{summary.Bleeding} Inj:{summary.InjuryType} Pn:{summary.PainLevel}",
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = "\n--- EXAMINATION ---",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Doctor: {summary.AssignedDoctorName}\nExam Time: {summary.ExamTime}\n\nNotes:\n{summary.Notes}",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            });

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Comprehensive Examination Summary",
                Content = contentPanel,
                CloseButtonText = "Close",
                XamlRoot = XamlRoot,
            };

            await dialog.ShowAsync();
        }

        private async Task<int> ResolveAssignedRoomIdAsync(int visitId)
        {
            ER_Room? currentRoom = (await erRoomProxy.GetAllAsync())
                .FirstOrDefault(room => room.Current_Visit_ID == visitId);

            if (currentRoom != null)
            {
                return currentRoom.Room_ID;
            }

            Examination? latestExam = (await examinationProxy.GetByVisitIdAsync(visitId))
                .OrderByDescending(examination => examination.Exam_Time)
                .FirstOrDefault();

            if (latestExam != null)
            {
                return latestExam.Room_ID;
            }

            ER_Room? fallbackRoom = (await erRoomProxy.GetAllAsync())
                .OrderBy(room => room.Room_ID)
                .FirstOrDefault();

            return fallbackRoom?.Room_ID ?? throw new InvalidOperationException("No ER rooms are available.");
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
                XamlRoot = XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
