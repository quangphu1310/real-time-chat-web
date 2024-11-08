using System.ComponentModel.DataAnnotations.Schema;

namespace real_time_chat_web.Models.DTO
{
    public class MessageCreateDTO
    {
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsPinned { get; set; }
        public string FileUrl { get; set; }
        public string UserId { get; set; }
     
        public int RoomId { get; set; }
      
        // Thêm cột IsRead
        public bool IsRead { get; set; } = false; 
    }
}
