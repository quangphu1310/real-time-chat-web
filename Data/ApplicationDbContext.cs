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
        public DbSet<Rooms> Rooms { get; set; }
        public DbSet<Messages> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Rooms>()
   .HasOne(rt => rt.User)
   .WithMany()
   .HasForeignKey(rt => rt.CreatedBy)
    .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
            builder.Entity<Messages>()
   .HasOne(rt => rt.User)
   .WithMany()
   .HasForeignKey(rt => rt.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
            builder.Entity<Messages>()
   .HasOne(rt => rt.Room)
   .WithMany()
   .HasForeignKey(rt => rt.RoomId)
    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
