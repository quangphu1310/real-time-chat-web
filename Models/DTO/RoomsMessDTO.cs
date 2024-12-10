namespace real_time_chat_web.Models.DTO
{
    public class RoomsMessDTO
    {
        public int IdRooms { get; set; }
        public string RoomName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastMessageContent { get; set; }
        public string IdPerMessLast { get; set; }
        public string NamePerMessLast { get; set; }
        public DateTime? LastMessageSentAt { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }
}
