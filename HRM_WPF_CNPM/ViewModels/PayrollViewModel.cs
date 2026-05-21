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
    public class PayrollViewModel : BaseViewModel
    {
        private readonly PayrollService _payrollService;
        private readonly EmployeeService _employeeService;

        private ObservableCollection<Payroll> _payrolls = new();
        private List<Payroll> _allLoadedPayrolls = new();

        // Dropdowns and Filters
        private string _searchText = string.Empty;
        private string _selectedStatusFilter = "Tất cả";
        private string _selectedMonth = DateTime.Today.ToString("MM");
        private string _selectedYear = DateTime.Today.ToString("yyyy");

        // Selected Row Details
        private Payroll? _selectedPayroll;

        // Form fields for editing
        private string _detailEmployeeName = string.Empty;
        private string _detailEmployeeCode = string.Empty;
        private string _detailMonth = string.Empty;
        private decimal _detailBaseSalary;
        private decimal _detailStandardDays;
        private decimal _detailActualDays;
        private decimal _detailBonus;
        private decimal _detailPenalty;
        private decimal _detailNetSalary;
        private string _detailStatus = string.Empty;
        private bool _detailIsLocked;

        // Alerts / Messages
        private string _successMsg = string.Empty;
        private string _errorMsg = string.Empty;

        // Permissions
        private bool _canManage = false; // Only Admin or HR can create/calculate/lock
        private bool _canEditBonusPenalty = false; // Admin or HR, but if IsLocked only Admin
        private bool _isEmployeeView = false;

        public PayrollViewModel(PayrollService payrollService, EmployeeService employeeService)
        {
            _payrollService = payrollService;
            _employeeService = employeeService;

            // Load filters dropdown lists
            MonthsList = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            YearsList = new List<string> { "2024", "2025", "2026", "2027" };
            StatusFilterList = new List<string> { "Tất cả", "Nháp", "Đã tính", "Đã chốt" };

            // Commands
            LoadPayrollsCommand = new RelayCommand(async _ => await ExecuteLoadPayrollsAsync());
            CreatePayrollCommand = new RelayCommand(async _ => await ExecuteCreatePayrollAsync());
            CalculateSelectedCommand = new RelayCommand(async _ => await ExecuteCalculateSelectedAsync());
            CalculateAllCommand = new RelayCommand(async _ => await ExecuteCalculateAllAsync());
            UpdateBonusPenaltyCommand = new RelayCommand(async _ => await ExecuteUpdateBonusPenaltyAsync());
            LockPayrollCommand = new RelayCommand(async _ => await ExecuteLockPayrollAsync());
            ResetFormCommand = new RelayCommand(_ => ExecuteResetForm());

            EvaluatePermissions();
            _ = ExecuteLoadPayrollsAsync();
        }

        #region Filters and Data Lists
        public List<string> MonthsList { get; }
        public List<string> YearsList { get; }
        public List<string> StatusFilterList { get; }

        public ObservableCollection<Payroll> Payrolls
        {
            get => _payrolls;
            set => SetProperty(ref _payrolls, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    _ = ExecuteLoadPayrollsAsync();
                }
            }
        }

        public string SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _ = ExecuteLoadPayrollsAsync();
                }
            }
        }

        public Payroll? SelectedPayroll
        {
            get => _selectedPayroll;
            set
            {
                if (SetProperty(ref _selectedPayroll, value))
                {
                    OnSelectedPayrollChanged();
                }
            }
        }
        #endregion

        #region Form properties
        public string DetailEmployeeName
        {
            get => _detailEmployeeName;
            set => SetProperty(ref _detailEmployeeName, value);
        }

        public string DetailEmployeeCode
        {
            get => _detailEmployeeCode;
            set => SetProperty(ref _detailEmployeeCode, value);
        }

        public string DetailMonth
        {
            get => _detailMonth;
            set => SetProperty(ref _detailMonth, value);
        }

        public decimal DetailBaseSalary
        {
            get => _detailBaseSalary;
            set => SetProperty(ref _detailBaseSalary, value);
        }

        public decimal DetailStandardDays
        {
            get => _detailStandardDays;
            set => SetProperty(ref _detailStandardDays, value);
        }

        public decimal DetailActualDays
        {
            get => _detailActualDays;
            set => SetProperty(ref _detailActualDays, value);
        }

        public decimal DetailBonus
        {
            get => _detailBonus;
            set => SetProperty(ref _detailBonus, value);
        }

        public decimal DetailPenalty
        {
            get => _detailPenalty;
            set => SetProperty(ref _detailPenalty, value);
        }

        public decimal DetailNetSalary
        {
            get => _detailNetSalary;
            set => SetProperty(ref _detailNetSalary, value);
        }

        public string DetailStatus
        {
            get => _detailStatus;
            set => SetProperty(ref _detailStatus, value);
        }

        public bool DetailIsLocked
        {
            get => _detailIsLocked;
            set => SetProperty(ref _detailIsLocked, value);
        }
        #endregion

        #region Alerts / Controls
        public string SuccessMsg
        {
            get => _successMsg;
            set
            {
                if (SetProperty(ref _successMsg, value) && !string.IsNullOrEmpty(value))
                {
                    ErrorMsg = string.Empty;
                }
            }
        }

        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (SetProperty(ref _errorMsg, value) && !string.IsNullOrEmpty(value))
                {
                    SuccessMsg = string.Empty;
                }
            }
        }

        public bool CanManage
        {
            get => _canManage;
            set => SetProperty(ref _canManage, value);
        }

        public bool CanEditBonusPenalty
        {
            get => _canEditBonusPenalty;
            set => SetProperty(ref _canEditBonusPenalty, value);
        }

        public bool IsEmployeeView
        {
            get => _isEmployeeView;
            set => SetProperty(ref _isEmployeeView, value);
        }
        #endregion

        #region Operations / Methods
        private void EvaluatePermissions()
        {
            var user = UserSession.CurrentUser;
            if (user == null)
            {
                CanManage = true;
                CanEditBonusPenalty = true;
                IsEmployeeView = false;
                return;
            }

            IsEmployeeView = user.Role == "Employee";

            // Only Admin or HR or Giám đốc can calculate/create/lock payrolls
            if (user.Role == "Admin" || user.Role == "HR" || user.Role == "Giám đốc")
            {
                CanManage = true;
            }
            else
            {
                CanManage = false;
            }

            EvaluateSelectedEditPermission();
        }

        private void EvaluateSelectedEditPermission()
        {
            var user = UserSession.CurrentUser;
            if (SelectedPayroll == null || !CanManage)
            {
                CanEditBonusPenalty = false;
                return;
            }

            if (SelectedPayroll.IsLocked)
            {
                // Only Admin can override a locked record
                CanEditBonusPenalty = user?.Role == "Admin";
            }
            else
            {
                CanEditBonusPenalty = true;
            }
        }

        private void OnSelectedPayrollChanged()
        {
            if (SelectedPayroll == null)
            {
                ExecuteResetForm();
                return;
            }

            DetailEmployeeName = SelectedPayroll.Employee?.FullName ?? "N/A";
            DetailEmployeeCode = SelectedPayroll.Employee?.EmployeeCode ?? "N/A";
            DetailMonth = SelectedPayroll.PayrollMonth;
            DetailBaseSalary = SelectedPayroll.BaseSalary;
            DetailStandardDays = SelectedPayroll.StandardWorkDays;
            DetailActualDays = SelectedPayroll.ActualWorkDays;
            DetailBonus = SelectedPayroll.Bonus;
            DetailPenalty = SelectedPayroll.Penalty;
            DetailNetSalary = SelectedPayroll.NetSalary;
            DetailStatus = SelectedPayroll.Status;
            DetailIsLocked = SelectedPayroll.IsLocked;

            EvaluateSelectedEditPermission();
        }

        private void ExecuteResetForm()
        {
            DetailEmployeeName = string.Empty;
            DetailEmployeeCode = string.Empty;
            DetailMonth = string.Empty;
            DetailBaseSalary = 0;
            DetailStandardDays = 26;
            DetailActualDays = 0;
            DetailBonus = 0;
            DetailPenalty = 0;
            DetailNetSalary = 0;
            DetailStatus = string.Empty;
            DetailIsLocked = false;
            CanEditBonusPenalty = false;
        }

        public async Task ExecuteLoadPayrollsAsync()
        {
            try
            {
                string monthQuery = $"{SelectedMonth}/{SelectedYear}";
                var user = UserSession.CurrentUser;

                List<Payroll> list;
                if (user != null && user.Role == "Employee" && user.EmployeeId.HasValue)
                {
                    // Employee only sees their own regardless of Month or filters
                    list = await _payrollService.GetPayrollsByEmployeeIdAsync(user.EmployeeId.Value);
                }
                else
                {
                    // Managers or Admins/HR gets the selected month
                    list = await _payrollService.GetPayrollsAsync(monthQuery);
                }

                _allLoadedPayrolls = list;
                ApplyFilters();
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Có lỗi xảy ra khi tải bảng lương: {ex.Message}";
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allLoadedPayrolls.AsEnumerable();

            // Status filter
            if (SelectedStatusFilter != "Tất cả")
            {
                filtered = filtered.Where(p => p.Status == SelectedStatusFilter);
            }

            // Search text filter (employee code or full name)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string term = SearchText.Trim().ToLower();
                filtered = filtered.Where(p => 
                    (p.Employee != null && (p.Employee.FullName.ToLower().Contains(term) || p.Employee.EmployeeCode.ToLower().Contains(term)))
                );
            }

            Payrolls = new ObservableCollection<Payroll>(filtered.OrderBy(p => p.Employee?.EmployeeCode));
        }

        private async Task ExecuteCreatePayrollAsync()
        {
            if (!CanManage)
            {
                ErrorMsg = "Bạn không có quyền thực hiện chức năng tạo bảng lương.";
                return;
            }

            string monthQuery = $"{SelectedMonth}/{SelectedYear}";
            var result = await _payrollService.CreatePayrollsForMonthAsync(monthQuery, 26);
            if (result.IsSuccess)
            {
                SuccessMsg = result.ErrorMsg; // Contains info count
                await ExecuteLoadPayrollsAsync();
            }
            else
            {
                ErrorMsg = result.ErrorMsg;
            }
        }

        private async Task ExecuteCalculateSelectedAsync()
        {
            if (SelectedPayroll == null)
            {
                ErrorMsg = "Vui lòng chọn một nhân sự từ danh sách bảng lương để tính.";
                return;
            }

            if (!CanManage)
            {
                ErrorMsg = "Bạn không có quyền tính lại bảng lương.";
                return;
            }

            var result = await _payrollService.CalculatePayrollAsync(SelectedPayroll.Id);
            if (result.IsSuccess)
            {
                SuccessMsg = result.ErrorMsg;
                await RefreshSelectedRowAsync();
            }
            else
            {
                ErrorMsg = result.ErrorMsg;
            }
        }

        private async Task ExecuteCalculateAllAsync()
        {
            if (!CanManage)
            {
                ErrorMsg = "Bạn không có quyền tính toán toàn bộ bảng lương.";
                return;
            }

            string monthQuery = $"{SelectedMonth}/{SelectedYear}";
            var result = await _payrollService.CalculateAllForMonthAsync(monthQuery);
            if (result.IsSuccess)
            {
                SuccessMsg = result.ErrorMsg;
                await ExecuteLoadPayrollsAsync();
            }
            else
            {
                ErrorMsg = result.ErrorMsg;
            }
        }

        private async Task ExecuteUpdateBonusPenaltyAsync()
        {
            if (SelectedPayroll == null)
            {
                ErrorMsg = "Vui lòng chọn một bản ghi bảng lương để cập nhật thưởng/phạt.";
                return;
            }

            EvaluateSelectedEditPermission();
            if (!CanEditBonusPenalty)
            {
                ErrorMsg = "Bạn không có quyền cập nhật bản ghi bảng lương đã chốt/khóa.";
                return;
            }

            if (DetailBonus < 0)
            {
                ErrorMsg = "Khoản Thưởng không được âm.";
                return;
            }

            if (DetailPenalty < 0)
            {
                ErrorMsg = "Khoản Phạt không được âm.";
                return;
            }

            var result = await _payrollService.UpdateBonusPenaltyAsync(SelectedPayroll.Id, DetailBonus, DetailPenalty);
            if (result.IsSuccess)
            {
                SuccessMsg = result.ErrorMsg;
                await RefreshSelectedRowAsync();
            }
            else
            {
                ErrorMsg = result.ErrorMsg;
            }
        }

        private async Task ExecuteLockPayrollAsync()
        {
            if (!CanManage)
            {
                ErrorMsg = "Chỉ Admin hoặc HR có quyền thực hiện chốt bảng lương.";
                return;
            }

            string monthQuery = $"{SelectedMonth}/{SelectedYear}";
            var result = await _payrollService.LockPayrollsForMonthAsync(monthQuery);
            if (result.IsSuccess)
            {
                SuccessMsg = result.ErrorMsg;
                await ExecuteLoadPayrollsAsync();
                EvaluateSelectedEditPermission();
            }
            else
            {
                ErrorMsg = result.ErrorMsg;
            }
        }

        private async Task RefreshSelectedRowAsync()
        {
            int savedId = SelectedPayroll?.Id ?? 0;
            await ExecuteLoadPayrollsAsync();
            if (savedId != 0)
            {
                SelectedPayroll = Payrolls.FirstOrDefault(p => p.Id == savedId);
            }
        }
        #endregion

        #region Commands Wiring
        public ICommand LoadPayrollsCommand { get; }
        public ICommand CreatePayrollCommand { get; }
        public ICommand CalculateSelectedCommand { get; }
        public ICommand CalculateAllCommand { get; }
        public ICommand UpdateBonusPenaltyCommand { get; }
        public ICommand LockPayrollCommand { get; }
        public ICommand ResetFormCommand { get; }
        #endregion
    }
}
