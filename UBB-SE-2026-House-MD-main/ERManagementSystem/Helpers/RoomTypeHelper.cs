using ERManagementSystem.Models;

namespace ERManagementSystem.Helpers
{
    public static class RoomTypeHelper
    {
        public static string DetermineRoomType(
            string? specialization,
            int bleeding,
            int injuryType,
            int consciousness,
            int breathing)
        {
            // 1. Strict Specializations assigned by Triage take utmost priority
            if (specialization == "General Surgery")
            {
                return ER_Room.RoomType.OperatingRoom;
            }

            if (specialization == "Neurology")
            {
                return ER_Room.RoomType.NeurologyRoom;
            }

            if (specialization == "Pulmonology")
            {
                return ER_Room.RoomType.RespiratoryRoom;
            }

            if (specialization == "Orthopedics")
            {
                return ER_Room.RoomType.OrthopedicRoom;
            }

            // 2. Fallbacks for 'Emergency Medicine' based on critical vitals
            if (consciousness == 3 || breathing == 3 || bleeding == 3 || injuryType == 3)
            {
                return ER_Room.RoomType.TraumaBay;
            }

            if (consciousness == 2)
            {
                return ER_Room.RoomType.NeurologyRoom;
            }

            if (breathing == 2)
            {
                return ER_Room.RoomType.RespiratoryRoom;
            }

            if (injuryType == 2)
            {
                return ER_Room.RoomType.OrthopedicRoom;
            }

            return ER_Room.RoomType.GeneralRoom;
        }
    }
}
