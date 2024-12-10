using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Models;

namespace real_time_chat_web.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task SendGroupNotification(string RoomId, string videoCallUrl, string message)
        {
            _logger.LogInformation($"Sending notification to room {RoomId} with URL {videoCallUrl}");

            // Gửi thông báo đến tất cả các thành viên trong nhóm
            await Clients.Group(RoomId).SendAsync("ReceiveGroupNotification", new
            {
                RoomId = RoomId,
                VideoCallUrl = videoCallUrl,
                Message = message
            });
        }

        public async Task JoinGroup(string RoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{RoomId}");
            Console.WriteLine($"Client {Context.ConnectionId} joined group room_{RoomId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
       

    }
}
