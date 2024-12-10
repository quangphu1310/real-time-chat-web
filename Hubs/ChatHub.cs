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
        public async Task SendMessage(string message, string fileUrl = null)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // Kiểm tra nếu cả message và fileUrl đều rỗng, không thực hiện thêm vào cơ sở dữ liệu
                if (string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "Cannot send an empty message or file.");
                    return;
                }

                //        // Tạo một thực thể message mới
                //        var newMessage = new Messages
                //        {
                //            Content = message?.Trim(),
                //            SentAt = DateTime.Now,
                //            IsPinned = false,
                //            UserId = userConnection.UserId,
                //            RoomId = userConnection.RoomId,
                //            IsRead = false,
                //            FileUrl = fileUrl?.Trim() ?? ""
                //        };

                //        _context.Messages.Add(newMessage);

                //        try
                //        {
                //            // Lưu tin nhắn vào cơ sở dữ liệu
                //            await _context.SaveChangesAsync();

                //            // Gửi tin nhắn đến tất cả các client trong cùng một phòng
                //            await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
                //                "ReceiveMessage",
                //                new
                //                {
                //                    UserId = userConnection.UserId,
                //                    Content = newMessage.Content,
                //                    FileUrl = newMessage.FileUrl, // Đảm bảo FileUrl hợp lệ hoặc chuỗi rỗng
                //                    SentAt = newMessage.SentAt,
                //                    RoomId = newMessage.RoomId,
                //                    Name = newMessage.User.Name,

                //                });
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Error saving message: {ex.Message}");
                //            await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
                //        }
                //    }
                //}

                var user = await _context.Users.FindAsync(userConnection.UserId);
                if (user == null)
                {
                    await Clients.Caller.SendAsync("Error", "User not found.");
                    return;
                }

                // Tạo một thực thể message mới
                var newMessage = new Messages
                {
                    Content = message?.Trim(),
                    SentAt = DateTime.Now,
                    IsPinned = false,
                    UserId = userConnection.UserId,
                    RoomId = userConnection.RoomId,
                    IsRead = false,
                    FileUrl = fileUrl?.Trim() ?? ""
                };

                _context.Messages.Add(newMessage);

                try
                {
                    // Lưu tin nhắn vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    // Gửi tin nhắn đến tất cả các client trong cùng một phòng
                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync(
                        "ReceiveMessage",
                        new
                        {
                            UserId = newMessage.UserId,
                            Content = newMessage.Content,
                            FileUrl = newMessage.FileUrl,
                            SentAt = newMessage.SentAt,
                            RoomId = newMessage.RoomId,
                            Name = user.Name 
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
    }
}
