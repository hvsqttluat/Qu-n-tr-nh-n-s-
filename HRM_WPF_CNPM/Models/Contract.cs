using System;

namespace HRM_WPF_CNPM.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string ContractCode { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty; // Thử việc, 1 năm, 3 năm, Không xác định thời hạn
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Salary { get; set; }
        public string Status { get; set; } = "Còn hiệu lực"; // Còn hiệu lực, Sắp hết hạn, Hết hạn, Đã thanh lý
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        public virtual Employee? Employee { get; set; }
    }
}
