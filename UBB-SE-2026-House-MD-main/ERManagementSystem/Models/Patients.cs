using System;
using System.Collections.Generic;
using System.Linq;

namespace ERManagementSystem.Models
{
	public class Patient
	{
		public string Patient_ID { get; set; } = string.Empty;

        public string First_Name { get; set; } = string.Empty;

       public string Last_Name { get; set; } = string.Empty;

        public DateTime Date_of_Birth { get; set; }

		public string Gender { get; set; } = string.Empty;

       public string Phone { get; set; } = string.Empty;

       public string Emergency_Contact { get; set; } = string.Empty;

		public bool Transferred { get; set; } = false;

		public static readonly IReadOnlyList<string> AllowedGenders =
			new[] { "Male", "Female" };

		public bool Validate(out List<string> errors)
		{
			errors = new List<string>();

            // Patient_ID (CNP)
			if (string.IsNullOrWhiteSpace(Patient_ID))
            {
                errors.Add("Patient ID (CNP) is required.");
            }
            else if (Patient_ID.Length > 20)
            {
                errors.Add("Patient ID (CNP) must not exceed 20 characters.");
            }

            // First Name
            if (string.IsNullOrWhiteSpace(First_Name))
            {
                errors.Add("First name is required.");
            }
            else if (First_Name.Length > 50)
            {
                errors.Add("First name must not exceed 50 characters.");
            }

            // Last Name
            if (string.IsNullOrWhiteSpace(Last_Name))
            {
                errors.Add("Last name is required.");
            }
            else if (Last_Name.Length > 50)
            {
                errors.Add("Last name must not exceed 50 characters.");
            }

            // Date of Birth – must be in the past
            if (Date_of_Birth == default)
            {
                errors.Add("Date of birth is required.");
            }
            else if (Date_of_Birth >= DateTime.Today)
            {
                errors.Add("Date of birth must be in the past.");
            }

            // Gender
            if (string.IsNullOrWhiteSpace(Gender))
            {
                errors.Add("Gender is required.");
            }
            else if (!AllowedGenders.Contains(Gender))
            {
                errors.Add($"Gender must be one of: {string.Join(", ", AllowedGenders)}.");
            }

            // Phone
            if (string.IsNullOrWhiteSpace(Phone))
            {
                errors.Add("Phone number is required.");
            }
            else if (Phone.Length > 20)
            {
                errors.Add("Phone number must not exceed 20 characters.");
            }

            // Emergency Contact
            if (string.IsNullOrWhiteSpace(Emergency_Contact))
            {
                errors.Add("Emergency contact is required.");
            }
            else if (Emergency_Contact.Length > 100)
            {
                errors.Add("Emergency contact must not exceed 100 characters.");
            }

            return errors.Count == 0;
		}
	}
}