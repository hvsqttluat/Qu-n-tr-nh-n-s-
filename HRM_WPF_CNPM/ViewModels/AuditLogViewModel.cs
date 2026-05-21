using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Commands;

namespace HRM_WPF_CNPM.ViewModels
{
    public class AuditLogViewModel : BaseViewModel
    {
        private readonly AuditLogService _auditLogService;
        private List<AuditLog> _allLogs;
        private ObservableCollection<AuditLog> _auditLogs;
        private string _searchText = string.Empty;
        private string _selectedTableName = "Tất cả";
        private string _selectedActionName = "Tất cả";
        private ObservableCollection<string> _tableNames;
        private ObservableCollection<string> _actions;
        private bool _isLoaded;

        public AuditLogViewModel(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
            _allLogs = new List<AuditLog>();
            _auditLogs = new ObservableCollection<AuditLog>();
            _tableNames = new ObservableCollection<string> { "Tất cả" };
            _actions = new ObservableCollection<string> { "Tất cả" };

            RefreshCommand = new RelayCommand(async _ => await LoadLogsAsync());
            ClearFilterCommand = new RelayCommand(ExecuteClearFilter);

            _ = LoadLogsAsync();
        }

        public ObservableCollection<AuditLog> AuditLogs
        {
            get => _auditLogs;
            set => SetProperty(ref _auditLogs, value);
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

        public string SelectedTableName
        {
            get => _selectedTableName;
            set
            {
                if (SetProperty(ref _selectedTableName, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedActionName
        {
            get => _selectedActionName;
            set
            {
                if (SetProperty(ref _selectedActionName, value))
                {
                    ApplyFilters();
                }
            }
        }

        public ObservableCollection<string> TableNames
        {
            get => _tableNames;
            set => SetProperty(ref _tableNames, value);
        }

        public ObservableCollection<string> Actions
        {
            get => _actions;
            set => SetProperty(ref _actions, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public async Task LoadLogsAsync()
        {
            _allLogs = await _auditLogService.GetAuditLogsAsync();

            // Distinct table names and actions in local data sources
            var uniqueTables = _allLogs
                .Select(l => l.TableName)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .OrderBy(t => t);

            var uniqueActions = _allLogs
                .Select(l => l.Action)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct()
                .OrderBy(a => a);

            TableNames = new ObservableCollection<string> { "Tất cả" };
            foreach (var table in uniqueTables)
            {
                TableNames.Add(table);
            }

            Actions = new ObservableCollection<string> { "Tất cả" };
            foreach (var act in uniqueActions)
            {
                Actions.Add(act);
            }

            ApplyFilters();
            _isLoaded = true;
        }

        private void ApplyFilters()
        {
            var filtered = _allLogs.AsEnumerable();

            // Filter by search text (Username, Action, Description or TableName)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var query = SearchText.ToLower();
                filtered = filtered.Where(l =>
                    (l.User != null && l.User.Username.ToLower().Contains(query)) ||
                    l.Action.ToLower().Contains(query) ||
                    l.TableName.ToLower().Contains(query) ||
                    l.Description.ToLower().Contains(query)
                );
            }

            // Filter by table name dropdown selection
            if (SelectedTableName != "Tất cả" && !string.IsNullOrEmpty(SelectedTableName))
            {
                filtered = filtered.Where(l => l.TableName == SelectedTableName);
            }

            // Filter by action dropdown selection
            if (SelectedActionName != "Tất cả" && !string.IsNullOrEmpty(SelectedActionName))
            {
                filtered = filtered.Where(l => l.Action == SelectedActionName);
            }

            AuditLogs = new ObservableCollection<AuditLog>(filtered);
        }

        private void ExecuteClearFilter(object? parameter)
        {
            _searchText = string.Empty;
            _selectedTableName = "Tất cả";
            _selectedActionName = "Tất cả";

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(SelectedTableName));
            OnPropertyChanged(nameof(SelectedActionName));

            ApplyFilters();
        }
    }
}
