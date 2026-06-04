using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuanLyNhanSuWpf;

public partial class LoginWindow : Window
{
    private readonly DispatcherTimer dongHo = new() { Interval = TimeSpan.FromSeconds(30) };
    private readonly KhoXacThuc khoXacThuc = new();

    public LoginWindow()
    {
        InitializeComponent();
        CapNhatDongHo();
        dongHo.Tick += (_, _) => CapNhatDongHo();
        dongHo.Start();
        TenDangNhapTextBox.Focus();
        TenDangNhapTextBox.SelectAll();
    }

    private async void DangNhap_Click(object sender, RoutedEventArgs e) => await DangNhap();

    private async void NhapMatKhau_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await DangNhap();
        }
    }

    private void Thoat_Click(object sender, RoutedEventArgs e) => Close();

    private async Task DangNhap()
    {
        var tenDangNhap = TenDangNhapTextBox.Text.Trim();
        var matKhau = MatKhauPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
        {
            ThongBaoTextBlock.Text = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            MatKhauPasswordBox.Focus();
            MatKhauPasswordBox.SelectAll();
            return;
        }

        DangNhapButton.IsEnabled = false;
        ThongBaoTextBlock.Text = "Đang xác thực tài khoản...";

        KetQuaDangNhap ketQua;
        try
        {
            ketQua = await khoXacThuc.DangNhapAsync(tenDangNhap, matKhau);
        }
        catch (Exception loi)
        {
            ThongBaoTextBlock.Text = $"Không thể xác thực: {loi.Message}";
            DangNhapButton.IsEnabled = true;
            return;
        }

        if (!ketQua.ThanhCong || ketQua.PhienDangNhap is null)
        {
            ThongBaoTextBlock.Text = ketQua.ThongBao;
            MatKhauPasswordBox.Focus();
            MatKhauPasswordBox.SelectAll();
            DangNhapButton.IsEnabled = true;
            return;
        }

        dongHo.Stop();
        var manHinhChinh = new MainWindow(ketQua.PhienDangNhap);
        Application.Current.MainWindow = manHinhChinh;
        manHinhChinh.Show();
        Close();
    }

    private void CapNhatDongHo()
    {
        ThoiGianTextBlock.Text = DateTime.Now.ToString("HH:mm - dd/MM/yyyy");
    }

}
