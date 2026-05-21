import React, { useState, useEffect } from 'react';
import { User, Department, Position, Employee, Contract, LeaveRequest, AttendanceRecord, Payroll, Notification, AuditLog } from './types';
import { Layout } from './components/Layout';
import { Shield, Key, Eye, EyeOff, Loader2, AlertCircle, Info, Monitor } from 'lucide-react';
import {
  initialUsers,
  initialDepartments,
  initialPositions,
  initialEmployees,
  initialContracts,
  initialLeaveRequests,
  initialAttendanceRecords,
  initialPayrolls,
  initialNotifications,
  initialAuditLogs
} from './data';

export default function App() {
  // Navigation states
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Core reactive data stores representing DB
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [positions, setPositions] = useState<Position[]>([]);
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecord[]>([]);
  const [payrolls, setPayrolls] = useState<Payroll[]>([]);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);

  // Input states
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [isBusy, setIsBusy] = useState(false);

  // Initialize and load from LocalStorage to represent a persistent DBMS
  useEffect(() => {
    // Helper to read storage or default to seeds
    const readStorageDef = <T,>(key: string, def: T): T => {
      const stored = localStorage.getItem(key);
      if (stored) {
        try { return JSON.parse(stored); } catch { return def; }
      }
      return def;
    };

    setEmployees(readStorageDef('hrm_employees', initialEmployees));
    setDepartments(readStorageDef('hrm_departments', initialDepartments));
    setPositions(readStorageDef('hrm_positions', initialPositions));
    setContracts(readStorageDef('hrm_contracts', initialContracts));
    setLeaveRequests(readStorageDef('hrm_leave_requests', initialLeaveRequests));
    setAttendanceRecords(readStorageDef('hrm_attendance', initialAttendanceRecords));
    setPayrolls(readStorageDef('hrm_payrolls', initialPayrolls));
    setNotifications(readStorageDef('hrm_notifications', initialNotifications));
    setAuditLogs(readStorageDef('hrm_audit_logs', initialAuditLogs));

    // Session recovery
    const savedUser = localStorage.getItem('hrm_user_session');
    if (savedUser) {
      try { setCurrentUser(JSON.parse(savedUser)); } catch { /* ignore */ }
    }

    setLoading(false);
  }, []);

  // Save changes to LocalStorage when states change to maintain database persistence
  useEffect(() => {
    if (loading) return;
    localStorage.setItem('hrm_employees', JSON.stringify(employees));
    localStorage.setItem('hrm_departments', JSON.stringify(departments));
    localStorage.setItem('hrm_positions', JSON.stringify(positions));
    localStorage.setItem('hrm_contracts', JSON.stringify(contracts));
    localStorage.setItem('hrm_leave_requests', JSON.stringify(leaveRequests));
    localStorage.setItem('hrm_attendance', JSON.stringify(attendanceRecords));
    localStorage.setItem('hrm_payrolls', JSON.stringify(payrolls));
    localStorage.setItem('hrm_notifications', JSON.stringify(notifications));
    localStorage.setItem('hrm_audit_logs', JSON.stringify(auditLogs));
  }, [employees, departments, positions, contracts, leaveRequests, attendanceRecords, payrolls, notifications, auditLogs, loading]);

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (!username || !password) {
      setErrorMsg('Vui lòng nhập đầy đủ tài khoản và mật khẩu.');
      return;
    }

    setIsBusy(true);
    setErrorMsg('');

    // Simulate database lookup matching WPF AuthService.cs
    setTimeout(() => {
      const matchedUser = initialUsers.find(
        u => u.username.toLowerCase() === username.toLowerCase() && password === '123'
      );

      if (matchedUser) {
        setCurrentUser(matchedUser);
        localStorage.setItem('hrm_user_session', JSON.stringify(matchedUser));
        
        // Add auth audit log
        const auditLog: AuditLog = {
          id: auditLogs.length + 1,
          userId: matchedUser.id,
          action: 'Đăng nhập',
          tableName: 'Users',
          description: `${matchedUser.fullName} (Quyền: ${matchedUser.role}) đăng nhập hệ thống thông qua ModernWpf.`,
          createdAt: new Date().toISOString()
        };
        setAuditLogs(prev => [auditLog, ...prev]);
      } else {
        setErrorMsg('Tên đăng nhập hoặc mật khẩu không chính xác (Mật khẩu mặc định là 123).');
      }
      setIsBusy(false);
    }, 600);
  };

  const handleUpdateCurrentUser = (updatedUser: User) => {
    setCurrentUser(updatedUser);
    localStorage.setItem('hrm_user_session', JSON.stringify(updatedUser));
  };

  const handleInstantLogin = (testUsername: string) => {
    const matchedUser = initialUsers.find(
      u => u.username.toLowerCase() === testUsername.toLowerCase()
    );
    if (matchedUser) {
      setCurrentUser(matchedUser);
      localStorage.setItem('hrm_user_session', JSON.stringify(matchedUser));
      
      const auditLog: AuditLog = {
        id: auditLogs.length + 1,
        userId: matchedUser.id,
        action: 'Đăng nhập giả lập',
        tableName: 'Users',
        description: `Chuyển quyền giả lập sang tài khoản: ${matchedUser.fullName} (${matchedUser.role}) thành công.`,
        createdAt: new Date().toISOString()
      };
      setAuditLogs(prev => [auditLog, ...prev]);
    }
  };

  const handleLogout = () => {
    if (currentUser) {
      const auditLog: AuditLog = {
        id: auditLogs.length + 1,
        userId: currentUser.id,
        action: 'Đăng xuất',
        tableName: 'Users',
        description: `${currentUser.fullName} đăng xuất khỏi phiên làm việc.`,
        createdAt: new Date().toISOString()
      };
      setAuditLogs(prev => [auditLog, ...prev]);
    }
    setCurrentUser(null);
    localStorage.removeItem('hrm_user_session');
    setUsername('');
    setPassword('');
  };

  if (loading) {
    return (
      <div className="h-screen w-full flex flex-col items-center justify-center bg-[#f8fafc] gap-4">
        <Loader2 className="w-10 h-10 animate-spin text-blue-600" />
        <p className="text-sm font-bold text-zinc-600 uppercase tracking-widest animate-pulse">Đang kết nối cơ sở dữ liệu HRM...</p>
      </div>
    );
  }

  if (!currentUser) {
    return (
      <div className="h-screen w-full flex flex-col items-center justify-center bg-[#f1f5f9] p-4 relative overflow-hidden font-sans">
        {/* Subtle grid background */}
        <div className="absolute inset-0 opacity-[0.05] pointer-events-none" style={{ backgroundImage: 'radial-gradient(#1e293b 1.2px, transparent 1.2px)', backgroundSize: '24px 24px' }} />

        {/* Windows 11 Login Window emulation */}
        <div className="max-w-md w-full bg-white border border-zinc-300 rounded-2xl shadow-2xl overflow-hidden relative z-10">
          
          {/* Win11 Titlebar */}
          <div className="bg-zinc-100/80 px-4 py-2 border-b flex items-center justify-between">
            <div className="flex items-center gap-2 text-xs font-bold text-zinc-500">
              <Monitor className="w-3.5 h-3.5" />
              <span>Đăng nhập cổng thông tin - NexusHQ HRM</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-zinc-300 hover:bg-zinc-400 cursor-pointer" />
              <div className="w-3 h-3 rounded-full bg-zinc-300 hover:bg-zinc-400 cursor-pointer" />
              <div className="w-3 h-3 rounded-full bg-red-400 hover:bg-red-500 cursor-pointer" />
            </div>
          </div>

          <div className="p-8">
            <div className="w-20 h-20 bg-[#1e293b] rounded-2xl flex items-center justify-center mx-auto mb-6 shadow-md border-2 border-blue-500">
              <Shield className="w-10 h-10 text-blue-500" />
            </div>
            <h1 className="text-2xl font-black text-center text-[#0f172a] tracking-tight uppercase leading-none">NEXUSHQ HRM</h1>
            <p className="text-center text-xs text-zinc-500 mt-2 font-bold uppercase tracking-wider">Hệ thống Quản trị & Phát triển Nhân lực Doanh nghiệp</p>

            <form onSubmit={handleLogin} className="mt-8 space-y-4">
              {errorMsg && (
                <div className="bg-red-50 text-red-650 px-4 py-3 rounded-xl border border-red-200 text-xs font-bold flex items-center gap-2 animate-bounce">
                  <AlertCircle className="w-4 h-4 flex-shrink-0" />
                  <span>{errorMsg}</span>
                </div>
              )}

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-widest">Tên đăng nhập</label>
                <div className="relative">
                  <input
                    type="text"
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    placeholder="admin, hr, manager, employee..."
                    className="w-full pl-4 pr-10 py-3 bg-zinc-50 border border-zinc-250/20 rounded-xl text-sm focus:ring-2 focus:ring-blue-500/25 focus:border-blue-500 outline-none transition-all placeholder:text-zinc-400"
                    disabled={isBusy}
                  />
                  <Shield className="w-4 h-4 text-zinc-400 absolute right-4 top-1/2 -translate-y-1/2" />
                </div>
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-widest">Mật khẩu (*)</label>
                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    placeholder="Nhập mật khẩu..."
                    className="w-full pl-4 pr-10 py-3 bg-zinc-50 border border-zinc-250/20 rounded-xl text-sm focus:ring-2 focus:ring-blue-500/25 focus:border-blue-500 outline-none transition-all"
                    disabled={isBusy}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-650 focus:outline-none"
                  >
                    {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
              </div>

              <button
                type="submit"
                disabled={isBusy}
                className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white py-3.5 rounded-xl font-bold text-sm transition-all shadow-md active:scale-95 disabled:opacity-50 mt-6"
              >
                {isBusy ? (
                  <Loader2 className="w-4 h-4 animate-spin text-white" />
                ) : (
                  <Key className="w-4 h-4" />
                )}
                Đăng nhập hệ thống
              </button>
            </form>

            {/* Hint Box for demo accounts */}
            <div className="mt-8 bg-zinc-50 border border-zinc-200/70 p-4 rounded-xl text-xs flex gap-3">
              <Info className="w-4 h-4 text-zinc-500 mt-1 flex-shrink-0" />
              <div>
                <p className="font-bold text-zinc-700">Tài khoản demo thử nghiệm (Mật khẩu: 123)</p>
                <div className="mt-1.5 grid grid-cols-2 gap-x-4 gap-y-1 text-zinc-500 text-[11px]">
                  <div>• <span className="font-bold text-zinc-700">giamdoc</span>: Giám đốc (CEO)</div>
                  <div>• <span className="font-bold text-zinc-700">thuky</span>: Thư ký tổng hợp</div>
                  <div>• <span className="font-bold text-zinc-700">ketoan</span>: Kế toán trưởng</div>
                  <div>• <span className="font-bold text-zinc-700">employee</span>: Nhân viên</div>
                  <div>• <span className="font-bold text-zinc-700">admin</span>: Toàn quyền Admin</div>
                  <div>• <span className="font-bold text-zinc-700">hr</span>: Quản lý Nhân sự</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-8 text-[11px] font-black tracking-[0.25em] uppercase text-zinc-400 text-center">
          © 2026 NexusHQ Enterprise • Hệ thống quản lý nhân sự chuyên nghiệp
        </div>
      </div>
    );
  }

  // Once logged in, show Layout
  return (
    <Layout
      currentUser={currentUser}
      onLogout={handleLogout}
      onUpdateCurrentUser={handleUpdateCurrentUser}
      onInstantLogin={handleInstantLogin}
      employees={employees}
      setEmployees={setEmployees}
      departments={departments}
      setDepartments={setDepartments}
      positions={positions}
      setPositions={setPositions}
      contracts={contracts}
      setContracts={setContracts}
      leaveRequests={leaveRequests}
      setLeaveRequests={setLeaveRequests}
      attendanceRecords={attendanceRecords}
      setAttendanceRecords={setAttendanceRecords}
      payrolls={payrolls}
      setPayrolls={setPayrolls}
      notifications={notifications}
      setNotifications={setNotifications}
      auditLogs={auditLogs}
      setAuditLogs={setAuditLogs}
    />
  );
}
