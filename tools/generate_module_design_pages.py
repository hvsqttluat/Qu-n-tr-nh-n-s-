from __future__ import annotations

import html
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "uiux" / "Thiết kế từng phân hệ"

WIDTH = 1366
HEIGHT = 768
SIDEBAR = 250


MODULES = [
    {
        "file": "01_Đăng nhập.svg",
        "title": "Đăng nhập hệ thống",
        "subtitle": "Xác thực người dùng trước khi vào phần mềm",
        "kpis": ["Tài khoản", "Mật khẩu", "Đăng nhập"],
        "table": [],
        "form": ["Tên đăng nhập", "Mật khẩu", "Thông báo lỗi", "Đăng nhập"],
        "notes": ["Màn hình riêng: LoginWindow.xaml", "Kiểm tra qua KhoXacThuc", "Mở MainWindow khi hợp lệ"],
    },
    {
        "file": "02_Tổng quan.svg",
        "title": "Tổng quan nhân sự",
        "subtitle": "Dashboard hiển thị nhanh tình hình vận hành",
        "kpis": ["Tổng nhân viên", "Nghỉ phép", "Chấm công", "Thông báo"],
        "table": ["Thông báo mới", "Ai đang nghỉ", "Báo cáo kỳ này"],
        "form": ["Bộ lọc kỳ", "Biểu đồ quân số", "Tạo thông báo"],
        "notes": ["Grid Tổng quan trong MainWindow.xaml", "Có KPI, biểu đồ, thông báo", "Phục vụ xem nhanh trước khi vào phân hệ"],
    },
    {
        "file": "03_Tuyển dụng.svg",
        "title": "Tuyển dụng / Ứng viên",
        "subtitle": "Quản lý ứng viên theo vị trí và giai đoạn tuyển dụng",
        "kpis": ["Ứng viên", "Đang phỏng vấn", "Đạt", "Chuyển nhân viên"],
        "table": ["Họ tên", "Email", "Vị trí", "Giai đoạn", "Trạng thái"],
        "form": ["Thông tin ứng viên", "Vị trí ứng tuyển", "Chuyển giai đoạn", "Tiếp nhận thành nhân viên"],
        "notes": ["CommandParameter = Ứng viên", "DataGrid DanhSachUngVienView", "Có nút tiếp nhận thành nhân viên"],
    },
    {
        "file": "04_Hồ sơ nhân viên.svg",
        "title": "Hồ sơ nhân viên",
        "subtitle": "Quản lý thông tin nhân viên và dữ liệu nhân sự",
        "kpis": ["Hồ sơ", "Đang làm", "Theo phòng ban", "BHXH"],
        "table": ["Mã NV", "Họ tên", "Phòng ban", "Vị trí", "Trạng thái"],
        "form": ["Mã nhân viên", "Họ tên", "Ngày sinh", "Phòng ban", "Vị trí", "Lưu nhân viên"],
        "notes": ["DataGrid binding DanhSachNhanVienView", "Form binding BieuMauNhanVien", "Ghi xuống HR_Employees"],
    },
    {
        "file": "05_Phòng ban.svg",
        "title": "Cơ cấu phòng ban",
        "subtitle": "Quản lý phòng ban, trưởng phòng và thống kê từng đơn vị",
        "kpis": ["Phòng ban", "Quân số", "Trưởng phòng", "Lương cao nhất"],
        "table": ["Mã phòng", "Tên phòng ban", "Trưởng phòng"],
        "form": ["Tên phòng ban", "Chọn trưởng phòng", "Lưu phòng ban", "Gán trưởng phòng"],
        "notes": ["Grid Phòng ban trong MainWindow.xaml", "Ghi xuống HR_Departments", "Có tổng hợp quân số theo phòng"],
    },
    {
        "file": "06_Chấm công.svg",
        "title": "Chấm công",
        "subtitle": "Ghi nhận vào ca, ra ca, tổng giờ và trạng thái công",
        "kpis": ["Bản ghi", "Tổng giờ", "Ngày công", "Đủ công"],
        "table": ["Ngày", "Nhân viên", "Ca", "Số giờ", "Trạng thái"],
        "form": ["Chọn nhân viên", "Vào ca", "Ra ca", "Điều chỉnh công", "Bộ lọc ngày"],
        "notes": ["DataGrid DanhSachChamCongView", "Ghi xuống HR_Attendances", "Có lọc theo phòng ban và trạng thái"],
    },
    {
        "file": "07_Nghỉ phép.svg",
        "title": "Nghỉ phép",
        "subtitle": "Tạo đơn nghỉ, duyệt hoặc từ chối đơn nghỉ phép",
        "kpis": ["Đơn nghỉ", "Chờ duyệt", "Đã duyệt", "Từ chối"],
        "table": ["Nhân viên", "Loại nghỉ", "Từ ngày", "Đến ngày", "Trạng thái"],
        "form": ["Chọn nhân viên", "Loại nghỉ", "Từ ngày", "Đến ngày", "Gửi đơn nghỉ"],
        "notes": ["Grid Nghỉ phép", "Ghi xuống HR_LeaveRequests", "Có quyền duyệt theo vai trò"],
    },
    {
        "file": "08_Đánh giá.svg",
        "title": "Đánh giá năng lực",
        "subtitle": "Ghi nhận điểm, nhận xét và chốt kết quả đánh giá",
        "kpis": ["Đánh giá", "Điểm TB", "Hoàn tất", "Xuất sắc"],
        "table": ["Nhân viên", "Người đánh giá", "Kỳ", "Điểm", "Trạng thái"],
        "form": ["Nhân viên", "Người đánh giá", "Kỳ đánh giá", "Điểm", "Nhận xét", "Chốt kết quả"],
        "notes": ["DataGrid DanhSachDanhGiaView", "Ghi xuống HR_Appraisals", "Dùng để phục vụ dashboard"],
    },
    {
        "file": "09_Bảng lương.svg",
        "title": "Bảng lương",
        "subtitle": "Tính lương, phụ cấp, khấu trừ và trạng thái trả lương",
        "kpis": ["Phiếu lương", "Chờ trả", "Lương cao", "Thực lãnh"],
        "table": ["Nhân viên", "Kỳ lương", "Lương cơ bản", "Khấu trừ", "Thực lãnh"],
        "form": ["Chọn nhân viên", "Tính lương", "Xem phiếu", "Xác nhận trả", "Xuất báo cáo lương"],
        "notes": ["Ghi xuống HR_Payslips", "Có tính phụ cấp và khấu trừ", "Có báo cáo lương"],
    },
    {
        "file": "10_Báo cáo.svg",
        "title": "Báo cáo nhân sự",
        "subtitle": "Xuất báo cáo hồ sơ, chấm công, nghỉ phép và lương",
        "kpis": ["Hồ sơ", "Chấm công", "Nghỉ phép", "Lương"],
        "table": ["Loại báo cáo", "Kỳ", "Bộ lọc", "Định dạng", "Trạng thái"],
        "form": ["Chọn kỳ báo cáo", "Xuất hồ sơ", "Xuất chấm công", "Xuất nghỉ phép", "Xuất lương"],
        "notes": ["Nút xuất gọi BoXuatOffice", "Hỗ trợ DOCX/XLSX/PDF/PPTX/TXT", "Dữ liệu lấy từ ViewModel"],
    },
    {
        "file": "11_Cài đặt tài khoản.svg",
        "title": "Cài đặt tài khoản",
        "subtitle": "Đồng bộ tài khoản, khóa/mở khóa và đặt lại mật khẩu",
        "kpis": ["Tài khoản", "Vai trò", "Đang hoạt động", "Tạm khóa"],
        "table": ["Tài khoản", "Họ tên", "Vai trò", "Trạng thái", "Đăng nhập gần nhất"],
        "form": ["Đồng bộ tài khoản", "Khóa/mở tài khoản", "Đặt lại mật khẩu", "Sao lưu", "Phục hồi"],
        "notes": ["Ghi xuống HR_Users", "Nhật ký ở HR_AuditLogs", "Có phân quyền theo vai trò"],
    },
    {
        "file": "12_Thông báo nội bộ.svg",
        "title": "Thông báo nội bộ",
        "subtitle": "Tạo, lọc, đọc và đính kèm thông báo theo phân hệ",
        "kpis": ["Thông báo", "Chưa đọc", "Khẩn cấp", "Có tệp"],
        "table": ["Tiêu đề", "Phân hệ", "Mức độ", "Thời gian", "Đã đọc"],
        "form": ["Tiêu đề", "Nội dung", "Phân hệ", "Mức độ", "Đính kèm", "Gửi thông báo"],
        "notes": ["Nằm trong MainWindow.xaml", "Có form tạo thông báo", "Có toast thông báo nhanh"],
    },
]

