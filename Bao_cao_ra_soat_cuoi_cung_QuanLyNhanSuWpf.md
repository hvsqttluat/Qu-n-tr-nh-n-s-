# BÁO CÁO RÀ SOÁT CUỐI CÙNG PHẦN MỀM QUẢN LÝ NHÂN SỰ

Ngày rà soát: 03/06/2026  
Dự án: QuanLyNhanSuWpf  
Nhóm thực hiện: Nhóm 3  
Nền tảng: Windows Desktop, WPF, .NET 10, SQL Server

## 1. Kết luận tổng quan

Sau khi rà soát mã nguồn, tài liệu bàn giao, cấu trúc cơ sở dữ liệu, bảo mật, kiểm thử và khả năng build, phần mềm hiện có đủ cơ sở để báo cáo trước hội đồng kiểm tra.

Kết quả kiểm chứng kỹ thuật mới nhất:

- Build Release thành công.
- Số cảnh báo: 0.
- Số lỗi build: 0.
- Test tự động: 17/17 passed.
- Thư mục bàn giao đã có tài liệu, mã nguồn, CSDL, UI/UX, hướng dẫn và file chạy nén.

Nhận định cuối: phần mềm không chỉ là bản giao diện minh họa, mà có luồng nghiệp vụ, dữ liệu mẫu, xác thực, phân quyền, kết nối SQL Server, xuất báo cáo và bộ kiểm thử đi kèm.

## 2. Mục tiêu phần mềm

Phần mềm được xây dựng nhằm hỗ trợ doanh nghiệp quản lý nhân sự tập trung trên máy Windows. Các nghiệp vụ chính gồm:

- Quản lý tài khoản và đăng nhập theo vai trò.
- Theo dõi tổng quan nhân sự, quân số, lương, thông báo.
- Quản lý hồ sơ nhân viên.
- Quản lý phòng ban và trưởng phòng.
- Quản lý tuyển dụng, ứng viên, tiếp nhận ứng viên thành nhân viên.
- Ghi nhận chấm công vào ca, ra ca và điều chỉnh công.
- Tạo, duyệt, từ chối đơn nghỉ phép.
- Ghi nhận và chốt đánh giá nhân sự.
- Tính lương theo công, phụ cấp, nghỉ phép và bảo hiểm.
- Xuất báo cáo hành chính ra nhiều định dạng.
- Sao lưu và phục hồi dữ liệu.

## 3. Công nghệ sử dụng

- Ngôn ngữ: C#.
- Giao diện: WPF XAML.
- Framework: .NET 10, target `net10.0-windows`.
- CSDL: SQL Server, SQL Server Express hoặc LocalDB.
- Thư viện SQL: `Microsoft.Data.SqlClient`.
- Kiểm thử: MSTest.
- Đóng gói: `dotnet publish` self-contained win-x64, script PowerShell, cấu hình Inno Setup.
- CI/CD: GitHub Actions workflow build, test, publish trên Windows.

## 4. Cấu trúc dự án

Các khu vực quan trọng:

- `QuanLyNhanSuWpf`: mã nguồn ứng dụng WPF.
- `QuanLyNhanSuWpf.Tests`: bộ test tự động.
- `Bàn giao phần mềm quản lý nhân sự - Nhóm 3`: bộ tài liệu và gói bàn giao.
- `installer`: cấu hình tạo bộ cài Inno Setup.
- `tools`: script đóng gói và script sinh tài liệu.
- `uiux`: wireframe UI/UX dạng SVG và hướng dẫn Figma.
- `.github/workflows`: workflow CI/CD.

Các file mã nguồn lõi:

