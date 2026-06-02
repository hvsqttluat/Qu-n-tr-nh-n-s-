namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class SapXepChucVuTests
{
    [TestMethod]
    public void LayThuTu_DuaChucVuQuanLyLenTruocNhanVien()
    {
        var danhSach = new[]
        {
            "Nhân viên kinh doanh",
            "Giám đốc điều hành",
            "Trưởng phòng nhân sự",
            "Công nhân sản xuất",
            "Phó giám đốc vận hành",
            "Chuyên viên pháp chế"
        };

        var ketQua = danhSach
            .OrderBy(BangXepHangChucVu.LayThuTu)
            .ToArray();

        Assert.AreEqual("Giám đốc điều hành", ketQua[0]);
        Assert.AreEqual("Phó giám đốc vận hành", ketQua[1]);
        Assert.AreEqual("Trưởng phòng nhân sự", ketQua[2]);
        Assert.AreEqual("Công nhân sản xuất", ketQua[^1]);
    }

    [TestMethod]
    public void NhanVien_CapBacChucVu_ThayDoiTheoViTri()
    {
        var nhanVien = new NhanVien { ViTri = "Trưởng phòng kỹ thuật" };

        Assert.AreEqual(3, nhanVien.ThuTuChucVu);
        Assert.AreEqual("Trưởng phòng", nhanVien.CapBacChucVu);

        nhanVien.ViTri = "Nhân viên kinh doanh";

        Assert.AreEqual(7, nhanVien.ThuTuChucVu);
        Assert.AreEqual("Nhân viên", nhanVien.CapBacChucVu);
    }
}
