$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$TemplateDir = "D:\TÀI LIỆU CÔNG NGHỆ PHẦN MỀM\Mẫu báo cáo phần mềm"
$GeneratedDir = Join-Path $Root "BaoCaoPhanMem"
$OutDir = Join-Path $Root "BaoCaoTheoMauGoc"
$BonusDir = Join-Path $OutDir "Bonus_Rieng"
$Today = "01/06/2026"
$MonthYear = "06/2026"
$ProjectName = "PHẦN MỀM QUẢN LÝ NHÂN SỰ WPF"
$ProjectCode = "QLNS-WPF"
$Team = "Nhóm 3"
$Teacher = "Phan Nguyên Hải"
$Members = @("Trần Văn Luật", "Nguyễn Đình Tuyến", "Trần Thanh Long")

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
New-Item -ItemType Directory -Force -Path $BonusDir | Out-Null

function Clean-WordText([string]$Text) {
    return (($Text -replace "`r|`a|\x07", "").Trim())
}

function Set-RangeText($Range, [string]$Text, [bool]$Paragraph = $true) {
    try {
        if ($Paragraph) {
            $Range.Text = $Text + "`r"
        }
        else {
            $Range.Text = $Text
        }
    }
    catch {
        try {
            $dup = $Range.Duplicate
            if ($dup.End -gt $dup.Start) {
                $dup.End = $dup.End - 1
            }
            $dup.Text = $Text
        }
        catch {}
    }
}

function Replace-InDoc($Doc, [hashtable]$Map) {
    foreach ($p in @($Doc.Paragraphs)) {
        $text = Clean-WordText $p.Range.Text
        if ($text.Length -eq 0) { continue }
        $new = $text
        foreach ($key in $Map.Keys) {
            $new = $new.Replace($key, [string]$Map[$key])
        }
        if ($new -ne $text) {
            Set-RangeText $p.Range $new $true
        }
    }

    foreach ($table in @($Doc.Tables)) {
        foreach ($cell in @($table.Range.Cells)) {
            try {
                $text = Clean-WordText $cell.Range.Text
                if ($text.Length -eq 0) { continue }
                $new = $text
                foreach ($key in $Map.Keys) {
                    $new = $new.Replace($key, [string]$Map[$key])
                }
                if ($new -ne $text) {
                    Set-RangeText $cell.Range $new $false
                }
            }
            catch {}
        }
    }
}

function Fill-Signatures($Doc) {
    $nameSeq = @($Members[0], $Members[1], $Members[2], $Members[0], $Teacher)
    $nameIndex = 0
    foreach ($p in @($Doc.Paragraphs)) {
        $text = Clean-WordText $p.Range.Text
        if ($text -like "*[Họ và tên]*") {
            $name = $nameSeq[[Math]::Min($nameIndex, $nameSeq.Count - 1)]
            $new = $text.Replace("[Họ và tên]", $name).Replace("Ngày", "Ngày $Today")
            Set-RangeText $p.Range $new $true
            $nameIndex++
        }
    }

    $roleSeq = @(
        "Nhóm trưởng - Phân tích yêu cầu, tài liệu",
        "Lập trình, thiết kế CSDL, kiểm thử",
        "Thiết kế giao diện, kiểm thử, báo cáo"
    )
    $roleIndex = 0
    foreach ($p in @($Doc.Paragraphs)) {
        $text = Clean-WordText $p.Range.Text
        if ($text -eq "[Chức vụ]") {
            $role = $roleSeq[[Math]::Min($roleIndex, $roleSeq.Count - 1)]
            Set-RangeText $p.Range $role $true
            $roleIndex++
        }
        elseif ($text -eq "[Nhóm trưởng]") {
            Set-RangeText $p.Range "Nhóm trưởng" $true
        }
        elseif ($text -eq "[Giáo viên hướng dẫn]") {
            Set-RangeText $p.Range "Giảng viên hướng dẫn" $true
        }
    }
}

function Fill-ChangeTable($Doc, [string]$Note) {
    if ($Doc.Tables.Count -lt 1) { return }
    $table = $Doc.Tables.Item(1)
    if ($table.Rows.Count -lt 2 -or $table.Columns.Count -lt 6) { return }
    $values = @($Today, "Hoàn thiện tài liệu", "Điền nội dung theo đề tài", "Mẫu báo cáo", $Note, "1.0")
    for ($i = 1; $i -le 6; $i++) {
        try { $table.Cell(2, $i).Range.Text = $values[$i - 1] } catch {}
    }
}

function Remove-BodyFromText($Doc, [string]$Prefix) {
    $start = $null
    foreach ($p in @($Doc.Paragraphs)) {
        $text = Clean-WordText $p.Range.Text
        $style = ""
        try { $style = [string]$p.Range.Style.NameLocal } catch {}
        if ($text -eq $Prefix -and $style -notlike "TOC*") {
            $start = $p.Range.Start
            break
        }
    }
    if ($null -eq $start) {
        foreach ($p in @($Doc.Paragraphs)) {
            $text = Clean-WordText $p.Range.Text
            $style = ""
            try { $style = [string]$p.Range.Style.NameLocal } catch {}
            if ($text.StartsWith($Prefix) -and $text -notmatch "`t\d+$" -and $style -notlike "TOC*") {
                $start = $p.Range.Start
                break
            }
        }
    }
    if ($null -eq $start) {
        throw "Không tìm thấy đoạn bắt đầu bằng '$Prefix'."
    }
    $range = $Doc.Range($start, $Doc.Content.End)
    $range.Delete() | Out-Null
}

function Remove-BodyFromFirstHeading($Doc) {
    $start = $null
    foreach ($p in @($Doc.Paragraphs)) {
        $text = Clean-WordText $p.Range.Text
        $style = ""
        try { $style = [string]$p.Range.Style.NameLocal } catch {}
        if ($text -eq "1. GIỚI THIỆU" -and $style -notlike "TOC*") {
            $start = $p.Range.Start
            break
        }
    }
    if ($null -eq $start) {
        foreach ($p in @($Doc.Paragraphs)) {
            $style = ""
            try { $style = [string]$p.Range.Style.NameLocal } catch {}
            if ($style -eq "Heading 1" -and (Clean-WordText $p.Range.Text) -notmatch "`t\d+$") {
                $start = $p.Range.Start
                break
            }
        }
    }
    if ($null -eq $start) {
        throw "Không tìm thấy Heading 1 để thay nội dung."
    }
    $range = $Doc.Range($start, $Doc.Content.End)
    $range.Delete() | Out-Null
}

function Set-SelectionStyle($Doc, $Selection, [string]$StyleName) {
    try {
        $Selection.Style = $Doc.Styles.Item($StyleName)
    }
    catch {
        $Selection.Style = $Doc.Styles.Item("Normal")
    }
}

function Add-Heading($Doc, $Selection, [string]$Text, [int]$Level = 1) {
    Set-SelectionStyle $Doc $Selection "Heading $Level"
    $Selection.TypeText($Text)
    $Selection.TypeParagraph()
}

function Add-Para($Doc, $Selection, [string]$Text) {
    Set-SelectionStyle $Doc $Selection "Normal"
    $Selection.TypeText($Text)
    $Selection.TypeParagraph()
}

function Add-BulletList($Doc, $Selection, [string[]]$Items) {
    foreach ($item in $Items) {
        try { Set-SelectionStyle $Doc $Selection "List Bullet" } catch { Set-SelectionStyle $Doc $Selection "Normal" }
        $Selection.TypeText($item)
        $Selection.TypeParagraph()
    }
    Set-SelectionStyle $Doc $Selection "Normal"
}

function Add-NumberList($Doc, $Selection, [string[]]$Items) {
    foreach ($item in $Items) {
        try { Set-SelectionStyle $Doc $Selection "List Number" } catch { Set-SelectionStyle $Doc $Selection "Normal" }
        $Selection.TypeText($item)
        $Selection.TypeParagraph()
    }
    Set-SelectionStyle $Doc $Selection "Normal"
}

