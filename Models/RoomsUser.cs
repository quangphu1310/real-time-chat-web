using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models
{
    public class RoomsUser
    {
        public int IdRooms { get; set; }
        public string IdUser { get; set; }
        public string IdPerAdd { get; set; }
        [ForeignKey("IdRooms")]
        public Rooms Rooms { get; set; }
        [ForeignKey("IdUser")]
        public ApplicationUser User { get; set; }
        [ForeignKey("IdPerAdd")]
        public ApplicationUser PerUser { get; set; }

        public DateTime DayAdd { get; set; }
    }
}