- `App.xaml`, `App.xaml.cs`: cấu hình ứng dụng.
- `LoginWindow.xaml`, `LoginWindow.xaml.cs`: màn hình đăng nhập.
- `MainWindow.xaml`, `MainWindow.xaml.cs`: giao diện chính.
- `ManHinhChinhViewModel.cs`: điều phối màn hình, command, bộ lọc, phân quyền và nghiệp vụ giao diện.
- `MoHinh.cs`: mô hình dữ liệu dùng trên giao diện.
- `KhoDuLieuNhanSu.cs`: lớp làm việc với SQL Server và dữ liệu mẫu.
- `KhoXacThuc.cs`: xác thực đăng nhập.
- `BaoMatMatKhau.cs`: băm và xác minh mật khẩu.
- `CauHinhUngDung.cs`: cấu hình chuỗi kết nối, mật khẩu khởi tạo, fallback.
- `SoDoQuanTriSql.cs`: tạo bảng tài khoản và audit log.
- `TaiKhoanNhanSuSql.cs`: đồng bộ tài khoản theo nhân sự.
- `QuyTacNghiepVuNhanSu.cs`: quy tắc tính lương, nghỉ phép, kỳ đánh giá, ngày công.
- `BoXuatOffice.cs`: xuất DOCX, XLSX, PDF, PPTX, TXT.
- `BoDuLieuKhoiTao.cs`: dữ liệu mẫu ban đầu.

## 5. Các phân hệ nghiệp vụ

### 5.1. Đăng nhập và phiên làm việc

Màn hình đăng nhập kiểm tra tài khoản, mật khẩu, trạng thái khóa và nguồn dữ liệu. Khi đăng nhập thành công, hệ thống tạo phiên làm việc gồm tên đăng nhập, họ tên và vai trò.

Điểm mạnh:

- Có tài khoản mặc định phục vụ bàn giao.
- Có kiểm tra tài khoản bị khóa.
- Có ghi nhận nhật ký đăng nhập thành công/thất bại.
- Có chế độ dự phòng cục bộ khi được bật bằng cấu hình.

### 5.2. Tổng quan

Dashboard thể hiện các chỉ số:

- Tổng nhân sự.
- Nhân sự đang làm việc.
- Nhân sự tạm nghỉ/nghỉ phép đã duyệt.
- Thông báo chưa đọc.
- Cơ cấu quân số theo kỳ báo cáo.
- Biểu đồ lương và quân số 12 tháng.
- Ứng viên theo vị trí tuyển dụng.
- Trung tâm thông báo.

Giá trị khi demo: đây là màn hình nên mở đầu khi báo cáo, vì nó cho hội đồng thấy phần mềm có dữ liệu tổng hợp, không chỉ nhập/xóa cơ bản.

### 5.3. Hồ sơ nhân viên

Chức năng chính:

- Xem danh sách nhân viên.
- Tìm kiếm theo mã, tên, phòng ban, chức vụ.
- Lọc theo phòng ban.
- Thêm, sửa, xóa hồ sơ theo quyền.
- Quản lý ngày sinh, tuổi, ngày vào làm, ngày tham gia BHXH, liên hệ khẩn cấp, tài khoản ngân hàng, số căn cước.
- Sắp xếp theo thứ bậc chức vụ.

Điểm đáng trình bày:

- Có phân biệt cấp bậc: giám đốc, phó giám đốc, trưởng phòng, phó phòng, quản lý, chuyên viên, nhân viên, công nhân.
- Có tính tuổi và số năm BHXH tự động.

### 5.4. Phòng ban

Chức năng chính:

- Tạo phòng ban.
- Sửa thông tin phòng ban.
- Xóa phòng ban khi không còn nhân viên.
- Gán trưởng phòng.
- Tự cập nhật quan hệ quản lý trong nhân viên.
- Tổng hợp quân số, quỹ lương, nhân sự nổi bật theo phòng ban.

Điểm mạnh: khi gán trưởng phòng, phần mềm không chỉ đổi tên hiển thị mà còn cập nhật quan hệ quản lý trên dữ liệu.

### 5.5. Tuyển dụng và ứng viên

Chức năng chính:

- Thêm ứng viên.
- Chuyển giai đoạn tuyển dụng.
- Chuyển ứng viên thành nhân viên.
- Xuất hợp đồng làm việc.
- Ngăn tiếp nhận trùng người đã là nhân viên.

Luồng demo nên nói:

1. Tạo ứng viên mới.
2. Chuyển qua các giai đoạn tuyển dụng.
3. Tiếp nhận thành nhân viên.
4. Kiểm tra nhân viên mới xuất hiện trong danh sách hồ sơ.

### 5.6. Chấm công

Chức năng chính:

- Ghi nhận vào ca.
- Ghi nhận ra ca.
- Tự tính số giờ làm.
- Quy đổi ngày công theo 8 giờ/ngày.
- Lọc theo phòng ban, trạng thái, ngày.
- Điều chỉnh công theo quyền.