MENU = [
    "Tổng quan",
    "Tuyển dụng",
    "Hồ sơ nhân viên",
    "Cơ cấu phòng ban",
    "Chấm công",
    "Nghỉ phép",
    "Đánh giá",
    "Bảng lương",
    "Báo cáo",
    "Cài đặt tài khoản",
]


def text(x: int, y: int, content: str, size: int = 18, color: str = "#172033", weight: str = "400") -> str:
    return (
        f'<text x="{x}" y="{y}" font-family="Segoe UI, Arial" font-size="{size}" '
        f'font-weight="{weight}" fill="{color}">{html.escape(content)}</text>'
    )


def rect(x: int, y: int, w: int, h: int, fill: str, stroke: str = "none", radius: int = 8) -> str:
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{radius}" fill="{fill}" stroke="{stroke}"/>'


def pill(x: int, y: int, label: str, fill: str = "#E8F5F3", color: str = "#0F766E") -> str:
    return rect(x, y, 150, 34, fill, "#B7DCD7", 17) + text(x + 16, y + 23, label, 13, color, "600")


def draw_sidebar(active: str) -> str:
    parts = [rect(0, 0, SIDEBAR, HEIGHT, "#111827", "none", 0)]
    parts.append(text(28, 44, "QL Nhân sự", 23, "#FFFFFF", "700"))
    parts.append(text(28, 70, "Nhóm 3 - WPF", 13, "#9CA3AF", "500"))
    y = 112
    for item in MENU:
        is_active = active in item or item in active
        fill = "#E8F5F3" if is_active else "#111827"
        color = "#0F766E" if is_active else "#D1D5DB"
        parts.append(rect(20, y - 24, 210, 40, fill, "none", 8))
        parts.append(text(38, y, item, 14, color, "600" if is_active else "500"))
        y += 48
    return "\n".join(parts)


