from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Inches, Pt, RGBColor


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "BaoCaoTheoMauGoc" / "KichBan_BaoCao_3Nguoi"


def set_run(run, size=11, bold=False, color=None):
    run.font.name = "Arial"
    run.font.size = Pt(size)
    run.bold = bold
    if color:
        run.font.color.rgb = RGBColor(*color)


def add_title(doc: Document, title: str, subtitle: str):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(title)
    set_run(r, 18, True, (31, 78, 121))

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(subtitle)
    set_run(r, 11, False, (71, 85, 105))
    doc.add_paragraph()


def add_heading(doc: Document, text: str, level=1):
    p = doc.add_paragraph()
    p.style = f"Heading {level}"
    r = p.add_run(text)
    set_run(r, 14 if level == 1 else 12, True, (15, 118, 110) if level == 1 else (37, 99, 235))


def add_para(doc: Document, text: str, bold_prefix: str | None = None):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    if bold_prefix and text.startswith(bold_prefix):
        r = p.add_run(bold_prefix)
        set_run(r, 11, True)
        rest = text[len(bold_prefix) :]
        if rest:
            r = p.add_run(rest)
            set_run(r)
    else:
        r = p.add_run(text)
        set_run(r)
    return p


def add_bullet(doc: Document, text: str):
    p = doc.add_paragraph(style="List Bullet")
    r = p.add_run(text)
    set_run(r)


def add_number(doc: Document, text: str):
    p = doc.add_paragraph(style="List Number")
    r = p.add_run(text)
    set_run(r)


def add_table(doc: Document, headers, rows):
    table = doc.add_table(rows=1, cols=len(headers))
    table.style = "Table Grid"
    for i, header in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = header
        for paragraph in cell.paragraphs:
            for run in paragraph.runs:
                set_run(run, 10, True, (255, 255, 255))
        cell._tc.get_or_add_tcPr().append(
            __import__("docx").oxml.parse_xml(
                r'<w:shd xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" w:fill="1F4E79"/>'
            )
        )

    for row in rows:
        cells = table.add_row().cells
        for i, value in enumerate(row):
            cells[i].text = value
            for paragraph in cells[i].paragraphs:
                for run in paragraph.runs:
                    set_run(run, 10)
    doc.add_paragraph()


def add_common_footer(doc: Document):
    section = doc.sections[0]
    section.top_margin = Inches(0.7)
    section.bottom_margin = Inches(0.7)
    section.left_margin = Inches(0.85)
    section.right_margin = Inches(0.85)
    footer = section.footer.paragraphs[0]
    footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = footer.add_run("QuanLyNhanSuWpf - Kịch bản báo cáo nhóm")
    set_run(r, 9, False, (100, 116, 139))


def new_doc(title: str, subtitle: str) -> Document:
    doc = Document()
    styles = doc.styles
    styles["Normal"].font.name = "Arial"
    styles["Normal"].font.size = Pt(11)
    add_common_footer(doc)
    add_title(doc, title, subtitle)
    return doc


