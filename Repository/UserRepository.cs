using real_time_chat_web.Models;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Repository;
using System.Linq.Expressions;
using real_time_chat_web.Data;
using Microsoft.AspNetCore.Identity;

namespace real_time_chat_web.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser entity)
        {
            await _userManager.AddPasswordAsync(entity, "Abc123@");
            entity.NormalizedEmail = entity.UserName.ToUpper();
            entity.Email = entity.UserName;
            _db.ApplicationUsers.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<ApplicationUser> UpdateAsync(ApplicationUser entity)
        {
            _db.ApplicationUsers.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