function Add-WordTable($Doc, $Selection, [string[]]$Headers, [object[]]$Rows) {
    $rowCount = $Rows.Count + 1
    $colCount = $Headers.Count
    $table = $Doc.Tables.Add($Selection.Range, $rowCount, $colCount)
    $table.Borders.Enable = 1
    try { $table.Style = "Table Grid" } catch {}
    for ($c = 1; $c -le $colCount; $c++) {
        $table.Cell(1, $c).Range.Text = $Headers[$c - 1]
        $table.Cell(1, $c).Range.Bold = 1
    }
    for ($r = 0; $r -lt $Rows.Count; $r++) {
        $row = $Rows[$r]
        for ($c = 0; $c -lt $colCount; $c++) {
            try { $table.Cell($r + 2, $c + 1).Range.Text = [string]$row[$c] } catch {}
        }
    }
    $Selection.SetRange($table.Range.End, $table.Range.End)
    $Selection.TypeParagraph()
}

function Prepare-Doc($Word, [string]$TemplateName, [string]$OutputName, [string]$DocCode, [string]$ChangeNote, [bool]$UseHeadingBody = $true, [string]$BodyPrefix = "") {
    $src = Join-Path $TemplateDir $TemplateName
    $dest = Join-Path $OutDir $OutputName
    Copy-Item -LiteralPath $src -Destination $dest -Force
    $doc = $Word.Documents.Open($dest, $false, $false)
    $map = @{
        "[Tên dự án]" = $ProjectName
        "[MaDA]" = $ProjectCode
        "[MaTailieu]" = $DocCode
        "[MaTL]" = $DocCode
        "[v1.0]" = "v1.0"
        "[tháng/năm]" = $MonthYear
        "Hà Nội, ngày    tháng    năm" = "Hà Nội, ngày 01 tháng 06 năm 2026"
        "[Khoa CNTT]" = "Khoa Công nghệ thông tin"
        "[Nhóm đề tài]" = $Team
        "[Giáo viên HD]" = $Teacher
        "[Từ ngày .. đến ngày]" = "27/05/2026 đến 01/06/2026"
    }
    Replace-InDoc $doc $map
    Fill-Signatures $doc
    Fill-ChangeTable $doc $ChangeNote
    if ($UseHeadingBody) {
        Remove-BodyFromFirstHeading $doc
    }
    else {
        Remove-BodyFromText $doc $BodyPrefix
    }
    $Word.Selection.EndKey(6) | Out-Null
    return $doc
}

function Finish-Doc($Doc) {
    try { $Doc.Fields.Update() | Out-Null } catch {}
    try {
        for ($i = 1; $i -le $Doc.TablesOfContents.Count; $i++) {
            $Doc.TablesOfContents.Item($i).Update() | Out-Null
        }
    } catch {}
    $Doc.Save()
    $Doc.Close($false)
}

function Write-Feasibility($Doc, $Selection) {
    Add-Heading $Doc $Selection "1. GIỚI THIỆU" 1
    Add-Heading $Doc $Selection "1.1. Mục đích tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu này trình bày kết quả nghiên cứu tính khả thi cho dự án $ProjectName. Báo cáo làm rõ nhu cầu, phạm vi, mục tiêu, phương án triển khai và mức độ khả thi của dự án trước khi nhóm thực hiện các bước đặc tả, thiết kế, lập trình và kiểm thử."
    Add-Heading $Doc $Selection "1.2. Phạm vi tài liệu" 2
    Add-Para $Doc $Selection "Phạm vi báo cáo tập trung vào đề tài phần mềm quản lý nhân sự chạy trên Windows bằng WPF, sử dụng SQL Server làm cơ sở dữ liệu chính và có dữ liệu mẫu cục bộ để phục vụ demo khi chưa có SQL Server."
    Add-Heading $Doc $Selection "1.3. Thuật ngữ và các từ viết tắt" 2
    Add-WordTable $Doc $Selection @("Thuật ngữ", "Định nghĩa", "Giải thích") @(
        @("CNTT", "Công nghệ thông tin", "Lĩnh vực ứng dụng công nghệ vào xử lý thông tin"),
        @("CNPM", "Công nghệ phần mềm", "Quy trình xây dựng, kiểm thử và bảo trì phần mềm"),
        @("CSDL", "Cơ sở dữ liệu", "Nơi lưu trữ dữ liệu nhân sự của hệ thống"),
        @("WPF", "Windows Presentation Foundation", "Nền tảng xây dựng giao diện desktop Windows"),
        @("SQL Server", "Hệ quản trị CSDL quan hệ", "Nơi lưu dữ liệu HRManagementDB")
    )
    Add-Heading $Doc $Selection "1.4. Tài liệu tham khảo" 2
    Add-WordTable $Doc $Selection @("STT", "Tài liệu", "Nguồn", "Ghi chú") @(
        @("1", "Mẫu báo cáo phần mềm", "Khoa Công nghệ thông tin", "Cấu trúc tài liệu"),
        @("2", "Mã nguồn QuanLyNhanSuWpf", "Project CNPM nhóm 3", "Căn cứ chức năng thực tế"),
        @("3", "Kết quả build/test", "dotnet build, dotnet test", "13/13 test tự động pass")
    )
    Add-Heading $Doc $Selection "1.5. Mô tả tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu gồm phần giới thiệu, tổng quan dự án, phân tích tính khả thi theo kinh tế, kỹ thuật, pháp lý, vận hành, thời gian và kết luận."
    Add-Heading $Doc $Selection "2. TỔNG QUAN VỀ DỰ ÁN VÀ PHƯƠNG ÁN TRIỂN KHAI" 1
    Add-Heading $Doc $Selection "2.1. Yêu cầu chung của phần mềm" 2
    Add-Para $Doc $Selection "Phần mềm cần hỗ trợ doanh nghiệp quản lý hồ sơ nhân viên, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, bảng lương, thông báo nội bộ, báo cáo và tài khoản người dùng trong một ứng dụng desktop dễ triển khai."
    Add-Heading $Doc $Selection "2.2. Mục tiêu của dự án" 2
    Add-BulletList $Doc $Selection @(
        "Tin học hóa các nghiệp vụ nhân sự cơ bản trong doanh nghiệp vừa và nhỏ.",
        "Xây dựng chương trình desktop có đăng nhập, phân quyền và lưu dữ liệu SQL Server.",
        "Cung cấp dashboard và báo cáo để hỗ trợ quản lý ra quyết định.",
        "Đáp ứng yêu cầu đồ án CNPM: tài liệu, chương trình, test case và gói bàn giao."
    )
    Add-Heading $Doc $Selection "2.3. Những vấn đề cần giải quyết" 2
    Add-BulletList $Doc $Selection @(
        "Dữ liệu nhân sự thường bị phân tán trên Excel hoặc giấy tờ.",
        "Quy trình nghỉ phép, chấm công và lương cần trạng thái xử lý rõ ràng.",
        "Cần phân quyền theo vai trò để nhân viên chỉ xem dữ liệu phù hợp.",
        "Môi trường demo có thể thiếu SQL Server nên cần phương án dự phòng dữ liệu mẫu."
    )
    Add-Heading $Doc $Selection "2.4. Phương án triển khai" 2
    Add-Para $Doc $Selection "Nhóm tự triển khai phần mềm bằng C#/.NET 10 và WPF. Dữ liệu chính lưu trên SQL Server HRManagementDB; ứng dụng tự tạo schema khi kết nối được. Khi demo thiếu SQL Server, ứng dụng có thể dùng chế độ dữ liệu mẫu cục bộ theo cấu hình."
    Add-Heading $Doc $Selection "3. PHÂN TÍCH TÍNH KHẢ THI" 1
    Add-Heading $Doc $Selection "3.1. Khả thi về kinh tế" 2
    Add-Para $Doc $Selection "Dự án sử dụng công cụ miễn phí hoặc phổ biến trong học tập như .NET SDK, Visual Studio/VS Code, SQL Server Express/LocalDB và Git. Chi phí triển khai thấp, phù hợp phạm vi đồ án môn học."
    Add-Heading $Doc $Selection "3.2. Khả thi về kỹ thuật và công nghệ" 2
    Add-WordTable $Doc $Selection @("Hạng mục", "Đánh giá", "Minh chứng") @(
        @("Nền tảng", "Khả thi", "WPF, .NET 10, target net10.0-windows"),
        @("CSDL", "Khả thi", "Microsoft.Data.SqlClient, tự tạo HRManagementDB và bảng HR_*"),
        @("Bảo mật", "Khá", "PBKDF2-SHA256, salt riêng, phân quyền vai trò, audit log"),
        @("Kiểm thử", "Khá", "MSTest 13/13 pass"),
        @("Đóng gói", "Khả thi", "PowerShell publish self-contained win-x64, Inno Setup")
    )
    Add-Heading $Doc $Selection "3.3. Khả thi về pháp lý" 2
    Add-Para $Doc $Selection "Dự án sử dụng thư viện và nền tảng hợp pháp trong môi trường học tập. Khi triển khai thật cần tuân thủ quy định bảo vệ dữ liệu cá nhân, phân quyền truy cập và sao lưu dữ liệu nhân sự."
    Add-Heading $Doc $Selection "3.4. Tính khả thi về hoạt động" 2
    Add-Para $Doc $Selection "Giao diện tiếng Việt, chia theo phân hệ rõ ràng, có dashboard và thao tác dữ liệu trực quan. Các vai trò Admin, Giám đốc, Trưởng phòng và Nhân viên phù hợp cách tổ chức thông thường của doanh nghiệp."
    Add-Heading $Doc $Selection "3.5. Khả thi về thời gian" 2
    Add-Para $Doc $Selection "Với nhóm 3 thành viên, phạm vi chức năng đã được chia theo giai đoạn phân tích, thiết kế, lập trình, kiểm thử và tài liệu. Project hiện build thành công và test tự động pass nên đủ điều kiện hoàn thiện hồ sơ nộp."
    Add-Heading $Doc $Selection "4. KẾT LUẬN VỀ TÍNH KHẢ THI" 1
    Add-Para $Doc $Selection "Dự án $ProjectName khả thi trong phạm vi đồ án Công nghệ phần mềm. Sản phẩm có chương trình desktop chạy được, có CSDL SQL Server, có test tự động, có đóng gói và có bộ tài liệu theo mẫu."
    Add-Heading $Doc $Selection "5. PHỤ LỤC" 1
    Add-BulletList $Doc $Selection @(
        "Solution: QuanLyNhanSuWpf\\QuanLyNhanSuWpf.sln",
        "Gói bàn giao: artifacts\\QuanLyNhanSuWpf-win-x64.zip",
        "Lệnh xác nhận: dotnet build -c Release; dotnet test -c Release --no-build"
    )
}

