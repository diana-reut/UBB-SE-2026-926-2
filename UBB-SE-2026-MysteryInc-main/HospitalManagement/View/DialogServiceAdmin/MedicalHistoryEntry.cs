using HospitalManagement.Entity;

namespace HospitalManagement.View.DialogServiceAdmin;

internal class MedicalHistoryEntry
{
    public MedicalHistory? History { get; set; } = null;

    public bool WasSkipped { get; set; }

}
