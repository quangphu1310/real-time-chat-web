﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models
{
    public class Messages
    {
        [Key]
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsPinned { get; set; } = false;
        public string? FileUrl { get; set; } // Optional URL field
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Rooms Room { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
