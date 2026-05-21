using System;
using System.Windows.Input;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Commands;

namespace HRM_WPF_CNPM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private readonly EmployeeService _employeeService;
        private readonly ContractService _contractService;
        private readonly LeaveRequestService? _leaveRequestService;
        private readonly AttendanceService? _attendanceService;
        private readonly PayrollService? _payrollService;
        private readonly AuditLogService? _auditLogService;

        // Cached viewmodels for quick navigation
        private EmployeeViewModel? _employeeViewModel;
        private ContractViewModel? _contractViewModel;
        private LeaveRequestViewModel? _leaveRequestViewModel;
        private AttendanceViewModel? _attendanceViewModel;
        private PayrollViewModel? _payrollViewModel;
        private AuditLogViewModel? _auditLogViewModel;

        private string _activeMenu = "Employees";

        public MainViewModel(EmployeeService employeeService, ContractService contractService)
            : this(employeeService, contractService, null, null, null, null)
        {
        }

        public MainViewModel(EmployeeService employeeService, ContractService contractService, LeaveRequestService? leaveRequestService)
            : this(employeeService, contractService, leaveRequestService, null, null, null)
        {
        }

        public MainViewModel(EmployeeService employeeService, ContractService contractService, LeaveRequestService? leaveRequestService, AttendanceService? attendanceService)
            : this(employeeService, contractService, leaveRequestService, attendanceService, null, null)
        {
        }

        public MainViewModel(EmployeeService employeeService, ContractService contractService, LeaveRequestService? leaveRequestService, AttendanceService? attendanceService, PayrollService? payrollService)
            : this(employeeService, contractService, leaveRequestService, attendanceService, payrollService, null)
        {
        }

        public MainViewModel(EmployeeService employeeService, ContractService contractService, LeaveRequestService? leaveRequestService, AttendanceService? attendanceService, PayrollService? payrollService, AuditLogService? auditLogService)
        {
            _employeeService = employeeService;
            _contractService = contractService;
            _leaveRequestService = leaveRequestService;
            _attendanceService = attendanceService;
            _payrollService = payrollService;
            _auditLogService = auditLogService;

            // Initialize commands
            NavigateToEmployeeCommand = new RelayCommand(_ => ExecuteNavigateToEmployee());
            NavigateToContractCommand = new RelayCommand(_ => ExecuteNavigateToContract());
            NavigateToLeaveRequestCommand = new RelayCommand(_ => ExecuteNavigateToLeaveRequest());
            NavigateToAttendanceCommand = new RelayCommand(_ => ExecuteNavigateToAttendance());
            NavigateToPayrollCommand = new RelayCommand(_ => ExecuteNavigateToPayroll());
            NavigateToAuditLogCommand = new RelayCommand(_ => ExecuteNavigateToAuditLog());

            // Default view: Employees
            _employeeViewModel = new EmployeeViewModel(_employeeService, _payrollService);
            _currentViewModel = _employeeViewModel;
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string ActiveMenu
        {
            get => _activeMenu;
            set => SetProperty(ref _activeMenu, value);
        }

        public ICommand NavigateToEmployeeCommand { get; }
        public ICommand NavigateToContractCommand { get; }
        public ICommand NavigateToLeaveRequestCommand { get; }
        public ICommand NavigateToAttendanceCommand { get; }
        public ICommand NavigateToPayrollCommand { get; }
        public ICommand NavigateToAuditLogCommand { get; }

        private void ExecuteNavigateToEmployee()
        {
            _employeeViewModel ??= new EmployeeViewModel(_employeeService, _payrollService);
            CurrentViewModel = _employeeViewModel;
            ActiveMenu = "Employees";
        }

        private void ExecuteNavigateToContract()
        {
            _contractViewModel ??= new ContractViewModel(_contractService);
            CurrentViewModel = _contractViewModel;
            ActiveMenu = "Contracts";
        }

        private void ExecuteNavigateToLeaveRequest()
        {
            _leaveRequestViewModel ??= new LeaveRequestViewModel(_leaveRequestService ?? new LeaveRequestService(null!, new NotificationService()));
            CurrentViewModel = _leaveRequestViewModel;
            ActiveMenu = "LeaveRequests";
        }

        private void ExecuteNavigateToAttendance()
        {
            _attendanceViewModel ??= new AttendanceViewModel(_attendanceService ?? new AttendanceService(null!));
            CurrentViewModel = _attendanceViewModel;
            ActiveMenu = "Attendance";
        }

        private void ExecuteNavigateToPayroll()
        {
            _payrollViewModel ??= new PayrollViewModel(_payrollService ?? new PayrollService(null!, new NotificationService()), _employeeService);
            CurrentViewModel = _payrollViewModel;
            ActiveMenu = "Payroll";
        }

        private void ExecuteNavigateToAuditLog()
        {
            if (_auditLogViewModel == null)
            {
                var auditLogService = _auditLogService ?? new AuditLogService(_employeeService.Context);
                _auditLogViewModel = new AuditLogViewModel(auditLogService);
            }
            CurrentViewModel = _auditLogViewModel;
            ActiveMenu = "AuditLogs";
        }
    }
}
