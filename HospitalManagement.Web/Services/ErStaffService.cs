using Common.Data.Models;

namespace HospitalManagement.Web.Services;

public class ErStaffService : IErStaffService
{
    public int? RequestAvailableNurse() => 2;

    public ErDoctorAssignment RequestDoctor(string specialization, Triage_Parameters parameters)
    {
        int doctorId = specialization.Trim().ToLowerInvariant() switch
        {
            "orthopedics" => 102,
            "neurology" => 103,
            "pulmonology" => 105,
            "emergency medicine" => 106,
            "general surgery" => 104,
            "general" => 104,
            _ => 104
        };

        return GetDoctorById(doctorId);
    }

    public ErDoctorAssignment GetDoctorById(int doctorId) =>
        doctorId switch
        {
            102 => new ErDoctorAssignment(102, "Dr. Johnson", "Orthopedics"),
            103 => new ErDoctorAssignment(103, "Dr. Williams", "Neurology"),
            104 => new ErDoctorAssignment(104, "Dr. Brown", "General Medicine"),
            105 => new ErDoctorAssignment(105, "Dr. Taylor", "Pulmonology"),
            106 => new ErDoctorAssignment(106, "Dr. Evans", "Emergency Medicine"),
            _ => new ErDoctorAssignment(0, "Unknown", "Unknown")
        };
}
