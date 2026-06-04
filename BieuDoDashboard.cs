using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace QuanLyNhanSuWpf;

public class BieuDoTronNhanSu : FrameworkElement
{
    public static readonly DependencyProperty DangLamViecProperty =
        DependencyProperty.Register(nameof(DangLamViec), typeof(int), typeof(BieuDoTronNhanSu),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TamNghiProperty =
        DependencyProperty.Register(nameof(TamNghi), typeof(int), typeof(BieuDoTronNhanSu),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int DangLamViec
    {
        get => (int)GetValue(DangLamViecProperty);
        set => SetValue(DangLamViecProperty, value);
    }

    public int TamNghi
    {
        get => (int)GetValue(TamNghiProperty);
        set => SetValue(TamNghiProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var tong = Math.Max(0, DangLamViec) + Math.Max(0, TamNghi);
        var kichThuoc = Math.Max(0, Math.Min(ActualWidth, ActualHeight) - 12);
        if (kichThuoc <= 0) return;

        var tam = new Point(ActualWidth / 2, ActualHeight / 2);
        var banKinh = kichThuoc / 2;
        var banKinhTrong = banKinh * 0.58;

        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(241, 245, 249)), null, tam, banKinh, banKinh);

        if (tong > 0)
        {
            var gocLamViec = 360.0 * DangLamViec / tong;
            VeLat(dc, tam, banKinh, -90, gocLamViec, new SolidColorBrush(Color.FromRgb(15, 118, 110)));
            VeLat(dc, tam, banKinh, -90 + gocLamViec, 360 - gocLamViec, new SolidColorBrush(Color.FromRgb(180, 83, 9)));
        }

        dc.DrawEllipse(Brushes.White, null, tam, banKinhTrong, banKinhTrong);
        VeChuCanGiua(dc, tong.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")), 24, FontWeights.SemiBold, "#172033", tam.X, tam.Y - 12);
        VeChuCanGiua(dc, "Tổng nhân sự", 12, FontWeights.SemiBold, "#667085", tam.X, tam.Y + 16);
    }

    private static void VeLat(DrawingContext dc, Point tam, double banKinh, double gocBatDau, double gocQuet, Brush mau)
    {
        if (gocQuet <= 0.1) return;
        if (gocQuet >= 359.9)
        {
            dc.DrawEllipse(mau, null, tam, banKinh, banKinh);
            return;
        }

        var diemDau = LayDiem(tam, banKinh, gocBatDau);
        var diemCuoi = LayDiem(tam, banKinh, gocBatDau + gocQuet);
        var hinh = new StreamGeometry();

        using (var ctx = hinh.Open())
        {
            ctx.BeginFigure(tam, true, true);
            ctx.LineTo(diemDau, true, true);
            ctx.ArcTo(diemCuoi, new Size(banKinh, banKinh), 0, gocQuet > 180, SweepDirection.Clockwise, true, true);
            ctx.LineTo(tam, true, true);
        }

        hinh.Freeze();
        dc.DrawGeometry(mau, null, hinh);
    }

    private static Point LayDiem(Point tam, double banKinh, double goc)
    {
        var rad = goc * Math.PI / 180;
        return new Point(tam.X + Math.Cos(rad) * banKinh, tam.Y + Math.Sin(rad) * banKinh);
    }

    private void VeChuCanGiua(DrawingContext dc, string noiDung, double coChu, FontWeight doDam, string mau, double x, double y)
    {
        var chu = TaoChu(noiDung, coChu, doDam, mau);
        dc.DrawText(chu, new Point(x - chu.Width / 2, y - chu.Height / 2));
    }

    private FormattedText TaoChu(string noiDung, double coChu, FontWeight doDam, string mau)
    {
        return new FormattedText(
            noiDung,
            CultureInfo.GetCultureInfo("vi-VN"),
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, doDam, FontStretches.Normal),
            coChu,
            (Brush)new BrushConverter().ConvertFromString(mau)!,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }
}

public class BieuDoDuongLuongNhanSu : FrameworkElement
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BieuDoDuongLuongNhanSu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var duLieu = ItemsSource?.OfType<DiemLuongThang>().ToList() ?? [];
        var khung = new Rect(54, 18, Math.Max(20, ActualWidth - 84), Math.Max(20, ActualHeight - 62));
        var vien = new Pen(new SolidColorBrush(Color.FromRgb(216, 224, 234)), 1);
        var luoi = new Pen(new SolidColorBrush(Color.FromRgb(226, 232, 240)), 1);

        for (var i = 0; i <= 4; i++)
        {
            var y = khung.Top + khung.Height * i / 4;
            dc.DrawLine(luoi, new Point(khung.Left, y), new Point(khung.Right, y));
        }

        dc.DrawLine(vien, new Point(khung.Left, khung.Top), new Point(khung.Left, khung.Bottom));
        dc.DrawLine(vien, new Point(khung.Left, khung.Bottom), new Point(khung.Right, khung.Bottom));

        if (duLieu.Count == 0)
        {
            VeChu(dc, "Chưa có dữ liệu lương", 13, FontWeights.SemiBold, "#667085", khung.Left + 12, khung.Top + 12);
            return;
        }

        var soNhanSuLonNhat = duLieu.Max(x => x.TongNhanVien);
        var maxLuong = Math.Max(1, (double)duLieu.Max(x => x.TongLuong));
        var maxNhanSu = Math.Max(1, soNhanSuLonNhat);
        var buocX = duLieu.Count == 1 ? khung.Width : khung.Width / (duLieu.Count - 1);
        var diemLuong = new List<Point>();
        var diemNhanSu = new List<Point>();

        for (var i = 0; i < duLieu.Count; i++)
        {
            var x = khung.Left + buocX * i;
            diemLuong.Add(new Point(x, khung.Bottom - (double)duLieu[i].TongLuong / maxLuong * khung.Height));
            diemNhanSu.Add(new Point(x, khung.Bottom - duLieu[i].TongNhanVien / (double)maxNhanSu * khung.Height));
            VeChuCanGiua(dc, duLieu[i].Thang, 10, FontWeights.SemiBold, "#667085", x, khung.Bottom + 18);
        }

        VeDuong(dc, diemLuong, new Pen(new SolidColorBrush(Color.FromRgb(37, 99, 235)), 3));
        VeDuong(dc, diemNhanSu, new Pen(new SolidColorBrush(Color.FromRgb(180, 83, 9)), 3));

        foreach (var diem in diemLuong)
        {
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(37, 99, 235)), new Pen(Brushes.White, 2), diem, 4.5, 4.5);
        }

        foreach (var diem in diemNhanSu)
        {
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(180, 83, 9)), new Pen(Brushes.White, 2), diem, 4.5, 4.5);
        }

        VeChu(dc, $"{duLieu.Max(x => x.TongLuong):N0} đ", 10, FontWeights.SemiBold, "#2563EB", 4, khung.Top - 3);
        VeChu(dc, $"{soNhanSuLonNhat:N0} NV", 10, FontWeights.SemiBold, "#B45309", khung.Right + 8, khung.Top - 3);
    }

    private static void VeDuong(DrawingContext dc, IList<Point> diem, Pen but)
    {
        if (diem.Count < 2) return;
        var hinh = new StreamGeometry();
        using (var ctx = hinh.Open())
        {
            ctx.BeginFigure(diem[0], false, false);
            ctx.PolyLineTo(diem.Skip(1).ToList(), true, true);
        }

        hinh.Freeze();
        dc.DrawGeometry(null, but, hinh);
    }

    private void VeChu(DrawingContext dc, string noiDung, double coChu, FontWeight doDam, string mau, double x, double y)
    {
        dc.DrawText(TaoChu(noiDung, coChu, doDam, mau), new Point(x, y));
    }

    private void VeChuCanGiua(DrawingContext dc, string noiDung, double coChu, FontWeight doDam, string mau, double x, double y)
    {
        var chu = TaoChu(noiDung, coChu, doDam, mau);
        dc.DrawText(chu, new Point(x - chu.Width / 2, y - chu.Height / 2));
    }

    private FormattedText TaoChu(string noiDung, double coChu, FontWeight doDam, string mau)
    {
        return new FormattedText(
            noiDung,
            CultureInfo.GetCultureInfo("vi-VN"),
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, doDam, FontStretches.Normal),
            coChu,
            (Brush)new BrushConverter().ConvertFromString(mau)!,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }
}

