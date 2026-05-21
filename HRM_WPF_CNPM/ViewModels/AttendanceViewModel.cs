using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HRM_WPF_CNPM.Commands;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Helpers;

namespace HRM_WPF_CNPM.ViewModels
{
    public class AttendanceViewModel : BaseViewModel
    {
        private readonly AttendanceService _attendanceService;

        private ObservableCollection<AttendanceRecord> _attendanceRecords = new();
        private ObservableCollection<Employee> _employees = new();
        private ObservableCollection<Department> _departments = new();

        // Search Filters
        private string _searchText = string.Empty;
        private Department? _selectedDepartmentFilter;
        private Employee? _selectedEmployeeFilter;
        private string _selectedStatusFilter = "Tất cả";
        private string _selectedMonthFilter = "Tất cả";
        private string _selectedYearFilter = "Tất cả";

        private AttendanceRecord? _selectedAttendanceRecord;

        // Form Fields
        private Employee? _formEmployee;
        private DateTime? _formWorkDate = DateTime.Today;
        private string _formCheckInTime = "08:00";
        private string _formCheckOutTime = "17:00";
        private double _formWorkHours = 8.0;
        private string _formAttendanceStatus = "Đủ công";
        private string _formNote = string.Empty;

        // Alerts
        private string _successMsg = string.Empty;
        private string _errorMsg = string.Empty;

        private List<AttendanceRecord> _allLoadedRecords = new();

        // Check Permissions
        private bool _canEditDelete = false;
        public bool CanEditDelete
        {
            get => _canEditDelete;
            set => SetProperty(ref _canEditDelete, value);
        }

