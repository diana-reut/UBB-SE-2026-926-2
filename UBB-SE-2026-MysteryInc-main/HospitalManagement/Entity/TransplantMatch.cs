using System;

namespace HospitalManagement.Entity;

/// <summary>
/// View model representation of a transplant match for display in the DataGrid.
/// </summary>
internal class TransplantMatch
{
    public int TransplantId { get; set; }

    public int ReceiverId { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string BloodType { get; set; } = null!;

    public float CompatibilityScore { get; set; }

    public DateTime RequestDate { get; set; }

    public int WaitingDays { get; set; }
}
