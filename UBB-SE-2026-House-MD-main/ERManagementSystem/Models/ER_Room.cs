using System;
using System.Collections.Generic;
using System.Linq;

namespace ERManagementSystem.Models
{
    public class ER_Room
    {
        public int Room_ID { get; set; }
        public string Room_Type { get; set; } = string.Empty;
        public string Availability_Status { get; set; } = RoomStatus.Available;

        /// <summary>
        /// The Visit_ID currently occupying this room.
        /// Set by RoomAssignmentService.AssignRoomToVisit(), cleared when room → cleaning.
        /// Null when the room is available or cleaning.
        /// </summary>
        public int? Current_Visit_ID { get; set; }

        public static class RoomStatus
        {
            public const string Available = "available";
            public const string Occupied = "occupied";
            public const string Cleaning = "cleaning";
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

        public void UpdateAvailabilityStatus(string newStatus)
        {
            if (!AllowedStatuses.Contains(newStatus))
            {
                throw new ArgumentException(
                    $"'{newStatus}' is not a valid room status. " +
                    $"Allowed values: {string.Join(", ", AllowedStatuses)}.");
            }

            if (!ValidTransitions.TryGetValue(Availability_Status, out string? expectedNext)
                || expectedNext != newStatus)
            {
                throw new InvalidOperationException(
                    $"Invalid room status transition: cannot move Room {Room_ID} " +
                    $"from '{Availability_Status}' to '{newStatus}'. " +
                    $"Expected next status: '{(ValidTransitions.TryGetValue(Availability_Status, out var next) ? next : "none")}'.");
            }

            Availability_Status = newStatus;
        }

        public override string ToString() =>
            $"[Room {Room_ID}] Type: {Room_Type} | Status: {Availability_Status}";
    }
}
