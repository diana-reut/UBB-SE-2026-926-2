using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Data.Models
{
    public class Triage_Parameters
    {
        public int TriageParametersId { get; set; }

        [JsonIgnore]
        public int TriageId { get; set; }

        [NotMapped]
        [JsonPropertyName("Triage_ID")]
        public int Triage_ID
        {
            get => TriageId;
            set => TriageId = value;
        }

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

        public void ValidateParameters()
        {
            if (Consciousness < 1 || Consciousness > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Consciousness), "Consciousness must be between 1 and 3.");
            }

            if (Breathing < 1 || Breathing > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Breathing), "Breathing must be between 1 and 3.");
            }

            if (Bleeding < 1 || Bleeding > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Bleeding), "Bleeding must be between 1 and 3.");
            }

            if (Injury_Type < 1 || Injury_Type > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Injury_Type), "Injury_Type must be between 1 and 3.");
            }

            if (Pain_Level < 1 || Pain_Level > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Pain_Level), "Pain_Level must be between 1 and 3.");
            }
        }
    }
}
