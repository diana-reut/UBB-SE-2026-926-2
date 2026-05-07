using System;

namespace Common.Data.Models
{
    public class Transfer_Log
    {
        public int Transfer_ID { get; set; }
        public int Visit_ID { get; set; }
        public DateTime Transfer_Time { get; set; }
        public string Target_System { get; set; } = string.Empty;
        public string? FilePath { get; set; }

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
        }
    }
}
