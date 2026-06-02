namespace QuanLyNhanSuWpf.Tests;

[TestClass]
public sealed class BaoMatMatKhauTests
{
    [TestMethod]
    public void BamMatKhau_TaoSaltKhacNhauChoCungMotMatKhau()
    {
        var mot = BaoMatMatKhau.BamMatKhau("Admin@2026!");
        var hai = BaoMatMatKhau.BamMatKhau("Admin@2026!");

        Assert.AreNotEqual(mot.SaltBase64, hai.SaltBase64);
        Assert.AreNotEqual(mot.HashBase64, hai.HashBase64);
    }

    [TestMethod]
    public void XacMinhMatKhau_ChiDungVoiMatKhauGoc()
    {
        var matKhau = BaoMatMatKhau.BamMatKhau("Admin@2026!");

        Assert.IsTrue(BaoMatMatKhau.XacMinhMatKhau("Admin@2026!", matKhau.HashBase64, matKhau.SaltBase64, matKhau.Iterations));
        Assert.IsFalse(BaoMatMatKhau.XacMinhMatKhau("SaiMatKhau", matKhau.HashBase64, matKhau.SaltBase64, matKhau.Iterations));
    }
}
