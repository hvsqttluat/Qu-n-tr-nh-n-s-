export interface CSharpFile {
  name: string;
  category: string;
  path: string;
  language: string;
  content: string;
  explanation: string;
}

export const csharpFiles: CSharpFile[] = [
  {
    name: "BaseViewModel.cs",
    category: "Cơ sở MVVM",
    path: "ViewModels/BaseViewModel.cs",
    language: "csharp",
    content: `using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HRM_WPF_CNPM.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}`,
    explanation: "BaseViewModel triển khai interface INotifyPropertyChanged - trái tim của MVVM. Nó giúp tự động liên kết dữ liệu hai chiều (Two-Way Binding) giữa View (XAML) và ViewModel (C#). Khi một thuộc tính trong ViewModel thay đổi, giao diện sẽ tự chuyển đổi giá trị ngay lập tức nhờ hàm OnPropertyChanged."
  },
  {
    name: "RelayCommand.cs",
    category: "Cơ sở MVVM",
    path: "Commands/RelayCommand.cs",
    language: "csharp",
    content: `using System;
using System.Windows.Input;

namespace HRM_WPF_CNPM.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}`,
    explanation: "RelayCommand đóng gói các thao tác xử lý hành động (nút bấm, sự kiện) trong WPF. Thay vì viết code-behind rườm rà, RelayCommand cho phép chúng ta ràng buộc sự kiện bấm nút trực tiếp vào một phương thức trong ViewModel, duy trì tính độc lập hoàn toàn của tầng Giao diện."
  },
  {
    name: "UserSession.cs",
    category: "Hỗ trợ",
    path: "Helpers/UserSession.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Models;

namespace HRM_WPF_CNPM.Helpers
{
    public static class UserSession
    {
        public static User? CurrentUser { get; set; }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}`,
    explanation: "UserSession là một static class đóng vai trò lưu trữ trạng thái người dùng hiện hành toàn cục. Vì được cấu hình static, bất cứ ViewModel nào cũng có thể đọc thuộc tính CurrentUser để kiểm tra phân quyền (Role) hoặc hiển thị tên người đăng nhập."
  },
  {
    name: "User.cs",
    category: "Models (Thực thể)",
    path: "Models/User.cs",
    language: "csharp",
    content: `using System;

namespace HRM_WPF_CNPM.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Admin, HR, Manager, Employee
        public int? EmployeeId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}`,
    explanation: "User lưu dữ liệu tài khoản đăng nhập. Thuộc tính 'Role' quyết định quyền hạn thực thi (Admin có quyền tối thượng, HR quản lý nhân sự/tiền lương, Manager quản lý phòng ban nhân viên, Employee chỉ xem và nộp đơn nghỉ phép)."
  },
  {
    name: "Department.cs",
    category: "Models (Thực thể)",
    path: "Models/Department.cs",
    language: "csharp",
    content: `using System;
using System.Collections.Generic;

namespace HRM_WPF_CNPM.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}`,
    explanation: "Department đại diện cho Phòng ban của doanh nghiệp. Một phòng ban có một tập danh sách các chức vụ (Positions) và nhân viên (Employees)."
  },
  {
    name: "Position.cs",
    category: "Models (Thực thể)",
    path: "Models/Position.cs",
    language: "csharp",
    content: `using System;
using System.Collections.Generic;

namespace HRM_WPF_CNPM.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}`,
    explanation: "Position đại diện cho Chức vụ lao động. Nó liên kết trực tiếp với DepartmentId (Foreign Key), bảo đảm quy tắc nghiệp vụ: Mỗi chức vụ thuộc về duy nhất một phòng phù đáng."
  },
  {
    name: "Employee.cs",
    category: "Models (Thực thể)",
    path: "Models/Employee.cs",
    language: "csharp",
    content: `using System;