function Write-Plan($Doc, $Selection) {
    Add-Heading $Doc $Selection "I. MỤC TIÊU, PHẠM VI DỰ ÁN" 1
    Add-Heading $Doc $Selection "1. Mục tiêu" 2
    Add-Para $Doc $Selection "Xây dựng phần mềm quản lý nhân sự desktop trên Windows, hỗ trợ các nghiệp vụ hồ sơ nhân viên, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, bảng lương, thông báo, báo cáo và quản trị tài khoản."
    Add-Heading $Doc $Selection "2. Phạm vi" 2
    Add-BulletList $Doc $Selection @(
        "Phân tích yêu cầu, thiết kế phần mềm và thiết kế CSDL.",
        "Lập trình ứng dụng WPF bằng C#/.NET 10.",
        "Kết nối SQL Server, có dữ liệu mẫu cục bộ cho demo.",
        "Kiểm thử tự động và test case thủ công.",
        "Đóng gói bản chạy Windows và lập tài liệu bàn giao."
    )
    Add-Heading $Doc $Selection "II. THÔNG TIN DỰ ÁN" 1
    Add-BulletList $Doc $Selection @(
        "Khách hàng: Khoa Công nghệ thông tin.",
        "Mã dự án: $ProjectCode.",
        "Tổ chức thực hiện: $Team.",
        "Quản trị/hướng dẫn: $Teacher.",
        "Thời hạn thực hiện: 27/05/2026 đến 01/06/2026."
    )
    Add-WordTable $Doc $Selection @("STT", "Họ và tên", "Tổ chức", "Chức vụ trong đội dự án") @(
        @("1", $Members[0], $Team, "Nhóm trưởng, phân tích yêu cầu, tài liệu"),
        @("2", $Members[1], $Team, "Lập trình, thiết kế CSDL, kiểm thử"),
        @("3", $Members[2], $Team, "Thiết kế giao diện, kiểm thử, báo cáo")
    )
    Add-Heading $Doc $Selection "III. KẾ HOẠCH THỰC HIỆN" 1
    Add-WordTable $Doc $Selection @("Giai đoạn", "Mục tiêu", "Thời gian", "Phân công") @(
        @("Khởi động", "Chọn đề tài, xác định phạm vi và công nghệ", "27/05/2026", "Cả nhóm"),
        @("Phân tích", "Lập khả thi, kế hoạch và đặc tả yêu cầu", "28/05/2026", "$($Members[0]) chủ trì"),
        @("Thiết kế", "Thiết kế kiến trúc WPF, CSDL SQL Server và giao diện", "29/05/2026", "$($Members[1]), $($Members[2])"),
        @("Xây dựng", "Lập trình các phân hệ và kết nối dữ liệu", "30/05/2026", "$($Members[1]) chủ trì"),
        @("Kiểm thử", "Chạy MSTest, kiểm thử thủ công, sửa lỗi", "31/05/2026", "$($Members[2]) chủ trì"),
        @("Bàn giao", "Hoàn thiện tài liệu, gói chạy và trình bày", "01/06/2026", "Cả nhóm")
    )
    Add-Heading $Doc $Selection "1. Giai đoạn khởi động" 2
    Add-Para $Doc $Selection "Xác định đề tài, mục tiêu, công nghệ sử dụng và phân chia vai trò trong nhóm."
    Add-Heading $Doc $Selection "2. Giai đoạn thiết kế và xây dựng chương trình" 2
    Add-Para $Doc $Selection "Thiết kế kiến trúc MVVM đơn giản cho WPF, tạo schema SQL Server, lập trình giao diện và các nghiệp vụ chính."
    Add-Heading $Doc $Selection "3. Giai đoạn kiểm thử" 2
    Add-Para $Doc $Selection "Chạy test tự động bằng MSTest và lập bảng test case thủ công theo từng phân hệ."
    Add-Heading $Doc $Selection "4. Giai đoạn sửa lỗi" 2
    Add-Para $Doc $Selection "Sửa lỗi phát hiện trong quá trình test, kiểm tra lại build Release và chạy lại test."
    Add-Heading $Doc $Selection "5. Giai đoạn xây dựng tài liệu" 2
    Add-Para $Doc $Selection "Hoàn thiện bộ tài liệu theo mẫu: khả thi, kế hoạch, đặc tả yêu cầu, thiết kế phần mềm, thiết kế CSDL, test case."
    Add-Heading $Doc $Selection "6. Giai đoạn bàn giao, triển khai và kết thúc dự án" 2
    Add-BulletList $Doc $Selection @(
        "Ngày bàn giao: 01/06/2026.",
        "Ngày triển khai demo: 01/06/2026.",
        "Ngày kết thúc dự án: 01/06/2026."
    )
}

