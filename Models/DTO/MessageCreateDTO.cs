using System;

namespace real_time_chat_web.Models.DTO
{
    public class MessageCreateDTO
    {
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }
        public string FileUrl { get; set; } 
    }
}