Các trạng thái có ý nghĩa:

- Đang trong ca.
- Thiếu giờ.
- Đủ công.
- Tăng ca.

### 5.7. Nghỉ phép

Chức năng chính:

- Tạo đơn nghỉ.
- Tính số ngày nghỉ bao gồm ngày bắt đầu và ngày kết thúc.
- Duyệt đơn.
- Từ chối đơn.
- Lưu lý do xử lý.
- Lọc theo kỳ và phòng ban.
- Ảnh hưởng tới quân số hiện diện và bảng lương.

Điểm quan trọng: đơn nghỉ đã duyệt được đưa vào tính quân số và khấu trừ khi tính lương.

### 5.8. Đánh giá nhân sự

Chức năng chính:

- Tạo đánh giá theo kỳ.
- Sửa đánh giá.
- Xóa đánh giá.
- Chốt đánh giá.
- Ghi điểm, nhận xét, người đánh giá, trạng thái.
- Dùng dữ liệu đánh giá để xác định cá nhân xuất sắc.

Điểm trình bày: đánh giá không đứng riêng lẻ mà liên kết với dashboard và báo cáo.

### 5.9. Bảng lương

Chức năng chính:

- Tính lương theo nhân viên.
- Lấy lương cơ bản từ hợp đồng hoặc lương vị trí.
- Tính ngày công từ chấm công.
- Tính phụ cấp cơ bản và phụ cấp thâm niên BHXH.
- Khấu trừ nghỉ phép đã duyệt.
- Khấu trừ BHXH/BHYT/BHTN người lao động theo tỷ lệ 10,5%.
- Xem phiếu lương.
- Xác nhận đã chi trả.
- Xuất báo cáo lương.

Công thức tổng quát:

`Thực lãnh = lương theo ngày công + phụ cấp - khấu trừ nghỉ phép - bảo hiểm bắt buộc`

Quy tắc chính trong mã:

- Ngày công chuẩn: 22 ngày/tháng.
- Giờ công chuẩn: 8 giờ/ngày.
- Bảo hiểm bắt buộc người lao động: 10,5%.
- Phụ cấp cơ bản: 5%.
- Phụ cấp thâm niên: 1%/năm, tối đa 5 năm.

### 5.10. Báo cáo

Phần mềm hỗ trợ xuất:

- Báo cáo hồ sơ nhân sự.
- Báo cáo chấm công.
- Báo cáo nghỉ phép.
- Báo cáo lương nhân sự.
- Hợp đồng làm việc.

Định dạng xuất:

- Word `.docx`.
- Excel `.xlsx`.
- PDF `.pdf`.
- PowerPoint `.pptx`.
- Text `.txt`.

Lưu ý kỹ thuật: xuất PDF cần máy có Microsoft Word để chuyển từ DOCX sang PDF.

### 5.11. Cài đặt tài khoản, thông báo, sao lưu

Chức năng tài khoản:

- Xem phiên đăng nhập.
- Xem danh sách tài khoản.
- Đồng bộ tài khoản theo nhân viên.
- Khóa/mở tài khoản.
- Đặt lại mật khẩu.

Chức năng thông báo:

- Tạo thông báo theo phân hệ.
- Lọc thông báo.
- Đánh dấu đã đọc/chưa đọc.
- Đính kèm tệp thông báo.

Chức năng sao lưu:

- Xuất bản sao dữ liệu `.hrmbackup.json`.
- Phục hồi dữ liệu từ bản sao.

## 6. Phân quyền

Các vai trò chính:

- Admin.
- Giám đốc.
- Trưởng phòng.
- Nhân viên.

Phạm vi quyền:

- Admin: toàn quyền, gồm tài khoản, dữ liệu, hồ sơ, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, lương, báo cáo.
- Giám đốc: điều hành toàn hệ thống, không tập trung vào quản trị kỹ thuật tài khoản như Admin.
- Trưởng phòng: quản lý đội nhóm, xử lý tuyển dụng, hồ sơ, chấm công, nghỉ phép, đánh giá và báo cáo theo phạm vi; trưởng phòng nhân sự được xử lý bảng lương.
- Nhân viên: tự phục vụ, xem dữ liệu liên quan, chấm công, nghỉ phép, xem thông báo, đánh giá, phiếu lương.

