import { User, Department, Position, Employee, LeaveRequest, AttendanceRecord, Payroll, Notification, AuditLog, Contract } from './types';

export const initialUsers: User[] = [
  { id: 1, username: 'admin', fullName: 'Nguyễn Văn Admin', email: 'admin@company.com', role: 'Admin', employeeId: 1, isActive: true, createdAt: new Date().toISOString() },
  { id: 2, username: 'hr', fullName: 'Trần Thị Nhân Sự', email: 'hr@company.com', role: 'HR', employeeId: 2, isActive: true, createdAt: new Date().toISOString() },
  { id: 3, username: 'manager', fullName: 'Lê Văn Quản Lý', email: 'manager@company.com', role: 'Manager', employeeId: 3, isActive: true, createdAt: new Date().toISOString() },
  { id: 4, username: 'employee', fullName: 'Phạm Văn Nhân Viên', email: 'employee@company.com', role: 'Employee', employeeId: 4, isActive: true, createdAt: new Date().toISOString() },
  { id: 5, username: 'giamdoc', fullName: 'Trần Minh Giám Đốc', email: 'giamdoc@company.com', role: 'Giám đốc', employeeId: 1, isActive: true, createdAt: new Date().toISOString() },
  { id: 6, username: 'thuky', fullName: 'Ngô Mỹ Thư Ký', email: 'thuky@company.com', role: 'Thư ký', employeeId: 2, isActive: true, createdAt: new Date().toISOString() },
  { id: 7, username: 'ketoan', fullName: 'Hoàng Thị Kế Toán', email: 'accounting@company.com', role: 'Kế toán', employeeId: 5, isActive: true, createdAt: new Date().toISOString() }
];

export const initialDepartments: Department[] = [
  { id: 1, departmentCode: 'NS', departmentName: 'Nhân sự', description: 'Phòng quản lý nhân sự và phát triển hệ thống', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 2, departmentCode: 'KD', departmentName: 'Kinh doanh', description: 'Bộ phận tiếp cận khách hàng và bán sản phẩm', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 3, departmentCode: 'KT', departmentName: 'Kế toán', description: 'Bộ phận tài chính công ty', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 4, departmentCode: 'IT', departmentName: 'Kỹ thuật', description: 'Phát triển phần mềm và duy trì hạ tầng', isActive: true, createdAt: new Date().toISOString(), isDeleted: false }
];

export const initialPositions: Position[] = [
  { id: 1, positionCode: 'TP', positionName: 'Trưởng phòng', departmentId: 1, description: 'Lãnh đạo bộ phận và quản lý tiến độ', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 2, positionCode: 'NV', positionName: 'Nhân viên', departmentId: 2, description: 'Nhân sự thực thi nhiệm vụ', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 3, positionCode: 'KTV', positionName: 'Kế toán viên', departmentId: 3, description: 'Kiểm toán và theo dõi thu chi', isActive: true, createdAt: new Date().toISOString(), isDeleted: false },
  { id: 4, positionCode: 'DEV', positionName: 'Lập trình viên', departmentId: 4, description: 'Phát triển các dòng sản phẩm của công ty', isActive: true, createdAt: new Date().toISOString(), isDeleted: false }
];

export const initialEmployees: Employee[] = [
  { id: 1, employeeCode: 'NV001', fullName: 'Nguyễn Văn Admin', gender: 'Nam', dateOfBirth: '1990-05-15', citizenId: '012345678912', phone: '0987654321', email: 'admin@company.com', address: 'Hà Nội', departmentId: 1, positionId: 1, joinDate: '2020-01-01', workStatus: 'Chính thức', baseSalary: 20000000, note: 'Quản trị viên hệ thống', isDeleted: false },
  { id: 2, employeeCode: 'NV002', fullName: 'Trần Thị Nhân Sự', gender: 'Nữ', dateOfBirth: '1992-08-20', citizenId: '023456789012', phone: '0912345678', email: 'hr@company.com', address: 'Đà Nẵng', departmentId: 1, positionId: 1, joinDate: '2021-06-15', workStatus: 'Chính thức', baseSalary: 15000000, note: 'Quản lý tuyển dụng và nhân sự', isDeleted: false },
  { id: 3, employeeCode: 'NV003', fullName: 'Lê Văn Quản Lý', gender: 'Nam', dateOfBirth: '1988-11-05', citizenId: '034567890123', phone: '0905123456', email: 'manager@company.com', address: 'TP. HCM', departmentId: 2, positionId: 2, joinDate: '2019-03-10', workStatus: 'Chính thức', baseSalary: 18000000, note: 'Quản lý phát triển kỹ thuật', isDeleted: false },
  { id: 4, employeeCode: 'NV004', fullName: 'Phạm Văn Nhân Viên', gender: 'Nam', dateOfBirth: '1995-02-28', citizenId: '045678901234', phone: '0977123456', email: 'employee@company.com', address: 'Hải Phòng', departmentId: 4, positionId: 4, joinDate: '2023-01-15', workStatus: 'Thử việc', baseSalary: 12000000, note: 'Nhân viên hỗ trợ IT', isDeleted: false },
  { id: 5, employeeCode: 'NV005', fullName: 'Hoàng Thị Kế Toán', gender: 'Nữ', dateOfBirth: '1994-09-12', citizenId: '056789012345', phone: '0966123456', email: 'accounting@company.com', address: 'Cần Thơ', departmentId: 3, positionId: 3, joinDate: '2022-04-01', workStatus: 'Chính thức', baseSalary: 13000000, note: 'Theo dõi quỹ tiền lương', isDeleted: false }
];

