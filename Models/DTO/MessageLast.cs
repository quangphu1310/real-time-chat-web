namespace real_time_chat_web.Models.DTO
{
    public class MessageLast
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }
    }
}
