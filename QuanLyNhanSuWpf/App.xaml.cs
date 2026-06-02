using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace QuanLyNhanSuWpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += XuLyLoiGiaoDien;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception loi)
            {
                GhiLoi(loi);
            }
        };
    }

    private static void XuLyLoiGiaoDien(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        GhiLoi(e.Exception);
        MessageBox.Show("Ứng dụng gặp lỗi ngoài dự kiến. Chi tiết đã được ghi vào nhật ký hệ thống.", "Lỗi ứng dụng", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void GhiLoi(Exception loi)
    {
        try
        {
            var thuMuc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyNhanSuWpf", "Logs");
            Directory.CreateDirectory(thuMuc);
            var tep = Path.Combine(thuMuc, $"app-{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(tep, $"[{DateTime.Now:O}] {loi}\n");
        }
        catch
        {
            // Khong de loi ghi log gay vong lap loi ung dung.
        }
    }
}