function Write-Srs($Doc, $Selection) {
    Add-Heading $Doc $Selection "1. GIỚI THIỆU" 1
    Add-Heading $Doc $Selection "1.1. Mục đích tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu đặc tả yêu cầu mô tả đầy đủ các chức năng, actor, quy trình nghiệp vụ và yêu cầu phi chức năng của $ProjectName."
    Add-Heading $Doc $Selection "1.2. Phạm vi tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu là căn cứ để thiết kế phần mềm, thiết kế CSDL, lập trình, kiểm thử và nghiệm thu sản phẩm."
    Add-Heading $Doc $Selection "1.3. Thuật ngữ và các từ viết tắt" 2
    Add-WordTable $Doc $Selection @("Thuật ngữ", "Định nghĩa", "Giải thích") @(
        @("WPF", "Windows Presentation Foundation", "Nền tảng giao diện desktop"),
        @("CSDL", "Cơ sở dữ liệu", "SQL Server HRManagementDB"),
        @("RBAC", "Role-Based Access Control", "Phân quyền theo vai trò"),
        @("MSTest", "Framework kiểm thử", "Dùng cho unit test tự động")
    )
    Add-Heading $Doc $Selection "1.4. Tài liệu tham khảo" 2
    Add-Para $Doc $Selection "Mẫu tài liệu đặc tả yêu cầu, mã nguồn QuanLyNhanSuWpf, kết quả build/test và yêu cầu bài tập Công nghệ phần mềm."
    Add-Heading $Doc $Selection "1.5. Mô tả tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu gồm tổng quan phần mềm, phân tích nghiệp vụ, yêu cầu chức năng, yêu cầu khác và tiêu chuẩn nghiệm thu."
    Add-Heading $Doc $Selection "2. TỔNG QUAN VỀ PHẦN MỀM" 1
    Add-Heading $Doc $Selection "2.1. Yêu cầu chung của phần mềm" 2
    Add-Para $Doc $Selection "Phần mềm quản lý nhân sự cần chạy trên Windows, giao diện tiếng Việt, có đăng nhập, phân quyền, lưu dữ liệu, báo cáo và hỗ trợ các nghiệp vụ nhân sự thiết yếu."
    Add-Heading $Doc $Selection "2.2. Mục tiêu của phần mềm" 2
    Add-BulletList $Doc $Selection @(
        "Tập trung dữ liệu nhân sự vào một hệ thống.",
        "Chuẩn hóa thao tác tuyển dụng, hồ sơ, chấm công, nghỉ phép, đánh giá và lương.",
        "Hỗ trợ lãnh đạo theo dõi dashboard và báo cáo.",
        "Đảm bảo an toàn cơ bản bằng xác thực, phân quyền và audit log."
    )
    Add-Heading $Doc $Selection "2.3. Đối tượng người dùng" 2
    Add-WordTable $Doc $Selection @("Actor", "Mô tả", "Quyền chính") @(
        @("Admin", "Quản trị hệ thống", "Toàn quyền tài khoản, dữ liệu, cấu hình"),
        @("Giám đốc", "Người điều hành", "Xem toàn hệ thống, xử lý nghiệp vụ quản lý"),
        @("Trưởng phòng", "Quản lý bộ phận", "Xem/duyệt dữ liệu trong phạm vi phòng ban"),
        @("Nhân viên", "Người dùng tự phục vụ", "Xem hồ sơ cá nhân, chấm công, nghỉ phép, phiếu lương")
    )
    Add-Heading $Doc $Selection "2.4. Mô hình tổng thể của phần mềm" 2
    Add-Para $Doc $Selection "Người dùng đăng nhập vào ứng dụng WPF. ViewModel điều phối thao tác giao diện và gọi lớp dữ liệu. Dữ liệu được lưu tại SQL Server HRManagementDB; khi không có SQL Server, chế độ fallback cục bộ phục vụ demo."
    Add-Heading $Doc $Selection "3. PHÂN TÍCH QUY TRÌNH NGHIỆP VỤ" 1
    Add-Heading $Doc $Selection "3.1. Quản lý hồ sơ nhân viên" 2
    Add-Heading $Doc $Selection "3.1.1. Sự kiện kích hoạt" 3
    Add-Para $Doc $Selection "Admin, Giám đốc hoặc Trưởng phòng cần thêm, cập nhật, tra cứu hoặc xóa hồ sơ nhân viên."
    Add-Heading $Doc $Selection "3.1.2. Mô hình quy trình nghiệp vụ" 3
    Add-Para $Doc $Selection "Chọn phân hệ Hồ sơ nhân viên -> lọc/chọn nhân viên -> nhập thông tin -> lưu -> hệ thống cập nhật CSDL và làm mới danh sách."
    Add-Heading $Doc $Selection "3.1.3. Mô tả các bước" 3
    Add-NumberList $Doc $Selection @("Người dùng chọn chức năng Tạo mới hoặc Nạp dòng chọn.", "Nhập mã nhân viên, họ tên, ngày sinh, ngày tham gia BHXH, phòng ban, vị trí, ngày vào làm và thông tin cá nhân.", "Hệ thống kiểm tra quyền và ghi dữ liệu vào HR_Employees; tuổi và số năm BHXH được tính tự động từ ngày tháng năm.", "Danh sách và dashboard được cập nhật.")
    Add-Heading $Doc $Selection "3.2. Chấm công và nghỉ phép" 2
    Add-Heading $Doc $Selection "3.2.1. Sự kiện kích hoạt" 3
    Add-Para $Doc $Selection "Nhân viên vào/ra ca hoặc tạo đơn nghỉ phép; quản lý duyệt hoặc từ chối đơn."
    Add-Heading $Doc $Selection "3.2.2. Mô hình quy trình nghiệp vụ" 3
    Add-Para $Doc $Selection "Chấm công ghi CheckIn/CheckOut vào HR_Attendances. Nghỉ phép lưu HR_LeaveRequests với trạng thái chờ duyệt, đã duyệt hoặc từ chối."
    Add-Heading $Doc $Selection "3.2.3. Mô tả các bước" 3
    Add-NumberList $Doc $Selection @("Người dùng chọn nhân viên trong phạm vi.", "Thao tác vào ca/ra ca hoặc tạo đơn nghỉ phép.", "Người có quyền duyệt cập nhật trạng thái.", "Hệ thống cập nhật dashboard và thông báo.")
    Add-Heading $Doc $Selection "3.3. Đánh giá và bảng lương" 2
    Add-Heading $Doc $Selection "3.3.1. Sự kiện kích hoạt" 3
    Add-Para $Doc $Selection "Đến kỳ đánh giá hoặc kỳ tính lương tháng."
    Add-Heading $Doc $Selection "3.3.2. Mô hình quy trình nghiệp vụ" 3
    Add-Para $Doc $Selection "Dữ liệu chấm công, nghỉ phép và hợp đồng được sử dụng để tạo đánh giá và phiếu lương."
    Add-Heading $Doc $Selection "3.3.3. Mô tả các bước" 3
    Add-NumberList $Doc $Selection @("Quản lý ghi điểm và nhận xét đánh giá.", "Người có quyền tính lương theo kỳ.", "Hệ thống lấy lương cơ bản, giờ công, ngày nghỉ đã duyệt và số năm tham gia BHXH.", "Phiếu lương tính phụ cấp cơ bản/thâm niên, khấu trừ BHXH/BHYT/BHTN người lao động và xác nhận trả lương.")
    Add-Heading $Doc $Selection "4. YÊU CẦU CHỨC NĂNG CỦA PHẦN MỀM" 1
    Add-WordTable $Doc $Selection @("Mã yêu cầu", "Mô tả") @(
        @("FR-01", "Đăng nhập bằng tài khoản SQL Server, mật khẩu băm PBKDF2-SHA256."),
        @("FR-02", "Phân quyền theo Admin, Giám đốc, Trưởng phòng, Nhân viên."),
        @("FR-03", "Quản lý hồ sơ nhân viên, phòng ban, vị trí công việc."),
        @("FR-04", "Quản lý tuyển dụng và tiếp nhận ứng viên thành nhân viên."),
        @("FR-05", "Ghi nhận chấm công vào ca, ra ca và điều chỉnh công."),
        @("FR-06", "Tạo, duyệt, từ chối đơn nghỉ phép."),
        @("FR-07", "Ghi nhận đánh giá năng lực theo kỳ quý."),
        @("FR-08", "Tính lương tháng theo ngày công, ngày nghỉ, phụ cấp thâm niên BHXH, khấu trừ bảo hiểm; xem phiếu lương và xác nhận trả lương."),
        @("FR-09", "Tạo, lọc, đánh dấu đã đọc thông báo nội bộ."),
        @("FR-10", "Xuất báo cáo nhân viên, chấm công, nghỉ phép, lương.")
    )
    Add-Heading $Doc $Selection "5. CÁC YÊU CẦU KHÁC" 1
    Add-Heading $Doc $Selection "5.1. Yêu cầu tính dễ sử dụng" 2
    Add-WordTable $Doc $Selection @("Mã yêu cầu", "Mô tả") @(@("NFR-01", "Giao diện tiếng Việt, menu phân hệ rõ ràng, có dashboard và thông báo thao tác."))
    Add-Heading $Doc $Selection "5.2. Yêu cầu về tính ổn định" 2
    Add-WordTable $Doc $Selection @("Mã yêu cầu", "Mô tả") @(@("NFR-02", "Ứng dụng xử lý lỗi kết nối SQL Server và có fallback demo khi được cấu hình."))
    Add-Heading $Doc $Selection "5.3. Yêu cầu về hiệu năng" 2
    Add-WordTable $Doc $Selection @("Mã yêu cầu", "Mô tả") @(@("NFR-03", "Danh sách dùng DataGrid có ảo hóa dòng/cột, đáp ứng tốt dữ liệu vừa trong phạm vi đồ án."))
    Add-Heading $Doc $Selection "5.4. Yêu cầu bảo mật" 2
    Add-WordTable $Doc $Selection @("Mã yêu cầu", "Mô tả") @(@("NFR-04", "Không lưu mật khẩu dạng rõ, có phân quyền theo vai trò và audit log."))
    Add-Heading $Doc $Selection "5.5. Yêu cầu sao lưu và phục hồi" 2
    Add-Para $Doc $Selection "Hệ thống hỗ trợ xuất và phục hồi dữ liệu bằng file .hrmbackup.json."
    Add-Heading $Doc $Selection "5.6. Yêu cầu về tính hỗ trợ" 2
    Add-Para $Doc $Selection "Có README, hướng dẫn cài đặt, hướng dẫn sử dụng và thông báo lỗi dễ hiểu."
    Add-Heading $Doc $Selection "5.7. Yêu cầu về công nghệ" 2
    Add-Para $Doc $Selection "C#/.NET 10, WPF, SQL Server, Microsoft.Data.SqlClient, MSTest, GitHub Actions."
    Add-Heading $Doc $Selection "5.8. Yêu cầu về giao tiếp" 2
    Add-Para $Doc $Selection "Ứng dụng giao tiếp với SQL Server qua connection string cấu hình."
    Add-Heading $Doc $Selection "5.9. Yêu cầu tài liệu người dùng và hỗ trợ trực tuyến" 2
    Add-Para $Doc $Selection "Có tài liệu hướng dẫn cài đặt và hướng dẫn sử dụng đặt riêng trong phần bonus."
    Add-Heading $Doc $Selection "5.10. Yêu cầu pháp lý" 2
    Add-Para $Doc $Selection "Tuân thủ bản quyền công cụ sử dụng và bảo vệ dữ liệu cá nhân khi triển khai thật."
    Add-Heading $Doc $Selection "5.11. Yêu cầu về các tiêu chuẩn áp dụng" 2
    Add-Para $Doc $Selection "Áp dụng quy trình phân tích, thiết kế, kiểm thử trong môn Công nghệ phần mềm; tham chiếu cách tổ chức tài liệu theo mẫu của khoa."
    Add-Heading $Doc $Selection "6. TIÊU CHUẨN NGHIỆM THU PHẦN MỀM" 1
    Add-BulletList $Doc $Selection @("Build Release thành công.", "dotnet test đạt 13/13 test.", "Chạy được các phân hệ chính trong demo.", "Bộ tài liệu và test case hoàn chỉnh theo mẫu.")
}

