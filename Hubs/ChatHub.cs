using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Models;
using System.Text.RegularExpressions;

public class ChatHub : Hub
{
    private readonly IRoomService _roomService;

    public ChatHub(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task SendMessage(string roomId, string message)
    {
        // Add message to database, potentially pinning it if required
        var msg = new Messages { RoomId = roomId, Content = message, UserId = Context.UserIdentifier };
        await _roomService.AddMessageAsync(msg);

        // Broadcast message to room
        await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    // Method to delete message for Moderators
    public async Task DeleteMessage(string messageId)
    {
        var message = await _roomService.GetMessageByIdAsync(messageId);
        if (message != null && (Context.User.IsInRole("Admin") || Context.User.IsInRole("Moderate")))
        {
            await _roomService.DeleteMessageAsync(message);
            await Clients.All.SendAsync("MessageDeleted", messageId);
        }
    }
}
