using Microsoft.Data.SqlClient;

namespace QuanLyNhanSuWpf;

public class KhoDuLieuNhanSu
{
    private string chuoiKetNoi;
    private readonly List<string> cacChuoiKetNoi;
    private KhoDuLieuUngDung? duLieuCucBo;

    public KhoDuLieuNhanSu()
    {
        cacChuoiKetNoi = CauHinhUngDung.LayChuoiKetNoiUngVien().ToList();
        chuoiKetNoi = cacChuoiKetNoi[0];
    }

    public async Task<(KhoDuLieuUngDung DuLieu, string NguonDuLieu)> TaiDuLieuAsync()
    {
        foreach (var ungVienChuoiKetNoi in cacChuoiKetNoi.Distinct())
        {
            try
            {
                chuoiKetNoi = ungVienChuoiKetNoi;
                await CauHinhUngDung.DamBaoCoSoDuLieuAsync(ungVienChuoiKetNoi);
                var duLieu = await TaiTuSqlServerAsync();
                return (duLieu, $"SQL Server HRManagementDB ({CauHinhUngDung.LayTenMayChu(ungVienChuoiKetNoi)})");
            }
            catch
            {
                // Thử nguồn kết nối tiếp theo.
            }
        }

        duLieuCucBo ??= TaoDuLieuMau();
        return (duLieuCucBo, "Dữ liệu mẫu cục bộ - chưa kết nối SQL Server");
    }

    public async Task LuuNhanVienAsync(NhanVien nhanVien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        var cauLenh = nhanVien.MaNhanVien == 0
            ? "INSERT INTO HR_Employees(EmployeeCode, FullName, DepartmentID, PositionID, BirthDate, SocialInsuranceStartDate, JoinDate, IsActive, EmergencyContact, BankAccount, IdentityNumber) VALUES(@MaSo,@HoTen,@MaPhongBan,@MaViTri,@NgaySinh,@NgayThamGiaBaoHiemXaHoi,@NgayVaoLam,@DangLamViec,@LienHeKhanCap,@TaiKhoanNganHang,@SoCanCuoc)"
            : "UPDATE HR_Employees SET EmployeeCode=@MaSo, FullName=@HoTen, DepartmentID=@MaPhongBan, PositionID=@MaViTri, BirthDate=@NgaySinh, SocialInsuranceStartDate=@NgayThamGiaBaoHiemXaHoi, JoinDate=@NgayVaoLam, IsActive=@DangLamViec, EmergencyContact=@LienHeKhanCap, BankAccount=@TaiKhoanNganHang, IdentityNumber=@SoCanCuoc WHERE EmployeeID=@MaNhanVien";
        await using var lenh = new SqlCommand(cauLenh, ketNoi);
        GanThamSoNhanVien(lenh, nhanVien);
        await lenh.ExecuteNonQueryAsync();
    }

    public async Task XoaNhanVienAsync(int maNhanVien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await using var giaoDich = (SqlTransaction)await ketNoi.BeginTransactionAsync();

        try
        {
            string[] cacLenh =
            [
                "UPDATE HR_Departments SET ManagerID = NULL WHERE ManagerID = @MaNhanVien",
                "UPDATE HR_Employees SET ManagerID = NULL WHERE ManagerID = @MaNhanVien",
                "DELETE FROM HR_Payslips WHERE EmployeeID = @MaNhanVien",
                "DELETE FROM HR_Appraisals WHERE EmployeeID = @MaNhanVien OR ReviewerID = @MaNhanVien",
                "DELETE FROM HR_LeaveRequests WHERE EmployeeID = @MaNhanVien OR ApproverID = @MaNhanVien",
                "DELETE FROM HR_Attendances WHERE EmployeeID = @MaNhanVien",
                "DELETE FROM HR_Contracts WHERE EmployeeID = @MaNhanVien",
                "DELETE FROM HR_Employees WHERE EmployeeID = @MaNhanVien"
            ];

            foreach (var cauLenh in cacLenh)
            {
                await using var lenh = new SqlCommand(cauLenh, ketNoi, giaoDich);
                lenh.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                await lenh.ExecuteNonQueryAsync();
            }

            await giaoDich.CommitAsync();
        }
        catch
        {
            await giaoDich.RollbackAsync();
            throw;
        }
    }

    public async Task ThemUngVienAsync(BieuMauUngVien ungVien)
    {
        await ThucThiAsync("""
            IF EXISTS (SELECT 1 FROM HR_Employees WHERE LTRIM(RTRIM(FullName)) = LTRIM(RTRIM(@HoTen)) AND IsActive = 1)
            BEGIN
                THROW 50006, N'Người này đã là nhân viên, không đưa vào danh sách ứng viên.', 1;
            END;

            INSERT INTO HR_Applicants(PositionID, FullName, Email, Phone, CVFile_Url, Stage)
            VALUES(@MaViTri, @HoTen, @Email, @DienThoai, NULL, N'Mới')
            """,
            ("@MaViTri", ungVien.MaViTri),
            ("@HoTen", ungVien.HoTen),
            ("@Email", ungVien.Email),
            ("@DienThoai", ungVien.DienThoai));
    }

    public async Task ChuyenUngVienThanhNhanVienAsync(UngVien ungVien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await using var giaoDich = (SqlTransaction)await ketNoi.BeginTransactionAsync();

        try
        {
            var sql = """
                DECLARE @ApplicantID INT = (
                    SELECT TOP 1 ApplicantID
                    FROM HR_Applicants
                    WHERE FullName = @HoTen AND Email = @Email
                    ORDER BY ApplicantID DESC
                );
                DECLARE @PositionID INT = ISNULL((SELECT PositionID FROM HR_Applicants WHERE ApplicantID = @ApplicantID), 1);
                DECLARE @DepartmentID INT = ISNULL((SELECT DepartmentID FROM HR_JobPositions WHERE PositionID = @PositionID), 1);
                DECLARE @NextCode VARCHAR(20) = 'NV' + RIGHT('000' + CAST((SELECT ISNULL(MAX(EmployeeID), 0) + 1 FROM HR_Employees) AS VARCHAR(10)), 3);

                IF @ApplicantID IS NULL
                BEGIN
                    THROW 50004, N'Không tìm thấy ứng viên cần tiếp nhận.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM HR_Employees
                    WHERE IsActive = 1
                      AND LTRIM(RTRIM(FullName)) = LTRIM(RTRIM(@HoTen))
                )
                BEGIN
                    THROW 50005, N'Ứng viên này đã được tiếp nhận thành nhân viên.', 1;
                END;

                IF @ApplicantID IS NOT NULL
                BEGIN
                    UPDATE HR_Employees SET ApplicantID = NULL WHERE ApplicantID = @ApplicantID;

                    INSERT INTO HR_Employees(EmployeeCode, ApplicantID, FullName, DepartmentID, PositionID, ManagerID, BirthDate, SocialInsuranceStartDate, JoinDate, IsActive)
                    VALUES(@NextCode, NULL, @HoTen, @DepartmentID, @PositionID, NULL, DATEADD(YEAR, -23, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE), 1);
                    DELETE FROM HR_Applicants WHERE ApplicantID = @ApplicantID;
                END
                """;
            await using var lenh = new SqlCommand(sql, ketNoi, giaoDich);
            lenh.Parameters.AddWithValue("@HoTen", ungVien.HoTen);
            lenh.Parameters.AddWithValue("@Email", ungVien.Email);
            await lenh.ExecuteNonQueryAsync();
            await giaoDich.CommitAsync();
        }
        catch
        {
            await giaoDich.RollbackAsync();
            throw;
        }
    }

    public async Task ThemPhongBanAsync(string tenPhongBan)
    {
        await ThucThiAsync(
            "INSERT INTO HR_Departments(Name, ManagerID) VALUES(@TenPhongBan, NULL)",
            ("@TenPhongBan", tenPhongBan));
    }

    public async Task LuuPhongBanAsync(BieuMauPhongBan phongBan)
    {
        if (phongBan.MaPhongBan == 0)
        {
            await ThucThiAsync(
                """
                INSERT INTO HR_Departments(Name, ManagerID) VALUES(@TenPhongBan, @MaTruongPhong);
                DECLARE @MaPhongBanMoi INT = CAST(SCOPE_IDENTITY() AS INT);
                IF @MaTruongPhong IS NOT NULL
                BEGIN
                    UPDATE HR_Employees SET DepartmentID = @MaPhongBanMoi WHERE EmployeeID = @MaTruongPhong;
                    DECLARE @GiamDocMoi INT = (SELECT TOP 1 EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');
                    UPDATE e
                    SET ManagerID =
                        CASE
                            WHEN e.EmployeeID = @MaTruongPhong THEN
                                CASE WHEN e.EmployeeID = @GiamDocMoi THEN NULL ELSE @GiamDocMoi END
                            ELSE @MaTruongPhong
                        END
                    FROM HR_Employees e
                    WHERE e.DepartmentID = @MaPhongBanMoi
                      AND e.IsActive = 1;
                END;
                """,
                ("@TenPhongBan", phongBan.TenPhongBan.Trim()),
                ("@MaTruongPhong", phongBan.MaTruongPhong));
            return;
        }

        await ThucThiAsync(
            """
            UPDATE HR_Departments SET Name=@TenPhongBan, ManagerID=@MaTruongPhong WHERE DepartmentID=@MaPhongBan;
            IF @MaTruongPhong IS NOT NULL
            BEGIN
                UPDATE HR_Employees SET DepartmentID = @MaPhongBan WHERE EmployeeID = @MaTruongPhong;
                DECLARE @GiamDoc INT = (SELECT TOP 1 EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');
                UPDATE e
                SET ManagerID =
                    CASE
                        WHEN e.EmployeeID = @MaTruongPhong THEN
                            CASE WHEN e.EmployeeID = @GiamDoc THEN NULL ELSE @GiamDoc END
                        ELSE @MaTruongPhong
                    END
                FROM HR_Employees e
                WHERE e.DepartmentID = @MaPhongBan
                  AND e.IsActive = 1;
            END;
            """,
            ("@TenPhongBan", phongBan.TenPhongBan.Trim()),
            ("@MaTruongPhong", phongBan.MaTruongPhong),
            ("@MaPhongBan", phongBan.MaPhongBan));
    }

    public async Task XoaPhongBanAsync(int maPhongBan)
    {
        await ThucThiAsync("""
            IF EXISTS (SELECT 1 FROM HR_Employees WHERE DepartmentID = @MaPhongBan)
            BEGIN
                THROW 50001, N'Phòng ban đang có nhân viên, không thể xóa.', 1;
            END;

            DELETE FROM HR_JobPositions WHERE DepartmentID = @MaPhongBan;
            DELETE FROM HR_Departments WHERE DepartmentID = @MaPhongBan;
            """,
            ("@MaPhongBan", maPhongBan));
    }

    public async Task GanTruongPhongAsync(int maPhongBan, int maNhanVien)
    {
        await ThucThiAsync(
            """
            UPDATE HR_Employees SET DepartmentID = @MaPhongBan WHERE EmployeeID = @MaNhanVien;
            UPDATE HR_Departments SET ManagerID = @MaNhanVien WHERE DepartmentID = @MaPhongBan;

            DECLARE @GiamDoc INT = (SELECT TOP 1 EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');
            UPDATE e
            SET ManagerID =
                CASE
                    WHEN e.EmployeeID = @MaNhanVien THEN
                        CASE WHEN e.EmployeeID = @GiamDoc THEN NULL ELSE @GiamDoc END
                    ELSE @MaNhanVien
                END
            FROM HR_Employees e
            WHERE e.DepartmentID = @MaPhongBan
              AND e.IsActive = 1;
            """,
            ("@MaNhanVien", maNhanVien),
            ("@MaPhongBan", maPhongBan));
    }

    public async Task ChuyenGiaiDoanUngVienAsync(UngVien ungVien, string giaiDoanMoi)
    {
        await ThucThiAsync("""
            UPDATE HR_Applicants
            SET Stage = @GiaiDoanMoi
            WHERE ApplicantID = (
                SELECT TOP 1 ApplicantID
                FROM HR_Applicants
                WHERE FullName = @HoTen AND Email = @Email
                ORDER BY ApplicantID DESC
            )
            """,
            ("@GiaiDoanMoi", giaiDoanMoi),
            ("@HoTen", ungVien.HoTen),
            ("@Email", ungVien.Email));
    }

    public async Task GhiNhanVaoCaAsync(int maNhanVien)
    {
        await ThucThiAsync("""
            IF EXISTS (SELECT 1 FROM HR_Attendances WHERE EmployeeID = @MaNhanVien AND CheckOutTime IS NULL)
            BEGIN
                THROW 50002, N'Nhân viên đang có ca chưa ra. Vui lòng ghi nhận ra ca trước khi vào ca mới.', 1;
            END;

            INSERT INTO HR_Attendances(EmployeeID, CheckInTime, CheckOutTime, WorkHours)
            VALUES(@MaNhanVien, GETDATE(), NULL, NULL)
            """,
            ("@MaNhanVien", maNhanVien));
    }

