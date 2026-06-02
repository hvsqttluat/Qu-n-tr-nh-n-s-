using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security;
using System.Text;

namespace QuanLyNhanSuWpf;

public record TaiLieuOffice(
    string TieuDe,
    IReadOnlyList<string> DoanVan,
    IReadOnlyList<string> TieuDeBang,
    IReadOnlyList<IReadOnlyList<string>> DongBang,
    string NguoiLap = "Nhân viên Phòng Nhân sự",
    string ChucVuNguoiLap = "Nhân viên Phòng Nhân sự");

public static class BoXuatOffice
{
    private static readonly UTF8Encoding Utf8KhongBom = new(false);
    private const string DonViChuQuan = "CÔNG TY QUẢN TRỊ NHÂN SỰ";
    private const string DonViLap = "PHÒNG NHÂN SỰ";
    private const string QuocHieu = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM";
    private const string TieuNgu = "Độc lập - Tự do - Hạnh phúc";
    private const string DiaDanh = "Hà Nội";

    public static void Xuat(string duongDan, TaiLieuOffice taiLieu)
    {
        var duoiFile = Path.GetExtension(duongDan).ToLowerInvariant();
        if (File.Exists(duongDan))
        {
            File.Delete(duongDan);
        }

        switch (duoiFile)
        {
            case ".docx":
                XuatDocx(duongDan, taiLieu);
                break;
            case ".xlsx":
                XuatXlsx(duongDan, taiLieu);
                break;
            case ".pptx":
                XuatPptx(duongDan, taiLieu);
                break;
            case ".pdf":
                XuatPdf(duongDan, taiLieu);
                break;
            case ".txt":
                File.WriteAllText(duongDan, TaoVanBanThuan(taiLieu), Utf8KhongBom);
                break;
            default:
                throw new InvalidOperationException("Chỉ hỗ trợ xuất .docx, .xlsx, .pdf, .pptx hoặc .txt.");
        }
    }

    private static void XuatDocx(string duongDan, TaiLieuOffice taiLieu)
    {
        using var zip = ZipFile.Open(duongDan, ZipArchiveMode.Create);
        ThemFile(zip, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
            </Types>
            """);
        ThemFile(zip, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
            </Relationships>
            """);
        ThemFile(zip, "word/document.xml", TaoDocumentXml(taiLieu));
    }

    private static void XuatXlsx(string duongDan, TaiLieuOffice taiLieu)
    {
        using var zip = ZipFile.Open(duongDan, ZipArchiveMode.Create);
        ThemFile(zip, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
            </Types>
            """);
        ThemFile(zip, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """);
        ThemFile(zip, "xl/workbook.xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="Bao cao" sheetId="1" r:id="rId1"/>
              </sheets>
            </workbook>
            """);
        ThemFile(zip, "xl/_rels/workbook.xml.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
            </Relationships>
            """);
        ThemFile(zip, "xl/worksheets/sheet1.xml", TaoWorksheetXml(taiLieu));
    }

    private static void XuatPptx(string duongDan, TaiLieuOffice taiLieu)
    {
        using var zip = ZipFile.Open(duongDan, ZipArchiveMode.Create);
        ThemFile(zip, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
              <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
            </Types>
            """);
        ThemFile(zip, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
            </Relationships>
            """);
        ThemFile(zip, "ppt/presentation.xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <p:presentation xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
              <p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst>
              <p:sldSz cx="12192000" cy="6858000" type="screen16x9"/>
              <p:notesSz cx="6858000" cy="9144000"/>
            </p:presentation>
            """);
        ThemFile(zip, "ppt/_rels/presentation.xml.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/>
            </Relationships>
            """);
        ThemFile(zip, "ppt/slides/slide1.xml", TaoSlideXml(taiLieu));
    }