namespace HRM_WPF_CNPM.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? CitizenId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public int? ManagerId { get; set; }
        
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? OfficialDate { get; set; }
        public string WorkStatus { get; set; } = "Thử việc"; // Thử việc, Chính thức, Tạm nghỉ, Đã nghỉ
        public decimal BaseSalary { get; set; }
        public string? Note { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual Position? Position { get; set; }
    }
}`,
    explanation: "Employee là trung tâm thông tin của dự án, chứa hồ sơ toàn diện của một nhân sự. Việc gán phòng ban và chức vụ giúp đảm bảo các nghiệp vụ chấm công, tính lương và phân quyền được đồng bộ chính xác."
  },
  {
    name: "AppDbContext.cs",
    category: "Database",
    path: "Data/AppDbContext.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace HRM_WPF_CNPM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình Precision cho các trường tiền tệ (decimal)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // SEED DATA
            // 1. Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentCode = "NS", DepartmentName = "Nhân sự" },
                new Department { Id = 2, DepartmentCode = "KD", DepartmentName = "Kinh doanh" },
                new Department { Id = 3, DepartmentCode = "KT", DepartmentName = "Kế toán" },
                new Department { Id = 4, DepartmentCode = "IT", DepartmentName = "Kỹ thuật" }
            );

            // 2. Positions
            modelBuilder.Entity<Position>().HasData(
                new Position { Id = 1, PositionCode = "TP", PositionName = "Trưởng phòng", DepartmentId = 1 },
                new Position { Id = 2, PositionCode = "NV", PositionName = "Nhân viên", DepartmentId = 2 },
                new Position { Id = 3, PositionCode = "KTV", PositionName = "Kế toán viên", DepartmentId = 3 },
                new Position { Id = 4, PositionCode = "DEV", PositionName = "Lập trình viên", DepartmentId = 4 }
            );

            // 3. Employees (5 mẫu)
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, EmployeeCode = "NV001", FullName = "Nguyễn Văn Admin", DepartmentId = 1, PositionId = 1, BaseSalary = 20000000 },
                new Employee { Id = 2, EmployeeCode = "NV002", FullName = "Trần Thị Nhân Sự", DepartmentId = 1, PositionId = 1, BaseSalary = 15000000 },
                new Employee { Id = 3, EmployeeCode = "NV003", FullName = "Lê Văn Quản Lý", DepartmentId = 2, PositionId = 2, BaseSalary = 18000000 },
                new Employee { Id = 4, EmployeeCode = "NV004", FullName = "Phạm Văn Nhân Viên", DepartmentId = 4, PositionId = 4, BaseSalary = 12000000 },
                new Employee { Id = 5, EmployeeCode = "NV005", FullName = "Hoàng Thị Kế Toán", DepartmentId = 3, PositionId = 3, BaseSalary = 13000000 }
            );

            // 4. Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", FullName = "Administrator", Role = "Admin", EmployeeId = 1 },
                new User { Id = 2, Username = "hr", Password = "123", FullName = "HR Manager", Role = "HR", EmployeeId = 2 },
                new User { Id = 3, Username = "manager", Password = "123", FullName = "Department Manager", Role = "Manager", EmployeeId = 3 },
                new User { Id = 4, Username = "employee", Password = "123", FullName = "Regular Employee", Role = "Employee", EmployeeId = 4 }
            );
        }
    }
}`,
    explanation: "AppDbContext là công cụ kết nối cơ sở dữ liệu qua Entity Framework Core. Nó tích hợp file appsettings.json, thiết lập các bộ dữ liệu DbSet, định hình kiểu tiền tệ (decimal) và gieo mầm (Seed Data) các dòng dữ liệu mặc định ban đầu để bắt đầu khởi tạo hệ thống."
  },
  {
    name: "AuthService.cs",
    category: "Services",
    path: "Services/AuthService.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Data;
using HRM_WPF_CNPM.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HRM_WPF_CNPM.Services
{
    public class AuthService
    {
        public async Task<User?> Authenticate(string username, string password)
        {
            using (var context = new AppDbContext())
            {
                return await context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == username 
                                         && u.Password == password 
                                         && !u.IsDeleted 
                                         && u.IsActive);
            }
        }
    }
}`,
    explanation: "AuthService đảm nhiệm nghiệp vụ xác thực tài khoản. Truy vấn DB tìm user có trùng Username, Password đồng thời bảo đảm tài trạng thái đang kích hoạt (IsActive) và chưa bị xoá (IsDeleted)."
  },
  {
    name: "LoginViewModel.cs",
    category: "ViewModels",
    path: "ViewModels/LoginViewModel.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Commands;
using HRM_WPF_CNPM.Helpers;
using HRM_WPF_CNPM.Services;
using HRM_WPF_CNPM.Views;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HRM_WPF_CNPM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isBusy;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(async (p) => await ExecuteLogin(p));
        }

        private async Task ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string password = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            var user = await _authService.Authenticate(Username, password);

            if (user != null)
            {
                UserSession.CurrentUser = user;
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow main = new MainWindow();
                    main.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                        {
                            window.Close();
                        }
                    }
                });
            }
            else
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
            }

            IsBusy = false;
        }
    }
}`,
    explanation: "LoginViewModel liên kết màn hình đăng nhập. Khi người dùng bấm nút đăng nhập, LoginCommand kích hoạt ExecuteLogin, nhận password bảo mật kiểu PasswordBox (tránh lưu plaintext dạng binding), kiểm thử DB, điều hành mở MainWindow và giải phóng LoginWindow."
  },
  {
    name: "LoginWindow.xaml",
    category: "Views",
    path: "Views/LoginWindow.xaml",
    language: "xml",
    content: `<Window x:Class="HRM_WPF_CNPM.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="http://schemas.modernwpf.com/2019"
        ui:WindowHelper.UseModernWindowStyle="True"
        Title="Đăng nhập hệ thống HRM" Height="450" Width="400"
        WindowStartupLocation="CenterScreen" ResizeMode="NoResize">
    
    <Grid Margin="30">
        <StackPanel VerticalAlignment="Center">
            <TextBlock Text="HỆ THỐNG HRM" FontSize="24" FontWeight="Bold" 
                       HorizontalAlignment="Center" Margin="0,0,0,30"/>

            <TextBlock Text="Tên đăng nhập" Margin="0,0,0,5"/>
            <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" 
                     ui:ControlHelper.PlaceholderText="Nhập username..."/>

            <TextBlock Text="Mật khẩu" Margin="0,15,0,5"/>
            <PasswordBox x:Name="TxtPassword" 
                         ui:ControlHelper.PlaceholderText="Nhập mật khẩu..."/>

            <TextBlock Text="{Binding ErrorMessage}" Foreground="Red" 
                       Margin="0,10,0,0" TextWrapping="Wrap"/>

            <Button Content="Đăng nhập" Margin="0,30,0,0" Height="40"
                    IsDefault="True" HorizontalAlignment="Stretch"
                    ui:ControlHelper.CornerRadius="4"
                    Command="{Binding LoginCommand}"
                    CommandParameter="{Binding ElementName=TxtPassword}"/>
            
            <ProgressBar IsIndeterminate="True" Margin="0,10,0,0"
                         Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"/>
        </StackPanel>
    </Grid>
</Window>`,
    explanation: "Màn hình Login Window sử dụng thư viện ModernWpf để dựng form có bo góc mềm mịn và ProgressBar tải dữ liệu sang trọng đúng phong cách thiết kế Windows 11 Fluent UI."
  },
  {
    name: "MainViewModel.cs",
    category: "ViewModels",
    path: "ViewModels/MainViewModel.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Commands;
using HRM_WPF_CNPM.Helpers;
using System.Windows.Input;

namespace HRM_WPF_CNPM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _currentUserName;
        private string _currentUserRole;
        private string _currentViewTitle = "Dashboard";
        private object _currentView;

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set => SetProperty(ref _currentUserRole, value);
        }

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set => SetProperty(ref _currentViewTitle, value);
        }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // --- CÁC THUỘC TÍNH PHÂN QUYỀN VAI TRÒ ---
        public bool IsAdmin => CurrentUserRole == "Admin";
        public bool IsAtLeastHR => CurrentUserRole == "Admin" || CurrentUserRole == "HR";
        public bool IsAtLeastManager => CurrentUserRole == "Admin" || CurrentUserRole == "HR" || CurrentUserRole == "Manager";
        
        public bool ShowEmployeeMenu => IsAtLeastManager;
        public bool ShowDeptPosContractMenu => IsAtLeastHR;
        public bool ShowAttendanceMenu => IsAtLeastManager;
        public bool ShowPayrollMenu => IsAtLeastHR || CurrentUserRole == "Employee";
        public bool ShowAdminOnlyMenu => IsAdmin;

        public ICommand SelectMenuCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            CurrentUserName = UserSession.CurrentUser?.FullName ?? "Người dùng";
            CurrentUserRole = UserSession.CurrentUser?.Role ?? "Employee";

            SelectMenuCommand = new RelayCommand((p) =>
            {
                if (p is string menuName)
                {
                    CurrentViewTitle = menuName;
                    if (menuName == "Dashboard")
                    {
                        CurrentView = new DashboardViewModel();
                    }
                    else if (menuName == "Thông báo")
                    {
                        CurrentView = new NotificationViewModel();
                    }
                    else if (menuName == "Phòng ban")
                    {
                        CurrentView = new DepartmentViewModel();
                    }
                    else if (menuName == "Chức vụ")
                    {
                        CurrentView = new PositionViewModel();
                    }
                    else if (menuName == "Nhân viên")
                    {
                        CurrentView = new EmployeeViewModel();
                    }
                    else
                    {
                        CurrentView = null;
                    }
                }
            });

            LogoutCommand = new RelayCommand((p) => ExecuteLogout());
            CurrentView = new DashboardViewModel();
        }

        private void ExecuteLogout()
        {
            UserSession.Logout();
        }
    }
}`,
    explanation: "MainViewModel là trung tâm hành chính điều hướng của ứng dụng. Nó chứa biến CurrentView kiểu object nhằm hoán đổi động các UserControl trên giao diện, đồng thời lưu các hằng số phân biệt vai trò quản lý (IsAdmin, IsAtLeastHR) để bật/tắt menu tự động chính xác."
  },
  {
    name: "MainWindow.xaml",
    category: "Views",
    path: "Views/MainWindow.xaml",
    language: "xml",
    content: `<Window x:Class="HRM_WPF_CNPM.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="http://schemas.modernwpf.com/2019"
        xmlns:vm="clr-namespace:HRM_WPF_CNPM.ViewModels"
        xmlns:v="clr-namespace:HRM_WPF_CNPM.Views"
        ui:WindowHelper.UseModernWindowStyle="True"
        Title="Hệ thống Quản lý nhân sự - HRM_WPF_CNPM" 
        Height="700" Width="1100" WindowStartupLocation="CenterScreen">

    <Window.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis" />
    </Window.Resources>

    <Grid>
        <ui:NavigationView x:Name="NavView" 
                           PaneDisplayMode="Left"
                           IsSettingsVisible="False"
                           Header="{Binding CurrentViewTitle}">
            
            <ui:NavigationView.MenuItems>
                <ui:NavigationViewItem Content="Dashboard" Icon="Home" Command="{Binding SelectMenuCommand}" CommandParameter="Dashboard"/>
                <ui:NavigationViewItemSeparator/>

                <ui:NavigationViewItem Content="Nhân viên" Icon="People" 
                                       Visibility="{Binding ShowEmployeeMenu, Converter={StaticResource BoolToVis}}"
                                       Command="{Binding SelectMenuCommand}" CommandParameter="Nhân viên"/>

                <ui:NavigationViewItem Content="Phòng ban" Icon="AllApps" 
                                       Visibility="{Binding ShowDeptPosContractMenu, Converter={StaticResource BoolToVis}}"
                                       Command="{Binding SelectMenuCommand}" CommandParameter="Phòng ban"/>
                
                <ui:NavigationViewItem Content="Chức vụ" Icon="Contact" 
                                       Visibility="{Binding ShowDeptPosContractMenu, Converter={StaticResource BoolToVis}}"
                                       Command="{Binding SelectMenuCommand}" CommandParameter="Chức vụ"/>

                <ui:NavigationViewItem Content="Nghỉ phép" Icon="Calendar" Command="{Binding SelectMenuCommand}" CommandParameter="Nghỉ phép"/>

                <ui:NavigationViewItem Content="Lương" Icon="Calculator" 
                                       Visibility="{Binding ShowPayrollMenu, Converter={StaticResource BoolToVis}}"
                                       Command="{Binding SelectMenuCommand}" CommandParameter="Lương"/>

                <ui:NavigationViewItemSeparator/>
                <ui:NavigationViewItem Content="Thông báo" Icon="Message" Command="{Binding SelectMenuCommand}" CommandParameter="Thông báo"/>

                <ui:NavigationViewItem Content="Audit Log" Icon="List" 
                                       Visibility="{Binding ShowAdminOnlyMenu, Converter={StaticResource BoolToVis}}"
                                       Command="{Binding SelectMenuCommand}" CommandParameter="Audit Log"/>
            </ui:NavigationView.MenuItems>

            <ui:NavigationView.PaneFooter>
                <StackPanel Margin="12,0,12,12">
                    <TextBlock Text="{Binding CurrentUserName}" FontWeight="Bold"/>
                    <TextBlock Text="{Binding CurrentUserRole}" FontSize="12" Opacity="0.7"/>
                    <Button Content="Đăng xuất" Icon="LeaveChat" Margin="0,10,0,0"
                            Command="{Binding LogoutCommand}" HorizontalAlignment="Left"
                            Click="Logout_Click"/>
                </StackPanel>
            </ui:NavigationView.PaneFooter>

            <Grid>
                <ContentControl Content="{Binding CurrentView}">
                    <ContentControl.Resources>
                        <DataTemplate DataType="{x:Type vm:DashboardViewModel}">
                            <v:DashboardView />
                        </DataTemplate>
                        <DataTemplate DataType="{x:Type vm:NotificationViewModel}">
                            <v:NotificationView />
                        </DataTemplate>
                        <DataTemplate DataType="{x:Type vm:DepartmentViewModel}">
                            <v:DepartmentView />
                        </DataTemplate>
                        <DataTemplate DataType="{x:Type vm:PositionViewModel}">
                            <v:PositionView />
                        </DataTemplate>
                        <DataTemplate DataType="{x:Type vm:EmployeeViewModel}">
                            <v:EmployeeView />
                        </DataTemplate>
                    </ContentControl.Resources>
                </ContentControl>
            </Grid>
        </ui:NavigationView>
    </Grid>
