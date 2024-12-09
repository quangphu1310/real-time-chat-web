using Microsoft.AspNetCore.SignalR;

namespace real_time_chat_web.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, object message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
