import React, { useState, useEffect } from 'react';
import { User, Department, Position, Employee, Contract, LeaveRequest, AttendanceRecord, Payroll, Notification, AuditLog } from '../types';
import { DashboardView } from './DashboardView';
import { DepartmentView } from './DepartmentView';
import { PositionView } from './PositionView';
import { EmployeeView } from './EmployeeView';
import { NotificationView } from './NotificationView';
import { LeavePayrollView } from './LeavePayrollView';
import { ProfileView } from './ProfileView';
import {
  Home,
  Users,
  Grid,
  FileText,
  Calendar,
  Clock,
  Calculator,
  Bell,
  List,
  Contact,
  ShieldAlert,
  LogOut,
  ChevronLeft,
  ChevronRight,
  Shield,
  Search,
  Settings
} from 'lucide-react';

interface LayoutProps {
  currentUser: User;
  onLogout: () => void;
  onUpdateCurrentUser: (user: User) => void;
  onInstantLogin: (username: string) => void;
  // Shared state to propagate changes across sections
  employees: Employee[];
  setEmployees: React.Dispatch<React.SetStateAction<Employee[]>>;
  departments: Department[];
  setDepartments: React.Dispatch<React.SetStateAction<Department[]>>;
  positions: Position[];
  setPositions: React.Dispatch<React.SetStateAction<Position[]>>;
  contracts: Contract[];
  setContracts: React.Dispatch<React.SetStateAction<Contract[]>>;
  leaveRequests: LeaveRequest[];
  setLeaveRequests: React.Dispatch<React.SetStateAction<LeaveRequest[]>>;
  attendanceRecords: AttendanceRecord[];
  setAttendanceRecords: React.Dispatch<React.SetStateAction<AttendanceRecord[]>>;
  payrolls: Payroll[];
  setPayrolls: React.Dispatch<React.SetStateAction<Payroll[]>>;
  notifications: Notification[];
  setNotifications: React.Dispatch<React.SetStateAction<Notification[]>>;
  auditLogs: AuditLog[];
  setAuditLogs: React.Dispatch<React.SetStateAction<AuditLog[]>>;
}