Điểm kiểm soát đã thấy trong mã:

- Menu được bật/tắt theo quyền.
- Command nghiệp vụ kiểm tra quyền trước khi thao tác.
- ViewModel lọc phạm vi dữ liệu theo vai trò.
- Test tự động có kiểm tra nhân viên, trưởng phòng nhân sự và giám đốc.

## 7. Cơ sở dữ liệu

Tên database mặc định: `HRManagementDB`.

Nguồn SQL Server được thử theo thứ tự cấu hình:

- Biến môi trường hoặc `appsettings.json`: `HRM_CONNECTION_STRING`.
- `.\SQLEXPRESS`.
- `localhost`.
- `(localdb)\MSSQLLocalDB`.

Các bảng chính:

- `HR_Departments`: phòng ban.
- `HR_JobPositions`: vị trí công việc.
- `HR_Employees`: nhân viên.
- `HR_Applicants`: ứng viên.
- `HR_Attendances`: chấm công.
- `HR_LeaveRequests`: nghỉ phép.
- `HR_Appraisals`: đánh giá.
- `HR_Payslips`: phiếu lương.
- `HR_Contracts`: hợp đồng.
- `HR_Users`: tài khoản.
- `HR_AuditLogs`: nhật ký thao tác.

Điểm mạnh:

- Ứng dụng có thể tự tạo database nếu chưa có.
- Ứng dụng có thể tự tạo bảng nghiệp vụ khi chạy lần đầu.
- Có nâng cấp cấu trúc bằng kiểm tra cột còn thiếu.
- Có dữ liệu mẫu để demo ngay.
- Có câu lệnh kiểm tra CSDL trong thư mục bàn giao.

## 8. Dữ liệu mẫu

Dữ liệu mẫu phục vụ demo gồm:

- Ban giám đốc.
- Các trưởng phòng.
- 20 nhân viên văn phòng.
- 200 công nhân sản xuất.
- Phòng ban.
- Vị trí công việc.
- Chấm công theo tháng.
- Nghỉ phép.
- Đánh giá.
- Phiếu lương.
- Ứng viên.
- Thông báo hệ thống.

Giá trị của dữ liệu mẫu là giúp hội đồng nhìn thấy dashboard, bảng lương, chấm công và báo cáo có dữ liệu thật để kiểm thử nhanh.

## 9. Bảo mật

Các điểm đã có:

- Mật khẩu không lưu plaintext.
- Mật khẩu được băm bằng PBKDF2-SHA256.
- Salt riêng cho từng tài khoản.
- Số vòng lặp mặc định: 210.000.
- So sánh hash bằng fixed-time comparison.
- Tài khoản có trạng thái active/inactive.
- Sai mật khẩu nhiều lần sẽ tăng bộ đếm.
- Sau 5 lần sai, tài khoản bị khóa tạm thời 5 giờ.
- Đăng nhập thành công/thất bại được ghi vào `HR_AuditLogs`.

Khuyến nghị khi bàn giao thật:

- Đổi `HRM_INITIAL_PASSWORD` trước lần chạy đầu.
- Không dùng mật khẩu mặc định `Admin@2026!` trong môi trường thật.
- Tắt fallback cục bộ bằng `HRM_ALLOW_LOCAL_FALLBACK=false`.
- Phân quyền SQL Server theo tài khoản nội bộ thay vì dùng quyền quá rộng.

## 10. Kiểm thử

Đã chạy lệnh:

```powershell
dotnet build QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln -c Release
dotnet test QuanLyNhanSuWpf\QuanLyNhanSuWpf.sln -c Release --no-build
```

Kết quả:

- Build succeeded.
- 0 Warning.
- 0 Error.
- Test passed: 17.
- Test failed: 0.
- Test skipped: 0.

Nhóm test hiện có:

- Bảo mật mật khẩu.
- Cấu hình ứng dụng.
- Phân quyền và phạm vi dữ liệu.
- Quy tắc nghiệp vụ nhân sự.
- Tính kỳ đánh giá.
- Tính ngày nghỉ giao với kỳ báo cáo.
- Tính phiếu lương theo công và nghỉ phép.
- Tính tuổi/số năm tròn.
- Quy đổi chấm công.
- Sắp xếp chức vụ.