function Write-Design($Doc, $Selection) {
    Add-Heading $Doc $Selection "1. GIỚI THIỆU" 1
    Add-Heading $Doc $Selection "1.1. Mục đích tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu mô tả thiết kế phần mềm $ProjectName, bao gồm kiến trúc, dữ liệu, phân hệ, giao diện và định hướng kiểm thử/bảo trì."
    Add-Heading $Doc $Selection "1.2. Phạm vi tài liệu" 2
    Add-Para $Doc $Selection "Phạm vi thiết kế tập trung vào ứng dụng WPF desktop, lớp ViewModel, lớp truy cập dữ liệu SQL Server, bảo mật tài khoản và chức năng xuất báo cáo."
    Add-Heading $Doc $Selection "1.3. Thuật ngữ và các từ viết tắt" 2
    Add-WordTable $Doc $Selection @("Thuật ngữ", "Định nghĩa", "Giải thích") @(
        @("MVVM", "Model-View-ViewModel", "Cách tách giao diện và xử lý trạng thái"),
        @("WPF", "Windows Presentation Foundation", "Framework UI desktop"),
        @("BHXH", "Bảo hiểm xã hội", "Thông tin ngày tham gia và số năm tham gia bảo hiểm của nhân viên"),
        @("PK", "Primary Key", "Khóa chính"),
        @("UK", "Unique Key", "Khóa duy nhất")
    )
    Add-Heading $Doc $Selection "1.4. Tài liệu tham khảo" 2
    Add-Para $Doc $Selection "Tài liệu đặc tả yêu cầu, mã nguồn WPF, thiết kế CSDL và kết quả kiểm thử."
    Add-Heading $Doc $Selection "1.5. Mô tả tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu gồm tổng quan, kiến trúc, thiết kế dữ liệu, thiết kế phân hệ và thiết kế giao diện."
    Add-Heading $Doc $Selection "2. TỔNG QUAN VỀ PHẦN MỀM" 1
    Add-Para $Doc $Selection "Phần mềm quản lý nhân sự là ứng dụng desktop Windows dùng cho doanh nghiệp vừa và nhỏ, hỗ trợ quản trị dữ liệu nhân sự xuyên suốt từ tuyển dụng đến bảng lương."
    Add-Heading $Doc $Selection "3. THIẾT KẾ KIẾN TRÚC PHẦN MỀM" 1
    Add-Heading $Doc $Selection "3.1. Mô hình kiến trúc" 2
    Add-Para $Doc $Selection "Kiến trúc chính: WPF View -> ManHinhChinhViewModel -> KhoDuLieuNhanSu/KhoXacThuc -> SQL Server HRManagementDB. Các lớp hạ tầng hỗ trợ cấu hình, băm mật khẩu, xuất Office và backup."
    Add-Heading $Doc $Selection "3.2. Mô tả kiến trúc" 2
    Add-WordTable $Doc $Selection @("Lớp", "Thành phần", "Vai trò") @(
        @("Presentation", "LoginWindow.xaml, MainWindow.xaml, App.xaml", "Giao diện, style, navigation, form và dashboard"),
        @("ViewModel", "ManHinhChinhViewModel.cs", "Command, state, lọc dữ liệu, phân quyền"),
        @("Domain", "MoHinh.cs, QuyTacNghiepVuNhanSu.cs", "Đối tượng và quy tắc nghiệp vụ"),
        @("Data Access", "KhoDuLieuNhanSu.cs, KhoXacThuc.cs", "CRUD, xác thực, schema SQL, audit"),
        @("Infrastructure", "CauHinhUngDung.cs, BaoMatMatKhau.cs, BoXuatOffice.cs", "Cấu hình, bảo mật, xuất báo cáo")
    )
    Add-Heading $Doc $Selection "4. THIẾT KẾ DỮ LIỆU" 1
    Add-Heading $Doc $Selection "4.1. Mô tả dữ liệu" 2
    Add-Para $Doc $Selection "Dữ liệu chính được tổ chức trong các bảng HR_Departments, HR_JobPositions, HR_Employees, HR_Applicants, HR_Attendances, HR_LeaveRequests, HR_Appraisals, HR_Payslips, HR_Contracts, HR_Users và HR_AuditLogs. Hồ sơ nhân viên lưu ngày sinh để hệ thống tự tính tuổi, lưu ngày tham gia BHXH để tự tính số năm bảo hiểm."
    Add-Heading $Doc $Selection "4.2. Từ điển dữ liệu" 2
    Add-WordTable $Doc $Selection @("Đối tượng", "Mô tả") @(
        @("NhanVien", "Hồ sơ nhân sự gồm mã, họ tên, ngày sinh, tuổi tính toán, ngày tham gia BHXH, số năm BHXH, phòng ban, vị trí và trạng thái."),
        @("NghiPhep", "Đơn nghỉ phép gồm nhân viên, loại nghỉ, ngày bắt đầu, ngày kết thúc, số ngày, trạng thái."),
        @("ChamCong", "Bản ghi vào ca, ra ca và số giờ làm."),
        @("PhieuLuong", "Phiếu lương gồm kỳ lương, lương cơ bản, ngày công, phụ cấp, khấu trừ bảo hiểm/nghỉ phép và thực lãnh.")
    )
    Add-Heading $Doc $Selection "5. THIẾT KẾ CÁC THÀNH PHẦN (PHÂN HỆ)" 1
    Add-WordTable $Doc $Selection @("Phân hệ", "Thiết kế xử lý") @(
        @("Đăng nhập", "KhoXacThuc kiểm tra tài khoản SQL Server, xác minh PBKDF2 và tạo phiên đăng nhập."),
        @("Hồ sơ nhân viên", "ViewModel binding DataGrid và form, gọi KhoDuLieuNhanSu để thêm/sửa/xóa."),
        @("Tuyển dụng", "Quản lý ứng viên, giai đoạn tuyển dụng và chuyển ứng viên thành nhân viên."),
        @("Chấm công", "Ghi CheckIn/CheckOut, tính WorkHours, kiểm tra ca chưa đóng."),
        @("Nghỉ phép", "Tạo đơn, tính số ngày, cập nhật trạng thái duyệt/từ chối."),
        @("Bảng lương", "Tính lương theo hợp đồng, ngày công, phụ cấp cơ bản/thâm niên BHXH, ngày nghỉ đã duyệt và khoản BHXH/BHYT/BHTN người lao động."),
        @("Thông báo", "Tạo/lọc/đánh dấu đã đọc thông báo nội bộ, hỗ trợ tệp đính kèm."),
        @("Báo cáo", "Xuất DOCX/XLSX/PDF/PPTX/TXT bằng BoXuatOffice.")
    )
    Add-Heading $Doc $Selection "6. THIẾT KẾ GIAO DIỆN NGƯỜI SỬ DỤNG" 1
    Add-Heading $Doc $Selection "6.1. Mô tả tổng quan" 2
    Add-Para $Doc $Selection "Giao diện desktop sử dụng sidebar phân hệ, dashboard tổng quan, card thống kê, DataGrid danh sách và form chi tiết. Màu chủ đạo xanh, font Segoe UI, phong cách gần Fluent Design."
    Add-Heading $Doc $Selection "6.2. Hình ảnh giao diện" 2
    Add-Para $Doc $Selection "Hình ảnh giao diện được thể hiện trực tiếp khi chạy ứng dụng: màn hình đăng nhập, tổng quan vận hành, danh sách nhân viên, chấm công, nghỉ phép, bảng lương và thông báo."
    Add-Heading $Doc $Selection "6.3. Các đối tượng giao diện và hoạt động đi kèm" 2
    Add-WordTable $Doc $Selection @("Đối tượng", "Hoạt động") @(
        @("RadioButton sidebar", "Chọn phân hệ đang làm việc."),
        @("DataGrid", "Hiển thị danh sách nhân viên, ứng viên, chấm công, nghỉ phép, lương."),
        @("TextBox/ComboBox", "Nhập và chọn dữ liệu trong form."),
        @("Button command", "Tạo mới, lưu, xóa, duyệt, tính lương, xuất báo cáo."),
        @("Thông báo nhanh", "Phản hồi kết quả thao tác cho người dùng.")
    )
    Add-Heading $Doc $Selection "7. PHỤ LỤC" 1
    Add-Para $Doc $Selection "Mã nguồn chính nằm trong thư mục QuanLyNhanSuWpf; kiểm thử trong QuanLyNhanSuWpf.Tests; script đóng gói trong tools/package-release.ps1."
}

