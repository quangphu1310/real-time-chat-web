using System.Threading.Tasks;

namespace real_time_chat_web.Services.IServices
{
    public interface INotificationService
    {
        Task NotifyUser(string userId, object message);
    }
}
