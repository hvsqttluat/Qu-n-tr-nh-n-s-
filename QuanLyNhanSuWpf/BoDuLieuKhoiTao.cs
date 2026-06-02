namespace QuanLyNhanSuWpf;

public record PhongBanKhoiTao(string TenPhongBan, string MaSoTruongPhong);
public record ViTriKhoiTao(string TenPhongBan, string TenViTri, decimal LuongDuKien);
public record NhanSuKhoiTao(string MaSo, string HoTen, string TenPhongBan, string TenViTri, string? MaSoQuanLy, decimal LuongCoBan, DateTime NgaySinh, DateTime NgayVaoLam, DateTime NgayThamGiaBaoHiemXaHoi);

public static class BoDuLieuKhoiTao
{
    private static readonly string[] Ho =
    [
        "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Phan", "Mai", "Tạ"
    ];

    private static readonly string[] TenDem =
    [
        "Văn", "Thị", "Minh", "Quốc", "Thanh", "Gia", "Ngọc", "Tuấn", "Hải", "Thu", "Anh", "Đức"
    ];

    private static readonly string[] Ten =
    [
        "An", "Bình", "Cường", "Dũng", "Hà", "Huy", "Khánh", "Lan", "Linh", "Mai", "Nam", "Phương"
    ];

    public static IReadOnlyList<PhongBanKhoiTao> PhongBan { get; } =
    [
        new("Ban Giám đốc", "GD001"),
        new("Phòng Kinh doanh", "TP001"),
        new("Phòng Sản xuất", "TP002"),
        new("Phòng Nhân sự", "TP003"),
        new("Phòng Hành chính", "TP004"),
        new("Phòng Pháp chế", "TP005")
    ];

    public static IReadOnlyList<ViTriKhoiTao> ViTri { get; } =
    [
        new("Ban Giám đốc", "Giám đốc điều hành", 55_000_000),
        new("Phòng Kinh doanh", "Trưởng phòng Kinh doanh", 28_000_000),
        new("Phòng Sản xuất", "Trưởng phòng Sản xuất", 30_000_000),
        new("Phòng Nhân sự", "Trưởng phòng Nhân sự", 26_000_000),
        new("Phòng Hành chính", "Trưởng phòng Hành chính", 24_000_000),
        new("Phòng Pháp chế", "Trưởng phòng Pháp chế", 29_000_000),
        new("Phòng Kinh doanh", "Nhân viên kinh doanh", 14_000_000),
        new("Phòng Sản xuất", "Nhân viên kế hoạch sản xuất", 13_500_000),
        new("Phòng Nhân sự", "Nhân viên nhân sự", 13_000_000),
        new("Phòng Hành chính", "Nhân viên hành chính", 12_000_000),
        new("Phòng Pháp chế", "Chuyên viên pháp chế", 15_000_000),
        new("Phòng Sản xuất", "Công nhân sản xuất", 8_500_000)
    ];

