using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository.IRepository;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace real_time_chat_web.Repository
{
    public class RoomsUserRepository : IRoomsUserRepository
    {
        private readonly ApplicationDbContext _db;
        public RoomsUserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RoomsUser> CreateRoomsUserAsync(RoomsUser entity)
        {
            entity.DayAdd = DateTime.Now;

            _db.RoomsUser.Add(entity);
            await SaveAsync();
            return entity;
        }
        //public async Task<RoomsUser> UpdateRoomsUserAsync(RoomsUser entity)
        //{
        //    _db.RoomsUsers.Remove(entity);
        //    await SaveAsync();
        //    return entity;
        //}
        public async Task RemoveRoomsUserAsync(RoomsUser entity)
        {
            RoomsUser user = await _db.RoomsUser.FirstOrDefaultAsync(n => n.IdUser == entity.IdUser && n.IdRooms == entity.IdRooms);
            _db.RoomsUser.Remove(user);
            await SaveAsync();
        }

        public async Task<List<ApplicationUser>> GetRoomsUserAsync(int IdRooms)
        {
            var users = await _db.RoomsUser
                .Where(r => r.IdRooms == IdRooms)
                .Include(r => r.User)
                .Select(r => r.User)
                .ToListAsync();

            return users;

        }

        public async Task SaveAsync()
        {
            _db.SaveChanges();
        }


        public async Task<List<RoomsDTO>> GetRoomsByUserAsync(string user)
        {
            var rooms = await _db.RoomsUser
                .Where(ru => ru.IdUser == user)
                .Include(ru => ru.Rooms)
                .Select(ru => new RoomsDTO
                {
                    Description = ru.Rooms.Description,
                    IdRooms = ru.IdRooms,
                    CreatedDate = ru.Rooms.CreatedDate,
                    IsActive = ru.Rooms.IsActive,
                    RoomName = ru.Rooms.RoomName,
                })
                .ToListAsync();
            return rooms;
        }

        public async Task<List<RoomsUser>> GetAllRoomsUserAsync()
        {

            return await _db.RoomsUser
        .Include(r => r.Rooms) // Bao gồm thông tin Rooms
        .Include(r => r.User)  // Bao gồm thông tin User
        .Include(r => r.PerUser) // Bao gồm thông tin PerUser
        .ToListAsync();
        }
    }
}
