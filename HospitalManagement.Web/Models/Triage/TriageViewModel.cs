using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models.Triage;

public class TriageViewModel
{
    public int? SelectedVisitId { get; set; }
    public List<TriageVisitViewModel> Visits { get; set; } = new ();
    public TriageFormViewModel Form { get; set; } = new ();
    public TriageResultViewModel? SelectedTriage { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TriageVisitViewModel
{
    public int VisitId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? TriageLevel { get; set; }
    public string? Specialization { get; set; }
}

public class TriageFormViewModel
{
    [Required]
    public int VisitId { get; set; }

    [Range(1, 3)]
    public int Consciousness { get; set; }

    [Range(1, 3)]
    public int Breathing { get; set; }

    [Range(1, 3)]
    public int Bleeding { get; set; }

    [Range(1, 3)]
    public int InjuryType { get; set; }

    [Range(1, 3)]
    public int PainLevel { get; set; }
}

public class TriageResultViewModel
{
    public int TriageId { get; set; }
    public int TriageLevel { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int NurseId { get; set; }
    public DateTime TriageTime { get; set; }
}