person1_script = [
    "Em xin trình bày phần thứ nhất là phân tích đề tài, tài liệu yêu cầu và thiết kế tổng thể của hệ thống.",
    "Đề tài nhóm em chọn là phần mềm quản lý nhân sự cho doanh nghiệp, xây dựng dưới dạng ứng dụng desktop WPF. Lý do chọn đề tài này là vì nghiệp vụ nhân sự có nhiều chức năng rõ ràng để áp dụng các bước của môn Công nghệ phần mềm, ví dụ như quản lý hồ sơ nhân viên, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, bảng lương, báo cáo và tài khoản người dùng.",
    "Trước khi lập trình, nhóm em lập tài liệu nghiên cứu tính khả thi. Trong tài liệu này, nhóm đánh giá dự án theo các góc độ kỹ thuật, kinh tế, thời gian và vận hành. Về kỹ thuật, dự án khả thi vì sử dụng C# .NET, WPF và SQL Server, đều là công nghệ phổ biến và phù hợp với ứng dụng desktop. Về kinh tế, các công cụ như Visual Studio, .NET SDK, SQL Server Express hoặc LocalDB đều có thể dùng trong phạm vi học tập. Về thời gian, nhóm chia công việc theo từng giai đoạn: khảo sát, phân tích, thiết kế, lập trình, kiểm thử và bàn giao.",
    "Sau đó nhóm lập kế hoạch dự án. Kế hoạch có phân công công việc cho từng thành viên, xác định các đầu việc chính và sản phẩm bàn giao. Các đầu việc gồm nghiên cứu khả thi, đặc tả yêu cầu, thiết kế phần mềm, thiết kế cơ sở dữ liệu, lập trình, kiểm thử và chuẩn bị báo cáo.",
    "Ở tài liệu đặc tả yêu cầu, nhóm xác định các actor chính gồm Admin, Giám đốc, Trưởng phòng và Nhân viên. Mỗi actor có phạm vi sử dụng khác nhau. Admin quản trị tài khoản và toàn bộ hệ thống. Giám đốc xem dữ liệu toàn công ty. Trưởng phòng quản lý dữ liệu trong phạm vi phòng ban. Nhân viên chủ yếu xem và thao tác thông tin cá nhân như chấm công, nghỉ phép, đánh giá và phiếu lương.",
    "Về yêu cầu chức năng, hệ thống có các phân hệ chính: đăng nhập, tổng quan dashboard, quản lý hồ sơ nhân viên, quản lý phòng ban, tuyển dụng ứng viên, chấm công, nghỉ phép, đánh giá, bảng lương, báo cáo và cài đặt tài khoản. Ngoài ra còn có thông báo nội bộ, sao lưu phục hồi dữ liệu và xuất báo cáo.",
    "Về thiết kế phần mềm, nhóm thiết kế ứng dụng theo hướng WPF kết hợp ViewModel. Giao diện được viết bằng XAML, còn dữ liệu và lệnh thao tác nằm trong C#. Các lớp mô hình dữ liệu như NhanVien, PhongBan, NghiPhep, ChamCong, DanhGia, PhieuLuong và TaiKhoanHeThong được đặt trong file MoHinh.cs. ViewModel chính điều phối dữ liệu, bộ lọc, menu và thao tác người dùng.",
    "Về thiết kế cơ sở dữ liệu, hệ thống sử dụng SQL Server với database HRManagementDB. Các bảng nghiệp vụ được đặt theo tiền tố HR, ví dụ HR_Employees, HR_Departments, HR_JobPositions, HR_Attendances, HR_LeaveRequests, HR_Appraisals, HR_Payslips, HR_Users và HR_AuditLogs. Thiết kế này giúp dữ liệu nhân sự, tài khoản và nhật ký hệ thống được tách rõ theo chức năng.",
    "Tóm lại, phần em phụ trách giúp dự án bám đúng quy trình Công nghệ phần mềm: có nghiên cứu khả thi, có kế hoạch, có đặc tả yêu cầu, có thiết kế phần mềm, có thiết kế cơ sở dữ liệu và có cơ sở để nhóm triển khai chương trình.",
]