def draw_module(module: dict, offset_y: int = 0, include_frame_label: bool = False) -> str:
    title = module["title"]
    active = title
    parts = [f'<g transform="translate(0,{offset_y})">']
    parts.append(rect(0, 0, WIDTH, HEIGHT, "#EEF2F6", "none", 0))

    if title == "Đăng nhập hệ thống":
        parts.append(rect(0, 0, WIDTH, HEIGHT, "#EEF2F6", "none", 0))
        parts.append(rect(110, 90, 520, 560, "#1F6F78", "none", 18))
        parts.append(text(155, 170, "Human Resource", 34, "#FFFFFF", "700"))
        parts.append(text(155, 215, "Quản lý nhân sự WPF", 22, "#DFF7F3", "600"))
        parts.append(text(155, 278, "Dashboard, phân quyền, CSDL SQL Server", 18, "#EAF7F6", "500"))
        parts.append(text(155, 312, "và báo cáo nhân sự trong một ứng dụng desktop.", 18, "#EAF7F6", "500"))
        parts.append(rect(720, 120, 420, 500, "#FFFFFF", "#D8E0EA", 14))
        parts.append(text(770, 190, "Đăng nhập", 30, "#172033", "700"))
        parts.append(text(770, 225, "Nhập tài khoản để vào hệ thống", 15, "#64748B", "500"))
        y = 285
        for label in ["Tên đăng nhập", "Mật khẩu"]:
            parts.append(text(770, y, label, 14, "#475569", "600"))
            parts.append(rect(770, y + 12, 310, 44, "#F8FAFC", "#CBD5E1", 6))
            y += 86
        parts.append(rect(770, 480, 150, 44, "#2563EB", "none", 8))
        parts.append(text(803, 508, "Đăng nhập", 15, "#FFFFFF", "700"))
        parts.append(rect(935, 480, 105, 44, "#FFFFFF", "#CBD5E1", 8))
        parts.append(text(968, 508, "Thoát", 15, "#334155", "600"))
        parts.append(text(770, 568, "Code: LoginWindow.xaml + KhoXacThuc.cs", 13, "#64748B", "500"))
    else:
        parts.append(draw_sidebar(active))
        parts.append(text(285, 48, title, 30, "#172033", "700"))
        parts.append(text(285, 78, module["subtitle"], 15, "#64748B", "500"))
        parts.append(pill(1060, 36, "WPF XAML"))
        parts.append(pill(1180, 36, "Binding", "#EEF2FF", "#2563EB"))

        x = 285
        for idx, kpi in enumerate(module["kpis"]):
            parts.append(rect(x + idx * 250, 115, 220, 92, "#FFFFFF", "#D8E0EA", 10))
            parts.append(text(x + idx * 250 + 22, 148, kpi, 14, "#64748B", "600"))
            parts.append(text(x + idx * 250 + 22, 184, str((idx + 2) * 8), 30, ["#172033", "#0F766E", "#B45309", "#2563EB"][idx % 4], "700"))

        parts.append(rect(285, 235, 650, 410, "#FFFFFF", "#D8E0EA", 10))
        parts.append(text(312, 272, "Danh sách dữ liệu", 19, "#172033", "700"))
        headers = module["table"] or ["Trường", "Giá trị", "Ghi chú"]
        col_w = 590 // len(headers)
        header_y = 305
        for i, h in enumerate(headers):
            parts.append(rect(312 + i * col_w, header_y, col_w - 4, 36, "#1F6F78", "none", 4))
            parts.append(text(322 + i * col_w, header_y + 24, h[:18], 12, "#FFFFFF", "700"))
        for r in range(5):
            y = 352 + r * 52
            parts.append(rect(312, y, 590, 44, "#F8FAFC" if r % 2 == 0 else "#FFFFFF", "#E2E8F0", 4))
            for i in range(len(headers)):
                parts.append(text(322 + i * col_w, y + 28, f"Dữ liệu {r + 1}", 12, "#475569", "500"))

        parts.append(rect(970, 235, 305, 410, "#FFFFFF", "#D8E0EA", 10))
        parts.append(text(995, 272, "Form thao tác", 19, "#172033", "700"))
        y = 305
        for field in module["form"][:6]:
            parts.append(text(995, y, field, 13, "#475569", "600"))
            parts.append(rect(995, y + 10, 240, 34, "#F8FAFC", "#CBD5E1", 5))
            y += 58
        parts.append(rect(995, 590, 120, 38, "#2563EB", "none", 8))
        parts.append(text(1024, 614, "Lưu", 14, "#FFFFFF", "700"))
        parts.append(rect(1126, 590, 110, 38, "#FFFFFF", "#CBD5E1", 8))
        parts.append(text(1154, 614, "Hủy", 14, "#334155", "600"))

        parts.append(rect(285, 670, 990, 50, "#FFFFFF", "#D8E0EA", 10))
        note_text = " | ".join(module["notes"])
        parts.append(text(310, 701, note_text[:150], 13, "#475569", "600"))

    if include_frame_label:
        parts.append(text(24, 24, title, 16, "#94A3B8", "700"))
    parts.append("</g>")
    return "\n".join(parts)


