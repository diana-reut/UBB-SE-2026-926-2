using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERManagementSystem.Models
{
    public class Triage_Parameters
    {
        public int Triage_ID { get; set; }

        [Range(1, 3, ErrorMessage = "Consciousness must be between 1 and 3.")]
        public int Consciousness { get; set; }

        [Range(1, 3, ErrorMessage = "Breathing must be between 1 and 3.")]
        public int Breathing { get; set; }

        [Range(1, 3, ErrorMessage = "Bleeding must be between 1 and 3.")]
        public int Bleeding { get; set; }

        [Range(1, 3, ErrorMessage = "Injury_Type must be between 1 and 3.")]
        public int Injury_Type { get; set; }

        [Range(1, 3, ErrorMessage = "Pain_Level must be between 1 and 3.")]
        public int Pain_Level { get; set; }

        public Triage_Parameters()
        {
        }

        public Triage_Parameters(int triageId, int consciousness, int breathing, int bleeding, int injuryType, int painLevel)
        {
            Triage_ID = triageId;
            Consciousness = consciousness;
            Breathing = breathing;
            Bleeding = bleeding;
            Injury_Type = injuryType;
            Pain_Level = painLevel;

            ValidateParameters();
        }

        /// <summary>
        /// Validates that all parameters are within the allowed range (1–3).
        /// Throws ArgumentOutOfRangeException if any value is invalid.
        /// Values: 3 = most critical, 1 = normal.
        /// </summary>
        public void ValidateParameters()
        {
            if (Consciousness < 1 || Consciousness > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Consciousness), "Consciousness must be between 1 (normal) and 3 (most critical).");
            }

            if (Breathing < 1 || Breathing > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Breathing), "Breathing must be between 1 (normal) and 3 (most critical).");
            }

            if (Bleeding < 1 || Bleeding > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Bleeding), "Bleeding must be between 1 (normal) and 3 (most critical).");
            }

            if (Injury_Type < 1 || Injury_Type > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Injury_Type), "Injury_Type must be between 1 (normal) and 3 (most critical).");
            }

            if (Pain_Level < 1 || Pain_Level > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Pain_Level), "Pain_Level must be between 1 (normal) and 3 (most critical).");
            }
        }
    }
}
