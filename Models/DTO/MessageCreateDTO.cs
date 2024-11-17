using System;

namespace real_time_chat_web.Models.DTO
{
    public class MessageCreateDTO
    {
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; } = false;

        public int RoomId { get; set; }
    }
}
