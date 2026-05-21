using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Commands;

namespace HRM_WPF_CNPM.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private readonly PayrollService? _payrollService;
        private ObservableCollection<Employee> _employees;
        private Employee? _selectedEmployee;
        private string _searchText = string.Empty;
        private EmployeeDetailViewModel? _currentDetailViewModel;

        public EmployeeViewModel(EmployeeService employeeService, PayrollService? payrollService = null)
        {
            _employeeService = employeeService;
            _payrollService = payrollService;
            _employees = new ObservableCollection<Employee>();

            ViewDetailCommand = new RelayCommand(ExecuteViewDetail, CanExecuteViewDetail);
            LoadEmployeesCommand = new RelayCommand(async _ => await LoadEmployeesAsync());

            _ = LoadEmployeesAsync();
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                {
                    // Refresh command state whenever selection changes
                    (ViewDetailCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = FilterEmployeesAsync();
                }
            }
        }

        public EmployeeDetailViewModel? CurrentDetailViewModel
        {
            get => _currentDetailViewModel;
            set => SetProperty(ref _currentDetailViewModel, value);
        }

        public ICommand ViewDetailCommand { get; }
        public ICommand LoadEmployeesCommand { get; }

        public async Task LoadEmployeesAsync()
        {
            var list = await _employeeService.GetEmployeesAsync();
            Employees = new ObservableCollection<Employee>(list);
        }

        private async Task FilterEmployeesAsync()
        {
            var all = await _employeeService.GetEmployeesAsync();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Employees = new ObservableCollection<Employee>(all);
            }
            else
            {
                var query = SearchText.ToLower();
                var filtered = all.Where(e => 
                    e.EmployeeCode.ToLower().Contains(query) || 
                    e.FullName.ToLower().Contains(query) || 
                    (e.Email != null && e.Email.ToLower().Contains(query)) ||
                    (e.Department != null && e.Department.DepartmentName.ToLower().Contains(query))
                );
                Employees = new ObservableCollection<Employee>(filtered);
            }
        }

        private bool CanExecuteViewDetail(object? parameter)
        {
            return SelectedEmployee != null;
        }

        private void ExecuteViewDetail(object? parameter)
        {
            if (SelectedEmployee == null) return;

            var detailVM = new EmployeeDetailViewModel(SelectedEmployee, _employeeService, _payrollService);
            detailVM.CloseRequested += (sender, args) =>
            {
                // Return back to list view
                CurrentDetailViewModel = null;
            };

            CurrentDetailViewModel = detailVM;
        }
    }
}
