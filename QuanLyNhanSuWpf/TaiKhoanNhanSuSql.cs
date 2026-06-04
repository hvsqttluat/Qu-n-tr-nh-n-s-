using Microsoft.Data.SqlClient;

namespace QuanLyNhanSuWpf;

public static class TaiKhoanNhanSuSql
{
    public static async Task<int> DamBaoTheoNhanVienAsync(SqlConnection ketNoi)
    {
        var matKhau = BaoMatMatKhau.BamMatKhau(CauHinhUngDung.LayMatKhauKhoiTao());
        await using var lenh = new SqlCommand("""
            IF OBJECT_ID(N'dbo.HR_Employees', N'U') IS NULL OR OBJECT_ID(N'dbo.HR_JobPositions', N'U') IS NULL
            BEGIN
                SELECT 0;
                RETURN;
            END;

            UPDATE dbo.HR_Users
            SET IsActive = 0
            WHERE Username IN (N'giamdoc', N'truongphong', N'nhanvien');

            ;WITH Nguon AS
            (
                SELECT
                    LOWER(e.EmployeeCode) AS Username,
                    e.FullName,
                    CASE
                        WHEN p.Name LIKE N'%Giám đốc%' THEN N'Giám đốc'
                        WHEN p.Name LIKE N'%Trưởng phòng%' THEN N'Trưởng phòng'
                        ELSE N'Nhân viên'
                    END AS RoleName,
                    e.IsActive
                FROM dbo.HR_Employees e
                JOIN dbo.HR_JobPositions p ON p.PositionID = e.PositionID
            )
            MERGE dbo.HR_Users AS Dich
            USING Nguon AS Nguon
                ON Dich.Username = Nguon.Username
            WHEN MATCHED THEN
                UPDATE SET
                    IsActive = Nguon.IsActive
            WHEN NOT MATCHED THEN
                INSERT (Username, FullName, RoleName, PasswordHash, PasswordSalt, PasswordIterations, IsActive, RequirePasswordChange)
                VALUES (Nguon.Username, Nguon.FullName, Nguon.RoleName, @PasswordHash, @PasswordSalt, @PasswordIterations, Nguon.IsActive, 0);

            SELECT COUNT(1)
            FROM dbo.HR_Employees
            WHERE IsActive = 1;
            """, ketNoi);
        lenh.Parameters.AddWithValue("@PasswordHash", matKhau.HashBase64);
        lenh.Parameters.AddWithValue("@PasswordSalt", matKhau.SaltBase64);
        lenh.Parameters.AddWithValue("@PasswordIterations", matKhau.Iterations);

        var ketQua = await lenh.ExecuteScalarAsync();
        return Convert.ToInt32(ketQua);
    }
}