Đánh giá: test tập trung vào các phần có rủi ro nghiệp vụ và bảo mật, phù hợp để chứng minh phần mềm có kiểm thử tự động.

## 11. Đóng gói và triển khai

Yêu cầu môi trường:

- Windows 10/11.
- SQL Server Express, LocalDB hoặc SQL Server đầy đủ.
- .NET SDK 10 nếu build từ source.
- Nếu dùng bản publish self-contained thì máy chạy không cần cài .NET runtime riêng.

Tài liệu triển khai đã có:

- README hướng dẫn chạy từ source.
- `appsettings.example.json`.
- Hướng dẫn CSDL.
- Script `tools/package-release.ps1`.
- Cấu hình Inno Setup `installer/QuanLyNhanSuWpf.iss`.
- File zip bàn giao ở thư mục gốc: `BanGiao_DoiTac_QuanLyNhanSu_Nhom3.zip`.
- Thư mục bàn giao có mục chương trình phần mềm gọn và mã nguồn nén.

Script đóng gói thực hiện:

- Chạy test.
- Publish self-contained win-x64.
- Copy README.
- Nén bản publish thành zip.

Lưu ý rà soát: trong lần kiểm tra này không chạy lại script package vì script có bước xóa/tạo lại thư mục artifacts. Việc không chạy lại package giúp giữ nguyên gói bàn giao hiện tại. Tình trạng kỹ thuật được xác nhận bằng build Release và test 17/17.

## 12. Tài liệu bàn giao

Bộ bàn giao hiện có các nhóm:

- Báo cáo phần mềm.
- Tài liệu bổ sung.
- Báo cáo chi tiết theo chức năng.
- Thiết kế UI/UX Figma.
- Cơ sở dữ liệu.
- Mã nguồn.
- Chương trình phần mềm gọn.
- Hướng dẫn bàn giao.

Các tài liệu chính phục vụ hội đồng:

- Tài liệu đặc tả yêu cầu.
- Tài liệu thiết kế phần mềm.
- Tài liệu thiết kế CSDL.
- Tài liệu testcase.
- Kế hoạch thực hiện dự án.
- Báo cáo nghiên cứu tính khả thi.
- Tài liệu hướng dẫn sử dụng.
- Tài liệu hướng dẫn cài đặt.
- Báo cáo bonus và checklist.
- Wireframe UI/UX.
- Lệnh kiểm tra CSDL.

## 13. Điểm mạnh để nhấn mạnh khi báo cáo

1. Phần mềm có đầy đủ luồng HR cơ bản: hồ sơ, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, lương, báo cáo.
2. Có CSDL SQL Server thật, không chỉ lưu biến tạm trên giao diện.
3. Có tự tạo database, tự tạo bảng và tự nạp dữ liệu mẫu.
4. Có xác thực tài khoản, phân quyền theo vai trò và audit log.
5. Có quy tắc tính lương rõ ràng, có bảo hiểm, phụ cấp, ngày công và nghỉ phép.
6. Có dashboard trực quan và thông báo nội bộ.
7. Có xuất báo cáo nhiều định dạng.
8. Có test tự động và kết quả mới nhất 17/17 passed.
9. Có tài liệu bàn giao, hướng dẫn cài đặt, hướng dẫn sử dụng, UI/UX và CSDL.
10. Có script đóng gói và CI/CD.

## 14. Rủi ro và lưu ý trước khi trình hội đồng

Các điểm cần nói đúng phạm vi:

- Đây là ứng dụng desktop WPF chạy trên Windows, không phải web app.
- SQL Server cần được cài hoặc có LocalDB/Express.
- PDF export cần Microsoft Word trên máy nếu xuất trực tiếp PDF.
- Mật khẩu mặc định chỉ dùng cho demo, phải đổi khi triển khai thật.
- Figma hiện có wireframe SVG và hướng dẫn import; nếu chưa có link Share chính thức thì không nên nói đã có file Figma online hoàn chỉnh.
- Một số tài liệu/tệp trong working tree đang có thay đổi chưa commit; trước khi nộp cuối nên chốt lại bộ bàn giao và mã nguồn.

Khuyến nghị trước buổi báo cáo:

