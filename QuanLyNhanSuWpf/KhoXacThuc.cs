using Microsoft.Data.SqlClient;

namespace QuanLyNhanSuWpf;

public record KetQuaDangNhap(bool ThanhCong, string ThongBao, PhienDangNhap? PhienDangNhap, string NguonDuLieu);

public class KhoXacThuc
{
    private const int SoLanSaiToiDa = 5;
    private static readonly TimeSpan ThoiGianKhoaTamThoi = TimeSpan.FromHours(5);
    private readonly IReadOnlyList<string> cacChuoiKetNoi = CauHinhUngDung.LayChuoiKetNoiUngVien();

    public async Task<KetQuaDangNhap> DangNhapAsync(string tenDangNhap, string matKhau)
    {
        var loiCuoi = "";

        foreach (var chuoiKetNoi in cacChuoiKetNoi)
        {
            try
            {
                await CauHinhUngDung.DamBaoCoSoDuLieuAsync(chuoiKetNoi);
                await using var ketNoi = new SqlConnection(chuoiKetNoi);
                await ketNoi.OpenAsync();
                await SoDoQuanTriSql.DamBaoAsync(ketNoi);
                await DamBaoTaiKhoanMacDinhAsync(ketNoi);
                await TaiKhoanNhanSuSql.DamBaoTheoNhanVienAsync(ketNoi);

                await MoKhoaTamThoiNeuHetHanAsync(ketNoi, tenDangNhap);
                var taiKhoan = await LayTaiKhoanAsync(ketNoi, tenDangNhap);
                if (taiKhoan is null)
                {
                    await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginFailed", "HR_Users", tenDangNhap, "Ten dang nhap khong ton tai.");
                    return new KetQuaDangNhap(false, "Tên đăng nhập hoặc mật khẩu chưa đúng.", null, CauHinhUngDung.LayTenMayChu(chuoiKetNoi));
                }

                if (!taiKhoan.IsActive)
                {
                    await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginBlocked", "HR_Users", tenDangNhap, "Tai khoan dang bi khoa.");
                    return new KetQuaDangNhap(false, "Tài khoản đang bị khóa. Vui lòng liên hệ quản trị hệ thống.", null, CauHinhUngDung.LayTenMayChu(chuoiKetNoi));
                }

                if (taiKhoan.LockoutUntilAt is not null && taiKhoan.LockoutUntilAt.Value > DateTime.UtcNow)
                {
                    var khoaDen = taiKhoan.LockoutUntilAt.Value.ToLocalTime();
                    await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginLockedOut", "HR_Users", tenDangNhap, $"Tai khoan dang bi khoa tam thoi den {khoaDen:HH:mm dd/MM/yyyy}.");
                    return new KetQuaDangNhap(false, $"Tài khoản tạm khóa đến {khoaDen:HH:mm dd/MM/yyyy} do nhập sai quá {SoLanSaiToiDa} lần.", null, CauHinhUngDung.LayTenMayChu(chuoiKetNoi));
                }

                var hopLe = BaoMatMatKhau.XacMinhMatKhau(matKhau, taiKhoan.PasswordHash, taiKhoan.PasswordSalt, taiKhoan.PasswordIterations);
                if (!hopLe)
                {
                    var ketQuaSai = await TangSoLanSaiAsync(ketNoi, tenDangNhap);
                    await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginFailed", "HR_Users", tenDangNhap, $"Sai mat khau lan {ketQuaSai.SoLanSai}.");

                    if (ketQuaSai.LockoutUntilAt is not null)
                    {
                        var khoaDen = ketQuaSai.LockoutUntilAt.Value.ToLocalTime();
                        await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginLockedOut", "HR_Users", tenDangNhap, $"Khoa tam thoi sau {ketQuaSai.SoLanSai} lan sai mat khau.");
                        return new KetQuaDangNhap(false, $"Sai mật khẩu {ketQuaSai.SoLanSai} lần. Tài khoản tạm khóa đến {khoaDen:HH:mm dd/MM/yyyy}.", null, CauHinhUngDung.LayTenMayChu(chuoiKetNoi));
                    }

                    var conLai = Math.Max(0, SoLanSaiToiDa - ketQuaSai.SoLanSai);
                    return new KetQuaDangNhap(false, $"Tên đăng nhập hoặc mật khẩu chưa đúng. Còn {conLai} lần trước khi tài khoản bị khóa {ThoiGianKhoaTamThoi.TotalHours:N0} giờ.", null, CauHinhUngDung.LayTenMayChu(chuoiKetNoi));
                }

                await CapNhatDangNhapThanhCongAsync(ketNoi, tenDangNhap);
                await GhiNhatKyAsync(ketNoi, tenDangNhap, "LoginSuccess", "HR_Users", tenDangNhap, "Dang nhap thanh cong.");
                var phien = new PhienDangNhap(taiKhoan.Username, taiKhoan.FullName, taiKhoan.RoleName);
                return new KetQuaDangNhap(true, "Đăng nhập thành công.", phien, $"SQL Server {CauHinhUngDung.LayTenMayChu(chuoiKetNoi)}");
            }
            catch (Exception ex)
            {
                loiCuoi = ex.Message;
            }
        }

        if (CauHinhUngDung.ChoPhepDuPhongCucBo())
        {
            return DangNhapDuPhongCucBo(tenDangNhap, matKhau, loiCuoi);
        }

        return new KetQuaDangNhap(false, $"Không kết nối được SQL Server xác thực. Chi tiết: {loiCuoi}", null, "Chưa kết nối SQL Server");
    }

