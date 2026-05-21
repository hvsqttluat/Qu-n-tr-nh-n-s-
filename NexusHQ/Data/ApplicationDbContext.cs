using Microsoft.EntityFrameworkCore;
using NexusHQ.Models;

namespace NexusHQ.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Procedure> Procedures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Khởi tạo Admin mặc định
            modelBuilder.Entity<User>().HasData(new User 
            { 
                Id = 1, 
                Username = "admin@nexus.hq", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), 
                Role = "Admin", 
                FullName = "Chỉ huy trưởng" 
            });
        }
    }
}
