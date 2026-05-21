using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM_WPF_CNPM.Services
{
    public class SystemNotification
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
    }

    public class NotificationService
    {
        private static readonly List<SystemNotification> _notifications = new List<SystemNotification>();
        private static int _nextId = 1;

        static NotificationService()
        {
            // Seed a few initial notifications for realistic UX
            _notifications.Add(new SystemNotification
            {
                Id = _nextId++,
                EmployeeId = 1,
                Title = "Chào mừng thành viên mới",
                Content = "Chào mừng bạn gia nhập công ty! Hãy cập nhật hồ sơ và hợp đồng đầu tiên.",
                CreatedAt = DateTime.Now.AddDays(-3),
                IsRead = true
            });
            _notifications.Add(new SystemNotification
            {
                Id = _nextId++,
                EmployeeId = 2,
                Title = "Hệ thống nghỉ phép hoạt động",
                Content = "Chào buổi sáng quản trị viên! Hệ thống duyệt nghỉ phép tự động đã sẵn sàng.",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsRead = false
            });
        }

        public void CreateNotification(int? employeeId, string title, string content)
        {
            lock (_notifications)
            {
                _notifications.Add(new SystemNotification
                {
                    Id = _nextId++,
                    EmployeeId = employeeId,
                    Title = title,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }
        }

        public List<SystemNotification> GetNotificationsForEmployee(int employeeId)
        {
            lock (_notifications)
            {
                return _notifications
                    .Where(n => n.EmployeeId == employeeId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
            }
        }

        public List<SystemNotification> GetAllNotifications()
        {
            lock (_notifications)
            {
                return _notifications.OrderByDescending(n => n.CreatedAt).ToList();
            }
        }
    }
}
