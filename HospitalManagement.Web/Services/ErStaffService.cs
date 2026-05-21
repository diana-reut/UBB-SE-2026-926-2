using Common.Data.Models;

namespace HospitalManagement.Web.Services;

public class ErStaffService : IErStaffService
{
    public int? RequestAvailableNurse() => 2;

    public int CalculateTriageLevel(Triage_Parameters parameters)
    {
        parameters.ValidateParameters();

        if (parameters.Consciousness == 3 ||
            parameters.Breathing == 3 ||
            parameters.Injury_Type == 3 ||
            parameters.Bleeding == 3)
        {
            return 1;
        }

        int severityScore =
            (parameters.Consciousness * 3) +
            (parameters.Breathing * 3) +
            (parameters.Bleeding * 2) +
            (parameters.Injury_Type * 2) +
            parameters.Pain_Level;

        if (severityScore >= 20)
        {
            return 2;
        }

        if (severityScore >= 16)
        {
            return 3;
        }

        if (severityScore >= 12)
        {
            return 4;
        }

        return 5;
    }

    public string DetermineSpecialization(Triage_Parameters parameters)
    {
        if (parameters.Bleeding == 3 || parameters.Injury_Type == 3)
        {
            return "General Surgery";
        }

        if (parameters.Injury_Type == 2)
        {
            return "Orthopedics";
        }

        if (parameters.Breathing == 2)
        {
            return "Pulmonology";
        }

        if (parameters.Consciousness == 2 || parameters.Consciousness == 3)
        {
            return "Neurology";
        }

        return "Emergency Medicine";
    }

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