function Write-Db($Doc, $Selection) {
    Add-Heading $Doc $Selection "1. GIỚI THIỆU" 1
    Add-Heading $Doc $Selection "1.1. Mục đích tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu mô tả thiết kế CSDL logic và vật lý của hệ thống quản lý nhân sự WPF."
    Add-Heading $Doc $Selection "1.2. Phạm vi tài liệu" 2
    Add-Para $Doc $Selection "Phạm vi gồm database HRManagementDB, các bảng HR_*, khóa chính, quan hệ nghiệp vụ và ghi chú triển khai SQL Server."
    Add-Heading $Doc $Selection "1.3. Thuật ngữ và các từ viết tắt" 2
    Add-WordTable $Doc $Selection @("Thuật ngữ", "Định nghĩa", "Giải thích") @(
        @("PK", "Primary Key", "Khóa chính"),
        @("FK", "Foreign Key", "Khóa ngoại/quan hệ nghiệp vụ"),
        @("UK", "Unique Key", "Khóa duy nhất"),
        @("CSDL", "Cơ sở dữ liệu", "HRManagementDB trên SQL Server")
    )
    Add-Heading $Doc $Selection "1.4. Tài liệu tham khảo" 2
    Add-Para $Doc $Selection "Mã nguồn KhoDuLieuNhanSu.cs, SoDoQuanTriSql.cs, KhoXacThuc.cs và tài liệu đặc tả yêu cầu."
    Add-Heading $Doc $Selection "1.5. Mô tả tài liệu" 2
    Add-Para $Doc $Selection "Tài liệu gồm thiết kế logic, danh sách bảng, mô tả trường chính, file dữ liệu và thiết kế vật lý."
    Add-Heading $Doc $Selection "2. THIẾT KẾ LOGIC CSDL" 1
    Add-Heading $Doc $Selection "2.1. Mô hình quan hệ của CSDL" 2
    Add-WordTable $Doc $Selection @("STT", "Tên bảng", "Alias", "Mô tả") @(
        @("1", "HR_Departments", "DEPT", "Phòng ban"),
        @("2", "HR_JobPositions", "POS", "Vị trí công việc"),
        @("3", "HR_Employees", "EMP", "Hồ sơ nhân viên, ngày sinh và ngày tham gia BHXH"),
        @("4", "HR_Applicants", "APP", "Ứng viên tuyển dụng"),
        @("5", "HR_Attendances", "ATT", "Chấm công"),
        @("6", "HR_LeaveRequests", "LEAVE", "Đơn nghỉ phép"),
        @("7", "HR_Appraisals", "APR", "Đánh giá năng lực"),
        @("8", "HR_Payslips", "PAY", "Phiếu lương"),
        @("9", "HR_Contracts", "CON", "Hợp đồng lao động"),
        @("10", "HR_Users", "USR", "Tài khoản hệ thống"),
        @("11", "HR_AuditLogs", "AUD", "Nhật ký hệ thống")
    )
    $tables = @(
        @("2.2. Bảng HR_Departments", @(@("DepartmentID", "INT IDENTITY", "PK", "Mã phòng ban"), @("Name", "NVARCHAR(150)", "UK nghiệp vụ", "Tên phòng ban"), @("ManagerID", "INT NULL", "FK nghiệp vụ", "Trưởng phòng"))),
        @("2.3. Bảng HR_Employees", @(@("EmployeeID", "INT IDENTITY", "PK", "Mã nhân viên"), @("EmployeeCode", "VARCHAR(20)", "UK nghiệp vụ", "Mã số nhân viên"), @("FullName", "NVARCHAR(150)", "", "Họ tên"), @("BirthDate", "DATE", "", "Ngày sinh, dùng để tính tuổi"), @("SocialInsuranceStartDate", "DATE", "", "Ngày bắt đầu tham gia BHXH"), @("DepartmentID", "INT", "FK", "Phòng ban"), @("PositionID", "INT", "FK", "Vị trí"), @("JoinDate", "DATE", "", "Ngày vào làm"), @("IsActive", "BIT", "", "Trạng thái làm việc"))),
        @("2.4. Bảng HR_Applicants", @(@("ApplicantID", "INT IDENTITY", "PK", "Mã ứng viên"), @("PositionID", "INT", "FK", "Vị trí ứng tuyển"), @("FullName", "NVARCHAR(150)", "", "Họ tên"), @("Email", "NVARCHAR(150)", "", "Email"), @("Stage", "VARCHAR(30)", "", "Giai đoạn tuyển dụng"))),
        @("2.5. Bảng HR_Attendances", @(@("AttendanceID", "INT IDENTITY", "PK", "Mã chấm công"), @("EmployeeID", "INT", "FK", "Nhân viên"), @("CheckInTime", "DATETIME2", "", "Giờ vào"), @("CheckOutTime", "DATETIME2 NULL", "", "Giờ ra"), @("WorkHours", "DECIMAL(10,2)", "", "Số giờ làm"))),
        @("2.6. Bảng HR_LeaveRequests", @(@("LeaveID", "INT IDENTITY", "PK", "Mã đơn nghỉ"), @("EmployeeID", "INT", "FK", "Nhân viên"), @("LeaveType", "NVARCHAR(80)", "", "Loại nghỉ"), @("TotalDays", "DECIMAL(10,2)", "", "Số ngày"), @("Status", "VARCHAR(30)", "", "Trạng thái"))),
        @("2.7. Bảng HR_Payslips", @(@("PayslipID", "INT IDENTITY", "PK", "Mã phiếu lương"), @("EmployeeID", "INT", "FK", "Nhân viên"), @("PayPeriod", "VARCHAR(7)", "", "Kỳ lương"), @("BasicSalary", "DECIMAL(18,2)", "", "Lương cơ bản"), @("WorkDays", "DECIMAL(10,2)", "", "Ngày công quy đổi"), @("TotalAllowances", "DECIMAL(18,2)", "", "Phụ cấp cơ bản/thâm niên BHXH"), @("TotalDeductions", "DECIMAL(18,2)", "", "Khấu trừ bảo hiểm và nghỉ phép"), @("NetSalary", "DECIMAL(18,2)", "", "Thực lãnh"))),
        @("2.8. Bảng HR_Users", @(@("UserID", "INT IDENTITY", "PK", "Mã tài khoản"), @("Username", "NVARCHAR(80)", "UK", "Tên đăng nhập"), @("RoleName", "NVARCHAR(80)", "", "Vai trò"), @("PasswordHash", "NVARCHAR(200)", "", "Hash mật khẩu"), @("PasswordSalt", "NVARCHAR(200)", "", "Salt mật khẩu"))),
        @("2.9. Bảng HR_AuditLogs", @(@("AuditID", "BIGINT IDENTITY", "PK", "Mã nhật ký"), @("ActorUsername", "NVARCHAR(80)", "", "Người thực hiện"), @("ActionName", "NVARCHAR(120)", "", "Hành động"), @("EntityName", "NVARCHAR(120)", "", "Đối tượng"), @("CreatedAt", "DATETIME2", "", "Thời điểm")))
    )
    foreach ($def in $tables) {
        Add-Heading $Doc $Selection $def[0] 2
        Add-WordTable $Doc $Selection @("Tên trường", "Kiểu dữ liệu", "Khóa/Ràng buộc", "Mô tả") $def[1]
    }
    Add-Heading $Doc $Selection "2.10. Constraints" 3
    Add-WordTable $Doc $Selection @("STT", "Tên constraint", "Loại", "Các trường liên quan") @(
        @("1", "PK_HR_Employees", "PK", "EmployeeID"),
        @("2", "UQ_HR_Users_Username", "UK", "Username"),
        @("3", "PK_HR_Payslips", "PK", "PayslipID"),
        @("4", "PK_HR_AuditLogs", "PK", "AuditID")
    )
    Add-Heading $Doc $Selection "2.11. Indexes" 3
    Add-Para $Doc $Selection "Các khóa chính và khóa duy nhất tạo index mặc định. Khi mở rộng hệ thống thật, nên bổ sung index cho EmployeeCode, DepartmentID, PayPeriod, CheckInTime và CreatedAt."
    Add-Heading $Doc $Selection "2.12. Triggers" 3
    Add-Para $Doc $Selection "Phiên bản hiện tại chưa sử dụng trigger; logic nghiệp vụ được xử lý trong tầng ứng dụng WPF."
    Add-Heading $Doc $Selection "3. CÁC FILE DỮ LIỆU" 1
    Add-Para $Doc $Selection "Ứng dụng hỗ trợ file cấu hình appsettings.json và file sao lưu .hrmbackup.json. Báo cáo có thể xuất ra DOCX/XLSX/PDF/PPTX/TXT."
    Add-Heading $Doc $Selection "4. THIẾT KẾ VẬT LÝ CSDL" 1
    Add-Para $Doc $Selection "Hệ quản trị CSDL sử dụng SQL Server LocalDB/Express/Server. Database mặc định là HRManagementDB. Ứng dụng thử kết nối theo HRM_CONNECTION_STRING, .\\SQLEXPRESS, localhost và (localdb)\\MSSQLLocalDB."
    Add-Heading $Doc $Selection "5. PHỤ LỤC" 1
    Add-Para $Doc $Selection "Schema được khởi tạo trong KhoDuLieuNhanSu.cs và SoDoQuanTriSql.cs khi ứng dụng kết nối SQL Server thành công."
}

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0

