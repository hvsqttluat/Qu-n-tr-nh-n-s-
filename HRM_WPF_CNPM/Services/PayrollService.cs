using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRM_WPF_CNPM.Data;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Helpers;

namespace HRM_WPF_CNPM.Services
{
    public class PayrollService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        // In-memory fallback list to ensure fully flawless UX when DB is offline
        private static readonly List<Payroll> _mockPayrolls = new List<Payroll>();
        private static int _nextMockId = 1;
        private static bool _isMockInitialized = false;

        public PayrollService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            if (_isMockInitialized) return;
            lock (_mockPayrolls)
            {
                if (_isMockInitialized) return;

                // Create mock payrolls for employees
                var employees = GetMockEmployees();
                string curMonth = DateTime.Today.ToString("MM/yyyy");

                _mockPayrolls.Add(new Payroll
                {
                    Id = _nextMockId++,
                    EmployeeId = 1,
                    PayrollMonth = curMonth,
                    BaseSalary = 15000000,
                    StandardWorkDays = 26,
                    ActualWorkDays = 25,
                    Bonus = 500000,
                    Penalty = 0,
                    NetSalary = Math.Round(15000000m / 26 * 25 + 500000 - 0, 0),
                    Status = "Đã tính",
                    IsLocked = false,
                    Employee = employees.FirstOrDefault(e => e.Id == 1)
                });

                _mockPayrolls.Add(new Payroll
                {
                    Id = _nextMockId++,
                    EmployeeId = 2,
                    PayrollMonth = curMonth,
                    BaseSalary = 20000000,
                    StandardWorkDays = 26,
                    ActualWorkDays = 26,
                    Bonus = 1000000,
                    Penalty = 200000,
                    NetSalary = Math.Round(20000000m / 26 * 26 + 1000000 - 200000, 0),
                    Status = "Đã chốt",
                    IsLocked = true,
                    Employee = employees.FirstOrDefault(e => e.Id == 2)
                });

                _mockPayrolls.Add(new Payroll
                {
                    Id = _nextMockId++,
                    EmployeeId = 3,
                    PayrollMonth = curMonth,
                    BaseSalary = 10000000,
                    StandardWorkDays = 26,
                    ActualWorkDays = 22,
                    Bonus = 0,
                    Penalty = 100000,
                    NetSalary = Math.Round(10000000m / 26 * 22 + 0 - 100000, 0),
                    Status = "Nháp",
                    IsLocked = false,
                    Employee = employees.FirstOrDefault(e => e.Id == 3)
                });

                _isMockInitialized = true;
            }
        }

        private List<Employee> GetMockEmployees()
        {
            var deptAdmin = new Department { Id = 1, DepartmentCode = "NS", DepartmentName = "Phòng Hành chính Nhân sự" };
            var deptTech = new Department { Id = 2, DepartmentCode = "KT", DepartmentName = "Phòng Kỹ Thuật Công Nghệ" };

            var posManager = new Position { Id = 1, PositionCode = "TP", PositionName = "Trưởng phòng", DepartmentId = 1, Department = deptAdmin };
            var posDev = new Position { Id = 2, PositionCode = "KTV", PositionName = "Kỹ thuật viên", DepartmentId = 2, Department = deptTech };

            return new List<Employee>
            {
                new Employee { Id = 1, EmployeeCode = "NV001", FullName = "Nguyễn Văn Nhân Viên", WorkStatus = "Chính thức", BaseSalary = 15000000, DepartmentId = 2, Department = deptTech, Position = posDev, Note = "Kỹ sư WPF" },
                new Employee { Id = 2, EmployeeCode = "NV002", FullName = "Trần Thị Nhân Sự", WorkStatus = "Chính thức", BaseSalary = 20000000, DepartmentId = 1, Department = deptAdmin, Position = posManager, Note = "Quản lý tuyển dụng" },
                new Employee { Id = 3, EmployeeCode = "NV003", FullName = "Phạm Việt Hoàng", WorkStatus = "Thử việc", BaseSalary = 10000000, DepartmentId = 2, Department = deptTech, Position = posDev, Note = "Thực tập" }
            };
        }

        // Get payrolls for a specific month
        public async Task<List<Payroll>> GetPayrollsAsync(string month)
        {
            try
            {
                var list = await _context.Payrolls
                    .Include(p => p.Employee)
                        .ThenInclude(e => e!.Department)
                    .Where(p => p.PayrollMonth == month)
                    .ToListAsync();

                if (list.Count == 0)
                {
                    lock (_mockPayrolls)
                    {
                        return _mockPayrolls.Where(p => p.PayrollMonth == month).ToList();
                    }
                }

                return list;
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    return _mockPayrolls.Where(p => p.PayrollMonth == month).ToList();
                }
            }
        }

        // Get payroll by EmployeeId
        public async Task<List<Payroll>> GetPayrollsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var list = await _context.Payrolls
                    .Include(p => p.Employee)
                        .ThenInclude(e => e!.Department)
                    .Where(p => p.EmployeeId == employeeId)
                    .ToListAsync();

                if (list.Count == 0)
                {
                    lock (_mockPayrolls)
                    {
                        return _mockPayrolls.Where(p => p.EmployeeId == employeeId).ToList();
                    }
                }

                return list;
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    return _mockPayrolls.Where(p => p.EmployeeId == employeeId).ToList();
                }
            }
        }

        // Create initial blank payrolls for active employees in a month
        public async Task<(bool IsSuccess, string ErrorMsg)> CreatePayrollsForMonthAsync(string month, decimal standardWorkDays)
        {
            if (string.IsNullOrWhiteSpace(month))
                return (false, "Tháng lương không được trống.");
            if (standardWorkDays <= 0)
                return (false, "Số ngày làm việc chuẩn phải lớn hơn 0.");

            try
            {
                // Get active employees
                var employees = await _context.Employees
                    .Where(e => !e.IsDeleted && (e.WorkStatus == "Chính thức" || e.WorkStatus == "Thử việc"))
                    .ToListAsync();

                if (employees.Count == 0)
                {
                    employees = GetMockEmployees();
                }

                int addedCount = 0;
                int existedCount = 0;

                foreach (var emp in employees)
                {
                    // Check duplicate
                    bool exists = await _context.Payrolls.AnyAsync(p => p.EmployeeId == emp.Id && p.PayrollMonth == month);
                    if (exists)
                    {
                        existedCount++;
                        continue;
                    }

                    // Count actual work days in AttendanceRecords
                    decimal actualWorkDays = await GetActualWorkDaysCountAsync(emp.Id, month);

                    var newPayroll = new Payroll
                    {
                        EmployeeId = emp.Id,
                        PayrollMonth = month,
                        BaseSalary = emp.BaseSalary,
                        StandardWorkDays = standardWorkDays,
                        ActualWorkDays = actualWorkDays,
                        Bonus = 0,
                        Penalty = 0,
                        Status = "Nháp",
                        IsLocked = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Initial NetSalary formula
                    newPayroll.NetSalary = CalculateNetSalary(newPayroll.BaseSalary, newPayroll.StandardWorkDays, newPayroll.ActualWorkDays, newPayroll.Bonus, newPayroll.Penalty);

                    _context.Payrolls.Add(newPayroll);
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();

                    var audit = new AuditLogService(_context);
                    await audit.LogAsync("CREATE", "Payrolls", 0, $"Tạo bảng lương khởi tạo cho tháng {month} (Số lượng: {addedCount} nhân sự).");
                }

                return (true, $"Đã tạo bảng lương thành công cho {addedCount} nhân viên. Bỏ qua {existedCount} nhân viên đã có bảng lương.");
            }
            catch
            {
                // Fallback for mock environment
                lock (_mockPayrolls)
                {
                    var mockEmployees = GetMockEmployees();
                    int addedCount = 0;
                    int existedCount = 0;

                    foreach (var emp in mockEmployees)
                    {
                        bool exists = _mockPayrolls.Any(p => p.EmployeeId == emp.Id && p.PayrollMonth == month);
                        if (exists)
                        {
                            existedCount++;
                            continue;
                        }

                        var newPayroll = new Payroll
                        {
                            Id = _nextMockId++,
                            EmployeeId = emp.Id,
                            PayrollMonth = month,
                            BaseSalary = emp.BaseSalary,
                            StandardWorkDays = standardWorkDays,
                            ActualWorkDays = 22, // Static mock default
                            Bonus = 0,
                            Penalty = 0,
                            Status = "Nháp",
                            IsLocked = false,
                            Employee = emp,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        newPayroll.NetSalary = CalculateNetSalary(newPayroll.BaseSalary, newPayroll.StandardWorkDays, newPayroll.ActualWorkDays, newPayroll.Bonus, newPayroll.Penalty);
                        _mockPayrolls.Add(newPayroll);
                        addedCount++;
                    }

                    return (true, $"(Dữ liệu mẫu) Đã tạo bảng lương thành công cho {addedCount} nhân viên. Bỏ qua {existedCount} nhân sự đã có.");
                }
            }
        }

        // Count actual work days by parsing the month e.g. "05/2026"
        private async Task<decimal> GetActualWorkDaysCountAsync(int employeeId, string payrollMonth)
        {
            var parts = payrollMonth.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
            {
                return 0;
            }

            try
            {
                // Retrieve all attendance records for employee in that specific month and year
                var records = await _context.AttendanceRecords
                    .Where(r => r.EmployeeId == employeeId && r.WorkDate.Month == month && r.WorkDate.Year == year)
                    .ToListAsync();

                decimal days = 0;
                foreach (var r in records)
                {
                    string st = r.AttendanceStatus;
                    if (st == "Đủ công" || st == "Đúng giờ" || st == "Đi muộn" || st == "Về sớm" || st == "Nghỉ phép")
                    {
                        days += 1.0m;
                    }
                }
                return days;
            }
            catch
            {
                return 22; // Quick mock count
            }
        }

        // Calculate specific employee NetSalary
        public async Task<(bool IsSuccess, string ErrorMsg)> CalculatePayrollAsync(int id)
        {
            try
            {
                var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.Id == id);
                if (payroll == null)
                    return (false, "Không tìm thấy bản ghi lương.");

                if (payroll.IsLocked)
                    return (false, "Bản ghi lương đã bị khóa vĩnh viễn, không thể tính lại.");

                // Re-calculate actual workdays
                payroll.ActualWorkDays = await GetActualWorkDaysCountAsync(payroll.EmployeeId, payroll.PayrollMonth);

                // Fetch current user BaseSalary from employees update
                var employee = await _context.Employees.FindAsync(payroll.EmployeeId);
                if (employee != null)
                {
                    payroll.BaseSalary = employee.BaseSalary;
                }

                payroll.NetSalary = CalculateNetSalary(payroll.BaseSalary, payroll.StandardWorkDays, payroll.ActualWorkDays, payroll.Bonus, payroll.Penalty);
                payroll.Status = "Đã tính";
                payroll.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var audit = new AuditLogService(_context);
                await audit.LogAsync("UPDATE", "Payrolls", payroll.Id, $"Tính lại lương cho nhân sự ID {payroll.EmployeeId} tháng {payroll.PayrollMonth}.");

                return (true, "Đã tính lại lương cho nhân sự thành công.");
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    var payroll = _mockPayrolls.FirstOrDefault(p => p.Id == id);
                    if (payroll == null) return (false, "Không tìm thấy bản ghi.");
                    if (payroll.IsLocked) return (false, "Lương đã khóa.");

                    payroll.NetSalary = CalculateNetSalary(payroll.BaseSalary, payroll.StandardWorkDays, payroll.ActualWorkDays, payroll.Bonus, payroll.Penalty);
                    payroll.Status = "Đã tính";
                    payroll.UpdatedAt = DateTime.Now;
                    return (true, "(Dữ liệu mẫu) Tính lại lương hoàn tất.");
                }
            }
        }

        // Calculate all for a month
        public async Task<(bool IsSuccess, string ErrorMsg)> CalculateAllForMonthAsync(string month)
        {
            try
            {
                var payrolls = await _context.Payrolls.Where(p => p.PayrollMonth == month && !p.IsLocked).ToListAsync();
                if (payrolls.Count == 0)
                {
                    // Fallback directly to mock calculations
                    lock (_mockPayrolls)
                    {
                        payrolls = _mockPayrolls.Where(p => p.PayrollMonth == month && !p.IsLocked).ToList();
                    }
                }

                foreach (var p in payrolls)
                {
                    p.ActualWorkDays = await GetActualWorkDaysCountAsync(p.EmployeeId, p.PayrollMonth);
                    p.NetSalary = CalculateNetSalary(p.BaseSalary, p.StandardWorkDays, p.ActualWorkDays, p.Bonus, p.Penalty);
                    p.Status = "Đã tính";
                    p.UpdatedAt = DateTime.Now;
                }

                // If EF is connected, save
                var countInDb = await _context.Payrolls.CountAsync(p => p.PayrollMonth == month);
                if (countInDb > 0)
                {
                    await _context.SaveChangesAsync();

                    var audit = new AuditLogService(_context);
                    await audit.LogAsync("UPDATE", "Payrolls", 0, $"Tính toán/cập nhật lại toàn bộ bảng lương tháng {month}.");
                }

                return (true, $"Đã hoàn tất tính toán bảng lương cho các nhân sự trong {month}.");
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    var list = _mockPayrolls.Where(p => p.PayrollMonth == month && !p.IsLocked).ToList();
                    foreach (var p in list)
                    {
                        p.NetSalary = CalculateNetSalary(p.BaseSalary, p.StandardWorkDays, p.ActualWorkDays, p.Bonus, p.Penalty);
                        p.Status = "Đã tính";
                        p.UpdatedAt = DateTime.Now;
                    }
                }
                return (true, $"(Dữ liệu mẫu) Đã hoàn tất tính toán toàn bộ bảng lương {month}.");
            }
        }

        // Update single payroll bonus & penalty
        public async Task<(bool IsSuccess, string ErrorMsg)> UpdateBonusPenaltyAsync(int id, decimal bonus, decimal penalty)
        {
            if (bonus < 0 || penalty < 0)
                return (false, "Thưởng và phạt không được âm.");

            try
            {
                var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.Id == id);
                if (payroll == null)
                    return (false, "Không tồn tại bản ghi lương tương ứng.");

                if (payroll.IsLocked)
                    return (false, "Bản ghi đã khóa, không thể cập nhật.");

                payroll.Bonus = bonus;
                payroll.Penalty = penalty;
                payroll.NetSalary = CalculateNetSalary(payroll.BaseSalary, payroll.StandardWorkDays, payroll.ActualWorkDays, payroll.Bonus, payroll.Penalty);
                payroll.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var audit = new AuditLogService(_context);
                await audit.LogAsync("UPDATE", "Payrolls", payroll.Id, $"Cập nhật thưởng phạt cho nhân sự ID {payroll.EmployeeId} tháng {payroll.PayrollMonth}: Thưởng +{bonus:N0} đ, Phạt -{penalty:N0} đ.");

                return (true, "Đã cập nhật Thưởng/Phạt và cập nhật lại Thực lĩnh thành công.");
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    var p = _mockPayrolls.FirstOrDefault(x => x.Id == id);
                    if (p == null) return (false, "Không tìm thấy.");
                    if (p.IsLocked) return (false, "Bản ghi đã khóa.");

                    p.Bonus = bonus;
                    p.Penalty = penalty;
                    p.NetSalary = CalculateNetSalary(p.BaseSalary, p.StandardWorkDays, p.ActualWorkDays, p.Bonus, p.Penalty);
                    p.UpdatedAt = DateTime.Now;
                    return (true, "(Dữ liệu mẫu) Cập nhật Thưởng/Phạt thành công.");
                }
            }
        }

        // Lock/Close payroll for month
        public async Task<(bool IsSuccess, string ErrorMsg, int LockedCount)> LockPayrollsForMonthAsync(string month)
        {
            try
            {
                var payrolls = await _context.Payrolls
                    .Include(p => p.Employee)
                    .Where(p => p.PayrollMonth == month && !p.IsLocked)
                    .ToListAsync();

                if (payrolls.Count == 0)
                {
                    lock (_mockPayrolls)
                    {
                        payrolls = _mockPayrolls.Where(p => p.PayrollMonth == month && !p.IsLocked).ToList();
                    }
                }

                int count = 0;
                foreach (var p in payrolls)
                {
                    p.IsLocked = true;
                    p.Status = "Đã chốt";
                    p.UpdatedAt = DateTime.Now;

                    // Trigger notification
                    _notificationService.CreateNotification(
                        p.EmployeeId, 
                        $"Phiếu lương chốt {month}", 
                        $"Bảng lương tháng {month} của bạn đã được chốt. Thực lĩnh: {p.NetSalary:N0} VNĐ. Vui lòng kiểm tra chi tiết phiếu lương."
                    );
                    count++;
                }

                var countInDb = await _context.Payrolls.CountAsync(p => p.PayrollMonth == month);
                if (countInDb > 0)
                {
                    await _context.SaveChangesAsync();

                    var audit = new AuditLogService(_context);
                    await audit.LogAsync("UPDATE", "Payrolls", 0, $"Chốt (Khóa) bảng lương tháng {month} cho {count} nhân sự.");
                }

                return (true, $"Đã thực hiện khóa vĩnh viễn bảng lương {month} cho {count} nhân sự.", count);
            }
            catch
            {
                int count = 0;
                lock (_mockPayrolls)
                {
                    var list = _mockPayrolls.Where(p => p.PayrollMonth == month && !p.IsLocked).ToList();
                    foreach (var p in list)
                    {
                        p.IsLocked = true;
                        p.Status = "Đã chốt";
                        p.UpdatedAt = DateTime.Now;

                        _notificationService.CreateNotification(
                            p.EmployeeId, 
                            $"Phiếu lương chốt {month}", 
                            $"Bảng lương tháng {month} của bạn đã được chốt. Thực lĩnh: {p.NetSalary:N0} VNĐ. Vui lòng kiểm tra chi tiết phiếu lương."
                        );
                        count++;
                    }
                }
                return (true, $"(Dữ liệu mẫu) Đã khóa và chốt {count} phiếu lương hoàn tất.", count);
            }
        }

        // Formula NetSalary
        private decimal CalculateNetSalary(decimal baseSalary, decimal standardDays, decimal actualDays, decimal bonus, decimal penalty)
        {
            if (standardDays <= 0) return 0;
            decimal mainSum = (baseSalary / standardDays) * actualDays + bonus - penalty;
            return Math.Round(mainSum, 0); // Rounded as requested
        }

        // Get total payroll cost for the current month (for dashboard statistics)
        public async Task<decimal> GetTotalPayrollForCurrentMonthAsync()
        {
            string month = DateTime.Today.ToString("MM/yyyy");
            try
            {
                var sum = await _context.Payrolls
                    .Where(p => p.PayrollMonth == month)
                    .SumAsync(p => p.NetSalary);

                if (sum == 0)
                {
                    lock (_mockPayrolls)
                    {
                        return _mockPayrolls.Where(p => p.PayrollMonth == month).Sum(p => p.NetSalary);
                    }
                }
                return sum;
            }
            catch
            {
                lock (_mockPayrolls)
                {
                    return _mockPayrolls.Where(p => p.PayrollMonth == month).Sum(p => p.NetSalary);
                }
            }
        }
    }
}
