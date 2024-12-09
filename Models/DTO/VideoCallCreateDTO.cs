using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models.DTO
{
    public class VideoCallCreateDTO
    {
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Rooms Rooms { get; set; }

        // Foreign key to Users table
        public string CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser User { get; set; }

        public string VideoCallUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // Ongoing, Ended
    }
}