    public static List<NhanSuKhoiTao> TaoNhanSu()
    {
        var danhSach = new List<NhanSuKhoiTao>
        {
            new("GD001", "Nguyễn Minh Đức", "Ban Giám đốc", "Giám đốc điều hành", null, 55_000_000, new DateTime(1982, 3, 12), new DateTime(2024, 1, 2), new DateTime(2010, 4, 1)),
            new("TP001", "Trần Quốc Huy", "Phòng Kinh doanh", "Trưởng phòng Kinh doanh", "GD001", 28_000_000, new DateTime(1988, 7, 18), new DateTime(2024, 2, 1), new DateTime(2014, 6, 1)),
            new("TP002", "Phạm Văn Long", "Phòng Sản xuất", "Trưởng phòng Sản xuất", "GD001", 30_000_000, new DateTime(1986, 11, 5), new DateTime(2024, 2, 5), new DateTime(2013, 8, 1)),
            new("TP003", "Lê Thu Hà", "Phòng Nhân sự", "Trưởng phòng Nhân sự", "GD001", 26_000_000, new DateTime(1990, 4, 22), new DateTime(2024, 2, 10), new DateTime(2016, 3, 1)),
            new("TP004", "Đỗ Thị Mai", "Phòng Hành chính", "Trưởng phòng Hành chính", "GD001", 24_000_000, new DateTime(1989, 9, 14), new DateTime(2024, 2, 15), new DateTime(2015, 5, 1)),
            new("TP005", "Vũ Anh Tuấn", "Phòng Pháp chế", "Trưởng phòng Pháp chế", "GD001", 29_000_000, new DateTime(1987, 6, 2), new DateTime(2024, 2, 20), new DateTime(2014, 9, 1))
        };
        var tenDaDung = danhSach.Select(nhanSu => nhanSu.HoTen).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var phongBanNhanVien = new[]
        {
            ("Phòng Kinh doanh", "Nhân viên kinh doanh", "TP001", 14_000_000m),
            ("Phòng Sản xuất", "Nhân viên kế hoạch sản xuất", "TP002", 13_500_000m),
            ("Phòng Nhân sự", "Nhân viên nhân sự", "TP003", 13_000_000m),
            ("Phòng Hành chính", "Nhân viên hành chính", "TP004", 12_000_000m),
            ("Phòng Pháp chế", "Chuyên viên pháp chế", "TP005", 15_000_000m)
        };
        var tenNhanVienVanPhong = new[]
        {
            "Vũ Hải An", "Đặng Minh Châu", "Bùi Thu Trang", "Đỗ Quốc Bảo", "Phan Gia Huy",
            "Mai Ngọc Linh", "Tạ Minh Quân", "Nguyễn Hoài Nam", "Trần Thu Phương", "Lê Thanh Tùng",
            "Hoàng Minh Khánh", "Phạm Bảo Ngọc", "Vũ Thùy Dương", "Đặng Anh Khoa", "Bùi Gia Bảo",
            "Đỗ Hồng Nhung", "Phan Đức Mạnh", "Mai Phương Anh", "Tạ Quang Minh", "Nguyễn Hà My"
        };

        for (var i = 1; i <= 20; i++)
        {
            var cauHinh = phongBanNhanVien[(i - 1) % phongBanNhanVien.Length];
            var hoTen = tenNhanVienVanPhong[i - 1];
            tenDaDung.Add(hoTen);
            danhSach.Add(new NhanSuKhoiTao(
                $"NV{i:000}",
                hoTen,
                cauHinh.Item1,
                cauHinh.Item2,
                cauHinh.Item3,
                cauHinh.Item4,
                TaoNgaySinhNhanVien(i),
                new DateTime(2025, ((i - 1) % 12) + 1, ((i - 1) % 24) + 1),
                TaoNgayThamGiaBaoHiemNhanVien(i)));
        }

        for (var i = 1; i <= 200; i++)
        {
            danhSach.Add(new NhanSuKhoiTao(
                $"CN{i:000}",
                TaoHoTenCongNhan(i, tenDaDung),
                "Phòng Sản xuất",
                "Công nhân sản xuất",
                "TP002",
                8_500_000,
                TaoNgaySinhCongNhan(i),
                new DateTime(2025, ((i - 1) % 12) + 1, ((i - 1) % 24) + 1),
                TaoNgayThamGiaBaoHiemCongNhan(i)));
        }

        return danhSach;
    }

