export interface User {
  id: number;
  username: string;
  fullName: string;
  email: string;
  role: string; // Admin, HR, Manager, Employee
  employeeId?: number;
  isActive: boolean;
  createdAt: string;
}

export interface Department {
  id: number;
  departmentCode: string;
  departmentName: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  isDeleted: boolean;
}

export interface Position {
  id: number;
  positionCode: string;
  positionName: string;
  departmentId: number;
  description?: string;
  isActive: boolean;
  createdAt: string;
  isDeleted: boolean;
}

export interface Employee {
  id: number;
  employeeCode: string;
  fullName: string;
  gender: string;
  dateOfBirth?: string;
  citizenId?: string;
  phone?: string;
  email?: string;
  address?: string;
  departmentId: number;
  positionId: number;
  joinDate: string;
  workStatus: string; // Thử việc, Chính thức, Tạm nghỉ, Đã nghỉ
  baseSalary: number;
  note?: string;
  isDeleted: boolean;
}

export interface Contract {
  id: number;
  employeeId: number;
  contractCode: string;
  contractType: string;
  startDate: string;
  endDate?: string;
  salary: number;
  status: string;
  note?: string;
}

export interface LeaveRequest {
  id: number;
  employeeId: number;
  leaveType: string;
  fromDate: string;
  toDate: string;
  totalDays: number;
  reason: string;
  status: string; // Chờ duyệt, Đã duyệt, Từ chối
  approvedById?: number;
  rejectReason?: string;
}

export interface AttendanceRecord {
  id: number;
  employeeId: number;
  workDate: string;
  checkInTime?: string;
  checkOutTime?: string;
  workHours: number;
  attendanceStatus: string;
  note?: string;
}

export interface Payroll {
  id: number;
  employeeId: number;
  payrollMonth: string;
  baseSalary: number;
  standardWorkDays: number;
  actualWorkDays: number;
  bonus: number;
  penalty: number;
  netSalary: number;
  status: string;
  isLocked: boolean;
}

export interface Notification {
  id: number;
  userId?: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface AuditLog {
  id: number;
  userId?: number;
  action: string;
  tableName: string;
  recordId?: number;
  description: string;
  createdAt: string;
}
