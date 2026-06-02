namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class CauHinhUngDungTests
{
    [TestMethod]
    public void LayChuoiKetNoiUngVien_LuonCoNguonMacDinh()
    {
        var danhSach = CauHinhUngDung.LayChuoiKetNoiUngVien();

        Assert.IsGreaterThanOrEqualTo(3, danhSach.Count);
        Assert.IsTrue(danhSach.Any(x => x.Contains(CauHinhUngDung.TenCoSoDuLieuMacDinh, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void LayMatKhauKhoiTao_CoGiaTriMacDinhDuManhChoBanGiao()
    {
        var matKhau = CauHinhUngDung.LayMatKhauKhoiTao();

        Assert.IsFalse(string.IsNullOrWhiteSpace(matKhau));
        Assert.IsGreaterThanOrEqualTo(10, matKhau.Length);
    }
}
