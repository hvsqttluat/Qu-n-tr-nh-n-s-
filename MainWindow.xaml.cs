using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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

    private void ThongBao_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (LaBamVaoDieuKhienTuongTac(e.OriginalSource as DependencyObject, sender as DependencyObject))
        {
            return;
        }

        if (sender is FrameworkElement { DataContext: ThongBaoHeThong thongBao }
            && viewModel.MoThongBaoLenh.CanExecute(thongBao))
        {
            viewModel.MoThongBaoLenh.Execute(thongBao);
            e.Handled = true;
        }
    }

    private static bool LaBamVaoDieuKhienTuongTac(DependencyObject? nguon, DependencyObject? gioiHan)
    {
        while (nguon is not null && !ReferenceEquals(nguon, gioiHan))
        {
            if (nguon is ButtonBase or TextBoxBase or ComboBox or DatePicker)
            {
                return true;
            }

            nguon = VisualTreeHelper.GetParent(nguon);
        }

        return false;
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

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {

    }
}
