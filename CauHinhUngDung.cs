using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSuWpf;

public static class CauHinhUngDung
{
    public const string TenCoSoDuLieuMacDinh = "HRManagementDB";
    public const string MatKhauKhoiTaoMacDinh = "Admin@2026!";

    public static IReadOnlyList<string> LayChuoiKetNoiUngVien()
    {
        var danhSach = new List<string>();
        var cauHinhTuMoiTruong = LayGiaTri("HRM_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(cauHinhTuMoiTruong))
        {
            danhSach.Add(ToiUuChuoiKetNoi(cauHinhTuMoiTruong));
        }

        danhSach.Add(ToiUuChuoiKetNoi($"Server=.\\SQLEXPRESS;Database={TenCoSoDuLieuMacDinh};Trusted_Connection=True;TrustServerCertificate=True;"));
        danhSach.Add(ToiUuChuoiKetNoi($"Server=localhost;Database={TenCoSoDuLieuMacDinh};Trusted_Connection=True;TrustServerCertificate=True;"));
        danhSach.Add(ToiUuChuoiKetNoi($"Server=(localdb)\\MSSQLLocalDB;Database={TenCoSoDuLieuMacDinh};Trusted_Connection=True;TrustServerCertificate=True;"));
        return danhSach.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static string LayMatKhauKhoiTao()
    {
        var matKhau = LayGiaTri("HRM_INITIAL_PASSWORD");
        return string.IsNullOrWhiteSpace(matKhau) ? MatKhauKhoiTaoMacDinh : matKhau;
    }

    public static bool ChoPhepDuPhongCucBo()
    {
        var giaTri = LayGiaTri("HRM_ALLOW_LOCAL_FALLBACK");
        return bool.TryParse(giaTri, out var choPhep) && choPhep;
    }

    public static async Task DamBaoCoSoDuLieuAsync(string chuoiKetNoi)
    {
        var boTao = new SqlConnectionStringBuilder(chuoiKetNoi);
        var tenCoSoDuLieu = boTao.InitialCatalog;
        if (string.IsNullOrWhiteSpace(tenCoSoDuLieu))
        {
            return;
        }

        boTao.InitialCatalog = "master";
        await using var ketNoi = new SqlConnection(boTao.ConnectionString);
        await ketNoi.OpenAsync();

        var tenAnToan = tenCoSoDuLieu.Replace("]", "]]");
        var giaTriAnToan = tenCoSoDuLieu.Replace("'", "''");
        await using var lenh = new SqlCommand($"IF DB_ID(N'{giaTriAnToan}') IS NULL CREATE DATABASE [{tenAnToan}];", ketNoi);
        await lenh.ExecuteNonQueryAsync();
    }

    public static string LayTenMayChu(string chuoiKetNoi)
    {
        try
        {
            return new SqlConnectionStringBuilder(chuoiKetNoi).DataSource;
        }
        catch
        {
            return "khong ro nguon";
        }
    }

    public static string ToiUuChuoiKetNoi(string chuoiKetNoi)
    {
        try
        {
            var boTao = new SqlConnectionStringBuilder(chuoiKetNoi)
            {
                ConnectTimeout = 3,
                TrustServerCertificate = true
            };
            boTao["Encrypt"] = false;
            return boTao.ConnectionString;
        }
        catch
        {
            var chuoi = chuoiKetNoi.Trim().TrimEnd(';');
            return $"{chuoi};Connect Timeout=3;Encrypt=False;TrustServerCertificate=True;";
        }
    }

    private static string? LayGiaTri(string khoa)
    {
        var tuMoiTruong = Environment.GetEnvironmentVariable(khoa);
        if (!string.IsNullOrWhiteSpace(tuMoiTruong))
        {
            return tuMoiTruong;
        }

        var tepCauHinh = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(tepCauHinh))
        {
            return null;
        }

        try
        {
            using var taiLieu = JsonDocument.Parse(File.ReadAllText(tepCauHinh));
            return taiLieu.RootElement.TryGetProperty(khoa, out var giaTri) ? giaTri.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
