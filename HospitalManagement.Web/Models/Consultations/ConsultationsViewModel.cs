using Common.Data.Entity.Enums;

namespace HospitalManagement.Web.Models.Consultations;

public class ConsultationDetailsViewModel
{
    public int RecordId { get; set; }
    public int PatientId { get; set; }
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientFullName => $"{PatientFirstName} {PatientLastName}";

    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public DateTime ConsultationDate { get; set; }

    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;

    // Billing
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
    public bool IsDiscountApplied => DiscountApplied.HasValue;

    // For back-navigation
    public bool IsArchived { get; set; }
}