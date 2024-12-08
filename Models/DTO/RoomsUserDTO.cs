using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models.DTO
{
    public class RoomsUserDTO
    {
        public int IdRooms { get; set; }
        public string IdUser { get; set; }
        public string IdPerAdd { get; set; }
        public string RoomName { get; set; }
        public string UserName { get; set; }
        public string PerUserName { get; set; }
        public DateTime DayAdd { get; set; }
    }
}
