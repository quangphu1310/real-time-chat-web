using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Models;

namespace real_time_chat_web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Rooms> rooms { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Rooms>()
               .HasOne(rt => rt.User)
               .WithMany()
               .HasForeignKey(rt => rt.CreatedBy)
               .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(builder);
        }
    }
}
