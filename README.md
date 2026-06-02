# QuanLyNhanSuWpf

Phần mềm quản lý nhân sự WPF cho doanh nghiệp, chạy trên Windows với .NET 10. Ứng dụng có các phân hệ tổng quan, nhân viên, phòng ban, tuyển dụng, chấm công, nghỉ phép, chi phí, đánh giá, bảng lương, kế toán và quản trị tài khoản.

## Nâng cấp enterprise

- Nền tảng: `net10.0-windows`.
- Xác thực: tài khoản lưu trong SQL Server, mật khẩu băm PBKDF2-SHA256, salt riêng từng tài khoản.
- Phân quyền: vai trò Admin, Giám đốc, Trưởng phòng, Nhân viên lấy từ bảng `HR_Users`.
- Audit log: đăng nhập và thao tác nghiệp vụ ghi vào bảng `HR_AuditLogs`.
- Cấu hình: ưu tiên biến môi trường, hỗ trợ `appsettings.json`.
- Sao lưu/phục hồi: xuất và nhập file `.hrmbackup.json` trong màn hình Cài đặt tài khoản.
- Báo cáo: xuất mẫu hành chính ra Word `.docx`, Excel `.xlsx`, PDF `.pdf`, PowerPoint `.pptx` hoặc văn bản `.txt`.
- Dữ liệu mẫu: khi kết nối SQL Server, hệ thống tự tạo dữ liệu cho nhân viên, ngày sinh/tuổi, năm tham gia BHXH, phòng ban, vị trí, ứng viên, chấm công, nghỉ phép, đánh giá, hợp đồng, bảng lương, tài khoản và audit log.
- Quân số vận hành: dashboard và báo cáo tự trừ nhân sự có đơn nghỉ đã duyệt giao với ngày/kỳ báo cáo; duyệt hoặc từ chối nghỉ phép sẽ làm mới quân số và tính lại phiếu lương liên quan.
- Tính lương: lương thực lãnh = lương theo ngày công + phụ cấp cơ bản/thâm niên BHXH - nghỉ phép đã duyệt - BHXH/BHYT/BHTN người lao động 10,5%.
- Kiểm thử: MSTest cho bảo mật mật khẩu và cấu hình kết nối.
- CI/CD: workflow GitHub Actions build, test, publish trên Windows.
- Đóng gói: script `tools/package-release.ps1` tạo bản self-contained win-x64 và file zip bàn giao.

## Yêu cầu môi trường

- Windows 10/11.
- .NET SDK 10 nếu build từ source.
- SQL Server LocalDB, SQL Server Express hoặc SQL Server đầy đủ.

## Cấu hình triển khai

Có thể cấu hình bằng biến môi trường hoặc tạo `appsettings.json` cạnh file `.exe`.

```json
{
  "HRM_CONNECTION_STRING": "Server=.\\SQLEXPRESS;Database=HRManagementDB;Trusted_Connection=True;TrustServerCertificate=True;",
  "HRM_INITIAL_PASSWORD": "Admin@2026!",
  "HRM_ALLOW_LOCAL_FALLBACK": "false"
}
```

Ý nghĩa:

- `HRM_CONNECTION_STRING`: chuỗi kết nối SQL Server chính.
- `HRM_INITIAL_PASSWORD`: mật khẩu khởi tạo cho các tài khoản mặc định và reset mật khẩu.
- `HRM_ALLOW_LOCAL_FALLBACK`: chỉ bật `true` khi cần demo không có SQL Server.

Ứng dụng tự thử kết nối theo thứ tự: `HRM_CONNECTION_STRING`, `(localdb)\MSSQLLocalDB`, `.\SQLEXPRESS`, `localhost`. Khi kết nối được SQL Server, ứng dụng tự tạo database `HRManagementDB`, các bảng nghiệp vụ, bảng tài khoản và bảng audit.

## Tài khoản khởi tạo

| Tên đăng nhập | Vai trò |
| --- | --- |
| `admin` | Admin |
| `gd001` | Giám đốc |
| `tp001` đến `tp005` | Trưởng phòng |
| `nv001` đến `nv020` | Nhân viên văn phòng |
| `cn001` đến `cn200` | Công nhân sản xuất |

Mỗi nhân viên có tài khoản riêng theo mã nhân viên viết thường. Mật khẩu ban đầu lấy từ `HRM_INITIAL_PASSWORD`, mặc định là `Admin@2026!`. Khi bàn giao thật, nên đặt giá trị này thành mật khẩu riêng của doanh nghiệp trước lần chạy đầu tiên.

## Chạy từ source

```powershell
dotnet restore .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln
dotnet build .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln
dotnet test .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln
dotnet run --project .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.csproj
```

## Đóng gói bàn giao

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

Kết quả:

- Thư mục chạy: `artifacts\QuanLyNhanSuWpf`
- File nén bàn giao: `artifacts\QuanLyNhanSuWpf-win-x64.zip`

Nếu cần trình cài đặt, publish trước bằng script trên rồi biên dịch file `installer\QuanLyNhanSuWpf.iss` bằng Inno Setup.

## Kiểm tra đã thực hiện

```powershell
dotnet build .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln -c Release
dotnet test .\QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln
powershell -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

Kết quả hiện tại: build sạch, test 13/13 passed, publish self-contained win-x64 thành công.