def svg_document(body: str, width: int, height: int) -> str:
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" '
        f'viewBox="0 0 {width} {height}">\n'
        f"{body}\n</svg>\n"
    )


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for module in MODULES:
        path = OUT_DIR / module["file"]
        path.write_text(svg_document(draw_module(module), WIDTH, HEIGHT), encoding="utf-8")

    combined_parts = []
    for index, module in enumerate(MODULES):
        combined_parts.append(draw_module(module, offset_y=index * (HEIGHT + 80), include_frame_label=True))
    combined = svg_document("\n".join(combined_parts), WIDTH, len(MODULES) * (HEIGHT + 80))
    (OUT_DIR / "Bộ thiết kế từng phân hệ.svg").write_text(combined, encoding="utf-8")

    readme = """# Thiết kế từng phân hệ

Thư mục này dùng khi báo cáo phần thiết kế giao diện. Mỗi phân hệ có một file SVG riêng để mở nhanh hoặc import vào Figma.

## Cách dùng khi đi thi

1. Mở `Bộ thiết kế từng phân hệ.svg` nếu muốn xem toàn bộ các màn hình trên một canvas.
2. Mở từng file SVG riêng nếu thầy hỏi cụ thể một phân hệ.
3. Khi giải thích code, đối chiếu với `MainWindow.xaml` vì các phân hệ trong app được hiện thực bằng các vùng `Grid`.

## Các trang có trong bộ thiết kế

- Đăng nhập
- Tổng quan
- Tuyển dụng / Ứng viên
- Hồ sơ nhân viên
- Cơ cấu phòng ban
- Chấm công
- Nghỉ phép
- Đánh giá năng lực
- Bảng lương
- Báo cáo nhân sự
- Cài đặt tài khoản
- Thông báo nội bộ

## Câu nói khi báo cáo

\"Ban đầu nhóm em phác thảo UI/UX theo từng phân hệ. Sau đó khi hiện thực bằng WPF, các thiết kế này được đưa vào `MainWindow.xaml` dưới dạng các vùng `Grid`; menu bên trái điều khiển phân hệ đang hiển thị thông qua biến `MucDangChon`.\"
"""
    (OUT_DIR / "Hướng dẫn thiết kế từng phân hệ.md").write_text(readme, encoding="utf-8")


if __name__ == "__main__":
    main()

