# Hướng dẫn xem thiết kế trên Visual Studio

Mục này dùng khi thầy hỏi: "Thiết kế giao diện trên Visual Studio ở đâu?"

## Cách mở trong Visual Studio Designer

1. Vào thư mục **Mã nguồn**.
2. Giải nén file **Mã nguồn QuanLyNhanSuWpf.zip**.
3. Mở file `QuanLyNhanSuWpf/QuanLyNhanSuWpf.sln` bằng Visual Studio.
4. Trong **Solution Explorer**, mở project **QuanLyNhanSuWpf**.
5. Mở file `ThietKeGiaoDienWindow.xaml`.
6. Chọn chế độ **Design** hoặc **Split** để xem giao diện.

## Khi báo cáo nói gì?

"Đây là file thiết kế giao diện tổng hợp bằng XAML. Em tạo riêng `ThietKeGiaoDienWindow.xaml` để trình bày bố cục các phân hệ trong Visual Studio Designer. Mỗi mục bên trái tương ứng với một màn hình hoặc một phân hệ của phần mềm như đăng nhập, tổng quan, tuyển dụng, nhân viên, phòng ban, chấm công, nghỉ phép, đánh giá, bảng lương, báo cáo và cài đặt tài khoản."

## Lưu ý

File này dùng để xem thiết kế và trình bày giao diện trong Visual Studio. Luồng chạy thật của phần mềm vẫn bắt đầu từ `LoginWindow.xaml`, sau đó mở sang `MainWindow.xaml`.