        public AttendanceViewModel(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;

            // Simple status values
            StatusFilterList = new List<string> { "Tất cả", "Đủ công", "Đi muộn", "Về sớm", "Nghỉ phép", "Nghỉ không phép" };
            FormStatusList = new List<string> { "Đủ công", "Đi muộn", "Về sớm", "Nghỉ phép", "Nghỉ không phép" };

            // Month/Year filter lists
            MonthFilterList = new List<string> { "Tất cả", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            YearFilterList = new List<string> { "Tất cả", "2024", "2025", "2026", "2027" };

            // Initialize commands
            LoadRecordsCommand = new RelayCommand(async _ => await ExecuteLoadRecordsAsync());
            AddCommand = new RelayCommand(async _ => await ExecuteAddAsync());
            UpdateCommand = new RelayCommand(async _ => await ExecuteUpdateAsync());
            DeleteCommand = new RelayCommand(async _ => await ExecuteDeleteAsync());
            CalculateTimeCommand = new RelayCommand(_ => ExecuteCalculateTime());
            ResetFormCommand = new RelayCommand(_ => ExecuteResetForm());

            // Bind triggers to form fields update
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadFilterContextsAsync();
            await ExecuteLoadRecordsAsync();
            EvaluatePermissionRoles();
        }

        private async Task LoadFilterContextsAsync()
        {
            var deptsTask = _attendanceService.GetActiveDepartmentsAsync();
            var empsTask = _attendanceService.GetActiveEmployeesAsync();

            await Task.WhenAll(deptsTask, empsTask);

            Departments = new ObservableCollection<Department>(deptsTask.Result);
            Employees = new ObservableCollection<Employee>(empsTask.Result);

            // Select active user by default if employee
            var curr = UserSession.CurrentUser;
            if (curr != null && curr.Role == "Employee" && curr.EmployeeId.HasValue)
            {
                FormEmployee = Employees.FirstOrDefault(e => e.Id == curr.EmployeeId.Value);
            }
        }

        private void EvaluatePermissionRoles()
        {
            var user = UserSession.CurrentUser;
            if (user == null)
            {
                CanEditDelete = true;
                return;
            }

            // High role admin or hr can edit/delete
            if (user.Role == "Admin" || user.Role == "HR" || user.Role == "Giám đốc")
            {
                CanEditDelete = true;
            }
            else
            {
                CanEditDelete = false;
            }
        }

        // Dropdown values
        public List<string> StatusFilterList { get; }
        public List<string> FormStatusList { get; }
        public List<string> MonthFilterList { get; }
        public List<string> YearFilterList { get; }

        public ObservableCollection<AttendanceRecord> AttendanceRecords
        {
            get => _attendanceRecords;
            set => SetProperty(ref _attendanceRecords, value);
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        #region Filters Binding
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        public Department? SelectedDepartmentFilter
        {
            get => _selectedDepartmentFilter;
            set
            {
                if (SetProperty(ref _selectedDepartmentFilter, value))
                    ApplyFilters();
            }
        }

        public Employee? SelectedEmployeeFilter
        {
            get => _selectedEmployeeFilter;
            set
            {
                if (SetProperty(ref _selectedEmployeeFilter, value))
                    ApplyFilters();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                    ApplyFilters();
            }
        }

        public string SelectedMonthFilter
        {
            get => _selectedMonthFilter;
            set
            {
                if (SetProperty(ref _selectedMonthFilter, value))
                    ApplyFilters();
            }
        }

        public string SelectedYearFilter
        {
            get => _selectedYearFilter;
            set
            {
                if (SetProperty(ref _selectedYearFilter, value))
                    ApplyFilters();
            }
        }
        #endregion

        #region Messages
        public string SuccessMsg
        {
            get => _successMsg;
            set => SetProperty(ref _successMsg, value);
        }

        public string ErrorMsg
        {
            get => _errorMsg;
            set => SetProperty(ref _errorMsg, value);
        }

        private void ClearMessages()
        {
            SuccessMsg = string.Empty;
            ErrorMsg = string.Empty;
        }
        #endregion

        #region Form Bidings
        public Employee? FormEmployee
        {
            get => _formEmployee;
            set
            {
                if (SetProperty(ref _formEmployee, value))
                {
                    _ = CheckApprovedLeaveStateAsync();
                }
            }
        }

        public DateTime? FormWorkDate
        {
            get => _formWorkDate;
            set
            {
                if (SetProperty(ref _formWorkDate, value))
                {
                    _ = CheckApprovedLeaveStateAsync();
                }
            }
        }

        public string FormCheckInTime
        {
            get => _formCheckInTime;
            set => SetProperty(ref _formCheckInTime, value);
        }

        public string FormCheckOutTime
        {
            get => _formCheckOutTime;
            set => SetProperty(ref _formCheckOutTime, value);
        }

        public double FormWorkHours
        {
            get => _formWorkHours;
            set => SetProperty(ref _formWorkHours, value);
        }

        public string FormAttendanceStatus
        {
            get => _formAttendanceStatus;
            set => SetProperty(ref _formAttendanceStatus, value);
        }

        public string FormNote
        {
            get => _formNote;
            set => SetProperty(ref _formNote, value);
        }

        private async Task CheckApprovedLeaveStateAsync()
        {
            if (FormEmployee != null && FormWorkDate.HasValue)
            {
                bool isLeave = await _attendanceService.IsEmployeeOnApprovedLeaveAsync(FormEmployee.Id, FormWorkDate.Value);
                if (isLeave)
                {
                    FormCheckInTime = string.Empty;
                    FormCheckOutTime = string.Empty;
                    FormWorkHours = 0.0;
                    FormAttendanceStatus = "Nghỉ phép";
                    FormNote = "Nghỉ phép tự động (Đã được duyệt đơn nghỉ phép).";
                }
            }
        }
        #endregion

        #region Active selection
        public AttendanceRecord? SelectedAttendanceRecord
        {
            get => _selectedAttendanceRecord;
            set
            {
                if (SetProperty(ref _selectedAttendanceRecord, value))
                {
                    if (value != null)
                    {
                        FormEmployee = Employees.FirstOrDefault(e => e.Id == value.EmployeeId);
                        FormWorkDate = value.WorkDate;
                        FormCheckInTime = value.CheckInTime ?? string.Empty;
                        FormCheckOutTime = value.CheckOutTime ?? string.Empty;
                        FormWorkHours = value.WorkHours;
                        FormAttendanceStatus = value.AttendanceStatus;
                        FormNote = value.Note ?? string.Empty;

                        ClearMessages();
                    }
                }
            }
        }
        #endregion

        #region Commands Actions
        public ICommand LoadRecordsCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CalculateTimeCommand { get; }
        public ICommand ResetFormCommand { get; }

        private async Task ExecuteLoadRecordsAsync()
        {
            ClearMessages();
            try
            {
                var all = await _attendanceService.GetAttendanceRecordsAsync();
                var user = UserSession.CurrentUser;

                // Handle authorization
                if (user == null || user.Role == "Admin" || user.Role == "HR" || user.Role == "Giám đốc")
                {
                    _allLoadedRecords = all;
                }
                else if (user.Role == "Manager")
                {
                    if (user.EmployeeId.HasValue)
                    {
                        var activeEmps = await _attendanceService.GetActiveEmployeesAsync();
                        var managerEmp = activeEmps.FirstOrDefault(e => e.Id == user.EmployeeId.Value);
                        if (managerEmp != null)
                        {
                            var deptEmployeeIds = activeEmps
                                .Where(e => e.DepartmentId == managerEmp.DepartmentId)
                                .Select(e => e.Id)
                                .ToList();

                            _allLoadedRecords = all.Where(r => deptEmployeeIds.Contains(r.EmployeeId)).ToList();
                        }
                        else
                        {
                            _allLoadedRecords = all;
                        }
                    }
                    else
                    {
                        _allLoadedRecords = all;
                    }
                }
                else // Employee
                {
                    if (user.EmployeeId.HasValue)
                    {
                        _allLoadedRecords = all.Where(r => r.EmployeeId == user.EmployeeId.Value).ToList();
                    }
                    else
                    {
                        _allLoadedRecords = new List<AttendanceRecord>();
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Lỗi khi tải bảng công: {ex.Message}";
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allLoadedRecords.AsEnumerable();

            // 1. Text search employee
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string query = SearchText.ToLower();
                filtered = filtered.Where(r => 
                    (r.Employee?.FullName != null && r.Employee.FullName.ToLower().Contains(query)) ||
                    (r.Employee?.EmployeeCode != null && r.Employee.EmployeeCode.ToLower().Contains(query))
                );
            }

            // 2. Department filter
            if (SelectedDepartmentFilter != null)
            {
                filtered = filtered.Where(r => r.Employee?.DepartmentId == SelectedDepartmentFilter.Id);
            }

            // 3. Employee filter
            if (SelectedEmployeeFilter != null)
            {
                filtered = filtered.Where(r => r.EmployeeId == SelectedEmployeeFilter.Id);
            }

            // 4. Status Filter
            if (SelectedStatusFilter != "Tất cả")
            {
                filtered = filtered.Where(r => r.AttendanceStatus == SelectedStatusFilter);
            }

            // 5. Month Filter
            if (SelectedMonthFilter != "Tất cả")
            {
                if (int.TryParse(SelectedMonthFilter, out int month))
                {
                    filtered = filtered.Where(r => r.WorkDate.Month == month);
                }
            }

            // 6. Year Filter
            if (SelectedYearFilter != "Tất cả")
            {
                if (int.TryParse(SelectedYearFilter, out int year))
                {
                    filtered = filtered.Where(r => r.WorkDate.Year == year);
                }
            }

            AttendanceRecords = new ObservableCollection<AttendanceRecord>(filtered);
        }

        private void ExecuteCalculateTime()
        {
            ClearMessages();

            bool isLeave = FormAttendanceStatus == "Nghỉ phép";
            var calc = _attendanceService.SuggestAttendanceDetails(FormCheckInTime, FormCheckOutTime, isLeave);
            if (!calc.IsSuccess)
            {
                ErrorMsg = calc.ErrorMsg;
                return;
            }

            FormWorkHours = calc.WorkHours;
            FormAttendanceStatus = calc.SuggestStatus;
            SuccessMsg = "Đã tính toán số giờ làm việc và đề xuất trạng thái thành công.";
        }

        private async Task ExecuteAddAsync()
        {
            ClearMessages();

            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee")
            {
                ErrorMsg = "Nhân sự không có quyền thêm mới bản công.";
                return;
            }

            if (FormEmployee == null)
            {
                ErrorMsg = "Nhân viên không được tuyển lựa trống.";
                return;
            }

            if (!FormWorkDate.HasValue)
            {
                ErrorMsg = "Ngày công danh nghĩa không được để trống.";
                return;
            }

            // Auto calculate first
            bool isLeave = FormAttendanceStatus == "Nghỉ phép";
            var calc = _attendanceService.SuggestAttendanceDetails(FormCheckInTime, FormCheckOutTime, isLeave);
            if (!calc.IsSuccess)
            {
                ErrorMsg = calc.ErrorMsg;
                return;
            }

            var newRecord = new AttendanceRecord
            {
                EmployeeId = FormEmployee.Id,
                WorkDate = FormWorkDate.Value,
                CheckInTime = string.IsNullOrWhiteSpace(FormCheckInTime) ? null : FormCheckInTime,
                CheckOutTime = string.IsNullOrWhiteSpace(FormCheckOutTime) ? null : FormCheckOutTime,
                WorkHours = calc.WorkHours,
                AttendanceStatus = FormAttendanceStatus,
                Note = FormNote
            };

            var (success, errorMsg) = await _attendanceService.AddAttendanceRecordAsync(newRecord);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadRecordsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private async Task ExecuteUpdateAsync()
        {
            ClearMessages();

            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee")
            {
                ErrorMsg = "Báo lỗi: Bạn không có quyền chỉnh sửa bảng công này.";
                return;
            }

            if (SelectedAttendanceRecord == null)
            {
                ErrorMsg = "Chưa chọn dòng bản ghi chấm công nào từ bảng để Cập nhật.";
                return;
            }

            if (FormEmployee == null)
            {
                ErrorMsg = "Nhân viên không được trống.";
                return;
            }

            if (!FormWorkDate.HasValue)
            {
                ErrorMsg = "Ngày công không được trống.";
                return;
            }

            bool isLeave = FormAttendanceStatus == "Nghỉ phép";
            var calc = _attendanceService.SuggestAttendanceDetails(FormCheckInTime, FormCheckOutTime, isLeave);
            if (!calc.IsSuccess)
            {
                ErrorMsg = calc.ErrorMsg;
                return;
            }

            var recordToUpdate = new AttendanceRecord
            {
                Id = SelectedAttendanceRecord.Id,
                EmployeeId = FormEmployee.Id,
                WorkDate = FormWorkDate.Value,
                CheckInTime = string.IsNullOrWhiteSpace(FormCheckInTime) ? null : FormCheckInTime,
                CheckOutTime = string.IsNullOrWhiteSpace(FormCheckOutTime) ? null : FormCheckOutTime,
                WorkHours = calc.WorkHours,
                AttendanceStatus = FormAttendanceStatus,
                Note = FormNote
            };

            var (success, errorMsg) = await _attendanceService.UpdateAttendanceRecordAsync(recordToUpdate);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadRecordsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            ClearMessages();

            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee")
            {
                ErrorMsg = "Báo lỗi: Bạn không có quyền xóa bảng công.";
                return;
            }

            if (SelectedAttendanceRecord == null)
            {
                ErrorMsg = "Chưa chọn bản ghi chấm công để Xóa.";
                return;
            }

            var (success, errorMsg) = await _attendanceService.DeleteAttendanceRecordAsync(SelectedAttendanceRecord.Id);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadRecordsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private void ExecuteResetForm()
        {
            SelectedAttendanceRecord = null;

            // Retain employee if employee role loaded
            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee" && user.EmployeeId.HasValue)
            {
                FormEmployee = Employees.FirstOrDefault(e => e.Id == user.EmployeeId.Value);
            }
            else
            {
                FormEmployee = null;
            }

            FormWorkDate = DateTime.Today;
            FormCheckInTime = "08:00";
            FormCheckOutTime = "17:00";
            FormWorkHours = 8.0;
            FormAttendanceStatus = "Đủ công";
            FormNote = string.Empty;

            ClearMessages();
        }
        #endregion
    }
}
