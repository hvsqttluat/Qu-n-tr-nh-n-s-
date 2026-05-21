using System;
using System.Collections.Generic;

namespace HRM_WPF_CNPM.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
