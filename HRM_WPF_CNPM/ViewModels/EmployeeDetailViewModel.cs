using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Helpers;
using HRM_WPF_CNPM.Commands;

namespace HRM_WPF_CNPM.ViewModels
{
    public class EmployeeDetailViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly PayrollService _payrollService;
        private Employee _employee;
        private bool _canViewSalary;
        private ObservableCollection<Contract> _contracts;
        private ObservableCollection<LeaveRequest> _leaveRequests;
        private ObservableCollection<AttendanceRecord> _attendanceRecords;
        private ObservableCollection<Payroll> _payrolls;

        public event EventHandler? CloseRequested;

        public EmployeeDetailViewModel(Employee employee, EmployeeService employeeService, PayrollService? payrollService = null)
        {
            _employee = employee;
            _employeeService = employeeService;
            _payrollService = payrollService ?? new PayrollService(null!, new NotificationService());
            _contracts = new ObservableCollection<Contract>();
            _leaveRequests = new ObservableCollection<LeaveRequest>();
            _attendanceRecords = new ObservableCollection<AttendanceRecord>();
            _payrolls = new ObservableCollection<Payroll>();

            BackCommand = new RelayCommand(ExecuteBack);
            EvaluateSalaryVisibility();
            
            // Asynchronously load the related details
            _ = LoadDetailsAsync();
        }

        public Employee Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        public bool CanViewSalary
        {
            get => _canViewSalary;
            private set => SetProperty(ref _canViewSalary, value);
        }

        public ObservableCollection<Contract> Contracts
        {
            get => _contracts;
            set
            {
                if (SetProperty(ref _contracts, value))
                {
                    OnPropertyChanged(nameof(HasContracts));
                    OnPropertyChanged(nameof(NoContracts));
                }
            }
        }

        public bool HasContracts => Contracts != null && Contracts.Count > 0;
        public bool NoContracts => !HasContracts;

        public ObservableCollection<LeaveRequest> LeaveRequests
        {
            get => _leaveRequests;
            set
            {
                if (SetProperty(ref _leaveRequests, value))
                {
                    OnPropertyChanged(nameof(HasLeaveRequests));
                    OnPropertyChanged(nameof(NoLeaveRequests));
                }
            }
        }

        public bool HasLeaveRequests => LeaveRequests != null && LeaveRequests.Count > 0;
        public bool NoLeaveRequests => !HasLeaveRequests;

        public ObservableCollection<AttendanceRecord> AttendanceRecords
        {
            get => _attendanceRecords;
            set
            {
                if (SetProperty(ref _attendanceRecords, value))
                {
                    OnPropertyChanged(nameof(HasAttendanceRecords));
                    OnPropertyChanged(nameof(NoAttendanceRecords));
                }
            }
        }

        public bool HasAttendanceRecords => AttendanceRecords != null && AttendanceRecords.Count > 0;
        public bool NoAttendanceRecords => !HasAttendanceRecords;

        public ObservableCollection<Payroll> Payrolls
        {
            get => _payrolls;
            set
            {
                if (SetProperty(ref _payrolls, value))
                {
                    OnPropertyChanged(nameof(HasPayrolls));
                    OnPropertyChanged(nameof(NoPayrolls));
                }
            }
        }

        public bool HasPayrolls => Payrolls != null && Payrolls.Count > 0;
        public bool NoPayrolls => !HasPayrolls;

        public ICommand BackCommand { get; }

        private void EvaluateSalaryVisibility()
        {
            var currentUser = UserSession.CurrentUser;
            if (currentUser == null)
            {
                CanViewSalary = false;
                return;
            }

            // Can see salary if:
            // 1. Logged in user has Role equal to: Admin, HR, Giám đốc, Kế toán
            // 2. Or if the employee of this detail is the actual logged in employee matching EmployeeId
            bool isHighRole = currentUser.Role == "Admin" || 
                              currentUser.Role == "HR" || 
                              currentUser.Role == "Giám đốc" || 
                              currentUser.Role == "Kế toán";

            bool isSelf = currentUser.EmployeeId != null && currentUser.EmployeeId == Employee.Id;

            CanViewSalary = isHighRole || isSelf;
        }

        private async Task LoadDetailsAsync()
        {
            var contractsList = await _employeeService.GetContractsByEmployeeIdAsync(Employee.Id);
            var activeContracts = contractsList.Where(c => !c.IsDeleted).ToList();
            Contracts = new ObservableCollection<Contract>(activeContracts);

            var leavesList = await _employeeService.GetLeaveRequestsByEmployeeIdAsync(Employee.Id);
            LeaveRequests = new ObservableCollection<LeaveRequest>(leavesList);

            var attendanceList = await _employeeService.GetAttendanceRecordsByEmployeeIdAsync(Employee.Id);
            var latestAttendance = attendanceList.OrderByDescending(a => a.WorkDate).Take(10).ToList();
            AttendanceRecords = new ObservableCollection<AttendanceRecord>(latestAttendance);

            if (CanViewSalary)
            {
                var payrollsList = await _payrollService.GetPayrollsByEmployeeIdAsync(Employee.Id);
                Payrolls = new ObservableCollection<Payroll>(payrollsList);
            }
        }

        private void ExecuteBack(object? parameter)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
