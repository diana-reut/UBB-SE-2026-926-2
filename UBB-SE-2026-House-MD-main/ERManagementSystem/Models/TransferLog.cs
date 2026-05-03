using System;

namespace ERManagementSystem.Models
{
    /// <summary>
    /// Task 6.1 - Transfer_Log model.
    /// Task 6.5 - FilePath stores the path to the saved JSON file.
    ///
    /// DB columns: Transfer_ID, Visit_ID, Transfer_Time, Target_System, Status
    /// Status must be: "SUCCESS", "FAILED", or "RETRYING"
    /// Target_System must be: "Patient Management"
    /// </summary>
    public class Transfer_Log
    {
        public int Transfer_ID { get; set; }
        public int Visit_ID { get; set; }
        public DateTime Transfer_Time { get; set; }

        public string Target_System { get; set; } = string.Empty;

        // Task 6.5: path to the saved JSON file on local disk
        public string? FilePath { get; set; }

        // schema.sql: Status CHECK IN ('SUCCESS','FAILED','RETRYING')
        private string status = "RETRYING";
        public string Status
        {
            get => status;
            set
            {
                if (value != "SUCCESS" && value != "FAILED" && value != "RETRYING")
                {
                    throw new ArgumentException(
                        $"Invalid status '{value}'. Allowed: SUCCESS, FAILED, RETRYING.");
                }

                status = value;
            }
        }

        /// <summary>
        /// Validates all required fields. Throws ArgumentException if any rule is violated.
        /// Task 6.1: Target_system must not be empty, Visit_ID must be valid,
        /// Status must be one of the three allowed values.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Target_System))
            {
                throw new ArgumentException("Target_System must not be empty.");
            }

            if (Visit_ID <= 0)
            {
                throw new ArgumentException("Visit_ID must be a valid positive integer.");
            }

            if (Status != "SUCCESS" && Status != "FAILED" && Status != "RETRYING")
            {
                throw new ArgumentException("Status must be SUCCESS, FAILED, or RETRYING.");
            }
        }
    }
}