namespace real_time_chat_web
{
    public class UserConnection
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }

        public UserConnection(string connectionId, string userId, int roomId)
        {
            ConnectionId = connectionId;
            UserId = userId;
            RoomId = roomId;
        }
    }




}
