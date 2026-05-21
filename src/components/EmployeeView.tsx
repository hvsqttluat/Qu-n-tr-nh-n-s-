import React, { useState } from 'react';
import { Employee, Department, Position, User, Contract, LeaveRequest, AttendanceRecord } from '../types';
import { Plus, Edit, Trash2, Search, Save, X, AlertCircle, FileText, Eye, Printer, Download, FileSpreadsheet, Presentation } from 'lucide-react';
import { ExportContractModal } from './ExportContractModal';
import { exportToExcel, exportToWord, exportToPPTX, triggerPrintSelection } from '../utils/exportUtils';

interface EmployeeViewProps {
  employees: Employee[];
  setEmployees: React.Dispatch<React.SetStateAction<Employee[]>>;
  departments: Department[];
  positions: Position[];
  addLog: (action: string, table: string, desc: string) => void;
  currentUser: User;
  contracts: Contract[];
  leaveRequests: LeaveRequest[];
  attendanceRecords: AttendanceRecord[];
}

export function EmployeeView({
  employees,
  setEmployees,
  departments,
  positions,
  addLog,
  currentUser,
  contracts,
  leaveRequests,
  attendanceRecords
}: EmployeeViewProps) {
  const [selectedEmp, setSelectedEmp] = useState<Employee | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editingEmp, setEditingEmp] = useState<Partial<Employee>>({});
  const [errorMsg, setErrorMsg] = useState('');
  const [isContractModalOpen, setIsContractModalOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);

  // Search & Filters
  const [searchText, setSearchText] = useState('');
  const [filterDeptId, setFilterDeptId] = useState<number | 'all'>('all');
  const [filterStatus, setFilterStatus] = useState<string | 'all'>('all');

  const statusList = ['Thử việc', 'Chính thức', 'Tạm nghỉ', 'Đã nghỉ'];
  const activeDepts = departments.filter(d => !d.isDeleted && d.isActive);
  const activePositions = positions.filter(p => !p.isDeleted && p.isActive);

  // Active employees list
  const activeEmployees = employees.filter(e => !e.isDeleted);

  // Apply filters
  const filteredEmployees = activeEmployees.filter(e => {
    const matchesSearch = searchText ? (
      e.fullName.toLowerCase().includes(searchText.toLowerCase()) ||
      e.employeeCode.toLowerCase().includes(searchText.toLowerCase()) ||
      (e.phone && e.phone.includes(searchText)) ||
      (e.email && e.email.toLowerCase().includes(searchText.toLowerCase()))
    ) : true;

    const matchesDept = filterDeptId !== 'all' ? e.departmentId === Number(filterDeptId) : true;
    const matchesStatus = filterStatus !== 'all' ? e.workStatus === filterStatus : true;

    return matchesSearch && matchesDept && matchesStatus;
  });

  const handleExportExcel = () => {
    const title = "Danh sách Nhân viên - Công ty HRM WPF CNPM";
    const headers = [
      { header: 'Mã Nhân Viên', key: 'employeeCode', width: 15 },
      { header: 'Họ và Tên', key: 'fullName', width: 25 },
      { header: 'Giới Tính', key: 'gender', width: 10 },
      { header: 'Ngày Sinh', key: 'dateOfBirth', width: 15 },
      { header: 'CCCD/CMND', key: 'citizenId', width: 18 },
      { header: 'Số Điện Thoại', key: 'phone', width: 15 },
      { header: 'Email', key: 'email', width: 25 },
      { header: 'Địa Chỉ', key: 'address', width: 30 },
      { header: 'Lương Cơ Bản', key: 'baseSalary', width: 15 },
      { header: 'Trạng Thái', key: 'workStatus', width: 15 },
      { header: 'Ngày Vào Làm', key: 'joinDate', width: 15 }
    ];

    const data = filteredEmployees.map(e => {
      const dept = departments.find(d => d.id === e.departmentId);
      const pos = positions.find(p => p.id === e.positionId);
      return {
        ...e,
        departmentName: dept?.departmentName || 'N/A',
        positionName: pos?.positionName || 'N/A'
      };
    });

    const excelHeaders = [
      ...headers.slice(0, 2),
      { header: 'Phòng Ban', key: 'departmentName', width: 20 },
      { header: 'Chức Vụ', key: 'positionName', width: 20 },
      ...headers.slice(2)
    ];

    exportToExcel(title, excelHeaders, data, 'Danh_Sach_Nhan_Vien_HRM_CNPM.xlsx');
    addLog('Xuất Excel', 'Employees', 'Đã xuất danh sách nhân sự trực tuyến ra tệp Excel');
  };

  const handleExportWord = () => {
    const title = "Báo cáo Danh sách Nhân sự Tổng bộ Sư đoàn";
    const headers = ['Mã NV', 'Họ & Tên', 'Phòng Ban', 'Chức Vụ', 'Trạng Thái', 'Ngày Gia Nhập'];
    
    const rows = filteredEmployees.map(e => {
      const dept = departments.find(d => d.id === e.departmentId);
      const pos = positions.find(p => p.id === e.positionId);
      return [
        e.employeeCode,
        e.fullName,
        dept?.departmentName || 'N/A',
        pos?.positionName || 'N/A',
        e.workStatus,
        e.joinDate ? new Date(e.joinDate).toLocaleDateString('vi-VN') : 'N/A'
      ];
    });

    const summaryText = `Tổng số nhân sự hoạt động trong diện lọc hiện hành: ${filteredEmployees.length} cán bộ nhân viên. Cơ cấu phân bổ đồng bộ trực tiếp từ cơ sở dữ liệu.`;
    
    exportToWord(title, headers, rows, 'Danh_Sach_Nhan_Vien_HRM_CNPM.docx', summaryText);
    addLog('Xuất Word', 'Employees', 'Đã hoàn tất tải xuống báo cáo Word của danh sách nhân viên');
  };

  const handleExportPPTX = () => {
    const title = "Báo cáo Thực trạng Nhân lực & Quân chế Sư đoàn HRM WPF CNPM";
    
    const deptStats = departments.map(d => {
      const count = employees.filter(e => e.departmentId === d.id && !e.isDeleted).length;
      return `${d.departmentName}: ${count} nhân sự`;
    });

    const statusStats = statusList.map(s => {
      const count = employees.filter(e => e.workStatus === s && !e.isDeleted).length;
      return `- Trạng thái [${s}]: ${count} nhân sự`;
    });

    const listSummary = filteredEmployees.slice(0, 5).map(e => {
      const dept = departments.find(d => d.id === e.departmentId);
      return `- ${e.fullName} (${e.employeeCode}) - ${dept?.departmentName || 'N/A'}`;
    });

    const slides = [
      {
        title: "Tổ chức Bộ máy & Cơ cấu Nhân sự",
        subtitle: "Cơ cấu nhân sự theo bộ phận phòng ban lập biểu",
        content: [
          `Tổng nhân sự hiện hữu: ${employees.filter(e => !e.isDeleted).length} nhân sự toàn chuỗi`,
          `Phân bổ cụ thể qua các phòng ban chức năng:`,
          ...deptStats.slice(0, 5)
        ]
      },
      {
        title: "Trạng thái tuyển dụng & Kế hoạch nhân sự chính thức",
        subtitle: "Thống kê tình trạng quản lý",
        content: [
          `Tỉ lệ phân bổ năng lực chính thức/thử việc:`,
          ...statusStats,
          `Định hướng: Nâng cao chất lượng nhân sự, tối ưu hiệu năng làm việc đồng bộ hóa dữ liệu ERP.`
        ]
      },
      {
        title: "Danh sách Nhân sự tiêu biểu mới tinh giản",
        subtitle: "Trích xuất danh sách hiện hành (Tối đa 5 hiển thị mẫu)",
        content: [
          `Một số hồ sơ nhân sự trong diện phân vùng truy xuất:`,
          ...listSummary,
          filteredEmployees.length > 5 ? `...và các cán bộ nhân sự khác (Tổng cộng ${filteredEmployees.length} nhân sự).` : "Hết danh sách truy xuất."
        ]
      }
    ];

    exportToPPTX(title, slides, 'Bao_Cao_Nhan_Su_HRM_CNPM.pptx');
    addLog('Xuất PowerPoint', 'Employees', 'Kết xuất đề án thuyết trình thuyết minh nhân sự .pptx thành công');
  };

  const handlePrintTable = () => {
    triggerPrintSelection('employee-table-printable', 'Báo Cáo Danh Sách Nhân Sự Trực Tuyến');
    addLog('In Ấn', 'Employees', 'Đã gọi lệnh In bảng danh sách nhân viên trực quan');
  };

  const startAdd = () => {
    setEditingEmp({
      id: 0,
      employeeCode: '',
      fullName: '',
      gender: 'Nam',
      dateOfBirth: '1995-01-01',
      citizenId: '',
      phone: '',
      email: '',
      address: '',
      departmentId: activeDepts[0]?.id || 0,
      positionId: activePositions[0]?.id || 0,
      joinDate: new Date().toISOString().split('T')[0],
      workStatus: 'Thử việc',
      baseSalary: 10000000,
      note: ''
    });
    setIsEditing(true);
    setSelectedEmp(null);
    setErrorMsg('');
  };

  const startEdit = () => {
    if (!selectedEmp) return;
    setEditingEmp({ ...selectedEmp });
    setIsEditing(true);
    setErrorMsg('');
  };

  const handleSave = () => {
    if (!editingEmp.employeeCode || !editingEmp.fullName) {
      setErrorMsg('Mã nhân viên và Họ tên không được để trống.');
      return;
    }

    const isDuplicate = employees.some(e =>
      e.employeeCode.toUpperCase() === editingEmp.employeeCode?.toUpperCase() &&
      e.id !== editingEmp.id &&
      !e.isDeleted
    );

    if (isDuplicate) {
      setErrorMsg('Mã nhân viên đã tồn tại trên định dạng hệ thống!');
      return;
    }

    if (editingEmp.id === 0) {
      // Add
      const newId = employees.length > 0 ? Math.max(...employees.map(e => e.id)) + 1 : 1;
      const newRecord: Employee = {
        id: newId,
        employeeCode: editingEmp.employeeCode.toUpperCase(),
        fullName: editingEmp.fullName,
        gender: editingEmp.gender || 'Nam',
        dateOfBirth: editingEmp.dateOfBirth,
        citizenId: editingEmp.citizenId || '',
        phone: editingEmp.phone || '',
        email: editingEmp.email || '',
        address: editingEmp.address || '',
        departmentId: Number(editingEmp.departmentId),
        positionId: Number(editingEmp.positionId),
        joinDate: editingEmp.joinDate || new Date().toISOString().split('T')[0],
        workStatus: editingEmp.workStatus || 'Thử việc',
        baseSalary: Number(editingEmp.baseSalary || 10000000),
        note: editingEmp.note || '',
        isDeleted: false
      };
      setEmployees(prev => [...prev, newRecord]);
      addLog('Thêm nhân sự', 'Employees', `Đã kiến tạo hồ sơ của: ${newRecord.fullName} (${newRecord.employeeCode})`);
    } else {
      // Update
      setEmployees(prev => prev.map(e => e.id === editingEmp.id ? { ...e, ...editingEmp as Employee, employeeCode: editingEmp.employeeCode!.toUpperCase() } : e));
      addLog('Cập nhật nhân sự', 'Employees', `Thay đổi hồ sơ của: ${editingEmp.fullName}`);
    }

    setIsEditing(false);
    setSelectedEmp(null);
    setErrorMsg('');
  };

  const handleDelete = () => {
    if (!selectedEmp) return;
    if (window.confirm(`Bạn có chắc muốn xóa nhân viên ${selectedEmp.fullName} không?`)) {
      setEmployees(prev => prev.map(e => e.id === selectedEmp.id ? { ...e, isDeleted: true } : e));
      addLog('Xóa nhân sự', 'Employees', `Đã xóa mềm hồ sơ của: ${selectedEmp.fullName}`);
      setSelectedEmp(null);
      setIsEditing(false);
    }
  };

  if (isDetailOpen && selectedEmp) {
    const dept = departments.find(d => d.id === selectedEmp.departmentId);
    const pos = positions.find(p => p.id === selectedEmp.positionId);
    const empContracts = contracts.filter(c => c.employeeId === selectedEmp.id);
    const empLeaves = leaveRequests.filter(l => l.employeeId === selectedEmp.id);
    const empAttendance = attendanceRecords.filter(a => a.employeeId === selectedEmp.id);

    const isHighRole = currentUser.role === 'Admin' || 
                       currentUser.role === 'HR' || 
                       currentUser.role === 'Giám đốc' || 
                       currentUser.role === 'Kế toán' ||
                       currentUser.role === 'Thư ký';
    const isSelf = currentUser.employeeId !== undefined && currentUser.employeeId === selectedEmp.id;
    const canViewSalary = isHighRole || isSelf;

    return (
      <div className="space-y-6 animate-fade-in">
        {/* Detail view layout modeled after EmployeeDetailView.xaml */}
        <div className="flex items-center justify-between border-b pb-4 border-zinc-200">
          <div>
            <button
              onClick={() => setIsDetailOpen(false)}
              className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-xl font-bold text-sm transition-all shadow-sm outline-none"
            >
              ← QUAY LẠI DANH SÁCH
            </button>
          </div>
          <div className="text-right">
            <h1 className="text-2xl font-black text-zinc-900 uppercase">Chi tiết hồ sơ nhân sự</h1>
            <p className="text-xs text-zinc-500 font-bold">Mã nhân viên: <span className="text-lg text-blue-600 font-mono ml-1">{selectedEmp.employeeCode}</span></p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
          {/* Left profile badge */}
          <div className="lg:col-span-4 bg-white border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col items-center">
            <div className="w-24 h-24 rounded-full bg-blue-50 border-2 border-blue-500 flex items-center justify-center text-3xl font-black text-blue-600 shadow-sm mb-4 animate-float">
              HRM
            </div>
            <h2 className="text-xl font-black text-zinc-900">{selectedEmp.fullName}</h2>
            <p className="text-xs text-zinc-400 font-mono font-bold mt-1">{selectedEmp.employeeCode}</p>
            <span className="mt-3 px-3 py-1 bg-emerald-50 border border-emerald-100 text-emerald-700 font-black text-xs rounded-full">
              {selectedEmp.workStatus}
            </span>

            <div className="w-full border-t border-zinc-100 my-6"></div>

            <div className="w-full space-y-4 text-sm">
              <div>
                <span className="block text-[10px] font-black uppercase text-zinc-400">Bộ phận</span>
                <span className="font-bold text-zinc-800">{dept?.departmentName || 'N/A'}</span>
              </div>
              <div>
                <span className="block text-[10px] font-black uppercase text-zinc-400">Chức vụ</span>
                <span className="font-bold text-zinc-800">{pos?.positionName || 'N/A'}</span>
              </div>
              <div>
                <span className="block text-[10px] font-black uppercase text-zinc-400">Văn phòng làm việc</span>
                <span className="font-bold text-zinc-500 italic">Trụ sở điều hành chính - NexusHQ</span>
              </div>
            </div>
          </div>

          {/* Right main sections */}
          <div className="lg:col-span-8 space-y-6">
            {/* Info Grid Card */}
            <div className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm">
              <h3 className="text-sm font-black text-zinc-800 uppercase tracking-widest border-b pb-3 mb-4 flex items-center gap-2">
                📂 THÔNG TIN CÁ NHÂN &amp; NGHIỆP VỤ CƠ BẢN
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">Ngày sinh</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.dateOfBirth ? new Date(selectedEmp.dateOfBirth).toLocaleDateString('vi-VN') : '15/08/1995'}</span>
                </div>
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">Giới tính</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.gender}</span>
                </div>
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">Số CCCD</span>
                  <span className="font-mono font-bold text-zinc-800">{selectedEmp.citizenId || '037095000452'}</span>
                </div>
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">SĐT liên lạc</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.phone || 'N/A'}</span>
                </div>
                <div className="md:col-span-2">
                  <span className="text-xs text-zinc-400 font-bold block">Email liên hệ</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.email || 'N/A'}</span>
                </div>
                <div className="md:col-span-2">
                  <span className="text-xs text-zinc-400 font-bold block">Địa chỉ cư trú</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.address || 'N/A'}</span>
                </div>
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">Ngày chính thức gia nhập</span>
                  <span className="font-bold text-zinc-800">{selectedEmp.joinDate ? new Date(selectedEmp.joinDate).toLocaleDateString('vi-VN') : 'N/A'}</span>
                </div>
                <div>
                  <span className="text-xs text-zinc-400 font-bold block">Hạn mức lương cơ cấu</span>
                  <span className={`font-mono text-sm font-black ${canViewSalary ? 'text-red-650' : 'text-zinc-450 italic'}`}>
                    {canViewSalary ? `${selectedEmp.baseSalary.toLocaleString('vi-VN')} VNĐ` : '****** (Không có quyền xem)'}
                  </span>
                </div>
              </div>
            </div>

            {/* List Contracts Card */}
            <div className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm">
              <h3 className="text-sm font-black text-zinc-800 uppercase tracking-widest border-b pb-3 mb-4">
                📜 DANH SÁCH HỢP ĐỒNG LAO ĐỘNG HÀNH CHÍNH
              </h3>
              {empContracts.length === 0 ? (
                <p className="text-sm text-zinc-400 italic">Chưa có dữ liệu hợp đồng cho nhân sự này.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm border-collapse">
                    <thead>
                      <tr className="bg-zinc-50 border-b border-zinc-100 text-xs text-zinc-400 uppercase font-black">
                        <th className="px-4 py-2">Mã HĐ</th>
                        <th className="px-4 py-2">Loại HĐ</th>
                        <th className="px-4 py-2">Ngày bắt đầu</th>
                        <th className="px-4 py-2">Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-50">
                      {empContracts.map(c => (
                        <tr key={c.id}>
                          <td className="px-4 py-3 font-mono font-bold text-zinc-900">{c.contractCode}</td>
                          <td className="px-4 py-3 font-semibold text-zinc-700">{c.contractType}</td>
                          <td className="px-4 py-3 font-semibold text-zinc-500">{new Date(c.startDate).toLocaleDateString('vi-VN')}</td>
                          <td className="px-4 py-3">
                            <span className="px-2 py-0.5 text-xs font-bold rounded-full bg-emerald-50 text-emerald-700 border border-emerald-100">
                              {c.status}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* List Leave Requests Card */}
            <div className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm">
              <h3 className="text-sm font-black text-zinc-800 uppercase tracking-widest border-b pb-3 mb-4">
                📅 LỊCH TRÌNH ĐƠN NGHỈ PHÉP GẦN ĐÂY
              </h3>
              {empLeaves.length === 0 ? (
                <p className="text-sm text-zinc-400 italic">Chưa có dữ liệu đăng ký nghỉ phép.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm border-collapse">
                    <thead>
                      <tr className="bg-zinc-50 border-b border-zinc-100 text-xs text-zinc-400 uppercase font-black">
                        <th className="px-4 py-2">Loại nghỉ</th>
                        <th className="px-4 py-2">Kéo dài</th>
                        <th className="px-4 py-2">Số ngày</th>
                        <th className="px-4 py-2">Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-50">
                      {empLeaves.map(l => (
                        <tr key={l.id}>
                          <td className="px-4 py-3 font-semibold text-zinc-800">{l.leaveType}</td>
                          <td className="px-4 py-3 font-semibold text-zinc-500">
                            {new Date(l.fromDate).toLocaleDateString('vi-VN')} - {new Date(l.toDate).toLocaleDateString('vi-VN')}
                          </td>
                          <td className="px-4 py-3 font-bold text-zinc-700">{l.totalDays}</td>
                          <td className="px-4 py-3">
                            <span className={`px-2 py-0.5 text-xs font-bold rounded-full ${
                              l.status === 'Đã duyệt' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' :
                              l.status === 'Chờ duyệt' ? 'bg-amber-50 text-amber-700 border border-amber-100' :
                              'bg-zinc-100 text-zinc-650'
                            }`}>
                              {l.status}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* List Attendance Records Card */}
            <div className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm">
              <h3 className="text-sm font-black text-zinc-800 uppercase tracking-widest border-b pb-3 mb-4">
                ⌚ NHẬT KÝ ĐIỂM DANH CHẤM CÔNG TUẦN QUA
              </h3>
              {empAttendance.length === 0 ? (
                <p className="text-sm text-zinc-400 italic">Chưa có dữ liệu lịch sử chấm công.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm border-collapse">
                    <thead>
                      <tr className="bg-zinc-50 border-b border-zinc-100 text-xs text-zinc-400 uppercase font-black">
                        <th className="px-4 py-2">Ngày</th>
                        <th className="px-4 py-2">Giờ vào</th>
                        <th className="px-4 py-2">Giờ ra</th>
                        <th className="px-4 py-2">Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-50">
                      {empAttendance.map(a => (
                        <tr key={a.id}>
                          <td className="px-4 py-3 font-semibold text-zinc-800">{new Date(a.workDate).toLocaleDateString('vi-VN')}</td>
                          <td className="px-4 py-3 font-mono text-zinc-600">{a.checkInTime || '--:--'}</td>
                          <td className="px-4 py-3 font-mono text-zinc-600">{a.checkOutTime || '--:--'}</td>
                          <td className="px-4 py-3">
                            <span className={`px-2 py-0.5 text-[10px] font-black uppercase rounded-full ${
                              a.attendanceStatus === 'Đúng giờ' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' :
                              a.attendanceStatus === 'Đi muộn' ? 'bg-amber-50 text-amber-700 border border-amber-100' :
                              'bg-red-50 text-red-700 border border-red-100'
                            }`}>
                              {a.attendanceStatus}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col">
        <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Quản lý Nhân sự</h1>
        <p className="text-sm text-zinc-500 mt-1">Hồ sơ thông tin chi tiết nhân sự công ty HRM_WPF_CNPM</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-12 gap-6 items-start">
        {/* Table list */}
        <div className={`xl:col-span-${isEditing ? '8' : '12'} bg-white border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6`}>
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
            <div className="flex flex-wrap items-center gap-2 w-full md:w-auto">
              <div className="relative flex-1 sm:flex-initial">
                <Search className="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  placeholder="Tìm mã, tên, SĐT..."
                  value={searchText}
                  onChange={e => setSearchText(e.target.value)}
                  className="pl-9 pr-4 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-sm w-full sm:w-48 focus:ring-2 focus:ring-emerald-500 outline-none transition-all"
                />
              </div>

              <select
                value={filterDeptId}
                onChange={e => setFilterDeptId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-semibold focus:ring-2 focus:ring-emerald-500 outline-none"
              >
                <option value="all">Tất cả phòng ban</option>
                {activeDepts.map(d => (
                  <option key={d.id} value={d.id}>{d.departmentName}</option>
                ))}
              </select>

              <select
                value={filterStatus}
                onChange={e => setFilterStatus(e.target.value)}
                className="px-3 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-xs font-semibold focus:ring-2 focus:ring-emerald-500 outline-none"
              >
                <option value="all">Tất cả trạng thái</option>
                {statusList.map(s => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>

            <div className="flex items-center gap-2 w-full md:w-auto flex-wrap">
              <button
                onClick={startAdd}
                className="flex-1 md:flex-initial flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Plus className="w-4 h-4" />
                Thêm mới
              </button>
              <button
                disabled={!selectedEmp}
                onClick={startEdit}
                className="flex-1 md:flex-initial flex items-center justify-center gap-2 bg-white hover:bg-zinc-50 border text-zinc-700 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Edit className="w-4 h-4" />
                Sửa
              </button>
              <button
                disabled={!selectedEmp}
                onClick={() => setIsDetailOpen(true)}
                className="flex-1 md:flex-initial flex items-center justify-center gap-2 bg-[#1e293b] hover:bg-[#0f172a] text-blue-400 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all shadow-sm outline-none border border-slate-700/30"
              >
                <Eye className="w-4 h-4" />
                Xem chi tiết
              </button>
              <button
                disabled={!selectedEmp}
                onClick={handleDelete}
                className="flex-1 md:flex-initial flex items-center justify-center gap-2 bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Trash2 className="w-4 h-4" />
                Xóa
              </button>
              <button
                disabled={!selectedEmp}
                onClick={() => setIsContractModalOpen(true)}
                className="flex-1 md:flex-initial flex items-center justify-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all shadow-sm outline-none"
              >
                <FileText className="w-4 h-4" />
                Xuất HĐ
              </button>
            </div>
          </div>

          {/* Secondary Report Row */}
          <div className="flex flex-wrap items-center justify-between border-t pt-4 border-zinc-100 gap-4">
            <div className="text-xs font-bold text-zinc-500 uppercase tracking-widest flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-600 animate-ping"></span>
              Xuất báo báo & In ấn (Live):
            </div>
            <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
              <button
                onClick={handlePrintTable}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 bg-zinc-100 hover:bg-zinc-200 text-zinc-800 border border-zinc-300 px-3.5 py-2 rounded-xl font-bold text-xs transition-all shadow-sm outline-none"
              >
                <Printer className="w-3.5 h-3.5 text-zinc-600" />
                In danh sách (LPT)
              </button>
              <button
                onClick={handleExportExcel}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 bg-emerald-50 hover:bg-emerald-100 text-emerald-800 border border-emerald-200 px-3.5 py-2 rounded-xl font-bold text-xs transition-all shadow-sm outline-none"
              >
                <FileSpreadsheet className="w-3.5 h-3.5 text-emerald-700" />
                Xuất Excel (.xlsx)
              </button>
              <button
                onClick={handleExportWord}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 bg-sky-50 hover:bg-sky-100 text-sky-800 border border-sky-200 px-3.5 py-2 rounded-xl font-bold text-xs transition-all shadow-sm outline-none"
              >
                <FileText className="w-3.5 h-3.5 text-sky-700" />
                Xuất Word (.docx)
              </button>
              <button
                onClick={handleExportPPTX}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-1.5 bg-amber-50 hover:bg-amber-100 text-amber-800 border border-amber-200 px-3.5 py-2 rounded-xl font-bold text-xs transition-all shadow-sm outline-none"
              >
                <Presentation className="w-3.5 h-3.5 text-amber-700" />
                Xuất PPTX (.pptx)
              </button>
            </div>
          </div>

          <div className="border border-zinc-100 rounded-xl overflow-x-auto overflow-y-auto max-h-[500px] w-full" id="employee-table-printable">
            <table className="w-full text-left border-collapse min-w-[950px]">
              <thead>
                <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                  <th className="px-6 py-4">Mã NV</th>
                  <th className="px-6 py-4">Họ và Tên</th>
                  <th className="px-6 py-4">Phòng ban</th>
                  <th className="px-6 py-4">Chức vụ</th>
                  <th className="px-6 py-4">Trạng thái</th>
                  <th className="px-6 py-4">Ngày vào làm</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-50 text-sm">
                {filteredEmployees.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-zinc-400">
                      Không có nhân viên nào phù hợp với bộ lọc lọc.
                    </td>
                  </tr>
                ) : (
                  filteredEmployees.map(e => {
                    const dept = departments.find(d => d.id === e.departmentId);
                    const pos = positions.find(p => p.id === e.positionId);
                    return (
                      <tr
                        key={e.id}
                        onClick={() => setSelectedEmp(e)}
                        className={`cursor-pointer transition-colors ${
                          selectedEmp?.id === e.id ? 'bg-[#2d3a2d]/5 border-l-4 border-[#2d3a2d]' : 'hover:bg-zinc-50/50'
                        }`}
                      >
                        <td className="px-6 py-4 font-mono font-bold text-zinc-950">{e.employeeCode}</td>
                        <td className="px-6 py-4">
                          <div className="font-bold text-zinc-900">{e.fullName}</div>
                          <div className="text-xs text-zinc-400 font-semibold">{e.phone || e.email || 'N/A'}</div>
                        </td>
                        <td className="px-6 py-4 font-bold text-zinc-650">{dept?.departmentName || 'N/A'}</td>
                        <td className="px-6 py-4 text-zinc-600 font-semibold">{pos?.positionName || 'N/A'}</td>
                        <td className="px-6 py-4">
                          <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${
                            e.workStatus === 'Chính thức' ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' :
                            e.workStatus === 'Thử việc' ? 'bg-blue-50 text-blue-700 border border-blue-100' :
                            'bg-zinc-100 text-zinc-650'
                          }`}>
                            {e.workStatus}
                          </span>
                        </td>
                        <td className="px-6 py-4 font-mono text-zinc-400 font-semibold">
                          {e.joinDate ? new Date(e.joinDate).toLocaleDateString('vi-VN') : 'N/A'}
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Scrollable Form Panel */}
        {isEditing && (
          <div className="xl:col-span-4 bg-[#f9f9f9] border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6 max-h-[680px] overflow-y-auto animate-slide-left custom-scrollbar">
            <div className="flex justify-between items-center pb-2 border-b border-zinc-100">
              <h2 className="text-lg font-bold text-zinc-800">Thông tin hồ sơ</h2>
              <button
                onClick={() => setIsEditing(false)}
                className="p-1 hover:bg-zinc-200 rounded-lg text-zinc-500 transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {errorMsg && (
              <div className="bg-red-50 text-red-600 px-4 py-3 rounded-xl border border-red-200 text-xs font-semibold flex items-center gap-2">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                {errorMsg}
              </div>
            )}

            <div className="space-y-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Mã nhân viên (*)</label>
                <input
                  type="text"
                  placeholder="Nhập mã nhân viên (ví dụ: NV001)..."
                  value={editingEmp.employeeCode || ''}
                  onChange={e => setEditingEmp(prev => ({ ...prev, employeeCode: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Họ và Tên (*)</label>
                <input
                  type="text"
                  placeholder="Nhập họ và tên..."
                  value={editingEmp.fullName || ''}
                  onChange={e => setEditingEmp(prev => ({ ...prev, fullName: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Giới tính</label>
                  <select
                    value={editingEmp.gender || 'Nam'}
                    onChange={e => setEditingEmp(prev => ({ ...prev, gender: e.target.value }))}
                    className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm outline-none"
                  >
                    <option value="Nam">Nam</option>
                    <option value="Nữ">Nữ</option>
                  </select>
                </div>

                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Ngày sinh</label>
                  <input
                    type="date"
                    value={editingEmp.dateOfBirth || '1995-01-01'}
                    onChange={e => setEditingEmp(prev => ({ ...prev, dateOfBirth: e.target.value }))}
                    className="px-4 py-2 bg-white border border-zinc-200 rounded-xl text-sm outline-none"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Phòng ban</label>
                  <select
                    value={editingEmp.departmentId || ''}
                    onChange={e => setEditingEmp(prev => ({ ...prev, departmentId: Number(e.target.value) }))}
                    className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm"
                  >
                    {activeDepts.map(d => (
                      <option key={d.id} value={d.id}>{d.departmentName}</option>
                    ))}
                  </select>
                </div>

                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Chức vụ</label>
                  <select
                    value={editingEmp.positionId || ''}
                    onChange={e => setEditingEmp(prev => ({ ...prev, positionId: Number(e.target.value) }))}
                    className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm"
                  >
                    {activePositions.map(p => (
                      <option key={p.id} value={p.id}>{p.positionName}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Số điện thoại</label>
                <input
                  type="text"
                  placeholder="Nhập SĐT..."
                  value={editingEmp.phone || ''}
                  onChange={e => setEditingEmp(prev => ({ ...prev, phone: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Địa chỉ Email</label>
                <input
                  type="email"
                  placeholder="Nhập email..."
                  value={editingEmp.email || ''}
                  onChange={e => setEditingEmp(prev => ({ ...prev, email: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Lương cơ bản (VND)</label>
                <input
                  type="number"
                  placeholder="Nhập số tiền..."
                  value={editingEmp.baseSalary || 10000000}
                  onChange={e => setEditingEmp(prev => ({ ...prev, baseSalary: Number(e.target.value) }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Trạng thái làm việc</label>
                <select
                  value={editingEmp.workStatus || 'Thử việc'}
                  onChange={e => setEditingEmp(prev => ({ ...prev, workStatus: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2"
                >
                  {statusList.map(s => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="flex flex-col gap-2 mt-4">
              <button
                onClick={handleSave}
                className="w-full flex items-center justify-center gap-2 bg-[#2d3a2d] hover:bg-[#1a241a] text-white py-3 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Save className="w-4 h-4" />
                Lưu hồ sơ
              </button>
              <button
                onClick={() => setIsEditing(false)}
                className="w-full bg-white hover:bg-zinc-50 border text-zinc-700 py-3 rounded-xl font-bold text-sm transition-all"
              >
                Thủ tiêu / Hủy
              </button>
            </div>
          </div>
        )}
      </div>

      {selectedEmp && (
        <ExportContractModal
          isOpen={isContractModalOpen}
          onClose={() => setIsContractModalOpen(false)}
          employee={selectedEmp}
        />
      )}
    </div>
  );
}
