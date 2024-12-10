using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
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

        public async Task<MessageGetDTO> GetLastMessageAsync(int idRooms)
        {
            var message = await _db.Messages
                .Where(n => n.RoomId == idRooms)
                .OrderByDescending(n => n.SentAt)
                .Select(n => new MessageGetDTO
                {
                    MessageId = n.MessageId,
                    Content = n.Content,
                    SentAt = n.SentAt,
                    UserId = n.UserId,
                    RoomId = n.RoomId
                }).FirstOrDefaultAsync();
            return message;
        }

        public async Task<Rooms> UpdateRoomsAsync(Rooms entity)
        {
            _db.rooms.Update(entity);
            await SaveAsync();
            return entity;
        }
    }
}
