namespace real_time_chat_web.Models.DTO
{
    public class RoomsUserCreateDTO
    {
        public int IdRooms { get; set; }
        public List<string> IdUser { get; set; }
        public string IdPerAdd { get; set; }
    }
}
