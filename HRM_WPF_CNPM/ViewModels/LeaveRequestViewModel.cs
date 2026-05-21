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
    public class LeaveRequestViewModel : BaseViewModel
    {
        private readonly LeaveRequestService _leaveRequestService;

        private ObservableCollection<LeaveRequest> _leaveRequests = new();
        private ObservableCollection<Employee> _employees = new();
        
        private string _searchText = string.Empty;
        private string _selectedLeaveTypeFilter = "Tất cả";
        private string _selectedStatusFilter = "Tất cả";
        private DateTime? _filterFromDate;
        private DateTime? _filterToDate;

        private LeaveRequest? _selectedLeaveRequest;

        // Form Fields
        private Employee? _selectedEmployeeForForm;
        private string _editingLeaveType = "Phép năm";
        private DateTime? _editingFromDate = DateTime.Today;
        private DateTime? _editingToDate = DateTime.Today;
        private double _editingTotalDays = 1;
        private string _editingReason = string.Empty;
        private string _editingStatus = "Chờ duyệt";
        private string _editingRejectReason = string.Empty;
        private string _employeeRemainingLeaveText = string.Empty;

        // Alerts
        private string _successMsg = string.Empty;
        private string _errorMsg = string.Empty;

        // Cache full list for search&filter
        private List<LeaveRequest> _allLoadedLeaveRequests = new();

        public LeaveRequestViewModel(LeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;

            // Types List
            LeaveTypeList = new List<string> { "Phép năm", "Nghỉ ốm", "Nghỉ không lương", "Việc riêng" };
            LeaveTypeFilterList = new List<string> { "Tất cả", "Phép năm", "Nghỉ ốm", "Nghỉ không lương", "Việc riêng" };
            StatusFilterList = new List<string> { "Tất cả", "Chờ duyệt", "Đã duyệt", "Từ chối", "Đã hủy" };

            // Default remaining text
            EmployeeRemainingLeaveText = "Chọn nhân viên để xem số ngày phép năm còn lại.";

            // Initialize Commands
            LoadLeaveRequestsCommand = new RelayCommand(async _ => await ExecuteLoadLeaveRequestsAsync());
            AddCommand = new RelayCommand(async _ => await ExecuteAddAsync());
            UpdateCommand = new RelayCommand(async _ => await ExecuteUpdateAsync());
            CancelCommand = new RelayCommand(async _ => await ExecuteCancelAsync());
            ApproveCommand = new RelayCommand(async _ => await ExecuteApproveAsync());
            RejectCommand = new RelayCommand(async _ => await ExecuteRejectAsync());
            ResetFormCommand = new RelayCommand(_ => ExecuteResetForm());

            // Load initial view
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await GetEmployeesListAsync();
            await ExecuteLoadLeaveRequestsAsync();
            EvaluatePermissionStates();
        }

        private async Task GetEmployeesListAsync()
        {
            var list = await _leaveRequestService.GetActiveEmployeesAsync();
            Employees = new ObservableCollection<Employee>(list);

            // Set default employee if logged in user has EmployeeId or is employee
            var curr = UserSession.CurrentUser;
            if (curr != null && curr.EmployeeId.HasValue)
            {
                SelectedEmployeeForForm = Employees.FirstOrDefault(e => e.Id == curr.EmployeeId.Value);
            }
        }

        // Active filters list
        public List<string> LeaveTypeList { get; }
        public List<string> LeaveTypeFilterList { get; }
        public List<string> StatusFilterList { get; }

        public ObservableCollection<LeaveRequest> LeaveRequests
        {
            get => _leaveRequests;
            set => SetProperty(ref _leaveRequests, value);
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        #region Filters & Properties
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        public string SelectedLeaveTypeFilter
        {
            get => _selectedLeaveTypeFilter;
            set
            {
                if (SetProperty(ref _selectedLeaveTypeFilter, value))
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

        public DateTime? FilterFromDate
        {
            get => _filterFromDate;
            set
            {
                if (SetProperty(ref _filterFromDate, value))
                    ApplyFilters();
            }
        }

        public DateTime? FilterToDate
        {
            get => _filterToDate;
            set
            {
                if (SetProperty(ref _filterToDate, value))
                    ApplyFilters();
            }
        }

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
        #endregion

        #region Permission Properties
        private bool _canApproveReject = false;
        public bool CanApproveReject
        {
            get => _canApproveReject;
            set => SetProperty(ref _canApproveReject, value);
        }

        private bool _canSelectEmployee = true;
        public bool CanSelectEmployee
        {
            get => _canSelectEmployee;
            set => SetProperty(ref _canSelectEmployee, value);
        }

        private void EvaluatePermissionStates()
        {
            var user = UserSession.CurrentUser;
            if (user == null)
            {
                CanApproveReject = true;
                CanSelectEmployee = true;
                return;
            }

            // Employee role cannot approve / reject and cannot select other employee
            if (user.Role == "Employee")
            {
                CanApproveReject = false;
                CanSelectEmployee = false;
                // Double secure form select matches current employee only
                if (user.EmployeeId.HasValue)
                {
                    SelectedEmployeeForForm = Employees.FirstOrDefault(e => e.Id == user.EmployeeId.Value);
                }
            }
            else // Admin, HR, Manager
            {
                CanApproveReject = true;
                CanSelectEmployee = true;
            }
        }
        #endregion

        #region Form Editing Properties
        public Employee? SelectedEmployeeForForm
        {
            get => _selectedEmployeeForForm;
            set
            {
                if (SetProperty(ref _selectedEmployeeForForm, value))
                {
                    _ = UpdateRemainingLeaveTextAsync();
                }
            }
        }

        public string EditingLeaveType
        {
            get => _editingLeaveType;
            set => SetProperty(ref _editingLeaveType, value);
        }

        public DateTime? EditingFromDate
        {
            get => _editingFromDate;
            set
            {
                if (SetProperty(ref _editingFromDate, value))
                {
                    RecalculateTotalDays();
                }
            }
        }

        public DateTime? EditingToDate
        {
            get => _editingToDate;
            set
            {
                if (SetProperty(ref _editingToDate, value))
                {
                    RecalculateTotalDays();
                }
            }
        }

        public double EditingTotalDays
        {
            get => _editingTotalDays;
            set => SetProperty(ref _editingTotalDays, value);
        }

        public string EditingReason
        {
            get => _editingReason;
            set => SetProperty(ref _editingReason, value);
        }

        public string EditingStatus
        {
            get => _editingStatus;
            set => SetProperty(ref _editingStatus, value);
        }

        public string EditingRejectReason
        {
            get => _editingRejectReason;
            set => SetProperty(ref _editingRejectReason, value);
        }

        public string EmployeeRemainingLeaveText
        {
            get => _employeeRemainingLeaveText;
            set => SetProperty(ref _employeeRemainingLeaveText, value);
        }

        private void RecalculateTotalDays()
        {
            if (EditingFromDate.HasValue && EditingToDate.HasValue)
            {
                if (EditingToDate.Value.Date >= EditingFromDate.Value.Date)
                {
                    EditingTotalDays = (EditingToDate.Value.Date - EditingFromDate.Value.Date).Days + 1;
                }
                else
                {
                    EditingTotalDays = 0;
                }
            }
        }

        private async Task UpdateRemainingLeaveTextAsync()
        {
            if (SelectedEmployeeForForm != null)
            {
                double remaining = await _leaveRequestService.GetRemainingAnnualLeaveDaysAsync(SelectedEmployeeForForm.Id, DateTime.Today.Year);
                EmployeeRemainingLeaveText = $"Số ngày phép năm còn lại trong năm {DateTime.Today.Year}: {remaining} / 12 ngày.";
            }
            else
            {
                EmployeeRemainingLeaveText = "Chọn nhân viên để xem số ngày phép năm còn lại.";
            }
        }
        #endregion

        #region Selection Active
        public LeaveRequest? SelectedLeaveRequest
        {
            get => _selectedLeaveRequest;
            set
            {
                if (SetProperty(ref _selectedLeaveRequest, value))
                {
                    if (value != null)
                    {
                        // Bind to form for view/edit
                        SelectedEmployeeForForm = Employees.FirstOrDefault(e => e.Id == value.EmployeeId);
                        EditingLeaveType = value.LeaveType;
                        EditingFromDate = value.FromDate;
                        EditingToDate = value.ToDate;
                        EditingTotalDays = value.TotalDays;
                        EditingReason = value.Reason;
                        EditingStatus = value.Status;
                        EditingRejectReason = value.RejectReason ?? string.Empty;

                        ClearMessages();
                    }
                }
            }
        }
        #endregion

        #region Commands Commands
        public ICommand LoadLeaveRequestsCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ResetFormCommand { get; }

        private void ClearMessages()
        {
            SuccessMsg = string.Empty;
            ErrorMsg = string.Empty;
        }

        private async Task ExecuteLoadLeaveRequestsAsync()
        {
            ClearMessages();
            try
            {
                var all = await _leaveRequestService.GetLeaveRequestsAsync();
                var user = UserSession.CurrentUser;

                // Authorization rules on backend fetch
                if (user == null || user.Role == "Admin" || user.Role == "HR" || user.Role == "Giám đốc")
                {
                    _allLoadedLeaveRequests = all;
                }
                else if (user.Role == "Manager")
                {
                    if (user.EmployeeId.HasValue)
                    {
                        var activeEmps = await _leaveRequestService.GetActiveEmployeesAsync();
                        var managerEmp = activeEmps.FirstOrDefault(e => e.Id == user.EmployeeId.Value);
                        if (managerEmp != null)
                        {
                            var deptEmployeeIds = activeEmps
                                .Where(e => e.DepartmentId == managerEmp.DepartmentId)
                                .Select(e => e.Id)
                                .ToList();

                            _allLoadedLeaveRequests = all.Where(r => deptEmployeeIds.Contains(r.EmployeeId)).ToList();
                        }
                        else
                        {
                            _allLoadedLeaveRequests = all;
                        }
                    }
                    else
                    {
                        _allLoadedLeaveRequests = all;
                    }
                }
                else // Employee
                {
                    if (user.EmployeeId.HasValue)
                    {
                        _allLoadedLeaveRequests = all.Where(r => r.EmployeeId == user.EmployeeId.Value).ToList();
                    }
                    else
                    {
                        _allLoadedLeaveRequests = new List<LeaveRequest>();
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Không thể tải danh sách phép: {ex.Message}";
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allLoadedLeaveRequests.AsEnumerable();

            // 1. Text search (EmployeeCode, FullName, Reason)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string query = SearchText.ToLower();
                filtered = filtered.Where(r => 
                    r.Reason.ToLower().Contains(query) ||
                    (r.Employee?.FullName != null && r.Employee.FullName.ToLower().Contains(query)) ||
                    (r.Employee?.EmployeeCode != null && r.Employee.EmployeeCode.ToLower().Contains(query))
                );
            }

            // 2. Type Filter
            if (SelectedLeaveTypeFilter != "Tất cả")
            {
                filtered = filtered.Where(r => r.LeaveType == SelectedLeaveTypeFilter);
            }

            // 3. Status Filter
            if (SelectedStatusFilter != "Tất cả")
            {
                filtered = filtered.Where(r => r.Status == SelectedStatusFilter);
            }

            // 4. FromDate filter
            if (FilterFromDate.HasValue)
            {
                filtered = filtered.Where(r => r.FromDate.Date >= FilterFromDate.Value.Date);
            }

            // 5. ToDate filter
            if (FilterToDate.HasValue)
            {
                filtered = filtered.Where(r => r.ToDate.Date <= FilterToDate.Value.Date);
            }

            LeaveRequests = new ObservableCollection<LeaveRequest>(filtered);
        }

        private async Task ExecuteAddAsync()
        {
            ClearMessages();

            if (SelectedEmployeeForForm == null)
            {
                ErrorMsg = "Vui lòng chọn nhân viên nộp đơn phép.";
                return;
            }

            if (EditingFromDate == null || EditingToDate == null)
            {
                ErrorMsg = "Vui lòng nhập đầy đủ từ ngày và đến ngày.";
                return;
            }

            if (EditingFromDate.Value.Date > EditingToDate.Value.Date)
            {
                ErrorMsg = "Ngày bắt đầu phép không được lớn hơn ngày kết thúc.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingReason))
            {
                ErrorMsg = "Vui lòng nhập rõ lý do xin phép nghỉ.";
                return;
            }

            var newRequest = new LeaveRequest
            {
                EmployeeId = SelectedEmployeeForForm.Id,
                LeaveType = EditingLeaveType,
                FromDate = EditingFromDate.Value,
                ToDate = EditingToDate.Value,
                TotalDays = EditingTotalDays,
                Reason = EditingReason,
                Status = "Chờ duyệt"
            };

            var (success, errorMsg) = await _leaveRequestService.AddLeaveRequestAsync(newRequest);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadLeaveRequestsAsync();
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

            if (SelectedLeaveRequest == null)
            {
                ErrorMsg = "Chưa chọn đơn nghỉ phép bất kỳ từ bảng để sửa đổi.";
                return;
            }

            if (SelectedLeaveRequest.Status != "Chờ duyệt")
            {
                ErrorMsg = "Không cho phép chỉnh sửa các đơn nghỉ phép đã được duyệt, bị từ chối hoặc đã hủy.";
                return;
            }

            if (EditingFromDate == null || EditingToDate == null)
            {
                ErrorMsg = "Ngày xin nghỉ không được trống.";
                return;
            }

            if (EditingFromDate.Value.Date > EditingToDate.Value.Date)
            {
                ErrorMsg = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingReason))
            {
                ErrorMsg = "Lý do xin phép không được để trống.";
                return;
            }

            var dataToUpdate = new LeaveRequest
            {
                Id = SelectedLeaveRequest.Id,
                EmployeeId = SelectedLeaveRequest.EmployeeId,
                LeaveType = EditingLeaveType,
                FromDate = EditingFromDate.Value,
                ToDate = EditingToDate.Value,
                TotalDays = EditingTotalDays,
                Reason = EditingReason
            };

            var (success, errorMsg) = await _leaveRequestService.UpdateLeaveRequestAsync(dataToUpdate);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadLeaveRequestsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private async Task ExecuteCancelAsync()
        {
            ClearMessages();

            if (SelectedLeaveRequest == null)
            {
                ErrorMsg = "Chưa chọn đơn nghỉ phép từ bảng để tiến hành hủy đơn.";
                return;
            }

            if (SelectedLeaveRequest.Status != "Chờ duyệt")
            {
                ErrorMsg = "Không thể hủy đơn nghỉ phép đã được duyệt hoặc từ chối trước đó.";
                return;
            }

            var (success, errorMsg) = await _leaveRequestService.CancelLeaveRequestAsync(SelectedLeaveRequest.Id);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadLeaveRequestsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private async Task ExecuteApproveAsync()
        {
            ClearMessages();

            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee")
            {
                ErrorMsg = "Chức năng phê duyệt chỉ dành riêng cho HR, Manager hoặc Admin.";
                return;
            }

            if (SelectedLeaveRequest == null)
            {
                ErrorMsg = "Vui lòng chọn đơn nghỉ phép từ danh sách để duyệt.";
                return;
            }

            if (SelectedLeaveRequest.Status != "Chờ duyệt")
            {
                ErrorMsg = "Đơn nghỉ này đã được duyệt hoặc từ chối từ trước.";
                return;
            }

            int approverId = user?.EmployeeId ?? 1; // Default to admin reference

            var (success, errorMsg) = await _leaveRequestService.ApproveLeaveRequestAsync(SelectedLeaveRequest.Id, approverId);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadLeaveRequestsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private async Task ExecuteRejectAsync()
        {
            ClearMessages();

            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee")
            {
                ErrorMsg = "Chức năng từ chối đơn chỉ dành riêng cho HR, Manager hoặc Admin.";
                return;
            }

            if (SelectedLeaveRequest == null)
            {
                ErrorMsg = "Vui lòng chọn đơn nghỉ phép từ danh sách để từ chối.";
                return;
            }

            if (SelectedLeaveRequest.Status != "Chờ duyệt")
            {
                ErrorMsg = "Đơn nghỉ này đã được giải quyết từ trước.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingRejectReason))
            {
                ErrorMsg = "Vui lòng nhập đầy đủ lý do từ chối đơn nghỉ phép.";
                return;
            }

            int approverId = user?.EmployeeId ?? 1;

            var (success, errorMsg) = await _leaveRequestService.RejectLeaveRequestAsync(SelectedLeaveRequest.Id, approverId, EditingRejectReason);
            if (success)
            {
                SuccessMsg = errorMsg;
                await ExecuteLoadLeaveRequestsAsync();
                ExecuteResetForm();
            }
            else
            {
                ErrorMsg = errorMsg;
            }
        }

        private void ExecuteResetForm()
        {
            SelectedLeaveRequest = null;

            // Retain current employee lock if they are employee role
            var user = UserSession.CurrentUser;
            if (user != null && user.Role == "Employee" && user.EmployeeId.HasValue)
            {
                SelectedEmployeeForForm = Employees.FirstOrDefault(e => e.Id == user.EmployeeId.Value);
            }
            else
            {
                SelectedEmployeeForForm = null;
            }

            EditingLeaveType = "Phép năm";
            EditingFromDate = DateTime.Today;
            EditingToDate = DateTime.Today;
            EditingTotalDays = 1;
            EditingReason = string.Empty;
            EditingStatus = "Chờ duyệt";
            EditingRejectReason = string.Empty;

            ClearMessages();
        }
        #endregion
    }
}