person2_script = [
    "Em xin trình bày phần thứ hai là thiết kế giao diện và xây dựng các chức năng chính của chương trình.",
    "Về công nghệ giao diện, nhóm em sử dụng WPF trên nền .NET 10. Giao diện được viết bằng XAML, còn phần xử lý dữ liệu và sự kiện được viết bằng C#. Trong file App.xaml, nhóm khai báo các tài nguyên dùng chung như màu nền, màu chữ, kiểu nút, ô nhập liệu, DataGrid và có nạp theme PresentationFramework.Fluent để giao diện theo phong cách Fluent Design.",
    "Đầu tiên là màn hình đăng nhập, nằm trong file LoginWindow.xaml. Màn hình này được chia thành hai vùng. Bên trái là nhận diện hệ thống quản trị nhân sự, bên phải là form đăng nhập gồm tên đăng nhập, mật khẩu, thông báo lỗi và các nút đăng nhập, thoát. Phần xử lý đăng nhập nằm trong LoginWindow.xaml.cs, khi người dùng nhập tài khoản thì chương trình gọi lớp xác thực để kiểm tra.",
    "Màn hình chính nằm trong file MainWindow.xaml. Em thiết kế màn hình chính theo bố cục có thanh điều hướng bên trái và vùng nội dung bên phải. Thanh điều hướng gồm các phân hệ: Tổng quan, Tuyển dụng, Hồ sơ nhân viên, Phòng ban, Chấm công, Nghỉ phép, Đánh giá, Bảng lương, Báo cáo và Cài đặt tài khoản.",
    "Ở trang Tổng quan, giao diện hiển thị các chỉ số nhanh như tổng nhân viên, quân số đang làm, thông báo, chấm công, nghỉ phép và bảng lương. Ngoài ra còn có các biểu đồ dashboard để người dùng nắm tình hình nhanh hơn. Phần biểu đồ được xử lý trong file BieuDoDashboard.cs.",
    "Ở phân hệ Nhân viên, giao diện có bộ lọc, bảng danh sách bằng DataGrid và form nhập liệu để thêm, sửa, lưu hoặc xóa hồ sơ. Các thông tin gồm mã nhân viên, họ tên, phòng ban, vị trí, ngày sinh, ngày vào làm, trạng thái, liên hệ khẩn cấp, tài khoản ngân hàng và căn cước.",
    "Ở phân hệ Phòng ban, người dùng có thể quản lý danh sách phòng ban, thêm phòng ban mới, sửa phòng ban, xóa phòng ban và gán trưởng phòng. Phần này giúp hệ thống có cơ cấu tổ chức rõ ràng để phục vụ phân quyền và thống kê.",
    "Ở phân hệ Ứng viên, hệ thống hỗ trợ nhập ứng viên, chuyển giai đoạn tuyển dụng và chuyển ứng viên thành nhân viên. Khi ứng viên được tiếp nhận, dữ liệu sẽ được cập nhật sang hồ sơ nhân viên.",
    "Ở phân hệ Chấm công, người dùng có thể ghi nhận vào ca, ra ca, điều chỉnh công và xem trạng thái công như đang trong ca, đủ công, thiếu giờ hoặc tăng ca. Phân hệ Nghỉ phép cho phép tạo đơn nghỉ, duyệt hoặc từ chối đơn nghỉ. Các thao tác này ảnh hưởng tới dashboard và tính lương.",
    "Ở phân hệ Đánh giá và Bảng lương, người có quyền có thể tạo đánh giá, chốt đánh giá, tính lương và xác nhận đã trả lương. Bảng lương được trình bày bằng DataGrid với các cột lương cơ bản, phụ cấp, khấu trừ, thực lãnh và trạng thái.",
    "Ở phân hệ Báo cáo, hệ thống hỗ trợ xuất báo cáo hồ sơ nhân viên, báo cáo chấm công, báo cáo nghỉ phép và báo cáo lương. Người dùng có thể chọn định dạng Word, Excel, PDF, PowerPoint hoặc văn bản tùy nhu cầu.",
    "Ngoài ra, hệ thống có trung tâm thông báo nội bộ. Người có quyền có thể tạo thông báo, chọn phân hệ, mức độ, nhập nội dung, đính kèm tệp và gửi. Người dùng có thể lọc thông báo chưa đọc và đánh dấu đã đọc.",
    "Tóm lại, phần giao diện không chỉ để hiển thị đẹp, mà được thiết kế theo đúng luồng nghiệp vụ: đăng nhập, xem tổng quan, quản lý nhân viên, xử lý chấm công, nghỉ phép, đánh giá, bảng lương, báo cáo và thông báo nội bộ.",
]