    public async Task GhiNhanRaCaAsync(int maNhanVien)
    {
        await ThucThiAsync("""
            DECLARE @AttendanceID INT = (
                SELECT TOP 1 AttendanceID
                FROM HR_Attendances
                WHERE EmployeeID = @MaNhanVien AND CheckOutTime IS NULL
                ORDER BY CheckInTime DESC, AttendanceID DESC
            );

            IF @AttendanceID IS NULL
            BEGIN
                THROW 50003, N'Nhân viên chưa có ca đang mở để ghi nhận ra ca.', 1;
            END;

            UPDATE HR_Attendances
            SET CheckOutTime = GETDATE(),
                WorkHours = CAST(DATEDIFF(MINUTE, CheckInTime, GETDATE()) AS DECIMAL(10,2)) / 60
            WHERE AttendanceID = @AttendanceID
            """, ("@MaNhanVien", maNhanVien));
    }

    public async Task DieuChinhCongAsync(ChamCong chamCong)
    {
        await ThucThiAsync("""
            UPDATE HR_Attendances
            SET CheckOutTime = DATEADD(HOUR, 8, CheckInTime),
                WorkHours = 8
            WHERE AttendanceID = (
                SELECT TOP 1 a.AttendanceID
                FROM HR_Attendances a
                JOIN HR_Employees e ON a.EmployeeID = e.EmployeeID
                WHERE e.FullName = @NhanVien AND a.CheckInTime = @GioVao
                ORDER BY a.AttendanceID DESC
            )
            """,
            ("@NhanVien", chamCong.NhanVien),
            ("@GioVao", chamCong.GioVao));
    }

    public async Task ThemNghiPhepAsync(BieuMauNghiPhep nghiPhep)
    {
        if (string.IsNullOrWhiteSpace(nghiPhep.LoaiNghi))
        {
            throw new InvalidOperationException("Loại nghỉ không được để trống.");
        }

        var tongNgay = QuyTacNghiepVuNhanSu.TinhSoNgayBaoGom(nghiPhep.TuNgay, nghiPhep.DenNgay);
        if (tongNgay <= 0)
        {
            throw new InvalidOperationException("Ngày kết thúc nghỉ phép phải sau hoặc bằng ngày bắt đầu.");
        }

        await ThucThiAsync("""
            INSERT INTO HR_LeaveRequests(EmployeeID, LeaveType, StartDate, EndDate, TotalDays, Status, ApproverID, Reason, ApprovalNote)
            VALUES(@MaNhanVien, @LoaiNghi, @TuNgay, @DenNgay, @TongNgay, N'Chờ duyệt', NULL, @LyDo, NULL)
            """,
            ("@MaNhanVien", nghiPhep.MaNhanVien),
            ("@LoaiNghi", nghiPhep.LoaiNghi),
            ("@TuNgay", nghiPhep.TuNgay.Date),
            ("@DenNgay", nghiPhep.DenNgay.Date),
            ("@TongNgay", tongNgay),
            ("@LyDo", nghiPhep.LyDo));
    }

    public async Task CapNhatTrangThaiNghiPhepAsync(NghiPhep nghiPhep, string trangThai)
    {
        await ThucThiAsync("""
            UPDATE HR_LeaveRequests
            SET Status = @TrangThai,
                ApprovalNote = NULLIF(@LyDoXuLy, N'')
            WHERE LeaveID = (
                SELECT TOP 1 l.LeaveID
                FROM HR_LeaveRequests l
                JOIN HR_Employees e ON l.EmployeeID = e.EmployeeID
                WHERE e.FullName = @NhanVien AND l.LeaveType = @LoaiNghi AND l.StartDate = @TuNgay AND l.EndDate = @DenNgay
                ORDER BY l.LeaveID DESC
            )
            """,
            ("@TrangThai", trangThai),
            ("@NhanVien", nghiPhep.NhanVien),
            ("@LoaiNghi", nghiPhep.LoaiNghi),
            ("@TuNgay", nghiPhep.TuNgay.Date),
            ("@DenNgay", nghiPhep.DenNgay.Date),
            ("@LyDoXuLy", nghiPhep.LyDoXuLy.Trim()));
    }

    public async Task TaoDanhGiaAsync(int maNhanVien)
    {
        await ThucThiAsync("""
            DECLARE @KyDanhGia NVARCHAR(20) = CONCAT(YEAR(GETDATE()), '-Q', DATEPART(QUARTER, GETDATE()));

            IF EXISTS (SELECT 1 FROM HR_Appraisals WHERE EmployeeID = @MaNhanVien AND ReviewPeriod = @KyDanhGia)
            BEGIN
                UPDATE HR_Appraisals
                SET ReviewerID = @MaNhanVien,
                    Score = 85,
                    Feedback = N'Đánh giá mới từ ứng dụng',
                    Status = N'Nháp'
                WHERE EmployeeID = @MaNhanVien AND ReviewPeriod = @KyDanhGia;
            END
            ELSE
            BEGIN
                INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                VALUES(@MaNhanVien, @MaNhanVien, @KyDanhGia, 85, N'Đánh giá mới từ ứng dụng', N'Nháp');
            END
            """, ("@MaNhanVien", maNhanVien));
    }

    public async Task LuuDanhGiaAsync(BieuMauDanhGia danhGia)
    {
        await ThucThiAsync("""
            DECLARE @AppraisalID INT = NULL;

            IF @MaNhanVienGoc > 0 AND LEN(@KyDanhGiaGoc) > 0
            BEGIN
                SELECT TOP 1 @AppraisalID = AppraisalID
                FROM HR_Appraisals
                WHERE EmployeeID = @MaNhanVienGoc AND ReviewPeriod = @KyDanhGiaGoc
                ORDER BY AppraisalID DESC;
            END;

            IF @AppraisalID IS NULL
            BEGIN
                SELECT TOP 1 @AppraisalID = AppraisalID
                FROM HR_Appraisals
                WHERE EmployeeID = @MaNhanVien AND ReviewPeriod = @KyDanhGia
                ORDER BY AppraisalID DESC;
            END;

            IF @AppraisalID IS NULL
            BEGIN
                INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                VALUES(@MaNhanVien, @MaNguoiDanhGia, @KyDanhGia, @Diem, @NhanXet, @TrangThai);
            END
            ELSE
            BEGIN
                UPDATE HR_Appraisals
                SET EmployeeID = @MaNhanVien,
                    ReviewerID = @MaNguoiDanhGia,
                    ReviewPeriod = @KyDanhGia,
                    Score = @Diem,
                    Feedback = @NhanXet,
                    Status = @TrangThai
                WHERE AppraisalID = @AppraisalID;
            END;
            """,
            ("@MaNhanVien", danhGia.MaNhanVien),
            ("@MaNguoiDanhGia", danhGia.MaNguoiDanhGia),
            ("@KyDanhGia", danhGia.KyDanhGia.Trim()),
            ("@Diem", danhGia.Diem),
            ("@NhanXet", danhGia.NhanXet.Trim()),
            ("@TrangThai", danhGia.TrangThai.Trim()),
            ("@MaNhanVienGoc", danhGia.MaNhanVienGoc),
            ("@KyDanhGiaGoc", danhGia.KyDanhGiaGoc));
    }

    public async Task ChotDanhGiaAsync(DanhGia danhGia)
    {
        await ThucThiAsync("""
            UPDATE HR_Appraisals
            SET Status = N'Hoàn tất'
            WHERE AppraisalID = (
                SELECT TOP 1 a.AppraisalID
                FROM HR_Appraisals a
                JOIN HR_Employees e ON a.EmployeeID = e.EmployeeID
                WHERE e.FullName = @NhanVien AND a.ReviewPeriod = @KyDanhGia
                ORDER BY a.AppraisalID DESC
            )
            """,
            ("@NhanVien", danhGia.NhanVien),
            ("@KyDanhGia", danhGia.KyDanhGia));
    }

    public async Task XoaDanhGiaAsync(DanhGia danhGia)
    {
        await ThucThiAsync("""
            DELETE FROM HR_Appraisals
            WHERE AppraisalID = (
                SELECT TOP 1 a.AppraisalID
                FROM HR_Appraisals a
                JOIN HR_Employees e ON a.EmployeeID = e.EmployeeID
                JOIN HR_Employees r ON a.ReviewerID = r.EmployeeID
                WHERE e.FullName = @NhanVien
                    AND r.FullName = @NguoiDanhGia
                    AND a.ReviewPeriod = @KyDanhGia
                ORDER BY a.AppraisalID DESC
            )
            """,
            ("@NhanVien", danhGia.NhanVien),
            ("@NguoiDanhGia", danhGia.NguoiDanhGia),
            ("@KyDanhGia", danhGia.KyDanhGia));
    }

    public async Task TaoPhieuLuongAsync(int maNhanVien)
    {
        await ThucThiAsync("""
            DECLARE @Luong DECIMAL(18,2) = ISNULL((SELECT TOP 1 BasicSalary FROM HR_Contracts WHERE EmployeeID = @MaNhanVien ORDER BY ContractID DESC), 10000000);
            DECLARE @Ky VARCHAR(20) = FORMAT(GETDATE(), 'yyyy-MM');
            DECLARE @DauThang DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
            DECLARE @CuoiThang DATE = EOMONTH(@DauThang);
            DECLARE @NgayThamGiaBaoHiem DATE = ISNULL((SELECT SocialInsuranceStartDate FROM HR_Employees WHERE EmployeeID = @MaNhanVien), @DauThang);
            DECLARE @SoNamBaoHiem INT = CASE
                WHEN @NgayThamGiaBaoHiem > CAST(GETDATE() AS DATE) THEN 0
                ELSE DATEDIFF(YEAR, @NgayThamGiaBaoHiem, CAST(GETDATE() AS DATE))
                    - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, @NgayThamGiaBaoHiem, CAST(GETDATE() AS DATE)), @NgayThamGiaBaoHiem) > CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END
            END;
            IF @SoNamBaoHiem < 0 SET @SoNamBaoHiem = 0;
            IF @SoNamBaoHiem > 5 SET @SoNamBaoHiem = 5;
            DECLARE @TongGio DECIMAL(10,2) = ISNULL((
                SELECT SUM(
                    CASE
                        WHEN WorkHours IS NOT NULL AND WorkHours > 0 THEN WorkHours
                        WHEN CheckOutTime IS NOT NULL THEN CAST(DATEDIFF(MINUTE, CheckInTime, CheckOutTime) AS DECIMAL(10,2)) / 60
                        ELSE 0
                    END)
                FROM HR_Attendances
                WHERE EmployeeID = @MaNhanVien
                  AND CheckInTime >= @DauThang
                  AND CheckInTime < DATEADD(DAY, 1, @CuoiThang)
            ), 0);
            DECLARE @NgayCong DECIMAL(10,2) = CASE WHEN @TongGio <= 0 THEN 22 ELSE ROUND(@TongGio / 8, 2) END;
            IF @NgayCong > 22 SET @NgayCong = 22;

            DECLARE @NgayNghiDaDuyet DECIMAL(10,2) = ISNULL((
                SELECT SUM(DATEDIFF(DAY,
                    CASE WHEN StartDate < @DauThang THEN @DauThang ELSE StartDate END,
                    DATEADD(DAY, 1, CASE WHEN EndDate > @CuoiThang THEN @CuoiThang ELSE EndDate END)))
                FROM HR_LeaveRequests
                WHERE EmployeeID = @MaNhanVien
                  AND Status IN (N'Đã duyệt', 'Approved')
                  AND StartDate <= @CuoiThang
                  AND EndDate >= @DauThang
            ), 0);
            DECLARE @PhuCap DECIMAL(18,2) = ROUND(@Luong * (0.05 + @SoNamBaoHiem * 0.01), 0);
            DECLARE @KhauTruBaoHiem DECIMAL(18,2) = ROUND(@Luong * 0.105, 0);
            DECLARE @KhauTru DECIMAL(18,2) = ROUND(@Luong / 22 * @NgayNghiDaDuyet, 0) + @KhauTruBaoHiem;
            DECLARE @LuongTheoCong DECIMAL(18,2) = ROUND(@Luong / 22 * @NgayCong, 0);
            DECLARE @ThucLanh DECIMAL(18,2) = @LuongTheoCong + @PhuCap - @KhauTru;
            IF @ThucLanh < 0 SET @ThucLanh = 0;

            IF EXISTS (SELECT 1 FROM HR_Payslips WHERE EmployeeID = @MaNhanVien AND PayPeriod = @Ky)
            BEGIN
                UPDATE HR_Payslips
                SET BasicSalary = @Luong,
                    WorkDays = @NgayCong,
                    TotalAllowances = @PhuCap,
                    TotalDeductions = @KhauTru,
                    NetSalary = @ThucLanh,
                    Status = N'Nháp'
                WHERE EmployeeID = @MaNhanVien AND PayPeriod = @Ky;
            END
            ELSE
            BEGIN
                INSERT INTO HR_Payslips(EmployeeID, PayPeriod, BasicSalary, WorkDays, TotalAllowances, TotalDeductions, NetSalary, Status)
                VALUES(@MaNhanVien, @Ky, @Luong, @NgayCong, @PhuCap, @KhauTru, @ThucLanh, N'Nháp');
            END
            """, ("@MaNhanVien", maNhanVien));
    }

