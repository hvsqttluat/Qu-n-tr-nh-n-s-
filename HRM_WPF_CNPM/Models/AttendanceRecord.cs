using System;

namespace HRM_WPF_CNPM.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public double WorkHours { get; set; }
        public string AttendanceStatus { get; set; } = "Đi muộn"; // Đúng giờ, Đi muộn, Vắng mặt...

        public virtual Employee? Employee { get; set; }
    }
}
