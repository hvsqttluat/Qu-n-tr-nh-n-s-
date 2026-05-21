using System;

namespace HRM_WPF_CNPM.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = "Nam";
        public DateTime? DateOfBirth { get; set; }
        public string? CitizenId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Today;
        public string WorkStatus { get; set; } = "Thử việc"; // Thử việc, Chính thức, Tạm nghỉ, Đã nghỉ
        public decimal BaseSalary { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual Position? Position { get; set; }
    }
}