export function Layout({
  currentUser,
  onLogout,
  onUpdateCurrentUser,
  onInstantLogin,
  employees,
  setEmployees,
  departments,
  setDepartments,
  positions,
  setPositions,
  contracts,
  setContracts,
  leaveRequests,
  setLeaveRequests,
  attendanceRecords,
  setAttendanceRecords,
  payrolls,
  setPayrolls,
  notifications,
  setNotifications,
  auditLogs,
  setAuditLogs
}: LayoutProps) {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  const formatVietnameseDateTime = (date: Date) => {
    const days = ['Chủ nhật', 'Thứ hai', 'Thứ ba', 'Thứ tư', 'Thứ năm', 'Thứ sáu', 'Thứ bảy'];
    const dayName = days[date.getDay()];
    const dd = String(date.getDate()).padStart(2, '0');
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const yyyy = date.getFullYear();
    const hh = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    const ss = String(date.getSeconds()).padStart(2, '0');
    return `${dayName}, ${dd}/${mm}/${yyyy} — ${hh}:${min}:${ss}`;
  };

  // Helper to add audit logs
  const addLog = (action: string, table: string, desc: string) => {
    const newLog: AuditLog = {
      id: auditLogs.length + 1,
      userId: currentUser.id,
      action,
      tableName: table,
      description: desc,
      createdAt: new Date().toISOString()
    };
    setAuditLogs(prev => [newLog, ...prev]);
  };

  // Permission checks matching WPF structure
  const role = currentUser.role;
  const isAdmin = role === 'Admin' || role === 'Giám đốc';
  const isAtLeastHR = role === 'Admin' || role === 'Giám đốc' || role === 'HR' || role === 'Thư ký';
  const isAtLeastManager = role === 'Admin' || role === 'Giám đốc' || role === 'HR' || role === 'Manager' || role === 'Thư ký';

  const showEmployeeMenu = isAtLeastManager;
  const showDeptPosContractMenu = isAtLeastHR;
  const showAttendanceMenu = isAtLeastManager || role === 'Kế toán';
  const showPayrollMenu = isAtLeastHR || role === 'Employee' || role === 'Kế toán';
  const showAdminOnlyMenu = role === 'Admin' || role === 'Giám đốc';

  // Sidebar items config
  const navItems = [
    { id: 'dashboard', label: 'Dashboard', icon: Home, visible: true },
    { id: 'employees', label: 'Nhân viên', icon: Users, visible: showEmployeeMenu },
    { id: 'departments', label: 'Phòng ban', icon: Grid, visible: showDeptPosContractMenu },
    { id: 'positions', label: 'Chức vụ', icon: Contact, visible: showDeptPosContractMenu },
    { id: 'contracts', label: 'Hợp đồng', icon: FileText, visible: showDeptPosContractMenu && role !== 'Thư ký' },
    { id: 'leaves', label: 'Nghỉ phép', icon: Calendar, visible: true },
    { id: 'attendance', label: 'Chấm công', icon: Clock, visible: showAttendanceMenu },
    { id: 'payroll', label: 'Lương', icon: Calculator, visible: showPayrollMenu },
    { id: 'notifications', label: 'Thông báo', icon: Bell, badgeCount: notifications.filter(n => !n.isRead).length, visible: true },
    { id: 'profile', label: 'Cài đặt tài khoản', icon: Settings, visible: true },
    { id: 'auditlog', label: 'Audit Log', icon: List, visible: showAdminOnlyMenu }
  ];

  return (
    <div className="h-screen w-full flex bg-slate-50 text-zinc-900 overflow-hidden font-sans">
      {/* Dynamic Navigation Pane Sidebar */}
      <aside className={`bg-white border-r-2 border-purple-100 transition-all duration-300 flex flex-col relative z-20 shadow-xl ${
        isSidebarOpen ? 'w-72' : 'w-24'
      }`}>
        {/* Sidebar Header */}
        <div className="p-6 flex items-center gap-4 bg-[#1e293b] border-b-4 border-blue-500">
          <div className="w-10 h-10 bg-blue-600 rounded-xl flex-shrink-0 flex items-center justify-center shadow-lg">
            <Shield className="w-6 h-6 text-white" />
          </div>
          {isSidebarOpen && (
            <div className="overflow-hidden">
              <h1 className="font-black text-white text-xl tracking-tight uppercase leading-none">NEXUSHQ HRM</h1>
              <p className="text-[10px] font-bold text-blue-400 uppercase tracking-widest mt-1">HRM System</p>
            </div>
          )}
        </div>

        {/* Sidebar Navigation Items */}
        <nav className="flex-1 px-3 py-6 space-y-2 overflow-y-auto custom-scrollbar">
          {navItems.filter(item => item.visible).map(item => (
            <button
              key={item.id}
              onClick={() => setActiveTab(item.id)}
              className={`w-full flex items-center gap-4 px-4 py-3.5 rounded-xl transition-all duration-200 group relative ${
                activeTab === item.id
                  ? 'bg-blue-600 text-white shadow-md'
                  : 'text-slate-700 hover:bg-blue-50/50 hover:text-blue-700'
              }`}
            >
              {activeTab === item.id && (
                <div className="absolute left-0 top-2 bottom-2 w-1.5 bg-indigo-400 rounded-r-full" />
              )}
              <item.icon className={`w-6 h-6 flex-shrink-0 transition-transform ${
                activeTab === item.id ? 'text-white' : 'text-slate-550 group-hover:text-blue-600'
              }`} />
              
              {isSidebarOpen && (
                <span className="font-bold tracking-tight text-md">{item.label}</span>
              )}

              {/* Notification Badges */}
              {item.badgeCount && item.badgeCount > 0 ? (
                <span className="absolute right-4 bg-red-650 text-white rounded-full text-[10px] w-5 h-5 flex items-center justify-center font-bold">
                  {item.badgeCount}
                </span>
              ) : null}
            </button>
          ))}
        </nav>

        {/* User Sidebar Footer */}
        <div className="p-4 border-t border-zinc-100 bg-zinc-50/50">
          <div className="flex items-center gap-3 mb-3 p-2 bg-white border border-zinc-250/20 rounded-xl">
            <div className="w-10 h-10 rounded-xl bg-blue-600/10 flex items-center justify-center text-blue-700 font-bold">
              {currentUser.fullName.charAt(0)}
            </div>
            {isSidebarOpen && (
              <div className="overflow-hidden">
                <p className="text-sm font-black text-zinc-900 truncate uppercase tracking-tight leading-tight">{currentUser.fullName}</p>
                <span className="text-[10px] font-black bg-blue-600 text-white px-2 py-0.5 rounded border border-blue-500/20 mt-1 inline-block">
                  {currentUser.role}
                </span>
              </div>
            )}
          </div>

          <button
            onClick={onLogout}
            className="w-full flex items-center gap-4 px-4 py-3 rounded-xl text-zinc-500 hover:bg-rose-50 hover:text-rose-600 transition-all font-bold"
          >
            <LogOut className="w-6 h-6 flex-shrink-0" />
            {isSidebarOpen && <span className="font-bold tracking-tight text-md">Đăng xuất</span>}
          </button>
        </div>
      </aside>

      {/* Main Content Pane */}
      <main className="flex-1 flex flex-col overflow-hidden relative">
          {/* Elegant Header */}
          <header className="h-20 bg-gradient-to-r from-[#1e293b] to-[#0f172a] text-white px-8 flex items-center justify-between sticky top-0 z-10 border-b-4 border-blue-500 shadow-lg">
            <div className="flex items-center gap-6">
              <button
                onClick={() => setIsSidebarOpen(!isSidebarOpen)}
                className="p-2 hover:bg-white/10 rounded-xl text-white/80 hover:text-white transition-all outline-none"
              >
                {isSidebarOpen ? <ChevronLeft className="w-6 h-6" /> : <ChevronRight className="w-6 h-6" />}
              </button>
              <div className="hidden lg:flex flex-col">
                <h2 className="font-black text-xl uppercase tracking-tight leading-none">Cổng thông tin nhân sự</h2>
                <p className="text-[10px] font-bold text-blue-400 uppercase tracking-[0.2em] mt-1">Công Ty Cổ Phần Công Nghệ &amp; Thương Mại NexusHQ</p>
              </div>
            </div>

            {/* Realtime Ticking Clock */}
            <div className="hidden md:flex items-center gap-2.5 px-4 py-2 bg-white/5 border border-white/10 rounded-xl text-xs text-blue-400 font-black font-mono shadow-inner">
              <Clock className="w-4 h-4 text-blue-400 animate-pulse" />
              <span>{formatVietnameseDateTime(currentTime)}</span>
            </div>

            {/* Quick Stats Search Header bar */}
            <div className="flex items-center gap-6">
              <div className="relative hidden lg:block">
                <Search className="w-4 h-4 absolute left-4 top-1/2 -translate-y-1/2 text-white/40" />
                <input
                  type="text"
                  placeholder="Tra cứu nhân viên, phòng ban..."
                  className="pl-11 pr-6 py-2 bg-white/10 border border-white/10 rounded-full text-xs w-52 focus:ring-2 focus:ring-blue-500 focus:bg-white/20 transition-all outline-none placeholder:text-white/30"
                />
              </div>

              <div className="flex items-center gap-2 px-3 py-1.5 bg-white/5 border border-white/10 rounded-xl text-xs text-blue-400 font-bold font-mono">
                <Shield className="w-4 h-4 text-blue-500" />
                SECURE
              </div>
            </div>
          </header>

        {/* View Router Render Content */}
        <div className="flex-1 overflow-y-auto p-8 custom-scrollbar relative z-0">
          <div className="max-w-7xl mx-auto">
            {activeTab === 'dashboard' && (
              <DashboardView
                employees={employees}
                departments={departments}
                positions={positions}
                leaveRequests={leaveRequests}
                contracts={contracts}
              />
            )}
            {activeTab === 'employees' && (
              <EmployeeView
                employees={employees}
                setEmployees={setEmployees}
                departments={departments}
                positions={positions}
                addLog={addLog}
                currentUser={currentUser}
                contracts={contracts}
                leaveRequests={leaveRequests}
                attendanceRecords={attendanceRecords}
              />
            )}
            {activeTab === 'departments' && (
              <DepartmentView
                departments={departments}
                setDepartments={setDepartments}
                addLog={addLog}
              />
            )}
            {activeTab === 'positions' && (
              <PositionView
                positions={positions}
                setPositions={setPositions}
                departments={departments}
                addLog={addLog}
              />
            )}
            {activeTab === 'contracts' && (
              <LeavePayrollView
                tab="contracts"
                employees={employees}
                contracts={contracts}
                setContracts={setContracts}
                leaveRequests={leaveRequests}
                setLeaveRequests={setLeaveRequests}
                attendanceRecords={attendanceRecords}
                payrolls={payrolls}
                setPayrolls={setPayrolls}
                addLog={addLog}
              />
            )}
            {activeTab === 'leaves' && (
              <LeavePayrollView
                tab="leaves"
                employees={employees}
                contracts={contracts}
                setContracts={setContracts}
                leaveRequests={leaveRequests}
                setLeaveRequests={setLeaveRequests}
                attendanceRecords={attendanceRecords}
                payrolls={payrolls}
                setPayrolls={setPayrolls}
                addLog={addLog}
              />
            )}
            {activeTab === 'attendance' && (
              <LeavePayrollView
                tab="attendance"
                employees={employees}
                contracts={contracts}
                setContracts={setContracts}
                leaveRequests={leaveRequests}
                setLeaveRequests={setLeaveRequests}
                attendanceRecords={attendanceRecords}
                setAttendanceRecords={setAttendanceRecords}
                payrolls={payrolls}
                setPayrolls={setPayrolls}
                addLog={addLog}
                currentUser={currentUser}
                departments={departments}
              />
            )}
            {activeTab === 'payroll' && (
              <LeavePayrollView
                tab="payroll"
                employees={employees}
                contracts={contracts}
                setContracts={setContracts}
                leaveRequests={leaveRequests}
                setLeaveRequests={setLeaveRequests}
                attendanceRecords={attendanceRecords}
                payrolls={payrolls}
                setPayrolls={setPayrolls}
                addLog={addLog}
              />
            )}
            {activeTab === 'notifications' && (
              <NotificationView
                notifications={notifications}
                setNotifications={setNotifications}
                addLog={addLog}
              />
            )}
            {activeTab === 'profile' && (
              <ProfileView
                currentUser={currentUser}
                onUpdateCurrentUser={onUpdateCurrentUser}
                addLog={addLog}
                onInstantLogin={onInstantLogin}
              />
            )}
            {activeTab === 'auditlog' && (
              <div className="space-y-6 animate-fade-in">
                <div className="flex flex-col">
                  <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Audit Logs (Nhật ký Hệ thống)</h1>
                  <p className="text-sm text-zinc-500 mt-1">Danh sách lưu chiểu toàn thể hoạt động tương ứng với Cơ sở dữ liệu</p>
                </div>
                <div className="bg-white border rounded-xl overflow-hidden shadow-sm">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                        <th className="px-6 py-4">Thời gian</th>
                        <th className="px-6 py-4">Hành động</th>
                        <th className="px-6 py-4">Bảng dữ liệu</th>
                        <th className="px-6 py-4">Chi tiết hoạt động</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-50 text-sm font-semibold">
                      {auditLogs.map(l => (
                        <tr key={l.id} className="hover:bg-zinc-50/50">
                          <td className="px-6 py-4 text-zinc-400 font-mono font-bold">
                            {new Date(l.createdAt).toLocaleString('vi-VN')}
                          </td>
                          <td className="px-6 py-4">
                            <span className="bg-emerald-50 text-emerald-800 px-2.5 py-1 rounded-lg border border-emerald-100 text-xs font-bold font-mono">
                              {l.action}
                            </span>
                          </td>
                          <td className="px-6 py-4 font-mono font-bold text-zinc-500">{l.tableName}</td>
                          <td className="px-6 py-4 text-zinc-700 font-bold">{l.description}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
