namespace QuanLyNhanSuWpf;

public class BanSaoDuLieuNhanSu
{
    public DateTime TaoLuc { get; set; } = DateTime.Now;
    public string PhienBanUngDung { get; set; } = "1.0";
    public List<NhanVien> NhanVien { get; set; } = [];
    public List<PhongBan> PhongBan { get; set; } = [];
    public List<ViTriCongViec> ViTri { get; set; } = [];
    public List<NghiPhep> NghiPhep { get; set; } = [];
    public List<ChamCong> ChamCong { get; set; } = [];
    public List<DanhGia> DanhGia { get; set; } = [];
    public List<PhieuLuong> PhieuLuong { get; set; } = [];
    public List<UngVien> UngVien { get; set; } = [];

    public static BanSaoDuLieuNhanSu TaoTu(KhoDuLieuUngDung duLieu) => new()
    {
        TaoLuc = DateTime.Now,
        NhanVien = duLieu.NhanVien.Select(x => x.TaoBanSao()).ToList(),
        PhongBan = duLieu.PhongBan.ToList(),
        ViTri = duLieu.ViTri.ToList(),
        NghiPhep = duLieu.NghiPhep.ToList(),
        ChamCong = duLieu.ChamCong.ToList(),
        DanhGia = duLieu.DanhGia.ToList(),
        PhieuLuong = duLieu.PhieuLuong.ToList(),
        UngVien = duLieu.UngVien.ToList()
    };

    public KhoDuLieuUngDung TaoKhoDuLieu()
    {
        var duLieu = new KhoDuLieuUngDung();
        foreach (var dong in NhanVien) duLieu.NhanVien.Add(dong);
        foreach (var dong in PhongBan) duLieu.PhongBan.Add(dong);
        foreach (var dong in ViTri) duLieu.ViTri.Add(dong);
        foreach (var dong in NghiPhep) duLieu.NghiPhep.Add(dong);
        foreach (var dong in ChamCong) duLieu.ChamCong.Add(dong);
        foreach (var dong in DanhGia) duLieu.DanhGia.Add(dong);
        foreach (var dong in PhieuLuong) duLieu.PhieuLuong.Add(dong);
        foreach (var dong in UngVien) duLieu.UngVien.Add(dong);
        return duLieu;
    }
}