try {
    $doc = Prepare-Doc $word "Báo cáo nghiên cứu tính khả thi.doc" "BaoCao_NghienCuuTinhKhaThi_QuanLyNhanSuWpf.doc" "BCKT-QLNS-WPF" "Điền báo cáo khả thi theo dự án WPF" $true
    Write-Feasibility $doc $word.Selection
    Finish-Doc $doc

    $doc = Prepare-Doc $word "Kế hoạch thực hiện dự án.doc" "KeHoach_ThucHienDuAn_QuanLyNhanSuWpf.doc" "KHDA-QLNS-WPF" "Điền kế hoạch thực hiện dự án" $false "I. MỤC TIÊU"
    try {
        $tbl = $doc.Tables.Item(2)
        $tbl.Cell(2,1).Range.Text = "1"; $tbl.Cell(2,2).Range.Text = $Members[0]; $tbl.Cell(2,3).Range.Text = $Team; $tbl.Cell(2,4).Range.Text = "Nhóm trưởng, phân tích yêu cầu, tài liệu"
        $tbl.Cell(3,1).Range.Text = "2"; $tbl.Cell(3,2).Range.Text = $Members[1]; $tbl.Cell(3,3).Range.Text = $Team; $tbl.Cell(3,4).Range.Text = "Lập trình, thiết kế CSDL, kiểm thử"
        $tbl.Cell(4,1).Range.Text = "3"; $tbl.Cell(4,2).Range.Text = $Members[2]; $tbl.Cell(4,3).Range.Text = $Team; $tbl.Cell(4,4).Range.Text = "Thiết kế giao diện, kiểm thử, báo cáo"
    } catch {}
    Write-Plan $doc $word.Selection
    Finish-Doc $doc

    $doc = Prepare-Doc $word "Tài liệu đặc tả yêu cầu.doc" "TaiLieu_DacTaYeuCau_QuanLyNhanSuWpf.doc" "SRS-QLNS-WPF" "Điền đặc tả yêu cầu phần mềm" $true
    Write-Srs $doc $word.Selection
    Finish-Doc $doc

    $doc = Prepare-Doc $word "Tài liệu thiết kế phần mềm.doc" "TaiLieu_ThietKePhanMem_QuanLyNhanSuWpf.doc" "TKPM-QLNS-WPF" "Điền thiết kế phần mềm" $true
    Write-Design $doc $word.Selection
    Finish-Doc $doc

    $doc = Prepare-Doc $word "Tài liệu thiết kế CSDL.doc" "TaiLieu_ThietKeCSDL_QuanLyNhanSuWpf.doc" "TKCSDL-QLNS-WPF" "Điền thiết kế CSDL" $true
    Write-Db $doc $word.Selection
    Finish-Doc $doc
}
finally {
    $word.Quit()
}

function Copy-GeneratedBonus {
    $bonusSummary = Join-Path $BonusDir "BaoCao_Bonus_QuanLyNhanSuWpf.docx"
    Copy-Item -LiteralPath (Join-Path $GeneratedDir "BaoCao_GitVaChecklist_QuanLyNhanSuWpf.docx") -Destination $bonusSummary -Force
    Copy-Item -LiteralPath (Join-Path $GeneratedDir "TaiLieu_HuongDanCaiDat_QuanLyNhanSuWpf.docx") -Destination (Join-Path $BonusDir "TaiLieu_HuongDanCaiDat_QuanLyNhanSuWpf.docx") -Force
    Copy-Item -LiteralPath (Join-Path $GeneratedDir "TaiLieu_HuongDanSuDung_QuanLyNhanSuWpf.docx") -Destination (Join-Path $BonusDir "TaiLieu_HuongDanSuDung_QuanLyNhanSuWpf.docx") -Force
}

