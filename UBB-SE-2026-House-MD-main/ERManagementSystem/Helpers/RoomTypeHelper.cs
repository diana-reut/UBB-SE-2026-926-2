using Common.Data.Models;
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
            // 1. Surgical emergencies take highest priority.
            if (specialization == "General Surgery" || bleeding == 3 || injuryType == 3)
            {
                return ER_Room.RoomType.OperatingRoom;
            }

            // 2. Critical airway / consciousness cases use trauma bays.
            if (consciousness == 3 || breathing == 3)
            {
                return ER_Room.RoomType.TraumaBay;
            }

            // 3. Non-critical specialization-specific routing.
            if (specialization == "Pulmonology" || breathing == 2)
            {
                return ER_Room.RoomType.RespiratoryRoom;
            }

            if (specialization == "Neurology" || consciousness == 2)
            {
                return ER_Room.RoomType.NeurologyRoom;
            }

            if (specialization == "Orthopedics" || injuryType == 2)
            {
                return ER_Room.RoomType.OrthopedicRoom;
            }

            // 4. Standard / general cases.
            return ER_Room.RoomType.GeneralRoom;
        }
    }
}
