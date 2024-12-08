using real_time_chat_web.Models;

namespace real_time_chat_web.Services.IServices
{
    public interface IVideoCallService
    {
        Task<VideoCall> CreateVideoCallAsync(VideoCall videoCall);
        Task<VideoCall> GetCurrentVideoCallAsync(int roomId);

    }
}
