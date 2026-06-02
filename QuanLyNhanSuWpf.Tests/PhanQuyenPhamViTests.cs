namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class PhanQuyenPhamViTests
{
    [TestMethod]
    public async Task NhanVienChiXemDuLieuCaNhanVaKhongQuanTri()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("nv001", "Vũ Hải An", "Nhân viên"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenBangLuong);
        Assert.IsFalse(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsFalse(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsTrue(viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.HoTen == "Vũ Hải An"));
        Assert.IsTrue(viewModel.DanhSachPhieuLuongView.Cast<PhieuLuong>().All(phieuLuong => phieuLuong.NhanVien == "Vũ Hải An"));
    }

    [TestMethod]
    public async Task TruongPhongNhanSuChiXemPhongNhanSuVaDuocXuLyBangLuong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("tp003", "Lê Thu Hà", "Trưởng phòng"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsTrue(
            viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.PhongBan == "Phòng Nhân sự"),
            string.Join(" | ", viewModel.NhanVienTrongPhamVi.Select(nhanVien => $"{nhanVien.HoTen}: {nhanVien.PhongBan}")));
    }

    [TestMethod]
    public async Task GiamDocXemToanHeThong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsGreaterThan(1, viewModel.NhanVienTrongPhamVi.Select(nhanVien => nhanVien.PhongBan).Distinct().Count());
    }
}
