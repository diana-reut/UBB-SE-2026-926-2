using System.Collections.Generic;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ERManagementSystem.Views
{
    public sealed partial class VisitDetailsPanel : UserControl
    {
        public VisitDetailsPanel()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PatientProperty =
            DependencyProperty.Register(nameof(Patient), typeof(Patient), typeof(VisitDetailsPanel), new PropertyMetadata(null, OnDataChanged));

        public Patient? Patient
        {
            get => (Patient?)GetValue(PatientProperty);
            set => SetValue(PatientProperty, value);
        }

        public static readonly DependencyProperty VisitProperty =
            DependencyProperty.Register(nameof(Visit), typeof(ER_Visit), typeof(VisitDetailsPanel), new PropertyMetadata(null, OnDataChanged));

        public ER_Visit? Visit
        {
            get => (ER_Visit?)GetValue(VisitProperty);
            set => SetValue(VisitProperty, value);
        }

        public static readonly DependencyProperty TriageProperty =
            DependencyProperty.Register(nameof(Triage), typeof(Triage), typeof(VisitDetailsPanel), new PropertyMetadata(null, OnDataChanged));

        public Triage? Triage
        {
            get => (Triage?)GetValue(TriageProperty);
            set => SetValue(TriageProperty, value);
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VisitDetailsPanel panel)
            {
                panel.LoadVisit(panel.Patient, panel.Visit, panel.Triage);
            }
        }

        // ─────────────────────────────────────────────
        // Public method — call this to load visit data.
        // Pass triage as null if not yet triaged.
        // ─────────────────────────────────────────────
        public void LoadVisit(Patient? patient, ER_Visit? visit, Triage? triage = null)
        {
            if (patient == null || visit == null)
            {
                ClearPanel();
                return;
            }

            // Patient
            FullNameText.Text = $"{patient.First_Name} {patient.Last_Name}";
            PatientIdText.Text = patient.Patient_ID;
            DateOfBirthText.Text = patient.Date_of_Birth.ToString("dd/MM/yyyy");
            GenderText.Text = patient.Gender;
            PhoneText.Text = patient.Phone;
            EmergencyContactText.Text = patient.Emergency_Contact;

            // Visit
            VisitIdText.Text = visit.Visit_ID.ToString();
            ArrivalText.Text = visit.Arrival_date_time.ToString("dd/MM/yyyy HH:mm");
            ChiefComplaintText.Text = visit.Chief_Complaint;

            // Triage — show card only if triage exists
            if (triage != null)
            {
                TriageCard.Visibility = Visibility.Visible;
                TriageLevelText.Text = $"Level {triage.Triage_Level}";
                SpecializationText.Text = triage.Specialization;
                NurseIdText.Text = triage.Nurse_ID.ToString();
                TriageTimeText.Text = triage.Triage_Time.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                TriageCard.Visibility = Visibility.Collapsed;
            }

            // Timeline - now delegated to service
            TimelineItems.ItemsSource = VisitTimelineService.BuildTimeline(visit.Status);
        }

        // ─────────────────────────────────────────────
        // Clear.
        // ─────────────────────────────────────────────
        private void ClearPanel()
        {
            FullNameText.Text = string.Empty;
            PatientIdText.Text = string.Empty;
            DateOfBirthText.Text = string.Empty;
            GenderText.Text = string.Empty;
            PhoneText.Text = string.Empty;
            EmergencyContactText.Text = string.Empty;
            VisitIdText.Text = string.Empty;
            ArrivalText.Text = string.Empty;
            ChiefComplaintText.Text = string.Empty;
            TriageCard.Visibility = Visibility.Collapsed;
            TimelineItems.ItemsSource = null;
        }
    }

    // ─────────────────────────────────────────────
    // Timeline item helper class.
    // ─────────────────────────────────────────────
    public class TimelineItem
    {
        public string Label { get; set; } = string.Empty;
        public SolidColorBrush DotColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush TextColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public string Weight { get; set; } = "Normal";
        public Visibility BadgeVisibility { get; set; } = Visibility.Collapsed;
    }
}