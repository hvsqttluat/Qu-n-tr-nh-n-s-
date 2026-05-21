using System;

namespace HRM_WPF_CNPM.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string PayrollMonth { get; set; } = string.Empty; // e.g. "05/2026"
        public decimal BaseSalary { get; set; }
        public decimal StandardWorkDays { get; set; } = 26;
        public decimal ActualWorkDays { get; set; }
        public decimal Bonus { get; set; } = 0;
        public decimal Penalty { get; set; } = 0;
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Nháp"; // Nháp, Đã tính, Đã chốt
        public bool IsLocked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Employee? Employee { get; set; }
    }
}
