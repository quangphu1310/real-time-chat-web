using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Hubs;
using real_time_chat_web.Models;
using real_time_chat_web.Services.IServices;

namespace real_time_chat_web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyRoom(int RoomId, string videoCallUrl, string message)
        {
            // Gửi thông báo đến nhóm theo định dạng chuẩn
            await _hubContext.Clients.Group($"room_{RoomId}").SendAsync("ReceiveGroupNotification", new
            {
                RoomId = RoomId,
                VideoCallUrl = videoCallUrl,
                Message = message
            });
        }
    }
}
