using System;

namespace real_time_chat_web.Models.DTO
{
    public class MessageUpdateDTO
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public string FileUrl { get; set; } // Có thể cập nhật file đính kèm nếu cần
    }
}
