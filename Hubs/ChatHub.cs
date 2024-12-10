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

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.RoomId.ToString()).SendAsync("ReceiveMessage", "MyChat Bot", $"{userConnection.UserId} has left");
                SendUsersConnected(userConnection.RoomId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(int roomId, string userId)
        {
            var userConnection = new UserConnection(Context.ConnectionId, userId, roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", "MyChat Bot", $"{userId} has joined the room");
            await SendUsersConnected(roomId);
        }

        //public async Task SendMessage(string message, string fileUrl = null)
        //{
        //    if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        //    {
        //        // Create a new message instance
        //        var newMessage = new Messages
        //        {
        //            Content = message,
        //            SentAt = DateTime.Now,
        //            IsPinned = false,
        //            UserId = userConnection.UserId,
        //            RoomId = userConnection.RoomId,
        //            IsRead = false,
        //            FileUrl = fileUrl ?? "" 
        //        };

        //        _context.Messages.Add(newMessage);

        //        try
        //        {
        //            await _context.SaveChangesAsync();


        //            await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
        //                "ReceiveMessage",
        //                new
        //                {
        //                    UserId = userConnection.UserId,
        //                    Content = message,
        //                    FileUrl = newMessage.FileUrl, 
        //                    SentAt = newMessage.SentAt
        //                });
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error saving message: {ex.Message}");
        //            await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
        //        }
        //    }
        //}
        public async Task SendMessage(string message, string fileUrl = null)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // Create a new message instance
                var newMessage = new Messages
                {
                    Content = message,
                    SentAt = DateTime.Now,
                    IsPinned = false,
                    UserId = userConnection.UserId,
                    RoomId = userConnection.RoomId,
                    IsRead = false,
                    FileUrl = fileUrl ?? ""
                };

                _context.Messages.Add(newMessage);

                try
                {
                    await _context.SaveChangesAsync();

                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
                        "ReceiveMessage",
                        new
                        {
                            UserId = userConnection.UserId,
                            Content = message,
                            FileUrl = fileUrl ?? "", // Ensure FileUrl is either a valid URL or an empty string
                            SentAt = newMessage.SentAt,
                            RoomId = userConnection.RoomId

                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving message: {ex.Message}");
                    await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
                }
            }
        }

        public async Task SendUsersConnected(int roomId)
        {
            var users = _connections.Values
                .Where(c => c.RoomId == roomId)
                .Select(c => c.UserId);

            await Clients.Group(roomId.ToString()).SendAsync("UsersInRoom", users);
        }

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
