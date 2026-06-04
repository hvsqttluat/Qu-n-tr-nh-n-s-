namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class PhongBanPhanQuyenTests
{
    [TestMethod]
    public void GiamDocDuocSuaTenVaGanTruongPhong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(1, "Phòng Nhân sự", "Chưa phân công");
        var truongPhong = new NhanVien
        {
            MaNhanVien = 10,
            HoTen = "Lê Thu Hà",
            MaPhongBan = 1,
            PhongBan = "Phòng Nhân sự"
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.NhanVien.Add(truongPhong);
        viewModel.PhongBanDangChon = phongBan;
        viewModel.BieuMauPhongBan = new BieuMauPhongBan
        {
            MaPhongBan = phongBan.MaPhongBan,
            TenPhongBan = "Phòng Nhân sự và Hành chính",
            MaTruongPhong = truongPhong.MaNhanVien
        };

        Assert.IsTrue(viewModel.CoQuyenPhongBan);
        Assert.IsTrue(viewModel.LuuPhongBanLenh.CanExecute(null));
        Assert.IsTrue(viewModel.GanTruongPhongLenh.CanExecute(null));
    }

    [TestMethod]
    public void NhanVienKhongDuocSuaPhongBan()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("nv001", "Vũ Hải An", "Nhân viên"));
        var phongBan = new PhongBan(1, "Phòng Nhân sự", "Chưa phân công");
        var truongPhong = new NhanVien
        {
            MaNhanVien = 10,
            HoTen = "Lê Thu Hà",
            MaPhongBan = 1,
            PhongBan = "Phòng Nhân sự"
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.NhanVien.Add(truongPhong);
        viewModel.PhongBanDangChon = phongBan;
        viewModel.BieuMauPhongBan = new BieuMauPhongBan
        {
            MaPhongBan = phongBan.MaPhongBan,
            TenPhongBan = "Phòng Nhân sự và Hành chính",
            MaTruongPhong = truongPhong.MaNhanVien
        };

        Assert.IsFalse(viewModel.CoQuyenPhongBan);
        Assert.IsFalse(viewModel.LuuPhongBanLenh.CanExecute(null));
        Assert.IsFalse(viewModel.GanTruongPhongLenh.CanExecute(null));
    }

    [TestMethod]
    public void DoiBieuMauPhongBanLamMoiTrangThaiLenh()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var daLamMoi = false;
        viewModel.LuuPhongBanLenh.CanExecuteChanged += (_, _) => daLamMoi = true;

        viewModel.BieuMauPhongBan.TenPhongBan = "Phòng Kiểm thử";

        Assert.IsTrue(daLamMoi);
        Assert.IsTrue(viewModel.LuuPhongBanLenh.CanExecute(null));
    }

    [TestMethod]
    public void ThayTruongPhongThiNguoiCuBiHaChuc()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(10, "Phòng Pháp chế", "Vũ Anh Tuấn");
        var truongPhongCu = new NhanVien
        {
            MaNhanVien = 20,
            HoTen = "Vũ Anh Tuấn",
            MaPhongBan = 10,
            PhongBan = "Phòng Pháp chế",
            MaViTri = 30,
            ViTri = "Trưởng phòng Pháp chế"
        };
        var truongPhongMoi = new NhanVien
        {
            MaNhanVien = 21,
            HoTen = "Trần Văn Luật",
            MaPhongBan = 2,
            PhongBan = "Phòng Nhân sự",
            MaViTri = 31,
            ViTri = "Nhân viên nhân sự"
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(30, 10, "Trưởng phòng Pháp chế", 29_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(32, 10, "Chuyên viên pháp chế", 15_000_000, "Đang tuyển"));
        viewModel.DuLieu.NhanVien.Add(truongPhongCu);
        viewModel.DuLieu.NhanVien.Add(truongPhongMoi);

        var method = typeof(ManHinhChinhViewModel).GetMethod(
            "CapNhatChucVuTruongPhongCucBo",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        method.Invoke(viewModel, [phongBan, truongPhongMoi]);

        Assert.AreEqual("Trưởng phòng Pháp chế", truongPhongMoi.ViTri);
        Assert.AreEqual("Phòng Pháp chế", truongPhongMoi.PhongBan);
        Assert.AreEqual("Chuyên viên pháp chế", truongPhongCu.ViTri);
        Assert.AreEqual("Phòng Pháp chế", truongPhongCu.PhongBan);
    }

    [TestMethod]
    public void DoiPhongBanNhanVienThiChiChonViTriCuaPhongMoi()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongNhanSu = new PhongBan(1, "Phòng Nhân sự", "Lê Thu Hà");
        var phongPhapChe = new PhongBan(2, "Phòng Pháp chế", "Chưa phân công");

        viewModel.DuLieu.PhongBan.Add(phongNhanSu);
        viewModel.DuLieu.PhongBan.Add(phongPhapChe);
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(11, 1, "Trưởng phòng Nhân sự", 20_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(12, 1, "Nhân viên nhân sự", 12_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(21, 2, "Chuyên viên pháp chế", 15_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(22, 2, "Trưởng phòng Pháp chế", 24_000_000, "Đang tuyển"));

        viewModel.BieuMauNhanVien = new NhanVien
        {
            MaNhanVien = 100,
            MaSo = "NV100",
            HoTen = "Trần Văn A",
            MaPhongBan = phongNhanSu.MaPhongBan,
            MaViTri = 12
        };

        viewModel.BieuMauNhanVien.MaPhongBan = phongPhapChe.MaPhongBan;

        CollectionAssert.AreEqual(
            new[] { 21, 22 },
            viewModel.ViTriTheoPhongBanBieuMauNhanVien.Select(v => v.MaViTri).ToArray());
        Assert.AreEqual(21, viewModel.BieuMauNhanVien.MaViTri);
        Assert.AreEqual("Chuyên viên pháp chế", viewModel.BieuMauNhanVien.ViTri);
    }

    [TestMethod]
    public async Task LuuPhongBanBoNhiemTruongPhongVaHaNguoiCu()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(10, "Phòng Pháp chế", "Vũ Anh Tuấn");
        var truongPhongCu = new NhanVien
        {
            MaNhanVien = 20,
            MaSo = "TP020",
            HoTen = "Vũ Anh Tuấn",
            MaPhongBan = 10,
            PhongBan = "Phòng Pháp chế",
            MaViTri = 30,
            ViTri = "Trưởng phòng Pháp chế",
            DangLamViec = true
        };
        var truongPhongMoi = new NhanVien
        {
            MaNhanVien = 21,
            MaSo = "NV021",
            HoTen = "Trần Văn Luật",
            MaPhongBan = 2,
            PhongBan = "Phòng Nhân sự",
            MaViTri = 31,
            ViTri = "Nhân viên nhân sự",
            DangLamViec = true
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(30, 10, "Trưởng phòng Pháp chế", 29_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(32, 10, "Chuyên viên pháp chế", 15_000_000, "Đang tuyển"));
        viewModel.DuLieu.NhanVien.Add(truongPhongCu);
        viewModel.DuLieu.NhanVien.Add(truongPhongMoi);
        viewModel.PhongBanDangChon = phongBan;
        viewModel.BieuMauPhongBan = new BieuMauPhongBan
        {
            MaPhongBan = phongBan.MaPhongBan,
            TenPhongBan = phongBan.TenPhongBan,
            MaTruongPhong = truongPhongMoi.MaNhanVien
        };

        await GoiRiengAsync(viewModel, "LuuPhongBan");

        var phongBanSauLuu = viewModel.DuLieu.PhongBan.Single(x => x.MaPhongBan == phongBan.MaPhongBan);
        Assert.AreEqual(truongPhongMoi.HoTen, phongBanSauLuu.TruongPhong);
        Assert.AreEqual("Trưởng phòng Pháp chế", truongPhongMoi.ViTri);
        Assert.AreEqual("Phòng Pháp chế", truongPhongMoi.PhongBan);
        Assert.AreEqual("Chuyên viên pháp chế", truongPhongCu.ViTri);
    }

    [TestMethod]
    public async Task NhanVienNghiViecDuocLuuHoSoVaBoKhoiTruongPhong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(10, "Phòng Pháp chế", "Vũ Anh Tuấn");
        var truongPhong = new NhanVien
        {
            MaNhanVien = 20,
            MaSo = "TP020",
            HoTen = "Vũ Anh Tuấn",
            MaPhongBan = 10,
            PhongBan = "Phòng Pháp chế",
            MaViTri = 30,
            ViTri = "Trưởng phòng Pháp chế",
            DangLamViec = true
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(30, 10, "Trưởng phòng Pháp chế", 29_000_000, "Đang tuyển"));
        viewModel.DuLieu.ViTri.Add(new ViTriCongViec(32, 10, "Chuyên viên pháp chế", 15_000_000, "Đang tuyển"));
        viewModel.DuLieu.NhanVien.Add(truongPhong);
        viewModel.DuLieu.ChamCong.Add(new ChamCong(truongPhong.HoTen, DateTime.Today.AddHours(8), DateTime.Today.AddHours(17), 8));
        viewModel.NhanVienDangChon = truongPhong;
        viewModel.BieuMauNhanVien = truongPhong.TaoBanSao();
        viewModel.BieuMauNhanVien.DangLamViec = false;

        await GoiRiengAsync(viewModel, "LuuNhanVien");

        Assert.IsFalse(truongPhong.DangLamViec);
        Assert.AreEqual("Nghỉ việc", truongPhong.TrangThai);
        Assert.AreEqual("Chưa phân công", viewModel.DuLieu.PhongBan.Single(x => x.MaPhongBan == phongBan.MaPhongBan).TruongPhong);
        Assert.HasCount(1, viewModel.DuLieu.NhanVien);
        Assert.HasCount(1, viewModel.DuLieu.ChamCong);
    }

    [TestMethod]
    public async Task TongHopPhongBanTinhCaNhanVienDaNghiViec()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(90, "Phòng Kiểm thử", "Chưa phân công");
        var viTri = new ViTriCongViec(90, 90, "Nhân viên kiểm thử", 14_000_000, "Đang tuyển");
        var nhanVienDangLam = new NhanVien
        {
            MaNhanVien = 90,
            MaSo = "NV090",
            HoTen = "Nguyễn Văn Test",
            MaPhongBan = 90,
            PhongBan = phongBan.TenPhongBan,
            MaViTri = viTri.MaViTri,
            ViTri = viTri.TenViTri,
            DangLamViec = true
        };
        var nhanVienDaNghi = new NhanVien
        {
            MaNhanVien = 91,
            MaSo = "NV091",
            HoTen = "Trần Thị Nghỉ",
            MaPhongBan = 90,
            PhongBan = phongBan.TenPhongBan,
            MaViTri = viTri.MaViTri,
            ViTri = viTri.TenViTri,
            DangLamViec = false
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.ViTri.Add(viTri);
        viewModel.DuLieu.NhanVien.Add(nhanVienDangLam);
        viewModel.DuLieu.NhanVien.Add(nhanVienDaNghi);

        await GoiRiengAsync(viewModel, "CapNhatTraCuuDieuHanh");

        var tongHop = viewModel.TongHopPhongBanDieuHanh.Single(x => x.MaPhongBan == phongBan.MaPhongBan);
        Assert.AreEqual(2, tongHop.SoNhanVien);
    }

    [TestMethod]
    public void KhongBoNhiemNhanVienDaNghiLamTruongPhong()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(10, "Phòng Pháp chế", "Chưa phân công");
        var nhanVienDaNghi = new NhanVien
        {
            MaNhanVien = 20,
            MaSo = "NV020",
            HoTen = "Vũ Anh Tuấn",
            MaPhongBan = 10,
            PhongBan = "Phòng Pháp chế",
            MaViTri = 32,
            ViTri = "Chuyên viên pháp chế",
            DangLamViec = false
        };

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.NhanVien.Add(nhanVienDaNghi);
        viewModel.PhongBanDangChon = phongBan;
        viewModel.BieuMauPhongBan = new BieuMauPhongBan
        {
            MaPhongBan = phongBan.MaPhongBan,
            TenPhongBan = phongBan.TenPhongBan,
            MaTruongPhong = nhanVienDaNghi.MaNhanVien
        };

        Assert.IsFalse(viewModel.GanTruongPhongLenh.CanExecute(null));
        Assert.IsFalse(viewModel.NhanVienCoTheBoNhiemTruongPhong.Any(x => x.MaNhanVien == nhanVienDaNghi.MaNhanVien));
    }

    [TestMethod]
    public async Task LuuUngVienCapNhatDungDongDangSua()
    {
        var viewModel = new ManHinhChinhViewModel(new PhienDangNhap("gd001", "Nguyễn Minh Đức", "Giám đốc"));
        var phongBan = new PhongBan(1, "Phòng Nhân sự", "Chưa phân công");
        var viTriNhanSu = new ViTriCongViec(11, 1, "Nhân viên nhân sự", 12_000_000, "Đang tuyển");
        var viTriPhapChe = new ViTriCongViec(21, 2, "Chuyên viên pháp chế", 15_000_000, "Đang tuyển");
        var ungVien = new UngVien("Nguyễn Văn A", "Nhân viên nhân sự", "a@example.com", "0901", "Mới", 7);

        viewModel.DuLieu.PhongBan.Add(phongBan);
        viewModel.DuLieu.PhongBan.Add(new PhongBan(2, "Phòng Pháp chế", "Chưa phân công"));
        viewModel.DuLieu.ViTri.Add(viTriNhanSu);
        viewModel.DuLieu.ViTri.Add(viTriPhapChe);
        viewModel.DuLieu.UngVien.Add(ungVien);
        viewModel.BieuMauUngVien = new BieuMauUngVien
        {
            MaUngVien = ungVien.MaUngVien,
            HoTen = "Nguyễn Văn A",
            Email = "a.new@example.com",
            DienThoai = "0902",
            MaViTri = viTriPhapChe.MaViTri,
            GiaiDoan = "Phỏng vấn"
        };

        await GoiRiengAsync(viewModel, "LuuUngVien");

        Assert.HasCount(1, viewModel.DuLieu.UngVien);
        var ungVienSauLuu = viewModel.DuLieu.UngVien.Single();
        Assert.AreEqual("a.new@example.com", ungVienSauLuu.Email);
        Assert.AreEqual("0902", ungVienSauLuu.DienThoai);
        Assert.AreEqual("Chuyên viên pháp chế", ungVienSauLuu.ViTri);
        Assert.AreEqual("Phỏng vấn", ungVienSauLuu.GiaiDoan);
        Assert.AreEqual(7, ungVienSauLuu.MaUngVien);
    }

    private static async Task GoiRiengAsync(ManHinhChinhViewModel viewModel, string tenPhuongThuc)
    {
        var method = typeof(ManHinhChinhViewModel).GetMethod(
            tenPhuongThuc,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var ketQua = method.Invoke(viewModel, []);
        if (ketQua is Task task)
        {
            await task;
        }
    }
}
