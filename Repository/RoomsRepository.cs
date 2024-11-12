using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Repository.IRepository;

namespace real_time_chat_web.Repository
{
    public class RoomsRepository : Repository<ApplicationRooms>, IRoomsRepository
    {
        private readonly ApplicationDbContext _db;
        public RoomsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<ApplicationRooms> UpdateRoomsAsync(ApplicationRooms entity)
        {
            _db.Update(entity);
            await SaveAsync();
            return entity;
        }
    }
}
