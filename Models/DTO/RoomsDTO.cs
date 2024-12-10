namespace real_time_chat_web.Models.DTO
{
    public class RoomsDTO
    {
        public int IdRooms { get; set; }
        public string RoomName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastMessageContent { get; set; }
        public DateTime? LastMessageSentAt { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }
}
