using System.Security.Cryptography;

namespace QuanLyNhanSuWpf;

public record MatKhauDaBam(string HashBase64, string SaltBase64, int Iterations);

public static class BaoMatMatKhau
{
    public const int SoVongLapMacDinh = 210_000;
    private const int DoDaiSalt = 16;
    private const int DoDaiHash = 32;

    public static MatKhauDaBam BamMatKhau(string matKhau, int soVongLap = SoVongLapMacDinh)
    {
        if (string.IsNullOrWhiteSpace(matKhau))
        {
            throw new ArgumentException("Mat khau khong duoc de trong.", nameof(matKhau));
        }

        var salt = RandomNumberGenerator.GetBytes(DoDaiSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(matKhau, salt, soVongLap, HashAlgorithmName.SHA256, DoDaiHash);
        return new MatKhauDaBam(Convert.ToBase64String(hash), Convert.ToBase64String(salt), soVongLap);
    }

    public static bool XacMinhMatKhau(string matKhau, string hashBase64, string saltBase64, int soVongLap)
    {
        if (string.IsNullOrEmpty(matKhau) || string.IsNullOrWhiteSpace(hashBase64) || string.IsNullOrWhiteSpace(saltBase64))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var hashDaLuu = Convert.FromBase64String(hashBase64);
            var hashNhapVao = Rfc2898DeriveBytes.Pbkdf2(matKhau, salt, soVongLap, HashAlgorithmName.SHA256, hashDaLuu.Length);
            return CryptographicOperations.FixedTimeEquals(hashDaLuu, hashNhapVao);
        }
        catch
        {
            return false;
        }
    }
}
