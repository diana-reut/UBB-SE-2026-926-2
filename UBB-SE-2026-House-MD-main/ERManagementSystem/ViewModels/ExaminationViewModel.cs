using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.ViewModels
{
    /// <summary>
    /// Tasks 4.6, 4.9, 4.11: ExaminationViewModel
    ///
    /// Properties (all [ObservableProperty]):
    ///   SelectedVisit, DoctorId, Notes, DoctorName, DoctorSpecialty
    ///
    /// Commands (all [RelayCommand] with CanExecute):
    ///   RequestDoctor(), SaveExamination(), LoadData()
    ///
    /// Task 4.9: Buttons disabled until conditions are met via CanExecute.
    /// Task 4.11: Shows doctor name and specialty.
    /// </summary>
    public partial class ExaminationViewModel : ObservableObject
    {
        private readonly IExaminationService examinationService;
        private readonly MockStaffService mockStaffService;
        private readonly IERVisitRepository erVisitRepository;
        private readonly IExaminationRepository examRepository;
        private readonly ITriageRepository triageRepository;
        private readonly ITriageParametersRepository triageParamsRepo;
        private readonly IRoomRepository? roomRepository;   // Task 5.13: resolve correct Room_ID

        // XamlRoot needed to show ContentDialogs — set by the View
        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        // Observable Properties
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
        private ObservableCollection<ER_Visit> eligibleVisits = new ();

        [ObservableProperty]
        private string statusMessage = string.Empty;

        // Task 4.10: Examination History
        [ObservableProperty]
        private ObservableCollection<Examination> examinationHistory = new ();

        // Task 4.14: Triage Details
        [ObservableProperty]
        private string triageLevelDisplay = string.Empty;

        [ObservableProperty]
        private string triageSpecialization = string.Empty;

        [ObservableProperty]
        private string triageNurseId = string.Empty;

        // Triage Detail visibility mechanismve state
        private Microsoft.UI.Xaml.DispatcherTimer autoSaveTimer;
        private string lastSavedNotes = string.Empty;

        [ObservableProperty]
        private string savedTimeDisplay = string.Empty;

        // Constructor
        public ExaminationViewModel(
            IExaminationService examinationService,
            MockStaffService mockStaffService,
            IERVisitRepository erVisitRepository,
            IExaminationRepository examRepository,
            ITriageRepository triageRepository,
            ITriageParametersRepository triageParamsRepo,
            IRoomRepository? roomRepository = null) // optional — Task 5.13
        {
            this.examinationService = examinationService;
            this.mockStaffService = mockStaffService;
            this.erVisitRepository = erVisitRepository;
            this.examRepository = examRepository;
            this.triageRepository = triageRepository;
            this.triageParamsRepo = triageParamsRepo;
            this.roomRepository = roomRepository;

            // Task 4.13: Initialize 10s auto-save timer
            autoSaveTimer = new Microsoft.UI.Xaml.DispatcherTimer();
            autoSaveTimer.Interval = TimeSpan.FromSeconds(10);
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object? sender, object e)
        {
            if (SelectedVisit != null && Notes != lastSavedNotes)
            {
                var existingExam = ExaminationHistory.FirstOrDefault(e => e.Visit_ID == SelectedVisit.Visit_ID);
                if (existingExam != null)
                {
                    examRepository.UpdateNotes(existingExam.Exam_ID, Notes);
                    lastSavedNotes = Notes;

                    existingExam.Notes = Notes;

                    ShowSavedIndicator();
                }
            }
        }

        private async void ShowSavedIndicator()
        {
            SavedTimeDisplay = $"Auto-saved at {DateTime.Now:HH:mm:ss}";
            await System.Threading.Tasks.Task.Delay(3000); // Show indicator for 3 seconds
            SavedTimeDisplay = string.Empty;
        }

        // Task 4.9: CanExecute methods
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

        // LoadData [RelayCommand]
        // Loads visits with status IN_ROOM and WAITING_FOR_DOCTOR.
        [RelayCommand]
        public void LoadData()
        {
            EligibleVisits.Clear();
            SelectedVisit = null;
            DoctorId = 0;
            DoctorName = string.Empty;
            DoctorSpecialty = string.Empty;
            Notes = string.Empty;
            StatusMessage = string.Empty;
            ClearTriageDetails();

            var inRoom = erVisitRepository.GetByStatus(ER_Visit.VisitStatus.IN_ROOM);
            var waiting = erVisitRepository.GetByStatus(ER_Visit.VisitStatus.WAITING_FOR_DOCTOR);

            var allEligible = inRoom.Concat(waiting).OrderBy(v => v.Arrival_date_time);

            foreach (var visit in allEligible)
            {
                EligibleVisits.Add(visit);
            }
        }

        // When SelectedVisit changes, show assigned doctor if applicable
        partial void OnSelectedVisitChanged(ER_Visit? value)
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
            var history = examRepository.GetByPatientId(value.Patient_ID);
            foreach (var exam in history)
            {
                ExaminationHistory.Add(exam);
            }

            // Task 4.14: Load triage details for the selected visit
            try
            {
                LoadTriageDetails(value.Visit_ID);
            }
            catch
            {
                // If triage data can't be loaded, continue — don't block doctor info
                ClearTriageDetails();
            }

            // For WAITING_FOR_DOCTOR or IN_EXAMINATION visits,
            // look up doctor details from the CURRENT examination record first
            if (value.Status == ER_Visit.VisitStatus.WAITING_FOR_DOCTOR ||
                value.Status == ER_Visit.VisitStatus.IN_EXAMINATION)
            {
                var existingExam = history.FirstOrDefault(e => e.Visit_ID == value.Visit_ID);
                if (existingExam != null)
                {
                    DoctorId = existingExam.Doctor_ID;
                    var doctor = mockStaffService.GetDoctorByID(DoctorId);
                    DoctorName = doctor.Name;
                    DoctorSpecialty = doctor.Specialty;
                    Notes = existingExam.Notes;
                }
                else
                {
                    // No Examination record yet. Recover the doctor ID dynamically.
                    var triage = triageRepository.GetByVisitId(value.Visit_ID);
                    if (triage != null && !string.IsNullOrEmpty(triage.Specialization))
                    {
                        // Even if Triage Parameters are corrupted/missing in the seed, we can still recover the doctor.
                        var triageParams = triageParamsRepo.GetByTriageId(triage.Triage_ID);
                        int recoveredDoctorId = mockStaffService.RequestDoctor(triage.Specialization, triageParams ?? new ERManagementSystem.Models.Triage_Parameters());

                        DoctorId = recoveredDoctorId;
                        var doctor = mockStaffService.GetDoctorByID(recoveredDoctorId);
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

            if (history.FirstOrDefault(e => e.Visit_ID == value.Visit_ID) == null)
            {
                Notes = string.Empty;
            }

            // Sync notes for auto-save baseline
            lastSavedNotes = Notes;
        }

        // Task 4.14: Triage Details helpers

        /// <summary>
        /// Task 4.14: Load all triage details for a visit using repository models.
        /// </summary>
        private void LoadTriageDetails(int visitId)
        {
            var triage = triageRepository.GetByVisitId(visitId);
            if (triage != null)
            {
                var triageParams = triageParamsRepo.GetByTriageId(triage.Triage_ID);
                if (triageParams != null)
                {
                    TriageLevelDisplay = $"Level {triage.Triage_Level}";
                    TriageSpecialization = string.IsNullOrEmpty(triage.Specialization) ? "N/A" : triage.Specialization;
                    TriageNurseId = $"Nurse #{triage.Nurse_ID}";

                    return;
                }
            }
            ClearTriageDetails();
        }

        private void ClearTriageDetails()
        {
            TriageLevelDisplay = string.Empty;
            TriageSpecialization = string.Empty;
            TriageNurseId = string.Empty;
        }

        // RequestDoctor [RelayCommand]
        // Task 4.5 / 4.6: Request a doctor for the selected visit.
        // Task 4.9: CanExecute disables button unless visit is IN_ROOM.
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
                int assignedDoctorId = examinationService.RequestDoctor(visitId);

                var doctor = mockStaffService.GetDoctorByID(assignedDoctorId);

                StatusMessage = $"Doctor {doctor.Name} ({doctor.Specialty}) assigned.";

                await ShowDialog("Doctor Assigned",
                    $"Doctor {doctor.Name} (ID: {assignedDoctorId}, Specialty: {doctor.Specialty})\n" +
                    $"Assigned to Visit {visitId}.");

                // Reload the list so the visit now shows WAITING_FOR_DOCTOR status.
                LoadData();

                // Re-select the same visit (now with WAITING_FOR_DOCTOR status)
                var reloadedVisit = EligibleVisits.FirstOrDefault(v => v.Visit_ID == visitId);
                if (reloadedVisit != null)
                {
                    SelectedVisit = reloadedVisit;
                }

                // Re-apply doctor info (OnSelectedVisitChanged will now recover
                // the doctor via triage specialization lookup, but we override
                // with the exact values we got from the service call)
                DoctorId = assignedDoctorId;
                DoctorName = doctor.Name;
                DoctorSpecialty = doctor.Specialty;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to request doctor: {ex.Message}");
            }
        }

        // SaveExamination [RelayCommand]
        // Task 4.4 / 4.8: Save the examination record and transition
        // WAITING_FOR_DOCTOR → IN_EXAMINATION.
        [RelayCommand(CanExecute = nameof(CanSaveExamination))]
        public async Task SaveExamination()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                // Task 5.13: use the actual room assigned to this visit so the auto-clean hook
                // in StateManagementService.ChangeVisitStatus() can find the right room later.
                int assignedRoomId;
                if (roomRepository != null)
                {
                    assignedRoomId = roomRepository.GetAssignedRoomIdForVisit(SelectedVisit.Visit_ID)
                                     ?? examRepository.GetFirstRoomId();
                }
                else
                {
                    assignedRoomId = examRepository.GetFirstRoomId();
                }

                var examination = new Examination
                {
                    Visit_ID = SelectedVisit.Visit_ID,
                    Doctor_ID = DoctorId,
                    Exam_Time = DateTime.Now,
                    Room_ID = assignedRoomId,
                    Notes = Notes
                };

                examinationService.SaveExamination(examination);

                await ShowDialog("Examination Saved",
                    $"Examination for Visit {SelectedVisit.Visit_ID} has been saved.\n" +
                    $"Doctor: {DoctorName} ({DoctorSpecialty})\n" +
                    $"Status transitioned to IN_EXAMINATION.");

                // Create a clone of the visit with updated status (needed because ER_Visit is not INotifyPropertyChanged)
                var updatedVisit = new ER_Visit
                {
                    Visit_ID = SelectedVisit.Visit_ID,
                    Patient_ID = SelectedVisit.Patient_ID,
                    Arrival_date_time = SelectedVisit.Arrival_date_time,
                    Chief_Complaint = SelectedVisit.Chief_Complaint,
                    Status = ER_Visit.VisitStatus.IN_EXAMINATION
                };

                int index = EligibleVisits.IndexOf(SelectedVisit);
                if (index != -1)
                {
                    EligibleVisits.RemoveAt(index);
                    EligibleVisits.Insert(index, updatedVisit);
                    SelectedVisit = updatedVisit;
                }

                // Sync notes for auto-save baseline
                lastSavedNotes = Notes;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to save examination: {ex.Message}");
            }
        }

        // Task 4.12: Examination Summary [RelayCommand]
        [RelayCommand(CanExecute = nameof(CanViewSummary))]
        public async Task ViewSummary()
        {
            if (SelectedVisit == null || XamlRoot == null)
            {
                return;
            }

            var existingExam = ExaminationHistory.FirstOrDefault(e => e.Visit_ID == SelectedVisit.Visit_ID);
            if (existingExam == null)
            {
                await ShowDialog("Notice", "No existing examination found for this current visit to summarize.");
                return;
            }

            var summary = examRepository.GetExaminationSummary(existingExam.Exam_ID);
            if (summary == null)
            {
                await ShowDialog("Error", "Could not aggregate summary data to display.");
                return;
            }

            // Assign doctor name
            var doctor = mockStaffService.GetDoctorByID(summary.DoctorId);
            summary.AssignedDoctorName = $"{doctor.Name} ({doctor.Specialty})";

            var contentPanel = new Microsoft.UI.Xaml.Controls.StackPanel { Spacing = 10 };

            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Patient: {summary.FirstName} {summary.LastName}", FontWeight = Microsoft.UI.Text.FontWeights.Bold, FontSize = 16
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Arrival: {summary.ArrivalDateTime}  |  Chief Complaint: {summary.ChiefComplaint}"
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"\n--- TRIAGE DETAILS ---", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Level {summary.TriageLevel} ({summary.Specialization})\nSeverity Score: {summary.SeverityScore}\n" +
                       $"Vitals: C:{summary.Consciousness} Br:{summary.Breathing} Bl:{summary.Bleeding} Inj:{summary.InjuryType} Pn:{summary.PainLevel}"
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"\n--- EXAMINATION ---", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            contentPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Doctor: {summary.AssignedDoctorName}\nExam Time: {summary.ExamTime}\n\nNotes:\n{summary.Notes}",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            });

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Comprehensive Examination Summary",
                Content = contentPanel,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // Helper: WinUI 3 ContentDialog
        private async System.Threading.Tasks.Task ShowDialog(string title, string message)
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
}
