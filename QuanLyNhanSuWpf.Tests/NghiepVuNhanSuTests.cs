namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class NghiepVuNhanSuTests
{
    [TestMethod]
    public void TaoKyDanhGia_TinhDungQuyTheoThang()
    {
        Assert.AreEqual("2026-Q1", QuyTacNghiepVuNhanSu.TaoKyDanhGia(new DateTime(2026, 1, 15)));
        Assert.AreEqual("2026-Q2", QuyTacNghiepVuNhanSu.TaoKyDanhGia(new DateTime(2026, 5, 30)));
        Assert.AreEqual("2026-Q4", QuyTacNghiepVuNhanSu.TaoKyDanhGia(new DateTime(2026, 12, 31)));
    }

    [TestMethod]
    public void TinhSoNgayTrongThang_ChiTinhPhanGiaoVoiThangLuong()
    {
        Assert.AreEqual(2m, QuyTacNghiepVuNhanSu.TinhSoNgayTrongThang(
            new DateTime(2026, 4, 29),
            new DateTime(2026, 5, 2),
            2026,
            5));

        Assert.AreEqual(31m, QuyTacNghiepVuNhanSu.TinhSoNgayTrongThang(
            new DateTime(2026, 4, 30),
            new DateTime(2026, 6, 2),
            2026,
            5));

        Assert.AreEqual(0m, QuyTacNghiepVuNhanSu.TinhSoNgayTrongThang(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 3),
            2026,
            5));
    }

    [TestMethod]
    public void TaoPhieuLuongThang_TinhTheoCongVaNgayNghiDaDuyet()
    {
        var nhanVien = new NhanVien
        {
            HoTen = "Nguyễn Minh Đức",
            NgayThamGiaBaoHiemXaHoi = new DateTime(2021, 1, 1)
        };

        var phieuLuong = QuyTacNghiepVuNhanSu.TaoPhieuLuongThang(
            nhanVien,
            22_000_000m,
            176m,
            2m,
            new DateTime(2026, 5, 30));

        Assert.AreEqual("2026-05", phieuLuong.KyLuong);
        Assert.AreEqual(2_200_000m, phieuLuong.PhuCap);
        Assert.AreEqual(4_310_000m, phieuLuong.KhauTru);
        Assert.AreEqual(19_890_000m, phieuLuong.ThucLanh);
        Assert.AreEqual("Nháp", phieuLuong.TrangThai);
    }

    [TestMethod]
    public void TinhSoNamTron_DungTheoNgayThangNam()
    {
        Assert.AreEqual(34, QuyTacNghiepVuNhanSu.TinhSoNamTron(
            new DateTime(1992, 6, 1),
            new DateTime(2026, 6, 1)));

        Assert.AreEqual(32, QuyTacNghiepVuNhanSu.TinhSoNamTron(
            new DateTime(1993, 12, 15),
            new DateTime(2026, 6, 1)));
    }

    [TestMethod]
    public void TaoMaNhanVienTiepTheo_BoQuaMaKhongCungTienTo()
    {
        var maMoi = QuyTacNghiepVuNhanSu.TaoMaNhanVienTiepTheo(["GD001", "NV001", "NV010", "CN200"]);

        Assert.AreEqual("NV011", maMoi);
    }

    [TestMethod]
    public void NhanVien_TrangThaiLamViec_DongBoVoiDangLamViec()
    {
        var nhanVien = new NhanVien { DangLamViec = true };

        nhanVien.TrangThaiLamViec = "Nghỉ việc";

        Assert.IsFalse(nhanVien.DangLamViec);
        Assert.AreEqual("Nghỉ việc", nhanVien.TrangThai);

        nhanVien.TrangThaiLamViec = "Đang làm";

        Assert.IsTrue(nhanVien.DangLamViec);
        Assert.AreEqual("Đang làm", nhanVien.TrangThaiLamViec);
    }

    [TestMethod]
    public void NghiPhepGiaoNgay_ChiTinhDonDaGiaoNgayCanXem()
    {
        var nghiPhep = new NghiPhep("Vũ Hải An", "Nghỉ phép năm", new DateTime(2026, 6, 1), new DateTime(2026, 6, 3), 3, "Đã duyệt");

        Assert.IsTrue(QuyTacNghiepVuNhanSu.LaTrangThaiNghiPhepDaDuyet(nghiPhep.TrangThai));
        Assert.IsTrue(QuyTacNghiepVuNhanSu.NghiPhepGiaoNgay(nghiPhep, new DateTime(2026, 6, 2)));
        Assert.IsFalse(QuyTacNghiepVuNhanSu.NghiPhepGiaoNgay(nghiPhep, new DateTime(2026, 6, 4)));
    }

    [TestMethod]
    public void NghiPhepGiaoKhoang_TinhDonBacQuaKyBaoCao()
    {
        var nghiPhep = new NghiPhep("Vũ Hải An", "Nghỉ phép năm", new DateTime(2026, 5, 30), new DateTime(2026, 6, 2), 4, "Đã duyệt");

        Assert.IsTrue(QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)));
        Assert.IsFalse(QuyTacNghiepVuNhanSu.NghiPhepGiaoKhoang(nghiPhep, new DateTime(2026, 6, 3), new DateTime(2026, 6, 30)));
    }

    [TestMethod]
    public void ChamCong_TuTinhTrangThaiVaNgayCongQuyDoi()
    {
        var duCong = new ChamCong("Vũ Hải An", new DateTime(2026, 6, 1, 8, 0, 0), new DateTime(2026, 6, 1, 16, 0, 0), 0);
        var thieuGio = new ChamCong("Vũ Hải An", new DateTime(2026, 6, 1, 8, 0, 0), new DateTime(2026, 6, 1, 12, 0, 0), 0);
        var tangCa = new ChamCong("Vũ Hải An", new DateTime(2026, 6, 1, 8, 0, 0), new DateTime(2026, 6, 1, 18, 0, 0), 0);
        var dangTrongCa = new ChamCong("Vũ Hải An", new DateTime(2026, 6, 1, 8, 0, 0), null, 0);

        Assert.AreEqual("Đủ công", duCong.TrangThaiCong);
        Assert.AreEqual(8m, duCong.SoGioTinhToan);
        Assert.AreEqual(1m, duCong.NgayCongQuyDoi);
        Assert.AreEqual("Thiếu giờ", thieuGio.TrangThaiCong);
        Assert.AreEqual("Tăng ca", tangCa.TrangThaiCong);
        Assert.AreEqual("Đang trong ca", dangTrongCa.TrangThaiCong);
    }
}
