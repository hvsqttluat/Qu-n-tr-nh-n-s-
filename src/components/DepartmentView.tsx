import React, { useState } from 'react';
import { Department } from '../types';
import { Plus, Edit, Trash2, Search, Save, X, AlertCircle, Printer, Download, FileSpreadsheet, FileText, Presentation } from 'lucide-react';
import { exportToExcel, exportToWord, exportToPPTX, triggerPrintSelection } from '../utils/exportUtils';

interface DepartmentViewProps {
  departments: Department[];
  setDepartments: React.Dispatch<React.SetStateAction<Department[]>>;
  addLog: (action: string, table: string, desc: string) => void;
}

export function DepartmentView({ departments, setDepartments, addLog }: DepartmentViewProps) {
  const [selectedDept, setSelectedDept] = useState<Department | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editingDept, setEditingDept] = useState<Partial<Department>>({});
  const [searchText, setSearchText] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  // Active departments
  const activeDepartments = departments.filter(d => !d.isDeleted);

  // Filtered List
  const filteredDepartments = activeDepartments.filter(d =>
    d.departmentName.toLowerCase().includes(searchText.toLowerCase()) ||
    d.departmentCode.toLowerCase().includes(searchText.toLowerCase())
  );

  const handleExportExcel = () => {
    const title = "Báo cáo Cơ cấu Tổ chức Phòng ban - HRM WPF CNPM";
    const headers = [
      { header: 'Mã Phòng Ban', key: 'departmentCode', width: 20 },
      { header: 'Tên Phòng Ban', key: 'departmentName', width: 30 },
      { header: 'Mô Tả / Chức năng', key: 'description', width: 50 },
      { header: 'Trạng Thái Hoạt Động', key: 'statusText', width: 25 },
      { header: 'Ngày Khởi Tạo', key: 'createdAtFormatted', width: 25 }
    ];

    const data = filteredDepartments.map(d => ({
      ...d,
      statusText: d.isActive ? 'Đang hoạt động' : 'Tạm dừng',
      createdAtFormatted: d.createdAt ? new Date(d.createdAt).toLocaleDateString('vi-VN') : 'N/A'
    }));

    exportToExcel(title, headers, data, 'Cơ_Cấu_Phòng_Ban_HRM_CNPM.xlsx');
    addLog('Xuất Excel', 'Departments', 'Đã lưu trữ thành công cơ cấu phòng ban ra tệp Excel');
  };

  const handleExportWord = () => {
    const title = "Báo cáo Quyết định Cơ cấu & Sơ đồ Ban Chỉ đạo";
    const headers = ['Mã Phòng', 'Tên Phòng Ban', 'Chức năng / Ghi chú', 'Trạng Thái'];
    
    const rows = filteredDepartments.map(d => [
      d.departmentCode,
      d.departmentName,
      d.description || 'Chưa có mô tả cụ thể',
      d.isActive ? 'Đang hoạt động' : 'Tạm dừng'
    ]);

    const summaryText = `Tổng số phòng ban phòng kho chức năng hiện hành: ${filteredDepartments.length} ban chỉ đạo. Toàn bộ cơ cấu được tối ưu đồng nhất theo chỉ thị cải cách hành chính.`;
    
    exportToWord(title, headers, rows, 'Quyet_Dinh_Co_Cau_Phong_Ban.docx', summaryText);
    addLog('Xuất Word', 'Departments', 'Tải xuống nghị quyết cơ cấu phòng ban dạng Word .docx');
  };

  const handleExportPPTX = () => {
    const title = "Thuyết Trình Sơ Đồ Tổ Chức & Phòng Ban Doanh Nghiệp - NexusHQ";
    
    const deptNotes = filteredDepartments.map(d => {
      return `- Bộ phận [${d.departmentName}]: Mã ${d.departmentCode}. Nhiệm vụ: ${d.description || 'Hành chính phối hợp doanh nghiệp.'}`;
    });

    const slides = [
      {
        title: "Tổng quan Sơ đồ Tổ chức Bộ máy",
        subtitle: "Cơ cấu quản lý & điều hành",
        content: [
          `Hệ thống gồm: ${filteredDepartments.length} Phòng Ban chuyên ban quản trị nghiệp vụ.`,
          `Phân nhóm hành chính cốt lõi:`,
          `- Quản trị nguồn nhân lực và tuyển dụng nhân sự`,
          `- Kiểm soát kế hoạch, ngân sách tài chính và chuyển đổi số`,
          `- Hành chính văn thư và thực thi văn hóa doanh nghiệp.`
        ]
      },
      {
        title: "Chi tiết các Phòng Ban trọng yếu hiện hữu",
        subtitle: "Phân vùng phụ trách (Mẫu trích xuất live)",
        content: [
          `Danh sách các phòng ban chuyên môn đang vận hành:`,
          ...deptNotes.slice(0, 5)
        ]
      }
    ];

    exportToPPTX(title, slides, 'Thuyet_Trinh_So_Do_To_Chuc_Phong_Ban.pptx');
    addLog('Xuất PowerPoint', 'Departments', 'Đã tải thành công tài liệu slide thuyết trình phòng ban .pptx');
  };

  const handlePrintTable = () => {
    triggerPrintSelection('department-table-printable', 'Sơ Đồ Sơ Cấp Phòng Ban Chỉ Huy');
    addLog('In Ấn', 'Departments', 'In sơ đồ phòng ban thành công ra định bản giấy LPT');
  };

  const startAdd = () => {
    setEditingDept({
      id: 0,
      departmentCode: '',
      departmentName: '',
      description: '',
      isActive: true,
      isDeleted: false
    });
    setIsEditing(true);
    setSelectedDept(null);
    setErrorMsg('');
  };

  const startEdit = () => {
    if (!selectedDept) return;
    setEditingDept({ ...selectedDept });
    setIsEditing(true);
    setErrorMsg('');
  };

  const handleSave = () => {
    if (!editingDept.departmentCode || !editingDept.departmentName) {
      setErrorMsg('Vui lòng nhập đủ Mã và Tên phòng ban.');
      return;
    }

    // Check duplicate code
    const isDuplicate = departments.some(d =>
      d.departmentCode.toUpperCase() === editingDept.departmentCode?.toUpperCase() &&
      d.id !== editingDept.id &&
      !d.isDeleted
    );

    if (isDuplicate) {
      setErrorMsg('Mã phòng ban đã tồn tại trên hệ thống!');
      return;
    }

    if (editingDept.id === 0) {
      // Add
      const newId = departments.length > 0 ? Math.max(...departments.map(d => d.id)) + 1 : 1;
      const newRecord: Department = {
        id: newId,
        departmentCode: editingDept.departmentCode.toUpperCase(),
        departmentName: editingDept.departmentName,
        description: editingDept.description || '',
        isActive: true,
        createdAt: new Date().toISOString(),
        isDeleted: false
      };
      setDepartments(prev => [...prev, newRecord]);
      addLog('Thêm phòng ban', 'Departments', `Đã tạo phòng ban mới: ${newRecord.departmentName} (${newRecord.departmentCode})`);
    } else {
      // Update
      setDepartments(prev => prev.map(d => d.id === editingDept.id ? { ...d, ...editingDept as Department, departmentCode: editingDept.departmentCode!.toUpperCase() } : d));
      addLog('Cập nhật phòng ban', 'Departments', `Đã làm mới thông tin phòng ban: ${editingDept.departmentName}`);
    }

    setIsEditing(false);
    setSelectedDept(null);
    setErrorMsg('');
  };

  const handleDelete = () => {
    if (!selectedDept) return;
    if (window.confirm(`Bạn có chắc chắn muốn xóa phòng ban ${selectedDept.departmentName} không?`)) {
      setDepartments(prev => prev.map(d => d.id === selectedDept.id ? { ...d, isDeleted: true } : d));
      addLog('Xóa phòng ban', 'Departments', `Xóa mềm phòng ban: ${selectedDept.departmentName}`);
      setSelectedDept(null);
      setIsEditing(false);
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col">
        <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Quản lý Phòng ban</h1>
        <p className="text-sm text-zinc-500 mt-1">Quản lý cơ cấu, tổ chức và sơ đồ sơ cấp công ty</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-12 gap-6 items-start">
        {/* Left Side: Department list */}
        <div className={`xl:col-span-${isEditing ? '8' : '12'} bg-white border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6`}>
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="relative w-full sm:w-72">
              <Search className="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Tìm mã hoặc tên..."
                value={searchText}
                onChange={e => setSearchText(e.target.value)}
                className="pl-9 pr-4 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-sm w-full focus:ring-2 focus:ring-emerald-500 outline-none transition-all placeholder:text-zinc-400"
              />
            </div>

            <div className="flex items-center gap-2 w-full sm:w-auto flex-wrap">
              <button
                onClick={startAdd}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Plus className="w-4 h-4" />
                Thêm mới
              </button>
              <button
                disabled={!selectedDept}
                onClick={startEdit}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-white hover:bg-zinc-50 border text-zinc-700 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Edit className="w-4 h-4" />
                Sửa
              </button>
              <button
                disabled={!selectedDept}
                onClick={handleDelete}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Trash2 className="w-4 h-4" />
                Xóa
              </button>
            </div>
          </div>

          {/* Secondary Report Row for Departments */}
          <div className="flex flex-wrap items-center justify-between border-t pt-4 border-zinc-100 gap-4">
            <div className="text-xs font-bold text-zinc-500 uppercase tracking-widest flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-600 animate-ping"></span>
              Xuất cơ cấu &amp; In ấn (Live):
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

          <div className="border border-zinc-100 rounded-xl overflow-x-auto overflow-y-auto max-h-[500px] w-full" id="department-table-printable">
            <table className="w-full text-left border-collapse min-w-[650px]">
              <thead>
                <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                  <th className="px-6 py-4">Mã</th>
                  <th className="px-6 py-4">Tên phòng ban</th>
                  <th className="px-6 py-4">Mô tả</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-50 text-sm">
                {filteredDepartments.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="px-6 py-12 text-center text-zinc-400">
                      Không tìm thấy phòng ban nào khớp điều kiện.
                    </td>
                  </tr>
                ) : (
                  filteredDepartments.map(d => (
                    <tr
                      key={d.id}
                      onClick={() => setSelectedDept(d)}
                      className={`cursor-pointer transition-colors ${
                        selectedDept?.id === d.id ? 'bg-blue-50/50 border-l-4 border-blue-600' : 'hover:bg-zinc-50/50'
                      }`}
                    >
                      <td className="px-6 py-4 font-mono font-bold text-zinc-900">{d.departmentCode}</td>
                      <td className="px-6 py-4 font-bold text-zinc-800">{d.departmentName}</td>
                      <td className="px-6 py-4 text-zinc-500 line-clamp-1 max-w-[250px]">{d.description || 'N/A'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Right Side: Form Edit Panel */}
        {isEditing && (
          <div className="xl:col-span-4 bg-[#f9f9f9] border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6 animate-slide-left">
            <div className="flex justify-between items-center pb-2 border-b border-zinc-100">
              <h2 className="text-lg font-bold text-zinc-800">Thông tin phòng ban</h2>
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
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Mã phòng ban (*)</label>
                <input
                  type="text"
                  placeholder="Nhập mã phòng ban..."
                  value={editingDept.departmentCode || ''}
                  onChange={e => setEditingDept(prev => ({ ...prev, departmentCode: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all placeholder:text-zinc-400"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Tên phòng ban (*)</label>
                <input
                  type="text"
                  placeholder="Nhập tên phòng ban..."
                  value={editingDept.departmentName || ''}
                  onChange={e => setEditingDept(prev => ({ ...prev, departmentName: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all placeholder:text-zinc-400"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Mô tả</label>
                <textarea
                  placeholder="Nhập ghi chú phòng ban..."
                  value={editingDept.description || ''}
                  onChange={e => setEditingDept(prev => ({ ...prev, description: e.target.value }))}
                  rows={4}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all placeholder:text-zinc-400 resize-none"
                />
              </div>
            </div>

            <div className="flex flex-col gap-2 mt-4 pb-2">
              <button
                onClick={handleSave}
                className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Save className="w-4 h-4" />
                Lưu phòng ban
              </button>
              <button
                onClick={() => setIsEditing(false)}
                className="w-full bg-white hover:bg-zinc-50 border text-zinc-700 py-3 rounded-xl font-bold text-sm transition-all"
              >
                Hủy bỏ
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
