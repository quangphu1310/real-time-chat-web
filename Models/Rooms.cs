
﻿using System.ComponentModel.DataAnnotations;
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

        public int MessageId { get; set; }
        [ForeignKey("MessageId")]
        public Messages mess { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser User { get; set; }

        public ICollection<RoomsUser> RoomsUsers { get; set; }
        public ICollection<VideoCall> VideoCalls { get; set; }
        public ICollection<Messages> Messages { get; set; }
    }
}
