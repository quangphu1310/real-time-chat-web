using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Hubs;
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
        public async Task NotifyUser(string userId, object message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
