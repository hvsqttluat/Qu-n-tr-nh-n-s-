using System;

namespace HRM_WPF_CNPM.DTOs
{
    public class ContractListDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ContractCode { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Salary { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }

        // Formatted properties for data rendering
        public string DisplayStartDate => StartDate.ToString("dd/MM/yyyy");
        public string DisplayEndDate => EndDate.HasValue ? EndDate.Value.ToString("dd/MM/yyyy") : "Không xác định";
        public string DisplaySalary => Salary.ToString("N0") + " VNĐ";
    }
}
