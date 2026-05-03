using System;
using System.Collections.Generic;
using System.Linq;

namespace ERManagementSystem.Services
{
    /// <summary>
    /// Mock service simulating the external Staff Management system.
    /// </summary>
    public class NurseService
    {
        // Simulated external data source
        private readonly List<NurseMock> nurses;

        public NurseService()
        {
            // Hardcoded list based on "external system"
            nurses = new List<NurseMock>
            {
                new NurseMock { Nurse_ID = 1, Availability_Status = false },
                new NurseMock { Nurse_ID = 2, Availability_Status = true },
                new NurseMock { Nurse_ID = 3, Availability_Status = true }
            };
        }

        /// <summary>
        /// Simulates requesting an available nurse from the external system.
        /// Returns the first available nurse (availability_status = true).
        /// </summary>
        public int? RequestAvailableNurse()
        {
            var nurse = nurses.FirstOrDefault(n => n.Availability_Status);

            if (nurse == null)
            {
                return null;
            }

            return nurse.Nurse_ID;
        }

        /// <summary>
        /// Internal mock model representing external nurse data.
        /// </summary>
        private class NurseMock
        {
            public int Nurse_ID { get; set; }
            public bool Availability_Status { get; set; }
        }
    }
}