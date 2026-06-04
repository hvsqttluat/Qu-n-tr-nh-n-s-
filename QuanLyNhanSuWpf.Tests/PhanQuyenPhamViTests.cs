namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class PhanQuyenPhamViTests
{
    [TestMethod]
    public void AdminCoToanBoChucNangVaPhamViDuLieu()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("admin", "Quản trị hệ thống", "Admin"));
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 1, MaSo = "NV001", HoTen = "Nhân viên A", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự" });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 2, MaSo = "NV002", HoTen = "Nhân viên B", PhongBan = "Phòng Kinh doanh", ViTri = "Nhân viên kinh doanh" });

        Assert.IsTrue(viewModel.CoQuyenTongQuan);
        Assert.IsTrue(viewModel.CoQuyenTuyenDung);
        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenPhongBan);
        Assert.IsTrue(viewModel.CoQuyenDieuChinhCong);
        Assert.IsTrue(viewModel.CoQuyenDuyetNghiPhep);
        Assert.IsTrue(viewModel.CoQuyenGhiNhanDanhGia);
        Assert.IsTrue(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsTrue(viewModel.CoQuyenBaoCaoNhanSu);
        Assert.IsTrue(viewModel.CoQuyenCaiDatTaiKhoan);
        Assert.HasCount(2, viewModel.NhanVienTrongPhamVi);
    }

    [TestMethod]
    public async Task NhanVienChiXemDuLieuCaNhanVaKhongQuanTri()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("nv001", "Vũ Hải An", "Nhân viên"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenThongBao);
        Assert.IsTrue(viewModel.CoQuyenBangLuong);
        Assert.IsFalse(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsFalse(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsFalse(viewModel.CoQuyenTongQuan);
        Assert.IsTrue(viewModel.CoQuyenChamCong);
        CollectionAssert.AreEquivalent(
            new[] { "Nhân viên", "Thông báo", "Chấm công", "Nghỉ phép", "Đánh giá", "Bảng lương" },
            viewModel.CacMucDieuHuong.ToArray());
        Assert.AreEqual("Nhân viên", viewModel.MucDangChon);
        Assert.HasCount(0, viewModel.CacTaiKhoanHeThong);
        Assert.IsFalse(viewModel.SaoLuuDuLieuLenh.CanExecute(null));
        viewModel.MucDangChon = "Cài đặt tài khoản";
        Assert.AreEqual("Nhân viên", viewModel.MucDangChon);
        Assert.IsTrue(viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.HoTen == "Vũ Hải An"));
        Assert.IsTrue(viewModel.DanhSachChamCongView.Cast<ChamCong>().All(chamCong => chamCong.NhanVien == "Vũ Hải An"));
        Assert.IsTrue(viewModel.DanhSachPhieuLuongView.Cast<PhieuLuong>().All(phieuLuong => phieuLuong.NhanVien == "Vũ Hải An"));
    }

    [TestMethod]
    public async Task TruongPhongNhanSuQuanTriNghiepVuToanHeThong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("tp003", "Lê Thu Hà", "Trưởng phòng"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsTrue(viewModel.CoQuyenPhongBan);
        Assert.IsGreaterThan(1, viewModel.NhanVienTrongPhamVi.Select(nhanVien => nhanVien.PhongBan).Distinct().Count());
    }

    [TestMethod]
    public async Task TruongPhongThuongQuanLyTrongPhongPhuTrach()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("tp001", "Trần Quốc Huy", "Trưởng phòng"));

        await viewModel.TaiDuLieu();

        Assert.IsTrue(viewModel.CoQuyenTongQuan);
        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenDuyetNghiPhep);
        Assert.IsFalse(viewModel.CoQuyenPhongBan);
        Assert.IsFalse(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsTrue(viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.PhongBan == "Phòng Kinh doanh"));
    }

    [TestMethod]
    public void ChuyenVienXemTheoPhongBanNhungKhongDuocQuanTri()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("cv001", "Chuyên viên A", "Chuyên viên"));
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 1, MaSo = "CV001", HoTen = "Chuyên viên A", PhongBan = "Phòng Pháp chế", ViTri = "Chuyên viên pháp chế" });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 2, MaSo = "NV002", HoTen = "Nhân viên cùng phòng", PhongBan = "Phòng Pháp chế", ViTri = "Nhân viên pháp chế" });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 3, MaSo = "NV003", HoTen = "Nhân viên phòng khác", PhongBan = "Phòng Kinh doanh", ViTri = "Nhân viên kinh doanh" });

        Assert.IsTrue(viewModel.CoQuyenTongQuan);
        Assert.IsTrue(viewModel.CoQuyenChamCong);
        Assert.IsTrue(viewModel.CoQuyenBaoCaoNhanSu);
        Assert.IsFalse(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsFalse(viewModel.CoQuyenDuyetNghiPhep);
        Assert.IsFalse(viewModel.CoQuyenTuyenDung);
        Assert.IsTrue(viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.PhongBan == "Phòng Pháp chế"));
    }

    [TestMethod]
    public void CongNhanDuocXemThongBaoVaChamCongCaNhan()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("cn001", "Công nhân A", "Công nhân"));
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 1, MaSo = "CN001", HoTen = "Công nhân A", PhongBan = "Phòng Sản xuất", ViTri = "Công nhân sản xuất" });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 2, MaSo = "CN002", HoTen = "Công nhân B", PhongBan = "Phòng Sản xuất", ViTri = "Công nhân sản xuất" });

        Assert.IsTrue(viewModel.CoQuyenThongBao);
        Assert.IsTrue(viewModel.CoQuyenChamCong);
        Assert.IsFalse(viewModel.CoQuyenTongQuan);
        Assert.IsFalse(viewModel.CoQuyenDieuChinhCong);
        CollectionAssert.Contains(viewModel.CacMucDieuHuong, "Thông báo");
        Assert.IsTrue(viewModel.NhanVienTrongPhamVi.All(nhanVien => nhanVien.HoTen == "Công nhân A"));
    }

    [TestMethod]
    public void NhanSuNghiepVuDuocQuanLyDuLieuToanHeThongNhungKhongCaiDatTaiKhoan()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("ns001", "Nhân sự A", "Nhân sự"));
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 1, MaSo = "NS001", HoTen = "Nhân sự A", PhongBan = "Phòng Nhân sự", ViTri = "Nhân viên nhân sự" });
        viewModel.DuLieu.NhanVien.Add(new NhanVien { MaNhanVien = 2, MaSo = "NV002", HoTen = "Nhân viên phòng khác", PhongBan = "Phòng Kinh doanh", ViTri = "Nhân viên kinh doanh" });

        Assert.IsTrue(viewModel.CoQuyenTongQuan);
        Assert.IsTrue(viewModel.CoQuyenTuyenDung);
        Assert.IsTrue(viewModel.CoQuyenQuanLyHoSoNhanVien);
        Assert.IsTrue(viewModel.CoQuyenXuLyBangLuong);
        Assert.IsFalse(viewModel.CoQuyenPhongBan);
        Assert.IsFalse(viewModel.CoQuyenCaiDatTaiKhoan);
        Assert.HasCount(2, viewModel.NhanVienTrongPhamVi);
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
