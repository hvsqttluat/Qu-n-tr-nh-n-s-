using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace QuanLyNhanSuWpf;

public class LenhGiaoDien(Action<object?> thucThi, Predicate<object?>? coTheThucThi = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => coTheThucThi?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => thucThi(parameter);
    public void LamMoi() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class SoSanhNhanVienTheoUuTien(ManHinhChinhViewModel manHinh) : IComparer
{
    public int Compare(object? x, object? y)
    {
        return manHinh.SoSanhNhanVien(x as NhanVien, y as NhanVien);
    }
}

public class ManHinhChinhViewModel : DoiTuongThongBao
{
    private const string TatCaPhongBan = "Tất cả phòng ban";
    private const string TatCaTrangThaiChamCong = "Tất cả trạng thái";
    private readonly KhoDuLieuNhanSu khoDuLieu = new();
    private KhoDuLieuUngDung duLieu = new();
    private string mucDangChon = "Tổng quan";
    private string tuKhoaTimKiem = "";
    private string tuKhoaThongBao = "";
    private string tuKhoaTraCuuDieuHanh = "";
    private string phongBanNhanVienDangChon = TatCaPhongBan;
    private string tuKhoaBangLuong = "";
    private string phongBanBangLuongDangChon = TatCaPhongBan;
    private string tuKhoaChamCong = "";
    private string phongBanChamCongDangChon = TatCaPhongBan;
    private string trangThaiChamCongDangChon = TatCaTrangThaiChamCong;
    private DateTime? tuNgayChamCong = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? denNgayChamCong = DateTime.Today;
    private string kyNghiPhepDangChon = "Tháng này";
    private string phongBanNghiPhepDangChon = TatCaPhongBan;
    private string tuKhoaDanhGia = "";
    private string phongBanDanhGiaDangChon = TatCaPhongBan;
    private int tabDanhGiaDangChon;
    private string kyTongHopLuongDangChon = "Tháng này";
    private string kyBaoCaoNhanSuDangChon = "Tháng này";
    private bool chiThongBaoChuaDoc;
    private bool dangMoBieuMauThongBao;
    private string tieuDeThongBaoMoi = "";
    private string noiDungThongBaoMoi = "";
    private string phanHeThongBaoMoi = "Hệ thống";
    private string mucDoThongBaoMoi = "Thông tin";
    private string duongDanTepThongBaoMoi = "";
    private string tenTepThongBaoMoi = "";
    private string thongBaoNhanh = "";
    private bool dangHienThongBaoNhanh;
    private string nguonDuLieu = "Đang tải...";
    private string thoiGianHienTai = "";
    private NhanVien? nhanVienDangChon;
    private NhanVien bieuMauNhanVien = new();
    private PhongBan? phongBanDangChon;
    private TongHopPhongBanDieuHanh? tongHopPhongBanDangChon;
    private UngVien? ungVienDangChon;
    private ChamCong? chamCongDangChon;
    private NghiPhep? nghiPhepDangChon;
    private DanhGia? danhGiaDangChon;
    private PhieuLuong? phieuLuongDangChon;
    private TaiKhoanHeThong? taiKhoanDangChon;
    private BieuMauTaiKhoan bieuMauTaiKhoan = new();
    private BieuMauUngVien bieuMauUngVien = new();
    private BieuMauNghiPhep bieuMauNghiPhep = new();
    private BieuMauDanhGia bieuMauDanhGia = new();
    private BieuMauPhongBan bieuMauPhongBan = new();
    private ObservableCollection<DiemLuongThang> duLieuLuong12Thang = [];
    private ObservableCollection<MucUngVienTheoViTri> duLieuUngVienTheoViTri = [];
    private ObservableCollection<DongTraCuuNhanSu> danhSachNhanSuTraCuu = [];
    private ObservableCollection<TongHopPhongBanDieuHanh> tongHopPhongBanDieuHanh = [];
    private IReadOnlyList<string> cacPhongBanTraCuu = [TatCaPhongBan];
    private string phongBanTraCuuDangChon = TatCaPhongBan;
    private readonly DispatcherTimer dongHo = new() { Interval = TimeSpan.FromSeconds(30) };
    private readonly DispatcherTimer boDemThongBaoNhanh = new() { Interval = TimeSpan.FromSeconds(3) };
    private readonly List<ThongBaoHeThong> thongBaoPhienLamViec = [];
    private readonly HashSet<string> khoaThongBaoDaDoc = new(StringComparer.Ordinal);

    public ManHinhChinhViewModel(PhienDangNhap? phienDangNhap = null)
    {
        PhienDangNhap = phienDangNhap ?? PhienDangNhap.MacDinh;
        TaiTrangThaiDocThongBao();
        mucDangChon = "Tổng quan";
        CacMucDieuHuong = ["Tổng quan", "Nhân viên", "Phòng ban", "Ứng viên", "Chấm công", "Nghỉ phép", "Đánh giá", "Bảng lương", "Báo cáo", "Cài đặt tài khoản"];
        CacTaiKhoanHeThong = TaoTaiKhoanHeThongMau(PhienDangNhap);
        ChonMucLenh = new LenhGiaoDien(ChonMucDieuHuong);
        ThemMoiLenh = new LenhGiaoDien(_ => TaoMoiNhanVien(), _ => CoQuyenQuanLyHoSoNhanVien);
        SuaLenh = new LenhGiaoDien(_ => DuaVaoBieuMau(), _ => CoTheThaoTacNhanVienDangChon());
        LuuLenh = new LenhGiaoDien(async _ => await LuuNhanVien(), _ => CoQuyenQuanLyHoSoNhanVien);
        XoaLenh = new LenhGiaoDien(async _ => await XoaNhanVien(), _ => CoTheSuaNhanVienDangChon());
        TaiLaiLenh = new LenhGiaoDien(async _ => await TaiDuLieu());
        TaoThongBaoThuLenh = new LenhGiaoDien(_ => MoBieuMauThongBao(), _ => CoQuyenTaoThongBao);
        GuiThongBaoLenh = new LenhGiaoDien(_ => GuiThongBaoMoi(), _ => CoQuyenTaoThongBao);
        HuyTaoThongBaoLenh = new LenhGiaoDien(_ => DongBieuMauThongBao());
        ChonTepThongBaoLenh = new LenhGiaoDien(_ => ChonTepThongBao(), _ => CoQuyenTaoThongBao);
        XoaTepThongBaoLenh = new LenhGiaoDien(_ => XoaTepThongBao(), _ => CoQuyenTaoThongBao && CoTepThongBaoMoi);
        MoTepThongBaoLenh = new LenhGiaoDien(p => MoTepThongBao(p));
        MoThongBaoLenh = new LenhGiaoDien(p => MoThongBao(p));
        CapNhatDocThongBaoLenh = new LenhGiaoDien(p => CapNhatDocThongBao(p));
        TaoPhongBanLenh = new LenhGiaoDien(_ => TaoMoiPhongBan());
        SuaPhongBanLenh = new LenhGiaoDien(_ => DuaPhongBanVaoBieuMau(), _ => PhongBanDangChon is not null);
        LuuPhongBanLenh = new LenhGiaoDien(async _ => await LuuPhongBan());
        XoaPhongBanLenh = new LenhGiaoDien(async _ => await XoaPhongBan(), _ => PhongBanDangChon is not null);
        GanTruongPhongLenh = new LenhGiaoDien(async _ => await GanTruongPhong(), _ => PhongBanDangChon is not null && DuLieu.NhanVien.Any());
        TaoUngVienLenh = new LenhGiaoDien(async _ => await TaoUngVien());
        ChuyenUngVienThanhNhanVienLenh = new LenhGiaoDien(async _ => await ChuyenUngVienThanhNhanVien(), _ => CoTheChuyenUngVienThanhNhanVien());
        ChuyenGiaiDoanUngVienLenh = new LenhGiaoDien(async _ => await ChuyenGiaiDoanUngVien(), _ => UngVienDangChon is not null && LocUngVien(UngVienDangChon));
        XuatHopDongLamViecLenh = new LenhGiaoDien(_ => XuatHopDongLamViec(), _ => UngVienDangChon is not null && LocUngVien(UngVienDangChon));
        VaoCaLenh = new LenhGiaoDien(async _ => await VaoCa(), _ => CoTheVaoCaDangChon);
        RaCaLenh = new LenhGiaoDien(async _ => await RaCa(), _ => CoTheRaCaDangChon);
        DieuChinhCongLenh = new LenhGiaoDien(async _ => await DieuChinhCong(), _ => CoQuyenDieuChinhCong && ChamCongDangChon is not null && LocChamCong(ChamCongDangChon));
        TaoNghiPhepLenh = new LenhGiaoDien(async _ => await TaoNghiPhep());
        DuyetNghiPhepLenh = new LenhGiaoDien(async _ => await CapNhatNghiPhep("Đã duyệt"), _ => CoTheXuLyNghiPhep(NghiPhepDangChon));
        TuChoiNghiPhepLenh = new LenhGiaoDien(async _ => await CapNhatNghiPhep("Từ chối"), _ => CoTheXuLyNghiPhep(NghiPhepDangChon));
        DuyetNghiPhepDongLenh = new LenhGiaoDien(async p => await CapNhatNghiPhepTuDong(p, "Đã duyệt"), CoTheXuLyNghiPhep);
        TuChoiNghiPhepDongLenh = new LenhGiaoDien(async p => await CapNhatNghiPhepTuDong(p, "Từ chối"), CoTheXuLyNghiPhep);
        TaoMoiDanhGiaLenh = new LenhGiaoDien(_ => TaoMoiDanhGia(), _ => CoQuyenGhiNhanDanhGia);
        SuaDanhGiaLenh = new LenhGiaoDien(_ => DuaDanhGiaVaoBieuMau(), _ => CoQuyenGhiNhanDanhGia && DanhGiaDangChon is not null && LocDanhGia(DanhGiaDangChon));
        TaoDanhGiaLenh = new LenhGiaoDien(async _ => await LuuDanhGia(), _ => CoQuyenGhiNhanDanhGia);
        XoaDanhGiaLenh = new LenhGiaoDien(async _ => await XoaDanhGia(), _ => CoQuyenGhiNhanDanhGia && DanhGiaDangChon is not null && LocDanhGia(DanhGiaDangChon));
        ChotDanhGiaLenh = new LenhGiaoDien(async _ => await ChotDanhGia(), _ => CoQuyenGhiNhanDanhGia && DanhGiaDangChon is not null && LocDanhGia(DanhGiaDangChon));
        TinhLuongLenh = new LenhGiaoDien(async _ => await TinhLuong(), _ => CoQuyenXuLyBangLuong && CoTheThaoTacNhanVienDangChon());
        XemPhieuLuongLenh = new LenhGiaoDien(_ => XemPhieuLuong(), _ => PhieuLuongDangChon is not null && LocPhieuLuong(PhieuLuongDangChon));
        XacNhanTraLuongLenh = new LenhGiaoDien(async _ => await XacNhanTraLuong(), _ => CoQuyenXuLyBangLuong && PhieuLuongDangChon is not null && LocPhieuLuong(PhieuLuongDangChon));
        XuatBaoCaoNhanVienLenh = new LenhGiaoDien(_ => XuatBaoCaoNhanVien());
        XuatBaoCaoChamCongLenh = new LenhGiaoDien(_ => XuatBaoCaoChamCong());
        XuatBaoCaoNghiPhepLenh = new LenhGiaoDien(_ => XuatBaoCaoNghiPhep());
        XuatBaoCaoLuongLenh = new LenhGiaoDien(_ => XuatBaoCaoLuong(), _ => CoQuyenXuLyBangLuong);
        TaoTaiKhoanMauLenh = new LenhGiaoDien(async _ => await TaoTaiKhoanMau());
        KhoaMoTaiKhoanLenh = new LenhGiaoDien(async _ => await KhoaMoTaiKhoan(), _ => TaiKhoanDangChon is not null);
        ThemTaiKhoanLenh = new LenhGiaoDien(_ => TaoMoiTaiKhoan());
        SuaThongTinTaiKhoanLenh = new LenhGiaoDien(_ => SuaThongTinTaiKhoan(), _ => TaiKhoanDangChon is not null);
        LuuTaiKhoanLenh = new LenhGiaoDien(async _ => await LuuTaiKhoan());
        HuySuaTaiKhoanLenh = new LenhGiaoDien(_ => BieuMauTaiKhoan.XoaTrang());
        SaoLuuDuLieuLenh = new LenhGiaoDien(_ => SaoLuuDuLieu());
        PhucHoiDuLieuLenh = new LenhGiaoDien(_ => PhucHoiDuLieu());

        CapNhatDongHo();
        dongHo.Tick += (_, _) => CapNhatDongHo();
        dongHo.Start();
        boDemThongBaoNhanh.Tick += (_, _) =>
        {
            boDemThongBaoNhanh.Stop();
            DangHienThongBaoNhanh = false;
        };
    }

    public PhienDangNhap PhienDangNhap { get; }
    public ObservableCollection<string> CacMucDieuHuong { get; }
    public ObservableCollection<TaiKhoanHeThong> CacTaiKhoanHeThong { get; }
    public IReadOnlyList<string> CacVaiTroTaiKhoan { get; } = ["Admin", "Giám đốc", "Trưởng phòng", "Nhân viên"];
    public ICollectionView DanhSachNhanVienView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<NhanVien>());
    public ICollectionView DanhSachThongBaoView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<ThongBaoHeThong>());
    public ICollectionView DanhSachUngVienView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<UngVien>());
    public ICollectionView DanhSachPhieuLuongView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<PhieuLuong>());
    public ICollectionView DanhSachChamCongView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<ChamCong>());
    public ICollectionView DanhSachNghiPhepView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<NghiPhep>());
    public ICollectionView DanhSachNghiPhepDaDuyetView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<NghiPhep>());
    public ICollectionView DanhSachDeNghiNghiPhepView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<NghiPhep>());
    public ICollectionView DanhSachDanhGiaView { get; private set; } = CollectionViewSource.GetDefaultView(new ObservableCollection<DanhGia>());
    public ICommand ChonMucLenh { get; }
    public ICommand ThemMoiLenh { get; }
    public ICommand SuaLenh { get; }
    public ICommand LuuLenh { get; }
    public ICommand XoaLenh { get; }
    public ICommand TaiLaiLenh { get; }
    public ICommand TaoThongBaoThuLenh { get; }
    public ICommand GuiThongBaoLenh { get; }
    public ICommand HuyTaoThongBaoLenh { get; }
    public ICommand ChonTepThongBaoLenh { get; }
    public ICommand XoaTepThongBaoLenh { get; }
    public ICommand MoTepThongBaoLenh { get; }
    public ICommand MoThongBaoLenh { get; }
    public ICommand CapNhatDocThongBaoLenh { get; }
    public ICommand TaoPhongBanLenh { get; }
    public ICommand SuaPhongBanLenh { get; }
    public ICommand LuuPhongBanLenh { get; }
    public ICommand XoaPhongBanLenh { get; }
    public ICommand GanTruongPhongLenh { get; }
    public ICommand TaoUngVienLenh { get; }
    public ICommand ChuyenUngVienThanhNhanVienLenh { get; }
    public ICommand ChuyenGiaiDoanUngVienLenh { get; }
    public ICommand XuatHopDongLamViecLenh { get; }
    public ICommand VaoCaLenh { get; }
    public ICommand RaCaLenh { get; }
    public ICommand DieuChinhCongLenh { get; }
    public ICommand TaoNghiPhepLenh { get; }
    public ICommand DuyetNghiPhepLenh { get; }
    public ICommand TuChoiNghiPhepLenh { get; }
    public ICommand DuyetNghiPhepDongLenh { get; }
    public ICommand TuChoiNghiPhepDongLenh { get; }
    public ICommand TaoMoiDanhGiaLenh { get; }
    public ICommand SuaDanhGiaLenh { get; }
    public ICommand TaoDanhGiaLenh { get; }
    public ICommand XoaDanhGiaLenh { get; }
    public ICommand ChotDanhGiaLenh { get; }
    public ICommand TinhLuongLenh { get; }
    public ICommand XemPhieuLuongLenh { get; }
    public ICommand XacNhanTraLuongLenh { get; }
    public ICommand XuatBaoCaoNhanVienLenh { get; }
    public ICommand XuatBaoCaoChamCongLenh { get; }
    public ICommand XuatBaoCaoNghiPhepLenh { get; }
    public ICommand XuatBaoCaoLuongLenh { get; }
    public ICommand TaoTaiKhoanMauLenh { get; }
    public ICommand KhoaMoTaiKhoanLenh { get; }
    public ICommand ThemTaiKhoanLenh { get; }
    public ICommand SuaThongTinTaiKhoanLenh { get; }
    public ICommand LuuTaiKhoanLenh { get; }
    public ICommand HuySuaTaiKhoanLenh { get; }
    public ICommand SaoLuuDuLieuLenh { get; }
    public ICommand PhucHoiDuLieuLenh { get; }

    public KhoDuLieuUngDung DuLieu { get => duLieu; private set { duLieu = value; BaoThayDoi(); } }
    public string MucDangChon
    {
        get => mucDangChon;
        set
        {
            if (string.Equals(mucDangChon, value, StringComparison.Ordinal))
            {
                return;
            }

            mucDangChon = value;
            BaoThayDoi();
            BaoThayDoi(nameof(DangXemTongQuan));
            BaoThayDoi(nameof(HienThiBieuMauThongBaoChung));
            BaoThayDoi(nameof(HienThiBieuMauThongBaoTrungTam));
            CapNhatHienThiMuc();
        }
    }
    public string NguonDuLieu { get => nguonDuLieu; set { nguonDuLieu = value; BaoThayDoi(); } }
    public string ThoiGianHienTai { get => thoiGianHienTai; set { thoiGianHienTai = value; BaoThayDoi(); } }
    public string TuKhoaTimKiem { get => tuKhoaTimKiem; set { tuKhoaTimKiem = value; BaoThayDoi(); LamMoiBoLocNhanVien(); } }
    public string TuKhoaThongBao { get => tuKhoaThongBao; set { tuKhoaThongBao = value; BaoThayDoi(); DanhSachThongBaoView.Refresh(); } }
    public string TuKhoaTraCuuDieuHanh
    {
        get => tuKhoaTraCuuDieuHanh;
        set
        {
            var giaTri = value ?? "";
            if (string.Equals(tuKhoaTraCuuDieuHanh, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            tuKhoaTraCuuDieuHanh = giaTri;
            BaoThayDoi();
            CapNhatTraCuuDieuHanh();
        }
    }
    public string PhongBanNhanVienDangChon
    {
        get => phongBanNhanVienDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanNhanVienDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanNhanVienDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocNhanVien();
        }
    }
    public string TuKhoaBangLuong
    {
        get => tuKhoaBangLuong;
        set
        {
            var giaTri = value ?? "";
            if (string.Equals(tuKhoaBangLuong, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            tuKhoaBangLuong = giaTri;
            BaoThayDoi();
            LamMoiBoLocBangLuong();
        }
    }
    public string PhongBanBangLuongDangChon
    {
        get => phongBanBangLuongDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanBangLuongDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanBangLuongDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocBangLuong();
        }
    }
    public string TuKhoaChamCong
    {
        get => tuKhoaChamCong;
        set
        {
            var giaTri = value ?? "";
            if (string.Equals(tuKhoaChamCong, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            tuKhoaChamCong = giaTri;
            BaoThayDoi();
            LamMoiBoLocChamCong();
        }
    }
    public string PhongBanChamCongDangChon
    {
        get => phongBanChamCongDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanChamCongDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanChamCongDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocChamCong();
        }
    }
    public string TrangThaiChamCongDangChon
    {
        get => trangThaiChamCongDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaTrangThaiChamCong : value;
            if (string.Equals(trangThaiChamCongDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            trangThaiChamCongDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocChamCong();
        }
    }
    public DateTime? TuNgayChamCong
    {
        get => tuNgayChamCong;
        set
        {
            if (tuNgayChamCong == value)
            {
                return;
            }

            tuNgayChamCong = value;
            BaoThayDoi();
            LamMoiBoLocChamCong();
        }
    }
    public DateTime? DenNgayChamCong
    {
        get => denNgayChamCong;
        set
        {
            if (denNgayChamCong == value)
            {
                return;
            }

            denNgayChamCong = value;
            BaoThayDoi();
            LamMoiBoLocChamCong();
        }
    }
    public string KyNghiPhepDangChon
    {
        get => kyNghiPhepDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? "Tháng này" : value;
            if (string.Equals(kyNghiPhepDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            kyNghiPhepDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocNghiPhep();
        }
    }
    public string PhongBanNghiPhepDangChon
    {
        get => phongBanNghiPhepDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanNghiPhepDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanNghiPhepDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocNghiPhep();
        }
    }
    public string TuKhoaDanhGia
    {
        get => tuKhoaDanhGia;
        set
        {
            var giaTri = value ?? "";
            if (string.Equals(tuKhoaDanhGia, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            tuKhoaDanhGia = giaTri;
            BaoThayDoi();
            LamMoiBoLocDanhGia();
        }
    }
    public string PhongBanDanhGiaDangChon
    {
        get => phongBanDanhGiaDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanDanhGiaDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanDanhGiaDangChon = giaTri;
            BaoThayDoi();
            LamMoiBoLocDanhGia();
        }
    }
    public int TabDanhGiaDangChon { get => tabDanhGiaDangChon; set { tabDanhGiaDangChon = value; BaoThayDoi(); } }
    public string KyTongHopLuongDangChon
    {
        get => kyTongHopLuongDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? "Tháng này" : value;
            if (string.Equals(kyTongHopLuongDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            kyTongHopLuongDangChon = giaTri;
            BaoThayDoi();
            BaoThayDoi(nameof(TongHopLuongTheoPhongBan));
        }
    }
    public string KyBaoCaoNhanSuDangChon
    {
        get => kyBaoCaoNhanSuDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? "Tháng này" : value;
            if (string.Equals(kyBaoCaoNhanSuDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            kyBaoCaoNhanSuDangChon = giaTri;
            BaoThayDoi();
            BaoThayDoi(nameof(SoChamCongTrongKyBaoCao));
            BaoThayDoi(nameof(TongGioCongTrongKyBaoCao));
            BaoThayDoi(nameof(SoNghiPhepTrongKyBaoCao));
            BaoThayDoi(nameof(NhanSuNghiDaDuyetTrongKyBaoCao));
            BaoThayDoi(nameof(QuanSoHienDienTrongKyBaoCao));
            BaoThayDoi(nameof(SoPhieuLuongTrongKyBaoCao));
            BaoThayDoi(nameof(DanhSachNghiPhepDaDuyetTheoKy));
            BaoThayDoi(nameof(TieuDeAiDangNghi));
            BaoThayDoi(nameof(MoTaAiDangNghi));
            BaoThayDoi(nameof(TomTatBaoCaoNhanSu));
        }
    }
    public bool ChiThongBaoChuaDoc { get => chiThongBaoChuaDoc; set { chiThongBaoChuaDoc = value; BaoThayDoi(); DanhSachThongBaoView.Refresh(); } }
    public bool DangMoBieuMauThongBao
    {
        get => dangMoBieuMauThongBao;
        set
        {
            dangMoBieuMauThongBao = value;
            BaoThayDoi();
            BaoThayDoi(nameof(HienThiBieuMauThongBao));
            BaoThayDoi(nameof(HienThiBieuMauThongBaoChung));
            BaoThayDoi(nameof(HienThiBieuMauThongBaoTrungTam));
        }
    }
    public string TieuDeThongBaoMoi { get => tieuDeThongBaoMoi; set { tieuDeThongBaoMoi = value; BaoThayDoi(); } }
    public string NoiDungThongBaoMoi { get => noiDungThongBaoMoi; set { noiDungThongBaoMoi = value; BaoThayDoi(); } }
    public string PhanHeThongBaoMoi { get => phanHeThongBaoMoi; set { phanHeThongBaoMoi = value; BaoThayDoi(); } }
    public string MucDoThongBaoMoi { get => mucDoThongBaoMoi; set { mucDoThongBaoMoi = value; BaoThayDoi(); } }
    public string DuongDanTepThongBaoMoi { get => duongDanTepThongBaoMoi; set { duongDanTepThongBaoMoi = value; BaoThayDoi(); BaoThayDoi(nameof(CoTepThongBaoMoi)); (XoaTepThongBaoLenh as LenhGiaoDien)?.LamMoi(); } }
    public string TenTepThongBaoMoi { get => tenTepThongBaoMoi; set { tenTepThongBaoMoi = value; BaoThayDoi(); BaoThayDoi(nameof(TenTepThongBaoHienThi)); } }
    public string ThongBaoNhanh { get => thongBaoNhanh; set { thongBaoNhanh = value; BaoThayDoi(); } }
    public bool DangHienThongBaoNhanh { get => dangHienThongBaoNhanh; set { dangHienThongBaoNhanh = value; BaoThayDoi(); BaoThayDoi(nameof(HienThiThongBaoNhanh)); } }
    public NhanVien? NhanVienDangChon { get => nhanVienDangChon; set { nhanVienDangChon = value is not null && !CoTheXemNhanVien(value) ? null : value; BaoThayDoi(); NeuDangSuaNhanVienThiNapBieuMau(); LamMoiLenhChonNhanVien(); } }
    public NhanVien BieuMauNhanVien { get => bieuMauNhanVien; set { bieuMauNhanVien = value; BaoThayDoi(); } }
    public PhongBan? PhongBanDangChon { get => phongBanDangChon; set { phongBanDangChon = value; BaoThayDoi(); DongBoDongTongHopPhongBanDangChon(); NeuDangSuaPhongBanThiNapBieuMau(); LamMoiLenhChonPhongBan(); } }
    public TongHopPhongBanDieuHanh? TongHopPhongBanDangChon { get => tongHopPhongBanDangChon; set { tongHopPhongBanDangChon = value; BaoThayDoi(); ChonPhongBanTuTongHop(value); } }
    public UngVien? UngVienDangChon { get => ungVienDangChon; set { ungVienDangChon = value; BaoThayDoi(); (ChuyenUngVienThanhNhanVienLenh as LenhGiaoDien)?.LamMoi(); (ChuyenGiaiDoanUngVienLenh as LenhGiaoDien)?.LamMoi(); (XuatHopDongLamViecLenh as LenhGiaoDien)?.LamMoi(); } }
    public ChamCong? ChamCongDangChon { get => chamCongDangChon; set { chamCongDangChon = value; BaoThayDoi(); (DieuChinhCongLenh as LenhGiaoDien)?.LamMoi(); } }
    public NghiPhep? NghiPhepDangChon { get => nghiPhepDangChon; set { nghiPhepDangChon = value; BaoThayDoi(); LamMoiLenhNghiPhep(); } }
    public DanhGia? DanhGiaDangChon { get => danhGiaDangChon; set { danhGiaDangChon = value; BaoThayDoi(); LamMoiLenhDanhGia(); } }
    public PhieuLuong? PhieuLuongDangChon { get => phieuLuongDangChon; set { phieuLuongDangChon = value; BaoThayDoi(); (XemPhieuLuongLenh as LenhGiaoDien)?.LamMoi(); (XacNhanTraLuongLenh as LenhGiaoDien)?.LamMoi(); } }
    public TaiKhoanHeThong? TaiKhoanDangChon { get => taiKhoanDangChon; set { taiKhoanDangChon = value; BaoThayDoi(); (KhoaMoTaiKhoanLenh as LenhGiaoDien)?.LamMoi(); (SuaThongTinTaiKhoanLenh as LenhGiaoDien)?.LamMoi(); } }
    public BieuMauTaiKhoan BieuMauTaiKhoan { get => bieuMauTaiKhoan; set { bieuMauTaiKhoan = value; BaoThayDoi(); } }
    public BieuMauUngVien BieuMauUngVien { get => bieuMauUngVien; set { bieuMauUngVien = value; BaoThayDoi(); } }
    public BieuMauNghiPhep BieuMauNghiPhep { get => bieuMauNghiPhep; set { bieuMauNghiPhep = value; BaoThayDoi(); } }
    public BieuMauDanhGia BieuMauDanhGia { get => bieuMauDanhGia; set { bieuMauDanhGia = value; BaoThayDoi(); } }
    public BieuMauPhongBan BieuMauPhongBan { get => bieuMauPhongBan; set { bieuMauPhongBan = value; BaoThayDoi(); } }

    public int TongNhanVien => LayNhanVienTrongPhamVi().Count();
    public int NhanSuConHieuLuc => LayNhanVienTrongPhamVi().Count(n => n.DangLamViec);
    public int NghiPhepDaDuyetHomNay => LayTenNhanVienNghiDaDuyetNgay(DateTime.Today).Count;
    public int NhanSuTamVangHomNay => Math.Max(0, NhanSuConHieuLuc - DangLamViec);
    public int DangLamViec
    {
        get
        {
            var nghiPhepHomNay = LayTenNhanVienNghiDaDuyetNgay(DateTime.Today);
            return LayNhanVienTrongPhamVi()
                .Count(nhanVien => nhanVien.DangLamViec && !nghiPhepHomNay.Contains(nhanVien.HoTen));
        }
    }
    public int TamNghi => Math.Max(0, TongNhanVien - DangLamViec);
    public int NghiChoDuyet => DuLieu.NghiPhep.Count(n => LocNghiPhep(n) && n.TrangThai.Contains("Chờ", StringComparison.OrdinalIgnoreCase));
    public decimal TongQuyLuong => DuLieu.PhieuLuong.Where(p => CoTheXemTheoTen(p.NhanVien)).Sum(p => p.ThucLanh);
    public int SoThongBao => DuLieu.ThongBao.Count;
    public int SoThongBaoChuaDoc => DuLieu.ThongBao.Count(t => !t.DaDoc);
    public string TenNguoiDung => PhienDangNhap.HoTen;
    public string VaiTroNguoiDung => PhienDangNhap.VaiTro;
    public string TenDangNhap => PhienDangNhap.TenDangNhap;
    public string MoTaQuyenNguoiDung => LayMoTaQuyen(VaiTroNguoiDung);
    public IEnumerable<TaiKhoanHeThong> CacTaiKhoanDangHoatDong => CacTaiKhoanHeThong
        .Where(taiKhoan => taiKhoan.TrangThai.Contains("Đang hoạt động", StringComparison.OrdinalIgnoreCase)
            || taiKhoan.TrangThai.Contains("Phiên hiện tại", StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(taiKhoan => string.Equals(taiKhoan.TenDangNhap, TenDangNhap, StringComparison.OrdinalIgnoreCase))
        .ThenByDescending(taiKhoan => taiKhoan.LanDangNhapGanNhat);
    public bool DangXemTongQuan => MucDangChon == "Tổng quan";
    public bool CoQuyenTuyenDung => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenHoSoNhanVien => true;
    public bool CoQuyenQuanLyHoSoNhanVien => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenPhongBan => LaVaiTro("Admin", "Giám đốc");
    public bool CoQuyenChamCong => true;
    public bool CoQuyenNghiPhep => true;
    public bool CoQuyenDanhGia => true;
    public bool CoQuyenDieuChinhCong => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenDuyetNghiPhep => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenGhiNhanDanhGia => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenBangLuong => true;
    public bool CoQuyenXuLyBangLuong => LaVaiTro("Admin", "Giám đốc") || LaTruongPhongNhanSu;
    public bool CoQuyenBaoCaoNhanSu => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoQuyenCaiDatTaiKhoan => LaVaiTro("Admin");
    public bool CoQuyenTaoThongBao => LaVaiTro("Admin", "Giám đốc", "Trưởng phòng");
    public bool CoTepThongBaoMoi => !string.IsNullOrWhiteSpace(DuongDanTepThongBaoMoi);
    public string TenTepThongBaoHienThi => CoTepThongBaoMoi ? TenTepThongBaoMoi : "Chưa chọn tệp đính kèm";
    public Visibility HienThiBieuMauThongBao => DangMoBieuMauThongBao ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HienThiBieuMauThongBaoChung => DangMoBieuMauThongBao && MucDangChon != "Tổng quan" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HienThiBieuMauThongBaoTrungTam => DangMoBieuMauThongBao && MucDangChon == "Tổng quan" ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HienThiThongBaoNhanh => DangHienThongBaoNhanh ? Visibility.Visible : Visibility.Collapsed;
    public IReadOnlyList<string> CacPhanHeThongBao { get; } = ["Hệ thống", "Tuyển dụng", "Nhân viên", "Phòng ban", "Chấm công", "Nghỉ phép", "Đánh giá", "Bảng lương", "Báo cáo", "Cài đặt tài khoản"];
    public IReadOnlyList<string> CacMucDoThongBao { get; } = ["Thông tin", "Cảnh báo", "Khẩn cấp", "Thành công"];
    public IReadOnlyList<string> CacKyBaoCaoNhanSu { get; } = ["Ngày hôm nay", "Tháng này", "Quý này", "Năm nay", "Toàn bộ"];
    public IReadOnlyList<string> CacTrangThaiChamCong { get; } = [TatCaTrangThaiChamCong, "Đang trong ca", "Đủ công", "Thiếu giờ", "Tăng ca"];
    public IReadOnlyList<string> CacTrangThaiDanhGia { get; } = ["Nháp", "Đang đánh giá", "Hoàn tất"];
    public decimal TongLuongChoTra => DuLieu.PhieuLuong.Where(p => CoTheXemTheoTen(p.NhanVien) && !p.TrangThai.Contains("Đã trả", StringComparison.OrdinalIgnoreCase)).Sum(p => p.ThucLanh);
    public int SoPhieuLuongChoTra => DuLieu.PhieuLuong.Count(p => CoTheXemTheoTen(p.NhanVien) && !p.TrangThai.Contains("Đã trả", StringComparison.OrdinalIgnoreCase));
    public string NhanVienThaoTac => NhanVienDangChon is null ? "Chưa chọn nhân viên" : $"{NhanVienDangChon.HoTen} - {NhanVienDangChon.ViTri}";
    public decimal TongGioCongThangDangChon => NhanVienDangChon is null ? 0 : TinhTongGioCongThang(NhanVienDangChon);
    public decimal NgayCongQuyDoiDangChon => NhanVienDangChon is null ? 0 : TinhNgayCongQuyDoi(NhanVienDangChon);
    public decimal LuongCoBanDuKienDangChon => NhanVienDangChon is null ? 0 : LayLuongCoBan(NhanVienDangChon);
    public bool CoTheVaoCaDangChon => NhanVienDangChon is not null && CoTheThaoTacNhanVienDangChon() && !CoCaDangMo(NhanVienDangChon);
    public bool CoTheRaCaDangChon => NhanVienDangChon is not null && CoTheThaoTacNhanVienDangChon() && CoCaDangMo(NhanVienDangChon);
    public string TrangThaiCaDangChon => TaoMoTaTrangThaiCaDangChon();
    public string CaGanNhatDangChon => TaoMoTaCaGanNhatDangChon();
    public ObservableCollection<DiemLuongThang> DuLieuLuong12Thang { get => duLieuLuong12Thang; private set { duLieuLuong12Thang = value; BaoThayDoi(); } }
    public ObservableCollection<MucUngVienTheoViTri> DuLieuUngVienTheoViTri { get => duLieuUngVienTheoViTri; private set { duLieuUngVienTheoViTri = value; BaoThayDoi(); } }
    public ObservableCollection<DongTraCuuNhanSu> DanhSachNhanSuTraCuu { get => danhSachNhanSuTraCuu; private set { danhSachNhanSuTraCuu = value; BaoThayDoi(); } }
    public ObservableCollection<TongHopPhongBanDieuHanh> TongHopPhongBanDieuHanh { get => tongHopPhongBanDieuHanh; private set { tongHopPhongBanDieuHanh = value; BaoThayDoi(); } }
    public IReadOnlyList<TongHopLuongPhongBan> TongHopLuongTheoPhongBan => TaoTongHopLuongTheoPhongBan().ToList();
    public IReadOnlyList<NhanVien> NhanVienTrongPhamVi => SapXepNhanVienTheoUuTien(LayNhanVienTrongPhamVi()).ToList();
    public IReadOnlyList<PhongBan> PhongBanTrongPhamVi => LayPhongBanTrongPhamVi().OrderBy(p => p.TenPhongBan).ToList();
    public IReadOnlyList<ViTriCongViec> ViTriTrongPhamVi => DuLieu.ViTri.Where(v => PhongBanTrongPhamVi.Any(p => p.MaPhongBan == v.MaPhongBan)).OrderBy(v => v.TenViTri).ToList();
    public IReadOnlyList<string> CacPhongBanTraCuu { get => cacPhongBanTraCuu; private set { cacPhongBanTraCuu = value; BaoThayDoi(); } }
    public string PhongBanTraCuuDangChon
    {
        get => phongBanTraCuuDangChon;
        set
        {
            var giaTri = string.IsNullOrWhiteSpace(value) ? TatCaPhongBan : value;
            if (string.Equals(phongBanTraCuuDangChon, giaTri, StringComparison.Ordinal))
            {
                return;
            }

            phongBanTraCuuDangChon = giaTri;
            BaoThayDoi();
            CapNhatTraCuuDieuHanh();
        }
    }
    public string PhamViTraCuu => PhongBanTraCuuDangChon == TatCaPhongBan ? "Toàn cơ quan" : PhongBanTraCuuDangChon;
    public string PhamViDuLieuHienThi => LaToanQuyenDuLieu
        ? "Toàn hệ thống"
        : LaVaiTro("Trưởng phòng")
            ? $"Phòng ban phụ trách: {PhongBanNguoiDungHienTai}"
            : $"Cá nhân: {TenNguoiDung}";
    public int SoNhanSuTraCuu => DanhSachNhanSuTraCuu.Count;
    public decimal QuyLuongTraCuu => DanhSachNhanSuTraCuu.Sum(x => x.ThucLanh);
    public int SoNhanVienHoSo => DuLieu.NhanVien.Count(nhanVien => LocNhanVien(nhanVien));
    public string CaNhanXuatSacHoSoHienThi => LayCaNhanXuatSac(
        DuLieu.NhanVien.Where(nhanVien => LocNhanVien(nhanVien)).Select(nhanVien => nhanVien.HoTen))?.NhanVien ?? "Chưa có đánh giá";
    public string LuongCaoHoSoHienThi => TaoMoTaLuongCao(
        LayPhieuLuongCaoNhat(DuLieu.NhanVien.Where(nhanVien => LocNhanVien(nhanVien)).Select(nhanVien => nhanVien.HoTen)));
    public int SoPhieuLuongDaLoc => DuLieu.PhieuLuong.Count(LocPhieuLuong);
    public decimal TongThucLanhDaLoc => DuLieu.PhieuLuong.Where(LocPhieuLuong).Sum(p => p.ThucLanh);
    public string LuongCaoBangLuongHienThi => TaoMoTaLuongCao(LayPhieuLuongCaoNhat(DuLieu.PhieuLuong.Where(LocPhieuLuong).Select(p => p.NhanVien)));
    public int SoChamCongDaLoc => LayChamCongDaLoc().Count();
    public decimal TongGioChamCongDaLoc => LayChamCongDaLoc().Sum(TinhSoGioChamCong);
    public decimal TongNgayCongChamCongDaLoc => Math.Round(TongGioChamCongDaLoc / QuyTacNghiepVuNhanSu.SoGioMotNgayCong, 2);
    public int SoCaDangMoDaLoc => LayChamCongDaLoc().Count(c => c.GioRa is null);
    public int SoCaDuCongDaLoc => LayChamCongDaLoc().Count(c => string.Equals(c.TrangThaiCong, "Đủ công", StringComparison.OrdinalIgnoreCase));
    public int SoCaCanRaSoatDaLoc => LayChamCongDaLoc().Count(CanRaSoatChamCong);
    public string TyLeHoanTatChamCongDaLoc
    {
        get
        {
            var tong = SoChamCongDaLoc;
            if (tong == 0)
            {
                return "0%";
            }

            var daRaCa = LayChamCongDaLoc().Count(c => c.GioRa is not null);
            return $"{(decimal)daRaCa / tong:P0}";
        }
    }
    public string KhungThoiGianChamCong => TaoMoTaKhungThoiGianChamCong();
    public int SoChamCongHomNay => DuLieu.ChamCong.Count(c => LocChamCong(c) && c.GioVao.Date == DateTime.Today);
    public decimal TongGioCongHomNay => DuLieu.ChamCong.Where(c => LocChamCong(c) && c.GioVao.Date == DateTime.Today).Sum(TinhSoGioChamCong);
    public int SoChamCongTrongKyBaoCao => DuLieu.ChamCong.Count(c => LocChamCong(c) && ThuocKyBaoCao(c.GioVao));
    public decimal TongGioCongTrongKyBaoCao => DuLieu.ChamCong.Where(c => LocChamCong(c) && ThuocKyBaoCao(c.GioVao)).Sum(TinhSoGioChamCong);
    public int SoNghiPhepDaLoc => DuLieu.NghiPhep.Count(LocNghiPhepTheoBoLocManHinh);
    public int SoNghiPhepChoDuyetDaLoc => DuLieu.NghiPhep.Count(n => LocNghiPhepTheoBoLocManHinh(n) && n.DangChoDuyet);
    public int SoNghiPhepDaDuyetDaLoc => DuLieu.NghiPhep.Count(n => LocNghiPhepTheoBoLocManHinh(n) && QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(n.TrangThai));
    public decimal SoNgayNghiPhepDaLoc => DuLieu.NghiPhep.Where(LocNghiPhepTheoBoLocManHinh).Sum(n => n.SoNgay);
    public string MoTaBoLocNghiPhep => $"Kỳ {KyNghiPhepDangChon.ToLowerInvariant()}, phòng ban: {PhongBanNghiPhepDangChon}.";
    public int SoNghiPhepTrongKyBaoCao => DuLieu.NghiPhep.Count(n => LocNghiPhep(n) && NghiPhepThuocKyBaoCao(n));
    public int NhanSuNghiDaDuyetTrongKyBaoCao => LayTenNhanVienNghiDaDuyetTrongKyBaoCao().Count;
    public int QuanSoHienDienTrongKyBaoCao => Math.Max(0, NhanSuConHieuLuc - NhanSuNghiDaDuyetTrongKyBaoCao);
    public int SoPhieuLuongTrongKyBaoCao => DuLieu.PhieuLuong.Count(p => CoTheXemTheoTen(p.NhanVien) && ThuocKyBaoCaoLuong(p.KyLuong));
    public IReadOnlyList<NghiPhep> DanhSachNghiPhepDaDuyetTheoKy => LayNghiPhepDaDuyetTrongKyBaoCao().ToList();
    public string TieuDeAiDangNghi => $"Ai đang nghỉ - {KyBaoCaoNhanSuDangChon}";
    public string MoTaAiDangNghi
    {
        get
        {
            var danhSach = DanhSachNghiPhepDaDuyetTheoKy;
            if (danhSach.Count == 0)
            {
                return "Chưa có nhân sự nghỉ đã duyệt trong kỳ này.";
            }

            var soNhanSu = danhSach.Select(x => x.NhanVien).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            return $"{soNhanSu} nhân sự, {danhSach.Count} lượt nghỉ đã duyệt.";
        }
    }
    public string NhanVienCongCaoHienThi => DuLieu.ChamCong
        .Where(LocChamCong)
        .GroupBy(c => c.NhanVien, StringComparer.OrdinalIgnoreCase)
        .Select(g => new { NhanVien = g.Key, TongGio = g.Sum(TinhSoGioChamCong) })
        .OrderByDescending(x => x.TongGio)
        .ThenBy(x => x.NhanVien)
        .Select(x => $"{x.NhanVien} - {x.TongGio:N2} giờ")
        .FirstOrDefault() ?? "Chưa có công";
    public DongTraCuuNhanSu? CaNhanXuatSacNhat => DanhSachNhanSuTraCuu
        .Where(x => x.DiemDanhGia.HasValue)
        .OrderByDescending(x => x.DiemDanhGia)
        .ThenBy(x => x.ThuTuCapBac)
        .FirstOrDefault();
    public string CaNhanXuatSacHienThi => CaNhanXuatSacNhat?.HoTen ?? "Chưa có đánh giá";
    public string ThanhTichXuatSacHienThi => CaNhanXuatSacNhat is null
        ? "Chưa có điểm trong phạm vi chọn"
        : $"{CaNhanXuatSacNhat.DiemDanhGia:N1} điểm - {CaNhanXuatSacNhat.ViTri}";
    public DongTraCuuNhanSu? NhanSuLuongCaoNhat => DanhSachNhanSuTraCuu
        .Where(x => x.ThucLanh > 0)
        .OrderByDescending(x => x.ThucLanh)
        .ThenBy(x => x.ThuTuCapBac)
        .FirstOrDefault();
    public string NhanSuLuongCaoHienThi => NhanSuLuongCaoNhat?.HoTen ?? "Chưa có bảng lương";
    public string LuongCaoHienThi => NhanSuLuongCaoNhat is null
        ? "Chưa có dữ liệu kỳ lương"
        : $"{NhanSuLuongCaoNhat.ThucLanh:N0} đ - {NhanSuLuongCaoNhat.ViTri}";
    public string KyLuongGanNhatHienThi => DuLieu.PhieuLuong
        .Where(p => CoTheXemTheoTen(p.NhanVien))
        .OrderByDescending(p => p.KyLuong)
        .Select(p => p.KyLuong)
        .FirstOrDefault() ?? "Chưa có kỳ lương";
    public string TomTatBaoCaoNhanSu =>
        $"{KyBaoCaoNhanSuDangChon}: quân số hiện diện {QuanSoHienDienTrongKyBaoCao}/{NhanSuConHieuLuc}, {NhanSuNghiDaDuyetTrongKyBaoCao} nhân sự nghỉ đã duyệt, {SoChamCongTrongKyBaoCao} lượt chấm công, {SoNghiPhepTrongKyBaoCao} đơn nghỉ, {SoPhieuLuongTrongKyBaoCao} phiếu lương; còn {NghiChoDuyet} đơn chờ duyệt.";
    public string TieuDeManHinh => MucDangChon == "Tổng quan" ? "Tổng quan nhân sự" : MucDangChon;

    public async Task TaiDuLieu()
    {
        var maNhanVienDangChon = NhanVienDangChon?.MaNhanVien;
        var tenNhanVienDangChon = NhanVienDangChon?.HoTen;
        var ketQua = await khoDuLieu.TaiDuLieuAsync();
        DuLieu = ketQua.DuLieu;
        NhapThongBaoPhienLamViec();
        ApDungTrangThaiDocThongBao();
        NguonDuLieu = ketQua.NguonDuLieu;
        CauHinhDanhSachNhanVienView();
        DanhSachThongBaoView = CollectionViewSource.GetDefaultView(DuLieu.ThongBao);
        DanhSachThongBaoView.Filter = LocThongBao;
        BaoThayDoi(nameof(DanhSachThongBaoView));
        DanhSachUngVienView = CollectionViewSource.GetDefaultView(DuLieu.UngVien);
        DanhSachUngVienView.Filter = obj => obj is UngVien ungVien && LocUngVien(ungVien);
        BaoThayDoi(nameof(DanhSachUngVienView));
        DanhSachPhieuLuongView = CollectionViewSource.GetDefaultView(DuLieu.PhieuLuong);
        DanhSachPhieuLuongView.Filter = obj => obj is PhieuLuong phieuLuong && LocPhieuLuong(phieuLuong);
        DanhSachPhieuLuongView.SortDescriptions.Clear();
        DanhSachPhieuLuongView.SortDescriptions.Add(new SortDescription(nameof(PhieuLuong.KyLuong), ListSortDirection.Descending));
        DanhSachPhieuLuongView.SortDescriptions.Add(new SortDescription(nameof(PhieuLuong.NhanVien), ListSortDirection.Ascending));
        BaoThayDoi(nameof(DanhSachPhieuLuongView));
        DanhSachChamCongView = CollectionViewSource.GetDefaultView(DuLieu.ChamCong);
        DanhSachChamCongView.Filter = obj => obj is ChamCong chamCong && LocChamCong(chamCong);
        DanhSachChamCongView.SortDescriptions.Clear();
        DanhSachChamCongView.SortDescriptions.Add(new SortDescription(nameof(ChamCong.GioVao), ListSortDirection.Descending));
        DanhSachChamCongView.SortDescriptions.Add(new SortDescription(nameof(ChamCong.NhanVien), ListSortDirection.Ascending));
        BaoThayDoi(nameof(DanhSachChamCongView));
        CauHinhDanhSachNghiPhepView();
        DanhSachDanhGiaView = CollectionViewSource.GetDefaultView(DuLieu.DanhGia);
        DanhSachDanhGiaView.Filter = obj => obj is DanhGia danhGia && LocDanhGia(danhGia);
        DanhSachDanhGiaView.SortDescriptions.Clear();
        DanhSachDanhGiaView.SortDescriptions.Add(new SortDescription(nameof(DanhGia.KyDanhGia), ListSortDirection.Descending));
        DanhSachDanhGiaView.SortDescriptions.Add(new SortDescription(nameof(DanhGia.NhanVien), ListSortDirection.Ascending));
        BaoThayDoi(nameof(DanhSachDanhGiaView));
        BaoCaoThongKe();
        LamMoiThongBao();
        await TaiDanhSachTaiKhoan();
        TaoMoiNhanVien(khoiTaoNoiBo: true);
        TaoMoiPhongBan();
        KhoiTaoBieuMauNghiepVu();
        NhanVienDangChon = ChonNhanVienTrongPhamVi(maNhanVienDangChon, tenNhanVienDangChon);
    }

    private void CauHinhDanhSachNhanVienView()
    {
        DanhSachNhanVienView = CollectionViewSource.GetDefaultView(DuLieu.NhanVien);
        DanhSachNhanVienView.Filter = LocNhanVien;
        DanhSachNhanVienView.SortDescriptions.Clear();
        if (DanhSachNhanVienView is ListCollectionView danhSach)
        {
            danhSach.CustomSort = new SoSanhNhanVienTheoUuTien(this);
        }
        else
        {
            DanhSachNhanVienView.SortDescriptions.Add(new SortDescription(nameof(NhanVien.ThuTuChucVu), ListSortDirection.Ascending));
            DanhSachNhanVienView.SortDescriptions.Add(new SortDescription(nameof(NhanVien.PhongBan), ListSortDirection.Ascending));
            DanhSachNhanVienView.SortDescriptions.Add(new SortDescription(nameof(NhanVien.HoTen), ListSortDirection.Ascending));
        }

        BaoThayDoi(nameof(DanhSachNhanVienView));
    }

    private void CauHinhDanhSachNghiPhepView()
    {
        CapNhatThongTinHienThiNghiPhep();
        DanhSachNghiPhepView = CollectionViewSource.GetDefaultView(DuLieu.NghiPhep);
        DanhSachNghiPhepView.Filter = obj => obj is NghiPhep nghiPhep && LocNghiPhepTheoBoLocManHinh(nghiPhep);
        DanhSachNghiPhepView.SortDescriptions.Clear();
        DanhSachNghiPhepView.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.ThuTuTrangThai), ListSortDirection.Ascending));
        DanhSachNghiPhepView.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.ThuTuMoiNhat), ListSortDirection.Descending));
        DanhSachNghiPhepView.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.TuNgay), ListSortDirection.Descending));
        DanhSachNghiPhepView.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.DenNgay), ListSortDirection.Descending));
        DanhSachNghiPhepView.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.NhanVien), ListSortDirection.Ascending));
        BaoThayDoi(nameof(DanhSachNghiPhepView));

        DanhSachNghiPhepDaDuyetView = new ListCollectionView(DuLieu.NghiPhep);
        DanhSachNghiPhepDaDuyetView.Filter = obj => obj is NghiPhep nghiPhep
            && LocNghiPhepTheoBoLocManHinh(nghiPhep)
            && QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai);
        SapXepDanhSachNghiPhep(DanhSachNghiPhepDaDuyetView);
        BaoThayDoi(nameof(DanhSachNghiPhepDaDuyetView));

        DanhSachDeNghiNghiPhepView = new ListCollectionView(DuLieu.NghiPhep);
        DanhSachDeNghiNghiPhepView.Filter = obj => obj is NghiPhep nghiPhep
            && LocNghiPhepTheoBoLocManHinh(nghiPhep)
            && nghiPhep.DangChoDuyet;
        SapXepDanhSachNghiPhep(DanhSachDeNghiNghiPhepView);
        BaoThayDoi(nameof(DanhSachDeNghiNghiPhepView));
    }

    private static void SapXepDanhSachNghiPhep(ICollectionView danhSach)
    {
        danhSach.SortDescriptions.Clear();
        danhSach.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.ThuTuMoiNhat), ListSortDirection.Descending));
        danhSach.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.TuNgay), ListSortDirection.Descending));
        danhSach.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.DenNgay), ListSortDirection.Descending));
        danhSach.SortDescriptions.Add(new SortDescription(nameof(NghiPhep.NhanVien), ListSortDirection.Ascending));
    }

    private NhanVien? ChonNhanVienTrongPhamVi(int? maNhanVienDangChon = null, string? tenNhanVienDangChon = null)
    {
        var danhSach = NhanVienTrongPhamVi;
        return danhSach.FirstOrDefault(n => n.MaNhanVien == maNhanVienDangChon)
            ?? danhSach.FirstOrDefault(n => string.Equals(n.HoTen, tenNhanVienDangChon, StringComparison.OrdinalIgnoreCase))
            ?? danhSach.FirstOrDefault(n => string.Equals(n.HoTen, TenNguoiDung, StringComparison.OrdinalIgnoreCase))
            ?? danhSach.FirstOrDefault();
    }

    private bool LaToanQuyenDuLieu => LaVaiTro("Admin", "Giám đốc");

    private bool LaTruongPhongNhanSu => LaVaiTro("Trưởng phòng")
        && PhongBanNguoiDungHienTai.Contains("Nhân sự", StringComparison.OrdinalIgnoreCase);

    private string? MaNhanVienMacDinhTheoTaiKhoan => TenDangNhap.ToLowerInvariant() switch
    {
        "giamdoc" => "GD001",
        "truongphong" => "TP003",
        "nhanvien" => "NV001",
        _ => null
    };

    private NhanVien? NhanVienNguoiDungHienTai => DuLieu.NhanVien.FirstOrDefault(n => string.Equals(n.MaSo, MaNhanVienMacDinhTheoTaiKhoan, StringComparison.OrdinalIgnoreCase))
        ?? DuLieu.NhanVien.FirstOrDefault(n => string.Equals(n.MaSo, TenDangNhap, StringComparison.OrdinalIgnoreCase))
        ?? DuLieu.NhanVien.FirstOrDefault(n => string.Equals(n.HoTen, TenNguoiDung, StringComparison.OrdinalIgnoreCase));

    private string PhongBanNguoiDungHienTai => NhanVienNguoiDungHienTai?.PhongBan
        ?? DuLieu.PhongBan.FirstOrDefault(p => string.Equals(p.TruongPhong, TenNguoiDung, StringComparison.OrdinalIgnoreCase))?.TenPhongBan
        ?? "Chưa xác định";

    private IEnumerable<NhanVien> LayNhanVienTrongPhamVi() => DuLieu.NhanVien.Where(CoTheXemNhanVien);

    private IEnumerable<NhanVien> SapXepNhanVienTheoUuTien(IEnumerable<NhanVien> danhSach)
    {
        return danhSach
            .OrderBy(nhanVien => LaTruongPhongChinhThuc(nhanVien) ? 0 : 1)
            .ThenBy(nhanVien => LayThuTuPhongBan(nhanVien.MaPhongBan, nhanVien.PhongBan))
            .ThenBy(nhanVien => nhanVien.ThuTuChucVu)
            .ThenBy(nhanVien => nhanVien.HoTen);
    }

    internal int SoSanhNhanVien(NhanVien? trai, NhanVien? phai)
    {
        if (ReferenceEquals(trai, phai))
        {
            return 0;
        }

        if (trai is null)
        {
            return 1;
        }

        if (phai is null)
        {
            return -1;
        }

        var ketQua = (LaTruongPhongChinhThuc(trai) ? 0 : 1).CompareTo(LaTruongPhongChinhThuc(phai) ? 0 : 1);
        if (ketQua != 0) return ketQua;

        ketQua = LayThuTuPhongBan(trai.MaPhongBan, trai.PhongBan).CompareTo(LayThuTuPhongBan(phai.MaPhongBan, phai.PhongBan));
        if (ketQua != 0) return ketQua;

        ketQua = trai.ThuTuChucVu.CompareTo(phai.ThuTuChucVu);
        if (ketQua != 0) return ketQua;

        return string.Compare(trai.HoTen, phai.HoTen, StringComparison.CurrentCultureIgnoreCase);
    }

    private bool LaTruongPhongChinhThuc(NhanVien nhanVien)
    {
        return LaTruongPhongChinhThuc(nhanVien.HoTen, nhanVien.MaPhongBan, nhanVien.PhongBan);
    }

    private bool LaTruongPhongChinhThuc(string hoTen, string tenPhongBan)
    {
        var phongBan = DuLieu.PhongBan.FirstOrDefault(p => string.Equals(p.TenPhongBan, tenPhongBan, StringComparison.OrdinalIgnoreCase));
        return phongBan is not null && LaTruongPhongChinhThuc(hoTen, phongBan.MaPhongBan, tenPhongBan);
    }

    private bool LaTruongPhongChinhThuc(string hoTen, int maPhongBan, string tenPhongBan)
    {
        var phongBan = DuLieu.PhongBan.FirstOrDefault(p => p.MaPhongBan == maPhongBan)
            ?? DuLieu.PhongBan.FirstOrDefault(p => string.Equals(p.TenPhongBan, tenPhongBan, StringComparison.OrdinalIgnoreCase));
        return phongBan is not null
            && !string.IsNullOrWhiteSpace(phongBan.TruongPhong)
            && !phongBan.TruongPhong.Contains("Chưa phân công", StringComparison.OrdinalIgnoreCase)
            && string.Equals(phongBan.TruongPhong, hoTen, StringComparison.OrdinalIgnoreCase);
    }

    private int LayThuTuPhongBan(int maPhongBan, string tenPhongBan)
    {
        for (var i = 0; i < DuLieu.PhongBan.Count; i++)
        {
            var phongBan = DuLieu.PhongBan[i];
            if (phongBan.MaPhongBan == maPhongBan
                || string.Equals(phongBan.TenPhongBan, tenPhongBan, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 10_000 + maPhongBan;
    }

    private int LayThuTuPhongBan(string tenPhongBan)
    {
        return LayThuTuPhongBan(0, tenPhongBan);
    }

    private IEnumerable<PhongBan> LayPhongBanTrongPhamVi()
    {
        if (LaToanQuyenDuLieu)
        {
            return DuLieu.PhongBan;
        }

        var phongBanNguoiDung = PhongBanNguoiDungHienTai;
        return DuLieu.PhongBan.Where(p => string.Equals(p.TenPhongBan, phongBanNguoiDung, StringComparison.OrdinalIgnoreCase));
    }

    private bool CoTheXemNhanVien(NhanVien nhanVien)
    {
        if (LaToanQuyenDuLieu)
        {
            return true;
        }

        var nhanVienNguoiDung = NhanVienNguoiDungHienTai;
        if (nhanVienNguoiDung is not null && nhanVien.MaNhanVien == nhanVienNguoiDung.MaNhanVien)
        {
            return true;
        }

        if (nhanVienNguoiDung is null && string.Equals(nhanVien.HoTen, TenNguoiDung, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return LaVaiTro("Trưởng phòng")
            && string.Equals(nhanVien.PhongBan, PhongBanNguoiDungHienTai, StringComparison.OrdinalIgnoreCase);
    }

    private bool CoTheXemTheoTen(string tenNhanVien)
    {
        if (LaToanQuyenDuLieu)
        {
            return true;
        }

        var nhanViensCungTen = DuLieu.NhanVien
            .Where(nhanVien => string.Equals(nhanVien.HoTen, tenNhanVien, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return nhanViensCungTen.Count > 0
            ? nhanViensCungTen.All(CoTheXemNhanVien)
            : NhanVienNguoiDungHienTai is null && string.Equals(tenNhanVien, TenNguoiDung, StringComparison.OrdinalIgnoreCase);
    }

    private bool CoTheXemPhongBan(string tenPhongBan)
    {
        return LaToanQuyenDuLieu || string.Equals(tenPhongBan, PhongBanNguoiDungHienTai, StringComparison.OrdinalIgnoreCase);
    }

    private bool LocUngVien(UngVien ungVien)
    {
        if (LaUngVienDaLaNhanVien(ungVien))
        {
            return false;
        }

        var viTri = DuLieu.ViTri.FirstOrDefault(v => string.Equals(v.TenViTri, ungVien.ViTri, StringComparison.OrdinalIgnoreCase));
        var phongBan = DuLieu.PhongBan.FirstOrDefault(p => p.MaPhongBan == viTri?.MaPhongBan);
        return phongBan is null || CoTheXemPhongBan(phongBan.TenPhongBan);
    }

    private bool LaUngVienDaLaNhanVien(UngVien ungVien)
    {
        return ungVien.GiaiDoan is "Đã tiếp nhận" or "Đã ký" or "Signed"
            || DuLieu.NhanVien.Any(nhanVien => string.Equals(nhanVien.HoTen.Trim(), ungVien.HoTen.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private bool CoTheThaoTacNhanVienDangChon() => NhanVienDangChon is not null && CoTheXemNhanVien(NhanVienDangChon);

    private bool CoTheSuaNhanVienDangChon() => CoQuyenQuanLyHoSoNhanVien && CoTheThaoTacNhanVienDangChon();

    private bool LocNhanVien(object obj)
    {
        if (obj is not NhanVien nv)
        {
            return false;
        }

        if (!CoTheXemNhanVien(nv))
        {
            return false;
        }

        if (!ThuocPhongBan(nv.PhongBan, PhongBanNhanVienDangChon))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(TuKhoaTimKiem))
        {
            return true;
        }

        var tuKhoa = TuKhoaTimKiem.Trim();
        return ChuaTuKhoa(nv.HoTen, tuKhoa)
            || ChuaTuKhoa(nv.MaSo, tuKhoa)
            || ChuaTuKhoa(nv.PhongBan, tuKhoa)
            || ChuaTuKhoa(nv.ViTri, tuKhoa)
            || ChuaTuKhoa(nv.CapBacChucVu, tuKhoa);
    }

    private bool LocPhieuLuong(PhieuLuong phieuLuong)
    {
        if (!CoTheXemTheoTen(phieuLuong.NhanVien))
        {
            return false;
        }

        var nhanVien = LayNhanVienTheoTen(phieuLuong.NhanVien);
        var phongBan = nhanVien?.PhongBan ?? "";
        var viTri = nhanVien?.ViTri ?? "";

        if (!ThuocPhongBan(phongBan, PhongBanBangLuongDangChon))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(TuKhoaBangLuong))
        {
            return true;
        }

        var tuKhoa = TuKhoaBangLuong.Trim();
        return ChuaTuKhoa(phieuLuong.NhanVien, tuKhoa)
            || ChuaTuKhoa(phongBan, tuKhoa)
            || ChuaTuKhoa(viTri, tuKhoa)
            || ChuaTuKhoa(phieuLuong.KyLuong, tuKhoa)
            || ChuaTuKhoa(phieuLuong.TrangThai, tuKhoa);
    }

    private bool LocChamCong(ChamCong chamCong)
    {
        if (!CoTheXemTheoTen(chamCong.NhanVien))
        {
            return false;
        }

        var nhanVien = LayNhanVienTheoTen(chamCong.NhanVien);
        var phongBan = nhanVien?.PhongBan ?? "";
        var viTri = nhanVien?.ViTri ?? "";

        if (TuNgayChamCong.HasValue && chamCong.GioVao.Date < TuNgayChamCong.Value.Date)
        {
            return false;
        }

        if (DenNgayChamCong.HasValue && chamCong.GioVao.Date > DenNgayChamCong.Value.Date)
        {
            return false;
        }

        if (!ThuocPhongBan(phongBan, PhongBanChamCongDangChon))
        {
            return false;
        }

        if (!string.Equals(TrangThaiChamCongDangChon, TatCaTrangThaiChamCong, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(chamCong.TrangThaiCong, TrangThaiChamCongDangChon, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(TuKhoaChamCong))
        {
            return true;
        }

        var tuKhoa = TuKhoaChamCong.Trim();
        return ChuaTuKhoa(chamCong.NhanVien, tuKhoa)
            || ChuaTuKhoa(phongBan, tuKhoa)
            || ChuaTuKhoa(viTri, tuKhoa)
            || ChuaTuKhoa(chamCong.CaLam, tuKhoa)
            || ChuaTuKhoa(chamCong.TrangThaiCong, tuKhoa);
    }

    private bool LocNghiPhep(NghiPhep nghiPhep)
    {
        return CoTheXemTheoTen(nghiPhep.NhanVien);
    }

    private bool LocNghiPhepTheoBoLocManHinh(NghiPhep nghiPhep)
    {
        if (!LocNghiPhep(nghiPhep) || !NghiPhepThuocKy(nghiPhep, KyNghiPhepDangChon))
        {
            return false;
        }

        if (string.Equals(PhongBanNghiPhepDangChon, TatCaPhongBan, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var nhanVien = LayNhanVienTheoTen(nghiPhep.NhanVien);
        return nhanVien is not null && ThuocPhongBan(nhanVien.PhongBan, PhongBanNghiPhepDangChon);
    }

    private void CapNhatThongTinHienThiNghiPhep()
    {
        foreach (var nghiPhep in DuLieu.NghiPhep)
        {
            var nhanVien = LayNhanVienTheoTen(nghiPhep.NhanVien);
            nghiPhep.ChucVuPhongBan = nhanVien is null
                ? "Chưa rõ"
                : $"{nhanVien.ViTri} / {nhanVien.PhongBan}";
        }
    }

    private bool CoTheXuLyNghiPhep(object? thamSo)
    {
        return thamSo is NghiPhep nghiPhep
            && CoQuyenDuyetNghiPhep
            && LocNghiPhep(nghiPhep)
            && nghiPhep.DangChoDuyet;
    }

    private bool LocDanhGia(DanhGia danhGia)
    {
        if (!CoTheXemTheoTen(danhGia.NhanVien) && !CoTheXemTheoTen(danhGia.NguoiDanhGia))
        {
            return false;
        }

        var nhanVien = LayNhanVienTheoTen(danhGia.NhanVien);
        var phongBan = nhanVien?.PhongBan ?? "";
        var viTri = nhanVien?.ViTri ?? "";

        if (!ThuocPhongBan(phongBan, PhongBanDanhGiaDangChon))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(TuKhoaDanhGia))
        {
            return true;
        }

        var tuKhoa = TuKhoaDanhGia.Trim();
        return ChuaTuKhoa(danhGia.NhanVien, tuKhoa)
            || ChuaTuKhoa(danhGia.NguoiDanhGia, tuKhoa)
            || ChuaTuKhoa(phongBan, tuKhoa)
            || ChuaTuKhoa(viTri, tuKhoa)
            || ChuaTuKhoa(danhGia.KyDanhGia, tuKhoa)
            || ChuaTuKhoa(danhGia.TrangThai, tuKhoa);
    }

    private bool ThuocPhongBan(string phongBan, string boLoc)
    {
        return string.Equals(boLoc, TatCaPhongBan, StringComparison.OrdinalIgnoreCase)
            || string.Equals(phongBan, boLoc, StringComparison.OrdinalIgnoreCase);
    }

    private bool ThuocKyBaoCao(DateTime ngay)
    {
        return ThuocKy(ngay, KyBaoCaoNhanSuDangChon);
    }

    private (DateTime TuNgay, DateTime DenNgay)? LayKhoangKyBaoCao()
    {
        return LayKhoangKy(KyBaoCaoNhanSuDangChon);
    }

    private bool ThuocKy(DateTime ngay, string ky)
    {
        var homNay = DateTime.Today;
        return ky switch
        {
            "Ngày hôm nay" => ngay.Date == homNay,
            "Tháng này" => ngay.Year == homNay.Year && ngay.Month == homNay.Month,
            "Quý này" => ngay.Year == homNay.Year && Quy(ngay) == Quy(homNay),
            "Năm nay" => ngay.Year == homNay.Year,
            _ => true
        };
    }

    private (DateTime TuNgay, DateTime DenNgay)? LayKhoangKy(string ky)
    {
        var homNay = DateTime.Today;
        return ky switch
        {
            "Ngày hôm nay" => (homNay, homNay),
            "Tháng này" => (new DateTime(homNay.Year, homNay.Month, 1), new DateTime(homNay.Year, homNay.Month, DateTime.DaysInMonth(homNay.Year, homNay.Month))),
            "Quý này" => (new DateTime(homNay.Year, (Quy(homNay) - 1) * 3 + 1, 1), new DateTime(homNay.Year, Quy(homNay) * 3, DateTime.DaysInMonth(homNay.Year, Quy(homNay) * 3))),
            "Năm nay" => (new DateTime(homNay.Year, 1, 1), new DateTime(homNay.Year, 12, 31)),
            _ => null
        };
    }

    private bool NghiPhepThuocKyBaoCao(NghiPhep nghiPhep)
    {
        return NghiPhepThuocKy(nghiPhep, KyBaoCaoNhanSuDangChon);
    }

    private bool NghiPhepThuocKy(NghiPhep nghiPhep, string ky)
    {
        var khoang = LayKhoangKy(ky);
        return khoang is null
            || QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, khoang.Value.TuNgay, khoang.Value.DenNgay);
    }

    private HashSet<string> LayTenNhanVienNghiDaDuyetNgay(DateTime ngay)
    {
        return DuLieu.NghiPhep
            .Where(LocNghiPhep)
            .Where(nghiPhep => QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai))
            .Where(nghiPhep => QuyTacNghiepVuNhanSu.NghiPhepGiaoNgay(nghiPhep, ngay))
            .Select(nghiPhep => nghiPhep.NhanVien)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private HashSet<string> LayTenNhanVienNghiDaDuyetTrongKyBaoCao()
    {
        var khoang = LayKhoangKyBaoCao();
        var truyVan = DuLieu.NghiPhep
            .Where(LocNghiPhep)
            .Where(nghiPhep => QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai));

        if (khoang is not null)
        {
            truyVan = truyVan.Where(nghiPhep => QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, khoang.Value.TuNgay, khoang.Value.DenNgay));
        }

        return truyVan.Select(nghiPhep => nghiPhep.NhanVien).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<NghiPhep> LayNghiPhepDaDuyetTrongKyBaoCao()
    {
        var khoang = LayKhoangKyBaoCao();
        var truyVan = DuLieu.NghiPhep
            .Where(LocNghiPhep)
            .Where(nghiPhep => QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai));

        if (khoang is not null)
        {
            truyVan = truyVan.Where(nghiPhep => QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, khoang.Value.TuNgay, khoang.Value.DenNgay));
        }

        return truyVan
            .OrderBy(nghiPhep => nghiPhep.TuNgay)
            .ThenBy(nghiPhep => nghiPhep.NhanVien);
    }

    private bool ThuocKyBaoCaoLuong(string kyLuong)
    {
        return DateTime.TryParse($"{kyLuong}-01", out var ngayLuong) && ThuocKyBaoCao(ngayLuong);
    }

    private bool ThuocKyTongHopLuong(string kyLuong)
    {
        return DateTime.TryParse($"{kyLuong}-01", out var ngayLuong) && ThuocKy(ngayLuong, KyTongHopLuongDangChon);
    }

    private IEnumerable<TongHopLuongPhongBan> TaoTongHopLuongTheoPhongBan()
    {
        return DuLieu.PhieuLuong
            .Where(phieuLuong => LocPhieuLuong(phieuLuong) && ThuocKyTongHopLuong(phieuLuong.KyLuong))
            .Select(phieuLuong => new { PhieuLuong = phieuLuong, NhanVien = LayNhanVienTheoTen(phieuLuong.NhanVien) })
            .GroupBy(x => x.NhanVien?.PhongBan ?? "Chưa rõ", StringComparer.OrdinalIgnoreCase)
            .Select(g => new TongHopLuongPhongBan(
                g.Key,
                KyTongHopLuongDangChon,
                g.Count(),
                g.Sum(x => x.PhieuLuong.LuongCoBan),
                g.Sum(x => x.PhieuLuong.PhuCap),
                g.Sum(x => x.PhieuLuong.KhauTru),
                g.Sum(x => x.PhieuLuong.ThucLanh)))
            .OrderByDescending(x => x.TongThucLanh)
            .ThenBy(x => x.PhongBan);
    }

    private static int Quy(DateTime ngay) => ((ngay.Month - 1) / 3) + 1;

    private NhanVien? LayNhanVienTheoTen(string hoTen)
    {
        return DuLieu.NhanVien.FirstOrDefault(x => string.Equals(x.HoTen, hoTen, StringComparison.OrdinalIgnoreCase));
    }

    private string LayTenNhanVienTheoMa(int maNhanVien)
    {
        return DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien)?.HoTen ?? "";
    }

    private IEnumerable<ChamCong> LayChamCongDaLoc()
    {
        return DuLieu.ChamCong.Where(LocChamCong);
    }

    private static decimal TinhSoGioChamCong(ChamCong chamCong)
    {
        return chamCong.SoGioTinhToan;
    }

    private static bool CanRaSoatChamCong(ChamCong chamCong)
    {
        if (chamCong.GioRa is null)
        {
            return chamCong.GioVao.Date < DateTime.Today;
        }

        var soGio = TinhSoGioChamCong(chamCong);
        return soGio is > 0 and < 4m || soGio > 12m;
    }

    private bool CoCaDangMo(NhanVien nhanVien)
    {
        return DuLieu.ChamCong.Any(chamCong =>
            string.Equals(chamCong.NhanVien, nhanVien.HoTen, StringComparison.OrdinalIgnoreCase)
            && chamCong.GioRa is null);
    }

    private string TaoMoTaTrangThaiCaDangChon()
    {
        if (NhanVienDangChon is null)
        {
            return "Chưa chọn nhân viên thao tác";
        }

        var caDangMo = DuLieu.ChamCong
            .Where(chamCong => string.Equals(chamCong.NhanVien, NhanVienDangChon.HoTen, StringComparison.OrdinalIgnoreCase) && chamCong.GioRa is null)
            .OrderByDescending(chamCong => chamCong.GioVao)
            .FirstOrDefault();

        if (caDangMo is not null)
        {
            return $"Đang trong ca từ {caDangMo.GioVao:HH:mm dd/MM}";
        }

        var caHomNay = DuLieu.ChamCong
            .Where(chamCong => string.Equals(chamCong.NhanVien, NhanVienDangChon.HoTen, StringComparison.OrdinalIgnoreCase) && chamCong.GioVao.Date == DateTime.Today)
            .OrderByDescending(chamCong => chamCong.GioVao)
            .FirstOrDefault();

        return caHomNay is null
            ? "Chưa vào ca hôm nay"
            : $"Đã ra ca {caHomNay.GioRa?.ToString("HH:mm") ?? "--"} - {TinhSoGioChamCong(caHomNay):N2} giờ";
    }

    private string TaoMoTaCaGanNhatDangChon()
    {
        if (NhanVienDangChon is null)
        {
            return "Ca gần nhất: chưa có dữ liệu";
        }

        var caGanNhat = DuLieu.ChamCong
            .Where(chamCong => string.Equals(chamCong.NhanVien, NhanVienDangChon.HoTen, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(chamCong => chamCong.GioVao)
            .FirstOrDefault();

        return caGanNhat is null
            ? "Ca gần nhất: chưa có dữ liệu"
            : $"Ca gần nhất: {caGanNhat.GioVao:dd/MM HH:mm} - {caGanNhat.TrangThaiCong}";
    }

    private string TaoMoTaKhungThoiGianChamCong()
    {
        return (TuNgayChamCong, DenNgayChamCong) switch
        {
            ({ } tuNgay, { } denNgay) => $"Từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}",
            ({ } tuNgay, null) => $"Từ {tuNgay:dd/MM/yyyy}",
            (null, { } denNgay) => $"Đến {denNgay:dd/MM/yyyy}",
            _ => "Toàn bộ thời gian"
        };
    }

    private DanhGia? LayCaNhanXuatSac(IEnumerable<string> tenNhanVien)
    {
        var tapTen = tenNhanVien.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return DuLieu.DanhGia
            .Where(x => tapTen.Contains(x.NhanVien))
            .OrderByDescending(x => x.Diem)
            .ThenBy(x => LayNhanVienTheoTen(x.NhanVien)?.ThuTuChucVu ?? 99)
            .FirstOrDefault();
    }

    private PhieuLuong? LayPhieuLuongCaoNhat(IEnumerable<string> tenNhanVien)
    {
        var tapTen = tenNhanVien.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return DuLieu.PhieuLuong
            .Where(x => tapTen.Contains(x.NhanVien) && x.ThucLanh > 0)
            .OrderByDescending(x => x.ThucLanh)
            .ThenBy(x => LayNhanVienTheoTen(x.NhanVien)?.ThuTuChucVu ?? 99)
            .FirstOrDefault();
    }

    private static string TaoMoTaLuongCao(PhieuLuong? phieuLuong)
    {
        return phieuLuong is null ? "Chưa có bảng lương" : $"{phieuLuong.NhanVien} - {phieuLuong.ThucLanh:N0} đ";
    }

    private void LamMoiBoLocNhanVien()
    {
        DanhSachNhanVienView.Refresh();
        BaoThayDoi(nameof(NhanVienTrongPhamVi));
        BaoThayDoi(nameof(PhongBanTrongPhamVi));
        BaoThayDoi(nameof(ViTriTrongPhamVi));
        BaoThayDoi(nameof(PhamViDuLieuHienThi));
        BaoThayDoi(nameof(NhanSuConHieuLuc));
        BaoThayDoi(nameof(NghiPhepDaDuyetHomNay));
        BaoThayDoi(nameof(NhanSuTamVangHomNay));
        BaoThayDoi(nameof(DangLamViec));
        BaoThayDoi(nameof(TamNghi));
        BaoThayDoi(nameof(CoQuyenQuanLyHoSoNhanVien));
        BaoThayDoi(nameof(CoQuyenXuLyBangLuong));
        BaoThayDoi(nameof(SoNhanVienHoSo));
        BaoThayDoi(nameof(CaNhanXuatSacHoSoHienThi));
        BaoThayDoi(nameof(LuongCaoHoSoHienThi));
    }

    private void LamMoiBoLocBangLuong()
    {
        DanhSachPhieuLuongView.Refresh();
        BaoThayDoi(nameof(SoPhieuLuongDaLoc));
        BaoThayDoi(nameof(TongThucLanhDaLoc));
        BaoThayDoi(nameof(LuongCaoBangLuongHienThi));
        BaoThayDoi(nameof(TongHopLuongTheoPhongBan));
    }

    private void LamMoiBoLocChamCong()
    {
        DanhSachChamCongView.Refresh();
        BaoThayDoi(nameof(SoChamCongDaLoc));
        BaoThayDoi(nameof(TongGioChamCongDaLoc));
        BaoThayDoi(nameof(TongNgayCongChamCongDaLoc));
        BaoThayDoi(nameof(SoCaDangMoDaLoc));
        BaoThayDoi(nameof(SoCaDuCongDaLoc));
        BaoThayDoi(nameof(SoCaCanRaSoatDaLoc));
        BaoThayDoi(nameof(TyLeHoanTatChamCongDaLoc));
        BaoThayDoi(nameof(KhungThoiGianChamCong));
        BaoThayDoi(nameof(NhanVienCongCaoHienThi));
        BaoThayDoi(nameof(SoChamCongHomNay));
        BaoThayDoi(nameof(TongGioCongHomNay));
        BaoThayDoi(nameof(TongNgayCongChamCongDaLoc));
        BaoThayDoi(nameof(SoCaDangMoDaLoc));
        BaoThayDoi(nameof(SoCaDuCongDaLoc));
        BaoThayDoi(nameof(SoCaCanRaSoatDaLoc));
        BaoThayDoi(nameof(TyLeHoanTatChamCongDaLoc));
        BaoThayDoi(nameof(TrangThaiCaDangChon));
        BaoThayDoi(nameof(CaGanNhatDangChon));
        BaoThayDoi(nameof(CoTheVaoCaDangChon));
        BaoThayDoi(nameof(CoTheRaCaDangChon));
        BaoThayDoi(nameof(SoChamCongTrongKyBaoCao));
        BaoThayDoi(nameof(TongGioCongTrongKyBaoCao));
        BaoThayDoi(nameof(TomTatBaoCaoNhanSu));
    }

    private void LamMoiBoLocNghiPhep()
    {
        CapNhatThongTinHienThiNghiPhep();
        DanhSachNghiPhepView.Refresh();
        DanhSachNghiPhepDaDuyetView.Refresh();
        DanhSachDeNghiNghiPhepView.Refresh();
        LamMoiLenhNghiPhep();
        BaoThayDoi(nameof(DanhSachNghiPhepDaDuyetView));
        BaoThayDoi(nameof(DanhSachDeNghiNghiPhepView));
        BaoThayDoi(nameof(SoNghiPhepDaLoc));
        BaoThayDoi(nameof(SoNghiPhepChoDuyetDaLoc));
        BaoThayDoi(nameof(SoNghiPhepDaDuyetDaLoc));
        BaoThayDoi(nameof(SoNgayNghiPhepDaLoc));
        BaoThayDoi(nameof(MoTaBoLocNghiPhep));
        BaoThayDoi(nameof(NghiChoDuyet));
        BaoThayDoi(nameof(SoNghiPhepTrongKyBaoCao));
        BaoThayDoi(nameof(NhanSuNghiDaDuyetTrongKyBaoCao));
        BaoThayDoi(nameof(QuanSoHienDienTrongKyBaoCao));
        BaoThayDoi(nameof(NghiPhepDaDuyetHomNay));
        BaoThayDoi(nameof(NhanSuTamVangHomNay));
        BaoThayDoi(nameof(DangLamViec));
        BaoThayDoi(nameof(TamNghi));
        BaoThayDoi(nameof(DanhSachNghiPhepDaDuyetTheoKy));
        BaoThayDoi(nameof(TieuDeAiDangNghi));
        BaoThayDoi(nameof(MoTaAiDangNghi));
        BaoThayDoi(nameof(TomTatBaoCaoNhanSu));
    }

    private void LamMoiBoLocDanhGia()
    {
        DanhSachDanhGiaView.Refresh();
        LamMoiLenhDanhGia();
        BaoThayDoi(nameof(CaNhanXuatSacHoSoHienThi));
        BaoThayDoi(nameof(CaNhanXuatSacNhat));
        BaoThayDoi(nameof(CaNhanXuatSacHienThi));
        BaoThayDoi(nameof(ThanhTichXuatSacHienThi));
    }

    private void LamMoiBoLocUngVien()
    {
        DanhSachUngVienView.Refresh();
        BaoThayDoi(nameof(DuLieuUngVienTheoViTri));
    }

    private bool LocThongBao(object obj)
    {
        if (obj is not ThongBaoHeThong thongBao) return false;
        if (ChiThongBaoChuaDoc && thongBao.DaDoc) return false;
        if (string.IsNullOrWhiteSpace(TuKhoaThongBao)) return true;

        var tuKhoa = TuKhoaThongBao.Trim();
        return thongBao.TieuDe.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)
            || thongBao.NoiDung.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)
            || thongBao.PhanHe.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)
            || thongBao.MucDo.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase);
    }

    private void ChonMucDieuHuong(object? thamSo)
    {
        var muc = thamSo?.ToString() ?? "Tổng quan";
        if (string.Equals(MucDangChon, muc, StringComparison.Ordinal))
        {
            return;
        }

        if (!CoQuyenTruyCap(muc))
        {
            MessageBox.Show($"Tài khoản {VaiTroNguoiDung} chưa có quyền vào phân hệ {muc}.", "Phân quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MucDangChon = muc;
    }

    private bool CoQuyenTruyCap(string muc)
    {
        return muc switch
        {
            "Ứng viên" => CoQuyenTuyenDung,
            "Nhân viên" => CoQuyenHoSoNhanVien,
            "Phòng ban" => CoQuyenPhongBan,
            "Chấm công" => CoQuyenChamCong,
            "Nghỉ phép" => CoQuyenNghiPhep,
            "Đánh giá" => CoQuyenDanhGia,
            "Bảng lương" => CoQuyenBangLuong,
            "Báo cáo" => CoQuyenBaoCaoNhanSu,
            "Cài đặt tài khoản" => CoQuyenCaiDatTaiKhoan,
            _ => true
        };
    }

    private void TaoMoiNhanVien(bool khoiTaoNoiBo = false)
    {
        if (!khoiTaoNoiBo && !CoQuyenQuanLyHoSoNhanVien)
        {
            MessageBox.Show("Bạn chỉ có quyền xem hồ sơ trong phạm vi của mình, không có quyền tạo hồ sơ mới.", "Phân quyền hồ sơ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (khoiTaoNoiBo && !CoQuyenQuanLyHoSoNhanVien)
        {
            BieuMauNhanVien = ChonNhanVienTrongPhamVi()?.TaoBanSao() ?? new NhanVien();
            return;
        }

        var phongBan = PhongBanTrongPhamVi.FirstOrDefault() ?? DuLieu.PhongBan.FirstOrDefault();
        var viTri = ViTriTrongPhamVi.FirstOrDefault(v => v.MaPhongBan == phongBan?.MaPhongBan)
            ?? DuLieu.ViTri.FirstOrDefault(v => v.MaPhongBan == phongBan?.MaPhongBan)
            ?? DuLieu.ViTri.FirstOrDefault();
        BieuMauNhanVien = new NhanVien
        {
            MaSo = QuyTacNghiepVuNhanSu.TaoMaNhanVienTiepTheo(DuLieu.NhanVien.Select(n => n.MaSo)),
            NgaySinh = DateTime.Today.AddYears(-23),
            NgayThamGiaBaoHiemXaHoi = DateTime.Today,
            NgayVaoLam = DateTime.Today,
            DangLamViec = true,
            MaPhongBan = phongBan?.MaPhongBan ?? 1,
            PhongBan = phongBan?.TenPhongBan ?? "",
            MaViTri = viTri?.MaViTri ?? 1,
            ViTri = viTri?.TenViTri ?? ""
        };
    }

    private void TaoMoiPhongBan()
    {
        BieuMauPhongBan = new BieuMauPhongBan
        {
            TenPhongBan = "",
            MaTruongPhong = NhanVienDangChon?.MaNhanVien
        };
    }

    private void KhoiTaoBieuMauNghiepVu()
    {
        var nhanVien = ChonNhanVienTrongPhamVi();
        var viTri = ViTriTrongPhamVi.FirstOrDefault() ?? DuLieu.ViTri.FirstOrDefault();
        BieuMauUngVien = new BieuMauUngVien
        {
            HoTen = "",
            Email = "",
            DienThoai = "",
            MaViTri = viTri?.MaViTri ?? 1
        };
        BieuMauNghiPhep = new BieuMauNghiPhep
        {
            MaNhanVien = nhanVien?.MaNhanVien ?? 1,
            LoaiNghi = "Nghỉ phép năm",
            TuNgay = DateTime.Today,
            DenNgay = DateTime.Today.AddDays(1)
        };
        TaoMoiDanhGia(khoiTaoNoiBo: true);
    }

    private void DuaVaoBieuMau()
    {
        if (NhanVienDangChon is null) return;
        BieuMauNhanVien = NhanVienDangChon.TaoBanSao();
    }

    private void TaoMoiDanhGia(bool khoiTaoNoiBo = false)
    {
        if (!khoiTaoNoiBo && !CoQuyenGhiNhanDanhGia)
        {
            MessageBox.Show("Bạn không có quyền tạo đánh giá năng lực.", "Phân quyền đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var nhanVien = NhanVienDangChon is not null && CoTheThaoTacNhanVienDangChon()
            ? NhanVienDangChon
            : ChonNhanVienTrongPhamVi();
        var nguoiDanhGia = LayNguoiDanhGiaMacDinh(nhanVien);
        BieuMauDanhGia = new BieuMauDanhGia
        {
            MaNhanVien = nhanVien?.MaNhanVien ?? 1,
            MaNguoiDanhGia = nguoiDanhGia?.MaNhanVien ?? nhanVien?.MaNhanVien ?? 1,
            KyDanhGia = QuyTacNghiepVuNhanSu.TaoKyDanhGia(DateTime.Today),
            Diem = 85,
            NhanXet = "Đánh giá mới từ ứng dụng",
            TrangThai = "Nháp",
            MaNhanVienGoc = 0,
            KyDanhGiaGoc = ""
        };
    }

    private NhanVien? LayNguoiDanhGiaMacDinh(NhanVien? nhanVien)
    {
        var trongPhamVi = NhanVienTrongPhamVi;
        if (LaVaiTro("Trưởng phòng"))
        {
            return trongPhamVi.FirstOrDefault(x => string.Equals(x.HoTen, TenNguoiDung, StringComparison.OrdinalIgnoreCase))
                ?? nhanVien
                ?? trongPhamVi.FirstOrDefault();
        }

        return trongPhamVi
            .OrderBy(x => x.ThuTuChucVu)
            .ThenBy(x => x.HoTen)
            .FirstOrDefault()
            ?? nhanVien;
    }

    private async Task LuuNhanVien()
    {
        if (!CoQuyenQuanLyHoSoNhanVien)
        {
            MessageBox.Show("Bạn chỉ có quyền xem hồ sơ trong phạm vi của mình, không có quyền lưu thay đổi.", "Phân quyền hồ sơ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(BieuMauNhanVien.MaSo) || string.IsNullOrWhiteSpace(BieuMauNhanVien.HoTen))
        {
            MessageBox.Show("Vui lòng nhập mã nhân viên và họ tên.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var phongBan = DuLieu.PhongBan.FirstOrDefault(p => p.MaPhongBan == BieuMauNhanVien.MaPhongBan);
        var viTri = DuLieu.ViTri.FirstOrDefault(v => v.MaViTri == BieuMauNhanVien.MaViTri);
        BieuMauNhanVien.PhongBan = phongBan?.TenPhongBan ?? "";
        BieuMauNhanVien.ViTri = viTri?.TenViTri ?? "";

        var nhanVienCu = DuLieu.NhanVien.FirstOrDefault(n => n.MaNhanVien == BieuMauNhanVien.MaNhanVien);
        if (nhanVienCu is not null && !CoTheXemNhanVien(nhanVienCu))
        {
            MessageBox.Show("Bạn không có quyền cập nhật hồ sơ ngoài phạm vi dữ liệu được phân công.", "Phân quyền hồ sơ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (phongBan is not null && !CoTheXemPhongBan(phongBan.TenPhongBan))
        {
            MessageBox.Show("Bạn không có quyền lưu nhân viên vào phòng ban ngoài phạm vi được phân công.", "Phân quyền hồ sơ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var laThemMoi = BieuMauNhanVien.MaNhanVien == 0 || DuLieu.NhanVien.All(n => n.MaNhanVien != BieuMauNhanVien.MaNhanVien);
            var maSoDaLuu = BieuMauNhanVien.MaSo.Trim();
            var hoTenDaLuu = BieuMauNhanVien.HoTen.Trim();
            if (DuLieu.NhanVien.Any(n => n.MaSo.Equals(BieuMauNhanVien.MaSo.Trim(), StringComparison.OrdinalIgnoreCase) && n.MaNhanVien != BieuMauNhanVien.MaNhanVien))
            {
                MessageBox.Show("Mã nhân viên đã tồn tại. Vui lòng dùng mã khác.", "Trùng mã nhân viên", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NguonDuLieu.StartsWith("SQL Server", StringComparison.OrdinalIgnoreCase))
            {
                await khoDuLieu.LuuNhanVienAsync(BieuMauNhanVien);
                await TaiDuLieu();
                NhanVienDangChon = DuLieu.NhanVien.FirstOrDefault(n => n.MaSo.Equals(maSoDaLuu, StringComparison.OrdinalIgnoreCase));
                BaoDaXong(laThemMoi ? "Thêm hồ sơ nhân viên" : "Cập nhật hồ sơ nhân viên", $"Đã {(laThemMoi ? "thêm" : "cập nhật")} hồ sơ {hoTenDaLuu}.", "Nhân viên");
                return;
            }

            NhanVien banDaLuu;
            if (nhanVienCu is null)
            {
                BieuMauNhanVien.MaNhanVien = DuLieu.NhanVien.Count == 0 ? 1 : DuLieu.NhanVien.Max(n => n.MaNhanVien) + 1;
                banDaLuu = BieuMauNhanVien.TaoBanSao();
                DuLieu.NhanVien.Add(banDaLuu);
            }
            else
            {
                CapNhatNhanVien(nhanVienCu, BieuMauNhanVien);
                banDaLuu = nhanVienCu;
            }

            NhanVienDangChon = banDaLuu;
            BieuMauNhanVien = banDaLuu.TaoBanSao();
            DanhSachNhanVienView.Refresh();
            BaoCaoThongKe();
            LamMoiThongBao();
            BaoDaXong(laThemMoi ? "Thêm hồ sơ nhân viên" : "Cập nhật hồ sơ nhân viên", $"Đã {(laThemMoi ? "thêm" : "cập nhật")} hồ sơ {banDaLuu.HoTen}.", "Nhân viên");
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể lưu nhân viên: {loi.Message}", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task XoaNhanVien()
    {
        if (NhanVienDangChon is null) return;
        if (!CoTheSuaNhanVienDangChon())
        {
            MessageBox.Show("Bạn không có quyền xóa hồ sơ ngoài phạm vi dữ liệu được phân công.", "Phân quyền hồ sơ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Xóa nhân viên {NhanVienDangChon.HoTen}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        try
        {
            var tenNhanVien = NhanVienDangChon.HoTen;
            if (NguonDuLieu.StartsWith("SQL Server", StringComparison.OrdinalIgnoreCase))
            {
                await khoDuLieu.XoaNhanVienAsync(NhanVienDangChon.MaNhanVien);
                await TaiDuLieu();
                BaoDaXong("Xóa hồ sơ nhân viên", $"Đã xóa hồ sơ {tenNhanVien}.", "Nhân viên");
                return;
            }

            DuLieu.NhanVien.Remove(NhanVienDangChon);
            CapNhatPhongBanSauKhiXoaNhanVien(tenNhanVien);
            XoaDuLieuLienQuanNhanVien(tenNhanVien);
            NhanVienDangChon = null;
            TaoMoiNhanVien();
            DanhSachNhanVienView.Refresh();
            BaoCaoThongKe();
            LamMoiThongBao();
            BaoDaXong("Xóa hồ sơ nhân viên", $"Đã xóa hồ sơ {tenNhanVien}.", "Nhân viên");
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể xóa nhân viên: {loi.Message}", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task TaoUngVien()
    {
        if (string.IsNullOrWhiteSpace(BieuMauUngVien.HoTen) || string.IsNullOrWhiteSpace(BieuMauUngVien.Email))
        {
            MessageBox.Show("Vui lòng nhập họ tên và email ứng viên.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var viTriTuyen = DuLieu.ViTri.FirstOrDefault(x => x.MaViTri == BieuMauUngVien.MaViTri);
        var phongBanTuyen = DuLieu.PhongBan.FirstOrDefault(x => x.MaPhongBan == viTriTuyen?.MaPhongBan);
        if (phongBanTuyen is not null && !CoTheXemPhongBan(phongBanTuyen.TenPhongBan))
        {
            MessageBox.Show("Bạn không có quyền tạo ứng viên cho phòng ban ngoài phạm vi được phân công.", "Phân quyền tuyển dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DuLieu.NhanVien.Any(nhanVien => string.Equals(nhanVien.HoTen.Trim(), BieuMauUngVien.HoTen.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Người này đã có trong danh sách nhân viên. Khi cần tuyển mới, hãy tạo ứng viên khác và cập nhật giai đoạn tuyển dụng.", "Ứng viên đã là nhân viên", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.ThemUngVienAsync(BieuMauUngVien);
            else
            {
                var viTri = viTriTuyen?.TenViTri ?? "";
                DuLieu.UngVien.Insert(0, new UngVien(BieuMauUngVien.HoTen, viTri, BieuMauUngVien.Email, BieuMauUngVien.DienThoai, "Mới"));
            }

            ThemThongBao("Ứng viên mới", $"Đã thêm ứng viên {BieuMauUngVien.HoTen}.", "Tuyển dụng");
        });
    }

    private void DuaPhongBanVaoBieuMau()
    {
        if (PhongBanDangChon is null) return;
        var truongPhong = DuLieu.NhanVien.FirstOrDefault(x => x.HoTen == PhongBanDangChon.TruongPhong);
        BieuMauPhongBan = new BieuMauPhongBan
        {
            MaPhongBan = PhongBanDangChon.MaPhongBan,
            TenPhongBan = PhongBanDangChon.TenPhongBan,
            MaTruongPhong = truongPhong?.MaNhanVien
        };
    }

    private void ChonPhongBanTuTongHop(TongHopPhongBanDieuHanh? tongHop)
    {
        if (tongHop is null)
        {
            return;
        }

        var phongBan = DuLieu.PhongBan.FirstOrDefault(p =>
            p.TenPhongBan.Equals(tongHop.TenPhongBan, StringComparison.OrdinalIgnoreCase));
        if (phongBan is not null && !Equals(PhongBanDangChon, phongBan))
        {
            PhongBanDangChon = phongBan;
        }
    }

    private void DongBoDongTongHopPhongBanDangChon()
    {
        var dongTongHop = PhongBanDangChon is null
            ? null
            : TongHopPhongBanDieuHanh.FirstOrDefault(x =>
                x.TenPhongBan.Equals(PhongBanDangChon.TenPhongBan, StringComparison.OrdinalIgnoreCase));

        if (!Equals(tongHopPhongBanDangChon, dongTongHop))
        {
            tongHopPhongBanDangChon = dongTongHop;
            BaoThayDoi(nameof(TongHopPhongBanDangChon));
        }
    }

    private async Task LuuPhongBan()
    {
        if (string.IsNullOrWhiteSpace(BieuMauPhongBan.TenPhongBan))
        {
            MessageBox.Show("Vui lòng nhập tên phòng ban.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DuLieu.PhongBan.Any(p =>
                p.TenPhongBan.Equals(BieuMauPhongBan.TenPhongBan.Trim(), StringComparison.OrdinalIgnoreCase) &&
                p.MaPhongBan != BieuMauPhongBan.MaPhongBan))
        {
            MessageBox.Show("Tên phòng ban đã tồn tại. Vui lòng dùng tên khác.", "Trùng phòng ban", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var laThemMoi = BieuMauPhongBan.MaPhongBan == 0 || DuLieu.PhongBan.All(p => p.MaPhongBan != BieuMauPhongBan.MaPhongBan);
            var tenPhongBan = BieuMauPhongBan.TenPhongBan.Trim();
            var truongPhong = DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == BieuMauPhongBan.MaTruongPhong)?.HoTen ?? "Chưa phân công";

            if (DangDungSql)
            {
                await khoDuLieu.LuuPhongBanAsync(BieuMauPhongBan);
                await TaiDuLieu();
                PhongBanDangChon = DuLieu.PhongBan.FirstOrDefault(p => p.TenPhongBan.Equals(tenPhongBan, StringComparison.OrdinalIgnoreCase));
                BaoDaXong(laThemMoi ? "Thêm phòng ban" : "Cập nhật phòng ban", $"Đã {(laThemMoi ? "thêm" : "cập nhật")} {tenPhongBan}.", "Phòng ban");
                return;
            }

            PhongBan phongBanDaLuu;
            var phongBanCu = DuLieu.PhongBan.FirstOrDefault(p => p.MaPhongBan == BieuMauPhongBan.MaPhongBan);
            if (phongBanCu is null)
            {
                phongBanDaLuu = new PhongBan(DuLieu.PhongBan.Count == 0 ? 1 : DuLieu.PhongBan.Max(x => x.MaPhongBan) + 1, tenPhongBan, truongPhong);
                DuLieu.PhongBan.Add(phongBanDaLuu);
            }
            else
            {
                var viTri = DuLieu.PhongBan.IndexOf(phongBanCu);
                phongBanDaLuu = phongBanCu with { TenPhongBan = tenPhongBan, TruongPhong = truongPhong };
                DuLieu.PhongBan[viTri] = phongBanDaLuu;
                CapNhatTenPhongBanNhanVien(phongBanDaLuu.MaPhongBan, tenPhongBan);
            }

            PhongBanDangChon = phongBanDaLuu;
            BieuMauPhongBan = new BieuMauPhongBan
            {
                MaPhongBan = phongBanDaLuu.MaPhongBan,
                TenPhongBan = phongBanDaLuu.TenPhongBan,
                MaTruongPhong = BieuMauPhongBan.MaTruongPhong
            };
            DanhSachNhanVienView.Refresh();
            BaoCaoThongKe();
            LamMoiThongBao();
            BaoDaXong(laThemMoi ? "Thêm phòng ban" : "Cập nhật phòng ban", $"Đã {(laThemMoi ? "thêm" : "cập nhật")} {tenPhongBan}.", "Phòng ban");
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể lưu phòng ban: {loi.Message}", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task XoaPhongBan()
    {
        if (PhongBanDangChon is null) return;
        var soNhanVien = DuLieu.NhanVien.Count(n => n.MaPhongBan == PhongBanDangChon.MaPhongBan);
        if (soNhanVien > 0)
        {
            MessageBox.Show($"Không thể xóa {PhongBanDangChon.TenPhongBan} vì đang có {soNhanVien} nhân viên. Hãy chuyển nhân viên sang phòng khác trước.", "Xóa phòng ban", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Xóa phòng ban {PhongBanDangChon.TenPhongBan}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        try
        {
            var tenPhongBan = PhongBanDangChon.TenPhongBan;
            if (DangDungSql)
            {
                await khoDuLieu.XoaPhongBanAsync(PhongBanDangChon.MaPhongBan);
                await TaiDuLieu();
                BaoDaXong("Xóa phòng ban", $"Đã xóa {tenPhongBan}.", "Phòng ban");
                return;
            }

            DuLieu.PhongBan.Remove(PhongBanDangChon);
            XoaViTriTheoPhongBan(PhongBanDangChon.MaPhongBan);
            PhongBanDangChon = null;
            TaoMoiPhongBan();
            BaoCaoThongKe();
            LamMoiThongBao();
            BaoDaXong("Xóa phòng ban", $"Đã xóa {tenPhongBan}.", "Phòng ban");
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể xóa phòng ban: {loi.Message}", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task GanTruongPhong()
    {
        if (PhongBanDangChon is null) return;
        var truongPhong = DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == BieuMauPhongBan.MaTruongPhong)
            ?? NhanVienDangChon
            ?? DuLieu.NhanVien.FirstOrDefault(x => x.MaPhongBan == PhongBanDangChon.MaPhongBan)
            ?? DuLieu.NhanVien.FirstOrDefault();
        if (truongPhong is null)
        {
            MessageBox.Show("Chưa có nhân viên để gán làm trưởng phòng.", "Gán trưởng phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.GanTruongPhongAsync(PhongBanDangChon.MaPhongBan, truongPhong.MaNhanVien);
            else
            {
                var viTri = DuLieu.PhongBan.IndexOf(PhongBanDangChon);
                if (viTri >= 0)
                {
                    PhongBanDangChon = PhongBanDangChon with { TruongPhong = truongPhong.HoTen };
                    DuLieu.PhongBan[viTri] = PhongBanDangChon;
                }
            }

            BieuMauPhongBan = new BieuMauPhongBan
            {
                MaPhongBan = PhongBanDangChon.MaPhongBan,
                TenPhongBan = PhongBanDangChon.TenPhongBan,
                MaTruongPhong = truongPhong.MaNhanVien
            };
            BaoCaoThongKe();
            BaoDaXong("Cập nhật trưởng phòng", $"{truongPhong.HoTen} đã được phân công phụ trách {PhongBanDangChon.TenPhongBan}.", "Phòng ban");
        });
    }

    private async Task ChuyenUngVienThanhNhanVien()
    {
        if (UngVienDangChon is null) return;
        var ungVien = UngVienDangChon;
        var viTriUngTuyen = DuLieu.ViTri.FirstOrDefault(x => x.TenViTri == ungVien.ViTri) ?? DuLieu.ViTri.FirstOrDefault();
        var phongBanUngTuyen = DuLieu.PhongBan.FirstOrDefault(x => x.MaPhongBan == viTriUngTuyen?.MaPhongBan);
        if (phongBanUngTuyen is not null && !CoTheXemPhongBan(phongBanUngTuyen.TenPhongBan))
        {
            MessageBox.Show("Bạn không có quyền tiếp nhận ứng viên vào phòng ban ngoài phạm vi được phân công.", "Phân quyền tuyển dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.ChuyenUngVienThanhNhanVienAsync(ungVien);
            else
            {
                if (DuLieu.NhanVien.Any(x => string.Equals(x.HoTen, ungVien.HoTen, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"{ungVien.HoTen} đã có trong danh sách nhân viên.", "Tiếp nhận nhân viên", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var viTri = viTriUngTuyen;
                var phongBan = phongBanUngTuyen ?? DuLieu.PhongBan.FirstOrDefault(x => x.MaPhongBan == viTri?.MaPhongBan) ?? DuLieu.PhongBan.FirstOrDefault();
                var nhanVienMoi = new NhanVien
                {
                    MaNhanVien = DuLieu.NhanVien.Count == 0 ? 1 : DuLieu.NhanVien.Max(x => x.MaNhanVien) + 1,
                    MaSo = QuyTacNghiepVuNhanSu.TaoMaNhanVienTiepTheo(DuLieu.NhanVien.Select(n => n.MaSo)),
                    HoTen = ungVien.HoTen,
                    MaPhongBan = phongBan?.MaPhongBan ?? 1,
                    PhongBan = phongBan?.TenPhongBan ?? "",
                    MaViTri = viTri?.MaViTri ?? 1,
                    ViTri = viTri?.TenViTri ?? "",
                    NgaySinh = DateTime.Today.AddYears(-23),
                    NgayThamGiaBaoHiemXaHoi = DateTime.Today,
                    NgayVaoLam = DateTime.Today,
                    DangLamViec = true
                };
                DuLieu.NhanVien.Add(nhanVienMoi);
                NhanVienDangChon = nhanVienMoi;

                var viTriUngVien = DuLieu.UngVien.IndexOf(ungVien);
                if (viTriUngVien >= 0)
                {
                    DuLieu.UngVien.RemoveAt(viTriUngVien);
                    UngVienDangChon = DuLieu.UngVien.FirstOrDefault(LocUngVien);
                }
            }

            ThemThongBao("Tiếp nhận nhân viên", $"Đã chuyển {ungVien.HoTen} từ ứng viên sang nhân viên chính thức.", "Tuyển dụng");
        });
    }

    private bool CoTheChuyenUngVienThanhNhanVien()
    {
        return UngVienDangChon is not null
            && string.Equals(UngVienDangChon.GiaiDoan, "Đề nghị nhận việc", StringComparison.OrdinalIgnoreCase)
            && CoTheXemPhongBan(DuLieu.PhongBan.FirstOrDefault(x => x.MaPhongBan == DuLieu.ViTri.FirstOrDefault(v => v.TenViTri == UngVienDangChon.ViTri)?.MaPhongBan)?.TenPhongBan ?? "");
    }

    private async Task ChuyenGiaiDoanUngVien()
    {
        if (UngVienDangChon is null) return;
        if (!LocUngVien(UngVienDangChon))
        {
            MessageBox.Show("Bạn không có quyền cập nhật ứng viên ngoài phạm vi được phân công.", "Phân quyền tuyển dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var giaiDoanCu = UngVienDangChon.GiaiDoan;
        var giaiDoanMoi = LayGiaiDoanUngVienTiepTheo(giaiDoanCu);
        if (string.Equals(giaiDoanCu, giaiDoanMoi, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show($"Ứng viên đang ở giai đoạn cuối: {giaiDoanMoi}. Nếu đã đạt yêu cầu, hãy dùng nút Tiếp nhận thành nhân viên.", "Luồng tuyển dụng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.ChuyenGiaiDoanUngVienAsync(UngVienDangChon, giaiDoanMoi);
            else
            {
                var viTri = DuLieu.UngVien.IndexOf(UngVienDangChon);
                if (viTri >= 0)
                {
                    DuLieu.UngVien[viTri] = UngVienDangChon with { GiaiDoan = giaiDoanMoi };
                    UngVienDangChon = DuLieu.UngVien[viTri];
                }
            }

            ThemThongBao("Cập nhật tuyển dụng", $"{UngVienDangChon.HoTen} đã chuyển từ {giaiDoanCu} sang {giaiDoanMoi}.", "Tuyển dụng");
        });
    }

    private void XuatHopDongLamViec()
    {
        if (UngVienDangChon is null)
        {
            MessageBox.Show("Vui lòng chọn ứng viên trước khi xuất hợp đồng.", "Chưa chọn ứng viên", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!LocUngVien(UngVienDangChon))
        {
            MessageBox.Show("Bạn không có quyền xuất hợp đồng cho ứng viên ngoài phạm vi được phân công.", "Phân quyền tuyển dụng", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var taiLieu = TaoTaiLieuHopDongLamViec(UngVienDangChon);
        var tenFile = $"HopDongLamViec_{LamSachTenFile(UngVienDangChon.HoTen)}_{DateTime.Today:yyyyMMdd}.docx";
        if (XuatTaiLieu(taiLieu, tenFile, "Hợp đồng làm việc"))
        {
            ThemThongBao("Xuất hợp đồng làm việc", $"Đã xuất hợp đồng cho {UngVienDangChon.HoTen}.", "Tuyển dụng");
        }
    }

    private async Task VaoCa()
    {
        if (NhanVienDangChon is null) return;
        if (!CoTheThaoTacNhanVienDangChon())
        {
            MessageBox.Show("Bạn không có quyền chấm công cho nhân viên ngoài phạm vi được phân công.", "Phân quyền chấm công", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.GhiNhanVaoCaAsync(NhanVienDangChon.MaNhanVien);
            else
            {
                var caDangMo = DuLieu.ChamCong.Any(x => x.NhanVien == NhanVienDangChon.HoTen && x.GioRa is null);
                if (caDangMo)
                {
                    MessageBox.Show($"{NhanVienDangChon.HoTen} đang có ca chưa ra. Vui lòng bấm Ra ca trước khi vào ca mới.", "Chấm công", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var caMoi = new ChamCong(NhanVienDangChon.HoTen, DateTime.Now, null, 0);
                DuLieu.ChamCong.Insert(0, caMoi);
                ChamCongDangChon = caMoi;
            }
            ThemThongBao("Chấm công vào ca", $"{NhanVienDangChon.HoTen} đã vào ca.", "Chấm công");
        });
    }

    private async Task RaCa()
    {
        if (NhanVienDangChon is null) return;
        if (!CoTheThaoTacNhanVienDangChon())
        {
            MessageBox.Show("Bạn không có quyền chấm công cho nhân viên ngoài phạm vi được phân công.", "Phân quyền chấm công", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.GhiNhanRaCaAsync(NhanVienDangChon.MaNhanVien);
            else
            {
                var caDangMo = DuLieu.ChamCong
                    .Select((chamCong, viTri) => new { chamCong, viTri })
                    .Where(x => x.chamCong.NhanVien == NhanVienDangChon.HoTen && x.chamCong.GioRa is null)
                    .OrderByDescending(x => x.chamCong.GioVao)
                    .FirstOrDefault();

                if (caDangMo is null)
                {
                    MessageBox.Show($"{NhanVienDangChon.HoTen} chưa có ca đang mở để ra ca.", "Chấm công", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var gioRa = DateTime.Now;
                var soGio = Math.Round((decimal)Math.Max(0, (gioRa - caDangMo.chamCong.GioVao).TotalHours), 2);
                DuLieu.ChamCong[caDangMo.viTri] = caDangMo.chamCong with { GioRa = gioRa, SoGio = soGio };
                ChamCongDangChon = DuLieu.ChamCong[caDangMo.viTri];
            }
            ThemThongBao("Chấm công ra ca", $"{NhanVienDangChon.HoTen} đã ra ca.", "Chấm công");
        });
    }

    private async Task DieuChinhCong()
    {
        if (ChamCongDangChon is null) return;
        if (!CoQuyenDieuChinhCong || !LocChamCong(ChamCongDangChon))
        {
            MessageBox.Show("Bạn không có quyền điều chỉnh công ngoài phạm vi được phân công.", "Phân quyền chấm công", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.DieuChinhCongAsync(ChamCongDangChon);
            else
            {
                var viTri = DuLieu.ChamCong.IndexOf(ChamCongDangChon);
                if (viTri >= 0)
                {
                    DuLieu.ChamCong[viTri] = ChamCongDangChon with { GioRa = ChamCongDangChon.GioVao.AddHours(8), SoGio = 8 };
                    ChamCongDangChon = DuLieu.ChamCong[viTri];
                }
            }

            ThemThongBao("Điều chỉnh công", $"Đã chuẩn hóa công của {ChamCongDangChon.NhanVien} thành 8 giờ.", "Chấm công");
        });
    }

    private async Task TaoNghiPhep()
    {
        var nhanVienNghi = DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == BieuMauNghiPhep.MaNhanVien);
        if (nhanVienNghi is null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên hợp lệ trước khi tạo đơn nghỉ.", "Nghỉ phép", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!CoTheXemNhanVien(nhanVienNghi))
        {
            MessageBox.Show("Bạn không có quyền tạo đơn nghỉ cho nhân viên ngoài phạm vi được phân công.", "Phân quyền nghỉ phép", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(BieuMauNghiPhep.LoaiNghi))
        {
            MessageBox.Show("Vui lòng nhập loại nghỉ.", "Nghỉ phép", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (BieuMauNghiPhep.DenNgay.Date < BieuMauNghiPhep.TuNgay.Date)
        {
            MessageBox.Show("Ngày kết thúc nghỉ phép phải sau hoặc bằng ngày bắt đầu.", "Nghỉ phép", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var loaiNghi = BieuMauNghiPhep.LoaiNghi.Trim();
        var tuNgay = BieuMauNghiPhep.TuNgay.Date;
        var denNgay = BieuMauNghiPhep.DenNgay.Date;

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.ThemNghiPhepAsync(BieuMauNghiPhep);
            else
            {
                var soNgay = QuyTacNghiepVuNhanSu.TinhSoNgayBaoGom(tuNgay, denNgay);
                var maDonMoi = DuLieu.NghiPhep.Count == 0 ? 1 : DuLieu.NghiPhep.Max(x => x.MaDon) + 1;
                var donNghiMoi = new NghiPhep(nhanVienNghi.HoTen, loaiNghi, tuNgay, denNgay, soNgay, "Chờ duyệt", maDonMoi);
                DuLieu.NghiPhep.Insert(0, donNghiMoi);
                NghiPhepDangChon = donNghiMoi;
            }

            ThemThongBao("Đơn nghỉ phép mới", $"{nhanVienNghi.HoTen} đã gửi đơn {loaiNghi} từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}, đang chờ duyệt.", "Nghỉ phép");
        });

        MucDangChon = "Nghỉ phép";
        NghiPhepDangChon = DuLieu.NghiPhep
            .Where(x => string.Equals(x.NhanVien, nhanVienNghi.HoTen, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.LoaiNghi, loaiNghi, StringComparison.OrdinalIgnoreCase)
                && x.TuNgay.Date == tuNgay
                && x.DenNgay.Date == denNgay)
            .OrderByDescending(x => x.ThuTuMoiNhat)
            .FirstOrDefault()
            ?? NghiPhepDangChon;
    }

    private async Task CapNhatNghiPhepTuDong(object? thamSo, string trangThai)
    {
        if (thamSo is not NghiPhep nghiPhep)
        {
            return;
        }

        NghiPhepDangChon = nghiPhep;
        await CapNhatNghiPhep(trangThai);
    }

    private async Task CapNhatNghiPhep(string trangThai)
    {
        if (NghiPhepDangChon is null) return;
        if (!CoTheXuLyNghiPhep(NghiPhepDangChon))
        {
            MessageBox.Show("Bạn không có quyền duyệt đơn nghỉ ngoài phạm vi được phân công.", "Phân quyền nghỉ phép", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var donNghiDangChon = NghiPhepDangChon;
        var lyDoXuLy = TaoLyDoXuLyNghiPhep(donNghiDangChon.LyDoXuLy, trangThai);
        donNghiDangChon.LyDoXuLy = lyDoXuLy;
        var donNghiSauCapNhat = donNghiDangChon with { TrangThai = trangThai };
        donNghiSauCapNhat.LyDoXuLy = lyDoXuLy;
        var ghiChuTongQuan = TaoGhiChuTongQuanSauDuyetNghi(donNghiSauCapNhat);

        await ChayLenhDuLieu(async () =>
        {
            var tenNhanVien = donNghiDangChon.NhanVien;
            if (DangDungSql)
            {
                await khoDuLieu.CapNhatTrangThaiNghiPhepAsync(donNghiDangChon, trangThai);
                var nhanVien = LayNhanVienTheoTen(tenNhanVien);
                if (nhanVien is not null)
                {
                    await khoDuLieu.TaoPhieuLuongAsync(nhanVien.MaNhanVien);
                }
            }
            else
            {
                var viTri = DuLieu.NghiPhep.IndexOf(donNghiDangChon);
                if (viTri >= 0)
                {
                    DuLieu.NghiPhep[viTri] = donNghiSauCapNhat;
                    NghiPhepDangChon = DuLieu.NghiPhep[viTri];
                }

                var nhanVien = LayNhanVienTheoTen(tenNhanVien);
                if (nhanVien is not null)
                {
                    var phieuLuongMoi = TaoPhieuLuongThang(nhanVien);
                    var viTriPhieuLuong = DuLieu.PhieuLuong
                        .Select((phieuLuong, chiSo) => new { phieuLuong, chiSo })
                        .FirstOrDefault(x => x.phieuLuong.NhanVien == phieuLuongMoi.NhanVien && x.phieuLuong.KyLuong == phieuLuongMoi.KyLuong);
                    if (viTriPhieuLuong is null)
                    {
                        DuLieu.PhieuLuong.Insert(0, phieuLuongMoi);
                    }
                    else
                    {
                        DuLieu.PhieuLuong[viTriPhieuLuong.chiSo] = phieuLuongMoi;
                    }
                }
            }
            ThemThongBao("Cập nhật nghỉ phép", $"Đơn nghỉ của {donNghiDangChon.NhanVien} đã chuyển sang {trangThai}. Lý do xử lý: {lyDoXuLy}. {ghiChuTongQuan}", "Nghỉ phép");
        });
    }

    private static string TaoLyDoXuLyNghiPhep(string lyDoDangNhap, string trangThai)
    {
        if (!string.IsNullOrWhiteSpace(lyDoDangNhap))
        {
            return lyDoDangNhap.Trim();
        }

        return QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(trangThai)
            ? "Đồng ý theo kế hoạch nhân sự."
            : "Từ chối sau khi rà soát lịch vận hành.";
    }

    private string TaoGhiChuTongQuanSauDuyetNghi(NghiPhep nghiPhep)
    {
        if (!QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai))
        {
            return "Tổng quan đã được làm mới theo trạng thái mới.";
        }

        if (QuyTacNghiepVuNhanSu.NghiPhepGiaoNgay(nghiPhep, DateTime.Today))
        {
            return "Quân số hôm nay và biểu đồ tổng quan đã được cập nhật.";
        }

        return NghiPhepThuocKyBaoCao(nghiPhep)
            ? "Biểu đồ theo kỳ đã cập nhật; quân số hôm nay không đổi vì đơn không thuộc hôm nay."
            : "Đơn không thuộc kỳ đang xem nên biểu đồ tổng quan không đổi.";
    }

    private void DuaDanhGiaVaoBieuMau()
    {
        if (DanhGiaDangChon is null) return;
        if (!CoQuyenGhiNhanDanhGia || !LocDanhGia(DanhGiaDangChon))
        {
            MessageBox.Show("Bạn không có quyền sửa đánh giá ngoài phạm vi được phân công.", "Phân quyền đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var nhanVien = LayNhanVienTheoTen(DanhGiaDangChon.NhanVien);
        var nguoiDanhGia = LayNhanVienTheoTen(DanhGiaDangChon.NguoiDanhGia) ?? LayNguoiDanhGiaMacDinh(nhanVien);
        BieuMauDanhGia = new BieuMauDanhGia
        {
            MaNhanVien = nhanVien?.MaNhanVien ?? NhanVienTrongPhamVi.FirstOrDefault()?.MaNhanVien ?? 1,
            MaNguoiDanhGia = nguoiDanhGia?.MaNhanVien ?? nhanVien?.MaNhanVien ?? 1,
            KyDanhGia = DanhGiaDangChon.KyDanhGia,
            Diem = DanhGiaDangChon.Diem,
            NhanXet = DanhGiaDangChon.NhanXet,
            TrangThai = DanhGiaDangChon.TrangThai,
            MaNhanVienGoc = nhanVien?.MaNhanVien ?? 0,
            KyDanhGiaGoc = DanhGiaDangChon.KyDanhGia
        };
        TabDanhGiaDangChon = 0;
    }

    private async Task LuuDanhGia()
    {
        var nhanVien = DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == BieuMauDanhGia.MaNhanVien);
        var nguoiDanhGia = DuLieu.NhanVien.FirstOrDefault(x => x.MaNhanVien == BieuMauDanhGia.MaNguoiDanhGia);
        if (nhanVien is null || nguoiDanhGia is null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên và người đánh giá hợp lệ.", "Đánh giá năng lực", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!CoQuyenGhiNhanDanhGia || !CoTheXemNhanVien(nhanVien))
        {
            MessageBox.Show("Bạn không có quyền lưu đánh giá ngoài phạm vi được phân công.", "Phân quyền đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(BieuMauDanhGia.KyDanhGia))
        {
            MessageBox.Show("Vui lòng nhập kỳ đánh giá, ví dụ 2026-Q2.", "Đánh giá năng lực", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (BieuMauDanhGia.Diem < 0 || BieuMauDanhGia.Diem > 100)
        {
            MessageBox.Show("Điểm đánh giá phải nằm trong khoảng 0 đến 100.", "Đánh giá năng lực", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BieuMauDanhGia.KyDanhGia = BieuMauDanhGia.KyDanhGia.Trim();
        BieuMauDanhGia.NhanXet = string.IsNullOrWhiteSpace(BieuMauDanhGia.NhanXet) ? "Chưa có nhận xét" : BieuMauDanhGia.NhanXet.Trim();
        BieuMauDanhGia.TrangThai = string.IsNullOrWhiteSpace(BieuMauDanhGia.TrangThai) ? "Nháp" : BieuMauDanhGia.TrangThai.Trim();
        var laCapNhat = BieuMauDanhGia.DangSua || DuLieu.DanhGia.Any(x =>
            string.Equals(x.NhanVien, nhanVien.HoTen, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.KyDanhGia, BieuMauDanhGia.KyDanhGia, StringComparison.OrdinalIgnoreCase));

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.LuuDanhGiaAsync(BieuMauDanhGia);
            else
            {
                var danhGiaMoi = new DanhGia(nhanVien.HoTen, nguoiDanhGia.HoTen, BieuMauDanhGia.KyDanhGia, BieuMauDanhGia.Diem, BieuMauDanhGia.NhanXet, BieuMauDanhGia.TrangThai);
                var viTri = BieuMauDanhGia.DangSua
                    ? DuLieu.DanhGia
                        .Select((danhGia, chiSo) => new { danhGia, chiSo })
                        .FirstOrDefault(x =>
                            string.Equals(x.danhGia.NhanVien, LayTenNhanVienTheoMa(BieuMauDanhGia.MaNhanVienGoc), StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.danhGia.KyDanhGia, BieuMauDanhGia.KyDanhGiaGoc, StringComparison.OrdinalIgnoreCase))
                    : null;

                viTri ??= DuLieu.DanhGia
                    .Select((danhGia, chiSo) => new { danhGia, chiSo })
                    .FirstOrDefault(x =>
                        string.Equals(x.danhGia.NhanVien, danhGiaMoi.NhanVien, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.danhGia.KyDanhGia, danhGiaMoi.KyDanhGia, StringComparison.OrdinalIgnoreCase));

                if (viTri is null)
                {
                    DuLieu.DanhGia.Insert(0, danhGiaMoi);
                }
                else
                {
                    DuLieu.DanhGia[viTri.chiSo] = danhGiaMoi;
                }

                DanhGiaDangChon = danhGiaMoi;
                BieuMauDanhGia.MaNhanVienGoc = nhanVien.MaNhanVien;
                BieuMauDanhGia.KyDanhGiaGoc = danhGiaMoi.KyDanhGia;
            }

            ThemThongBao(laCapNhat ? "Cập nhật đánh giá" : "Tạo đánh giá", $"Đã {(laCapNhat ? "cập nhật" : "tạo")} đánh giá kỳ {BieuMauDanhGia.KyDanhGia} cho {nhanVien.HoTen}.", "Đánh giá");
        });
    }

    private async Task XoaDanhGia()
    {
        if (DanhGiaDangChon is null) return;
        if (!CoQuyenGhiNhanDanhGia || !LocDanhGia(DanhGiaDangChon))
        {
            MessageBox.Show("Bạn không có quyền xóa đánh giá ngoài phạm vi được phân công.", "Phân quyền đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Xóa đánh giá kỳ {DanhGiaDangChon.KyDanhGia} của {DanhGiaDangChon.NhanVien}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        var danhGiaCanXoa = DanhGiaDangChon;
        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.XoaDanhGiaAsync(danhGiaCanXoa);
            else DuLieu.DanhGia.Remove(danhGiaCanXoa);

            DanhGiaDangChon = null;
            TaoMoiDanhGia(khoiTaoNoiBo: true);
            ThemThongBao("Xóa đánh giá", $"Đã xóa đánh giá kỳ {danhGiaCanXoa.KyDanhGia} của {danhGiaCanXoa.NhanVien}.", "Đánh giá");
        });
    }

    private async Task ChotDanhGia()
    {
        if (DanhGiaDangChon is null) return;
        if (!CoQuyenGhiNhanDanhGia || !LocDanhGia(DanhGiaDangChon))
        {
            MessageBox.Show("Bạn không có quyền chốt đánh giá ngoài phạm vi được phân công.", "Phân quyền đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.ChotDanhGiaAsync(DanhGiaDangChon);
            else
            {
                var viTri = DuLieu.DanhGia.IndexOf(DanhGiaDangChon);
                if (viTri >= 0)
                {
                    DuLieu.DanhGia[viTri] = DanhGiaDangChon with { TrangThai = "Hoàn tất" };
                    DanhGiaDangChon = DuLieu.DanhGia[viTri];
                }
            }

            ThemThongBao("Chốt đánh giá", $"Đã chốt kết quả đánh giá kỳ {DanhGiaDangChon.KyDanhGia}.", "Đánh giá");
        });
    }

    private async Task TinhLuong()
    {
        if (NhanVienDangChon is null) return;
        if (!CoQuyenXuLyBangLuong || !CoTheThaoTacNhanVienDangChon())
        {
            MessageBox.Show("Bạn không có quyền tính lương ngoài phạm vi được phân công.", "Phân quyền bảng lương", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.TaoPhieuLuongAsync(NhanVienDangChon.MaNhanVien);
            else
            {
                var phieuLuongMoi = TaoPhieuLuongThang(NhanVienDangChon);
                var viTri = DuLieu.PhieuLuong
                    .Select((phieuLuong, chiSo) => new { phieuLuong, chiSo })
                    .FirstOrDefault(x => x.phieuLuong.NhanVien == phieuLuongMoi.NhanVien && x.phieuLuong.KyLuong == phieuLuongMoi.KyLuong);

                if (viTri is null)
                {
                    DuLieu.PhieuLuong.Insert(0, phieuLuongMoi);
                    PhieuLuongDangChon = phieuLuongMoi;
                }
                else
                {
                    DuLieu.PhieuLuong[viTri.chiSo] = phieuLuongMoi;
                    PhieuLuongDangChon = phieuLuongMoi;
                }
            }
            ThemThongBao("Tính lương", $"Đã tính lương kỳ {DateTime.Today:yyyy-MM} cho {NhanVienDangChon.HoTen}.", "Bảng lương");
        });
    }

    private PhieuLuong TaoPhieuLuongThang(NhanVien nhanVien)
    {
        var luongCoBan = LayLuongCoBan(nhanVien);
        var tongGioCong = TinhTongGioCongThang(nhanVien);
        var soNgayNghiDaDuyet = TinhNgayNghiDaDuyetThang(nhanVien);
        return QuyTacNghiepVuNhanSu.TaoPhieuLuongThang(nhanVien, luongCoBan, tongGioCong, soNgayNghiDaDuyet, DateTime.Today);
    }

    private decimal LayLuongCoBan(NhanVien nhanVien)
    {
        var luongTheoViTri = DuLieu.ViTri.FirstOrDefault(x => x.MaViTri == nhanVien.MaViTri)?.LuongDuKien ?? 0;
        return luongTheoViTri > 0 ? luongTheoViTri : 10_000_000m;
    }

    private decimal TinhTongGioCongThang(NhanVien nhanVien)
    {
        var homNay = DateTime.Today;
        return DuLieu.ChamCong
            .Where(x => x.NhanVien == nhanVien.HoTen && x.GioVao.Year == homNay.Year && x.GioVao.Month == homNay.Month)
            .Sum(TinhSoGioChamCong);
    }

    private decimal TinhNgayCongQuyDoi(NhanVien nhanVien)
    {
        var tongGio = TinhTongGioCongThang(nhanVien);
        if (tongGio <= 0)
        {
            return QuyTacNghiepVuNhanSu.SoNgayCongChuanThang;
        }

        return QuyTacNghiepVuNhanSu.TinhNgayCongQuyDoi(tongGio);
    }

    private decimal TinhNgayNghiDaDuyetThang(NhanVien nhanVien)
    {
        var homNay = DateTime.Today;
        return DuLieu.NghiPhep
            .Where(x => x.NhanVien == nhanVien.HoTen
                && x.TrangThai.Contains("Đã duyệt", StringComparison.OrdinalIgnoreCase)
                && QuyTacNghiepVuNhanSu.TinhSoNgayTrongThang(x.TuNgay, x.DenNgay, homNay.Year, homNay.Month) > 0)
            .Sum(x => QuyTacNghiepVuNhanSu.TinhSoNgayTrongThang(x.TuNgay, x.DenNgay, homNay.Year, homNay.Month));
    }

    private void XemPhieuLuong()
    {
        if (PhieuLuongDangChon is null) return;
        if (!LocPhieuLuong(PhieuLuongDangChon))
        {
            MessageBox.Show("Bạn không có quyền xem phiếu lương ngoài phạm vi được phân công.", "Phân quyền bảng lương", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var nhanVien = LayNhanVienTheoTen(PhieuLuongDangChon.NhanVien);
        var khauTruBaoHiem = QuyTacNghiepVuNhanSu.TinhKhauTruBaoHiem(PhieuLuongDangChon.LuongCoBan);
        var noiDung = $"""
            Nhân viên: {PhieuLuongDangChon.NhanVien}
            Kỳ lương: {PhieuLuongDangChon.KyLuong}
            Lương cơ bản: {PhieuLuongDangChon.LuongCoBan:N0} đ
            Phụ cấp: {PhieuLuongDangChon.PhuCap:N0} đ
            Khấu trừ: {PhieuLuongDangChon.KhauTru:N0} đ
            Trong đó BHXH/BHYT/BHTN người lao động: {khauTruBaoHiem:N0} đ
            Số năm tham gia BHXH: {nhanVien?.SoNamBaoHiemXaHoi ?? 0}
            Thực lãnh: {PhieuLuongDangChon.ThucLanh:N0} đ
            Trạng thái: {PhieuLuongDangChon.TrangThai}
            """;
        MessageBox.Show(noiDung, "Phiếu lương", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task XacNhanTraLuong()
    {
        if (PhieuLuongDangChon is null) return;
        if (!CoQuyenXuLyBangLuong || !LocPhieuLuong(PhieuLuongDangChon))
        {
            MessageBox.Show("Bạn không có quyền xác nhận phiếu lương ngoài phạm vi được phân công.", "Phân quyền bảng lương", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ChayLenhDuLieu(async () =>
        {
            if (DangDungSql) await khoDuLieu.XacNhanTraLuongAsync(PhieuLuongDangChon);
            else
            {
                var viTri = DuLieu.PhieuLuong.IndexOf(PhieuLuongDangChon);
                if (viTri >= 0)
                {
                    DuLieu.PhieuLuong[viTri] = PhieuLuongDangChon with { TrangThai = "Đã trả" };
                    PhieuLuongDangChon = DuLieu.PhieuLuong[viTri];
                }
            }

            ThemThongBao("Trả lương", $"Đã xác nhận trả lương kỳ {PhieuLuongDangChon.KyLuong}.", "Bảng lương");
        });
    }

    private void XuatBaoCaoNhanVien()
    {
        var taiLieu = TaoTaiLieuBaoCaoNhanVien();
        var tenFile = $"BaoCaoHoSoNhanSu_{DateTime.Today:yyyyMMdd}.xlsx";
        if (XuatTaiLieu(taiLieu, tenFile, "báo cáo hồ sơ nhân sự"))
        {
            ThemThongBao("Xuất báo cáo hồ sơ", "Báo cáo hồ sơ nhân sự đã được xuất.", "Báo cáo");
        }
    }

    private void XuatBaoCaoChamCong()
    {
        var taiLieu = TaoTaiLieuBaoCaoChamCong();
        var tenFile = $"BaoCaoChamCong_{DateTime.Today:yyyyMMdd}.xlsx";
        if (XuatTaiLieu(taiLieu, tenFile, "báo cáo chấm công"))
        {
            ThemThongBao("Xuất báo cáo chấm công", "Báo cáo chấm công đã được xuất.", "Báo cáo");
        }
    }

    private void XuatBaoCaoNghiPhep()
    {
        var taiLieu = TaoTaiLieuBaoCaoNghiPhep();
        var tenFile = $"BaoCaoNghiPhep_{DateTime.Today:yyyyMMdd}.xlsx";
        if (XuatTaiLieu(taiLieu, tenFile, "báo cáo nghỉ phép"))
        {
            ThemThongBao("Xuất báo cáo nghỉ phép", "Báo cáo nghỉ phép đã được xuất.", "Báo cáo");
        }
    }

    private void XuatBaoCaoLuong()
    {
        if (!CoQuyenXuLyBangLuong)
        {
            MessageBox.Show("Bạn không có quyền xuất báo cáo lương.", "Phân quyền bảng lương", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var taiLieu = TaoTaiLieuBaoCaoLuong();
        var tenFile = $"BaoCaoLuongNhanSu_{DateTime.Today:yyyyMMdd}.xlsx";
        if (XuatTaiLieu(taiLieu, tenFile, "Báo cáo lương nhân sự"))
        {
            ThemThongBao("Xuất báo cáo lương", "Báo cáo lương nhân sự đã được xuất.", "Báo cáo");
        }
    }

    private async Task TaoTaiKhoanMau()
    {
        if (!DangDungSql)
        {
            MessageBox.Show("Đồng bộ tài khoản nhân sự cần kết nối SQL Server để ghi vào bảng HR_Users.", "Chưa kết nối SQL Server", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var soTaiKhoan = await khoDuLieu.DongBoTaiKhoanNhanSuAsync(TenDangNhap);
        await TaiDanhSachTaiKhoan();
        ThemThongBao("Đồng bộ tài khoản", $"Đã đồng bộ {soTaiKhoan} tài khoản theo mã nhân viên.", "Cài đặt tài khoản");
    }

    private async Task KhoaMoTaiKhoan()
    {
        if (TaiKhoanDangChon is null) return;
        var viTri = CacTaiKhoanHeThong.IndexOf(TaiKhoanDangChon);
        if (viTri < 0) return;

        var trangThaiMoi = TaiKhoanDangChon.TrangThai == "Đang hoạt động" ? "Tạm khóa" : "Đang hoạt động";
        if (DangDungSql)
        {
            await khoDuLieu.KhoaMoTaiKhoanAsync(TaiKhoanDangChon.TenDangNhap, trangThaiMoi == "Đang hoạt động", TenDangNhap);
        }

        CacTaiKhoanHeThong[viTri] = TaiKhoanDangChon with { TrangThai = trangThaiMoi };
        TaiKhoanDangChon = CacTaiKhoanHeThong[viTri];
        BaoThayDoi(nameof(CacTaiKhoanDangHoatDong));
        ThemThongBao("Cập nhật tài khoản", $"{TaiKhoanDangChon.TenDangNhap} đã chuyển sang trạng thái {trangThaiMoi}.", "Cài đặt tài khoản");
    }

    private void TaoMoiTaiKhoan()
    {
        BieuMauTaiKhoan.XoaTrang();
    }

    private void SuaThongTinTaiKhoan()
    {
        if (TaiKhoanDangChon is null) return;

        BieuMauTaiKhoan.TenDangNhapGoc = TaiKhoanDangChon.TenDangNhap;
        BieuMauTaiKhoan.TenDangNhap = TaiKhoanDangChon.TenDangNhap;
        BieuMauTaiKhoan.HoTen = TaiKhoanDangChon.HoTen;
        BieuMauTaiKhoan.VaiTro = TaiKhoanDangChon.VaiTro;
        BieuMauTaiKhoan.MatKhauMoi = "";
        BieuMauTaiKhoan.DangHoatDong = TaiKhoanDangChon.TrangThai != "Tạm khóa";
        BieuMauTaiKhoan.DangSua = true;
    }

    private async Task LuuTaiKhoan()
    {
        BieuMauTaiKhoan.TenDangNhap = BieuMauTaiKhoan.TenDangNhap.Trim();
        BieuMauTaiKhoan.HoTen = BieuMauTaiKhoan.HoTen.Trim();
        BieuMauTaiKhoan.MatKhauMoi = BieuMauTaiKhoan.MatKhauMoi.Trim();

        if (string.IsNullOrWhiteSpace(BieuMauTaiKhoan.TenDangNhap) || string.IsNullOrWhiteSpace(BieuMauTaiKhoan.HoTen))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập và họ tên tài khoản.", "Thiếu thông tin tài khoản", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!BieuMauTaiKhoan.DangSua
            && CacTaiKhoanHeThong.Any(x => string.Equals(x.TenDangNhap, BieuMauTaiKhoan.TenDangNhap, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Tên đăng nhập đã tồn tại trong danh sách tài khoản.", "Trùng tài khoản", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (BieuMauTaiKhoan.DangSua
            && !string.Equals(BieuMauTaiKhoan.TenDangNhapGoc, BieuMauTaiKhoan.TenDangNhap, StringComparison.OrdinalIgnoreCase)
            && CacTaiKhoanHeThong.Any(x => string.Equals(x.TenDangNhap, BieuMauTaiKhoan.TenDangNhap, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Tên đăng nhập mới đã tồn tại trong danh sách tài khoản.", "Trùng tài khoản", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DangDungSql)
        {
            try
            {
                await khoDuLieu.LuuTaiKhoanAsync(BieuMauTaiKhoan, TenDangNhap);
                await TaiDanhSachTaiKhoan();
            }
            catch (Exception loi)
            {
                MessageBox.Show($"Không thể lưu tài khoản: {loi.Message}", "Lỗi dữ liệu tài khoản", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            CapNhatTaiKhoanCucBo();
        }

        ThemThongBao(
            BieuMauTaiKhoan.DangSua ? "Sửa thông tin tài khoản" : "Thêm mới tài khoản",
            $"{BieuMauTaiKhoan.TenDangNhap} đã được lưu thông tin tài khoản.",
            "Cài đặt tài khoản");
        BieuMauTaiKhoan.XoaTrang();
        BaoThayDoi(nameof(CacTaiKhoanDangHoatDong));
    }

    private void CapNhatTaiKhoanCucBo()
    {
        var taiKhoan = new TaiKhoanHeThong(
            BieuMauTaiKhoan.TenDangNhap,
            BieuMauTaiKhoan.HoTen,
            BieuMauTaiKhoan.VaiTro,
            LayMoTaQuyen(BieuMauTaiKhoan.VaiTro),
            BieuMauTaiKhoan.DangHoatDong ? "Đang hoạt động" : "Tạm khóa",
            BieuMauTaiKhoan.DangSua ? TaiKhoanDangChon?.LanDangNhapGanNhat ?? DateTime.MinValue : DateTime.MinValue);

        var viTri = BieuMauTaiKhoan.DangSua
            ? CacTaiKhoanHeThong
                .Select((giaTri, chiSo) => new { giaTri, chiSo })
                .FirstOrDefault(x => string.Equals(x.giaTri.TenDangNhap, BieuMauTaiKhoan.TenDangNhapGoc, StringComparison.OrdinalIgnoreCase))?.chiSo ?? -1
            : -1;

        if (viTri >= 0)
        {
            CacTaiKhoanHeThong[viTri] = taiKhoan;
            TaiKhoanDangChon = CacTaiKhoanHeThong[viTri];
            return;
        }

        CacTaiKhoanHeThong.Add(taiKhoan);
        TaiKhoanDangChon = taiKhoan;
    }

    private async Task TaiDanhSachTaiKhoan()
    {
        if (!DangDungSql)
        {
            return;
        }

        try
        {
            var danhSach = await khoDuLieu.TaiTaiKhoanHeThongAsync();
            CacTaiKhoanHeThong.Clear();
            foreach (var taiKhoan in danhSach)
            {
                CacTaiKhoanHeThong.Add(taiKhoan);
            }

            TaiKhoanDangChon = CacTaiKhoanHeThong.FirstOrDefault(x =>
                string.Equals(x.TenDangNhap, TenDangNhap, StringComparison.OrdinalIgnoreCase));
            BaoThayDoi(nameof(CacTaiKhoanDangHoatDong));
        }
        catch (Exception loi)
        {
            HienThongBaoNhanh($"Không thể tải danh sách tài khoản SQL: {loi.Message}");
        }
    }

    private void SaoLuuDuLieu()
    {
        var hopThoai = new SaveFileDialog
        {
            Title = "Sao lưu dữ liệu nhân sự",
            FileName = $"QuanLyNhanSu_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.hrmbackup.json",
            Filter = "HRM backup (*.hrmbackup.json)|*.hrmbackup.json|JSON (*.json)|*.json",
            AddExtension = true
        };

        if (hopThoai.ShowDialog() != true)
        {
            return;
        }

        var banSao = BanSaoDuLieuNhanSu.TaoTu(DuLieu);
        var tuyChon = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(hopThoai.FileName, JsonSerializer.Serialize(banSao, tuyChon));
        ThemThongBao("Sao lưu dữ liệu", $"Đã tạo bản sao lưu tại {hopThoai.FileName}.", "Cài đặt tài khoản");
        MessageBox.Show($"Đã sao lưu dữ liệu:\n{hopThoai.FileName}", "Sao lưu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PhucHoiDuLieu()
    {
        var hopThoai = new OpenFileDialog
        {
            Title = "Phục hồi dữ liệu nhân sự",
            Filter = "HRM backup (*.hrmbackup.json)|*.hrmbackup.json|JSON (*.json)|*.json|Tất cả tệp|*.*",
            Multiselect = false
        };

        if (hopThoai.ShowDialog() != true)
        {
            return;
        }

        if (MessageBox.Show("Phục hồi sẽ thay dữ liệu đang hiển thị trong phiên làm việc hiện tại. Bạn muốn tiếp tục?", "Phục hồi dữ liệu", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var banSao = JsonSerializer.Deserialize<BanSaoDuLieuNhanSu>(File.ReadAllText(hopThoai.FileName));
            if (banSao is null)
            {
                MessageBox.Show("Tệp sao lưu không hợp lệ.", "Phục hồi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DuLieu = banSao.TaoKhoDuLieu();
            ApDungTrangThaiDocThongBao();
            NguonDuLieu = $"Bản phục hồi cục bộ - {Path.GetFileName(hopThoai.FileName)}";
            CauHinhDanhSachNhanVienView();
            DanhSachThongBaoView = CollectionViewSource.GetDefaultView(DuLieu.ThongBao);
            DanhSachThongBaoView.Filter = LocThongBao;
            BaoThayDoi(nameof(DanhSachThongBaoView));
            DanhSachUngVienView = CollectionViewSource.GetDefaultView(DuLieu.UngVien);
            DanhSachUngVienView.Filter = obj => obj is UngVien ungVien && LocUngVien(ungVien);
            BaoThayDoi(nameof(DanhSachUngVienView));
            DanhSachPhieuLuongView = CollectionViewSource.GetDefaultView(DuLieu.PhieuLuong);
            DanhSachPhieuLuongView.Filter = obj => obj is PhieuLuong phieuLuong && LocPhieuLuong(phieuLuong);
            DanhSachPhieuLuongView.SortDescriptions.Clear();
            DanhSachPhieuLuongView.SortDescriptions.Add(new SortDescription(nameof(PhieuLuong.KyLuong), ListSortDirection.Descending));
            DanhSachPhieuLuongView.SortDescriptions.Add(new SortDescription(nameof(PhieuLuong.NhanVien), ListSortDirection.Ascending));
            BaoThayDoi(nameof(DanhSachPhieuLuongView));
            DanhSachChamCongView = CollectionViewSource.GetDefaultView(DuLieu.ChamCong);
            DanhSachChamCongView.Filter = obj => obj is ChamCong chamCong && LocChamCong(chamCong);
            DanhSachChamCongView.SortDescriptions.Clear();
            DanhSachChamCongView.SortDescriptions.Add(new SortDescription(nameof(ChamCong.GioVao), ListSortDirection.Descending));
            DanhSachChamCongView.SortDescriptions.Add(new SortDescription(nameof(ChamCong.NhanVien), ListSortDirection.Ascending));
            BaoThayDoi(nameof(DanhSachChamCongView));
            CauHinhDanhSachNghiPhepView();
            DanhSachDanhGiaView = CollectionViewSource.GetDefaultView(DuLieu.DanhGia);
            DanhSachDanhGiaView.Filter = obj => obj is DanhGia danhGia && LocDanhGia(danhGia);
            DanhSachDanhGiaView.SortDescriptions.Clear();
            DanhSachDanhGiaView.SortDescriptions.Add(new SortDescription(nameof(DanhGia.KyDanhGia), ListSortDirection.Descending));
            DanhSachDanhGiaView.SortDescriptions.Add(new SortDescription(nameof(DanhGia.NhanVien), ListSortDirection.Ascending));
            BaoThayDoi(nameof(DanhSachDanhGiaView));
            NhanVienDangChon = ChonNhanVienTrongPhamVi();
            KhoiTaoBieuMauNghiepVu();
            BaoCaoThongKe();
            LamMoiThongBao();
            ThemThongBao("Phục hồi dữ liệu", $"Đã phục hồi dữ liệu từ {hopThoai.FileName}.", "Cài đặt tài khoản");
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể phục hồi dữ liệu: {loi.Message}", "Phục hồi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool DangDungSql => NguonDuLieu.StartsWith("SQL Server", StringComparison.OrdinalIgnoreCase);

    private async Task ChayLenhDuLieu(Func<Task> hanhDong)
    {
        try
        {
            await hanhDong();
            if (DangDungSql)
            {
                await TaiDuLieu();
            }
            else
            {
                DanhSachNhanVienView.Refresh();
                BaoCaoThongKe();
                LamMoiLenhChonNhanVien();
                LamMoiThongBao();
            }
        }
        catch (Exception loi)
        {
            MessageBox.Show($"Không thể thực hiện thao tác: {loi.Message}", "Lỗi nghiệp vụ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TaiTrangThaiDocThongBao()
    {
        try
        {
            var duongDan = DuongDanLuuTrangThaiDocThongBao();
            if (!File.Exists(duongDan))
            {
                return;
            }

            var danhSach = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(duongDan));
            if (danhSach is null)
            {
                return;
            }

            foreach (var khoa in danhSach.Where(khoa => !string.IsNullOrWhiteSpace(khoa)))
            {
                khoaThongBaoDaDoc.Add(khoa);
            }
        }
        catch
        {
            // Trạng thái đọc chỉ là dữ liệu hỗ trợ giao diện, bỏ qua nếu file cục bộ bị lỗi.
        }
    }

    private void LuuTrangThaiDocThongBao()
    {
        try
        {
            var duongDan = DuongDanLuuTrangThaiDocThongBao();
            Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
            File.WriteAllText(duongDan, JsonSerializer.Serialize(khoaThongBaoDaDoc.OrderBy(x => x).ToList()));
        }
        catch
        {
            // Không chặn nghiệp vụ nếu Windows không cho ghi file trạng thái cục bộ.
        }
    }

    private string DuongDanLuuTrangThaiDocThongBao()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyNhanSuWpf",
            "ThongBao",
            $"DaDoc_{LamSachTenFile(TenDangNhap)}.json");
    }

    private void ApDungTrangThaiDocThongBao()
    {
        foreach (var thongBao in DuLieu.ThongBao)
        {
            ApDungTrangThaiDocThongBao(thongBao);
        }
    }

    private void ApDungTrangThaiDocThongBao(ThongBaoHeThong thongBao)
    {
        if (khoaThongBaoDaDoc.Contains(TaoKhoaThongBao(thongBao)))
        {
            thongBao.DaDoc = true;
        }
    }

    private void CapNhatDocThongBao(object? thamSo)
    {
        if (thamSo is ThongBaoHeThong thongBao)
        {
            LuuMotTrangThaiDocThongBao(thongBao);
        }
        else
        {
            foreach (var dong in DuLieu.ThongBao)
            {
                LuuMotTrangThaiDocThongBao(dong, luuNgay: false);
            }

            LuuTrangThaiDocThongBao();
        }

        LamMoiThongBao();
    }

    private void DanhDauThongBaoDaDoc(ThongBaoHeThong thongBao)
    {
        thongBao.DaDoc = true;
        LuuMotTrangThaiDocThongBao(thongBao);
    }

    private void LuuMotTrangThaiDocThongBao(ThongBaoHeThong thongBao, bool luuNgay = true)
    {
        var khoa = TaoKhoaThongBao(thongBao);
        if (thongBao.DaDoc)
        {
            khoaThongBaoDaDoc.Add(khoa);
        }
        else
        {
            khoaThongBaoDaDoc.Remove(khoa);
        }

        if (luuNgay)
        {
            LuuTrangThaiDocThongBao();
        }
    }

    private static string TaoKhoaThongBao(ThongBaoHeThong thongBao)
    {
        return $"{thongBao.PhanHe}|{thongBao.TieuDe}|{thongBao.NoiDung}|{thongBao.MucDo}|{thongBao.TenTepDinhKem}";
    }

    private void ThemThongBaoVaoDanhSach(ThongBaoHeThong thongBao)
    {
        ApDungTrangThaiDocThongBao(thongBao);
        if (!thongBaoPhienLamViec.Any(x => LaCungThongBao(x, thongBao)))
        {
            thongBaoPhienLamViec.Insert(0, thongBao);
        }

        if (!DuLieu.ThongBao.Any(x => LaCungThongBao(x, thongBao)))
        {
            DuLieu.ThongBao.Insert(0, thongBao);
        }
    }

    private void NhapThongBaoPhienLamViec()
    {
        for (var i = thongBaoPhienLamViec.Count - 1; i >= 0; i--)
        {
            var thongBao = thongBaoPhienLamViec[i];
            ApDungTrangThaiDocThongBao(thongBao);
            if (!DuLieu.ThongBao.Any(x => LaCungThongBao(x, thongBao)))
            {
                DuLieu.ThongBao.Insert(0, thongBao);
            }
        }
    }

    private static bool LaCungThongBao(ThongBaoHeThong trai, ThongBaoHeThong phai)
    {
        return ReferenceEquals(trai, phai)
            || (string.Equals(trai.TieuDe, phai.TieuDe, StringComparison.Ordinal)
                && string.Equals(trai.NoiDung, phai.NoiDung, StringComparison.Ordinal)
                && string.Equals(trai.PhanHe, phai.PhanHe, StringComparison.Ordinal)
                && string.Equals(trai.TenTepDinhKem, phai.TenTepDinhKem, StringComparison.Ordinal)
                && Math.Abs((trai.ThoiGian - phai.ThoiGian).TotalSeconds) < 1);
    }

    private void ThemThongBao(string tieuDe, string noiDung, string phanHe)
    {
        var thongBao = new ThongBaoHeThong(tieuDe, noiDung, phanHe, DateTime.Now, "Thông tin");
        ThemThongBaoVaoDanhSach(thongBao);
        LamMoiThongBao();
        HienThongBaoNhanh(noiDung);
        if (DangDungSql)
        {
            _ = khoDuLieu.GhiNhatKyAsync(TenDangNhap, tieuDe, phanHe, "", noiDung);
        }
    }

    private void BaoDaXong(string tieuDe, string noiDung, string phanHe)
    {
        ThemThongBao(tieuDe, noiDung, phanHe);
    }

    private void HienThongBaoNhanh(string noiDung)
    {
        ThongBaoNhanh = $"Đã xong: {noiDung}";
        DangHienThongBaoNhanh = true;
        boDemThongBaoNhanh.Stop();
        boDemThongBaoNhanh.Start();
    }

    private static string LayGiaiDoanUngVienTiepTheo(string giaiDoanHienTai)
    {
        return giaiDoanHienTai switch
        {
            "New" => "Sàng lọc hồ sơ",
            "Mới" => "Sàng lọc hồ sơ",
            "Screening" => "Phỏng vấn",
            "Sàng lọc hồ sơ" => "Phỏng vấn",
            "Interview" => "Đề nghị nhận việc",
            "Phỏng vấn" => "Đề nghị nhận việc",
            "Offer" => "Đề nghị nhận việc",
            "Đề nghị nhận việc" => "Đề nghị nhận việc",
            "Signed" => "Đã tiếp nhận",
            "Đã ký" => "Đã tiếp nhận",
            "Đã tiếp nhận" => "Đã tiếp nhận",
            "Rejected" => "Từ chối",
            "Từ chối" => "Từ chối",
            _ => "Sàng lọc hồ sơ"
        };
    }

    private void LamMoiLenhChonNhanVien()
    {
        (ThemMoiLenh as LenhGiaoDien)?.LamMoi();
        (SuaLenh as LenhGiaoDien)?.LamMoi();
        (LuuLenh as LenhGiaoDien)?.LamMoi();
        (XoaLenh as LenhGiaoDien)?.LamMoi();
        (VaoCaLenh as LenhGiaoDien)?.LamMoi();
        (RaCaLenh as LenhGiaoDien)?.LamMoi();
        (TaoDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (TaoMoiDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (SuaDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (XoaDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (ChotDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (TinhLuongLenh as LenhGiaoDien)?.LamMoi();
        (XacNhanTraLuongLenh as LenhGiaoDien)?.LamMoi();
        (XuatBaoCaoLuongLenh as LenhGiaoDien)?.LamMoi();
        BaoThayDoi(nameof(NhanVienThaoTac));
        BaoThayDoi(nameof(TongGioCongThangDangChon));
        BaoThayDoi(nameof(NgayCongQuyDoiDangChon));
        BaoThayDoi(nameof(LuongCoBanDuKienDangChon));
        BaoThayDoi(nameof(CoTheVaoCaDangChon));
        BaoThayDoi(nameof(CoTheRaCaDangChon));
        BaoThayDoi(nameof(TrangThaiCaDangChon));
        BaoThayDoi(nameof(CaGanNhatDangChon));
    }

    private void LamMoiLenhDanhGia()
    {
        (TaoMoiDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (SuaDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (TaoDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (XoaDanhGiaLenh as LenhGiaoDien)?.LamMoi();
        (ChotDanhGiaLenh as LenhGiaoDien)?.LamMoi();
    }

    private void LamMoiLenhNghiPhep()
    {
        (DuyetNghiPhepLenh as LenhGiaoDien)?.LamMoi();
        (TuChoiNghiPhepLenh as LenhGiaoDien)?.LamMoi();
        (DuyetNghiPhepDongLenh as LenhGiaoDien)?.LamMoi();
        (TuChoiNghiPhepDongLenh as LenhGiaoDien)?.LamMoi();
    }

    private void NeuDangSuaNhanVienThiNapBieuMau()
    {
        if (MucDangChon == "Nhân viên" && NhanVienDangChon is not null)
        {
            BieuMauNhanVien = NhanVienDangChon.TaoBanSao();
        }
    }

    private void LamMoiLenhChonPhongBan()
    {
        (SuaPhongBanLenh as LenhGiaoDien)?.LamMoi();
        (LuuPhongBanLenh as LenhGiaoDien)?.LamMoi();
        (XoaPhongBanLenh as LenhGiaoDien)?.LamMoi();
        (GanTruongPhongLenh as LenhGiaoDien)?.LamMoi();
    }

    private void NeuDangSuaPhongBanThiNapBieuMau()
    {
        if (MucDangChon == "Phòng ban" && PhongBanDangChon is not null)
        {
            DuaPhongBanVaoBieuMau();
        }
    }

    private static void CapNhatNhanVien(NhanVien dich, NhanVien nguon)
    {
        dich.MaSo = nguon.MaSo;
        dich.HoTen = nguon.HoTen;
        dich.MaPhongBan = nguon.MaPhongBan;
        dich.MaViTri = nguon.MaViTri;
        dich.PhongBan = nguon.PhongBan;
        dich.ViTri = nguon.ViTri;
        dich.NgaySinh = nguon.NgaySinh;
        dich.NgayThamGiaBaoHiemXaHoi = nguon.NgayThamGiaBaoHiemXaHoi;
        dich.NgayVaoLam = nguon.NgayVaoLam;
        dich.DangLamViec = nguon.DangLamViec;
        dich.LienHeKhanCap = nguon.LienHeKhanCap;
        dich.TaiKhoanNganHang = nguon.TaiKhoanNganHang;
        dich.SoCanCuoc = nguon.SoCanCuoc;
    }

    private void CapNhatTenPhongBanNhanVien(int maPhongBan, string tenPhongBan)
    {
        foreach (var nhanVien in DuLieu.NhanVien.Where(n => n.MaPhongBan == maPhongBan))
        {
            nhanVien.PhongBan = tenPhongBan;
        }
    }

    private void CapNhatPhongBanSauKhiXoaNhanVien(string tenNhanVien)
    {
        for (var i = 0; i < DuLieu.PhongBan.Count; i++)
        {
            if (DuLieu.PhongBan[i].TruongPhong.Equals(tenNhanVien, StringComparison.OrdinalIgnoreCase))
            {
                DuLieu.PhongBan[i] = DuLieu.PhongBan[i] with { TruongPhong = "Chưa phân công" };
            }
        }
    }

    private void XoaDuLieuLienQuanNhanVien(string tenNhanVien)
    {
        XoaNhieu(DuLieu.ChamCong, x => x.NhanVien == tenNhanVien);
        XoaNhieu(DuLieu.NghiPhep, x => x.NhanVien == tenNhanVien);
        XoaNhieu(DuLieu.DanhGia, x => x.NhanVien == tenNhanVien || x.NguoiDanhGia == tenNhanVien);
        XoaNhieu(DuLieu.PhieuLuong, x => x.NhanVien == tenNhanVien);
    }

    private void XoaViTriTheoPhongBan(int maPhongBan)
    {
        XoaNhieu(DuLieu.ViTri, x => x.MaPhongBan == maPhongBan);
    }

    private static void XoaNhieu<T>(ObservableCollection<T> danhSach, Func<T, bool> dieuKien)
    {
        foreach (var dong in danhSach.Where(dieuKien).ToList())
        {
            danhSach.Remove(dong);
        }
    }

    private void BaoCaoThongKe()
    {
        BaoThayDoi(nameof(TongNhanVien));
        BaoThayDoi(nameof(NhanSuConHieuLuc));
        BaoThayDoi(nameof(NghiPhepDaDuyetHomNay));
        BaoThayDoi(nameof(NhanSuTamVangHomNay));
        BaoThayDoi(nameof(DangLamViec));
        BaoThayDoi(nameof(TamNghi));
        BaoThayDoi(nameof(NghiChoDuyet));
        BaoThayDoi(nameof(TongQuyLuong));
        BaoThayDoi(nameof(TongLuongChoTra));
        BaoThayDoi(nameof(SoPhieuLuongChoTra));
        BaoThayDoi(nameof(TongHopLuongTheoPhongBan));
        BaoThayDoi(nameof(SoChamCongHomNay));
        BaoThayDoi(nameof(TongGioCongHomNay));
        BaoThayDoi(nameof(SoChamCongTrongKyBaoCao));
        BaoThayDoi(nameof(TongGioCongTrongKyBaoCao));
        BaoThayDoi(nameof(SoNghiPhepTrongKyBaoCao));
        BaoThayDoi(nameof(NhanSuNghiDaDuyetTrongKyBaoCao));
        BaoThayDoi(nameof(QuanSoHienDienTrongKyBaoCao));
        BaoThayDoi(nameof(DanhSachNghiPhepDaDuyetTheoKy));
        BaoThayDoi(nameof(TieuDeAiDangNghi));
        BaoThayDoi(nameof(MoTaAiDangNghi));
        BaoThayDoi(nameof(SoPhieuLuongTrongKyBaoCao));
        BaoThayDoi(nameof(KyLuongGanNhatHienThi));
        BaoThayDoi(nameof(TomTatBaoCaoNhanSu));
        BaoThayDoi(nameof(TongGioCongThangDangChon));
        BaoThayDoi(nameof(NgayCongQuyDoiDangChon));
        BaoThayDoi(nameof(LuongCoBanDuKienDangChon));
        LamMoiBoLocNhanVien();
        LamMoiBoLocBangLuong();
        LamMoiBoLocChamCong();
        LamMoiBoLocNghiPhep();
        LamMoiBoLocDanhGia();
        LamMoiBoLocUngVien();
        CapNhatDuLieuBieuDo();
        CapNhatTraCuuDieuHanh();
    }

    private void LamMoiThongBao()
    {
        DanhSachThongBaoView.Refresh();
        BaoThayDoi(nameof(SoThongBao));
        BaoThayDoi(nameof(SoThongBaoChuaDoc));
    }

    private void CapNhatDuLieuBieuDo()
    {
        var namHienTai = DateTime.Today.Year;
        DuLieuLuong12Thang = new ObservableCollection<DiemLuongThang>(
            Enumerable.Range(1, 12).Select(thang =>
            {
                var khoaThang = $"{namHienTai}-{thang:00}";
                var phieuLuongThang = DuLieu.PhieuLuong
                    .Where(p => CoTheXemTheoTen(p.NhanVien) && p.KyLuong.StartsWith(khoaThang, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var ngayDauThang = new DateTime(namHienTai, thang, 1);
                var ngayCuoiThang = new DateTime(namHienTai, thang, DateTime.DaysInMonth(namHienTai, thang));
                var nghiPhepTrongThang = DuLieu.NghiPhep
                    .Where(LocNghiPhep)
                    .Where(nghiPhep => QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai))
                    .Where(nghiPhep => QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, ngayDauThang, ngayCuoiThang))
                    .Select(nghiPhep => nghiPhep.NhanVien)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                return new DiemLuongThang(
                    thang.ToString("00"),
                    phieuLuongThang.Sum(p => p.ThucLanh),
                    LayNhanVienTrongPhamVi().Count(n => n.DangLamViec && n.NgayVaoLam.Date <= ngayCuoiThang && !nghiPhepTrongThang.Contains(n.HoTen)));
            }));

        DuLieuUngVienTheoViTri = new ObservableCollection<MucUngVienTheoViTri>(
            DuLieu.UngVien
                .Where(LocUngVien)
                .GroupBy(u => string.IsNullOrWhiteSpace(u.ViTri) ? "Chưa xác định" : u.ViTri)
                .Select(g => new MucUngVienTheoViTri(g.Key, g.Count()))
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.TenViTri)
                .Take(8));
    }

    private void CapNhatTraCuuDieuHanh()
    {
        var tuKhoa = TuKhoaTraCuuDieuHanh.Trim();
        var dangLocTatCaPhongBan = string.Equals(PhongBanTraCuuDangChon, TatCaPhongBan, StringComparison.OrdinalIgnoreCase);
        var nghiPhepHomNay = LayTenNhanVienNghiDaDuyetNgay(DateTime.Today);
        var phieuLuongTheoNhanVien = DuLieu.PhieuLuong
            .GroupBy(x => x.NhanVien, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(p => p.KyLuong).ThenByDescending(p => p.ThucLanh).First(),
                StringComparer.OrdinalIgnoreCase);
        var diemTheoNhanVien = DuLieu.DanhGia
            .GroupBy(x => x.NhanVien, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => (decimal?)x.Max(p => p.Diem),
                StringComparer.OrdinalIgnoreCase);

        var tatCaNhanSu = LayNhanVienTrongPhamVi()
            .Select(nhanVien =>
            {
                phieuLuongTheoNhanVien.TryGetValue(nhanVien.HoTen, out var phieuLuong);
                diemTheoNhanVien.TryGetValue(nhanVien.HoTen, out var diemDanhGia);

                return new DongTraCuuNhanSu(
                    LayThuTuCapBac(nhanVien.ViTri),
                    LayTenCapBac(nhanVien.ViTri),
                    nhanVien.MaSo,
                    nhanVien.HoTen,
                    nhanVien.PhongBan,
                    nhanVien.ViTri,
                    diemDanhGia,
                    phieuLuong?.LuongCoBan ?? LayLuongCoBan(nhanVien),
                    phieuLuong?.ThucLanh ?? 0,
                    phieuLuong?.KyLuong ?? "--");
            })
            .ToList();

        var thuTuPhongBan = LayPhongBanTrongPhamVi()
            .Select(phongBan => new { phongBan.TenPhongBan, ThuTu = LayThuTuPhongBan(phongBan.MaPhongBan, phongBan.TenPhongBan) })
            .OrderBy(x => x.ThuTu)
            .ThenBy(x => x.TenPhongBan)
            .ToList();

        var tongHopPhongBan = LayPhongBanTrongPhamVi()
            .Where(phongBan => dangLocTatCaPhongBan
                || string.Equals(phongBan.TenPhongBan, PhongBanTraCuuDangChon, StringComparison.OrdinalIgnoreCase))
            .Select(phongBan =>
            {
                var nhanSu = tatCaNhanSu
                    .Where(x => string.Equals(x.PhongBan, phongBan.TenPhongBan, StringComparison.OrdinalIgnoreCase))
                    .Where(x => KiemTraTuKhoaTraCuu(x, tuKhoa))
                    .OrderBy(x => LaTruongPhongChinhThuc(x.HoTen, x.PhongBan) ? 0 : 1)
                    .ThenBy(x => x.ThuTuCapBac)
                    .ThenByDescending(x => x.ThucLanh)
                    .ToList();
                var truongPhong = DuLieu.NhanVien.FirstOrDefault(nhanVien =>
                        string.Equals(nhanVien.HoTen, phongBan.TruongPhong, StringComparison.OrdinalIgnoreCase)
                        && nhanVien.MaPhongBan == phongBan.MaPhongBan)
                    ?? DuLieu.NhanVien.FirstOrDefault(nhanVien =>
                        string.Equals(nhanVien.HoTen, phongBan.TruongPhong, StringComparison.OrdinalIgnoreCase));
                var xuatSac = nhanSu
                    .Where(x => x.DiemDanhGia.HasValue)
                    .OrderByDescending(x => x.DiemDanhGia)
                    .ThenBy(x => x.ThuTuCapBac)
                    .FirstOrDefault();
                var luongCao = nhanSu
                    .Where(x => x.ThucLanh > 0)
                    .OrderByDescending(x => x.ThucLanh)
                    .ThenBy(x => x.ThuTuCapBac)
                    .FirstOrDefault();
                var diemXuatSac = xuatSac?.DiemDanhGia;

                return new TongHopPhongBanDieuHanh(
                    LayThuTuPhongBan(phongBan.MaPhongBan, phongBan.TenPhongBan),
                    phongBan.TenPhongBan,
                    phongBan.TruongPhong,
                    truongPhong?.ViTri ?? "Chưa bố trí",
                    nhanSu.Count(x => !nghiPhepHomNay.Contains(x.HoTen)),
                    nhanSu.Sum(x => x.ThucLanh),
                    xuatSac?.HoTen ?? "Chưa có đánh giá",
                    diemXuatSac.HasValue ? $"{diemXuatSac.Value:N1}" : "--",
                    luongCao?.HoTen ?? "Chưa có bảng lương",
                    luongCao is null ? "--" : $"{luongCao.ThucLanh:N0} đ");
            })
            .Where(x => string.IsNullOrWhiteSpace(tuKhoa) || x.SoNhanVien > 0)
            .OrderBy(x => x.ThuTuCapBac)
            .ThenBy(x => x.TenPhongBan)
            .ToList();

        TongHopPhongBanDieuHanh = new ObservableCollection<TongHopPhongBanDieuHanh>(tongHopPhongBan);
        CacPhongBanTraCuu =
        [
            TatCaPhongBan,
            .. thuTuPhongBan.Select(x => x.TenPhongBan)
        ];

        if (!CacPhongBanTraCuu.Contains(PhongBanTraCuuDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanTraCuuDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanTraCuuDangChon));
        }

        if (!CacPhongBanTraCuu.Contains(PhongBanNhanVienDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanNhanVienDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanNhanVienDangChon));
        }

        if (!CacPhongBanTraCuu.Contains(PhongBanBangLuongDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanBangLuongDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanBangLuongDangChon));
        }

        if (!CacPhongBanTraCuu.Contains(PhongBanChamCongDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanChamCongDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanChamCongDangChon));
        }

        if (!CacPhongBanTraCuu.Contains(PhongBanNghiPhepDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanNghiPhepDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanNghiPhepDangChon));
        }

        if (!CacPhongBanTraCuu.Contains(PhongBanDanhGiaDangChon, StringComparer.OrdinalIgnoreCase))
        {
            phongBanDanhGiaDangChon = TatCaPhongBan;
            BaoThayDoi(nameof(PhongBanDanhGiaDangChon));
        }

        LamMoiBoLocNhanVien();
        LamMoiBoLocBangLuong();
        LamMoiBoLocChamCong();
        LamMoiBoLocNghiPhep();

        DanhSachNhanSuTraCuu = new ObservableCollection<DongTraCuuNhanSu>(
            tatCaNhanSu
                .Where(x => dangLocTatCaPhongBan
                    || string.Equals(x.PhongBan, PhongBanTraCuuDangChon, StringComparison.OrdinalIgnoreCase))
                .Where(x => KiemTraTuKhoaTraCuu(x, tuKhoa))
                .OrderBy(x => LaTruongPhongChinhThuc(x.HoTen, x.PhongBan) ? 0 : 1)
                .ThenBy(x => LayThuTuPhongBan(x.PhongBan))
                .ThenBy(x => x.ThuTuCapBac)
                .ThenByDescending(x => x.ThucLanh)
                .ThenBy(x => x.HoTen));

        BaoThayDoi(nameof(PhamViTraCuu));
        BaoThayDoi(nameof(SoNhanSuTraCuu));
        BaoThayDoi(nameof(QuyLuongTraCuu));
        BaoThayDoi(nameof(CaNhanXuatSacNhat));
        BaoThayDoi(nameof(CaNhanXuatSacHienThi));
        BaoThayDoi(nameof(ThanhTichXuatSacHienThi));
        BaoThayDoi(nameof(NhanSuLuongCaoNhat));
        BaoThayDoi(nameof(NhanSuLuongCaoHienThi));
        BaoThayDoi(nameof(LuongCaoHienThi));
    }

    private static bool KiemTraTuKhoaTraCuu(DongTraCuuNhanSu nhanSu, string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
        {
            return true;
        }

        return ChuaTuKhoa(nhanSu.HoTen, tuKhoa)
            || ChuaTuKhoa(nhanSu.MaSo, tuKhoa)
            || ChuaTuKhoa(nhanSu.PhongBan, tuKhoa)
            || ChuaTuKhoa(nhanSu.ViTri, tuKhoa)
            || ChuaTuKhoa(nhanSu.CapBac, tuKhoa);
    }

    private static bool ChuaTuKhoa(string giaTri, string tuKhoa)
    {
        return giaTri.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase);
    }

    private static int LayThuTuCapBac(string chucVu)
    {
        return BangXepHangChucVu.LayThuTu(chucVu);
    }

    private static string LayTenCapBac(string chucVu) => BangXepHangChucVu.LayTenCapBac(chucVu);

    private void MoBieuMauThongBao()
    {
        if (!CoQuyenTaoThongBao)
        {
            MessageBox.Show("Tài khoản nhân viên chỉ được xem thông báo, không được tạo thông báo mới.", "Phân quyền thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(PhanHeThongBaoMoi))
        {
            PhanHeThongBaoMoi = MucDangChon == "Tổng quan" ? "Hệ thống" : MucDangChon;
        }

        DangMoBieuMauThongBao = true;
    }

    private void GuiThongBaoMoi()
    {
        if (!CoQuyenTaoThongBao)
        {
            MessageBox.Show("Tài khoản nhân viên chỉ được xem thông báo, không được tạo thông báo mới.", "Phân quyền thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(TieuDeThongBaoMoi) || string.IsNullOrWhiteSpace(NoiDungThongBaoMoi))
        {
            MessageBox.Show("Vui lòng nhập tiêu đề và nội dung thông báo.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var tieuDe = TieuDeThongBaoMoi.Trim();
        var noiDung = $"{NoiDungThongBaoMoi.Trim()}\nNgười gửi: {TenNguoiDung} ({VaiTroNguoiDung})";
        var phanHe = string.IsNullOrWhiteSpace(PhanHeThongBaoMoi) ? "Hệ thống" : PhanHeThongBaoMoi.Trim();
        var mucDo = string.IsNullOrWhiteSpace(MucDoThongBaoMoi) ? "Thông tin" : MucDoThongBaoMoi.Trim();
        var tenTepDinhKem = "";
        var duongDanTepDinhKem = "";

        if (CoTepThongBaoMoi)
        {
            duongDanTepDinhKem = LuuTepDinhKemThongBao(DuongDanTepThongBaoMoi);
            tenTepDinhKem = TenTepThongBaoMoi;
        }

        var thongBao = new ThongBaoHeThong(tieuDe, noiDung, phanHe, DateTime.Now, mucDo, false, tenTepDinhKem, duongDanTepDinhKem);
        ThemThongBaoVaoDanhSach(thongBao);
        LamMoiThongBao();
        DatLaiBieuMauThongBao();
        DangMoBieuMauThongBao = false;
        HienThongBaoNhanh("Đã gửi thông báo mới.");
    }

    private void DongBieuMauThongBao()
    {
        DatLaiBieuMauThongBao();
        DangMoBieuMauThongBao = false;
    }

    private void DatLaiBieuMauThongBao()
    {
        TieuDeThongBaoMoi = "";
        NoiDungThongBaoMoi = "";
        PhanHeThongBaoMoi = MucDangChon == "Tổng quan" ? "Hệ thống" : MucDangChon;
        MucDoThongBaoMoi = "Thông tin";
        XoaTepThongBao();
    }

    private void ChonTepThongBao()
    {
        var hopThoai = new OpenFileDialog
        {
            Title = "Chọn tệp đính kèm thông báo",
            Filter = "Tệp thường dùng|*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.pdf;*.png;*.jpg;*.jpeg;*.txt|Tất cả tệp|*.*",
            Multiselect = false
        };

        if (hopThoai.ShowDialog() != true)
        {
            return;
        }

        DuongDanTepThongBaoMoi = hopThoai.FileName;
        TenTepThongBaoMoi = Path.GetFileName(hopThoai.FileName);
    }

    private void XoaTepThongBao()
    {
        DuongDanTepThongBaoMoi = "";
        TenTepThongBaoMoi = "";
    }

    private void MoThongBao(object? thamSo)
    {
        if (thamSo is not ThongBaoHeThong thongBao)
        {
            return;
        }

        DanhDauThongBaoDaDoc(thongBao);
        LamMoiThongBao();

        var phanHe = thongBao.PhanHe;
        var vanBan = TaoVanBanThongBao(thongBao);
        if (CoTuKhoaThongBao(phanHe, "Nghỉ phép", "nghi phep") || CoTuKhoaThongBao(vanBan, "đơn nghỉ", "nghỉ phép", "nghi phep"))
        {
            MoThongBaoNghiPhep();
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Bảng lương", "bang luong", "payroll") || CoTuKhoaThongBao(vanBan, "phiếu lương", "bảng lương", "payroll"))
        {
            MoThongBaoBangLuong(thongBao);
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Tuyển dụng", "tuyen dung") || CoTuKhoaThongBao(vanBan, "ứng viên", "tuyển dụng", "tuyen dung"))
        {
            MoThongBaoTuyenDung(thongBao);
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Nhân viên", "nhan vien", "hồ sơ") || CoTuKhoaThongBao(vanBan, "hồ sơ nhân sự", "hồ sơ nhân viên"))
        {
            MoHoSoNhanVien(TimNhanVienTrongThongBao(thongBao));
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Chấm công", "cham cong"))
        {
            MoThongBaoChamCong(thongBao);
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Đánh giá", "danh gia"))
        {
            MoThongBaoDanhGia(thongBao);
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Phòng ban", "phong ban"))
        {
            MoThongBaoPhongBan(thongBao);
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Báo cáo", "bao cao"))
        {
            MoMucTheoThongBao("Báo cáo");
            HienThongBaoNhanh("Đã mở phân hệ báo cáo.");
            return;
        }

        if (CoTuKhoaThongBao(phanHe, "Cài đặt", "tai khoan", "tài khoản"))
        {
            MoMucTheoThongBao("Cài đặt tài khoản");
            HienThongBaoNhanh("Đã mở phân hệ tài khoản.");
            return;
        }

        MoMucTheoThongBao("Tổng quan");
        HienThongBaoNhanh("Đã mở tổng quan hệ thống.");
    }

    private void MoThongBaoNghiPhep()
    {
        if (!MoMucTheoThongBao("Nghỉ phép"))
        {
            return;
        }

        NghiPhepDangChon = DuLieu.NghiPhep
            .Where(LocNghiPhep)
            .OrderBy(x => x.ThuTuTrangThai)
            .ThenByDescending(x => x.ThuTuMoiNhat)
            .ThenByDescending(x => x.TuNgay)
            .ThenByDescending(x => x.DenNgay)
            .FirstOrDefault();

        HienThongBaoNhanh(NghiPhepDangChon is null
            ? "Đã mở phân hệ nghỉ phép."
            : $"Đã mở đơn nghỉ của {NghiPhepDangChon.NhanVien}.");
    }

    private void MoThongBaoBangLuong(ThongBaoHeThong thongBao)
    {
        if (!MoMucTheoThongBao("Bảng lương"))
        {
            return;
        }

        var phieuLuong = TimPhieuLuongTrongThongBao(thongBao);
        PhongBanBangLuongDangChon = TatCaPhongBan;
        TuKhoaBangLuong = phieuLuong?.NhanVien ?? "";
        PhieuLuongDangChon = phieuLuong;
        if (phieuLuong is not null)
        {
            NhanVienDangChon = LayNhanVienTheoTen(phieuLuong.NhanVien) ?? NhanVienDangChon;
        }

        HienThongBaoNhanh(phieuLuong is null
            ? "Đã mở phân hệ bảng lương."
            : $"Đã mở phiếu lương {phieuLuong.KyLuong} của {phieuLuong.NhanVien}.");
    }

    private void MoThongBaoTuyenDung(ThongBaoHeThong thongBao)
    {
        var ungVien = TimUngVienTrongThongBao(thongBao);
        if (ungVien is not null)
        {
            if (MoMucTheoThongBao("Ứng viên"))
            {
                UngVienDangChon = ungVien;
                HienThongBaoNhanh($"Đã mở hồ sơ ứng viên {ungVien.HoTen}.");
            }
            return;
        }

        var nhanVien = TimNhanVienTrongThongBao(thongBao);
        if (nhanVien is not null)
        {
            MoHoSoNhanVien(nhanVien);
            return;
        }

        if (MoMucTheoThongBao("Ứng viên"))
        {
            UngVienDangChon = DuLieu.UngVien.FirstOrDefault(LocUngVien);
            HienThongBaoNhanh("Đã mở phân hệ tuyển dụng.");
        }
    }

    private void MoHoSoNhanVien(NhanVien? nhanVien)
    {
        if (!MoMucTheoThongBao("Nhân viên"))
        {
            return;
        }

        PhongBanNhanVienDangChon = TatCaPhongBan;
        TuKhoaTimKiem = nhanVien?.HoTen ?? "";
        NhanVienDangChon = nhanVien ?? ChonNhanVienTrongPhamVi();
        HienThongBaoNhanh(NhanVienDangChon is null
            ? "Đã mở phân hệ hồ sơ nhân viên."
            : $"Đã mở hồ sơ nhân viên {NhanVienDangChon.HoTen}.");
    }

    private void MoThongBaoChamCong(ThongBaoHeThong thongBao)
    {
        if (!MoMucTheoThongBao("Chấm công"))
        {
            return;
        }

        var nhanVien = TimNhanVienTrongThongBao(thongBao);
        TuKhoaChamCong = nhanVien?.HoTen ?? "";
        ChamCongDangChon = DuLieu.ChamCong
            .Where(LocChamCong)
            .OrderByDescending(x => x.GioVao)
            .FirstOrDefault();
        HienThongBaoNhanh(ChamCongDangChon is null
            ? "Đã mở phân hệ chấm công."
            : $"Đã mở ca chấm công của {ChamCongDangChon.NhanVien}.");
    }

    private void MoThongBaoDanhGia(ThongBaoHeThong thongBao)
    {
        if (!MoMucTheoThongBao("Đánh giá"))
        {
            return;
        }

        var nhanVien = TimNhanVienTrongThongBao(thongBao);
        DanhGiaDangChon = DuLieu.DanhGia
            .Where(LocDanhGia)
            .Where(x => nhanVien is null || string.Equals(x.NhanVien, nhanVien.HoTen, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.KyDanhGia)
            .ThenByDescending(x => x.Diem)
            .FirstOrDefault();
        HienThongBaoNhanh(DanhGiaDangChon is null
            ? "Đã mở phân hệ đánh giá."
            : $"Đã mở đánh giá của {DanhGiaDangChon.NhanVien}.");
    }

    private void MoThongBaoPhongBan(ThongBaoHeThong thongBao)
    {
        if (!MoMucTheoThongBao("Phòng ban"))
        {
            return;
        }

        var vanBan = TaoVanBanThongBao(thongBao);
        PhongBanDangChon = DuLieu.PhongBan
            .FirstOrDefault(x => vanBan.Contains(x.TenPhongBan, StringComparison.OrdinalIgnoreCase))
            ?? DuLieu.PhongBan.FirstOrDefault();
        HienThongBaoNhanh(PhongBanDangChon is null
            ? "Đã mở phân hệ phòng ban."
            : $"Đã mở phòng ban {PhongBanDangChon.TenPhongBan}.");
    }

    private bool MoMucTheoThongBao(string muc)
    {
        if (!CoQuyenTruyCap(muc))
        {
            MessageBox.Show($"Tài khoản {VaiTroNguoiDung} chưa có quyền vào phân hệ {muc}.", "Phân quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        MucDangChon = muc;
        return true;
    }

    private NhanVien? TimNhanVienTrongThongBao(ThongBaoHeThong thongBao)
    {
        var vanBan = TaoVanBanThongBao(thongBao);
        return DuLieu.NhanVien
            .Where(CoTheXemNhanVien)
            .OrderByDescending(x => x.HoTen.Length)
            .FirstOrDefault(x => vanBan.Contains(x.HoTen, StringComparison.OrdinalIgnoreCase)
                || vanBan.Contains(x.MaSo, StringComparison.OrdinalIgnoreCase));
    }

    private UngVien? TimUngVienTrongThongBao(ThongBaoHeThong thongBao)
    {
        var vanBan = TaoVanBanThongBao(thongBao);
        return DuLieu.UngVien
            .Where(LocUngVien)
            .OrderByDescending(x => x.HoTen.Length)
            .FirstOrDefault(x => vanBan.Contains(x.HoTen, StringComparison.OrdinalIgnoreCase)
                || vanBan.Contains(x.Email, StringComparison.OrdinalIgnoreCase)
                || vanBan.Contains(x.DienThoai, StringComparison.OrdinalIgnoreCase));
    }

    private PhieuLuong? TimPhieuLuongTrongThongBao(ThongBaoHeThong thongBao)
    {
        var vanBan = TaoVanBanThongBao(thongBao);
        var nhanVien = TimNhanVienTrongThongBao(thongBao);
        var truyVan = DuLieu.PhieuLuong.Where(x => CoTheXemTheoTen(x.NhanVien));
        if (nhanVien is not null)
        {
            truyVan = truyVan.Where(x => string.Equals(x.NhanVien, nhanVien.HoTen, StringComparison.OrdinalIgnoreCase));
        }

        return truyVan
            .OrderByDescending(x => vanBan.Contains(x.KyLuong, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => x.KyLuong)
            .ThenByDescending(x => x.ThucLanh)
            .FirstOrDefault();
    }

    private static string TaoVanBanThongBao(ThongBaoHeThong thongBao)
    {
        return $"{thongBao.TieuDe} {thongBao.NoiDung} {thongBao.PhanHe} {thongBao.MucDo}";
    }

    private static bool CoTuKhoaThongBao(string vanBan, params string[] cacTuKhoa)
    {
        return cacTuKhoa.Any(tuKhoa => vanBan.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase));
    }

    private void MoTepThongBao(object? thamSo)
    {
        if (thamSo is not ThongBaoHeThong thongBao || !thongBao.CoTepDinhKem)
        {
            return;
        }

        if (!File.Exists(thongBao.DuongDanTepDinhKem))
        {
            MessageBox.Show("Không tìm thấy tệp đính kèm. Có thể tệp đã bị xóa khỏi thư mục dữ liệu.", "Tệp đính kèm", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DanhDauThongBaoDaDoc(thongBao);
        LamMoiThongBao();
        Process.Start(new ProcessStartInfo(thongBao.DuongDanTepDinhKem) { UseShellExecute = true });
    }

    private static string LuuTepDinhKemThongBao(string duongDanGoc)
    {
        if (!File.Exists(duongDanGoc))
        {
            throw new FileNotFoundException("Không tìm thấy tệp đính kèm đã chọn.", duongDanGoc);
        }

        var thuMuc = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyNhanSuWpf",
            "ThongBao",
            "TepDinhKem");
        Directory.CreateDirectory(thuMuc);

        var tenGoc = Path.GetFileNameWithoutExtension(duongDanGoc);
        var duoiFile = Path.GetExtension(duongDanGoc);
        var tenAnToan = LamSachTenFile(string.IsNullOrWhiteSpace(tenGoc) ? "TepDinhKem" : tenGoc);
        var duongDanLuu = Path.Combine(thuMuc, $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{tenAnToan}{duoiFile}");
        File.Copy(duongDanGoc, duongDanLuu, overwrite: true);
        return duongDanLuu;
    }

    private void CapNhatDongHo()
    {
        ThoiGianHienTai = DateTime.Now.ToString("HH:mm - dd/MM/yyyy");
    }

    private bool LaVaiTro(params string[] vaiTroChoPhep)
    {
        return vaiTroChoPhep.Any(x => string.Equals(VaiTroNguoiDung, x, StringComparison.OrdinalIgnoreCase));
    }

    private static string LayMoTaQuyen(string vaiTro)
    {
        return vaiTro switch
        {
            "Admin" => "Toàn quyền: tài khoản, dữ liệu, tuyển dụng, hồ sơ, chấm công, nghỉ phép, lương và báo cáo nhân sự.",
            "Giám đốc" => "Điều hành nghiệp vụ nhân sự: phòng ban, tuyển dụng, chấm công, nghỉ phép, lương, đánh giá và báo cáo.",
            "Trưởng phòng" => "Quản lý đội nhóm: hồ sơ nhân viên, tuyển dụng, chấm công, nghỉ phép, đánh giá và báo cáo; trưởng phòng nhân sự được xử lý bảng lương.",
            "Nhân viên" => "Tự phục vụ: xem hồ sơ, chấm công, nghỉ phép, phiếu lương, thông báo và đánh giá liên quan.",
            _ => "Chưa phân quyền."
        };
    }

    private static ObservableCollection<TaiKhoanHeThong> TaoTaiKhoanHeThongMau(PhienDangNhap phienDangNhap)
    {
        ObservableCollection<TaiKhoanHeThong> danhSach =
        [
            new TaiKhoanHeThong("admin", "Quản trị hệ thống", "Admin", LayMoTaQuyen("Admin"), "Đang hoạt động", DateTime.Now.AddMinutes(-5)),
            new TaiKhoanHeThong("gd001", "Nguyễn Minh Đức", "Giám đốc", LayMoTaQuyen("Giám đốc"), "Đang hoạt động", DateTime.Now.AddMinutes(-8)),
            new TaiKhoanHeThong("tp003", "Lê Thu Hà", "Trưởng phòng", LayMoTaQuyen("Trưởng phòng"), "Đang hoạt động", DateTime.Now.AddHours(-2)),
            new TaiKhoanHeThong("nv001", "Vũ Hải An", "Nhân viên", LayMoTaQuyen("Nhân viên"), "Đang hoạt động", DateTime.Now.AddDays(-1)),
            new TaiKhoanHeThong("cn001", "Vũ Tuấn Phương", "Nhân viên", LayMoTaQuyen("Nhân viên"), "Đang hoạt động", DateTime.Now.AddDays(-2))
        ];

        var viTriPhien = danhSach
            .Select((taiKhoan, viTri) => new { taiKhoan, viTri })
            .FirstOrDefault(x => string.Equals(x.taiKhoan.TenDangNhap, phienDangNhap.TenDangNhap, StringComparison.OrdinalIgnoreCase));

        if (viTriPhien is null)
        {
            danhSach.Add(new TaiKhoanHeThong(phienDangNhap.TenDangNhap, phienDangNhap.HoTen, phienDangNhap.VaiTro, LayMoTaQuyen(phienDangNhap.VaiTro), "Phiên hiện tại", DateTime.Now));
        }
        else
        {
            danhSach[viTriPhien.viTri] = viTriPhien.taiKhoan with { TrangThai = "Phiên hiện tại", LanDangNhapGanNhat = DateTime.Now };
        }

        return danhSach;
    }

    private TaiLieuOffice TaoTaiLieuHopDongLamViec(UngVien ungVien)
    {
        var viTri = DuLieu.ViTri.FirstOrDefault(x => x.TenViTri == ungVien.ViTri);
        var phongBan = DuLieu.PhongBan.FirstOrDefault(x => x.MaPhongBan == viTri?.MaPhongBan);
        var luongDuKien = viTri?.LuongDuKien > 0 ? viTri.LuongDuKien : 12_000_000;
        var (nguoiLap, chucVuNguoiLap) = LayNguoiLapBaoCaoNhanSu();

        var soHopDong = $"HDLV-{DateTime.Today:yyyyMMdd}-{Math.Max(1, DuLieu.UngVien.IndexOf(ungVien) + 1):000}";
        return new TaiLieuOffice(
            "HỢP ĐỒNG LÀM VIỆC",
            [
                $"Số: {soHopDong}",
                $"Ngày lập: {DateTime.Today:dd/MM/yyyy}",
                "",
                "BÊN SỬ DỤNG LAO ĐỘNG",
                "Công ty: Công ty Quản Trị Nhân Sự",
                $"Đại diện lập biểu: {nguoiLap}",
                $"Chức vụ/Bộ phận: {chucVuNguoiLap}",
                "",
                "NGƯỜI LAO ĐỘNG",
                $"Họ tên: {ungVien.HoTen}",
                $"Email: {ungVien.Email}",
                $"Số điện thoại: {ungVien.DienThoai}",
                "",
                "ĐIỀU KHOẢN CHÍNH",
                "1. Người lao động thực hiện công việc theo phân công của phòng ban chuyên môn.",
                "2. Công ty đảm bảo chế độ lương, thưởng, nghỉ phép và đánh giá năng lực theo quy định nội bộ.",
                "3. Hai bên hoàn thiện hồ sơ nhân viên trước ngày bắt đầu làm việc.",
                "",
                "ĐẠI DIỆN CÔNG TY                         NGƯỜI LAO ĐỘNG",
                "",
                $"{nguoiLap}                           {ungVien.HoTen}"
            ],
            ["Thông tin", "Nội dung"],
            [
                ["Vị trí", ungVien.ViTri],
                ["Phòng ban", phongBan?.TenPhongBan ?? "Chưa phân công"],
                ["Ngày bắt đầu dự kiến", DateTime.Today.AddDays(7).ToString("dd/MM/yyyy")],
                ["Mức lương dự kiến", $"{luongDuKien:N0} đ/tháng"],
                ["Trạng thái tuyển dụng", ungVien.GiaiDoan]
            ],
            nguoiLap,
            chucVuNguoiLap);
    }

    private (string NguoiLap, string ChucVuNguoiLap) LayNguoiLapBaoCaoNhanSu()
    {
        var nhanVienNhanSu = DuLieu.NhanVien
            .Where(nhanVien => nhanVien.DangLamViec
                && string.Equals(nhanVien.PhongBan, "Phòng Nhân sự", StringComparison.OrdinalIgnoreCase))
            .OrderBy(nhanVien => nhanVien.ViTri.Contains("Nhân viên", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(nhanVien => nhanVien.MaSo)
            .FirstOrDefault();

        return nhanVienNhanSu is null
            ? ("Nhân viên Phòng Nhân sự", "Nhân viên Phòng Nhân sự")
            : (nhanVienNhanSu.HoTen, $"{nhanVienNhanSu.ViTri} - {nhanVienNhanSu.PhongBan}");
    }

    private TaiLieuOffice TaoTaiLieuBaoCaoNhanVien()
    {
        var (nguoiLap, chucVuNguoiLap) = LayNguoiLapBaoCaoNhanSu();
        var danhSach = DuLieu.NhanVien
            .Where(nhanVien => LocNhanVien(nhanVien))
            .OrderBy(nhanVien => nhanVien.ThuTuChucVu)
            .ThenBy(nhanVien => nhanVien.PhongBan)
            .ThenBy(nhanVien => nhanVien.HoTen)
            .ToList();
        var nghiPhepHomNay = LayTenNhanVienNghiDaDuyetNgay(DateTime.Today);

        var dongBang = danhSach
            .Select(nhanVien => (IReadOnlyList<string>)
            [
                nhanVien.MaSo,
                nhanVien.HoTen,
                nhanVien.Tuoi.ToString(),
                nhanVien.SoNamBaoHiemXaHoi.ToString(),
                nhanVien.PhongBan,
                nhanVien.ViTri,
                nhanVien.CapBacChucVu,
                nhanVien.NgaySinh.ToString("dd/MM/yyyy"),
                nhanVien.NgayThamGiaBaoHiemXaHoi.ToString("dd/MM/yyyy"),
                nhanVien.NgayVaoLam.ToString("dd/MM/yyyy"),
                nghiPhepHomNay.Contains(nhanVien.HoTen) ? "Nghỉ phép đã duyệt" : nhanVien.TrangThai,
                $"{LayLuongCoBan(nhanVien):N0} đ"
            ])
            .ToList();

        return new TaiLieuOffice(
            "BÁO CÁO HỒ SƠ NHÂN SỰ",
            [
                $"Ngày lập: {DateTime.Now:HH:mm dd/MM/yyyy}",
                $"Người lập: {nguoiLap} - {chucVuNguoiLap}",
                $"Phòng ban lọc: {PhongBanNhanVienDangChon}",
                $"Từ khóa lọc: {(string.IsNullOrWhiteSpace(TuKhoaTimKiem) ? "Không" : TuKhoaTimKiem.Trim())}",
                "",
                "TỔNG HỢP",
                $"Nhân viên theo bộ lọc: {danhSach.Count}",
                $"Hồ sơ còn hiệu lực: {danhSach.Count(nhanVien => nhanVien.DangLamViec)}",
                $"Quân số hiện diện hôm nay: {danhSach.Count(nhanVien => nhanVien.DangLamViec && !nghiPhepHomNay.Contains(nhanVien.HoTen))}",
                $"Nghỉ phép đã duyệt hôm nay: {danhSach.Count(nhanVien => nghiPhepHomNay.Contains(nhanVien.HoTen))}",
                $"Cá nhân xuất sắc: {CaNhanXuatSacHoSoHienThi}",
                $"Lương cao nhất: {LuongCaoHoSoHienThi}"
            ],
            ["Mã NV", "Họ tên", "Tuổi", "Năm BHXH", "Phòng ban", "Chức vụ", "Cấp bậc", "Ngày sinh", "Ngày BHXH", "Ngày vào", "Trạng thái", "Lương dự kiến"],
            dongBang,
            nguoiLap,
            chucVuNguoiLap);
    }

    private TaiLieuOffice TaoTaiLieuBaoCaoChamCong()
    {
        var (nguoiLap, chucVuNguoiLap) = LayNguoiLapBaoCaoNhanSu();
        var danhSach = DuLieu.ChamCong
            .Where(chamCong => LocChamCong(chamCong) && ThuocKyBaoCao(chamCong.GioVao))
            .OrderByDescending(chamCong => chamCong.GioVao)
            .ThenBy(chamCong => chamCong.NhanVien)
            .ToList();

        var dongBang = danhSach
            .Select(chamCong =>
            {
                var nhanVien = LayNhanVienTheoTen(chamCong.NhanVien);
                return (IReadOnlyList<string>)
                [
                    chamCong.NhanVien,
                    nhanVien?.PhongBan ?? "Chưa rõ",
                    nhanVien?.ViTri ?? "Chưa rõ",
                    chamCong.GioVao.ToString("dd/MM/yyyy"),
                    chamCong.GioVao.ToString("HH:mm"),
                    chamCong.GioRa?.ToString("HH:mm") ?? "Chưa ra ca",
                    $"{TinhSoGioChamCong(chamCong):N2}",
                    $"{chamCong.NgayCongQuyDoi:N2}",
                    chamCong.TrangThaiCong
                ];
            })
            .ToList();

        return new TaiLieuOffice(
            "BÁO CÁO CHẤM CÔNG",
            [
                $"Ngày lập: {DateTime.Now:HH:mm dd/MM/yyyy}",
                $"Người lập: {nguoiLap} - {chucVuNguoiLap}",
                $"Kỳ báo cáo: {KyBaoCaoNhanSuDangChon}",
                $"Phòng ban lọc: {PhongBanChamCongDangChon}",
                $"Từ khóa lọc: {(string.IsNullOrWhiteSpace(TuKhoaChamCong) ? "Không" : TuKhoaChamCong.Trim())}",
                "",
                "TỔNG HỢP",
                $"Quân số hiện diện theo kỳ: {QuanSoHienDienTrongKyBaoCao}/{NhanSuConHieuLuc}",
                $"Nhân sự nghỉ đã duyệt trong kỳ: {NhanSuNghiDaDuyetTrongKyBaoCao}",
                $"Lượt chấm công: {danhSach.Count}",
                $"Tổng giờ công: {danhSach.Sum(TinhSoGioChamCong):N2}",
                $"Ngày công quy đổi: {danhSach.Sum(chamCong => chamCong.NgayCongQuyDoi):N2}",
                $"Tỷ lệ đã ra ca: {TyLeHoanTatChamCongDaLoc}",
                $"Ca cần rà soát: {SoCaCanRaSoatDaLoc}",
                $"Nhân viên nhiều giờ nhất theo bộ lọc: {NhanVienCongCaoHienThi}"
            ],
            ["Nhân viên", "Phòng ban", "Chức vụ", "Ngày", "Giờ vào", "Giờ ra", "Số giờ", "Ngày công", "Trạng thái"],
            dongBang,
            nguoiLap,
            chucVuNguoiLap);
    }

    private TaiLieuOffice TaoTaiLieuBaoCaoNghiPhep()
    {
        var (nguoiLap, chucVuNguoiLap) = LayNguoiLapBaoCaoNhanSu();
        var danhSach = DuLieu.NghiPhep
            .Where(nghiPhep => LocNghiPhep(nghiPhep) && NghiPhepThuocKyBaoCao(nghiPhep))
            .OrderByDescending(nghiPhep => nghiPhep.TuNgay)
            .ThenBy(nghiPhep => nghiPhep.NhanVien)
            .ToList();

        var dongBang = danhSach
            .Select(nghiPhep =>
            {
                var nhanVien = LayNhanVienTheoTen(nghiPhep.NhanVien);
                return (IReadOnlyList<string>)
                [
                    nghiPhep.NhanVien,
                    nhanVien?.PhongBan ?? "Chưa rõ",
                    nhanVien?.ViTri ?? "Chưa rõ",
                    nghiPhep.LoaiNghi,
                    nghiPhep.TuNgay.ToString("dd/MM/yyyy"),
                    nghiPhep.DenNgay.ToString("dd/MM/yyyy"),
                    $"{nghiPhep.SoNgay:N2}",
                    nghiPhep.TrangThai
                ];
            })
            .ToList();

        return new TaiLieuOffice(
            "BÁO CÁO NGHỈ PHÉP",
            [
                $"Ngày lập: {DateTime.Now:HH:mm dd/MM/yyyy}",
                $"Người lập: {nguoiLap} - {chucVuNguoiLap}",
                $"Kỳ báo cáo: {KyBaoCaoNhanSuDangChon}",
                "",
                "TỔNG HỢP",
                $"Đơn nghỉ trong kỳ: {danhSach.Count}",
                $"Đơn chờ duyệt trong kỳ: {danhSach.Count(nghiPhep => nghiPhep.TrangThai.Contains("Chờ", StringComparison.OrdinalIgnoreCase))}",
                $"Nhân sự nghỉ đã duyệt trong kỳ: {NhanSuNghiDaDuyetTrongKyBaoCao}",
                $"Quân số hiện diện theo kỳ: {QuanSoHienDienTrongKyBaoCao}/{NhanSuConHieuLuc}",
                $"Số ngày đã duyệt trong kỳ: {danhSach.Where(nghiPhep => QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai)).Sum(nghiPhep => nghiPhep.SoNgay):N2}",
                $"Đơn nghỉ chờ duyệt toàn hệ thống: {NghiChoDuyet}"
            ],
            ["Nhân viên", "Phòng ban", "Chức vụ", "Loại nghỉ", "Từ ngày", "Đến ngày", "Số ngày", "Trạng thái"],
            dongBang,
            nguoiLap,
            chucVuNguoiLap);
    }

    private TaiLieuOffice TaoTaiLieuBaoCaoLuong()
    {
        var (nguoiLap, chucVuNguoiLap) = LayNguoiLapBaoCaoNhanSu();
        var danhSachLuong = DuLieu.PhieuLuong
            .Where(phieuLuong => LocPhieuLuong(phieuLuong) && ThuocKyBaoCaoLuong(phieuLuong.KyLuong))
            .OrderByDescending(phieuLuong => phieuLuong.KyLuong)
            .ThenBy(phieuLuong => phieuLuong.NhanVien)
            .ToList();
        var phieuLuongCaoNhat = danhSachLuong
            .OrderByDescending(phieuLuong => phieuLuong.ThucLanh)
            .FirstOrDefault();
        var dongBang = danhSachLuong
            .Select(p =>
            {
                var nhanVien = LayNhanVienTheoTen(p.NhanVien);
                return (IReadOnlyList<string>)
                [
                    p.NhanVien,
                    nhanVien?.PhongBan ?? "Chưa rõ",
                    nhanVien?.SoNamBaoHiemXaHoi.ToString() ?? "0",
                    p.KyLuong,
                    $"{p.LuongCoBan:N0} đ",
                    $"{p.PhuCap:N0} đ",
                    $"{p.KhauTru:N0} đ",
                    $"{QuyTacNghiepVuNhanSu.TinhKhauTruBaoHiem(p.LuongCoBan):N0} đ",
                    $"{p.ThucLanh:N0} đ",
                    p.TrangThai
                ];
            })
            .ToList();

        return new TaiLieuOffice(
            "BÁO CÁO LƯƠNG NHÂN SỰ",
            [
                $"Ngày lập: {DateTime.Now:HH:mm dd/MM/yyyy}",
                $"Người lập: {nguoiLap} - {chucVuNguoiLap}",
                $"Kỳ báo cáo: {KyBaoCaoNhanSuDangChon}",
                $"Phòng ban lọc: {PhongBanBangLuongDangChon}",
                $"Từ khóa lọc: {(string.IsNullOrWhiteSpace(TuKhoaBangLuong) ? "Không" : TuKhoaBangLuong.Trim())}",
                "",
                "TỔNG HỢP",
                $"Phiếu theo bộ lọc và kỳ báo cáo: {dongBang.Count}",
                $"Tổng thực lãnh theo bộ lọc và kỳ báo cáo: {danhSachLuong.Sum(phieuLuong => phieuLuong.ThucLanh):N0} đ",
                $"Lương cao nhất trong báo cáo: {TaoMoTaLuongCao(phieuLuongCaoNhat)}",
                $"Phiếu lương chờ chi trả toàn hệ thống: {SoPhieuLuongChoTra}",
                $"Tổng lương chờ chi trả toàn hệ thống: {TongLuongChoTra:N0} đ"
            ],
            ["Nhân viên", "Phòng ban", "Năm BHXH", "Kỳ lương", "Lương cơ bản", "Phụ cấp", "Khấu trừ", "BHXH NLĐ", "Thực lãnh", "Trạng thái"],
            dongBang,
            nguoiLap,
            chucVuNguoiLap);
    }

    private static bool XuatTaiLieu(TaiLieuOffice taiLieu, string tenFileMacDinh, string tieuDe)
    {
        var hopThoai = new SaveFileDialog
        {
            Title = $"Xuất {tieuDe}",
            FileName = tenFileMacDinh,
            Filter = "Word (*.docx)|*.docx|Excel (*.xlsx)|*.xlsx|PDF (*.pdf)|*.pdf|PowerPoint (*.pptx)|*.pptx|Văn bản (*.txt)|*.txt",
            DefaultExt = Path.GetExtension(tenFileMacDinh),
            FilterIndex = Path.GetExtension(tenFileMacDinh).ToLowerInvariant() switch
            {
                ".xlsx" => 2,
                ".pdf" => 3,
                ".pptx" => 4,
                ".txt" => 5,
                _ => 1
            },
            AddExtension = true
        };

        if (hopThoai.ShowDialog() != true)
        {
            return false;
        }

        BoXuatOffice.Xuat(hopThoai.FileName, taiLieu);
        MessageBox.Show($"Đã xuất {tieuDe}:\n{hopThoai.FileName}", "Xuất file", MessageBoxButton.OK, MessageBoxImage.Information);
        return true;
    }

    private static string LamSachTenFile(string giaTri)
    {
        var kyTuKhongHopLe = Path.GetInvalidFileNameChars();
        var ketQua = new string(giaTri.Select(kyTu => kyTuKhongHopLe.Contains(kyTu) ? '_' : kyTu).ToArray());
        return string.IsNullOrWhiteSpace(ketQua) ? "UngVien" : ketQua.Replace(' ', '_');
    }

    private void CapNhatHienThiMuc()
    {
        BaoThayDoi(nameof(TieuDeManHinh));
        NeuDangSuaNhanVienThiNapBieuMau();
        NeuDangSuaPhongBanThiNapBieuMau();
        if (DangMoBieuMauThongBao && string.IsNullOrWhiteSpace(TieuDeThongBaoMoi))
        {
            PhanHeThongBaoMoi = MucDangChon == "Tổng quan" ? "Hệ thống" : MucDangChon;
        }
    }
}
