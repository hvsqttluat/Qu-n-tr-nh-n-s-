# Hướng dẫn cơ sở dữ liệu

## Cơ sở dữ liệu của phần mềm

- Tên database: `HRManagementDB`
- Hệ quản trị: SQL Server, SQL Server Express hoặc LocalDB
- Chuỗi kết nối mẫu nằm trong file `appsettings.example.json`

## Cách chạy với SQL Server

1. Cài SQL Server Express hoặc LocalDB.
2. Vào thư mục **Chương trình chạy thử** và giải nén file phần mềm.
3. Copy `appsettings.example.json` thành `appsettings.json`.
4. Đặt `appsettings.json` cùng thư mục với `QuanLyNhanSuWpf.exe`.
5. Nếu máy không dùng `.\\SQLEXPRESS`, sửa `HRM_CONNECTION_STRING` cho đúng tên SQL Server.
6. Chạy `QuanLyNhanSuWpf.exe`.

Lần chạy đầu, ứng dụng sẽ tự tạo database `HRManagementDB`, tạo các bảng `HR_*` và nạp dữ liệu mẫu.

Lưu ý: các bảng chính của phần mềm đều có tiền tố `HR_`. Khi kiểm tra trong SQL Server Management Studio, nên xem `dbo.HR_Employees`, `dbo.HR_Departments`, `dbo.HR_Attendances`... Không dùng bảng `dbo.Employees` nếu có, vì đó có thể là bảng cũ/khác và không phải bảng chính của phần mềm này.

## Các bảng chính

- `HR_Departments`
- `HR_JobPositions`
- `HR_Employees`
- `HR_Applicants`
- `HR_Attendances`
- `HR_LeaveRequests`
- `HR_Appraisals`
- `HR_Payslips`
- `HR_Contracts`
- `HR_Users`
- `HR_AuditLogs`

## Lệnh kiểm tra nhanh

Mở file `Lệnh xem dữ liệu đúng của phần mềm.sql` trong SSMS để xem số lượng dữ liệu từng bảng và xem danh sách nhân viên đúng từ `dbo.HR_Employees`.

## Có cần file `.bak` không?

Không bắt buộc, vì chương trình có cơ chế tự tạo cơ sở dữ liệu khi kết nối SQL Server. Nếu thầy yêu cầu file backup, sau khi chạy phần mềm và có database `HRManagementDB`, mở SQL Server Management Studio rồi backup database đó ra file `.bak`.
