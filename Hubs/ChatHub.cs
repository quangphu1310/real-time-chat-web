using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace real_time_chat_web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IDictionary<string, UserConnection> _connections;
        private readonly IWebHostEnvironment _env;

        public ChatHub(ApplicationDbContext context, IDictionary<string, UserConnection> connections, IWebHostEnvironment env)
        {
            _context = context;
            _connections = connections;
            _env = env;
        }

        // This method is called when a client disconnects
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.RoomId.ToString()).SendAsync("ReceiveMessage", "MyChat Bot", $"{userConnection.UserId} has left");
                SendUsersConnected(userConnection.RoomId); // Updates the user list
            }

            return base.OnDisconnectedAsync(exception);
        }

        // Method to join a room
        public async Task JoinRoom(int roomId, string userId)
        {
            var userConnection = new UserConnection(Context.ConnectionId, userId, roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            _connections[Context.ConnectionId] = userConnection;

            // Notify other clients in the room
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", "MyChat Bot", $"{userId} has joined the room");
            await SendUsersConnected(roomId);
        }

        // Method to send a message
        public async Task SendMessage(string message, string? fileUrl = null)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // Create a new message object
                var newMessage = new Messages
                {
                    Content = message,
                    SentAt = DateTime.Now,
                    IsPinned = false,
                    UserId = userConnection.UserId,
                    RoomId = userConnection.RoomId,
                    IsRead = false,
                    FileUrl = fileUrl ?? "" // Default to an empty string if fileUrl is null
                };

                // Add the message to the context
                _context.Messages.Add(newMessage);

                try
                {
                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Send message to all clients in the room
                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
                        "ReceiveMessage",
                        new
                        {
                            UserId = userConnection.UserId,
                            Content = message,
                            FileUrl = fileUrl ?? "", // Ensure FileUrl is either a valid URL or an empty string
                            SentAt = newMessage.SentAt
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving message: {ex.Message}");
                    await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
                }
            }
        }


        // Method to send the list of users in the room
        public async Task SendUsersConnected(int roomId)
        {
            var users = _connections.Values
                .Where(c => c.RoomId == roomId)
                .Select(c => c.UserId);

            // Send the list of users in the room to all clients
            await Clients.Group(roomId.ToString()).SendAsync("UsersInRoom", users);
        }

        // Helper method to get user connection by connection ID
        private UserConnection GetUserConnection(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var userConnection)
                ? userConnection
                : null;
        }

        // Method to delete a message
        public async Task DeleteMessage(int messageId)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // Tìm tin nhắn trong database
                var message = await _context.Messages.FindAsync(messageId);
                if (message == null)
                {
                    // Gửi thông báo lỗi nếu tin nhắn không tồn tại
                    await Clients.Caller.SendAsync("Error", "Message not found.");
                    return;
                }

                // Kiểm tra xem người dùng có quyền xóa tin nhắn hay không
                if (message.UserId != userConnection.UserId)
                {
                    await Clients.Caller.SendAsync("Error", "You do not have permission to delete this message.");
                    return;
                }

                // Xóa tin nhắn
                _context.Messages.Remove(message);

                try
                {
                    // Lưu thay đổi vào database
                    await _context.SaveChangesAsync();

                    // Gửi thông báo cho tất cả các client trong phòng
                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
                        "MessageDeleted",
                        new { MessageId = messageId }
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting message: {ex.Message}");
                    await Clients.Caller.SendAsync("Error", "An error occurred while deleting the message.");
                }
            }
        }
    }
}
