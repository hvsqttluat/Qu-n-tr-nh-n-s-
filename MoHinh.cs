using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyNhanSuWpf;

public class DoiTuongThongBao : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void BaoThayDoi([CallerMemberName] string? tenThuocTinh = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(tenThuocTinh));
    }
}

public class NhanVien : DoiTuongThongBao
{
    private int maNhanVien;
    private string maSo = "";
    private string hoTen = "";
    private int maPhongBan;
    private int maViTri;
    private string phongBan = "";
    private string viTri = "";
    private DateTime ngaySinh = new(1995, 1, 1);
    private DateTime ngayThamGiaBaoHiemXaHoi = DateTime.Today;
    private DateTime ngayVaoLam = DateTime.Today;
    private bool dangLamViec = true;
    private string lienHeKhanCap = "";
    private string taiKhoanNganHang = "";
    private string soCanCuoc = "";

    public int MaNhanVien { get => maNhanVien; set { maNhanVien = value; BaoThayDoi(); } }
    public string MaSo { get => maSo; set { maSo = value; BaoThayDoi(); } }
    public string HoTen { get => hoTen; set { hoTen = value; BaoThayDoi(); } }
    public int MaPhongBan { get => maPhongBan; set { maPhongBan = value; BaoThayDoi(); } }
    public int MaViTri { get => maViTri; set { maViTri = value; BaoThayDoi(); } }
    public string PhongBan { get => phongBan; set { phongBan = value; BaoThayDoi(); } }
    public string ViTri
    {
        get => viTri;
        set
        {
            viTri = value;
            BaoThayDoi();
            BaoThayDoi(nameof(ThuTuChucVu));
            BaoThayDoi(nameof(CapBacChucVu));
        }
    }
    public DateTime NgaySinh
    {
        get => ngaySinh;
        set
        {
            ngaySinh = value;
            BaoThayDoi();
            BaoThayDoi(nameof(Tuoi));
        }
    }
    public DateTime NgayThamGiaBaoHiemXaHoi
    {
        get => ngayThamGiaBaoHiemXaHoi;
        set
        {
            ngayThamGiaBaoHiemXaHoi = value;
            BaoThayDoi();
            BaoThayDoi(nameof(SoNamBaoHiemXaHoi));
        }
    }
    public DateTime NgayVaoLam { get => ngayVaoLam; set { ngayVaoLam = value; BaoThayDoi(); } }
    public bool DangLamViec { get => dangLamViec; set { dangLamViec = value; BaoThayDoi(); } }
    public string LienHeKhanCap { get => lienHeKhanCap; set { lienHeKhanCap = value; BaoThayDoi(); } }
    public string TaiKhoanNganHang { get => taiKhoanNganHang; set { taiKhoanNganHang = value; BaoThayDoi(); } }
    public string SoCanCuoc { get => soCanCuoc; set { soCanCuoc = value; BaoThayDoi(); } }
    public int ThuTuChucVu => BangXepHangChucVu.LayThuTu(ViTri);
    public string CapBacChucVu => BangXepHangChucVu.LayTenCapBac(ViTri);
    public string TrangThai => DangLamViec ? "Đang làm" : "Tạm nghỉ";
    public int Tuoi
    {
        get
        {
            return QuyTacNghiepVuNhanSu.TinhSoNamTron(NgaySinh, DateTime.Today);
        }
    }
    public int SoNamBaoHiemXaHoi => QuyTacNghiepVuNhanSu.TinhSoNamTron(NgayThamGiaBaoHiemXaHoi, DateTime.Today);

    public NhanVien TaoBanSao() => (NhanVien)MemberwiseClone();
}