    private static KetQuaDangNhap DangNhapDuPhongCucBo(string tenDangNhap, string matKhau, string loiCuoi)
    {
        var matKhauKhoiTao = CauHinhUngDung.LayMatKhauKhoiTao();
        var taiKhoan = TaiKhoanMacDinh()
            .FirstOrDefault(x => string.Equals(x.Username, tenDangNhap, StringComparison.OrdinalIgnoreCase));

        if (taiKhoan is null || matKhau != matKhauKhoiTao)
        {
            return new KetQuaDangNhap(false, "Tên đăng nhập hoặc mật khẩu chưa đúng.", null, "Dự phòng cục bộ");
        }

        var phien = new PhienDangNhap(taiKhoan.Username, taiKhoan.FullName, taiKhoan.RoleName);
        return new KetQuaDangNhap(true, $"Đăng nhập bằng chế độ dự phòng cục bộ. SQL Server chưa sẵn sàng: {loiCuoi}", phien, "Dự phòng cục bộ");
    }

    private static async Task DamBaoTaiKhoanMacDinhAsync(SqlConnection ketNoi)
    {
        var matKhauKhoiTao = CauHinhUngDung.LayMatKhauKhoiTao();
        foreach (var taiKhoan in TaiKhoanMacDinh())
        {
            var daCo = await TaiKhoanTonTaiAsync(ketNoi, taiKhoan.Username);
            if (daCo)
            {
                continue;
            }

            var matKhau = BaoMatMatKhau.BamMatKhau(matKhauKhoiTao);
            await using var lenh = new SqlCommand("""
                INSERT INTO dbo.HR_Users(Username, FullName, RoleName, PasswordHash, PasswordSalt, PasswordIterations, IsActive, RequirePasswordChange)
                VALUES(@Username, @FullName, @RoleName, @PasswordHash, @PasswordSalt, @PasswordIterations, 1, 1)
                """, ketNoi);
            lenh.Parameters.AddWithValue("@Username", taiKhoan.Username);
            lenh.Parameters.AddWithValue("@FullName", taiKhoan.FullName);
            lenh.Parameters.AddWithValue("@RoleName", taiKhoan.RoleName);
            lenh.Parameters.AddWithValue("@PasswordHash", matKhau.HashBase64);
            lenh.Parameters.AddWithValue("@PasswordSalt", matKhau.SaltBase64);
            lenh.Parameters.AddWithValue("@PasswordIterations", matKhau.Iterations);
            await lenh.ExecuteNonQueryAsync();
        }
    }

