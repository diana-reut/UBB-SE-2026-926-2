namespace HospitalManagement.Entity.DTOs;

internal class DoctorDTO
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Specialization { get; set; } = "";
}
