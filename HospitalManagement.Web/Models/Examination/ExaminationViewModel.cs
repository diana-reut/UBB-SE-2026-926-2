using System.ComponentModel.DataAnnotations;
using Common.Data.Entity.DTOs;

namespace HospitalManagement.Web.Models.Examination;

public class ExaminationViewModel
{
    public int? SelectedVisitId { get; set; }
    public List<ExaminationVisitViewModel> EligibleVisits { get; set; } = new ();
    public ExaminationVisitViewModel? SelectedVisit { get; set; }
    public ExaminationTriageViewModel? TriageDetails { get; set; }
    public List<ExaminationHistoryItemViewModel> ExaminationHistory { get; set; } = new ();
    public ExaminationFormViewModel Form { get; set; } = new ();
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public bool CanRequestDoctor { get; set; }
    public bool CanSaveExamination { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ExaminationVisitViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ExaminationTriageViewModel
{
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public int Consciousness { get; set; }
    public int Breathing { get; set; }
    public int Bleeding { get; set; }
    public int InjuryType { get; set; }
    public int PainLevel { get; set; }
}

public class ExaminationHistoryItemViewModel
{
    public int ExamId { get; set; }
    public int VisitId { get; set; }
    public int DoctorId { get; set; }
    public DateTime ExamTime { get; set; }
    public int RoomId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ExaminationFormViewModel
{
    [Required]
    public int VisitId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Doctor ID is required.")]
    public int DoctorId { get; set; }

    [Required]
    public string Notes { get; set; } = string.Empty;
}

public class ExaminationSummaryViewModel
{
    public int VisitId { get; set; }
    public ERExaminationSummaryDto Summary { get; set; } = new ();
}
