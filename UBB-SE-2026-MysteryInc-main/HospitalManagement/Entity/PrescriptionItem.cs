namespace HospitalManagement.Entity;

internal class PrescriptionItem
{
    public int PrescrItemId { get; set; }

    public int PrescriptionId { get; set; }

    public string MedName { get; set; } = "";

    public string? Quantity { get; set; }
}
