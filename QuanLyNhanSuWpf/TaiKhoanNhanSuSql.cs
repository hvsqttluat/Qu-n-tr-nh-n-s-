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

            UPDATE taiKhoan
            SET IsActive = 0
            FROM dbo.HR_Users taiKhoan
            WHERE taiKhoan.Username <> N'admin'
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.HR_Employees nhanVien
                  WHERE LOWER(nhanVien.EmployeeCode) = taiKhoan.Username
              );

            ;WITH Nguon AS
            (
                SELECT
                    LOWER(e.EmployeeCode) AS Username,
                    e.FullName,
                    CASE
                        WHEN p.Name LIKE N'%Giám đốc%' THEN N'Giám đốc'
                        WHEN p.Name LIKE N'%Trưởng phòng%' THEN N'Trưởng phòng'
                        WHEN d.Name LIKE N'%Nhân sự%' OR p.Name LIKE N'%nhân sự%' THEN N'Nhân sự'
                        WHEN p.Name LIKE N'%Chuyên viên%' THEN N'Chuyên viên'
                        WHEN p.Name LIKE N'%Công nhân%' THEN N'Công nhân'
                        ELSE N'Nhân viên'
                    END AS RoleName,
                    e.IsActive
                FROM dbo.HR_Employees e
                JOIN dbo.HR_JobPositions p ON p.PositionID = e.PositionID
                JOIN dbo.HR_Departments d ON d.DepartmentID = e.DepartmentID
            )
            MERGE dbo.HR_Users AS Dich
            USING Nguon AS Nguon
                ON Dich.Username = Nguon.Username
            WHEN MATCHED THEN
                UPDATE SET
                    FullName = Nguon.FullName,
                    RoleName = Nguon.RoleName,
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