    public async Task XacNhanTraLuongAsync(PhieuLuong phieuLuong)
    {
        await ThucThiAsync("""
            UPDATE HR_Payslips
            SET Status = N'Đã trả'
            WHERE PayslipID = (
                SELECT TOP 1 p.PayslipID
                FROM HR_Payslips p
                JOIN HR_Employees e ON p.EmployeeID = e.EmployeeID
                WHERE e.FullName = @NhanVien AND p.PayPeriod = @KyLuong
                ORDER BY p.PayslipID DESC
            )
            """,
            ("@NhanVien", phieuLuong.NhanVien),
            ("@KyLuong", phieuLuong.KyLuong));
    }

    public async Task<IReadOnlyList<TaiKhoanHeThong>> TaiTaiKhoanHeThongAsync()
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);
        await TaiKhoanNhanSuSql.DamBaoTheoNhanVienAsync(ketNoi);

        var ketQua = new List<TaiKhoanHeThong>();
        await using var lenh = new SqlCommand("""
            SELECT Username, FullName, RoleName, IsActive, LastLoginAt
            FROM dbo.HR_Users
            ORDER BY
                CASE RoleName
                    WHEN N'Admin' THEN 1
                    WHEN N'Giám đốc' THEN 2
                    WHEN N'Trưởng phòng' THEN 3
                    ELSE 4
                END,
                Username
            """, ketNoi);

        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
        {
            var vaiTro = doc.GetString(2);
            ketQua.Add(new TaiKhoanHeThong(
                doc.GetString(0),
                doc.GetString(1),
                vaiTro,
                LayMoTaQuyen(vaiTro),
                doc.GetBoolean(3) ? "Đang hoạt động" : "Tạm khóa",
                doc.IsDBNull(4) ? DateTime.MinValue : doc.GetDateTime(4)));
        }

        return ketQua;
    }

    public async Task<int> DongBoTaiKhoanNhanSuAsync(string nguoiThucHien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);
        var soTaiKhoan = await TaiKhoanNhanSuSql.DamBaoTheoNhanVienAsync(ketNoi);

        await GhiNhatKyAsync(nguoiThucHien, "SyncEmployeeUsers", "HR_Users", "EmployeeCode", $"Dong bo {soTaiKhoan} tai khoan theo ho so nhan vien.");
        return soTaiKhoan;
    }

    public async Task<TaiKhoanHeThong> TaoTaiKhoanNhanVienAsync(int soThuTu, string nguoiThucHien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);

        var tenDangNhap = $"nhanvien{soThuTu:00}";
        var hoTen = $"Nhân viên mới {soThuTu:00}";
        var matKhau = BaoMatMatKhau.BamMatKhau(CauHinhUngDung.LayMatKhauKhoiTao());

        await using var lenh = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.HR_Users WHERE Username=@Username)
            BEGIN
                INSERT INTO dbo.HR_Users(Username, FullName, RoleName, PasswordHash, PasswordSalt, PasswordIterations, IsActive, RequirePasswordChange)
                VALUES(@Username, @FullName, N'Nhân viên', @PasswordHash, @PasswordSalt, @PasswordIterations, 1, 1);
            END
            """, ketNoi);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        lenh.Parameters.AddWithValue("@FullName", hoTen);
        lenh.Parameters.AddWithValue("@PasswordHash", matKhau.HashBase64);
        lenh.Parameters.AddWithValue("@PasswordSalt", matKhau.SaltBase64);
        lenh.Parameters.AddWithValue("@PasswordIterations", matKhau.Iterations);
        await lenh.ExecuteNonQueryAsync();

        await GhiNhatKyAsync(nguoiThucHien, "CreateUser", "HR_Users", tenDangNhap, $"Tao tai khoan {tenDangNhap}.");
        return new TaiKhoanHeThong(tenDangNhap, hoTen, "Nhân viên", LayMoTaQuyen("Nhân viên"), "Đang hoạt động", DateTime.MinValue);
    }

    public async Task LuuTaiKhoanAsync(BieuMauTaiKhoan bieuMau, string nguoiThucHien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);

        var dangSua = !string.IsNullOrWhiteSpace(bieuMau.TenDangNhapGoc);
        if (dangSua)
        {
            await using var lenh = new SqlCommand("""
                UPDATE dbo.HR_Users
                SET Username=@Username,
                    FullName=@FullName,
                    RoleName=@RoleName,
                    IsActive=@IsActive
                WHERE Username=@UsernameGoc
                """, ketNoi);
            lenh.Parameters.AddWithValue("@Username", bieuMau.TenDangNhap.Trim());
            lenh.Parameters.AddWithValue("@FullName", bieuMau.HoTen.Trim());
            lenh.Parameters.AddWithValue("@RoleName", bieuMau.VaiTro);
            lenh.Parameters.AddWithValue("@IsActive", bieuMau.DangHoatDong);
            lenh.Parameters.AddWithValue("@UsernameGoc", bieuMau.TenDangNhapGoc);
            await lenh.ExecuteNonQueryAsync();

            if (!string.IsNullOrWhiteSpace(bieuMau.MatKhauMoi))
            {
                await DatLaiMatKhauNoiBoAsync(ketNoi, bieuMau.TenDangNhap.Trim(), bieuMau.MatKhauMoi.Trim(), yeuCauDoiMatKhau: true);
            }

            await GhiNhatKyAsync(nguoiThucHien, "UpdateUser", "HR_Users", bieuMau.TenDangNhap.Trim(), $"Cap nhat thong tin tai khoan {bieuMau.TenDangNhap.Trim()}.");
            return;
        }

        var matKhau = BaoMatMatKhau.BamMatKhau(string.IsNullOrWhiteSpace(bieuMau.MatKhauMoi)
            ? CauHinhUngDung.LayMatKhauKhoiTao()
            : bieuMau.MatKhauMoi.Trim());
        await using var lenhThem = new SqlCommand("""
            INSERT INTO dbo.HR_Users(Username, FullName, RoleName, PasswordHash, PasswordSalt, PasswordIterations, IsActive, RequirePasswordChange)
            VALUES(@Username, @FullName, @RoleName, @PasswordHash, @PasswordSalt, @PasswordIterations, @IsActive, 1)
            """, ketNoi);
        lenhThem.Parameters.AddWithValue("@Username", bieuMau.TenDangNhap.Trim());
        lenhThem.Parameters.AddWithValue("@FullName", bieuMau.HoTen.Trim());
        lenhThem.Parameters.AddWithValue("@RoleName", bieuMau.VaiTro);
        lenhThem.Parameters.AddWithValue("@PasswordHash", matKhau.HashBase64);
        lenhThem.Parameters.AddWithValue("@PasswordSalt", matKhau.SaltBase64);
        lenhThem.Parameters.AddWithValue("@PasswordIterations", matKhau.Iterations);
        lenhThem.Parameters.AddWithValue("@IsActive", bieuMau.DangHoatDong);
        await lenhThem.ExecuteNonQueryAsync();

        await GhiNhatKyAsync(nguoiThucHien, "CreateUser", "HR_Users", bieuMau.TenDangNhap.Trim(), $"Tao tai khoan {bieuMau.TenDangNhap.Trim()}.");
    }

    public async Task KhoaMoTaiKhoanAsync(string tenDangNhap, bool kichHoat, string nguoiThucHien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);

        await using var lenh = new SqlCommand("""
            UPDATE dbo.HR_Users
            SET IsActive=@IsActive,
                FailedLoginCount = CASE WHEN @IsActive = 1 THEN 0 ELSE FailedLoginCount END,
                LockoutUntilAt = CASE WHEN @IsActive = 1 THEN NULL ELSE LockoutUntilAt END
            WHERE Username=@Username
            """, ketNoi);
        lenh.Parameters.AddWithValue("@IsActive", kichHoat);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        await lenh.ExecuteNonQueryAsync();

        await GhiNhatKyAsync(nguoiThucHien, kichHoat ? "EnableUser" : "DisableUser", "HR_Users", tenDangNhap, $"Cap nhat trang thai tai khoan {tenDangNhap}.");
    }

    public async Task DatLaiMatKhauAsync(string tenDangNhap, string matKhauMoi, string nguoiThucHien)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);

        await DatLaiMatKhauNoiBoAsync(ketNoi, tenDangNhap, matKhauMoi, yeuCauDoiMatKhau: true);
        await GhiNhatKyAsync(nguoiThucHien, "ResetPassword", "HR_Users", tenDangNhap, $"Dat lai mat khau cho {tenDangNhap}.");
    }

    private static async Task DatLaiMatKhauNoiBoAsync(SqlConnection ketNoi, string tenDangNhap, string matKhauMoi, bool yeuCauDoiMatKhau)
    {
        var matKhau = BaoMatMatKhau.BamMatKhau(matKhauMoi);
        await using var lenh = new SqlCommand("""
            UPDATE dbo.HR_Users
            SET PasswordHash=@PasswordHash,
                PasswordSalt=@PasswordSalt,
                PasswordIterations=@PasswordIterations,
                RequirePasswordChange=@RequirePasswordChange,
                FailedLoginCount=0,
                LockoutUntilAt=NULL
            WHERE Username=@Username
            """, ketNoi);
        lenh.Parameters.AddWithValue("@PasswordHash", matKhau.HashBase64);
        lenh.Parameters.AddWithValue("@PasswordSalt", matKhau.SaltBase64);
        lenh.Parameters.AddWithValue("@PasswordIterations", matKhau.Iterations);
        lenh.Parameters.AddWithValue("@RequirePasswordChange", yeuCauDoiMatKhau);
        lenh.Parameters.AddWithValue("@Username", tenDangNhap);
        await lenh.ExecuteNonQueryAsync();
    }

    public async Task GhiNhatKyAsync(string nguoiThucHien, string hanhDong, string thucThe, string maThucThe, string chiTiet)
    {
        try
        {
            await using var ketNoi = new SqlConnection(chuoiKetNoi);
            await ketNoi.OpenAsync();
            await SoDoQuanTriSql.DamBaoAsync(ketNoi);

            await using var lenh = new SqlCommand("""
                INSERT INTO dbo.HR_AuditLogs(ActorUsername, ActionName, EntityName, EntityKey, Detail, MachineName)
                VALUES(@ActorUsername, @ActionName, @EntityName, @EntityKey, @Detail, @MachineName)
                """, ketNoi);
            lenh.Parameters.AddWithValue("@ActorUsername", nguoiThucHien);
            lenh.Parameters.AddWithValue("@ActionName", hanhDong);
            lenh.Parameters.AddWithValue("@EntityName", thucThe);
            lenh.Parameters.AddWithValue("@EntityKey", maThucThe);
            lenh.Parameters.AddWithValue("@Detail", chiTiet);
            lenh.Parameters.AddWithValue("@MachineName", Environment.MachineName);
            await lenh.ExecuteNonQueryAsync();
        }
        catch
        {
            // Audit khong duoc lam gian doan nghiep vu chinh.
        }
    }

    private async Task ThucThiAsync(string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await using var lenh = new SqlCommand(sql, ketNoi);
        foreach (var (ten, giaTri) in thamSo)
        {
            lenh.Parameters.AddWithValue(ten, giaTri ?? DBNull.Value);
        }

        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task DamBaoCoSoDuLieuAsync(string chuoiKetNoi)
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

    private static async Task DamBaoCauTrucVaDuLieuSqlAsync(SqlConnection ketNoi)
    {
        await DamBaoCauTrucSqlAsync(ketNoi);
        await SoDoQuanTriSql.DamBaoAsync(ketNoi);
        await DamBaoDuLieuMacDinhSqlAsync(ketNoi);
        await DamBaoPhanCongTruongPhongHopLeSqlAsync(ketNoi);
        await TaiKhoanNhanSuSql.DamBaoTheoNhanVienAsync(ketNoi);
    }

    private static async Task DamBaoCauTrucSqlAsync(SqlConnection ketNoi)
    {
        await using var lenh = new SqlCommand("""
            IF OBJECT_ID(N'dbo.HR_Departments', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Departments
                (
                    DepartmentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Name NVARCHAR(150) NOT NULL,
                    ManagerID INT NULL
                );
            END;

            IF OBJECT_ID(N'dbo.HR_JobPositions', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_JobPositions
                (
                    PositionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    DepartmentID INT NOT NULL,
                    Name NVARCHAR(150) NOT NULL,
                    ExpectedSalary DECIMAL(18,2) NULL,
                    Status NVARCHAR(60) NOT NULL DEFAULT(N'Đang tuyển')
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Employees', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Employees
                (
                    EmployeeID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeCode VARCHAR(20) NOT NULL,
                    ApplicantID INT NULL,
                    FullName NVARCHAR(150) NOT NULL,
                    DepartmentID INT NOT NULL,
                    PositionID INT NOT NULL,
                    ManagerID INT NULL,
                    BirthDate DATE NULL,
                    SocialInsuranceStartDate DATE NULL,
                    JoinDate DATE NOT NULL,
                    IsActive BIT NOT NULL DEFAULT(1),
                    EmergencyContact NVARCHAR(50) NULL,
                    BankAccount NVARCHAR(50) NULL,
                    IdentityNumber NVARCHAR(50) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Applicants', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Applicants
                (
                    ApplicantID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    PositionID INT NOT NULL,
                    FullName NVARCHAR(150) NOT NULL,
                    Email NVARCHAR(150) NOT NULL,
                    Phone NVARCHAR(30) NULL,
                    CVFile_Url NVARCHAR(500) NULL,
                    Stage NVARCHAR(80) NOT NULL DEFAULT(N'Mới')
                );
            END;

            IF OBJECT_ID(N'dbo.HR_LeaveRequests', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_LeaveRequests
                (
                    LeaveID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeID INT NOT NULL,
                    LeaveType NVARCHAR(100) NOT NULL,
                    StartDate DATE NOT NULL,
                    EndDate DATE NOT NULL,
                    TotalDays DECIMAL(10,2) NOT NULL,
                    Status NVARCHAR(60) NOT NULL DEFAULT(N'Chờ duyệt'),
                    ApproverID INT NULL,
                    Reason NVARCHAR(500) NULL,
                    ApprovalNote NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Attendances', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Attendances
                (
                    AttendanceID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeID INT NOT NULL,
                    CheckInTime DATETIME NOT NULL,
                    CheckOutTime DATETIME NULL,
                    WorkHours DECIMAL(10,2) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Appraisals', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Appraisals
                (
                    AppraisalID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeID INT NOT NULL,
                    ReviewerID INT NOT NULL,
                    ReviewPeriod NVARCHAR(20) NOT NULL,
                    Score DECIMAL(5,2) NULL,
                    Feedback NVARCHAR(500) NULL,
                    Status NVARCHAR(60) NOT NULL DEFAULT(N'Hoàn tất')
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Payslips', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Payslips
                (
                    PayslipID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeID INT NOT NULL,
                    PayPeriod VARCHAR(20) NOT NULL,
                    BasicSalary DECIMAL(18,2) NOT NULL,
                    WorkDays DECIMAL(10,2) NOT NULL DEFAULT(22),
                    TotalAllowances DECIMAL(18,2) NOT NULL DEFAULT(0),
                    TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT(0),
                    NetSalary DECIMAL(18,2) NOT NULL,
                    Status NVARCHAR(60) NOT NULL DEFAULT(N'Nháp')
                );
            END;

            IF OBJECT_ID(N'dbo.HR_Contracts', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Contracts
                (
                    ContractID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    EmployeeID INT NOT NULL,
                    ContractType NVARCHAR(100) NOT NULL,
                    StartDate DATE NOT NULL,
                    EndDate DATE NULL,
                    BasicSalary DECIMAL(18,2) NOT NULL,
                    Status NVARCHAR(60) NOT NULL DEFAULT(N'Đang hiệu lực')
                );
            END;

            DECLARE @DropDefaultSql NVARCHAR(MAX) = N'';
            SELECT @DropDefaultSql = @DropDefaultSql
                + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
            FROM sys.default_constraints dc
            JOIN sys.tables t ON dc.parent_object_id = t.object_id
            JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = dc.parent_column_id
            WHERE (t.name = N'HR_Applicants' AND c.name = N'Stage')
               OR (t.name IN (N'HR_JobPositions', N'HR_LeaveRequests', N'HR_Appraisals', N'HR_Payslips', N'HR_Contracts', N'HR_Expenses') AND c.name = N'Status');
            IF @DropDefaultSql <> N'' EXEC sp_executesql @DropDefaultSql;

            IF COL_LENGTH(N'dbo.HR_Departments', N'ManagerID') IS NULL
                ALTER TABLE dbo.HR_Departments ADD ManagerID INT NULL;

            IF COL_LENGTH(N'dbo.HR_JobPositions', N'ExpectedSalary') IS NULL
                ALTER TABLE dbo.HR_JobPositions ADD ExpectedSalary DECIMAL(18,2) NULL;
            IF COL_LENGTH(N'dbo.HR_JobPositions', N'Status') IS NULL
                ALTER TABLE dbo.HR_JobPositions ADD Status NVARCHAR(60) NOT NULL DEFAULT(N'Đang tuyển');
            ELSE
                ALTER TABLE dbo.HR_JobPositions ALTER COLUMN Status NVARCHAR(60) NOT NULL;

            IF COL_LENGTH(N'dbo.HR_Employees', N'ApplicantID') IS NULL
                ALTER TABLE dbo.HR_Employees ADD ApplicantID INT NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'ManagerID') IS NULL
                ALTER TABLE dbo.HR_Employees ADD ManagerID INT NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'EmergencyContact') IS NULL
                ALTER TABLE dbo.HR_Employees ADD EmergencyContact NVARCHAR(50) NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'BankAccount') IS NULL
                ALTER TABLE dbo.HR_Employees ADD BankAccount NVARCHAR(50) NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'IdentityNumber') IS NULL
                ALTER TABLE dbo.HR_Employees ADD IdentityNumber NVARCHAR(50) NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'BirthDate') IS NULL
                ALTER TABLE dbo.HR_Employees ADD BirthDate DATE NULL;
            IF COL_LENGTH(N'dbo.HR_Employees', N'SocialInsuranceStartDate') IS NULL
                ALTER TABLE dbo.HR_Employees ADD SocialInsuranceStartDate DATE NULL;
            UPDATE dbo.HR_Employees
            SET BirthDate = ISNULL(BirthDate, DATEADD(YEAR, -25, JoinDate)),
                SocialInsuranceStartDate = ISNULL(SocialInsuranceStartDate, DATEADD(MONTH, -((EmployeeID % 96) + 12), CAST(GETDATE() AS DATE)));

            IF COL_LENGTH(N'dbo.HR_Applicants', N'CVFile_Url') IS NULL
                ALTER TABLE dbo.HR_Applicants ADD CVFile_Url NVARCHAR(500) NULL;
            IF COL_LENGTH(N'dbo.HR_Applicants', N'Stage') IS NULL
                ALTER TABLE dbo.HR_Applicants ADD Stage NVARCHAR(80) NOT NULL DEFAULT(N'Mới');
            ELSE
                ALTER TABLE dbo.HR_Applicants ALTER COLUMN Stage NVARCHAR(80) NOT NULL;

            IF COL_LENGTH(N'dbo.HR_LeaveRequests', N'ApproverID') IS NULL
                ALTER TABLE dbo.HR_LeaveRequests ADD ApproverID INT NULL;
            IF COL_LENGTH(N'dbo.HR_LeaveRequests', N'Reason') IS NULL
                ALTER TABLE dbo.HR_LeaveRequests ADD Reason NVARCHAR(500) NULL;
            IF COL_LENGTH(N'dbo.HR_LeaveRequests', N'ApprovalNote') IS NULL
                ALTER TABLE dbo.HR_LeaveRequests ADD ApprovalNote NVARCHAR(500) NULL;

            IF COL_LENGTH(N'dbo.HR_Attendances', N'WorkHours') IS NULL
                ALTER TABLE dbo.HR_Attendances ADD WorkHours DECIMAL(10,2) NULL;

            IF COL_LENGTH(N'dbo.HR_Appraisals', N'Score') IS NULL
                ALTER TABLE dbo.HR_Appraisals ADD Score DECIMAL(5,2) NULL;
            IF COL_LENGTH(N'dbo.HR_Appraisals', N'Feedback') IS NULL
                ALTER TABLE dbo.HR_Appraisals ADD Feedback NVARCHAR(500) NULL;
            IF COL_LENGTH(N'dbo.HR_Appraisals', N'Status') IS NULL
                ALTER TABLE dbo.HR_Appraisals ADD Status NVARCHAR(60) NOT NULL DEFAULT(N'Hoàn tất');
            ELSE
                ALTER TABLE dbo.HR_Appraisals ALTER COLUMN Status NVARCHAR(60) NOT NULL;

            IF COL_LENGTH(N'dbo.HR_Payslips', N'WorkDays') IS NULL
                ALTER TABLE dbo.HR_Payslips ADD WorkDays DECIMAL(10,2) NOT NULL DEFAULT(22);
            IF COL_LENGTH(N'dbo.HR_Payslips', N'TotalAllowances') IS NULL
                ALTER TABLE dbo.HR_Payslips ADD TotalAllowances DECIMAL(18,2) NOT NULL DEFAULT(0);
            IF COL_LENGTH(N'dbo.HR_Payslips', N'TotalDeductions') IS NULL
                ALTER TABLE dbo.HR_Payslips ADD TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT(0);
            IF COL_LENGTH(N'dbo.HR_Payslips', N'Status') IS NULL
                ALTER TABLE dbo.HR_Payslips ADD Status NVARCHAR(60) NOT NULL DEFAULT(N'Nháp');
            ELSE
                ALTER TABLE dbo.HR_Payslips ALTER COLUMN Status NVARCHAR(60) NOT NULL;

            IF COL_LENGTH(N'dbo.HR_Contracts', N'EndDate') IS NULL
                ALTER TABLE dbo.HR_Contracts ADD EndDate DATE NULL;
            IF COL_LENGTH(N'dbo.HR_Contracts', N'Status') IS NULL
                ALTER TABLE dbo.HR_Contracts ADD Status NVARCHAR(60) NOT NULL DEFAULT(N'Đang hiệu lực');
            ELSE
                ALTER TABLE dbo.HR_Contracts ALTER COLUMN Status NVARCHAR(60) NOT NULL;

            IF COL_LENGTH(N'dbo.HR_LeaveRequests', N'Status') IS NOT NULL
                ALTER TABLE dbo.HR_LeaveRequests ALTER COLUMN Status NVARCHAR(60) NOT NULL;

            UPDATE dbo.HR_Applicants
            SET Stage = CASE Stage
                WHEN 'New' THEN N'Mới'
                WHEN 'Screening' THEN N'Sàng lọc hồ sơ'
                WHEN 'Interview' THEN N'Phỏng vấn'
                WHEN 'Offer' THEN N'Đề nghị nhận việc'
                WHEN 'Signed' THEN N'Đã tiếp nhận'
                WHEN 'Rejected' THEN N'Từ chối'
                ELSE Stage
            END;

            UPDATE dbo.HR_JobPositions
            SET Status = CASE Status
                WHEN 'Open' THEN N'Đang tuyển'
                WHEN 'Closed' THEN N'Đã đóng'
                ELSE Status
            END;

            UPDATE dbo.HR_LeaveRequests
            SET Status = CASE Status
                WHEN 'Pending' THEN N'Chờ duyệt'
                WHEN 'Waiting' THEN N'Chờ duyệt'
                WHEN 'Approved' THEN N'Đã duyệt'
                WHEN 'Rejected' THEN N'Từ chối'
                ELSE Status
            END;

            UPDATE dbo.HR_Appraisals
            SET Status = CASE Status
                WHEN 'Draft' THEN N'Nháp'
                WHEN 'Completed' THEN N'Hoàn tất'
                ELSE Status
            END;

            UPDATE dbo.HR_Payslips
            SET Status = CASE Status
                WHEN 'Draft' THEN N'Nháp'
                WHEN 'Paid' THEN N'Đã trả'
                ELSE Status
            END;

            UPDATE dbo.HR_Contracts
            SET Status = CASE Status
                WHEN 'Active' THEN N'Đang hiệu lực'
                WHEN 'Running' THEN N'Đang hiệu lực'
                ELSE Status
            END;

            IF OBJECT_ID(N'dbo.HR_Expenses', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.HR_Expenses', N'Status') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.HR_Expenses ALTER COLUMN Status NVARCHAR(60) NOT NULL;
                UPDATE dbo.HR_Expenses
                SET Status = CASE Status
                    WHEN 'Pending' THEN N'Chờ duyệt'
                    WHEN 'Approved' THEN N'Đã duyệt'
                    WHEN 'Rejected' THEN N'Từ chối'
                    WHEN 'Paid' THEN N'Đã thanh toán'
                    ELSE Status
                END;
            END;

            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_Applicants' AND c.name=N'Stage')
                ALTER TABLE dbo.HR_Applicants ADD CONSTRAINT DF_HR_Applicants_Stage DEFAULT(N'Mới') FOR Stage;
            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_JobPositions' AND c.name=N'Status')
                ALTER TABLE dbo.HR_JobPositions ADD CONSTRAINT DF_HR_JobPositions_Status DEFAULT(N'Đang tuyển') FOR Status;
            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_LeaveRequests' AND c.name=N'Status')
                ALTER TABLE dbo.HR_LeaveRequests ADD CONSTRAINT DF_HR_LeaveRequests_Status DEFAULT(N'Chờ duyệt') FOR Status;
            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_Appraisals' AND c.name=N'Status')
                ALTER TABLE dbo.HR_Appraisals ADD CONSTRAINT DF_HR_Appraisals_Status DEFAULT(N'Hoàn tất') FOR Status;
            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_Payslips' AND c.name=N'Status')
                ALTER TABLE dbo.HR_Payslips ADD CONSTRAINT DF_HR_Payslips_Status DEFAULT(N'Nháp') FOR Status;
            IF NOT EXISTS (SELECT 1 FROM sys.default_constraints dc JOIN sys.tables t ON dc.parent_object_id=t.object_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=dc.parent_column_id WHERE t.name=N'HR_Contracts' AND c.name=N'Status')
                ALTER TABLE dbo.HR_Contracts ADD CONSTRAINT DF_HR_Contracts_Status DEFAULT(N'Đang hiệu lực') FOR Status;
            """, ketNoi);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task DamBaoDuLieuMacDinhSqlAsync(SqlConnection ketNoi)
    {
        var daCoDuLieuKhoiTao = await LaySoNguyenAsync(ketNoi,
            "SELECT COUNT(1) FROM HR_Employees WHERE EmployeeCode IN ('GD001', 'CN200')");
        if (daCoDuLieuKhoiTao.GetValueOrDefault() >= 2)
        {
            await CapNhatDuLieuNhanSuKhoiTaoSqlAsync(ketNoi);
            await DamBaoDuLieuNghiepVuMauSqlAsync(ketNoi);
            await DamBaoDanhGiaMauSqlAsync(ketNoi);
            return;
        }

        var phongBanIds = new Dictionary<string, int>();
        foreach (var phongBan in BoDuLieuKhoiTao.PhongBan)
        {
            phongBanIds[phongBan.TenPhongBan] = await LayHoacThemPhongBanAsync(ketNoi, phongBan.TenPhongBan);
        }

        var viTriIds = new Dictionary<string, int>();
        foreach (var viTri in BoDuLieuKhoiTao.ViTri)
        {
            var maPhongBan = phongBanIds[viTri.TenPhongBan];
            viTriIds[viTri.TenViTri] = await LayHoacThemViTriAsync(ketNoi, maPhongBan, viTri);
        }

        var nhanSu = BoDuLieuKhoiTao.TaoNhanSu();
        var nhanVienIds = new Dictionary<string, int>();
        for (var i = 0; i < nhanSu.Count; i++)
        {
            var dong = nhanSu[i];
            var maNhanVien = await LayHoacThemNhanVienAsync(
                ketNoi,
                dong,
                phongBanIds[dong.TenPhongBan],
                viTriIds[dong.TenViTri],
                i + 1);
            nhanVienIds[dong.MaSo] = maNhanVien;
        }

        foreach (var dong in nhanSu.Where(x => !string.IsNullOrWhiteSpace(x.MaSoQuanLy)))
        {
            if (nhanVienIds.TryGetValue(dong.MaSo, out var maNhanVien) &&
                nhanVienIds.TryGetValue(dong.MaSoQuanLy!, out var maQuanLy))
            {
                await ThucThiTrenKetNoiAsync(ketNoi,
                    "UPDATE HR_Employees SET ManagerID=@MaQuanLy WHERE EmployeeID=@MaNhanVien",
                    ("@MaQuanLy", maQuanLy),
                    ("@MaNhanVien", maNhanVien));
            }
        }

        foreach (var phongBan in BoDuLieuKhoiTao.PhongBan)
        {
            if (nhanVienIds.TryGetValue(phongBan.MaSoTruongPhong, out var maTruongPhong))
            {
                await ThucThiTrenKetNoiAsync(ketNoi,
                    "UPDATE HR_Departments SET ManagerID=@MaTruongPhong WHERE DepartmentID=@MaPhongBan",
                    ("@MaTruongPhong", maTruongPhong),
                    ("@MaPhongBan", phongBanIds[phongBan.TenPhongBan]));
            }
        }

        var kyLuong = DateTime.Today.ToString("yyyy-MM");
        foreach (var dong in nhanSu)
        {
            var maNhanVien = nhanVienIds[dong.MaSo];
            var phuCap = dong.LuongCoBan >= 24_000_000 ? 2_500_000m : 700_000m;
            var khauTru = dong.LuongCoBan >= 24_000_000 ? 1_000_000m : 300_000m;
            await ThucThiTrenKetNoiAsync(ketNoi, """
                IF NOT EXISTS (SELECT 1 FROM HR_Contracts WHERE EmployeeID=@MaNhanVien)
                BEGIN
                    INSERT INTO HR_Contracts(EmployeeID, ContractType, StartDate, EndDate, BasicSalary, Status)
                    VALUES(@MaNhanVien, N'Hợp đồng làm việc', @NgayBatDau, NULL, @LuongCoBan, N'Đang hiệu lực');
                END
                """,
                ("@MaNhanVien", maNhanVien),
                ("@NgayBatDau", dong.NgayVaoLam),
                ("@LuongCoBan", dong.LuongCoBan));

            await ThucThiTrenKetNoiAsync(ketNoi, """
                IF NOT EXISTS (SELECT 1 FROM HR_Payslips WHERE EmployeeID=@MaNhanVien AND PayPeriod=@KyLuong)
                BEGIN
                    INSERT INTO HR_Payslips(EmployeeID, PayPeriod, BasicSalary, WorkDays, TotalAllowances, TotalDeductions, NetSalary, Status)
                    VALUES(@MaNhanVien, @KyLuong, @LuongCoBan, 22, @PhuCap, @KhauTru, @ThucLanh, N'Đã trả');
                END
                """,
                ("@MaNhanVien", maNhanVien),
                ("@KyLuong", kyLuong),
                ("@LuongCoBan", dong.LuongCoBan),
                ("@PhuCap", phuCap),
                ("@KhauTru", khauTru),
                ("@ThucLanh", dong.LuongCoBan + phuCap - khauTru));
        }

        await DamBaoDuLieuNghiepVuMauSqlAsync(ketNoi);
        await DamBaoDanhGiaMauSqlAsync(ketNoi);
    }

    private static async Task DamBaoPhanCongTruongPhongHopLeSqlAsync(SqlConnection ketNoi)
    {
        await ThucThiTrenKetNoiAsync(ketNoi, """
            DECLARE @GiamDoc INT = (SELECT TOP 1 EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');

            DECLARE @PhanCong TABLE
            (
                TenPhongBan NVARCHAR(150) NOT NULL,
                MaNhanVien NVARCHAR(20) NULL,
                HoTen NVARCHAR(150) NULL
            );

            INSERT INTO @PhanCong(TenPhongBan, MaNhanVien, HoTen) VALUES
                (N'Ban Giám đốc', 'GD001', N'Nguyễn Minh Đức'),
                (N'Phòng Kinh doanh', 'TP001', N'Trần Quốc Huy'),
                (N'Phòng Sản xuất', 'TP002', N'Phạm Văn Long'),
                (N'Phòng Nhân sự', 'TP003', N'Lê Thu Hà'),
                (N'Phòng Kế toán', 'NV003', N'Bùi Thu Trang'),
                (N'Phòng CNTT', NULL, N'Phạm Gia Dũng'),
                (N'Phòng Marketing', 'NV006', N'Mai Ngọc Linh'),
                (N'Phòng Hành chính', 'TP004', N'Đỗ Thị Mai'),
                (N'Phòng Vận hành', NULL, N'Bùi Văn Hậu'),
                (N'Phòng Chăm sóc khách hàng', NULL, N'Vũ Minh Huy'),
                (N'Phòng Pháp chế', 'TP005', N'Vũ Anh Tuấn');

            UPDATE e
            SET DepartmentID = d.DepartmentID
            FROM HR_Employees e
            JOIN @PhanCong pc
                ON (pc.MaNhanVien IS NOT NULL AND e.EmployeeCode = pc.MaNhanVien)
                OR (pc.HoTen IS NOT NULL AND LTRIM(RTRIM(e.FullName)) = LTRIM(RTRIM(pc.HoTen)))
            JOIN HR_Departments d ON d.Name = pc.TenPhongBan
            WHERE e.DepartmentID <> d.DepartmentID;

            UPDATE d
            SET ManagerID = COALESCE(uuTien.EmployeeID, nhanSuDauPhong.EmployeeID)
            FROM HR_Departments d
            JOIN @PhanCong pc ON pc.TenPhongBan = d.Name
            OUTER APPLY
            (
                SELECT TOP 1 e.EmployeeID
                FROM HR_Employees e
                WHERE (pc.MaNhanVien IS NOT NULL AND e.EmployeeCode = pc.MaNhanVien)
                   OR (pc.HoTen IS NOT NULL AND LTRIM(RTRIM(e.FullName)) = LTRIM(RTRIM(pc.HoTen)))
                ORDER BY CASE WHEN e.DepartmentID = d.DepartmentID THEN 0 ELSE 1 END, e.EmployeeID
            ) uuTien
            OUTER APPLY
            (
                SELECT TOP 1 e.EmployeeID
                FROM HR_Employees e
                WHERE e.DepartmentID = d.DepartmentID
                  AND e.IsActive = 1
                ORDER BY
                    CASE WHEN e.PositionID IN (
                        SELECT PositionID
                        FROM HR_JobPositions
                        WHERE DepartmentID = d.DepartmentID
                          AND Name LIKE N'%Trưởng phòng%'
                    ) THEN 0 ELSE 1 END,
                    e.EmployeeID
            ) nhanSuDauPhong
            WHERE COALESCE(uuTien.EmployeeID, nhanSuDauPhong.EmployeeID) IS NOT NULL
              AND (d.ManagerID IS NULL OR d.ManagerID <> COALESCE(uuTien.EmployeeID, nhanSuDauPhong.EmployeeID));

            UPDATE e
            SET ManagerID =
                CASE
                    WHEN e.EmployeeID = d.ManagerID THEN
                        CASE WHEN e.EmployeeID = @GiamDoc THEN NULL ELSE @GiamDoc END
                    ELSE d.ManagerID
                END
            FROM HR_Employees e
            JOIN HR_Departments d ON d.DepartmentID = e.DepartmentID
            WHERE e.IsActive = 1
              AND d.ManagerID IS NOT NULL
              AND ISNULL(e.ManagerID, -1) <> ISNULL(
                    CASE
                        WHEN e.EmployeeID = d.ManagerID THEN
                            CASE WHEN e.EmployeeID = @GiamDoc THEN NULL ELSE @GiamDoc END
                        ELSE d.ManagerID
                    END,
                    -1);
            """);
    }

    private static async Task CapNhatDuLieuNhanSuKhoiTaoSqlAsync(SqlConnection ketNoi)
    {
        var phongBanIds = new Dictionary<string, int>();
        foreach (var phongBan in BoDuLieuKhoiTao.PhongBan)
        {
            phongBanIds[phongBan.TenPhongBan] = await LayHoacThemPhongBanAsync(ketNoi, phongBan.TenPhongBan);
        }

        var viTriIds = new Dictionary<string, int>();
        foreach (var viTri in BoDuLieuKhoiTao.ViTri)
        {
            var maPhongBan = phongBanIds[viTri.TenPhongBan];
            viTriIds[viTri.TenViTri] = await LayHoacThemViTriAsync(ketNoi, maPhongBan, viTri);
        }

        var nhanSu = BoDuLieuKhoiTao.TaoNhanSu();
        var nhanVienIds = new Dictionary<string, int>();
        for (var i = 0; i < nhanSu.Count; i++)
        {
            var dong = nhanSu[i];
            var maNhanVien = await LayHoacThemNhanVienAsync(
                ketNoi,
                dong,
                phongBanIds[dong.TenPhongBan],
                viTriIds[dong.TenViTri],
                i + 1);
            nhanVienIds[dong.MaSo] = maNhanVien;

            await ThucThiTrenKetNoiAsync(ketNoi, """
                UPDATE HR_Contracts
                SET BasicSalary=@LuongCoBan,
                    Status=N'Đang hiệu lực'
                WHERE ContractID = (
                    SELECT TOP 1 ContractID
                    FROM HR_Contracts
                    WHERE EmployeeID=@MaNhanVien
                    ORDER BY StartDate DESC, ContractID DESC
                );
                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO HR_Contracts(EmployeeID, ContractType, StartDate, EndDate, BasicSalary, Status)
                    VALUES(@MaNhanVien, N'Hợp đồng làm việc', @NgayBatDau, NULL, @LuongCoBan, N'Đang hiệu lực');
                END;
                """,
                ("@MaNhanVien", maNhanVien),
                ("@NgayBatDau", dong.NgayVaoLam),
                ("@LuongCoBan", dong.LuongCoBan));
        }

        foreach (var dong in nhanSu.Where(x => !string.IsNullOrWhiteSpace(x.MaSoQuanLy)))
        {
            if (nhanVienIds.TryGetValue(dong.MaSo, out var maNhanVien) &&
                nhanVienIds.TryGetValue(dong.MaSoQuanLy!, out var maQuanLy))
            {
                await ThucThiTrenKetNoiAsync(ketNoi,
                    "UPDATE HR_Employees SET ManagerID=@MaQuanLy WHERE EmployeeID=@MaNhanVien",
                    ("@MaQuanLy", maQuanLy),
                    ("@MaNhanVien", maNhanVien));
            }
        }

        foreach (var phongBan in BoDuLieuKhoiTao.PhongBan)
        {
            if (nhanVienIds.TryGetValue(phongBan.MaSoTruongPhong, out var maTruongPhong))
            {
                await ThucThiTrenKetNoiAsync(ketNoi,
                    "UPDATE HR_Departments SET ManagerID=@MaTruongPhong WHERE DepartmentID=@MaPhongBan",
                    ("@MaTruongPhong", maTruongPhong),
                    ("@MaPhongBan", phongBanIds[phongBan.TenPhongBan]));
            }
        }
    }

    private static async Task DamBaoDuLieuNghiepVuMauSqlAsync(SqlConnection ketNoi)
    {
        await ThucThiTrenKetNoiAsync(ketNoi, """
            DECLARE @HomNay DATE = CAST(GETDATE() AS DATE);
            DECLARE @KyLuong VARCHAR(20) = CONVERT(CHAR(7), @HomNay, 120);
            DECLARE @DauThang DATE = DATEFROMPARTS(YEAR(@HomNay), MONTH(@HomNay), 1);
            DECLARE @CuoiThang DATE = EOMONTH(@DauThang);
            DECLARE @GiamDoc INT = (SELECT TOP 1 EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');

            INSERT INTO HR_Contracts(EmployeeID, ContractType, StartDate, EndDate, BasicSalary, Status)
            SELECT e.EmployeeID,
                   N'Hợp đồng làm việc',
                   e.JoinDate,
                   NULL,
                   ISNULL(p.ExpectedSalary, 10000000),
                   N'Đang hiệu lực'
            FROM HR_Employees e
            JOIN HR_JobPositions p ON p.PositionID = e.PositionID
            WHERE e.IsActive = 1
              AND NOT EXISTS (SELECT 1 FROM HR_Contracts c WHERE c.EmployeeID = e.EmployeeID);

            INSERT INTO HR_Payslips(EmployeeID, PayPeriod, BasicSalary, WorkDays, TotalAllowances, TotalDeductions, NetSalary, Status)
            SELECT e.EmployeeID,
                   @KyLuong,
                   ISNULL(p.ExpectedSalary, 10000000),
                   22,
                   CASE WHEN ISNULL(p.ExpectedSalary, 0) >= 24000000 THEN 2500000 ELSE 700000 END,
                   CASE WHEN ISNULL(p.ExpectedSalary, 0) >= 24000000 THEN 1000000 ELSE 300000 END,
                   ISNULL(p.ExpectedSalary, 10000000)
                       + CASE WHEN ISNULL(p.ExpectedSalary, 0) >= 24000000 THEN 2500000 ELSE 700000 END
                       - CASE WHEN ISNULL(p.ExpectedSalary, 0) >= 24000000 THEN 1000000 ELSE 300000 END,
                   CASE WHEN e.EmployeeID % 7 = 0 THEN N'Nháp' ELSE N'Đã trả' END
            FROM HR_Employees e
            JOIN HR_JobPositions p ON p.PositionID = e.PositionID
            WHERE e.IsActive = 1
              AND NOT EXISTS (SELECT 1 FROM HR_Payslips s WHERE s.EmployeeID = e.EmployeeID AND s.PayPeriod = @KyLuong);

            ;WITH NgayLam AS
            (
                SELECT DATEADD(DAY, v.SoNgay, @DauThang) AS Ngay, v.SoNgay
                FROM (VALUES
                    (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
                    (11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21)
                ) AS v(SoNgay)
            ),
            DuLieuCong AS
            (
                SELECT e.EmployeeID,
                       DATEADD(MINUTE, (e.EmployeeID + n.SoNgay * 3) % 18, DATEADD(HOUR, 8, CAST(n.Ngay AS DATETIME))) AS GioVao,
                       CAST(7.50 + ((e.EmployeeID + n.SoNgay) % 4) * 0.25 AS DECIMAL(10,2)) AS SoGio
                FROM HR_Employees e
                CROSS JOIN NgayLam n
                WHERE e.IsActive = 1
            )
            INSERT INTO HR_Attendances(EmployeeID, CheckInTime, CheckOutTime, WorkHours)
            SELECT d.EmployeeID,
                   d.GioVao,
                   DATEADD(MINUTE, CAST(d.SoGio * 60 AS INT), d.GioVao),
                   d.SoGio
            FROM DuLieuCong d
            WHERE NOT EXISTS (
                SELECT 1
                FROM HR_Attendances a
                WHERE a.EmployeeID = d.EmployeeID
                  AND CAST(a.CheckInTime AS DATE) = CAST(d.GioVao AS DATE)
            );

            ;WITH NhanVienNghi AS
            (
                SELECT TOP (24)
                       e.EmployeeID,
                       ISNULL(e.ManagerID, @GiamDoc) AS ApproverID,
                       ROW_NUMBER() OVER (ORDER BY e.EmployeeID) AS ThuTu
                FROM HR_Employees e
                WHERE e.IsActive = 1
                  AND e.EmployeeCode <> 'GD001'
                ORDER BY e.EmployeeID
            ),
            DonNghi AS
            (
                SELECT EmployeeID,
                       ApproverID,
                       CASE ThuTu % 4
                           WHEN 0 THEN N'Nghỉ phép năm'
                           WHEN 1 THEN N'Nghỉ việc riêng'
                           WHEN 2 THEN N'Nghỉ bệnh'
                           ELSE N'Nghỉ không lương'
                       END AS LeaveType,
                       DATEADD(DAY, (ThuTu % 18) - 6, @HomNay) AS StartDate,
                       DATEADD(DAY, (ThuTu % 18) - 6 + CASE WHEN ThuTu % 3 = 0 THEN 1 ELSE 0 END, @HomNay) AS EndDate,
                       CAST(CASE WHEN ThuTu % 3 = 0 THEN 2 ELSE 1 END AS DECIMAL(10,2)) AS TotalDays,
                       CASE ThuTu % 5
                           WHEN 0 THEN N'Từ chối'
                           WHEN 1 THEN N'Chờ duyệt'
                           ELSE N'Đã duyệt'
                       END AS Status,
                       N'Dữ liệu mẫu phục vụ demo nghiệp vụ nghỉ phép.' AS Reason,
                       CASE ThuTu % 5
                           WHEN 0 THEN N'Từ chối do trùng lịch vận hành.'
                           WHEN 1 THEN N''
                           ELSE N'Đồng ý theo kế hoạch nhân sự.'
                       END AS ApprovalNote
                FROM NhanVienNghi
            )
            INSERT INTO HR_LeaveRequests(EmployeeID, LeaveType, StartDate, EndDate, TotalDays, Status, ApproverID, Reason, ApprovalNote)
            SELECT EmployeeID, LeaveType, StartDate, EndDate, TotalDays, Status, ApproverID, Reason, ApprovalNote
            FROM DonNghi d
            WHERE NOT EXISTS (
                SELECT 1
                FROM HR_LeaveRequests l
                WHERE l.EmployeeID = d.EmployeeID
                  AND l.StartDate = d.StartDate
                  AND l.LeaveType = d.LeaveType
            );

            DECLARE @BangLuong TABLE
            (
                EmployeeID INT NOT NULL PRIMARY KEY,
                BasicSalary DECIMAL(18,2) NOT NULL,
                WorkDays DECIMAL(10,2) NOT NULL,
                TotalAllowances DECIMAL(18,2) NOT NULL,
                TotalDeductions DECIMAL(18,2) NOT NULL,
                NetSalary DECIMAL(18,2) NOT NULL
            );

            INSERT INTO @BangLuong(EmployeeID, BasicSalary, WorkDays, TotalAllowances, TotalDeductions, NetSalary)
            SELECT x.EmployeeID,
                   x.BasicSalary,
                   x.WorkDays,
                   x.TotalAllowances,
                   x.TotalDeductions,
                   CASE
                       WHEN ROUND(x.BasicSalary / 22 * x.WorkDays, 0) + x.TotalAllowances - x.TotalDeductions < 0 THEN 0
                       ELSE ROUND(x.BasicSalary / 22 * x.WorkDays, 0) + x.TotalAllowances - x.TotalDeductions
                   END AS NetSalary
            FROM
            (
                SELECT e.EmployeeID,
                       ISNULL(c.BasicSalary, ISNULL(p.ExpectedSalary, 10000000)) AS BasicSalary,
                       CASE
                           WHEN ISNULL(cc.TongGio, 0) <= 0 THEN 22
                           WHEN ROUND(ISNULL(cc.TongGio, 0) / 8, 2) > 22 THEN 22
                           ELSE ROUND(ISNULL(cc.TongGio, 0) / 8, 2)
                       END AS WorkDays,
                       ROUND(ISNULL(c.BasicSalary, ISNULL(p.ExpectedSalary, 10000000)) * (0.05 + bh.SoNamTinhPhuCap * 0.01), 0) AS TotalAllowances,
                       ROUND(ISNULL(c.BasicSalary, ISNULL(p.ExpectedSalary, 10000000)) / 22 * ISNULL(np.SoNgayNghi, 0), 0)
                           + ROUND(ISNULL(c.BasicSalary, ISNULL(p.ExpectedSalary, 10000000)) * 0.105, 0) AS TotalDeductions
                FROM HR_Employees e
                JOIN HR_JobPositions p ON p.PositionID = e.PositionID
                OUTER APPLY
                (
                    SELECT TOP 1 BasicSalary
                    FROM HR_Contracts
                    WHERE EmployeeID = e.EmployeeID
                    ORDER BY StartDate DESC, ContractID DESC
                ) c
                OUTER APPLY
                (
                    SELECT SUM(
                        CASE
                            WHEN WorkHours IS NOT NULL AND WorkHours > 0 THEN WorkHours
                            WHEN CheckOutTime IS NOT NULL THEN CAST(DATEDIFF(MINUTE, CheckInTime, CheckOutTime) AS DECIMAL(10,2)) / 60
                            ELSE 0
                        END) AS TongGio
                    FROM HR_Attendances
                    WHERE EmployeeID = e.EmployeeID
                      AND CheckInTime >= @DauThang
                      AND CheckInTime < DATEADD(DAY, 1, @CuoiThang)
                ) cc
                OUTER APPLY
                (
                    SELECT SUM(DATEDIFF(DAY,
                        CASE WHEN StartDate < @DauThang THEN @DauThang ELSE StartDate END,
                        DATEADD(DAY, 1, CASE WHEN EndDate > @CuoiThang THEN @CuoiThang ELSE EndDate END))) AS SoNgayNghi
                    FROM HR_LeaveRequests
                    WHERE EmployeeID = e.EmployeeID
                      AND Status IN (N'Đã duyệt', 'Approved')
                      AND StartDate <= @CuoiThang
                      AND EndDate >= @DauThang
                ) np
                CROSS APPLY
                (
                    SELECT CASE
                        WHEN ISNULL(e.SocialInsuranceStartDate, e.JoinDate) > @HomNay THEN 0
                        ELSE DATEDIFF(YEAR, ISNULL(e.SocialInsuranceStartDate, e.JoinDate), @HomNay)
                            - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, ISNULL(e.SocialInsuranceStartDate, e.JoinDate), @HomNay), ISNULL(e.SocialInsuranceStartDate, e.JoinDate)) > @HomNay THEN 1 ELSE 0 END
                    END AS SoNamBaoHiemDayDu
                ) sn
                CROSS APPLY
                (
                    SELECT CASE
                        WHEN sn.SoNamBaoHiemDayDu < 0 THEN 0
                        WHEN sn.SoNamBaoHiemDayDu > 5 THEN 5
                        ELSE sn.SoNamBaoHiemDayDu
                    END AS SoNamTinhPhuCap
                ) bh
                WHERE e.IsActive = 1
            ) x;

            UPDATE p
            SET BasicSalary = b.BasicSalary,
                WorkDays = b.WorkDays,
                TotalAllowances = b.TotalAllowances,
                TotalDeductions = b.TotalDeductions,
                NetSalary = b.NetSalary
            FROM HR_Payslips p
            JOIN @BangLuong b ON b.EmployeeID = p.EmployeeID
            WHERE p.PayPeriod = @KyLuong;

            INSERT INTO HR_Payslips(EmployeeID, PayPeriod, BasicSalary, WorkDays, TotalAllowances, TotalDeductions, NetSalary, Status)
            SELECT b.EmployeeID,
                   @KyLuong,
                   b.BasicSalary,
                   b.WorkDays,
                   b.TotalAllowances,
                   b.TotalDeductions,
                   b.NetSalary,
                   CASE WHEN b.EmployeeID % 7 = 0 THEN N'Nháp' ELSE N'Đã trả' END
            FROM @BangLuong b
            WHERE NOT EXISTS (SELECT 1 FROM HR_Payslips p WHERE p.EmployeeID = b.EmployeeID AND p.PayPeriod = @KyLuong);

            ;WITH DanhGiaNguon AS
            (
                SELECT e.EmployeeID,
                       COALESCE(e.ManagerID, @GiamDoc, e.EmployeeID) AS ReviewerID,
                       75 + (e.EmployeeID % 24) AS Score
                FROM HR_Employees e
                WHERE e.IsActive = 1
            )
            INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
            SELECT EmployeeID,
                   ReviewerID,
                   '2026-Q2',
                   Score,
                   CASE
                       WHEN Score >= 90 THEN N'Hiệu suất nổi bật, chủ động hỗ trợ đội nhóm.'
                       WHEN Score >= 82 THEN N'Hoàn thành tốt mục tiêu công việc.'
                       ELSE N'Cần tiếp tục cải thiện kỹ năng và tiến độ.'
                   END,
                   N'Hoàn tất'
            FROM DanhGiaNguon d
            WHERE NOT EXISTS (
                SELECT 1
                FROM HR_Appraisals a
                WHERE a.EmployeeID = d.EmployeeID
                  AND a.ReviewPeriod = '2026-Q2'
            );

            DECLARE @UngVien TABLE(FullName NVARCHAR(150), PositionName NVARCHAR(150), Email NVARCHAR(150), Phone NVARCHAR(30), Stage NVARCHAR(80));
            INSERT INTO @UngVien VALUES
                (N'Kiều Mỹ Vy', N'Nhân viên nhân sự', N'vy.kieu@example.com', N'0901000001', N'Mới'),
                (N'Hà Minh Sơn', N'Chuyên viên pháp chế', N'son.ham@example.com', N'0901000002', N'Sàng lọc hồ sơ'),
                (N'Lê Quang Hòa', N'Nhân viên hành chính', N'hoa.le@example.com', N'0901000003', N'Phỏng vấn'),
                (N'Trần Hải Yến', N'Nhân viên kinh doanh', N'yen.tran@example.com', N'0901000004', N'Đề nghị nhận việc'),
                (N'Phạm Minh Khang', N'Nhân viên kế hoạch sản xuất', N'khang.pham@example.com', N'0901000005', N'Mới'),
                (N'Nguyễn Thùy Dung', N'Nhân viên nhân sự', N'dung.nguyen@example.com', N'0901000006', N'Phỏng vấn');

            INSERT INTO HR_Applicants(PositionID, FullName, Email, Phone, CVFile_Url, Stage)
            SELECT p.PositionID, u.FullName, u.Email, u.Phone, NULL, u.Stage
            FROM @UngVien u
            JOIN HR_JobPositions p ON p.Name = u.PositionName
            WHERE NOT EXISTS (SELECT 1 FROM HR_Applicants a WHERE a.Email = u.Email)
              AND NOT EXISTS (SELECT 1 FROM HR_Employees e WHERE LTRIM(RTRIM(e.FullName)) = LTRIM(RTRIM(u.FullName)));

            IF NOT EXISTS (SELECT 1 FROM HR_AuditLogs WHERE ActionName = N'SeedDemoData')
            BEGIN
                INSERT INTO HR_AuditLogs(ActorUsername, ActionName, EntityName, EntityKey, Detail, MachineName)
                VALUES(N'admin', N'SeedDemoData', N'HRManagementDB', N'demo', N'Bổ sung dữ liệu mẫu cho chấm công, nghỉ phép, tuyển dụng, lương và đánh giá.', HOST_NAME());
            END;
            """);
    }

    private static async Task DamBaoDanhGiaMauSqlAsync(SqlConnection ketNoi)
    {
        await ThucThiTrenKetNoiAsync(ketNoi, """
            IF NOT EXISTS (SELECT 1 FROM HR_Appraisals)
            BEGIN
                DECLARE @GiamDoc INT = (SELECT EmployeeID FROM HR_Employees WHERE EmployeeCode = 'GD001');
                DECLARE @TruongPhongNhanSu INT = (SELECT EmployeeID FROM HR_Employees WHERE EmployeeCode = 'TP003');
                DECLARE @NhanVienKinhDoanh INT = (SELECT EmployeeID FROM HR_Employees WHERE EmployeeCode = 'NV001');
                DECLARE @NhanVienPhapChe INT = (SELECT EmployeeID FROM HR_Employees WHERE EmployeeCode = 'NV005');

                IF @GiamDoc IS NOT NULL
                    INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                    VALUES(@GiamDoc, @GiamDoc, '2026-Q1', 95, N'Điều hành xuất sắc, hoàn thành mục tiêu chiến lược.', N'Hoàn tất');

                IF @TruongPhongNhanSu IS NOT NULL AND @GiamDoc IS NOT NULL
                    INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                    VALUES(@TruongPhongNhanSu, @GiamDoc, '2026-Q1', 92, N'Vận hành nhân sự hiệu quả.', N'Hoàn tất');

                IF @NhanVienKinhDoanh IS NOT NULL AND @GiamDoc IS NOT NULL
                    INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                    VALUES(@NhanVienKinhDoanh, @GiamDoc, '2026-Q1', 94, N'Kết quả kinh doanh nổi bật.', N'Hoàn tất');

                IF @NhanVienPhapChe IS NOT NULL AND @GiamDoc IS NOT NULL
                    INSERT INTO HR_Appraisals(EmployeeID, ReviewerID, ReviewPeriod, Score, Feedback, Status)
                    VALUES(@NhanVienPhapChe, @GiamDoc, '2026-Q1', 90, N'Kiểm soát hồ sơ tốt.', N'Hoàn tất');
            END;
            """);
    }

    private static async Task<int> LayHoacThemPhongBanAsync(SqlConnection ketNoi, string tenPhongBan)
    {
        var maPhongBan = await LaySoNguyenAsync(ketNoi, "SELECT DepartmentID FROM HR_Departments WHERE Name=@TenPhongBan", ("@TenPhongBan", tenPhongBan));
        if (maPhongBan.HasValue)
        {
            return maPhongBan.Value;
        }

        return await ThemVaLayIdAsync(ketNoi,
            "INSERT INTO HR_Departments(Name, ManagerID) VALUES(@TenPhongBan, NULL); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ("@TenPhongBan", tenPhongBan));
    }

    private static async Task<int> LayHoacThemViTriAsync(SqlConnection ketNoi, int maPhongBan, ViTriKhoiTao viTri)
    {
        var maViTri = await LaySoNguyenAsync(ketNoi, "SELECT PositionID FROM HR_JobPositions WHERE DepartmentID=@MaPhongBan AND Name=@TenViTri", ("@MaPhongBan", maPhongBan), ("@TenViTri", viTri.TenViTri));
        if (maViTri.HasValue)
        {
            await ThucThiTrenKetNoiAsync(ketNoi,
                "UPDATE HR_JobPositions SET ExpectedSalary=@LuongDuKien, Status=N'Đang tuyển' WHERE PositionID=@MaViTri",
                ("@LuongDuKien", viTri.LuongDuKien),
                ("@MaViTri", maViTri.Value));
            return maViTri.Value;
        }

        return await ThemVaLayIdAsync(ketNoi, """
            INSERT INTO HR_JobPositions(DepartmentID, Name, ExpectedSalary, Status)
            VALUES(@MaPhongBan, @TenViTri, @LuongDuKien, N'Đang tuyển');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            ("@MaPhongBan", maPhongBan),
            ("@TenViTri", viTri.TenViTri),
            ("@LuongDuKien", viTri.LuongDuKien));
    }

    private static async Task<int> LayHoacThemNhanVienAsync(SqlConnection ketNoi, NhanSuKhoiTao dong, int maPhongBan, int maViTri, int thuTu)
    {
        var maNhanVien = await LaySoNguyenAsync(ketNoi, "SELECT EmployeeID FROM HR_Employees WHERE EmployeeCode=@MaSo", ("@MaSo", dong.MaSo));
        var lienHe = $"09{thuTu:00000000}";
        var taiKhoan = $"9704{thuTu:000000000}";
        var canCuoc = $"0792{thuTu:00000000}";

        if (maNhanVien.HasValue)
        {
            await ThucThiTrenKetNoiAsync(ketNoi, """
                UPDATE HR_Employees
                SET FullName=@HoTen, DepartmentID=@MaPhongBan, PositionID=@MaViTri, BirthDate=@NgaySinh, SocialInsuranceStartDate=@NgayThamGiaBaoHiemXaHoi, JoinDate=@NgayVaoLam, IsActive=1,
                    EmergencyContact=@LienHe, BankAccount=@TaiKhoan, IdentityNumber=@CanCuoc
                WHERE EmployeeID=@MaNhanVien
                """,
                ("@HoTen", dong.HoTen),
                ("@MaPhongBan", maPhongBan),
                ("@MaViTri", maViTri),
                ("@NgaySinh", dong.NgaySinh),
                ("@NgayThamGiaBaoHiemXaHoi", dong.NgayThamGiaBaoHiemXaHoi),
                ("@NgayVaoLam", dong.NgayVaoLam),
                ("@LienHe", lienHe),
                ("@TaiKhoan", taiKhoan),
                ("@CanCuoc", canCuoc),
                ("@MaNhanVien", maNhanVien.Value));
            return maNhanVien.Value;
        }

        return await ThemVaLayIdAsync(ketNoi, """
            INSERT INTO HR_Employees(EmployeeCode, ApplicantID, FullName, DepartmentID, PositionID, ManagerID, BirthDate, SocialInsuranceStartDate, JoinDate, IsActive, EmergencyContact, BankAccount, IdentityNumber)
            VALUES(@MaSo, NULL, @HoTen, @MaPhongBan, @MaViTri, NULL, @NgaySinh, @NgayThamGiaBaoHiemXaHoi, @NgayVaoLam, 1, @LienHe, @TaiKhoan, @CanCuoc);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            ("@MaSo", dong.MaSo),
            ("@HoTen", dong.HoTen),
            ("@MaPhongBan", maPhongBan),
            ("@MaViTri", maViTri),
            ("@NgaySinh", dong.NgaySinh),
            ("@NgayThamGiaBaoHiemXaHoi", dong.NgayThamGiaBaoHiemXaHoi),
            ("@NgayVaoLam", dong.NgayVaoLam),
            ("@LienHe", lienHe),
            ("@TaiKhoan", taiKhoan),
            ("@CanCuoc", canCuoc));
    }

    private static async Task<int?> LaySoNguyenAsync(SqlConnection ketNoi, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        var giaTri = await ThucThiScalarAsync(ketNoi, sql, thamSo);
        return giaTri is null or DBNull ? null : Convert.ToInt32(giaTri);
    }

    private static async Task<int> ThemVaLayIdAsync(SqlConnection ketNoi, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        var giaTri = await ThucThiScalarAsync(ketNoi, sql, thamSo);
        return Convert.ToInt32(giaTri);
    }

    private static async Task<object?> ThucThiScalarAsync(SqlConnection ketNoi, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        await using var lenh = new SqlCommand(sql, ketNoi);
        GanThamSo(lenh, thamSo);
        return await lenh.ExecuteScalarAsync();
    }

    private static async Task ThucThiTrenKetNoiAsync(SqlConnection ketNoi, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        await using var lenh = new SqlCommand(sql, ketNoi);
        GanThamSo(lenh, thamSo);
        await lenh.ExecuteNonQueryAsync();
    }

    private static void GanThamSo(SqlCommand lenh, params (string Ten, object? GiaTri)[] thamSo)
    {
        foreach (var (ten, giaTri) in thamSo)
        {
            lenh.Parameters.AddWithValue(ten, giaTri ?? DBNull.Value);
        }
    }

    private static string ToiUuChuoiKetNoi(string chuoiKetNoi)
    {
        try
        {
            var boTao = new SqlConnectionStringBuilder(chuoiKetNoi)
            {
                ConnectTimeout = 1,
                TrustServerCertificate = true
            };
            boTao["Encrypt"] = false;
            return boTao.ConnectionString;
        }
        catch
        {
            var chuoi = chuoiKetNoi.Trim().TrimEnd(';');
            return $"{chuoi};Connect Timeout=1;Encrypt=False;TrustServerCertificate=True;";
        }
    }

    private static string LayTenMayChu(string chuoiKetNoi)
    {
        try
        {
            return new SqlConnectionStringBuilder(chuoiKetNoi).DataSource;
        }
        catch
        {
            return "không rõ nguồn";
        }
    }

    private static void GanThamSoNhanVien(SqlCommand lenh, NhanVien nhanVien)
    {
        lenh.Parameters.AddWithValue("@MaNhanVien", nhanVien.MaNhanVien);
        lenh.Parameters.AddWithValue("@MaSo", nhanVien.MaSo);
        lenh.Parameters.AddWithValue("@HoTen", nhanVien.HoTen);
        lenh.Parameters.AddWithValue("@MaPhongBan", nhanVien.MaPhongBan);
        lenh.Parameters.AddWithValue("@MaViTri", nhanVien.MaViTri);
        lenh.Parameters.AddWithValue("@NgaySinh", nhanVien.NgaySinh);
        lenh.Parameters.AddWithValue("@NgayThamGiaBaoHiemXaHoi", nhanVien.NgayThamGiaBaoHiemXaHoi);
        lenh.Parameters.AddWithValue("@NgayVaoLam", nhanVien.NgayVaoLam);
        lenh.Parameters.AddWithValue("@DangLamViec", nhanVien.DangLamViec);
        lenh.Parameters.AddWithValue("@LienHeKhanCap", string.IsNullOrWhiteSpace(nhanVien.LienHeKhanCap) ? DBNull.Value : nhanVien.LienHeKhanCap);
        lenh.Parameters.AddWithValue("@TaiKhoanNganHang", string.IsNullOrWhiteSpace(nhanVien.TaiKhoanNganHang) ? DBNull.Value : nhanVien.TaiKhoanNganHang);
        lenh.Parameters.AddWithValue("@SoCanCuoc", string.IsNullOrWhiteSpace(nhanVien.SoCanCuoc) ? DBNull.Value : nhanVien.SoCanCuoc);
    }

    private async Task<KhoDuLieuUngDung> TaiTuSqlServerAsync()
    {
        var duLieu = new KhoDuLieuUngDung();
        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync();
        await DamBaoCauTrucVaDuLieuSqlAsync(ketNoi);
        await DocBang(ketNoi, duLieu);
        return duLieu;
    }

    private static async Task DocBang(SqlConnection ketNoi, KhoDuLieuUngDung duLieu)
    {
        await Doc(ketNoi, """
            SELECT d.DepartmentID,d.Name,ISNULL(e.FullName,N'Chưa phân công')
            FROM HR_Departments d
            LEFT JOIN HR_Employees e ON d.ManagerID=e.EmployeeID
            ORDER BY CASE d.Name
                WHEN N'Ban Giám đốc' THEN 1
                WHEN N'Phòng Nhân sự' THEN 2
                WHEN N'Phòng Kế toán' THEN 3
                WHEN N'Phòng Kinh doanh' THEN 4
                WHEN N'Phòng Marketing' THEN 5
                WHEN N'Phòng CNTT' THEN 6
                WHEN N'Phòng Hành chính' THEN 7
                WHEN N'Phòng Vận hành' THEN 8
                WHEN N'Phòng Chăm sóc khách hàng' THEN 9
                WHEN N'Phòng Pháp chế' THEN 10
                WHEN N'Phòng Sản xuất' THEN 11
                ELSE 99
            END, d.Name
            """,
            r => duLieu.PhongBan.Add(new PhongBan(r.GetInt32(0), r.GetString(1), r.GetString(2))));
        await Doc(ketNoi, "SELECT PositionID,DepartmentID,Name,ISNULL(ExpectedSalary,0),Status FROM HR_JobPositions ORDER BY PositionID",
            r => duLieu.ViTri.Add(new ViTriCongViec(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetDecimal(3), r.GetString(4))));
        await Doc(ketNoi, """
            SELECT e.EmployeeID,e.EmployeeCode,e.FullName,e.DepartmentID,e.PositionID,d.Name,p.Name,ISNULL(e.BirthDate, CAST('1995-01-01' AS DATE)),e.JoinDate,e.IsActive,
                   ISNULL(e.EmergencyContact,N''),ISNULL(e.BankAccount,N''),ISNULL(e.IdentityNumber,N''),ISNULL(e.SocialInsuranceStartDate, e.JoinDate)
            FROM HR_Employees e JOIN HR_Departments d ON e.DepartmentID=d.DepartmentID JOIN HR_JobPositions p ON e.PositionID=p.PositionID ORDER BY e.EmployeeID
            """, r => duLieu.NhanVien.Add(new NhanVien
        {
            MaNhanVien = r.GetInt32(0), MaSo = r.GetString(1), HoTen = r.GetString(2), MaPhongBan = r.GetInt32(3), MaViTri = r.GetInt32(4),
            PhongBan = r.GetString(5), ViTri = r.GetString(6), NgaySinh = r.GetDateTime(7), NgayVaoLam = r.GetDateTime(8), DangLamViec = r.GetBoolean(9),
            LienHeKhanCap = r.GetString(10), TaiKhoanNganHang = r.GetString(11), SoCanCuoc = r.GetString(12), NgayThamGiaBaoHiemXaHoi = r.GetDateTime(13)
        }));
        await Doc(ketNoi, """
            SELECT e.FullName,l.LeaveType,l.StartDate,l.EndDate,l.TotalDays,l.Status,l.LeaveID,ISNULL(l.ApprovalNote,N'')
            FROM HR_LeaveRequests l
            JOIN HR_Employees e ON l.EmployeeID=e.EmployeeID
            ORDER BY
                CASE WHEN l.Status IN (N'Chờ duyệt', N'Pending') THEN 0 ELSE 1 END,
                l.LeaveID DESC,
                l.StartDate DESC,
                l.EndDate DESC
            """,
            r => duLieu.NghiPhep.Add(new NghiPhep(r.GetString(0), r.GetString(1), r.GetDateTime(2), r.GetDateTime(3), r.GetDecimal(4), DichTrangThai(r.GetString(5)), r.GetInt32(6), r.GetString(7))));
        await Doc(ketNoi, "SELECT e.FullName,a.CheckInTime,a.CheckOutTime,ISNULL(a.WorkHours,0) FROM HR_Attendances a JOIN HR_Employees e ON a.EmployeeID=e.EmployeeID",
            r => duLieu.ChamCong.Add(new ChamCong(r.GetString(0), r.GetDateTime(1), r.IsDBNull(2) ? null : r.GetDateTime(2), r.GetDecimal(3))));
        await Doc(ketNoi, "SELECT e.FullName,r.FullName,a.ReviewPeriod,ISNULL(a.Score,0),ISNULL(a.Feedback,N''),a.Status FROM HR_Appraisals a JOIN HR_Employees e ON a.EmployeeID=e.EmployeeID JOIN HR_Employees r ON a.ReviewerID=r.EmployeeID",
            r => duLieu.DanhGia.Add(new DanhGia(r.GetString(0), r.GetString(1), r.GetString(2), r.GetDecimal(3), r.GetString(4), DichTrangThai(r.GetString(5)))));
        await Doc(ketNoi, "SELECT e.FullName,p.PayPeriod,p.BasicSalary,p.TotalAllowances,p.TotalDeductions,p.NetSalary,p.Status FROM HR_Payslips p JOIN HR_Employees e ON p.EmployeeID=e.EmployeeID ORDER BY p.PayPeriod DESC, p.PayslipID DESC",
            r => duLieu.PhieuLuong.Add(new PhieuLuong(r.GetString(0), r.GetString(1), r.GetDecimal(2), r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5), DichTrangThai(r.GetString(6)))));
        await Doc(ketNoi, """
            SELECT a.FullName,p.Name,a.Email,ISNULL(a.Phone,''),a.Stage
            FROM HR_Applicants a
            JOIN HR_JobPositions p ON a.PositionID=p.PositionID
            WHERE a.Stage NOT IN (N'Đã tiếp nhận', N'Đã ký', N'Signed')
              AND NOT EXISTS (
                  SELECT 1
                  FROM HR_Employees e
                  WHERE e.IsActive = 1
                    AND LTRIM(RTRIM(e.FullName)) = LTRIM(RTRIM(a.FullName))
              )
            ORDER BY a.ApplicantID
            """,
            r => duLieu.UngVien.Add(new UngVien(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3), DichTrangThai(r.GetString(4)))));
        TaoThongBaoTuDuLieu(duLieu);
    }

    private static async Task Doc(SqlConnection ketNoi, string sql, Action<SqlDataReader> napDong)
    {
        await using var lenh = new SqlCommand(sql, ketNoi);
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync()) napDong(doc);
    }

    private static string DichTrangThai(string trangThai) => trangThai switch
    {
        "Open" => "Đang tuyển",
        "Closed" => "Đã đóng",
        "New" => "Mới",
        "Screening" => "Sàng lọc hồ sơ",
        "Interview" => "Phỏng vấn",
        "Offer" => "Đề nghị nhận việc",
        "Signed" => "Đã tiếp nhận",
        "Approved" => "Đã duyệt",
        "Pending" or "Waiting" => "Chờ duyệt",
        "Rejected" => "Từ chối",
        "Completed" => "Hoàn tất",
        "Draft" => "Nháp",
        "Paid" => "Đã trả",
        "Active" or "Running" => "Đang hiệu lực",
        _ => trangThai
    };

    private static string LayMoTaQuyen(string vaiTro) => vaiTro switch
    {
        "Admin" => "Toàn quyền: tài khoản, dữ liệu, tuyển dụng, hồ sơ, chấm công, nghỉ phép, lương và báo cáo nhân sự.",
        "Giám đốc" => "Điều hành nghiệp vụ nhân sự: phòng ban, tuyển dụng, chấm công, nghỉ phép, lương, đánh giá và báo cáo.",
        "Trưởng phòng" => "Quản lý đội nhóm: hồ sơ nhân viên, tuyển dụng, chấm công, nghỉ phép, đánh giá và báo cáo.",
        "Nhân viên" => "Tự phục vụ: chấm công, nghỉ phép, xem thông báo và đánh giá liên quan.",
        _ => "Chưa phân quyền."
    };

    public static KhoDuLieuUngDung TaoDuLieuMau()
    {
        var duLieu = BoDuLieuKhoiTao.TaoDuLieuUngDung();
        var giamDoc = duLieu.NhanVien.First(x => x.MaSo == "GD001");
        var truongPhongNhanSu = duLieu.NhanVien.First(x => x.MaSo == "TP003");

        duLieu.UngVien.Add(new UngVien("Kiều Mỹ Vy", "Nhân viên nhân sự", "vy.kieu@example.com", "0901000001", "Mới"));
        duLieu.UngVien.Add(new UngVien("Hà Minh Sơn", "Chuyên viên pháp chế", "son.ham@example.com", "0901000002", "Sàng lọc hồ sơ"));
        duLieu.UngVien.Add(new UngVien("Lê Quang Hòa", "Nhân viên hành chính", "hoa.le@example.com", "0901000003", "Phỏng vấn"));
        duLieu.UngVien.Add(new UngVien("Trần Hải Yến", "Nhân viên kinh doanh", "yen.tran@example.com", "0901000004", "Đề nghị nhận việc"));
        duLieu.UngVien.Add(new UngVien("Phạm Minh Khang", "Nhân viên kế hoạch sản xuất", "khang.pham@example.com", "0901000005", "Mới"));
        duLieu.UngVien.Add(new UngVien("Nguyễn Thùy Dung", "Nhân viên nhân sự", "dung.nguyen@example.com", "0901000006", "Phỏng vấn"));

        var nhanVienDemo = duLieu.NhanVien.Where(nhanVien => nhanVien.DangLamViec).ToList();
        var dauThang = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        for (var i = 0; i < nhanVienDemo.Count; i++)
        {
            var nhanVien = nhanVienDemo[i];
            for (var ngay = 0; ngay < 22; ngay++)
            {
                var gioVao = dauThang.AddDays(ngay).AddHours(8).AddMinutes((i + ngay * 3) % 18);
                var soGio = 7.5m + ((i + ngay) % 4) * 0.25m;
                duLieu.ChamCong.Add(new ChamCong(nhanVien.HoTen, gioVao, gioVao.AddMinutes((double)(soGio * 60)), soGio));
            }
        }

        var loaiNghi = new[] { "Nghỉ phép năm", "Nghỉ việc riêng", "Nghỉ bệnh", "Nghỉ không lương" };
        var trangThaiNghi = new[] { "Chờ duyệt", "Đã duyệt", "Đã duyệt", "Từ chối" };
        for (var i = 0; i < Math.Min(24, nhanVienDemo.Count); i++)
        {
            var tuNgay = DateTime.Today.AddDays((i % 18) - 6);
            var soNgay = i % 3 == 0 ? 2 : 1;
            duLieu.NghiPhep.Add(new NghiPhep(
                nhanVienDemo[i].HoTen,
                loaiNghi[i % loaiNghi.Length],
                tuNgay,
                tuNgay.AddDays(soNgay - 1),
                soNgay,
                trangThaiNghi[i % trangThaiNghi.Length],
                i + 1));
        }

        var danhSachDanhGia = duLieu.NhanVien.Take(60).ToList();
        for (var i = 0; i < danhSachDanhGia.Count; i++)
        {
            var nhanVien = danhSachDanhGia[i];
            var nguoiDanhGia = nhanVien.MaSo.StartsWith("TP", StringComparison.OrdinalIgnoreCase) ? giamDoc : truongPhongNhanSu;
            var diem = 75 + (i % 24);
            var nhanXet = diem >= 90
                ? "Hiệu suất nổi bật, chủ động hỗ trợ đội nhóm."
                : diem >= 82
                    ? "Hoàn thành tốt mục tiêu công việc."
                    : "Cần tiếp tục cải thiện kỹ năng và tiến độ.";
            duLieu.DanhGia.Add(new DanhGia(nhanVien.HoTen, nguoiDanhGia.HoTen, "2026-Q2", diem, nhanXet, "Hoàn tất"));
        }

        TaoThongBaoTuDuLieu(duLieu);
        return duLieu;
    }

    private static void TaoThongBaoTuDuLieu(KhoDuLieuUngDung duLieu)
    {
        duLieu.ThongBao.Clear();

        var donNghiChoDuyet = duLieu.NghiPhep
            .Where(x => x.TrangThai.Contains("Chờ", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.ThuTuMoiNhat)
            .ThenByDescending(x => x.TuNgay)
            .ToList();
        var soDonNghiChoDuyet = donNghiChoDuyet.Count;
        if (soDonNghiChoDuyet > 0)
        {
            var donMoiNhat = donNghiChoDuyet.First();
            duLieu.ThongBao.Add(new ThongBaoHeThong(
                "Có đơn nghỉ phép chờ duyệt",
                $"{soDonNghiChoDuyet} đơn nghỉ phép cần duyệt. Gần nhất: {donMoiNhat.NhanVien} nghỉ {donMoiNhat.LoaiNghi} từ {donMoiNhat.TuNgay:dd/MM/yyyy}.",
                "Nghỉ phép",
                DateTime.Now.AddMinutes(-18),
                "Cảnh báo"));
        }

        var ungVienMoi = duLieu.UngVien.FirstOrDefault();
        if (ungVienMoi is not null)
        {
            duLieu.ThongBao.Add(new ThongBaoHeThong(
                "Tuyển dụng có cập nhật",
                $"{ungVienMoi.HoTen} đang ở giai đoạn {ungVienMoi.GiaiDoan}.",
                "Tuyển dụng",
                DateTime.Now.AddHours(-1),
                "Thông tin"));
        }

        var phieuLuongNhan = duLieu.PhieuLuong
            .OrderByDescending(x => x.KyLuong)
            .ThenByDescending(x => x.ThucLanh)
            .FirstOrDefault();
        if (phieuLuongNhan is not null)
        {
            duLieu.ThongBao.Add(new ThongBaoHeThong(
                "Bảng lương đã sẵn sàng",
                $"Kỳ lương {phieuLuongNhan.KyLuong} có dữ liệu để kiểm tra.",
                "Bảng lương",
                DateTime.Now.AddHours(-2),
                "Thành công"));
        }
    }
}
