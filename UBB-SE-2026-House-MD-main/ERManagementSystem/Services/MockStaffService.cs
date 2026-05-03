using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    using System;
    public class MockStaffService
    {
        // Simulates requesting a doctor from Staff Management system
        public int RequestDoctor(string specialization, Triage_Parameters symptoms)
        {
            // Hardcoded doctor assignment based on specialization
            return specialization?.Trim().ToLowerInvariant() switch
            {
                "orthopedics" => 102,
                "neurology" => 103,
                "pulmonology" => 105,
                "emergency medicine" => 106,
                "general surgery" => 104,
                "general" => 104,
                _ => 104 // Default to General practitioner
            };
        }

        public Doctor GetDoctorByID(int doctorID)
        {
            return doctorID switch
            {
                102 => new Doctor { DoctorID = 102, Name = "Dr. Johnson", Specialty = "Orthopedics" },
                103 => new Doctor { DoctorID = 103, Name = "Dr. Williams", Specialty = "Neurology" },
                104 => new Doctor { DoctorID = 104, Name = "Dr. Brown", Specialty = "General Medicine" },
                105 => new Doctor { DoctorID = 105, Name = "Dr. Taylor", Specialty = "Pulmonology" },
                106 => new Doctor { DoctorID = 106, Name = "Dr. Evans", Specialty = "Emergency Medicine" },
                _ => new Doctor { DoctorID = 0, Name = "Unknown", Specialty = "Unknown" }
            };
        }
    }
}