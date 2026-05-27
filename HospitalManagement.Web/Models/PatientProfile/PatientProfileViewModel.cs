using Common.Data.Entity;

namespace HospitalManagement.Web.Models.PatientProfile;

public class PatientProfileViewModel
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty;
    public string? BloodType { get; set; }
    public string? Rh { get; set; }
    public string ChronicConditions { get; set; } = "None";
    public List<string> Allergies { get; set; } = new ();
    public List<MedicalRecordRowViewModel> MedicalRecords { get; set; } = new ();
    public int? SelectedRecordId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
    public PrescriptionViewModel? Prescription { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public static PatientProfileViewModel FromModel(PatientProfileModel model)
    {
        var viewModel = new PatientProfileViewModel
        {
            PatientId = model.PatientId,
            FullName = $"{model.FirstName} {model.LastName}",
            Dob = model.Dob.ToString("yyyy-MM-dd"),
            BloodType = model.BloodType,
            Rh = model.Rh,
            ChronicConditions = model.ChronicConditionsFormatted,
            Allergies = model.Allergies,
            SelectedRecordId = model.SelectedRecordId,
            BasePrice = model.BasePrice,
            FinalPrice = model.FinalPrice,
            DiscountApplied = model.DiscountApplied,
            MedicalRecords = model.MedicalRecords
                .OrderByDescending(record => record.ConsultationDate)
                .Select(record => new MedicalRecordRowViewModel
                {
                    Id = record.Id,
                    ConsultationDate = record.ConsultationDate.ToString("yyyy-MM-dd"),
                    SourceType = record.SourceType.ToString(),
                    StaffId = record.StaffId,
                    Symptoms = record.Symptoms ?? "N/A",
                    Diagnosis = record.Diagnosis ?? "N/A",
                    BasePrice = record.BasePrice,
                    FinalPrice = record.FinalPrice,
                    DiscountApplied = record.DiscountApplied,
                    IsSelected = record.Id == model.SelectedRecordId,
                })
                .ToList(),
        };

        if (model.SelectedPrescription is not null)
        {
            viewModel.Prescription = new PrescriptionViewModel
            {
                Id = model.SelectedPrescription.Id,
                Date = model.SelectedPrescription.Date.ToString("yyyy-MM-dd"),
                DoctorNotes = model.SelectedPrescription.DoctorNotes,
                Items = model.SelectedPrescription.MedicationList
                    .Select(item => new PrescriptionItemViewModel
                    {
                        MedName = item.MedName,
                        Quantity = item.Quantity,
                    })
                    .ToList(),
            };
        }

        return viewModel;
    }
}

public class MedicalRecordRowViewModel
{
    public int Id { get; set; }
    public string ConsultationDate { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int? DiscountApplied { get; set; }
    public bool IsSelected { get; set; }
}

public class PrescriptionViewModel
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? DoctorNotes { get; set; }
    public List<PrescriptionItemViewModel> Items { get; set; } = new ();
}

public class PrescriptionItemViewModel
{
    public string MedName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}
