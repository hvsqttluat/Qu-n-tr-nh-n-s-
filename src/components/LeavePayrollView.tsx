import React, { useState } from 'react';
import { Contract, LeaveRequest, AttendanceRecord, Payroll, Employee, User, Department } from '../types';
import { Calendar, FileText, Calculator, Clock, Check, X, ShieldAlert, Award, FileSpreadsheet, Search } from 'lucide-react';
import { ExportContractModal } from './ExportContractModal';

interface LeavePayrollViewProps {
  tab: string; // contracts, leaves, attendance, payroll
  employees: Employee[];
  contracts: Contract[];
  setContracts: React.Dispatch<React.SetStateAction<Contract[]>>;
  leaveRequests: LeaveRequest[];
  setLeaveRequests: React.Dispatch<React.SetStateAction<LeaveRequest[]>>;
  attendanceRecords: AttendanceRecord[];
  setAttendanceRecords?: React.Dispatch<React.SetStateAction<AttendanceRecord[]>>;
  payrolls: Payroll[];
  setPayrolls: React.Dispatch<React.SetStateAction<Payroll[]>>;
  addLog: (action: string, table: string, desc: string) => void;
  currentUser?: User;
  departments?: Department[];
}

export function LeavePayrollView({
  tab,
  employees,
  contracts,
  setContracts,
  leaveRequests,
  setLeaveRequests,
  attendanceRecords,
  setAttendanceRecords,
  payrolls,
  setPayrolls,
  addLog,
  currentUser,
  departments = []
}: LeavePayrollViewProps) {
  const [bonusInput, setBonusInput] = useState<{ [empId: number]: number }>({});
  const [penaltyInput, setPenaltyInput] = useState<{ [empId: number]: number }>({});
  
  // Contract Export States
  const [isContractModalOpen, setIsContractModalOpen] = useState(false);
  const [selectedContractEmp, setSelectedContractEmp] = useState<Employee | null>(null);
  const [selectedContract, setSelectedContract] = useState<Contract | null>(null);

  // Attendance Management Core States
  const [attendSearch, setAttendSearch] = useState('');
  const [attendDeptFilter, setAttendDeptFilter] = useState('all');
  const [attendEmpFilter, setAttendEmpFilter] = useState('all');
  const [attendStatusFilter, setAttendStatusFilter] = useState('all');
  const [attendMonthFilter, setAttendMonthFilter] = useState('all');
  const [attendYearFilter, setAttendYearFilter] = useState('all');

  // Attendance Form States
  const [selectedAttend, setSelectedAttend] = useState<AttendanceRecord | null>(null);
  const [formEmpId, setFormEmpId] = useState('');
  const [formDate, setFormDate] = useState(new Date().toISOString().split('T')[0]);
  const [formIn, setFormIn] = useState('08:00');
  const [formOut, setFormOut] = useState('17:00');
  const [formHours, setFormHours] = useState(8.0);
  const [formStatus, setFormStatus] = useState('Đủ công');
  const [formNote, setFormNote] = useState('');
  const [attendSuccess, setAttendSuccess] = useState('');
  const [attendError, setAttendError] = useState('');

  // Auto-calculate work hours, lunch breaks & late/early arrivals
  const checkTimeDetails = (chkIn: string, chkOut: string) => {
    if (!chkIn || !chkOut) {
      return { ok: true, hours: 0, status: 'Nghỉ không phép', err: '' };
    }
    const parseTime = (t: string) => {
      const parts = t.split(':');
      if (parts.length < 2) return null;
      const h = parseInt(parts[0], 10);
      const m = parseInt(parts[1], 10);
      return isNaN(h) || isNaN(m) ? null : h + m / 60;
    };
    const inVal = parseTime(chkIn);
    const outVal = parseTime(chkOut);
    if (inVal === null || outVal === null) {
      return { ok: false, hours: 0, status: 'Nghỉ không phép', err: 'Giờ vào/ra không đúng định dạng HH:mm.' };
    }
    if (outVal <= inVal) {
      return { ok: false, hours: 0, status: 'Nghỉ không phép', err: 'Giờ ra (Check-Out) phải lớn hơn giờ vào.' };
    }
    let dur = outVal - inVal;
    if (dur > 5.0) {
      dur -= 1.0; // Break deduction
    }
    dur = Math.round(dur * 100) / 100;

    let suggested = 'Đủ công';
    if (inVal > 8.0) {
      suggested = 'Đi muộn'; // Priority late as requested
    } else if (outVal < 17.0) {
      suggested = 'Về sớm';
    }
    return { ok: true, hours: dur, status: suggested, err: '' };
  };

  // Run calculation action
  const runCalculate = () => {
    setAttendSuccess('');
    setAttendError('');
    if (formStatus === 'Nghỉ phép') {
      setFormHours(0);
      setAttendSuccess('Đã nhận diện trạng thái Nghỉ phép. Đặt số giờ làm bằng 0.');
      return;
    }
    const res = checkTimeDetails(formIn, formOut);
    if (!res.ok) {
      setAttendError(res.err);
      return;
    }
    setFormHours(res.hours);
    setFormStatus(res.status);
    setAttendSuccess('Đã tự động tính toán giờ làm việc và đề xuất trạng thái thành công.');
  };

  // Sync state if employee has approved leave request for the chosen work-date
  React.useEffect(() => {
    if (!formEmpId || !formDate) return;
    const empIdNum = parseInt(formEmpId, 10);
    const onLeave = leaveRequests.some(l => 
      l.employeeId === empIdNum &&
      l.status === 'Đã duyệt' &&
      new Date(l.fromDate).getTime() <= new Date(formDate).getTime() &&
      new Date(l.toDate).getTime() >= new Date(formDate).getTime()
    );
    if (onLeave) {
      setFormIn('');
      setFormOut('');
      setFormHours(0);
      setFormStatus('Nghỉ phép');
      setFormNote('Nghỉ phép tự động (Đã được duyệt đơn nghỉ phép).');
    }
  }, [formEmpId, formDate, leaveRequests]);

  const resetAttendForm = () => {
    setSelectedAttend(null);
    setFormEmpId('');
    setFormDate(new Date().toISOString().split('T')[0]);
    setFormIn('08:00');
    setFormOut('17:00');
    setFormHours(8.0);
    setFormStatus('Đủ công');
    setFormNote('');
    setAttendSuccess('');
    setAttendError('');
  };

  const handleSelectAttend = (record: AttendanceRecord) => {
    setSelectedAttend(record);
    setFormEmpId(String(record.employeeId));
    setFormDate(record.workDate.split('T')[0]);
    setFormIn(record.checkInTime || '');
    setFormOut(record.checkOutTime || '');
    setFormHours(record.workHours);
    setFormStatus(record.attendanceStatus);
    setFormNote(record.note || '');
    setAttendSuccess('');
    setAttendError('');
  };

  const handleAddAttend = () => {
    setAttendSuccess('');
    setAttendError('');
    if (!formEmpId) {
      setAttendError('Vui lòng chọn nhân viên.');
      return;
    }
    if (!formDate) {
      setAttendError('Ngày công không được trống.');
      return;
    }

    const empIdNum = parseInt(formEmpId, 10);
    // Overlap checks
    const exists = attendanceRecords.some(r => 
      r.employeeId === empIdNum && 
      new Date(r.workDate).toDateString() === new Date(formDate).toDateString()
    );
    if (exists) {
      setAttendError(`Nhân viên đã được chấm công cho ngày ${new Date(formDate).toLocaleDateString('vi-VN')}. Một ngày chỉ chấm công tối đa 1 lần.`);
      return;
    }

    const calc = checkTimeDetails(formIn, formOut);
    if (!calc.ok) {
      setAttendError(calc.err);
      return;
    }

    const newRecord: AttendanceRecord = {
      id: attendanceRecords.length > 0 ? Math.max(...attendanceRecords.map(r => r.id)) + 1 : 1,
      employeeId: empIdNum,
      workDate: formDate,
      checkInTime: formIn || undefined,
      checkOutTime: formOut || undefined,
      workHours: formStatus === 'Nghỉ phép' ? 0 : calc.hours,
      attendanceStatus: formStatus,
      note: formNote
    };

    if (setAttendanceRecords) {
      setAttendanceRecords(prev => [newRecord, ...prev]);
    }
    addLog('Thêm chấm công', 'AttendanceRecords', `Thêm mới bản ghi chấm công cho nhân viên ID ${empIdNum} ngày ${formDate}`);
    setAttendSuccess('Thêm bản ghi chấm công thành công.');
    resetAttendForm();
  };

  const handleUpdateAttend = () => {
    setAttendSuccess('');
    setAttendError('');
    if (!selectedAttend) {
      setAttendError('Chưa chọn bản ghi chấm công nào từ bảng để Cập nhật.');
      return;
    }
    if (!formEmpId) {
      setAttendError('Nhân viên không được trống.');
      return;
    }
    if (!formDate) {
      setAttendError('Ngày công không được trống.');
      return;
    }

    const empIdNum = parseInt(formEmpId, 10);

    // Swap day check
    const duplicate = attendanceRecords.some(r => 
      r.id !== selectedAttend.id &&
      r.employeeId === empIdNum &&
      new Date(r.workDate).toDateString() === new Date(formDate).toDateString()
    );
    if (duplicate) {
      setAttendError(`Nhân viên đã được chấm công cho ngày ${new Date(formDate).toLocaleDateString('vi-VN')}.`);
      return;
    }

    const calc = checkTimeDetails(formIn, formOut);
    if (!calc.ok) {
      setAttendError(calc.err);
      return;
    }

    if (setAttendanceRecords) {
      setAttendanceRecords(prev => prev.map(r => r.id === selectedAttend.id ? {
        ...r,
        employeeId: empIdNum,
        workDate: formDate,
        checkInTime: formIn || undefined,
        checkOutTime: formOut || undefined,
        workHours: formStatus === 'Nghỉ phép' ? 0 : calc.hours,
        attendanceStatus: formStatus,
        note: formNote
      } : r));
    }
    addLog('Sửa chấm công', 'AttendanceRecords', `Cập nhật thông tin bản ghi chấm công ID ${selectedAttend.id} ngày ${formDate}`);
    setAttendSuccess('Cập nhật bản ghi chấm công thành công.');
    resetAttendForm();
  };

  const handleDeleteAttend = () => {
    setAttendSuccess('');
    setAttendError('');
    if (!selectedAttend) {
      setAttendError('Chưa chọn bản ghi chấm công để Xóa.');
      return;
    }

    if (setAttendanceRecords) {
      setAttendanceRecords(prev => prev.filter(r => r.id !== selectedAttend.id));
    }
    addLog('Xóa chấm công', 'AttendanceRecords', `Hủy xóa dứt điểm bản ghi chấm công ID ${selectedAttend.id}`);
    setAttendSuccess('Đã xóa bản ghi chấm công thành công.');
    resetAttendForm();
  };

  const handleOpenContractModal = (emp: Employee, c: Contract) => {
    setSelectedContractEmp(emp);
    setSelectedContract(c);
    setIsContractModalOpen(true);
  };

  const handleApproveLeave = (id: number) => {
    setLeaveRequests(prev => prev.map(l => l.id === id ? { ...l, status: 'Đã duyệt' } : l));
    addLog('Duyệt nghỉ phép', 'LeaveRequests', `Đã duyệt phép cho đơn nghỉ của nhân viên với ID ${id}`);
  };

  const handleRejectLeave = (id: number) => {
    setLeaveRequests(prev => prev.map(l => l.id === id ? { ...l, status: 'Từ chối' } : l));
    addLog('Từ chối nghỉ phép', 'LeaveRequests', `Bác bỏ đơn nghỉ của nhân viên với ID ${id}`);
  };

  const calculatePayroll = () => {
    // Generate/refresh payroll for all non-deleted employees for current month
    const validEmployees = employees.filter(e => !e.isDeleted);
    const newPayrolls: Payroll[] = validEmployees.map((emp, index) => {
      const bonus = bonusInput[emp.id] || 0;
      const penalty = penaltyInput[emp.id] || 0;
      const baseSalary = emp.baseSalary;
      const actualDays = emp.workStatus === 'Chính thức' ? 22 : 20; // Simulated workdays
      const salaryFactor = actualDays / 22;
      const netSalary = Math.round((baseSalary * salaryFactor) + bonus - penalty);

      return {
        id: index + 1,
        employeeId: emp.id,
        payrollMonth: '2026-05-01',
        baseSalary,
        standardWorkDays: 22,
        actualWorkDays: actualDays,
        bonus,
        penalty,
        netSalary,
        status: 'Đã tính',
        isLocked: false
      };
    });

    setPayrolls(newPayrolls);
    addLog('Tính tiền lương', 'Payrolls', 'Đã thực thi chạy thuật toán đồng bộ tiền lương cho toàn thể nhân sự');
  };

  const lockPayrollAll = () => {
    setPayrolls(prev => prev.map(p => ({ ...p, isLocked: true, status: 'Đã khóa' })));
    addLog('Chốt sổ lương', 'Payrolls', 'Đã khóa vĩnh viễn bảng lương tháng hiện hành');
  };

  const getFilteredAttendanceRecords = () => {
    const roleStr = currentUser?.role || 'Admin';
    let records = attendanceRecords || [];

    if (roleStr === 'Employee' && currentUser?.employeeId) {
      records = records.filter(r => r.employeeId === currentUser.employeeId);
    } else if (roleStr === 'Manager' && currentUser?.employeeId) {
      const managerObj = employees.find(e => e.id === currentUser.employeeId);
      if (managerObj) {
        const deptIds = employees.filter(e => e.departmentId === managerObj.departmentId).map(e => e.id);
        records = records.filter(r => deptIds.includes(r.employeeId));
      }
    }

    if (attendSearch) {
      const q = attendSearch.toLowerCase();
      records = records.filter(r => {
        const emp = employees.find(e => e.id === r.employeeId);
        return (
          emp?.fullName.toLowerCase().includes(q) ||
          emp?.employeeCode.toLowerCase().includes(q)
        );
      });
    }

    if (attendDeptFilter !== 'all') {
      const dId = parseInt(attendDeptFilter, 10);
      records = records.filter(r => {
        const emp = employees.find(e => e.id === r.employeeId);
        return emp?.departmentId === dId;
      });
    }

    if (attendEmpFilter !== 'all') {
      const eId = parseInt(attendEmpFilter, 10);
      records = records.filter(r => r.employeeId === eId);
    }

    if (attendStatusFilter !== 'all') {
      records = records.filter(r => r.attendanceStatus === attendStatusFilter);
    }

    if (attendMonthFilter !== 'all') {
      const mNum = parseInt(attendMonthFilter, 10);
      records = records.filter(r => {
        const dateObj = new Date(r.workDate);
        return (dateObj.getMonth() + 1) === mNum;
      });
    }

    if (attendYearFilter !== 'all') {
      const yNum = parseInt(attendYearFilter, 10);
      records = records.filter(r => {
        const dateObj = new Date(r.workDate);
        return dateObj.getFullYear() === yNum;
      });
    }

    return records;
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Dynamic Tab titles */}
      {tab === 'contracts' && (
        <div className="flex flex-col">
          <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Quản lý Hợp đồng</h1>
          <p className="text-sm text-zinc-500 mt-1">Lưu trữ các hợp đồng pháp lý của nhân sự</p>
        </div>
      )}
      {tab === 'leaves' && (
        <div className="flex flex-col">
          <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Phê duyệt Nghỉ phép</h1>
          <p className="text-sm text-zinc-500 mt-1">Hệ thống xử lý phê quy hoạch các đơn xin nghỉ phép của nhân sự</p>
        </div>
      )}
      {tab === 'attendance' && (
        <div className="flex flex-col">
          <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Chấm công hàng ngày</h1>
          <p className="text-sm text-zinc-500 mt-1">Dữ liệu chấm công điện tử thời gian thực</p>
        </div>
      )}
      {tab === 'payroll' && (
        <div className="flex justify-between items-center bg-zinc-50 p-6 border rounded-xl shadow-sm">
          <div className="flex flex-col">
            <h1 className="text-3xl font-bold tracking-tight text-zinc-900 text-emerald-900">Chi trả tiền Lương</h1>
            <p className="text-sm text-zinc-500 mt-1">Chốt lương, thưởng, phạt tự động thông qua phần mềm</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={calculatePayroll}
              className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-xl font-bold text-sm transition-all shadow-sm"
            >
              <Calculator className="w-4 h-4" />
              Tính lương tự động
            </button>
            <button
              onClick={lockPayrollAll}
              className="flex items-center gap-2 bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 px-4 py-2.5 rounded-xl font-bold text-sm transition-all"
            >
              <ShieldAlert className="w-4 h-4" />
              Chốt sổ & Khóa lương
            </button>
          </div>
        </div>
      )}

      {/* Render Table accordingly */}
      <div className="bg-white border rounded-xl overflow-hidden shadow-sm">
        {tab === 'contracts' && (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                <th className="px-6 py-4">Mã HĐ</th>
                <th className="px-6 py-4">Nhân viên</th>
                <th className="px-6 py-4">Loại hợp đồng</th>
                <th className="px-6 py-4">Ngày bắt đầu</th>
                <th className="px-6 py-4">Ngày kết thúc</th>
                <th className="px-6 py-4">Mức lương cơ cấu</th>
                <th className="px-6 py-4">Trạng thái</th>
                <th className="px-6 py-4 text-center">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-50 text-sm">
              {contracts.map(c => {
                const emp = employees.find(e => e.id === c.employeeId);
                return (
                  <tr key={c.id} className="hover:bg-zinc-50/50">
                    <td className="px-6 py-4 font-mono font-bold text-zinc-900">{c.contractCode}</td>
                    <td className="px-6 py-4">
                      <div className="font-bold text-zinc-900">{emp?.fullName || 'N/A'}</div>
                      <div className="text-xs text-zinc-400 font-semibold">{emp?.employeeCode}</div>
                    </td>
                    <td className="px-6 py-4 font-semibold text-zinc-700">{c.contractType}</td>
                    <td className="px-6 py-4 text-zinc-500 font-mono font-semibold">
                      {new Date(c.startDate).toLocaleDateString('vi-VN')}
                    </td>
                    <td className="px-6 py-4 text-zinc-500 font-mono font-semibold">
                      {c.endDate ? new Date(c.endDate).toLocaleDateString('vi-VN') : 'Dài hạn'}
                    </td>
                    <td className="px-6 py-4 font-mono font-bold text-zinc-900">
                      {c.salary.toLocaleString('vi-VN')} đ
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${
                        c.status === 'Còn hiệu lực' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' :
                        c.status === 'Sắp hết hạn' ? 'bg-amber-50 text-amber-700 border border-amber-100' :
                        'bg-red-50 text-red-705'
                      }`}>
                        {c.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-center">
                      <button
                        onClick={() => emp && handleOpenContractModal(emp, c)}
                        className="bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-250 px-3 py-1.5 rounded-lg text-xs font-bold transition-all shadow-sm flex items-center gap-1.5 mx-auto outline-none"
                      >
                        <FileText className="w-3.5 h-3.5" />
                        Xuất HĐ
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}

        {tab === 'leaves' && (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                <th className="px-6 py-4">Nhân viên</th>
                <th className="px-6 py-4">Phòng xin nghỉ</th>
                <th className="px-6 py-4">Từ ngày - Đến ngày</th>
                <th className="px-6 py-4">Số lượng ngày</th>
                <th className="px-6 py-4">Lý do xin phép</th>
                <th className="px-6 py-4">Trạng thái</th>
                <th className="px-6 py-4 text-right">Lãnh đạo phê duyệt</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-50 text-sm">
              {leaveRequests.map(l => {
                const emp = employees.find(e => e.id === l.employeeId);
                return (
                  <tr key={l.id} className="hover:bg-zinc-50/50">
                    <td className="px-6 py-4">
                      <div className="font-bold text-zinc-900">{emp?.fullName || 'N/A'}</div>
                      <div className="text-xs text-zinc-400 font-semibold">{emp?.employeeCode}</div>
                    </td>
                    <td className="px-6 py-4 font-bold text-zinc-500">{l.leaveType}</td>
                    <td className="px-6 py-4 text-zinc-650 font-mono font-semibold">
                      {new Date(l.fromDate).toLocaleDateString('vi-VN')} ➜ {new Date(l.toDate).toLocaleDateString('vi-VN')}
                    </td>
                    <td className="px-6 py-4 font-bold text-zinc-800">{l.totalDays} ngày</td>
                    <td className="px-6 py-4 text-zinc-500 italic max-w-[200px] truncate">"{l.reason}"</td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${
                        l.status === 'Approved' || l.status === 'Đã duyệt' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' :
                        l.status === 'Từ chối' ? 'bg-red-50 text-red-650' : 
                        'bg-zinc-100 text-zinc-600'
                      }`}>
                        {l.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      {l.status === 'Chờ duyệt' ? (
                        <div className="flex gap-2 justify-end">
                          <button
                            onClick={() => handleApproveLeave(l.id)}
                            className="bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 px-3 py-1.5 rounded-lg text-xs font-bold transition-all"
                          >
                            Duyệt
                          </button>
                          <button
                            onClick={() => handleRejectLeave(l.id)}
                            className="bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 px-3 py-1.5 rounded-lg text-xs font-bold transition-all"
                          >
                            Từ chối
                          </button>
                        </div>
                      ) : (
                        <span className="text-zinc-400 text-xs font-semibold">Đã đóng quy trình</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}

        {tab === 'attendance' && (
          <div className="p-6 bg-zinc-50 border-t space-y-6">
            
            {/* 1. Horizontal Filter Dashboard */}
            <div className="bg-white p-4 rounded-2xl border border-zinc-200 shadow-sm flex flex-wrap gap-4 items-center justify-between">
              <div className="flex flex-wrap gap-3 items-center">
                {/* Search Text */}
                <div className="relative">
                  <Search className="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
                  <input
                    type="text"
                    placeholder="Tìm mã, tên nhân viên..."
                    value={attendSearch}
                    onChange={e => setAttendSearch(e.target.value)}
                    className="pl-9 pr-4 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs w-52 focus:ring-2 focus:ring-blue-500/20 outline-none font-bold"
                  />
                </div>

                {/* Dept Filter */}
                <select
                  value={attendDeptFilter}
                  onChange={e => setAttendDeptFilter(e.target.value)}
                  className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-bold outline-none"
                >
                  <option value="all">Sở ban: Tất cả</option>
                  {departments.map(d => (
                    <option key={d.id} value={d.id}>{d.departmentName}</option>
                  ))}
                </select>

                {/* Employee Filter */}
                <select
                  value={attendEmpFilter}
                  onChange={e => setAttendEmpFilter(e.target.value)}
                  className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-bold outline-none w-44"
                >
                  <option value="all">Nhân sự: Tất cả</option>
                  {employees.filter(e => !e.isDeleted).map(e => (
                    <option key={e.id} value={e.id}>{e.fullName} ({e.employeeCode})</option>
                  ))}
                </select>

                {/* Month Filter */}
                <select
                  value={attendMonthFilter}
                  onChange={e => setAttendMonthFilter(e.target.value)}
                  className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-bold outline-none"
                >
                  <option value="all">Tháng: Tất cả</option>
                  {['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'].map(m => (
                    <option key={m} value={m}>{`Tháng ${m}`}</option>
                  ))}
                </select>

                {/* Year Filter */}
                <select
                  value={attendYearFilter}
                  onChange={e => setAttendYearFilter(e.target.value)}
                  className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-bold outline-none"
                >
                  <option value="all">Năm: Tất cả</option>
                  {['2024', '2025', '2026', '2027'].map(y => (
                    <option key={y} value={y}>{`Năm ${y}`}</option>
                  ))}
                </select>

                {/* Status Filter */}
                <select
                  value={attendStatusFilter}
                  onChange={e => setAttendStatusFilter(e.target.value)}
                  className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-bold outline-none"
                >
                  <option value="all">Trạng thái: Tất cả</option>
                  {['Đủ công', 'Đi muộn', 'Về sớm', 'Nghỉ phép', 'Nghỉ không phép'].map(st => (
                    <option key={st} value={st}>{st}</option>
                  ))}
                </select>
              </div>

              {/* Reset Filters */}
              <button
                onClick={() => {
                  setAttendSearch('');
                  setAttendDeptFilter('all');
                  setAttendEmpFilter('all');
                  setAttendStatusFilter('all');
                  setAttendMonthFilter('all');
                  setAttendYearFilter('all');
                }}
                className="px-4 py-2 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 text-xs font-black rounded-xl transition-all uppercase tracking-wider"
              >
                Đặt lại lọc
              </button>
            </div>

            {/* 2. Double-Column Layout */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
              
              {/* Left Column: List of records (7 cols) */}
              <div className="lg:col-span-7 bg-white p-4 rounded-2xl border border-zinc-200 shadow-sm space-y-4">
                <div className="overflow-x-auto">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-zinc-50/70 border-b border-zinc-150 text-xs font-black tracking-wider text-zinc-400 uppercase">
                        <th className="px-3 py-3">Nhân sự</th>
                        <th className="px-3 py-3">Ngày công</th>
                        <th className="px-3 py-3">Vào/Ra</th>
                        <th className="px-3 py-3">Giờ làm</th>
                        <th className="px-3 py-3">Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-50 text-xs">
                      {getFilteredAttendanceRecords().length === 0 ? (
                        <tr>
                          <td colSpan={5} className="text-center py-10 font-bold text-zinc-400">
                            Chưa có dữ liệu chấm công thỏa mãn bộ lọc.
                          </td>
                        </tr>
                      ) : (
                        getFilteredAttendanceRecords().map(a => {
                          const emp = employees.find(e => e.id === a.employeeId);
                          const dept = departments.find(d => d.id === emp?.departmentId);
                          const isSelected = selectedAttend?.id === a.id;
                          return (
                            <tr
                              key={a.id}
                              onClick={() => handleSelectAttend(a)}
                              className={`cursor-pointer transition-all duration-150 ${
                                isSelected ? 'bg-blue-500/10 hover:bg-blue-500/15' : 'hover:bg-zinc-50'
                              }`}
                            >
                              <td className="px-3 py-3.5">
                                <div className="font-extrabold text-zinc-800">{emp?.fullName || 'N/A'}</div>
                                <div className="text-[10px] text-zinc-400 font-bold">{emp?.employeeCode} • {dept?.departmentName || 'Không có ban'}</div>
                              </td>
                              <td className="px-3 py-3.5 font-bold text-zinc-500 font-mono">
                                {new Date(a.workDate).toLocaleDateString('vi-VN')}
                              </td>
                              <td className="px-3 py-3.5 font-bold text-zinc-600 font-mono">
                                {a.checkInTime || '--:--'} → {a.checkOutTime || '--:--'}
                              </td>
                              <td className="px-3 py-3.5 font-extrabold text-blue-600">
                                {a.workHours}h
                              </td>
                              <td className="px-3 py-3.5">
                                <span className={`px-2.5 py-1 text-[10px] font-black rounded-full border ${
                                  a.attendanceStatus === 'Đủ công' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                                  a.attendanceStatus === 'Đi muộn' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                                  a.attendanceStatus === 'Về sớm' ? 'bg-orange-50 text-orange-700 border-orange-100' :
                                  a.attendanceStatus === 'Nghỉ phép' ? 'bg-blue-50 text-blue-700 border-blue-100' :
                                  'bg-red-50 text-red-700 border-red-100'
                                }`}>
                                  {a.attendanceStatus}
                                </span>
                              </td>
                            </tr>
                          );
                        })
                      )}
                    </tbody>
                  </table>
                </div>
                <div className="text-[10px] text-zinc-400 font-bold italic">
                  * Nhấp chọn một dòng tương ứng để xem chi tiết, và hiển thị các quyền Thêm/Sửa/Xóa tùy thuộc vào vai trò.
                </div>
              </div>

              {/* Right Column: Detailed Form Panel (5 cols) */}
              <div className="lg:col-span-5 bg-white p-5 rounded-2xl border border-zinc-200 shadow-sm space-y-4">
                <h3 className="text-sm font-black text-zinc-800 uppercase tracking-wider flex items-center gap-2">
                  <span>📝 Chi tiết & Chỉnh sửa</span>
                  {selectedAttend && (
                    <span className="bg-amber-50 text-amber-800 border-amber-100 text-[10px] px-2 py-0.5 rounded-full border">ĐANG CHỌN</span>
                  )}
                </h3>

                {/* Notifications Alert banner */}
                {attendError && (
                  <div className="bg-red-50 text-red-800 border border-red-200 rounded-xl p-3.5 text-xs font-bold leading-relaxed flex items-start gap-2">
                    <span className="text-base text-red-650">⚠</span>
                    <span>{attendError}</span>
                  </div>
                )}
                {attendSuccess && (
                  <div className="bg-emerald-50 text-emerald-850 border border-emerald-200 rounded-xl p-3.5 text-xs font-bold leading-relaxed flex items-start gap-2">
                    <span className="text-base text-emerald-600">✔</span>
                    <span>{attendSuccess}</span>
                  </div>
                )}

                {/* Inputs Fields */}
                <div className="space-y-3 text-xs font-bold text-zinc-600">
                  
                  {/* Select Employee */}
                  <div>
                    <label className="block mb-1">Nhân viên *</label>
                    <select
                      value={formEmpId}
                      onChange={e => setFormEmpId(e.target.value)}
                      disabled={currentUser?.role === 'Employee'}
                      className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-bold"
                    >
                      <option value="">-- Chọn nhân viên --</option>
                      {employees.filter(e => !e.isDeleted).map(e => (
                        <option key={e.id} value={e.id}>{`[${e.employeeCode}] - ${e.fullName}`}</option>
                      ))}
                    </select>
                  </div>

                  {/* Date selection */}
                  <div>
                    <label className="block mb-1">Ngày chấm công *</label>
                    <input
                      type="date"
                      value={formDate}
                      onChange={e => setFormDate(e.target.value)}
                      disabled={currentUser?.role === 'Employee'}
                      className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-bold text-zinc-800"
                    />
                  </div>

                  {/* Times input */}
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block mb-1">Giờ vào (Check-In)</label>
                      <input
                        type="text"
                        placeholder="08:00"
                        value={formIn}
                        onChange={e => setFormIn(e.target.value)}
                        disabled={currentUser?.role === 'Employee' || formStatus === 'Nghỉ phép'}
                        className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-mono text-zinc-800"
                      />
                    </div>
                    <div>
                      <label className="block mb-1">Giờ ra (Check-Out)</label>
                      <input
                        type="text"
                        placeholder="17:00"
                        value={formOut}
                        disabled={currentUser?.role === 'Employee' || formStatus === 'Nghỉ phép'}
                        onChange={e => setFormOut(e.target.value)}
                        className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-mono text-zinc-800"
                      />
                    </div>
                  </div>

                  {/* Work Hours and Calculator */}
                  <div className="flex items-end gap-2.5">
                    <div className="flex-1">
                      <label className="block mb-1">Số giờ làm việc (Tự tính)</label>
                      <input
                        type="number"
                        readOnly
                        value={formHours}
                        className="w-full px-3 py-2 bg-zinc-150 border border-zinc-200 rounded-xl font-extrabold text-blue-600 outline-none"
                      />
                    </div>
                    {currentUser?.role !== 'Employee' && (
                      <button
                        onClick={runCalculate}
                        className="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-black rounded-xl text-xs transition-all uppercase tracking-wider"
                      >
                        Tính giờ
                      </button>
                    )}
                  </div>

                  {/* Status select */}
                  <div>
                    <label className="block mb-1">Trạng thái chấm công</label>
                    <select
                      value={formStatus}
                      onChange={e => setFormStatus(e.target.value)}
                      disabled={currentUser?.role === 'Employee'}
                      className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-bold"
                    >
                      {['Đủ công', 'Đi muộn', 'Về sớm', 'Nghỉ phép', 'Nghỉ không phép'].map(opt => (
                        <option key={opt} value={opt}>{opt}</option>
                      ))}
                    </select>
                  </div>

                  {/* Ghi chú */}
                  <div>
                    <label className="block mb-1">Ghi chú</label>
                    <textarea
                      placeholder="Ghi nhận lý do, đi muộn, hoặc lý do công tác phép ngoại cảnh..."
                      value={formNote}
                      onChange={e => setFormNote(e.target.value)}
                      disabled={currentUser?.role === 'Employee'}
                      className="w-full px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl outline-none font-bold"
                      rows={2}
                    />
                  </div>

                </div>

                {/* Submitting Buttons Block */}
                <div className="pt-3 border-t">
                  {currentUser?.role !== 'Employee' ? (
                    <div className="grid grid-cols-2 gap-2 text-xs font-black">
                      <button
                        onClick={handleAddAttend}
                        className="px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl transition-all shadow-md active:scale-95"
                      >
                        ➕ THÊM CÔNG
                      </button>
                      <button
                        onClick={handleUpdateAttend}
                        disabled={!selectedAttend}
                        className={`px-4 py-3 text-white rounded-xl transition-all shadow-md active:scale-95 flex items-center justify-center gap-1.5 ${
                          selectedAttend ? 'bg-amber-600 hover:bg-amber-700' : 'bg-zinc-300 opacity-50 cursor-not-allowed'
                        }`}
                      >
                        💾 CẬP NHẬT
                      </button>
                      <button
                        onClick={handleDeleteAttend}
                        disabled={!selectedAttend}
                        className={`px-4 py-3 border rounded-xl transition-all active:scale-95 flex items-center justify-center gap-1.5 ${
                          selectedAttend ? 'bg-red-50 text-red-600 hover:bg-red-150 border-red-200' : 'bg-zinc-50 text-zinc-300 border-zinc-150 cursor-not-allowed'
                        }`}
                      >
                        🗑 XÓA CÔNG
                      </button>
                      <button
                        onClick={resetAttendForm}
                        className="px-4 py-3 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 rounded-xl transition-all border border-zinc-250/20"
                      >
                        🧹 LÀM TRẮNG
                      </button>
                    </div>
                  ) : (
                    <div className="bg-amber-50 border border-amber-200 p-3 rounded-xl text-xs font-bold leading-relaxed text-amber-800">
                      Tài khoản của bạn chỉ có quyền XEM bảng chấm công cá nhân, không thể quản lý/chỉnh sửa bảng công.
                    </div>
                  )}
                </div>

              </div>

            </div>

          </div>
        )}

        {tab === 'payroll' && (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                <th className="px-6 py-4">Nhân sự</th>
                <th className="px-6 py-4">Mức lương cơ bản</th>
                <th className="px-6 py-4">Công tiêu chuẩn</th>
                <th className="px-6 py-4">Công thực tế</th>
                <th className="px-6 py-4">Tiền thưởng (+)</th>
                <th className="px-6 py-4">Khấu trừ (-)</th>
                <th className="px-6 py-4">Lương Thực nhận (NET)</th>
                <th className="px-6 py-4">Sổ cái</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-50 text-sm">
              {employees.filter(e => !e.isDeleted).map(emp => {
                const pay = payrolls.find(p => p.employeeId === emp.id);

                return (
                  <tr key={emp.id} className="hover:bg-zinc-50/50">
                    <td className="px-6 py-4">
                      <div className="font-bold text-zinc-900">{emp.fullName}</div>
                      <div className="text-xs text-emerald-600 font-bold">{emp.employeeCode}</div>
                    </td>
                    <td className="px-6 py-4 font-mono font-bold text-zinc-500">
                      {emp.baseSalary.toLocaleString('vi-VN')} đ
                    </td>
                    <td className="px-6 py-4 font-mono font-bold text-zinc-400">22 ngày</td>
                    <td className="px-6 py-4 font-mono font-bold text-zinc-800">
                      {pay ? pay.actualWorkDays : (emp.workStatus === 'Chính thức' ? 22 : 20)} ngày
                    </td>
                    <td className="px-6 py-4 font-mono">
                      {pay?.isLocked ? (
                        <span className="font-bold text-emerald-700">+{pay.bonus.toLocaleString('vi-VN')} đ</span>
                      ) : (
                        <input
                          type="number"
                          placeholder="Thưởng..."
                          value={bonusInput[emp.id] || ''}
                          onChange={e => setBonusInput(prev => ({ ...prev, [emp.id]: Number(e.target.value) }))}
                          className="w-24 px-2 py-1 bg-zinc-50 border border-zinc-200 rounded-lg text-xs"
                        />
                      )}
                    </td>
                    <td className="px-6 py-4 font-mono">
                      {pay?.isLocked ? (
                        <span className="font-bold text-rose-700">-{pay.penalty.toLocaleString('vi-VN')} đ</span>
                      ) : (
                        <input
                          type="number"
                          placeholder="Phạt..."
                          value={penaltyInput[emp.id] || ''}
                          onChange={e => setPenaltyInput(prev => ({ ...prev, [emp.id]: Number(e.target.value) }))}
                          className="w-24 px-2 py-1 bg-zinc-50 border border-zinc-200 rounded-lg text-xs"
                        />
                      )}
                    </td>
                    <td className="px-6 py-4 font-mono font-black text-emerald-800">
                      {pay ? `${pay.netSalary.toLocaleString('vi-VN')} đ` : `${emp.baseSalary.toLocaleString('vi-VN')} đ`}
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${
                        pay?.isLocked ? 'bg-rose-50 text-rose-700 border border-rose-100' : 'bg-zinc-100 text-zinc-600'
                      }`}>
                        {pay?.isLocked ? 'ĐÃ KHÓA' : 'TỰ DO'}
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {selectedContractEmp && (
        <ExportContractModal
          isOpen={isContractModalOpen}
          onClose={() => {
            setIsContractModalOpen(false);
            setSelectedContractEmp(null);
            setSelectedContract(null);
          }}
          employee={selectedContractEmp}
          contract={selectedContract || undefined}
        />
      )}
    </div>
  );
}