public static class BangXepHangChucVu
{
    public static int LayThuTu(string chucVu)
    {
        if (chucVu.Contains("Phó giám đốc", StringComparison.OrdinalIgnoreCase)) return 2;
        if (chucVu.Contains("Giám đốc", StringComparison.OrdinalIgnoreCase)) return 1;
        if (chucVu.Contains("Trưởng phòng", StringComparison.OrdinalIgnoreCase)) return 3;
        if (chucVu.Contains("Phó phòng", StringComparison.OrdinalIgnoreCase)) return 4;
        if (chucVu.Contains("Trưởng nhóm", StringComparison.OrdinalIgnoreCase)
            || chucVu.Contains("Quản lý", StringComparison.OrdinalIgnoreCase)) return 5;
        if (chucVu.Contains("Chuyên viên", StringComparison.OrdinalIgnoreCase)) return 6;
        if (chucVu.Contains("Nhân viên", StringComparison.OrdinalIgnoreCase)) return 7;
        if (chucVu.Contains("Công nhân", StringComparison.OrdinalIgnoreCase)) return 8;
        return 9;
    }

    public static string LayTenCapBac(string chucVu) => LayThuTu(chucVu) switch
    {
        1 => "Ban lãnh đạo",
        2 => "Phó giám đốc",
        3 => "Trưởng phòng",
        4 => "Phó phòng",
        5 => "Quản lý",
        6 => "Chuyên viên",
        7 => "Nhân viên",
        8 => "Công nhân",
        _ => "Khác"
    };
}

public record PhongBan(int MaPhongBan, string TenPhongBan, string TruongPhong);
public record ViTriCongViec(int MaViTri, int MaPhongBan, string TenViTri, decimal LuongDuKien, string TrangThai);
public record NghiPhep(string NhanVien, string LoaiNghi, DateTime TuNgay, DateTime DenNgay, decimal SoNgay, string TrangThai, int MaDon = 0, string LyDoXuLyBanDau = "")
{
    public string LyDoXuLy { get; set; } = LyDoXuLyBanDau;
    public string ChucVuPhongBan { get; set; } = "";
    public string GhiChu => LyDoXuLy;
    public int ThuTuTrangThai => TrangThai.Contains("Chờ", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
    public int ThuTuMoiNhat => MaDon;
    public bool DangChoDuyet => ThuTuTrangThai == 0;
}
public record ChamCong(string NhanVien, DateTime GioVao, DateTime? GioRa, decimal SoGio)
{
    public bool DangTrongCa => GioRa is null;
    public decimal SoGioTinhToan => SoGio > 0
        ? SoGio
        : GioRa is null
            ? 0
            : Math.Round((decimal)Math.Max(0, (GioRa.Value - GioVao).TotalHours), 2);
    public decimal NgayCongQuyDoi => Math.Round(SoGioTinhToan / QuyTacNghiepVuNhanSu.SoGioMotNgayCong, 2);
    public string NgayChamCongHienThi => GioVao.ToString("dd/MM/yyyy");
    public string KhungGioHienThi => GioRa is null
        ? $"{GioVao:HH:mm} - đang mở"
        : $"{GioVao:HH:mm} - {GioRa:HH:mm}";
    public string CaLam => GioVao.Hour switch
    {
        < 12 => "Ca sáng",
        < 18 => "Ca chiều",
        _ => "Ngoài giờ"
    };
    public string TrangThaiCong => GioRa is null
        ? "Đang trong ca"
        : SoGioTinhToan < 7.5m
            ? "Thiếu giờ"
            : SoGioTinhToan > 9m
                ? "Tăng ca"
                : "Đủ công";
}
public record DanhGia(string NhanVien, string NguoiDanhGia, string KyDanhGia, decimal Diem, string NhanXet, string TrangThai);
public record PhieuLuong(string NhanVien, string KyLuong, decimal LuongCoBan, decimal PhuCap, decimal KhauTru, decimal ThucLanh, string TrangThai);
public record UngVien(string HoTen, string ViTri, string Email, string DienThoai, string GiaiDoan);
public record DiemLuongThang(string Thang, decimal TongLuong, int TongNhanVien);
public record TongHopLuongPhongBan(string PhongBan, string Ky, int SoPhieu, decimal TongLuongCoBan, decimal TongPhuCap, decimal TongKhauTru, decimal TongThucLanh);
public record MucUngVienTheoViTri(string TenViTri, int SoLuong);
public record DongTraCuuNhanSu(
    int ThuTuCapBac,
    string CapBac,
    string MaSo,
    string HoTen,
    string PhongBan,
    string ViTri,
    decimal? DiemDanhGia,
    decimal LuongCoBan,
    decimal ThucLanh,
    string KyLuong)
{
    public string DiemHienThi => DiemDanhGia.HasValue ? $"{DiemDanhGia:N1}" : "--";
}

public record TongHopPhongBanDieuHanh(
    int ThuTuCapBac,
    string TenPhongBan,
    string NhanSuCapCao,
    string ChucVuCaoNhat,
    int SoNhanVien,
    decimal QuyLuong,
    string CaNhanXuatSac,
    string DiemXuatSacHienThi,
    string NhanSuLuongCao,
    string LuongCaoHienThi);

public record PhienDangNhap(string TenDangNhap, string HoTen, string VaiTro)
{
    public static PhienDangNhap MacDinh { get; } = new("admin", "Quản trị hệ thống", "Admin");
    public string HienThi => $"{HoTen} - {VaiTro}";
}

public record TaiKhoanHeThong(string TenDangNhap, string HoTen, string VaiTro, string QuyenHan, string TrangThai, DateTime LanDangNhapGanNhat);

public class BieuMauTaiKhoan : DoiTuongThongBao
{
    private string tenDangNhapGoc = "";
    private string tenDangNhap = "";
    private string hoTen = "";
    private string vaiTro = "Nhân viên";
    private string matKhauMoi = "";
    private bool dangHoatDong = true;
    private bool dangSua;

