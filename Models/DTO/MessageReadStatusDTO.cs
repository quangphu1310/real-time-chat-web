namespace real_time_chat_web.Models.DTO
{
    public class MessageReadStatusDTO
    {
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; }
    }
}