public class BieuDoCotUngVien : FrameworkElement
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BieuDoCotUngVien),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var duLieu = ItemsSource?.OfType<MucUngVienTheoViTri>().Take(8).ToList() ?? [];
        var khung = new Rect(18, 18, Math.Max(20, ActualWidth - 36), Math.Max(20, ActualHeight - 36));

        if (duLieu.Count == 0)
        {
            VeChu(dc, "Chưa có ứng viên", 13, FontWeights.SemiBold, "#667085", khung.Left + 12, khung.Top + 12);
            return;
        }

        var max = Math.Max(1, duLieu.Max(x => x.SoLuong));
        var nhanRong = Math.Min(190, Math.Max(130, khung.Width * 0.34));
        var thanhTrai = khung.Left + nhanRong + 18;
        var thanhRongToiDa = Math.Max(80, khung.Right - thanhTrai - 46);
        var buocDong = Math.Min(40, khung.Height / Math.Max(1, duLieu.Count));
        var caoThanh = Math.Min(24, Math.Max(14, buocDong * 0.58));
        var mau = new[]
        {
            Color.FromRgb(37, 99, 235),
            Color.FromRgb(16, 118, 110),
            Color.FromRgb(217, 119, 6),
            Color.FromRgb(124, 58, 237),
            Color.FromRgb(8, 145, 178),
            Color.FromRgb(220, 38, 38)
        };

        for (var i = 0; i < duLieu.Count; i++)
        {
            var muc = duLieu[i];
            var y = khung.Top + i * buocDong + (buocDong - caoThanh) / 2;
            var rong = thanhRongToiDa * muc.SoLuong / max;
            var brush = new SolidColorBrush(mau[i % mau.Length]);

            VeChu(dc, CatNgan(muc.TenViTri), 11, FontWeights.SemiBold, "#334155", khung.Left, y + 3);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(241, 245, 249)), null, new Rect(thanhTrai, y, thanhRongToiDa, caoThanh), 7, 7);
            dc.DrawRoundedRectangle(brush, null, new Rect(thanhTrai, y, rong, caoThanh), 7, 7);
            VeChu(dc, muc.SoLuong.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")), 11, FontWeights.SemiBold, "#172033", thanhTrai + thanhRongToiDa + 12, y + 3);
        }
    }

    private static string CatNgan(string giaTri)
    {
        return giaTri.Length <= 26 ? giaTri : giaTri[..25] + "...";
    }

    private void VeChu(DrawingContext dc, string noiDung, double coChu, FontWeight doDam, string mau, double x, double y)
    {
        dc.DrawText(TaoChu(noiDung, coChu, doDam, mau), new Point(x, y));
    }

    private void VeChuCanGiua(DrawingContext dc, string noiDung, double coChu, FontWeight doDam, string mau, double x, double y)
    {
        var chu = TaoChu(noiDung, coChu, doDam, mau);
        dc.DrawText(chu, new Point(x - chu.Width / 2, y - chu.Height / 2));
    }

    private FormattedText TaoChu(string noiDung, double coChu, FontWeight doDam, string mau)
    {
        return new FormattedText(
            noiDung,
            CultureInfo.GetCultureInfo("vi-VN"),
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, doDam, FontStretches.Normal),
            coChu,
            (Brush)new BrushConverter().ConvertFromString(mau)!,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }
}