    public string TenDangNhapGoc { get => tenDangNhapGoc; set { tenDangNhapGoc = value; BaoThayDoi(); } }
    public string TenDangNhap { get => tenDangNhap; set { tenDangNhap = value; BaoThayDoi(); } }
    public string HoTen { get => hoTen; set { hoTen = value; BaoThayDoi(); } }
    public string VaiTro { get => vaiTro; set { vaiTro = value; BaoThayDoi(); } }
    public string MatKhauMoi { get => matKhauMoi; set { matKhauMoi = value; BaoThayDoi(); } }
    public bool DangHoatDong { get => dangHoatDong; set { dangHoatDong = value; BaoThayDoi(); } }
    public bool DangSua { get => dangSua; set { dangSua = value; BaoThayDoi(); BaoThayDoi(nameof(TieuDe)); } }
    public string TieuDe => DangSua ? "Sửa thông tin tài khoản" : "Thêm mới tài khoản";

    public void XoaTrang()
    {
        TenDangNhapGoc = "";
        TenDangNhap = "";
        HoTen = "";
        VaiTro = "Nhân viên";
        MatKhauMoi = "";
        DangHoatDong = true;
        DangSua = false;
    }
}

public class ThongBaoHeThong : DoiTuongThongBao
{
    private bool daDoc;

    public ThongBaoHeThong(string tieuDe, string noiDung, string phanHe, DateTime thoiGian, string mucDo, bool daDoc = false, string? tenTepDinhKem = null, string? duongDanTepDinhKem = null)
    {
        TieuDe = tieuDe;
        NoiDung = noiDung;
        PhanHe = phanHe;
        ThoiGian = thoiGian;
        MucDo = mucDo;
        TenTepDinhKem = tenTepDinhKem ?? "";
        DuongDanTepDinhKem = duongDanTepDinhKem ?? "";
        this.daDoc = daDoc;
    }

    public string TieuDe { get; }
    public string NoiDung { get; }
    public string PhanHe { get; }
    public DateTime ThoiGian { get; }
    public string MucDo { get; }
    public string TenTepDinhKem { get; }
    public string DuongDanTepDinhKem { get; }
    public bool CoTepDinhKem => !string.IsNullOrWhiteSpace(DuongDanTepDinhKem);
    public string TepDinhKemHienThi => CoTepDinhKem ? TenTepDinhKem : "Không có tệp đính kèm";
    public bool DaDoc
    {
        get => daDoc;
        set
        {
            if (daDoc == value) return;
            daDoc = value;
            BaoThayDoi();
            BaoThayDoi(nameof(TrangThaiDoc));
        }
    }

    public string TrangThaiDoc => DaDoc ? "Đã đọc" : "Chưa đọc";
}

public class BieuMauUngVien : DoiTuongThongBao
{
    private string hoTen = "";
    private string email = "";
    private string dienThoai = "";
    private int maViTri = 1;

