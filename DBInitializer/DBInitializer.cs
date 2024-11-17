using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;

namespace real_time_chat_web.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DBInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }

            if (!_roleManager.RoleExistsAsync("user").GetAwaiter().GetResult())
            {
                // Tạo các roles
                _roleManager.CreateAsync(new IdentityRole("user")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole("admin")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole("mod")).GetAwaiter().GetResult();

                // Tạo tài khoản admin
                _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        Email = "admin@chatweb.com",
                        UserName = "admin@chatweb.com",
                        Name = "Admin",
                        NormalizedEmail = "ADMIN@CHATWEB.COM",
                        EmailConfirmed = true
                    }, "Admin123@"
                ).GetAwaiter().GetResult();

                ApplicationUser admin = _db.ApplicationUsers.FirstOrDefault(x => x.Email == "admin@chatweb.com");
                _userManager.AddToRoleAsync(admin, "admin").GetAwaiter().GetResult();

                // Tạo tài khoản mod
                _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        Email = "mod@chatweb.com",
                        UserName = "mod@chatweb.com",
                        Name = "Moderator",
                        NormalizedEmail = "MOD@CHATWEB.COM",
                        EmailConfirmed = true
                    }, "Mod123@"
                ).GetAwaiter().GetResult();

                ApplicationUser mod = _db.ApplicationUsers.FirstOrDefault(x => x.Email == "mod@chatweb.com");
                _userManager.AddToRoleAsync(mod, "mod").GetAwaiter().GetResult();

                // Tạo tài khoản user 1
                _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        Email = "user1@chatweb.com",
                        UserName = "user1@chatweb.com",
                        Name = "User One",
                        NormalizedEmail = "USER1@CHATWEB.COM",
                        EmailConfirmed = true
                    }, "User123@"
                ).GetAwaiter().GetResult();

                ApplicationUser user1 = _db.ApplicationUsers.FirstOrDefault(x => x.Email == "user1@chatweb.com");
                _userManager.AddToRoleAsync(user1, "user").GetAwaiter().GetResult();

                // Tạo tài khoản user 2
                _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        Email = "user2@chatweb.com",
                        UserName = "user2@chatweb.com",
                        Name = "User Two",
                        NormalizedEmail = "USER2@CHATWEB.COM",
                        EmailConfirmed = true
                    }, "User123@"
                ).GetAwaiter().GetResult();

                ApplicationUser user2 = _db.ApplicationUsers.FirstOrDefault(x => x.Email == "user2@chatweb.com");
                _userManager.AddToRoleAsync(user2, "user").GetAwaiter().GetResult();
            }
        }

    }
}
