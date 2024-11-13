using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models
{
    public class Rooms
    {
        [Key]
        public int IdRooms { get; set; }
        public string RoomName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser User { get; set; }
    }
}
