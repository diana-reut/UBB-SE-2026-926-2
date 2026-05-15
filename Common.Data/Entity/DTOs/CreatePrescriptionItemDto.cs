namespace Common.Data.Entity.DTOs;

public class CreatePrescriptionItemDto
{
    public string MedName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
}
