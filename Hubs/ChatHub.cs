using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace real_time_chat_web.Hubs
{
    //public class ChatHub : Hub
    //{
    //    private readonly string _botUser;
    //    private readonly IDictionary<string, UserConnection> _connections;
    //    private readonly ApplicationDbContext _context;

    //    public ChatHub(IDictionary<string, UserConnection> connections, ApplicationDbContext context)
    //    {
    //        _botUser = "MyChat Bot";
    //        _connections = connections;
    //        _context = context;
    //    }

    //    public override Task OnDisconnectedAsync(Exception exception)
    //    {
    //        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
    //        {
    //            _connections.Remove(Context.ConnectionId);
    //            Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
    //            SendUsersConnected(userConnection.Room);
    //        }

    //        return base.OnDisconnectedAsync(exception);
    //    }

    //    public async Task JoinRoom(UserConnection userConnection)
    //    {
    //        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
    //        _connections[Context.ConnectionId] = userConnection;

    //        await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has joined {userConnection.Room}");
    //        await SendUsersConnected(userConnection.Room);
    //    }

    //    public async Task SendMessage(string message)
    //    {
    //        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
    //        {
    //            var chatMessage = new Messages
    //            {
    //                Content = message,
    //                SentAt = DateTime.Now,
    //                UserId = userConnection.UserId,
    //                RoomId = userConnection.RoomId
    //            };

    //            _context.Messages.Add(chatMessage);
    //            await _context.SaveChangesAsync();

    //            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
    //        }
    //    }

    //    public Task SendUsersConnected(string room)
    //    {
    //        var users = _connections.Values
    //            .Where(c => c.Room == room)
    //            .Select(c => c.User);

    //        return Clients.Group(room).SendAsync("UsersInRoom", users);
    //    }
    //}
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;  // Đảm bảo có quyền truy cập vào ApplicationDbContext
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(ApplicationDbContext context, IDictionary<string, UserConnection> connections)
        {
            _context = context;
            _connections = connections;
        }

        // Khi người dùng ngắt kết nối khỏi SignalR
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

        // Khi người dùng tham gia phòng
        public async Task JoinRoom(int roomId, string userId)
        {
            var userConnection = new UserConnection(Context.ConnectionId, userId, roomId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            _connections[Context.ConnectionId] = userConnection;

            // Gửi thông báo vào phòng khi người dùng tham gia
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", "MyChat Bot", $"{userId} has joined the room");

            // Gửi danh sách người dùng trong phòng
            await SendUsersConnected(roomId);
        }

        // Khi người dùng gửi tin nhắn
        public async Task SendMessage(string message)
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
                    FileUrl = "" // Set FileUrl to an empty string or null if no file is attached
                };

                // Add the new message to the database
                _context.Messages.Add(newMessage);

                try
                {
                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Notify all clients in the same room about the new message
                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync("ReceiveMessage", userConnection.UserId, message);
                }
                catch (Exception ex)
                {
                    // Log the exception (you can log it or handle it as needed)
                    Console.WriteLine($"Error saving message: {ex.Message}");
                    // You can send an error message to the client if needed
                    await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
                }
            }
        }


        // Gửi danh sách người dùng đang kết nối trong phòng
        public Task SendUsersConnected(int roomId)
        {
            var users = _connections.Values
                .Where(c => c.RoomId == roomId)
                .Select(c => c.UserId);

            return Clients.Group(roomId.ToString()).SendAsync("UsersInRoom", users);
        }
    }
}

