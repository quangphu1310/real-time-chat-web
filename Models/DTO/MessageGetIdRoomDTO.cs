using System;

namespace real_time_chat_web.Models.DTO
{
    public class MessageGetIdRoomDTO
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } 
        public bool IsRead { get; set; }
    }
}
