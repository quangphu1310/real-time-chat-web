using System;

namespace real_time_chat_web.Models.DTO
{
    public class MessageGetDTO
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsPinned { get; set; }
        public string FileUrl { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } 
        public int RoomId { get; set; }
        public Rooms Room { get; set; }
        public bool IsRead { get; set; }
    }
}