function Fill-TestcaseWorkbook {
    $template = Join-Path $TemplateDir "Tài liệu testcase.xls"
    $dest = Join-Path $OutDir "TaiLieu_TestCase_QuanLyNhanSuWpf.xls"
    Copy-Item -LiteralPath $template -Destination $dest -Force
    $source = Join-Path $GeneratedDir "TaiLieu_TestCase_QuanLyNhanSuWpf.xlsx"

    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    try {
        $srcWb = $excel.Workbooks.Open($source, $null, $true)
        $dstWb = $excel.Workbooks.Open($dest)

        $dstWb.Worksheets.Item("Trang bìa").Range("A3").Value2 = "$ProjectName`nTÀI LIỆU ĐẶC TẢ TEST CASE"
        $dstWb.Worksheets.Item("Trang bìa").Range("A4").Value2 = "Mã dự án: $ProjectCode"
        $dstWb.Worksheets.Item("Trang bìa").Range("A5").Value2 = "Mã tài liệu: TC-QLNS-WPF"
        $dstWb.Worksheets.Item("Trang bìa").Range("A6").Value2 = "Phiên bản: v1.0"

        $sign = $dstWb.Worksheets.Item("Trang ký")
        $sign.Cells.Item(8,3).Value2 = $Members[2]
        $sign.Cells.Item(9,3).Value2 = "Tester"
        $sign.Cells.Item(11,3).Value2 = $Members[0]
        $sign.Cells.Item(12,3).Value2 = "Nhóm trưởng"
        $sign.Cells.Item(20,3).Value2 = $Teacher
        $sign.Cells.Item(21,3).Value2 = "Giảng viên hướng dẫn"

        $chg = $dstWb.Worksheets.Item("Trang ghi nhận thay đổi")
        $chg.Cells.Item(6,1).Value2 = $Today
        $chg.Cells.Item(6,2).Value2 = "Hoàn thiện test case"
        $chg.Cells.Item(6,3).Value2 = "Điền theo project WPF"
        $chg.Cells.Item(6,4).Value2 = "Mẫu test case"
        $chg.Cells.Item(6,5).Value2 = "0.9"
        $chg.Cells.Item(6,6).Value2 = "Bổ sung test suite các phân hệ nhân sự"
        $chg.Cells.Item(6,7).Value2 = "1.0"

        $intro = $dstWb.Worksheets.Item("Giới thiệu")
        $intro.Cells.Item(4,1).Value2 = "1"
        $intro.Cells.Item(4,2).Value2 = "Tài liệu đặc tả yêu cầu"
        $intro.Cells.Item(4,3).Value2 = "v1.0"
        $intro.Cells.Item(4,4).Value2 = "BaoCaoTheoMauGoc"
        $intro.Cells.Item(16,1).Value2 = "Môi trường test: Windows 10/11, .NET 10, SQL Server Express/LocalDB hoặc chế độ dữ liệu mẫu, ứng dụng WPF QuanLyNhanSuWpf."

        $suiteTemplate = $dstWb.Worksheets.Item("Yêu cầu 1")
        foreach ($ws in @($dstWb.Worksheets)) {
            if ($ws.Name -in @("Yêu cầu 2", "Sheet9", "Sheet10", "Sheet11", "Sheet12", "Yêu cầu n")) {
                try { $ws.Delete() } catch {}
            }
        }

        $suiteNames = @()
        foreach ($ws in @($srcWb.Worksheets)) {
            if ($ws.Name -ne "TongHop") { $suiteNames += $ws.Name }
        }

        $suiteList = $dstWb.Worksheets.Item("Bảng các test suite")
        for ($i = 0; $i -lt $suiteNames.Count; $i++) {
            $suiteList.Cells.Item((4 + $i), 1).Value2 = [string]($i + 1)
            $suiteList.Cells.Item((4 + $i), 2).Value2 = ("TS{0:D2}" -f ($i + 1))
            $suiteList.Cells.Item((4 + $i), 3).Value2 = $suiteNames[$i]
        }

        $summary = $dstWb.Worksheets.Item("Báo cáo tổng hợp")
        $totalPassed = 0; $totalPe = 0; $total = 0

        for ($i = 0; $i -lt $suiteNames.Count; $i++) {
            $srcSheet = $srcWb.Worksheets.Item($suiteNames[$i])
            if ($i -eq 0) {
                $dstSheet = $suiteTemplate
            }
            else {
                $suiteTemplate.Copy($dstWb.Worksheets.Item($dstWb.Worksheets.Count))
                $dstSheet = $dstWb.Worksheets.Item($dstWb.Worksheets.Count)
            }
            $safeName = ("TS{0:D2}_{1}" -f ($i + 1), ($suiteNames[$i] -replace "[\\/\?\*\[\]:]", "")) 
            if ($safeName.Length -gt 31) { $safeName = $safeName.Substring(0,31) }
            $dstSheet.Name = $safeName
            $dstSheet.Range("E2").Value2 = $suiteNames[$i]
            $dstSheet.Range("E3").Value2 = ("TS{0:D2}" -f ($i + 1))

            $lastRow = $srcSheet.UsedRange.Rows.Count
            $caseCount = [Math]::Max(0, $lastRow - 1)
            $p = 0; $pe = 0; $f = 0
            for ($r = 2; $r -le $lastRow; $r++) {
                $dstRow = 9 + $r
                $status = [string]$srcSheet.Cells.Item($r, 9).Text
                $result = "PE"
                if ($status -eq "Pass") { $result = "P"; $p++ } else { $pe++ }
                $dstSheet.Cells.Item($dstRow, 1).Value2 = [string]$srcSheet.Cells.Item($r, 1).Text
                $dstSheet.Cells.Item($dstRow, 2).Value2 = [string]$srcSheet.Cells.Item($r, 3).Text
                $dstSheet.Cells.Item($dstRow, 3).Value2 = "Tiền điều kiện: $($srcSheet.Cells.Item($r, 4).Text)`nBước: $($srcSheet.Cells.Item($r, 5).Text)`nDữ liệu: $($srcSheet.Cells.Item($r, 6).Text)"
                $dstSheet.Cells.Item($dstRow, 4).Value2 = [string]$srcSheet.Cells.Item($r, 7).Text
                $dstSheet.Cells.Item($dstRow, 5).Value2 = if ($result -eq "P") { "Đạt theo automated test" } else { "" }
                $dstSheet.Cells.Item($dstRow, 6).Value2 = $result
                $dstSheet.Cells.Item($dstRow, 7).Value2 = if ($result -eq "P") { $Today } else { "" }
                $dstSheet.Cells.Item($dstRow, 8).Value2 = if ($result -eq "P") { $Members[2] } else { "" }
                $dstSheet.Cells.Item($dstRow, 9).Value2 = [string]$srcSheet.Cells.Item($r, 8).Text
            }
            $dstSheet.Range("E4").Value2 = [string]$p
            $dstSheet.Range("E5").Value2 = [string]$f
            $dstSheet.Range("E6").Value2 = [string]$pe
            $dstSheet.Range("E7").Value2 = "0"
            $dstSheet.Range("E8").Value2 = [string]$caseCount

            $summaryRow = 4 + $i
            $summary.Cells.Item($summaryRow, 1).Value2 = [string]($i + 1)
            $summary.Cells.Item($summaryRow, 2).Value2 = $suiteNames[$i]
            $summary.Cells.Item($summaryRow, 3).Value2 = [string]$p
            $summary.Cells.Item($summaryRow, 4).Value2 = [string]$f
            $summary.Cells.Item($summaryRow, 5).Value2 = [string]$pe
            $summary.Cells.Item($summaryRow, 6).Value2 = "0"
            $summary.Cells.Item($summaryRow, 7).Value2 = [string]$caseCount
            if ($caseCount -gt 0) {
                $summary.Cells.Item($summaryRow, 8).Value2 = [string]([Math]::Round($p * 100 / $caseCount, 2))
                $summary.Cells.Item($summaryRow, 9).Value2 = [string]([Math]::Round($f * 100 / $caseCount, 2))
                $summary.Cells.Item($summaryRow, 10).Value2 = [string]([Math]::Round(($p + $f + $pe) * 100 / $caseCount, 2))
            }
            $totalPassed += $p; $totalPe += $pe; $total += $caseCount
        }

        $dstWb.Save()
        $srcWb.Close($false)
        $dstWb.Close($true)
    }
    finally {
        $excel.Quit()
    }
}

Fill-TestcaseWorkbook
Copy-GeneratedBonus

$zipPath = Join-Path $Root "BaoCaoTheoMauGoc_QuanLyNhanSuWpf.zip"
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
$zipItems = Get-ChildItem -LiteralPath $OutDir -Force | Where-Object { $_.Name -notlike "~*" } | Select-Object -ExpandProperty FullName
Compress-Archive -Path $zipItems -DestinationPath $zipPath -Force

Write-Host "Created: $OutDir"
Write-Host "Created: $zipPath"
