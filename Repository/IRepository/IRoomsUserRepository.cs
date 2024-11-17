﻿using real_time_chat_web.Models;
using System.Linq.Expressions;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IRoomsUserRepository
    {
        Task<RoomsUser> CreateRoomsUserAsync(RoomsUser entity);
        //Task<RoomsUser> UpdateRoomsUserAsync(RoomsUser entity);
        Task RemoveRoomsUserAsync(RoomsUser entity);
        Task <List<ApplicationUser>> GetRoomsUserAsync(int IdRooms);
        Task SaveAsync();
    }
}