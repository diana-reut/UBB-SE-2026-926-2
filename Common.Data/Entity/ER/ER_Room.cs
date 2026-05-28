using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Data.Models
{
    public class ER_Room
    {
        public int Room_ID { get; set; }
        public string Room_Type { get; set; } = string.Empty;
        public string Availability_Status { get; set; } = RoomStatus.Available;
        public int? Current_Visit_ID { get; set; }

        public static class RoomStatus
        {
            public const string Available = "Available";
            public const string Occupied = "Occupied";
            public const string Cleaning = "Cleaning";
        }

        public static class RoomType
        {
            public const string OperatingRoom = "Operating Room (OR)";
            public const string TraumaBay = "Trauma/Resuscitation Bay";
            public const string RespiratoryRoom = "Respiratory/Monitored Room";
            public const string NeurologyRoom = "Neurology/Quiet Observation Room";
            public const string OrthopedicRoom = "Orthopedic/Procedure Room";
            public const string GeneralRoom = "General Examination Room";
        }

        public static readonly IReadOnlyList<string> AllowedStatuses = new[]
        {
            RoomStatus.Available,
            RoomStatus.Occupied,
            RoomStatus.Cleaning
        };

        private static readonly Dictionary<string, string> ValidTransitions = new ()
        {
            { RoomStatus.Available, RoomStatus.Occupied },
            { RoomStatus.Occupied,  RoomStatus.Cleaning },
            { RoomStatus.Cleaning,  RoomStatus.Available }
        };

        public static bool StatusEquals(string? left, string? right)
            => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        public static string NormalizeStatus(string? status)
        {
            string? knownStatus = AllowedStatuses.FirstOrDefault(allowedStatus => StatusEquals(allowedStatus, status));
            return knownStatus ?? status ?? string.Empty;
        }

        public void UpdateAvailabilityStatus(string newStatus)
        {
            string normalizedCurrentStatus = NormalizeStatus(Availability_Status);
            string normalizedNewStatus = NormalizeStatus(newStatus);

            if (!AllowedStatuses.Any(status => StatusEquals(status, normalizedNewStatus)))
            {
                throw new ArgumentException(
                    $"'{newStatus}' is not a valid room status. " +
                    $"Allowed values: {string.Join(", ", AllowedStatuses)}.");
            }

            if (!ValidTransitions.TryGetValue(normalizedCurrentStatus, out string? expectedNext)
                || !StatusEquals(expectedNext, normalizedNewStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid room status transition: cannot move Room {Room_ID} " +
                    $"from '{Availability_Status}' to '{newStatus}'. " +
                    $"Expected next status: '{(ValidTransitions.TryGetValue(normalizedCurrentStatus, out var next) ? next : "none")}'.");
            }

            Availability_Status = normalizedNewStatus;
        }

        public override string ToString() =>
            $"[Room {Room_ID}] Type: {Room_Type} | Status: {Availability_Status}";
    }
}