    public static KhoDuLieuUngDung TaoDuLieuUngDung()
    {
        var duLieu = new KhoDuLieuUngDung();
        var nhanSu = TaoNhanSu();
        var maPhongBan = PhongBan.Select((p, i) => new { p.TenPhongBan, Ma = i + 1 }).ToDictionary(x => x.TenPhongBan, x => x.Ma);
        var maViTri = ViTri.Select((v, i) => new { v.TenViTri, Ma = i + 1 }).ToDictionary(x => x.TenViTri, x => x.Ma);
        var tenTheoMaSo = nhanSu.ToDictionary(x => x.MaSo, x => x.HoTen);

        foreach (var phongBan in PhongBan)
        {
            duLieu.PhongBan.Add(new PhongBan(
                maPhongBan[phongBan.TenPhongBan],
                phongBan.TenPhongBan,
                tenTheoMaSo.GetValueOrDefault(phongBan.MaSoTruongPhong, "Chưa phân công")));
        }

        var i = 1;
        foreach (var viTri in ViTri)
        {
            duLieu.ViTri.Add(new ViTriCongViec(i++, maPhongBan[viTri.TenPhongBan], viTri.TenViTri, viTri.LuongDuKien, "Đang tuyển"));
        }

        i = 1;
        var kyLuong = DateTime.Today.ToString("yyyy-MM");
        foreach (var dong in nhanSu)
        {
            duLieu.NhanVien.Add(new NhanVien
            {
                MaNhanVien = i,
                MaSo = dong.MaSo,
                HoTen = dong.HoTen,
                MaPhongBan = maPhongBan[dong.TenPhongBan],
                MaViTri = maViTri[dong.TenViTri],
                PhongBan = dong.TenPhongBan,
                ViTri = dong.TenViTri,
                NgaySinh = dong.NgaySinh,
                NgayThamGiaBaoHiemXaHoi = dong.NgayThamGiaBaoHiemXaHoi,
                NgayVaoLam = dong.NgayVaoLam,
                DangLamViec = true,
                LienHeKhanCap = $"09{i:00000000}",
                TaiKhoanNganHang = $"9704{i:000000000}",
                SoCanCuoc = $"0792{i:00000000}"
            });

            var phuCap = QuyTacNghiepVuNhanSu.TinhPhuCapLuong(dong.LuongCoBan, QuyTacNghiepVuNhanSu.TinhSoNamTron(dong.NgayThamGiaBaoHiemXaHoi, DateTime.Today));
            var khauTru = QuyTacNghiepVuNhanSu.TinhKhauTruBaoHiem(dong.LuongCoBan);
            duLieu.PhieuLuong.Add(new PhieuLuong(
                dong.HoTen,
                kyLuong,
                dong.LuongCoBan,
                phuCap,
                khauTru,
                dong.LuongCoBan + phuCap - khauTru,
                "Đã trả"));

            i++;
        }

        return duLieu;
    }

    private static string TaoHoTen(int chiSo)
    {
        var ho = Ho[chiSo % Ho.Length];
        var dem = TenDem[(chiSo / Ho.Length) % TenDem.Length];
        var ten = Ten[(chiSo / (Ho.Length * TenDem.Length)) % Ten.Length];
        return $"{ho} {dem} {ten}";
    }

    private static string TaoHoTenKhongTrung(int chiSo, HashSet<string> tenDaDung)
    {
        var buocNhay = Ho.Length * TenDem.Length;
        var hoTen = TaoHoTen(chiSo);
        while (!tenDaDung.Add(hoTen))
        {
            chiSo += buocNhay;
            hoTen = TaoHoTen(chiSo);
        }

        return hoTen;
    }

    private static DateTime TaoNgaySinhNhanVien(int thuTu)
    {
        var nam = 1992 + (thuTu % 9);
        var thang = ((thuTu * 3) % 12) + 1;
        var ngay = ((thuTu * 5) % 24) + 1;
        return new DateTime(nam, thang, ngay);
    }

    private static DateTime TaoNgaySinhCongNhan(int thuTu)
    {
        var nam = 1985 + (thuTu % 18);
        var thang = ((thuTu * 2) % 12) + 1;
        var ngay = ((thuTu * 7) % 24) + 1;
        return new DateTime(nam, thang, ngay);
    }

    private static DateTime TaoNgayThamGiaBaoHiemNhanVien(int thuTu)
    {
        var nam = 2018 + (thuTu % 6);
        var thang = ((thuTu * 2) % 12) + 1;
        return new DateTime(nam, thang, 1);
    }

    private static DateTime TaoNgayThamGiaBaoHiemCongNhan(int thuTu)
    {
        var nam = 2019 + (thuTu % 5);
        var thang = ((thuTu * 3) % 12) + 1;
        return new DateTime(nam, thang, 1);
    }

    private static string TaoHoTenCongNhan(int thuTu, HashSet<string> tenDaDung)
    {
        var lanThu = thuTu;
        while (true)
        {
            var ho = Ho[(lanThu * 5) % Ho.Length];
            var dem = TenDem[((lanThu / Ho.Length) + lanThu * 7) % TenDem.Length];
            var ten = Ten[((lanThu / 24) + lanThu * 11) % Ten.Length];
            var hoTen = $"{ho} {dem} {ten}";
            if (tenDaDung.Add(hoTen))
            {
                return hoTen;
            }

            lanThu += Ho.Length + TenDem.Length + Ten.Length;
        }
    }
}
