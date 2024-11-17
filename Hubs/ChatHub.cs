using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace real_time_chat_web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;  
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(ApplicationDbContext context, IDictionary<string, UserConnection> connections)
        {
            _context = context;
            _connections = connections;
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

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                var newMessage = new Messages
                {
                    Content = message,
                    SentAt = DateTime.Now,
                    IsPinned = false,
                    UserId = userConnection.UserId,
                    RoomId = userConnection.RoomId,
                    IsRead = false,
                    FileUrl = "" 
                };

                _context.Messages.Add(newMessage);

                try
                {
                    await _context.SaveChangesAsync();
                    await Clients.Group(userConnection.RoomId.ToString()).SendAsync("ReceiveMessage", userConnection.UserId, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving message: {ex.Message}");
                    await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
                }
            }
        }


        public Task SendUsersConnected(int roomId)
        {
            var users = _connections.Values
                .Where(c => c.RoomId == roomId)
                .Select(c => c.UserId);

            return Clients.Group(roomId.ToString()).SendAsync("UsersInRoom", users);
        }
    }
}

