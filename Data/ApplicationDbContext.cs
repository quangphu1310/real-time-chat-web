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
        public DbSet<RoomsUser> RoomsUser { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Rooms>()
               .HasOne(rt => rt.User)
               .WithMany()
               .HasForeignKey(rt => rt.CreatedBy)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoomsUser>()
    .HasKey(r => new { r.IdRooms, r.IdUser });

            builder.Entity<RoomsUser>()
                .HasOne(r => r.Rooms)
                .WithMany()
                .HasForeignKey(r => r.IdRooms)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoomsUser>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.IdUser)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RoomsUser>()
                .HasOne(r => r.PerUser)
                .WithMany()
                .HasForeignKey(r => r.IdPerAdd)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}
