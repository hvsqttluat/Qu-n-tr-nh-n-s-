using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NexusHQ.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string FullName { get; set; } = string.Empty;
    }

    public class Procedure
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Hành chính";
        public string Author { get; set; } = "Hệ thống";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class CommandLog
    {
        public int Id { get; set; }
        public string Command { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data cho Admin mặc định
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
