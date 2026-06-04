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

    [TestMethod]
    public void HoSoNhanVienLocDuocNhanVienDaNghiViec()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var dangLam = new NhanVien { MaNhanVien = 1, MaSo = "NV001", HoTen = "Nguyễn Đang Làm", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự", DangLamViec = true };
        var daNghi = new NhanVien { MaNhanVien = 2, MaSo = "NV002", HoTen = "Trần Đã Nghỉ", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự", DangLamViec = false };

        viewModel.DuLieu.NhanVien.Add(dangLam);
        viewModel.DuLieu.NhanVien.Add(daNghi);
        typeof(ManHinhChinhViewModel)
            .GetMethod("CauHinhDanhSachNhanVienView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(viewModel, []);
        viewModel.TrangThaiNhanVienDangChon = "Nghỉ việc";

        Assert.AreEqual(1, viewModel.SoNhanVienHoSo);
        Assert.AreEqual("Trần Đã Nghỉ", viewModel.DanhSachNhanVienView.Cast<NhanVien>().Single().HoTen);
    }

    [TestMethod]
    public void BangLuongKhongHienThiVaKhongChonPhieuAmLamLuongCaoNhat()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        viewModel.DuLieu.NhanVien.Add(new NhanVien { HoTen = "Người Lương Âm", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự", DangLamViec = true });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { HoTen = "Người Lương Dương", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự", DangLamViec = true });
        viewModel.DuLieu.PhieuLuong.Add(new PhieuLuong("Người Lương Âm", "2026-06", 10_000_000, 0, 99_000_000, -89_000_000, "Nháp"));
        viewModel.DuLieu.PhieuLuong.Add(new PhieuLuong("Người Lương Dương", "2026-06", 12_000_000, 1_000_000, 500_000, 12_500_000, "Đã trả"));
        viewModel.DuLieu.PhieuLuong.Add(new PhieuLuong("Người Lương Dương", "2026-05", 12_000_000, 1_000_000, 500_000, 11_500_000, "Đã trả"));

        Assert.AreEqual(1, viewModel.SoPhieuLuongDaLoc);
        Assert.AreEqual("2026-06", viewModel.BangLuongNhanVienHienThi.Single().KyLuong);
        Assert.AreEqual("Người Lương Dương: 12,500,000 đ", viewModel.LuongCaoBangLuongHienThi);
        Assert.AreEqual("Người Lương Dương: 12,500,000 đ", viewModel.LuongCaoHoSoHienThi);
    }
}