person3_script = [
    "Em xin trình bày phần thứ ba là bảo mật, phân quyền, kiểm thử, triển khai và bàn giao hệ thống.",
    "Đầu tiên là phần bảo mật mật khẩu. Code nằm trong file BaoMatMatKhau.cs. Nhóm em không lưu mật khẩu trực tiếp dưới dạng văn bản thường, mà sử dụng thuật toán PBKDF2-SHA256 để băm mật khẩu. Mỗi tài khoản có salt riêng được tạo bằng RandomNumberGenerator. Khi đăng nhập, hệ thống băm lại mật khẩu người dùng nhập vào rồi so sánh với hash đã lưu.",
    "Tiếp theo là luồng đăng nhập. Phần này nằm trong file KhoXacThuc.cs, hàm DangNhapAsync. Khi người dùng đăng nhập, chương trình kết nối SQL Server, kiểm tra tài khoản có tồn tại không, tài khoản có bị khóa không, sau đó xác minh mật khẩu. Nếu đăng nhập thành công thì hệ thống tạo phiên đăng nhập. Nếu thất bại thì hiển thị thông báo phù hợp.",
    "Phần tài khoản và nhật ký hệ thống được tạo trong file SoDoQuanTriSql.cs. Bảng HR_Users dùng để lưu thông tin tài khoản, vai trò, hash mật khẩu, salt, trạng thái tài khoản và lần đăng nhập gần nhất. Bảng HR_AuditLogs dùng để lưu nhật ký thao tác. Trong KhoXacThuc.cs có hàm GhiNhatKyAsync để ghi lại các sự kiện như đăng nhập thành công, đăng nhập thất bại hoặc tài khoản bị khóa.",
    "Về phân quyền, phần này nằm chủ yếu trong ManHinhChinhViewModel.cs. Hệ thống chia vai trò gồm Admin, Giám đốc, Trưởng phòng và Nhân viên. Các biến CoQuyen... quyết định vai trò nào được truy cập phân hệ nào. Ví dụ Admin có quyền cài đặt tài khoản, Admin và Giám đốc có quyền xử lý bảng lương, Trưởng phòng thao tác trong phạm vi phòng ban, còn Nhân viên chỉ xem dữ liệu liên quan tới cá nhân.",
    "Phân quyền không chỉ nằm ở menu, mà còn nằm ở từng lệnh thao tác. Ví dụ thêm và lưu nhân viên cần quyền quản lý hồ sơ, duyệt nghỉ phép cần quyền duyệt nghỉ, tính lương cần quyền xử lý bảng lương. Vì vậy người dùng không có quyền thì không thể thao tác sai phạm vi.",
    "Ngoài ra, em phụ trách phần sao lưu và phục hồi dữ liệu. Trong ManHinhChinhViewModel.cs có hàm SaoLuuDuLieu để xuất dữ liệu ra file .hrmbackup.json và hàm PhucHoiDuLieu để đọc lại file backup. Model bản sao dữ liệu nằm trong BanSaoDuLieuNhanSu.cs. Chức năng này giúp bảo vệ dữ liệu khi cần bàn giao hoặc khôi phục.",
    "Về kiểm thử, nhóm em sử dụng MSTest. Các file test nằm trong project QuanLyNhanSuWpf.Tests. BaoMatMatKhauTests kiểm tra việc băm và xác minh mật khẩu. PhanQuyenPhamViTests kiểm tra phạm vi dữ liệu theo vai trò. NghiepVuNhanSuTests kiểm tra các quy tắc nghiệp vụ như tính kỳ đánh giá, tính số ngày nghỉ phép, tính lương và chấm công. CauHinhUngDungTests kiểm tra cấu hình kết nối và mật khẩu khởi tạo. SapXepChucVuTests kiểm tra sắp xếp chức vụ.",
    "Kết quả chạy test hiện tại là 17 trên 17 test đều passed. Điều này chứng minh các phần quan trọng như bảo mật, phân quyền và nghiệp vụ nhân sự đã được kiểm tra tự động.",
    "Về triển khai, nhóm em có script package-release.ps1 trong thư mục tools. Script này chạy kiểm thử, publish ứng dụng bản Windows 64-bit self-contained và nén thành file zip để bàn giao. File chạy bàn giao nằm trong thư mục artifacts với tên QuanLyNhanSuWpf-win-x64.zip.",
    "Về quản lý phiên bản, dự án đã được commit bằng Git và đẩy lên GitHub. Trên GitHub có nhánh codex/quan-ly-nhan-su-wpf để lưu mã nguồn, tài liệu và minh chứng bonus. Nhóm cũng có workflow GitHub Actions để build, test và publish trên môi trường Windows.",
    "Tóm lại, phần em phụ trách giúp hệ thống hoàn thiện ở góc độ vận hành thực tế: đăng nhập an toàn, phân quyền theo vai trò, ghi nhật ký, sao lưu phục hồi, kiểm thử tự động, đóng gói bàn giao và lưu trữ trên GitHub.",
]


def add_script_section(doc: Document, title: str, script: list[str]):
    add_heading(doc, title, 1)
    add_para(doc, "Phần dưới đây có thể đọc nguyên văn khi báo cáo:")
    for para in script:
        add_para(doc, para)


def add_demo_steps(doc: Document, rows):
    add_heading(doc, "Vừa nói vừa chỉ", 1)
    add_table(doc, ["Khi nói tới", "Mở/chỉ vào", "Ý cần nhấn mạnh"], rows)


def add_questions(doc: Document, rows):
    add_heading(doc, "Câu trả lời nhanh nếu thầy hỏi", 1)
    add_table(doc, ["Câu hỏi", "Trả lời ngắn"], rows)