export const initialContracts: Contract[] = [
  { id: 1, employeeId: 1, contractCode: 'HD001', contractType: 'Không xác định', startDate: '2020-01-01', salary: 20000000, status: 'Còn hiệu lực', note: 'Hợp đồng dài hạn' },
  { id: 2, employeeId: 2, contractCode: 'HD002', contractType: '3 năm', startDate: '2021-06-15', endDate: '2024-06-15', salary: 15000000, status: 'Sắp hết hạn', note: 'Hợp đồng lao động' },
  { id: 3, employeeId: 3, contractCode: 'HD003', contractType: '3 năm', startDate: '2019-03-10', endDate: '2022-03-10', salary: 18000000, status: 'Hết hạn', note: 'Hợp đồng đã kết thúc' },
];

export const initialLeaveRequests: LeaveRequest[] = [
  { id: 1, employeeId: 1, leaveType: 'Phép năm', fromDate: '2026-05-10', toDate: '2026-05-12', totalDays: 2.5, reason: 'Nghỉ gia đình có việc riêng', status: 'Approved' },
  { id: 2, employeeId: 5, leaveType: 'Nghỉ ốm', fromDate: '2026-05-22', toDate: '2026-05-23', totalDays: 1, reason: 'Nghỉ khám bệnh định kỳ', status: 'Chờ duyệt' },
  { id: 3, employeeId: 4, leaveType: 'Phép năm', fromDate: '2026-06-01', toDate: '2026-06-02', totalDays: 1, reason: 'Nghỉ giải quyết thủ tục hành chính', status: 'Chờ duyệt' },
];

export const initialAttendanceRecords: AttendanceRecord[] = [
  { id: 1, employeeId: 1, workDate: '2026-05-20', checkInTime: '08:00', checkOutTime: '17:30', workHours: 8.5, attendanceStatus: 'Đủ công' },
  { id: 2, employeeId: 2, workDate: '2026-05-20', checkInTime: '08:15', checkOutTime: '17:30', workHours: 8.25, attendanceStatus: 'Đi muộn' },
  { id: 3, employeeId: 3, workDate: '2026-05-20', checkInTime: '07:55', checkOutTime: '17:00', workHours: 8.0, attendanceStatus: 'Đủ công' },
];

export const initialPayrolls: Payroll[] = [
  { id: 1, employeeId: 1, payrollMonth: '2026-05-01', baseSalary: 20000000, standardWorkDays: 22, actualWorkDays: 22, bonus: 1500000, penalty: 0, netSalary: 21500000, status: 'Đã tính', isLocked: false },
  { id: 2, employeeId: 2, payrollMonth: '2026-05-01', baseSalary: 15000000, standardWorkDays: 22, actualWorkDays: 21, bonus: 500000, penalty: 100000, netSalary: 14718181.82, status: 'Nháp', isLocked: false },
];

export const initialNotifications: Notification[] = [
  { id: 1, userId: 1, title: 'Hệ thống khởi chạy', message: 'Hệ thống Quản lý Nhân sự HRM_WPF_CNPM đã khởi động thành công.', type: 'Success', isRead: true, createdAt: new Date().toISOString() },
  { id: 2, userId: 1, title: 'Nhắc nhở duyệt nghỉ phép', message: 'Đồng chí có 2 đơn xin nghỉ phép đang ở trạng thái chờ duyệt.', type: 'Warning', isRead: false, createdAt: new Date().toISOString() },
  { id: 3, userId: 2, title: 'Hợp đồng sắp hết hạn', message: 'Hợp đồng số HD002 của Trần Thị Nhân Sự sắp hết hạn vào 15/06/2026.', type: 'Warning', isRead: false, createdAt: new Date().toISOString() },
];

export const initialAuditLogs: AuditLog[] = [
  { id: 1, userId: 1, action: 'Đăng nhập', tableName: 'Users', recordId: 1, description: 'Administrator đăng nhập hệ thống thành công.', createdAt: new Date().toISOString() },
];
