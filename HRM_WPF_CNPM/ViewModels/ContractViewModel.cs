using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Commands;
using HRM_WPF_CNPM.DTOs;

namespace HRM_WPF_CNPM.ViewModels
{
    public class ContractViewModel : BaseViewModel
    {
        private readonly ContractService _contractService;
        
        // Lists
        private ObservableCollection<ContractListDto> _contracts;
        private List<Employee> _employees;
        private List<string> _contractTypes;
        private List<string> _statuses;

        // Selection
        private ContractListDto? _selectedContract;
        private Employee? _selectedEmployeeForForm;

        // Form Fields
        private string _editingContractCode = string.Empty;
        private string _editingContractType = string.Empty;
        private DateTime _editingStartDate = DateTime.Today;
        private DateTime? _editingEndDate = DateTime.Today.AddYears(1);
        private decimal _editingSalary = 0;
        private string _editingStatus = "Còn hiệu lực";
        private string? _editingNote;

        // Filters
        private string _searchText = string.Empty;
        private string _selectedTypeFilter = "Tất cả";
        private string _selectedStatusFilter = "Tất cả";
        private bool _isExpiringSoonFilter = false;

        // Messaging
        private string _errorMsg = string.Empty;
        private string _successMsg = string.Empty;

        public ContractViewModel(ContractService contractService)
        {
            _contractService = contractService;
            _contracts = new ObservableCollection<ContractListDto>();
            _employees = new List<Employee>();
            
            _contractTypes = new List<string>
            {
                "Thử việc",
                "1 năm",
                "3 năm",
                "Không xác định thời hạn"
            };

            _statuses = new List<string>
            {
                "Còn hiệu lực",
                "Sắp hết hạn",
                "Hết hạn",
                "Đã thanh lý"
            };

            // Setup Commands
            LoadContractsCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddCommand = new RelayCommand(async _ => await ExecuteAddAsync());
            UpdateCommand = new RelayCommand(async _ => await ExecuteUpdateAsync());
            DeleteCommand = new RelayCommand(async _ => await ExecuteDeleteAsync());
            ResetFormCommand = new RelayCommand(_ => ExecuteResetForm());
            ToggleExpiringFilterCommand = new RelayCommand(_ => { IsExpiringSoonFilter = !IsExpiringSoonFilter; });

            // Initialize background loading
            _ = LoadDataAsync();
        }

        #region Properties for Lists & ComboBoxes
        public ObservableCollection<ContractListDto> Contracts
        {
            get => _contracts;
            set => SetProperty(ref _contracts, value);
        }

        public List<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public List<string> ContractTypes => _contractTypes;
        
        public List<string> Statuses => _statuses;

        public List<string> TypeFilterList
        {
            get
            {
                var list = new List<string> { "Tất cả" };
                list.AddRange(_contractTypes);
                return list;
            }
        }

        public List<string> StatusFilterList
        {
            get
            {
                var list = new List<string> { "Tất cả" };
                list.AddRange(_statuses);
                return list;
            }
        }
        #endregion