    private static async Task<bool> TaiKhoanTonTaiAsync(SqlConnection ketNoi, string tenDangNhap)
    {
        await using var lenh = new SqlCommand("SELECT COUNT(1) FROM dbo.HR_Users WHERE Username=@Username", ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        return Convert.ToInt32(await lenh.ExecuteScalarAsync()) > 0;
    }

    private static async Task<TaiKhoanXacThuc?> LayTaiKhoanAsync(SqlConnection ketNoi, string tenDangNhap)
    {
        await using var lenh = new SqlCommand("""
            SELECT Username, FullName, RoleName, PasswordHash, PasswordSalt, PasswordIterations, IsActive, LockoutUntilAt
            FROM dbo.HR_Users
            WHERE Username=@Username
            """, ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);

        await using var doc = await lenh.ExecuteReaderAsync();
        if (!await doc.ReadAsync())
        {
            return null;
        }

        return new TaiKhoanXacThuc(
            doc.GetString(0),
            doc.GetString(1),
            doc.GetString(2),
            doc.GetString(3),
            doc.GetString(4),
            doc.GetInt32(5),
            doc.GetBoolean(6),
            doc.IsDBNull(7) ? null : doc.GetDateTime(7));
    }

    private static async Task MoKhoaTamThoiNeuHetHanAsync(SqlConnection ketNoi, string tenDangNhap)
    {
        await using var lenh = new SqlCommand("""
            UPDATE dbo.HR_Users
            SET FailedLoginCount = 0,
                LockoutUntilAt = NULL
            WHERE Username=@Username
              AND LockoutUntilAt IS NOT NULL
              AND LockoutUntilAt <= SYSUTCDATETIME()
            """, ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task<KetQuaSaiDangNhap> TangSoLanSaiAsync(SqlConnection ketNoi, string tenDangNhap)
    {
        await using var lenh = new SqlCommand("""
            UPDATE dbo.HR_Users
            SET FailedLoginCount = FailedLoginCount + 1,
                LockoutUntilAt = CASE
                    WHEN FailedLoginCount + 1 >= @SoLanSaiToiDa THEN DATEADD(MINUTE, @SoPhutKhoa, SYSUTCDATETIME())
                    ELSE LockoutUntilAt
                END
            WHERE Username=@Username;

            SELECT FailedLoginCount, LockoutUntilAt
            FROM dbo.HR_Users
            WHERE Username=@Username;
            """, ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        lenh.Parameters.AddWithValue("@SoLanSaiToiDa", SoLanSaiToiDa);
        lenh.Parameters.AddWithValue("@SoPhutKhoa", (int)ThoiGianKhoaTamThoi.TotalMinutes);
        await using var doc = await lenh.ExecuteReaderAsync();
        if (await doc.ReadAsync())
        {
            return new KetQuaSaiDangNhap(doc.GetInt32(0), doc.IsDBNull(1) ? null : doc.GetDateTime(1));
        }

        return new KetQuaSaiDangNhap(0, null);
    }

    private static async Task CapNhatDangNhapThanhCongAsync(SqlConnection ketNoi, string tenDangNhap)
    {
        await using var lenh = new SqlCommand("""
            UPDATE dbo.HR_Users
            SET LastLoginAt = SYSUTCDATETIME(),
                FailedLoginCount = 0,
                LockoutUntilAt = NULL
            WHERE Username=@Username
            """, ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task GhiNhatKyAsync(SqlConnection ketNoi, string tenDangNhap, string hanhDong, string thucThe, string maThucThe, string chiTiet)
    {
        await using var lenh = new SqlCommand("""
            INSERT INTO dbo.HR_AuditLogs(ActorUsername, ActionName, EntityName, EntityKey, Detail, MachineName)
            VALUES(@ActorUsername, @ActionName, @EntityName, @EntityKey, @Detail, @MachineName)
            """, ketNoi);
        lenh.Parameters.AddWithValue("@ActorUsername", tenDangNhap);
        lenh.Parameters.AddWithValue("@ActionName", hanhDong);
        lenh.Parameters.AddWithValue("@EntityName", thucThe);
        lenh.Parameters.AddWithValue("@EntityKey", maThucThe);
        lenh.Parameters.AddWithValue("@Detail", chiTiet);
        lenh.Parameters.AddWithValue("@MachineName", Environment.MachineName);
        await lenh.ExecuteNonQueryAsync();
    }

    private static IReadOnlyList<TaiKhoanMacDinhDto> TaiKhoanMacDinh() =>
    [
        new("admin", "Quản trị hệ thống", "Admin"),
        new("gd001", "Nguyễn Minh Đức", "Giám đốc"),
        new("tp001", "Trần Quốc Huy", "Trưởng phòng"),
        new("tp002", "Phạm Văn Long", "Trưởng phòng"),
        new("tp003", "Lê Thu Hà", "Trưởng phòng"),
        new("tp004", "Đỗ Thị Mai", "Trưởng phòng"),
        new("tp005", "Vũ Anh Tuấn", "Trưởng phòng"),
        new("nv001", "Vũ Hải An", "Nhân viên"),
        new("cn001", "Vũ Tuấn Phương", "Nhân viên")
    ];

    private record TaiKhoanXacThuc(
        string Username,
        string FullName,
        string RoleName,
        string PasswordHash,
        string PasswordSalt,
        int PasswordIterations,
        bool IsActive,
        DateTime? LockoutUntilAt);

    private record TaiKhoanMacDinhDto(string Username, string FullName, string RoleName);
    private record KetQuaSaiDangNhap(int SoLanSai, DateTime? LockoutUntilAt);
}
