namespace QuanLyNhanSuWpf;

public static class QuyTacNghiepVuNhanSu
{
    public const decimal SoNgayCongChuanThang = 22m;
    public const decimal SoGioMotNgayCong = 8m;
    public const decimal TyLeBaoHiemBatBuocNguoiLaoDong = 0.105m;
    public const decimal TyLePhuCapCoBan = 0.05m;
    public const decimal TyLePhuCapThamNienMoiNam = 0.01m;
    public const int SoNamThamNienTinhPhuCapToiDa = 5;

    public static string TaoKyLuong(DateTime ngayTinhLuong) => ngayTinhLuong.ToString("yyyy-MM");

    public static string TaoKyDanhGia(DateTime ngayDanhGia) => $"{ngayDanhGia:yyyy}-Q{TinhQuy(ngayDanhGia)}";

    public static int TinhQuy(DateTime ngay) => ((ngay.Month - 1) / 3) + 1;

    public static decimal TinhSoNgayBaoGom(DateTime tuNgay, DateTime denNgay)
    {
        return denNgay.Date < tuNgay.Date ? 0 : (decimal)(denNgay.Date - tuNgay.Date).TotalDays + 1;
    }

    public static decimal TinhSoNgayTrongThang(DateTime tuNgay, DateTime denNgay, int nam, int thang)
    {
        if (denNgay.Date < tuNgay.Date)
        {
            return 0;
        }

        var dauThang = new DateTime(nam, thang, 1);
        var cuoiThang = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));
        var ngayBatDauTinh = tuNgay.Date > dauThang ? tuNgay.Date : dauThang;
        var ngayKetThucTinh = denNgay.Date < cuoiThang ? denNgay.Date : cuoiThang;
        return TinhSoNgayBaoGom(ngayBatDauTinh, ngayKetThucTinh);
    }

    public static decimal TinhNgayCongQuyDoi(decimal tongGioCong)
    {
        return tongGioCong <= 0
            ? SoNgayCongChuanThang
            : Math.Min(SoNgayCongChuanThang, Math.Round(tongGioCong / SoGioMotNgayCong, 2));
    }

    public static int TinhSoNamTron(DateTime ngayBatDau, DateTime ngayTinh)
    {
        if (ngayBatDau.Date > ngayTinh.Date)
        {
            return 0;
        }

        var soNam = ngayTinh.Year - ngayBatDau.Year;
        return ngayBatDau.Date > ngayTinh.Date.AddYears(-soNam) ? Math.Max(0, soNam - 1) : soNam;
    }

    public static decimal TinhPhuCapLuong(decimal luongCoBan, int soNamBaoHiemXaHoi)
    {
        var tyLeThamNien = Math.Min(Math.Max(soNamBaoHiemXaHoi, 0), SoNamThamNienTinhPhuCapToiDa) * TyLePhuCapThamNienMoiNam;
        return Math.Round(luongCoBan * (TyLePhuCapCoBan + tyLeThamNien), 0);
    }

    public static decimal TinhKhauTruBaoHiem(decimal luongCoBan)
    {
        return Math.Round(luongCoBan * TyLeBaoHiemBatBuocNguoiLaoDong, 0);
    }

    public static bool LaTrangThaiNghiPhepDaDuyet(string trangThai)
    {
        return trangThai.Contains("Đã duyệt", StringComparison.OrdinalIgnoreCase)
            || trangThai.Contains("Approved", StringComparison.OrdinalIgnoreCase);
    }

    public static bool NghiPhepGiaoNgay(NghiPhep nghiPhep, DateTime ngay)
    {
        return nghiPhep.TuNgay.Date <= ngay.Date && nghiPhep.DenNgay.Date >= ngay.Date;
    }

    public static bool NghiPhepGiaoKhoang(NghiPhep nghiPhep, DateTime tuNgay, DateTime denNgay)
    {
        return nghiPhep.TuNgay.Date <= denNgay.Date && nghiPhep.DenNgay.Date >= tuNgay.Date;
    }

    public static PhieuLuong TaoPhieuLuongThang(NhanVien nhanVien, decimal luongCoBan, decimal tongGioCong, decimal soNgayNghiDaDuyet, DateTime ngayTinhLuong)
    {
        var kyLuong = TaoKyLuong(ngayTinhLuong);
        var ngayCong = TinhNgayCongQuyDoi(tongGioCong);
        var luongTheoCong = Math.Round(luongCoBan / SoNgayCongChuanThang * ngayCong, 0);
        var phuCap = TinhPhuCapLuong(luongCoBan, nhanVien.SoNamBaoHiemXaHoi);
        var khauTruNghiPhep = Math.Round(luongCoBan / SoNgayCongChuanThang * soNgayNghiDaDuyet, 0);
        var khauTru = khauTruNghiPhep + TinhKhauTruBaoHiem(luongCoBan);
        var thucLanh = Math.Max(0, luongTheoCong + phuCap - khauTru);

        return new PhieuLuong(nhanVien.HoTen, kyLuong, luongCoBan, phuCap, khauTru, thucLanh, "Nháp");
    }

    public static string TaoMaNhanVienTiepTheo(IEnumerable<string> maSoHienCo, string tienTo = "NV")
    {
        var soLonNhat = maSoHienCo
            .Select(maSo => LaySoThuTuMaNhanVien(maSo, tienTo))
            .Where(soThuTu => soThuTu.HasValue)
            .Select(soThuTu => soThuTu!.Value)
            .DefaultIfEmpty(0)
            .Max();

        return $"{tienTo}{soLonNhat + 1:000}";
    }

    private static int? LaySoThuTuMaNhanVien(string maSo, string tienTo)
    {
        if (!maSo.StartsWith(tienTo, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.TryParse(maSo[tienTo.Length..], out var soThuTu) ? soThuTu : null;
    }
}
