using System.Collections.Generic;

namespace HospitalManagement.Entity.DTOs;

internal static class MockDoctorProvider
{
    public static List<DoctorDTO> FakeDoctors
        => [
            new DoctorDTO { DoctorId = 1, FirstName = "Gregory", LastName = "House", Specialization = "Diagnostician", },
            new DoctorDTO { DoctorId = 2, FirstName = "James", LastName = "Wilson", Specialization = "Oncology", },
            new DoctorDTO { DoctorId = 3, FirstName = "Lisa", LastName = "Cuddy", Specialization = "Endocrinology", },
            new DoctorDTO { DoctorId = 4, FirstName = "Meredith", LastName = "Grey", Specialization = "General Surgery", }
        ];
}