def create_person1():
    doc = new_doc("Kịch bản báo cáo - Người 1", "Phân tích yêu cầu, kế hoạch dự án và thiết kế tổng thể")
    add_script_section(doc, "Lời đọc nguyên văn", person1_script)
    add_demo_steps(
        doc,
        [
            ("Nghiên cứu khả thi", "BaoCao_NghienCuuTinhKhaThi_QuanLyNhanSuWpf.doc", "Dự án khả thi về kỹ thuật, chi phí, thời gian và vận hành."),
            ("Kế hoạch dự án", "KeHoach_ThucHienDuAn_QuanLyNhanSuWpf.doc", "Có phân chia giai đoạn và phân công nhóm."),
            ("Đặc tả yêu cầu", "TaiLieu_DacTaYeuCau_QuanLyNhanSuWpf.doc", "Actor, chức năng, yêu cầu phi chức năng."),
            ("Thiết kế phần mềm", "TaiLieu_ThietKePhanMem_QuanLyNhanSuWpf.doc", "Kiến trúc WPF, lớp dữ liệu, luồng nghiệp vụ."),
            ("Thiết kế CSDL", "TaiLieu_ThietKeCSDL_QuanLyNhanSuWpf.doc", "Database HRManagementDB và các bảng HR_* trong SQL Server."),
            ("Mô hình trong code", "QuanLyNhanSuWpf/MoHinh.cs", "Các entity chính: nhân viên, phòng ban, chấm công, nghỉ phép, lương."),
        ],
    )
    add_questions(
        doc,
        [
            ("Vì sao chọn đề tài này?", "Vì nghiệp vụ nhân sự rõ ràng, có đủ yêu cầu phân tích, thiết kế, lập trình và kiểm thử."),
            ("Dự án khả thi ở đâu?", "Trong tài liệu nghiên cứu tính khả thi, đánh giá kỹ thuật, kinh tế, thời gian và vận hành."),
            ("Thiết kế CSDL thể hiện ở đâu?", "Trong tài liệu thiết kế CSDL và trong code tạo bảng SQL Server."),
        ],
    )
    return doc


def create_person2():
    doc = new_doc("Kịch bản báo cáo - Người 2", "Thiết kế giao diện WPF và xây dựng chức năng nghiệp vụ")
    add_script_section(doc, "Lời đọc nguyên văn", person2_script)
    add_demo_steps(
        doc,
        [
            ("Công nghệ giao diện", "QuanLyNhanSuWpf/App.xaml", "WPF XAML, style chung, theme PresentationFramework.Fluent."),
            ("Màn hình đăng nhập", "QuanLyNhanSuWpf/LoginWindow.xaml", "Bố cục 2 vùng, form tài khoản/mật khẩu, thông báo lỗi."),
            ("Màn hình chính", "QuanLyNhanSuWpf/MainWindow.xaml", "Sidebar phân hệ bên trái, vùng nội dung bên phải."),
            ("Binding và lệnh bấm", "QuanLyNhanSuWpf/ManHinhChinhViewModel.cs", "Dữ liệu, bộ lọc, command và cập nhật giao diện."),
            ("Dashboard", "QuanLyNhanSuWpf/BieuDoDashboard.cs", "Biểu đồ tròn, đường, cột cho tổng quan nhân sự."),
            ("Thông báo", "MainWindow.xaml và ManHinhChinhViewModel.cs", "Tạo/lọc/đánh dấu đã đọc/đính kèm tệp."),
        ],
    )
    add_questions(
        doc,
        [
            ("Em dùng công nghệ gì?", "WPF trên .NET 10, giao diện XAML, xử lý bằng C#, binding qua ViewModel."),
            ("Em thiết kế giao diện thế nào?", "Thiết kế theo phân hệ nghiệp vụ, sidebar điều hướng, DataGrid danh sách, form chi tiết và dashboard."),
            ("Có dùng Fluent UI không?", "Có nạp theme PresentationFramework.Fluent trong App.xaml và tùy biến style theo Fluent Design."),
            ("UI/UX Figma ở đâu?", "Wireframe import Figma nằm trong thư mục uiux, gồm màn hình đăng nhập, tổng quan và quản lý nhân viên."),
        ],
    )
    return doc


