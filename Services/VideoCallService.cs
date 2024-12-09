using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Services.IServices;

namespace real_time_chat_web.Services
{
    public class VideoCallService : IVideoCallService
    {
        private readonly ApplicationDbContext _dbContext;
        public VideoCallService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<VideoCall> CreateVideoCallAsync(VideoCall videoCall)
        {
            _dbContext.videoCalls.Add(videoCall);
            await _dbContext.SaveChangesAsync();
            return videoCall;
        }

        public async Task<VideoCall> GetCurrentVideoCallAsync(int roomId)
        {
            return await _dbContext.videoCalls
                .Where(vc => vc.RoomId == roomId && vc.Status == "Ongoing")
                .OrderByDescending(vc => vc.CreatedAt)
                .FirstOrDefaultAsync();
        }

    }
}
