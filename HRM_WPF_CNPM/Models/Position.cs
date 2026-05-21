using System;

namespace HRM_WPF_CNPM.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Department? Department { get; set; }
    }
}