        #region Selection & Bindable Form Fields
        public ContractListDto? SelectedContract
        {
            get => _selectedContract;
            set
            {
                if (SetProperty(ref _selectedContract, value))
                {
                    PopulateFormFromSelection();
                    (UpdateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public Employee? SelectedEmployeeForForm
        {
            get => _selectedEmployeeForForm;
            set => SetProperty(ref _selectedEmployeeForForm, value);
        }

        public string EditingContractCode
        {
            get => _editingContractCode;
            set => SetProperty(ref _editingContractCode, value);
        }

        public string EditingContractType
        {
            get => _editingContractType;
            set
            {
                if (SetProperty(ref _editingContractType, value))
                {
                    // AdjustEndDate conditionally based on Type request
                    if (_editingContractType == "Không xác định thời hạn")
                    {
                        EditingEndDate = null;
                    }
                    else if (_editingContractType == "Thử việc" && (!EditingEndDate.HasValue || EditingEndDate.Value <= EditingStartDate))
                    {
                        EditingEndDate = EditingStartDate.AddMonths(2);
                    }
                    else if (_editingContractType == "1 năm" && (!EditingEndDate.HasValue || EditingEndDate.Value <= EditingStartDate))
                    {
                        EditingEndDate = EditingStartDate.AddYears(1);
                    }
                    else if (_editingContractType == "3 năm" && (!EditingEndDate.HasValue || EditingEndDate.Value <= EditingStartDate))
                    {
                        EditingEndDate = EditingStartDate.AddYears(3);
                    }
                }
            }
        }

        public DateTime EditingStartDate
        {
            get => _editingStartDate;
            set => SetProperty(ref _editingStartDate, value);
        }

        public DateTime? EditingEndDate
        {
            get => _editingEndDate;
            set => SetProperty(ref _editingEndDate, value);
        }

        public decimal EditingSalary
        {
            get => _editingSalary;
            set => SetProperty(ref _editingSalary, value);
        }

        public string EditingStatus
        {
            get => _editingStatus;
            set => SetProperty(ref _editingStatus, value);
        }

        public string? EditingNote
        {
            get => _editingNote;
            set => SetProperty(ref _editingNote, value);
        }
        #endregion

        #region Filter Properties
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                if (SetProperty(ref _selectedTypeFilter, value))
                {
                    _ = ApplyFiltersAsync();
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
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public bool IsExpiringSoonFilter
        {
            get => _isExpiringSoonFilter;
            set
            {
                if (SetProperty(ref _isExpiringSoonFilter, value))
                {
                    if (value)
                    {
                        SelectedStatusFilter = "Sắp hết hạn";
                    }
                    else
                    {
                        _ = ApplyFiltersAsync();
                    }
                }
            }
        }
        #endregion

        #region Feedback Messaging
        public string ErrorMsg
        {
            get => _errorMsg;
            set => SetProperty(ref _errorMsg, value);
        }

        public string SuccessMsg
        {
            get => _successMsg;
            set => SetProperty(ref _successMsg, value);
        }
        #endregion

        #region Commands Declarations
        public ICommand LoadContractsCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetFormCommand { get; }
        public ICommand ToggleExpiringFilterCommand { get; }
        #endregion

        #region Data Access & Core Methods
        public async Task LoadDataAsync()
        {
            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;

            // Load employees list first to feed ComboBox
            var employeesList = await _contractService.GetActiveEmployeesAsync();
            Employees = employeesList;

            // Load contracts
            await ApplyFiltersAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            var rawList = await _contractService.GetContractsAsync();
            
            // Map models to DTOs for simple, clean presentation binding
            var dtos = rawList.Select(c => new ContractListDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                EmployeeCode = c.Employee?.EmployeeCode ?? $"NV{c.EmployeeId:D3}",
                EmployeeName = c.Employee?.FullName ?? $"Nhân viên {c.EmployeeId}",
                ContractCode = c.ContractCode,
                ContractType = c.ContractType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Salary = c.Salary,
                Status = c.Status,
                Note = c.Note
            }).ToList();

            // 1. Text Search Filter (ContractCode, EmployeeCode, EmployeeName)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchPattern = SearchText.ToLower();
                dtos = dtos.Where(d => 
                    d.ContractCode.ToLower().Contains(searchPattern) ||
                    d.EmployeeCode.ToLower().Contains(searchPattern) ||
                    d.EmployeeName.ToLower().Contains(searchPattern)
                ).ToList();
            }

            // 2. Type Filter
            if (SelectedTypeFilter != "Tất cả")
            {
                dtos = dtos.Where(d => d.ContractType == SelectedTypeFilter).ToList();
            }

            // 3. Status Filter or 30 Days Expiry check
            if (IsExpiringSoonFilter)
            {
                dtos = dtos.Where(d => d.Status == "Sắp hết hạn").ToList();
            }
            else if (SelectedStatusFilter != "Tất cả")
            {
                dtos = dtos.Where(d => d.Status == SelectedStatusFilter).ToList();
            }

            Contracts = new ObservableCollection<ContractListDto>(dtos);
        }

        private void PopulateFormFromSelection()
        {
            if (SelectedContract == null) return;

            EditingContractCode = SelectedContract.ContractCode;
            SelectedEmployeeForForm = Employees.FirstOrDefault(e => e.Id == SelectedContract.EmployeeId);
            EditingContractType = SelectedContract.ContractType;
            EditingStartDate = SelectedContract.StartDate;
            EditingEndDate = SelectedContract.EndDate;
            EditingSalary = SelectedContract.Salary;
            EditingStatus = SelectedContract.Status;
            EditingNote = SelectedContract.Note;

            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;
        }

        private void ExecuteResetForm()
        {
            SelectedContract = null;
            EditingContractCode = string.Empty;
            SelectedEmployeeForForm = null;
            EditingContractType = "1 năm";
            EditingStartDate = DateTime.Today;
            EditingEndDate = DateTime.Today.AddYears(1);
            EditingSalary = 0;
            EditingStatus = "Còn hiệu lực";
            EditingNote = string.Empty;

            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;
        }

        private bool ValidateForm(out string validationError)
        {
            validationError = string.Empty;

            if (string.IsNullOrWhiteSpace(EditingContractCode))
            {
                validationError = "Mã hợp đồng không được trống!";
                return false;
            }

            if (SelectedEmployeeForForm == null)
            {
                validationError = "Nhân viên không được để trống!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingContractType))
            {
                validationError = "Vui lòng chọn loại hợp đồng!";
                return false;
            }

            if (EditingStartDate == default)
            {
                validationError = "Ngày bắt đầu không được để trống!";
                return false;
            }

            if (EditingContractType != "Không xác định thời hạn")
            {
                if (!EditingEndDate.HasValue)
                {
                    validationError = "Ngày kết thúc không được để trống cho loại hợp đồng này!";
                    return false;
                }

                if (EditingEndDate.Value.Date < EditingStartDate.Date)
                {
                    validationError = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
                    return false;
                }
            }

            if (EditingSalary < 0)
            {
                validationError = "Mức lương thỏa thuận không được âm!";
                return false;
            }

            return true;
        }

        private async Task ExecuteAddAsync()
        {
            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;

            if (!ValidateForm(out string err))
            {
                ErrorMsg = err;
                return;
            }

            // Uniqueness check across existing models
            var existingContracts = await _contractService.GetContractsAsync();
            if (existingContracts.Any(c => c.ContractCode.Equals(EditingContractCode, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMsg = $"Mã hợp đồng '{EditingContractCode}' đã được sử dụng!";
                return;
            }

            var newContract = new Contract
            {
                ContractCode = EditingContractCode,
                EmployeeId = SelectedEmployeeForForm!.Id,
                ContractType = EditingContractType,
                StartDate = EditingStartDate,
                EndDate = EditingEndDate,
                Salary = EditingSalary,
                Status = EditingStatus,
                Note = EditingNote,
                IsDeleted = false
            };

            bool success = await _contractService.AddContractAsync(newContract);
            if (success)
            {
                SuccessMsg = "Thêm mới hợp đồng thành công!";
                ExecuteResetForm();
                await ApplyFiltersAsync();
            }
            else
            {
                ErrorMsg = "Thêm hợp đồng thất bại. Vui lòng kiểm tra lại dữ liệu.";
            }
        }

        private async Task ExecuteUpdateAsync()
        {
            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;

            if (SelectedContract == null)
            {
                ErrorMsg = "Vui lòng chọn một hợp đồng trong danh sách để cập nhật!";
                return;
            }

            if (!ValidateForm(out string err))
            {
                ErrorMsg = err;
                return;
            }

            // Uniqueness check across other contracts
            var existingContracts = await _contractService.GetContractsAsync();
            if (existingContracts.Any(c => c.Id != SelectedContract.Id && c.ContractCode.Equals(EditingContractCode, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMsg = $"Mã hợp đồng '{EditingContractCode}' đã được sử dụng bởi một hồ sơ khác!";
                return;
            }

            var updatedContract = new Contract
            {
                Id = SelectedContract.Id,
                ContractCode = EditingContractCode,
                EmployeeId = SelectedEmployeeForForm!.Id,
                ContractType = EditingContractType,
                StartDate = EditingStartDate,
                EndDate = EditingEndDate,
                Salary = EditingSalary,
                Status = EditingStatus,
                Note = EditingNote
            };

            bool success = await _contractService.UpdateContractAsync(updatedContract);
            if (success)
            {
                SuccessMsg = "Cập nhật hợp đồng thành công!";
                ExecuteResetForm();
                await ApplyFiltersAsync();
            }
            else
            {
                ErrorMsg = "Không thể lưu cập nhật hợp đồng.";
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            ErrorMsg = string.Empty;
            SuccessMsg = string.Empty;

            if (SelectedContract == null)
            {
                ErrorMsg = "Vui lòng chọn một hợp đồng trong danh sách để xóa!";
                return;
            }

            bool confirmed = true; // In WPF we'd show MessageBox.Show, but in pure VM let's soft-delete immediately
            if (confirmed)
            {
                bool success = await _contractService.DeleteContractAsync(SelectedContract.Id);
                if (success)
                {
                    SuccessMsg = "Xóa mềm hợp đồng lao động thành công!";
                    ExecuteResetForm();
                    await ApplyFiltersAsync();
                }
                else
                {
                    ErrorMsg = "Không thể thực hiện xóa hợp đồng này.";
                }
            }
        }
        #endregion
    }
}
