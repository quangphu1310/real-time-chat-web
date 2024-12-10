using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Repository.IRepository;

namespace real_time_chat_web.Repository
{
    public class RoomsRepository : Repository<Rooms>, IRoomsRepository
    {
        private readonly ApplicationDbContext _db;
        public RoomsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Rooms> CreateRoomsAsync(Rooms entity)
        {
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            _db.rooms.Add(entity);
            await SaveAsync();
            return entity;
        }

        public async Task<Messages> GetLastMessagesAsync(int roomId)
        {
            var lastMessage = await _db.Messages
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new Messages
                {
                    MessageId = m.MessageId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    UserId = m.UserId,
                    RoomId = m.RoomId
                }).FirstOrDefaultAsync();

            return lastMessage;
        }


        public async Task<Rooms> UpdateRoomsAsync(Rooms entity)
        {
            _db.rooms.Update(entity);
            await SaveAsync();
            return entity;
        }
    }
}
