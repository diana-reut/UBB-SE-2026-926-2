namespace HospitalManagement.Web.Models.Consultations;

public class DiscountRouletteViewModel
{
    public int PatientId { get; set; }
    public int RecordId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
}

public class DiscountAppliedResult
{
    public int Discount { get; set; }
    public decimal FinalPrice { get; set; }
}
