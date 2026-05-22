using Common.Data.Models;

namespace HospitalManagement.Web.Services;

public interface IErStaffService
{
    int? RequestAvailableNurse();
    ErDoctorAssignment RequestDoctor(string specialization, Triage_Parameters parameters);
    ErDoctorAssignment GetDoctorById(int doctorId);
}

public sealed record ErDoctorAssignment(int DoctorId, string Name, string Specialty);
