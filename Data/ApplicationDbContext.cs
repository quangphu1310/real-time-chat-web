using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Models;
using System.Reflection.Emit;

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
        public DbSet<Messages> Messages { get; set; }
        public DbSet<VideoCall> videoCalls { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Rooms>()
               .HasOne(rt => rt.User)
               .WithMany()
               .HasForeignKey(rt => rt.CreatedBy)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rooms>()
                .HasMany(r => r.RoomsUsers)
                .WithOne(ru => ru.Rooms)
                .HasForeignKey(ru => ru.IdRooms);

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

            //base.OnModelCreating(builder);
            //builder.Entity<Rooms>()
            //   .HasOne(rt => rt.User)
            //   .WithMany()
            //   .HasForeignKey(rt => rt.CreatedBy)
            //    .OnDelete(DeleteBehavior.Restrict);

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

            // add relationships VideoCall
            builder.Entity<VideoCall>()
                .HasOne(vc => vc.Rooms)
                .WithMany(r => r.VideoCalls)
                .HasForeignKey(vc => vc.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<VideoCall>()
                .HasOne(vc => vc.User)
                .WithMany(u => u.CreatedVideoCalls)
                .HasForeignKey(vc => vc.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình quan hệ Messages - Rooms
            //builder.Entity<Messages>()
            //    .HasOne(m => m.Room)
            //    .WithMany(r => r.Messages)
            //    .HasForeignKey(m => m.RoomId);
        }
    }
}
