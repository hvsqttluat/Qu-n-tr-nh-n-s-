using System;

namespace HRM_WPF_CNPM.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty; // Nghỉ phép năm, Ốm đau, việc riêng...
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Chờ duyệt"; // Chờ duyệt, Đã duyệt, Từ chối
        public int? ApprovedById { get; set; }
        public string? RejectReason { get; set; }

        public virtual Employee? Employee { get; set; }
    }
}