- Kiểm tra mở được file chạy trong thư mục bàn giao.
- Chuẩn bị SQL Server Express hoặc LocalDB trên máy demo.
- Đặt sẵn `appsettings.json` nếu máy demo không dùng `.\SQLEXPRESS`.
- Đăng nhập thử bằng `admin`, `gd001`, `tp003`, `nv001`.
- Demo theo thứ tự: đăng nhập, tổng quan, hồ sơ, nghỉ phép, chấm công, tính lương, xuất báo cáo, tài khoản.
- Mở sẵn tài liệu CSDL và testcase nếu hội đồng hỏi.

## 15. Kịch bản trình bày gợi ý

### Mở đầu

"Phần mềm của nhóm em là ứng dụng quản lý nhân sự chạy trên Windows bằng WPF và .NET. Mục tiêu là hỗ trợ doanh nghiệp quản lý tập trung hồ sơ nhân viên, phòng ban, tuyển dụng, chấm công, nghỉ phép, đánh giá, bảng lương và báo cáo."

### Nói về kiến trúc

"Ứng dụng chia thành giao diện WPF, ViewModel điều phối nghiệp vụ, lớp dữ liệu làm việc với SQL Server và lớp quy tắc nghiệp vụ. CSDL dùng các bảng tiền tố HR_ như HR_Employees, HR_Departments, HR_Attendances, HR_Payslips, HR_Users và HR_AuditLogs."

### Nói về bảo mật

"Tài khoản được lưu trong SQL Server, mật khẩu không lưu dạng rõ mà được băm bằng PBKDF2-SHA256 với salt riêng. Hệ thống có phân quyền Admin, Giám đốc, Trưởng phòng, Nhân viên; đồng thời ghi audit log khi đăng nhập và thao tác quan trọng."

### Nói về nghiệp vụ

"Luồng nhân sự bắt đầu từ tuyển dụng, sau đó ứng viên có thể được tiếp nhận thành nhân viên. Nhân viên được quản lý theo phòng ban, vị trí, ngày vào làm, BHXH. Dữ liệu chấm công và nghỉ phép được dùng để tính quân số, báo cáo và bảng lương."

### Nói về lương

"Bảng lương được tính dựa trên lương cơ bản, ngày công quy đổi, phụ cấp cơ bản, phụ cấp thâm niên theo năm BHXH, khấu trừ nghỉ phép đã duyệt và bảo hiểm bắt buộc người lao động 10,5%."

### Nói về kiểm thử

"Nhóm đã có bộ test tự động bằng MSTest. Kết quả rà soát cuối cùng là build Release thành công với 0 warning, 0 error và 17/17 test passed."

### Kết luận

"Phần mềm đã có đủ mã nguồn, CSDL, tài liệu, giao diện, dữ liệu mẫu, test và hướng dẫn triển khai. Vì vậy nhóm em đánh giá sản phẩm đủ điều kiện demo và bàn giao trong phạm vi đề tài môn học."

## 16. Checklist cuối trước khi nộp

- [x] Build Release thành công.
- [x] Test tự động 17/17 passed.
- [x] Có README.
- [x] Có mã nguồn.
- [x] Có project test.
- [x] Có cấu hình SQL Server mẫu.
- [x] Có hướng dẫn CSDL.
- [x] Có script kiểm tra CSDL.
- [x] Có tài liệu bàn giao.
- [x] Có tài liệu cài đặt/sử dụng.
- [x] Có wireframe UI/UX.
- [x] Có script đóng gói.
- [x] Có cấu hình installer.
- [x] Có file zip bàn giao.
- [ ] Nên chốt lại working tree/git trước khi nộp cuối.
- [ ] Nên kiểm tra trên máy demo đúng máy sẽ trình bày.
- [ ] Nên đổi mật khẩu khởi tạo nếu demo như môi trường thật.

## 17. Kết luận cuối cùng

Phần mềm quản lý nhân sự của Nhóm 3 đã đạt trạng thái sẵn sàng báo cáo. Sản phẩm có đầy đủ thành phần cần thiết của một ứng dụng quản lý: giao diện, dữ liệu, nghiệp vụ, phân quyền, bảo mật, báo cáo, tài liệu và kiểm thử. Điểm cần chú ý duy nhất trước khi nộp là chốt lại gói bàn giao cuối cùng, kiểm tra máy demo và chuẩn bị câu trả lời về giới hạn triển khai desktop/SQL Server.
