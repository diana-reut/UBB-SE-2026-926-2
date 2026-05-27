namespace HospitalManagement.Web.Models.Patient;

public class PatientProfileViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Cnp { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsHighRisk { get; set; }
    public string BloodType { get; set; } = "N/A";
    public string Rh { get; set; } = "N/A";
    public string ChronicConditions { get; set; } = "None";
    public List<string> Allergies { get; set; } = new ();
    public List<PatientRecordViewModel> MedicalRecords { get; set; } = new ();
    public int? SelectedRecordId { get; set; }
    public PatientRecordViewModel? SelectedRecord { get; set; }
    public PatientPrescriptionViewModel? SelectedPrescription { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public int? TemporaryDiscount { get; set; }
    public decimal? TemporaryDiscountedPrice { get; set; }
}

public class PatientRecordViewModel
{
    public int Id { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string Symptoms { get; set; } = "N/A";
    public string Diagnosis { get; set; } = "N/A";
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
}

public class PatientPrescriptionViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string DoctorNotes { get; set; } = "None";
    public List<PatientPrescriptionItemViewModel> Items { get; set; } = new ();
}

public class PatientPrescriptionItemViewModel
{
    public string MedicationName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
