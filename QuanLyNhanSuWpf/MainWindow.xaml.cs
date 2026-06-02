using System.Windows;
using System.Windows.Input;

namespace QuanLyNhanSuWpf;

public partial class MainWindow : Window
{
    private readonly ManHinhChinhViewModel viewModel;
    private bool thanhBenDangMo = true;

    public MainWindow() : this(PhienDangNhap.MacDinh)
    {
    }

    public MainWindow(PhienDangNhap phienDangNhap)
    {
        viewModel = new ManHinhChinhViewModel(phienDangNhap);
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.TaiDuLieu();
    }

    private void LogoHr_Click(object sender, MouseButtonEventArgs e)
    {
        thanhBenDangMo = !thanhBenDangMo;
        CapNhatThanhBen();
    }

    private void CapNhatThanhBen()
    {
        CotThanhBen.Width = new GridLength(thanhBenDangMo ? 288 : 82);
        CotKhoangCach.Width = new GridLength(thanhBenDangMo ? 20 : 12);
        ThanhBen.Padding = thanhBenDangMo ? new Thickness(16) : new Thickness(12);

        DauThanhBen.HorizontalAlignment = thanhBenDangMo ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
        LogoHr.Margin = thanhBenDangMo ? new Thickness(0, 0, 14, 0) : new Thickness(0);

        var cheDoHienThi = thanhBenDangMo ? Visibility.Visible : Visibility.Collapsed;
        NhanThuongHieu.Visibility = cheDoHienThi;
        ThePhienLamViec.Visibility = cheDoHienThi;
        VungMenuThanhBen.Visibility = cheDoHienThi;
        TheDuLieuThanhBen.Visibility = cheDoHienThi;
    }

    private void DangXuat_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Bạn muốn đăng xuất khỏi phiên làm việc hiện tại?", "Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}
