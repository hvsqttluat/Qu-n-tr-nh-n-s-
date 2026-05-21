namespace NexusHQ.Models
{
    public class Procedure
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Hành chính";
        public string Author { get; set; } = "Hệ thống";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