</Window>`,
    explanation: "MainWindow sử dụng kiến trúc Windows 11 NavigationView điều hướng cực kỳ đẹp. Kết hợp DataTemplate liên kết trực tiếp giữa class ViewModel trong C# với các thẻ UserControl XAML thực thể, giúp mã lập trình tách biệt và chuẩn chỉ."
  },
  {
    name: "DepartmentService.cs",
    category: "Services",
    path: "Services/DepartmentService.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Data;
using HRM_WPF_CNPM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRM_WPF_CNPM.Services
{
    public class DepartmentService
    {
        public async Task<List<Department>> GetAllAsync(string searchText = "")
        {
            using (var db = new AppDbContext())
            {
                var query = db.Departments.Where(d => !d.IsDeleted);
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(d => d.DepartmentName.Contains(searchText) || d.DepartmentCode.Contains(searchText));
                }
                return await query.ToListAsync();
            }
        }

        public async Task<bool> IsCodeExists(string code, int excludeId = 0)
        {
            using (var db = new AppDbContext())
            {
                return await db.Departments.AnyAsync(d => d.DepartmentCode == code && d.Id != excludeId && !d.IsDeleted);
            }
        }

        public async Task<bool> AddAsync(Department dept)
        {
            using (var db = new AppDbContext())
            {
                db.Departments.Add(dept);
                return await db.SaveChangesAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(Department dept)
        {
            using (var db = new AppDbContext())
            {
                var existing = await db.Departments.FindAsync(dept.Id);
                if (existing == null) return false;

                existing.DepartmentName = dept.DepartmentName;
                existing.DepartmentCode = dept.DepartmentCode;
                existing.Description = dept.Description;
                existing.UpdatedAt = DateTime.Now;

                return await db.SaveChangesAsync() > 0;
            }
        }

        public async Task<bool> DeleteSoftAsync(int id)
        {
            using (var db = new AppDbContext())
            {
                var dept = await db.Departments.FindAsync(id);
                if (dept != null)
                {
                    dept.IsDeleted = true;
                    return await db.SaveChangesAsync() > 0;
                }
                return false;
            }
        }
    }
}`,
    explanation: "DepartmentService điều vận CRUD phòng ban, xử lý lọc văn bản theo mã hoặc tên, xác thực mã phòng (DepartmentCode) độc nhất khi chèn mới, và đặc biệt là áp dụng 'DeleteSoft' (Xoá mềm, IsDeleted = true) đảm bảo tính vẹn toàn dữ liệu lịch sử quan trọng hệ thống."
  },
  {
    name: "EmployeeViewModel.cs",
    category: "ViewModels",
    path: "ViewModels/EmployeeViewModel.cs",
    language: "csharp",
    content: `using HRM_WPF_CNPM.Commands;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HRM_WPF_CNPM.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly EmployeeService _service;
        private readonly DepartmentService _deptService;
        private readonly PositionService _posService;

        private ObservableCollection<Employee> _employees;
        private Employee _selectedEmployee;
        private Employee _editingEmployee;
        private bool _isEditing;

        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public List<string> StatusList { get; } = new() { "Thử việc", "Chính thức", "Tạm nghỉ", "Đã nghỉ" };

        private string _searchText;
        private int? _filterDeptId;
        private string _filterStatus;

        public ObservableCollection<Employee> Employees { get => _employees; set => SetProperty(ref _employees, value); }
        public Employee SelectedEmployee { get => _selectedEmployee; set => SetProperty(ref _selectedEmployee, value); }
        public Employee EditingEmployee { get => _editingEmployee; set => SetProperty(ref _editingEmployee, value); }
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
        public string SearchText { get => _searchText; set { _searchText = value; _ = LoadData(); } }
        public int? FilterDeptId { get => _filterDeptId; set { _filterDeptId = value; _ = LoadData(); } }
        public string FilterStatus { get => _filterStatus; set { _filterStatus = value; _ = LoadData(); } }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        public EmployeeViewModel()
        {
            _service = new EmployeeService();
            _deptService = new DepartmentService();
            _posService = new PositionService();

            AddCommand = new RelayCommand(_ => { 
                IsEditing = true; 
                EditingEmployee = new Employee { JoinDate = DateTime.Today, WorkStatus = "Thử việc" }; 
            });

            EditCommand = new RelayCommand(_ => {
                if (SelectedEmployee == null) return;
                IsEditing = true;
                EditingEmployee = SelectedEmployee; 
            }, _ => SelectedEmployee != null);

            SaveCommand = new RelayCommand(async _ => await ExecuteSave());
            DeleteCommand = new RelayCommand(async _ => await ExecuteDelete(), _ => SelectedEmployee != null);
            CancelCommand = new RelayCommand(_ => IsEditing = false);

            _ = Initialize();
        }

        private async Task Initialize()
        {
            Departments = await _deptService.GetAllAsync();
            Positions = await _posService.GetAllAsync();
            OnPropertyChanged(nameof(Departments));
            OnPropertyChanged(nameof(Positions));
            await LoadData();
        }

        public async Task LoadData()
        {
            var list = await _service.GetAllAsync(SearchText, FilterDeptId, null, FilterStatus);
            Employees = new ObservableCollection<Employee>(list);
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(EditingEmployee.EmployeeCode) || string.IsNullOrEmpty(EditingEmployee.FullName))
            {
                MessageBox.Show("Mã và Họ tên không được để trống.");
                return;
            }

            if (await _service.IsCodeExists(EditingEmployee.EmployeeCode, EditingEmployee.Id))
            {
                MessageBox.Show("Mã nhân viên đã tồn tại.");
                return;
            }

            bool success = EditingEmployee.Id == 0 ? await _service.AddAsync(EditingEmployee) : await _service.UpdateAsync(EditingEmployee);
            if (success) { IsEditing = false; await LoadData(); }
        }

        private async Task ExecuteDelete()
        {
            if (MessageBox.Show("Xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (await _service.DeleteSoftAsync(SelectedEmployee.Id)) await LoadData();
            }
        }
    }
}`,
    explanation: "EmployeeViewModel điều khiển toàn bộ form dữ liệu tương tác phức tạp của Hồ sơ nhân sự. Ràng buộc đa chiều giúp dữ liệu tự động đồng bộ khi chọn ComboBox lọc, gõ tìm kiếm, hay đổi kiểu dữ liệu giới tính/ngày sinh."
  }
];
