using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Data;
using System.Threading.Tasks;
using real_time_chat_web.Models;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _db;

    public ChatHub(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SendMessageToRoom(int roomId, string messageContent, string userName)
    {
        var message = new Messages
        {
            RoomId = roomId,
            Content = messageContent,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        // Gửi tin nhắn tới tất cả client trong phòng chat
        await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", new
        {
            messageId = message.MessageId,
            content = message.Content,
            sentAt = message.SentAt
        });
    }

    // Tham gia phòng chat
    public async Task JoinRoom(int roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    }

    // Rời khỏi phòng chat
    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
    }
}