    public string HoTen { get => hoTen; set { hoTen = value; BaoThayDoi(); } }
    public string Email { get => email; set { email = value; BaoThayDoi(); } }
    public string DienThoai { get => dienThoai; set { dienThoai = value; BaoThayDoi(); } }
    public int MaViTri { get => maViTri; set { maViTri = value; BaoThayDoi(); } }
}

public class BieuMauNghiPhep : DoiTuongThongBao
{
    private int maNhanVien = 1;
    private string loaiNghi = "Nghỉ phép năm";
    private DateTime tuNgay = DateTime.Today;
    private DateTime denNgay = DateTime.Today.AddDays(1);
    private string lyDo = "";

    public int MaNhanVien { get => maNhanVien; set { maNhanVien = value; BaoThayDoi(); } }
    public string LoaiNghi { get => loaiNghi; set { loaiNghi = value; BaoThayDoi(); } }
    public DateTime TuNgay { get => tuNgay; set { tuNgay = value; BaoThayDoi(); } }
    public DateTime DenNgay { get => denNgay; set { denNgay = value; BaoThayDoi(); } }
    public string LyDo { get => lyDo; set { lyDo = value; BaoThayDoi(); } }
}

public class BieuMauDanhGia : DoiTuongThongBao
{
    private int maNhanVien = 1;
    private int maNguoiDanhGia = 1;
    private string kyDanhGia = QuyTacNghiepVuNhanSu.TaoKyDanhGia(DateTime.Today);
    private decimal diem = 85;
    private string nhanXet = "Đánh giá mới từ ứng dụng";
    private string trangThai = "Nháp";
    private int maNhanVienGoc;
    private string kyDanhGiaGoc = "";

    public int MaNhanVien { get => maNhanVien; set { maNhanVien = value; BaoThayDoi(); } }
    public int MaNguoiDanhGia { get => maNguoiDanhGia; set { maNguoiDanhGia = value; BaoThayDoi(); } }
    public string KyDanhGia { get => kyDanhGia; set { kyDanhGia = value; BaoThayDoi(); } }
    public decimal Diem { get => diem; set { diem = value; BaoThayDoi(); } }
    public string NhanXet { get => nhanXet; set { nhanXet = value; BaoThayDoi(); } }
    public string TrangThai { get => trangThai; set { trangThai = value; BaoThayDoi(); } }
    public int MaNhanVienGoc { get => maNhanVienGoc; set { maNhanVienGoc = value; BaoThayDoi(); } }
    public string KyDanhGiaGoc { get => kyDanhGiaGoc; set { kyDanhGiaGoc = value; BaoThayDoi(); } }
    public bool DangSua => MaNhanVienGoc > 0 && !string.IsNullOrWhiteSpace(KyDanhGiaGoc);
}

public class BieuMauPhongBan : DoiTuongThongBao
{
    private int maPhongBan;
    private string tenPhongBan = "";
    private int? maTruongPhong;

    public int MaPhongBan { get => maPhongBan; set { maPhongBan = value; BaoThayDoi(); } }
    public string TenPhongBan { get => tenPhongBan; set { tenPhongBan = value; BaoThayDoi(); } }
    public int? MaTruongPhong { get => maTruongPhong; set { maTruongPhong = value; BaoThayDoi(); } }
}

public class KhoDuLieuUngDung
{
    public ObservableCollection<NhanVien> NhanVien { get; } = [];
    public ObservableCollection<PhongBan> PhongBan { get; } = [];
    public ObservableCollection<ViTriCongViec> ViTri { get; } = [];
    public ObservableCollection<NghiPhep> NghiPhep { get; } = [];
    public ObservableCollection<ChamCong> ChamCong { get; } = [];
    public ObservableCollection<DanhGia> DanhGia { get; } = [];
    public ObservableCollection<PhieuLuong> PhieuLuong { get; } = [];
    public ObservableCollection<UngVien> UngVien { get; } = [];
    public ObservableCollection<ThongBaoHeThong> ThongBao { get; } = [];
}