    private static void XuatPdf(string duongDan, TaiLieuOffice taiLieu)
    {
        var wordType = Type.GetTypeFromProgID("Word.Application")
            ?? throw new InvalidOperationException("Máy cần cài Microsoft Word để xuất PDF từ mẫu hành chính.");

        var tam = Path.Combine(Path.GetTempPath(), $"QuanLyNhanSu_{Guid.NewGuid():N}.docx");
        object? word = null;
        object? doc = null;

        try
        {
            XuatDocx(tam, taiLieu);
            word = Activator.CreateInstance(wordType);
            wordType.InvokeMember("Visible", BindingFlags.SetProperty, null, word, [false]);
            wordType.InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, word, [0]);

            var documents = wordType.InvokeMember("Documents", BindingFlags.GetProperty, null, word, null);
            var openArgs = new object[]
            {
                tam, false, true, false,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, false,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing
            };
            doc = documents!.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, documents, openArgs);
            doc!.GetType().InvokeMember("ExportAsFixedFormat", BindingFlags.InvokeMethod, null, doc, [duongDan, 17]);
        }
        finally
        {
            if (doc is not null)
            {
                try { doc.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, doc, [false]); } catch { }
            }

            if (word is not null)
            {
                try { wordType.InvokeMember("Quit", BindingFlags.InvokeMethod, null, word, null); } catch { }
            }

            try { if (File.Exists(tam)) File.Delete(tam); } catch { }
        }
    }

    private static string TaoDocumentXml(TaiLieuOffice taiLieu)
    {
        var than = new StringBuilder();
        than.Append(TaoDauTrangHanhChinhWord(taiLieu));
        than.Append(TaoDoanWord(taiLieu.TieuDe, KieuDoanWord.TieuDe));
        than.Append(TaoDoanWord($"Kính gửi: Ban Giám đốc và Phòng Nhân sự", KieuDoanWord.Thuong));
        than.Append(TaoDoanWord("I. THÔNG TIN CHUNG", KieuDoanWord.Muc));
        foreach (var dong in LayCacDong(taiLieu.DoanVan))
        {
            if (string.IsNullOrWhiteSpace(dong))
            {
                than.Append(TaoDoanWord("", KieuDoanWord.Thuong));
            }
            else if (LaDongTieuMuc(dong))
            {
                than.Append(TaoDoanWord(dong, KieuDoanWord.Muc));
            }
            else
            {
                than.Append(TaoDoanWord(dong, KieuDoanWord.Thuong));
            }
        }

        if (taiLieu.TieuDeBang.Count > 0)
        {
            than.Append(TaoDoanWord("II. NỘI DUNG CHI TIẾT", KieuDoanWord.Muc));
            than.Append("<w:tbl><w:tblPr><w:tblW w:w=\"0\" w:type=\"auto\"/><w:tblBorders><w:top w:val=\"single\" w:sz=\"6\"/><w:left w:val=\"single\" w:sz=\"6\"/><w:bottom w:val=\"single\" w:sz=\"6\"/><w:right w:val=\"single\" w:sz=\"6\"/><w:insideH w:val=\"single\" w:sz=\"6\"/><w:insideV w:val=\"single\" w:sz=\"6\"/></w:tblBorders><w:tblCellMar><w:top w:w=\"80\" w:type=\"dxa\"/><w:left w:w=\"80\" w:type=\"dxa\"/><w:bottom w:w=\"80\" w:type=\"dxa\"/><w:right w:w=\"80\" w:type=\"dxa\"/></w:tblCellMar></w:tblPr>");
            than.Append(TaoDongWord(taiLieu.TieuDeBang, true));
            foreach (var dong in taiLieu.DongBang)
            {
                than.Append(TaoDongWord(dong, false));
            }
            than.Append("</w:tbl>");
        }

        than.Append(TaoChanTrangHanhChinhWord(taiLieu));

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
                {{than}}
                <w:sectPr><w:pgSz w:w="11906" w:h="16838"/><w:pgMar w:top="1134" w:right="1134" w:bottom="1134" w:left="1701"/></w:sectPr>
              </w:body>
            </w:document>
            """;
    }

    private static string TaoWorksheetXml(TaiLieuOffice taiLieu)
    {
        var dongSheet = TaoDongHanhChinh(taiLieu).Select(x => (IReadOnlyList<string>)x).ToList();
        dongSheet.AddRange(taiLieu.DoanVan.Select(x => new[] { x }));
        if (taiLieu.TieuDeBang.Count > 0)
        {
            dongSheet.Add(Array.Empty<string>());
            dongSheet.Add(taiLieu.TieuDeBang);
            dongSheet.AddRange(taiLieu.DongBang);
        }
        dongSheet.Add(Array.Empty<string>());
        dongSheet.Add(["III. NHẬN XÉT, KIẾN NGHỊ"]);
        dongSheet.Add(["Báo cáo được lập từ dữ liệu phần mềm quản lý nhân sự; các đơn vị liên quan kiểm tra, đối chiếu khi cần."]);
        dongSheet.Add(Array.Empty<string>());
        dongSheet.Add(["Nơi nhận:", "NGƯỜI LẬP BIỂU"]);
        dongSheet.Add(["- Ban Giám đốc; - Phòng Nhân sự; - Lưu: Hồ sơ dự án.", "(Ký, ghi rõ họ tên)"]);
        dongSheet.Add(["", taiLieu.NguoiLap]);

        var sheetData = new StringBuilder();
        for (var r = 0; r < dongSheet.Count; r++)
        {
            var rowIndex = r + 1;
            sheetData.Append($"<row r=\"{rowIndex}\">");
            for (var c = 0; c < dongSheet[r].Count; c++)
            {
                var cell = TaoTenCot(c + 1) + rowIndex;
                sheetData.Append($"<c r=\"{cell}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{Xml(dongSheet[r][c])}</t></is></c>");
            }
            sheetData.Append("</row>");
        }

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <cols><col min="1" max="12" width="24" customWidth="1"/></cols>
              <sheetData>{{sheetData}}</sheetData>
            </worksheet>
            """;
    }

    private static string TaoSlideXml(TaiLieuOffice taiLieu)
    {
        var dong = new List<string>
        {
            DonViChuQuan,
            DonViLap,
            $"{QuocHieu} - {TieuNgu}",
            $"{DiaDanh}, {TaoNgayHanhChinh(DateTime.Today)}",
            $"Số: {TaoSoVanBan(taiLieu)}/BC-NS",
            ""
        };
        dong.AddRange(LayCacDong(taiLieu.DoanVan).Take(9));
        if (taiLieu.TieuDeBang.Count > 0)
        {
            dong.Add("");
            dong.Add(string.Join(" | ", taiLieu.TieuDeBang));
            dong.AddRange(taiLieu.DongBang.Take(8).Select(x => string.Join(" | ", x)));
        }
        dong.Add("");
        dong.Add("Nơi nhận: Ban Giám đốc, Phòng Nhân sự, lưu hồ sơ dự án.");
        dong.Add($"Người lập biểu: {taiLieu.NguoiLap} - {taiLieu.ChucVuNguoiLap}");

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <p:sld xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
              <p:cSld>
                <p:spTree>
                  <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                  <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                  {{TaoHopChuPpt(2, "Tieu de", taiLieu.TieuDe, 600000, 360000, 10900000, 680000, 3200, true)}}
                  {{TaoHopChuPpt(3, "Noi dung", string.Join("\n", dong), 760000, 1180000, 10600000, 5200000, 1700, false)}}
                </p:spTree>
              </p:cSld>
              <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
            </p:sld>
            """;
    }

    private static string TaoDauTrangHanhChinhWord(TaiLieuOffice taiLieu)
    {
        var soVanBan = $"Số: {TaoSoVanBan(taiLieu)}/BC-NS";
        var ngay = $"{DiaDanh}, {TaoNgayHanhChinh(DateTime.Today)}";
        return $$"""
            <w:tbl><w:tblPr><w:tblW w:w="0" w:type="auto"/></w:tblPr>
              <w:tr>
                <w:tc><w:tcPr><w:tcW w:w="4600" w:type="dxa"/></w:tcPr>
                  {{TaoDoanWord(DonViChuQuan, KieuDoanWord.CoQuan)}}
                  {{TaoDoanWord(DonViLap, KieuDoanWord.CoQuan)}}
                  {{TaoDoanWord(soVanBan, KieuDoanWord.CanTrai)}}
                </w:tc>
                <w:tc><w:tcPr><w:tcW w:w="5200" w:type="dxa"/></w:tcPr>
                  {{TaoDoanWord(QuocHieu, KieuDoanWord.CoQuan)}}
                  {{TaoDoanWord(TieuNgu, KieuDoanWord.CoQuan)}}
                  {{TaoDoanWord(ngay, KieuDoanWord.NgayThang)}}
                </w:tc>
              </w:tr>
            </w:tbl>
            {{TaoDoanWord("", KieuDoanWord.Thuong)}}
            """;
    }

    private static string TaoChanTrangHanhChinhWord(TaiLieuOffice taiLieu)
    {
        return $$"""
            {{TaoDoanWord("III. NHẬN XÉT, KIẾN NGHỊ", KieuDoanWord.Muc)}}
            {{TaoDoanWord("Báo cáo được lập từ dữ liệu phần mềm quản lý nhân sự. Các đơn vị liên quan kiểm tra, đối chiếu và phản hồi khi cần điều chỉnh.", KieuDoanWord.Thuong)}}
            <w:tbl><w:tblPr><w:tblW w:w="0" w:type="auto"/></w:tblPr>
              <w:tr>
                <w:tc><w:tcPr><w:tcW w:w="4800" w:type="dxa"/></w:tcPr>
                  {{TaoDoanWord("Nơi nhận:", KieuDoanWord.DamTrai)}}
                  {{TaoDoanWord("- Ban Giám đốc;", KieuDoanWord.CanTrai)}}
                  {{TaoDoanWord("- Phòng Nhân sự;", KieuDoanWord.CanTrai)}}
                  {{TaoDoanWord("- Lưu: Hồ sơ dự án.", KieuDoanWord.CanTrai)}}
                </w:tc>
                <w:tc><w:tcPr><w:tcW w:w="5000" w:type="dxa"/></w:tcPr>
                  {{TaoDoanWord("NGƯỜI LẬP BIỂU", KieuDoanWord.CoQuan)}}
                  {{TaoDoanWord("(Ký, ghi rõ họ tên)", KieuDoanWord.NgayThang)}}
                  {{TaoDoanWord("", KieuDoanWord.Thuong)}}
                  {{TaoDoanWord("", KieuDoanWord.Thuong)}}
                  {{TaoDoanWord(taiLieu.NguoiLap, KieuDoanWord.CoQuan)}}
                </w:tc>
              </w:tr>
            </w:tbl>
            """;
    }

    private static string TaoDoanWord(string vanBan, KieuDoanWord kieu = KieuDoanWord.Thuong)
    {
        var (canLe, dam, nghieng, coChu, allCaps) = kieu switch
        {
            KieuDoanWord.TieuDe => ("center", true, false, 32, true),
            KieuDoanWord.Muc => ("left", true, false, 28, false),
            KieuDoanWord.CoQuan => ("center", true, false, 26, false),
            KieuDoanWord.NgayThang => ("center", false, true, 26, false),
            KieuDoanWord.DamTrai => ("left", true, false, 26, false),
            KieuDoanWord.CanTrai => ("left", false, false, 26, false),
            _ => ("both", false, false, 28, false)
        };
        var text = allCaps ? vanBan.ToUpperInvariant() : vanBan;
        var rPr = $"<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"{coChu}\"/>{(dam ? "<w:b/>" : "")}{(nghieng ? "<w:i/>" : "")}</w:rPr>";
        return $"<w:p><w:pPr><w:jc w:val=\"{canLe}\"/><w:spacing w:after=\"120\"/></w:pPr><w:r>{rPr}<w:t xml:space=\"preserve\">{Xml(text)}</w:t></w:r></w:p>";
    }

    private static string TaoDongWord(IReadOnlyList<string> o, bool dam)
    {
        var ketQua = new StringBuilder("<w:tr>");
        foreach (var giaTri in o)
        {
            var rPr = $"<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"24\"/>{(dam ? "<w:b/>" : "")}</w:rPr>";
            ketQua.Append($"<w:tc><w:tcPr><w:tcW w:w=\"2400\" w:type=\"dxa\"/></w:tcPr><w:p><w:r>{rPr}<w:t xml:space=\"preserve\">{Xml(giaTri)}</w:t></w:r></w:p></w:tc>");
        }
        ketQua.Append("</w:tr>");
        return ketQua.ToString();
    }

    private static string TaoHopChuPpt(int id, string ten, string noiDung, int x, int y, int cx, int cy, int coChu, bool dam)
    {
        var rPr = dam ? $"<a:rPr lang=\"vi-VN\" sz=\"{coChu}\" b=\"1\"/>" : $"<a:rPr lang=\"vi-VN\" sz=\"{coChu}\"/>";
        var doan = string.Join("", noiDung.Split('\n').Select(dong => $"<a:p><a:r>{rPr}<a:t>{Xml(dong)}</a:t></a:r></a:p>"));
        return $$"""
            <p:sp>
              <p:nvSpPr><p:cNvPr id="{{id}}" name="{{Xml(ten)}}"/><p:cNvSpPr txBox="1"/><p:nvPr/></p:nvSpPr>
              <p:spPr><a:xfrm><a:off x="{{x}}" y="{{y}}"/><a:ext cx="{{cx}}" cy="{{cy}}"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom><a:noFill/></p:spPr>
              <p:txBody><a:bodyPr wrap="square"/><a:lstStyle/>{{doan}}</p:txBody>
            </p:sp>
            """;
    }

    private static string TaoVanBanThuan(TaiLieuOffice taiLieu)
    {
        var ketQua = new StringBuilder();
        foreach (var dong in TaoDongHanhChinh(taiLieu).SelectMany(x => x))
        {
            ketQua.AppendLine(dong);
        }

        ketQua.AppendLine();
        ketQua.AppendLine();
        foreach (var dong in taiLieu.DoanVan)
        {
            ketQua.AppendLine(dong);
        }

        if (taiLieu.TieuDeBang.Count > 0)
        {
            ketQua.AppendLine();
            ketQua.AppendLine(string.Join("\t", taiLieu.TieuDeBang));
            foreach (var dong in taiLieu.DongBang)
            {
                ketQua.AppendLine(string.Join("\t", dong));
            }
        }

        ketQua.AppendLine();
        ketQua.AppendLine("III. NHẬN XÉT, KIẾN NGHỊ");
        ketQua.AppendLine("Báo cáo được lập từ dữ liệu phần mềm quản lý nhân sự; các đơn vị liên quan kiểm tra, đối chiếu khi cần.");
        ketQua.AppendLine();
        ketQua.AppendLine("Nơi nhận:\tNGƯỜI LẬP BIỂU");
        ketQua.AppendLine("- Ban Giám đốc; - Phòng Nhân sự; - Lưu: Hồ sơ dự án.\t(Ký, ghi rõ họ tên)");
        ketQua.AppendLine($"\t{taiLieu.NguoiLap}");

        return ketQua.ToString();
    }

    private static IEnumerable<string[]> TaoDongHanhChinh(TaiLieuOffice taiLieu)
    {
        yield return [DonViChuQuan, QuocHieu];
        yield return [DonViLap, TieuNgu];
        yield return [$"Số: {TaoSoVanBan(taiLieu)}/BC-NS", $"{DiaDanh}, {TaoNgayHanhChinh(DateTime.Today)}"];
        yield return [];
        yield return [taiLieu.TieuDe.ToUpperInvariant()];
        yield return ["Kính gửi: Ban Giám đốc và Phòng Nhân sự"];
        yield return [];
        yield return ["I. THÔNG TIN CHUNG"];
    }

    private static string TaoNgayHanhChinh(DateTime ngay)
    {
        return $"ngày {ngay:dd} tháng {ngay:MM} năm {ngay:yyyy}";
    }

    private static string TaoSoVanBan(TaiLieuOffice taiLieu)
    {
        var khongDau = LoaiBoDau(taiLieu.TieuDe).ToUpperInvariant();
        if (khongDau.Contains("HOP DONG")) return $"HD-{DateTime.Today:yyyyMMdd}";
        if (khongDau.Contains("CHAM CONG")) return $"CC-{DateTime.Today:yyyyMMdd}";
        if (khongDau.Contains("NGHI PHEP")) return $"NP-{DateTime.Today:yyyyMMdd}";
        if (khongDau.Contains("LUONG")) return $"LNS-{DateTime.Today:yyyyMMdd}";
        if (khongDau.Contains("HO SO")) return $"HSNS-{DateTime.Today:yyyyMMdd}";
        return $"NS-{DateTime.Today:yyyyMMdd}";
    }

    private static bool LaDongTieuMuc(string dong)
    {
        var giaTri = dong.Trim();
        return giaTri.Length > 0
            && giaTri.Length <= 80
            && giaTri == giaTri.ToUpperInvariant()
            && giaTri.Any(char.IsLetter);
    }

    private static string LoaiBoDau(string vanBan)
    {
        var chuanHoa = vanBan.Normalize(NormalizationForm.FormD);
        var ketQua = new StringBuilder();
        foreach (var kyTu in chuanHoa)
        {
            var loai = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(kyTu);
            if (loai != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                ketQua.Append(kyTu == 'đ' ? 'd' : kyTu == 'Đ' ? 'D' : kyTu);
            }
        }

        return ketQua.ToString().Normalize(NormalizationForm.FormC);
    }

    private static IEnumerable<string> LayCacDong(IEnumerable<string> doanVan)
    {
        return doanVan.SelectMany(x => x.Replace("\r", "").Split('\n'));
    }

    private static void ThemFile(ZipArchive zip, string ten, string noiDung)
    {
        var entry = zip.CreateEntry(ten, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Utf8KhongBom);
        writer.Write(noiDung);
    }

    private static string Xml(string? giaTri)
    {
        return SecurityElement.Escape(giaTri ?? "") ?? "";
    }

    private static string TaoTenCot(int soCot)
    {
        var ten = "";
        while (soCot > 0)
        {
            soCot--;
            ten = (char)('A' + soCot % 26) + ten;
            soCot /= 26;
        }
        return ten;
    }

    private enum KieuDoanWord
    {
        Thuong,
        TieuDe,
        Muc,
        CoQuan,
        NgayThang,
        DamTrai,
        CanTrai
    }
}