def create_person3():
    doc = new_doc("Kịch bản báo cáo - Người 3", "Bảo mật, phân quyền, kiểm thử, triển khai và bàn giao")
    add_script_section(doc, "Lời đọc nguyên văn", person3_script)
    add_demo_steps(
        doc,
        [
            ("Bảo mật mật khẩu", "QuanLyNhanSuWpf/BaoMatMatKhau.cs", "PBKDF2-SHA256, salt riêng, xác minh hash."),
            ("Đăng nhập", "QuanLyNhanSuWpf/KhoXacThuc.cs", "DangNhapAsync kiểm tra tài khoản, mật khẩu, trạng thái."),
            ("Audit log", "QuanLyNhanSuWpf/SoDoQuanTriSql.cs và KhoXacThuc.cs", "Bảng HR_AuditLogs và hàm GhiNhatKyAsync."),
            ("Phân quyền", "QuanLyNhanSuWpf/ManHinhChinhViewModel.cs", "CoQuyen..., LaVaiTro, CoQuyenTruyCap, CoTheXemNhanVien."),
            ("Backup/restore", "ManHinhChinhViewModel.cs và BanSaoDuLieuNhanSu.cs", "Sao lưu .hrmbackup.json và phục hồi dữ liệu."),
            ("Test", "QuanLyNhanSuWpf.Tests", "Bảo mật, phân quyền, nghiệp vụ, cấu hình, chức vụ."),
            ("Đóng gói", "tools/package-release.ps1", "dotnet publish win-x64 và Compress-Archive tạo zip."),
            ("GitHub", "git log và link nhánh codex/quan-ly-nhan-su-wpf", "Có commit và đã đẩy lên GitHub."),
        ],
    )
    add_questions(
        doc,
        [
            ("Mật khẩu có an toàn không?", "Không lưu plain text, dùng PBKDF2-SHA256 và salt riêng."),
            ("Phân quyền nằm ở đâu?", "Trong ManHinhChinhViewModel.cs qua các biến CoQuyen và hàm kiểm tra vai trò."),
            ("Test được những gì?", "Bảo mật mật khẩu, phân quyền, tính lương, nghỉ phép, chấm công, cấu hình và chức vụ."),
            ("Bàn giao chạy thế nào?", "Giải nén QuanLyNhanSuWpf-win-x64.zip, chạy QuanLyNhanSuWpf.exe, đăng nhập admin/Admin@2026!."),
        ],
    )
    return doc


def create_combined():
    doc = new_doc("Kịch bản báo cáo phần mềm cho 3 người", "Đọc nguyên văn khi bảo vệ đồ án QuanLyNhanSuWpf")
    add_heading(doc, "Phân chia phần báo cáo", 1)
    add_table(
        doc,
        ["Người", "Nội dung chính", "Minh chứng"],
        [
            ("Người 1", "Phân tích yêu cầu, kế hoạch, thiết kế phần mềm và CSDL", "Các tài liệu trong BaoCaoTheoMauGoc"),
            ("Người 2", "Thiết kế giao diện WPF và chức năng nghiệp vụ", "LoginWindow.xaml, MainWindow.xaml, App.xaml, ViewModel"),
            ("Người 3", "Bảo mật, phân quyền, kiểm thử, triển khai và GitHub", "BaoMatMatKhau.cs, KhoXacThuc.cs, Tests, package-release.ps1"),
        ],
    )
    add_script_section(doc, "Người 1 - Lời đọc nguyên văn", person1_script)
    add_script_section(doc, "Người 2 - Lời đọc nguyên văn", person2_script)
    add_script_section(doc, "Người 3 - Lời đọc nguyên văn", person3_script)
    add_heading(doc, "Câu chốt chung cho nhóm", 1)
    add_para(
        doc,
        "Tóm lại, nhóm em đã hoàn thành các yêu cầu bắt buộc gồm nghiên cứu khả thi, kế hoạch dự án, đặc tả yêu cầu, thiết kế, test case và chương trình. Phần bonus có Git/GitHub, thiết kế CSDL, hướng dẫn sử dụng, hướng dẫn cài đặt, giao diện theo Fluent Design, UI/UX Figma-ready và notification nội bộ.",
    )
    return doc


def save_doc(doc: Document, name: str):
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    path = OUT_DIR / name
    doc.save(path)
    return path


def main():
    outputs = [
        save_doc(create_person1(), "Nguoi_1_PhanTich_YeuCau_ThietKe.docx"),
        save_doc(create_person2(), "Nguoi_2_GiaoDien_ChucNang.docx"),
        save_doc(create_person3(), "Nguoi_3_BaoMat_KiemThu_TrienKhai.docx"),
        save_doc(create_combined(), "KichBan_BaoCao_TongHop_3Nguoi.docx"),
    ]
    for output in outputs:
        print(output)


if __name__ == "__main__":
    main()
