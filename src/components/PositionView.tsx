import React, { useState } from 'react';
import { Position, Department } from '../types';
import { Plus, Edit, Trash2, Search, Save, X, AlertCircle } from 'lucide-react';

interface PositionViewProps {
  positions: Position[];
  setPositions: React.Dispatch<React.SetStateAction<Position[]>>;
  departments: Department[];
  addLog: (action: string, table: string, desc: string) => void;
}

export function PositionView({ positions, setPositions, departments, addLog }: PositionViewProps) {
  const [selectedPos, setSelectedPos] = useState<Position | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [editingPos, setEditingPos] = useState<Partial<Position>>({});
  const [searchText, setSearchText] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  // Active items
  const activePositions = positions.filter(p => !p.isDeleted);
  const activeDepts = departments.filter(d => !d.isDeleted && d.isActive);

  // Filtered
  const filteredPositions = activePositions.filter(p =>
    p.positionName.toLowerCase().includes(searchText.toLowerCase()) ||
    p.positionCode.toLowerCase().includes(searchText.toLowerCase())
  );

  const startAdd = () => {
    setEditingPos({
      id: 0,
      positionCode: '',
      positionName: '',
      departmentId: activeDepts[0]?.id || 0,
      description: '',
      isActive: true,
      isDeleted: false
    });
    setIsEditing(true);
    setSelectedPos(null);
    setErrorMsg('');
  };

  const startEdit = () => {
    if (!selectedPos) return;
    setEditingPos({ ...selectedPos });
    setIsEditing(true);
    setErrorMsg('');
  };

  const handleSave = () => {
    if (!editingPos.positionCode || !editingPos.positionName || !editingPos.departmentId) {
      setErrorMsg('Vui lòng nhập đầy đủ thông tin mã, tên và phòng ban.');
      return;
    }

    const isDuplicate = positions.some(p =>
      p.positionCode.toUpperCase() === editingPos.positionCode?.toUpperCase() &&
      p.id !== editingPos.id &&
      !p.isDeleted
    );

    if (isDuplicate) {
      setErrorMsg('Mã chức vụ này đã tồn tại trên định dạng hệ thống!');
      return;
    }

    if (editingPos.id === 0) {
      // Add
      const newId = positions.length > 0 ? Math.max(...positions.map(p => p.id)) + 1 : 1;
      const newRecord: Position = {
        id: newId,
        positionCode: editingPos.positionCode.toUpperCase(),
        positionName: editingPos.positionName,
        departmentId: Number(editingPos.departmentId),
        description: editingPos.description || '',
        isActive: !!editingPos.isActive,
        createdAt: new Date().toISOString(),
        isDeleted: false
      };
      setPositions(prev => [...prev, newRecord]);
      addLog('Thêm chức vụ', 'Positions', `Tạo chức vụ mới: ${newRecord.positionName} (${newRecord.positionCode})`);
    } else {
      // Update
      setPositions(prev => prev.map(p => p.id === editingPos.id ? { ...p, ...editingPos as Position, positionCode: editingPos.positionCode!.toUpperCase() } : p));
      addLog('Cập nhật chức vụ', 'Positions', `Thay đổi thông tin chức vụ: ${editingPos.positionName}`);
    }

    setIsEditing(false);
    setSelectedPos(null);
    setErrorMsg('');
  };

  const handleDelete = () => {
    if (!selectedPos) return;
    if (window.confirm(`Bạn có chắc chắn muốn xóa chức vụ ${selectedPos.positionName} không?`)) {
      setPositions(prev => prev.map(p => p.id === selectedPos.id ? { ...p, isDeleted: true } : p));
      addLog('Xóa chức vụ', 'Positions', `Xóa mềm chức vụ: ${selectedPos.positionName}`);
      setSelectedPos(null);
      setIsEditing(false);
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col">
        <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Quản lý Chức vụ</h1>
        <p className="text-sm text-zinc-500 mt-1">Cấu hình chức vụ phân cấp kỹ thuật tương thích với phòng ban</p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-12 gap-6 items-start">
        {/* Table list */}
        <div className={`xl:col-span-${isEditing ? '8' : '12'} bg-white border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6`}>
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="relative w-full sm:w-72">
              <Search className="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Tìm mã hoặc tên..."
                value={searchText}
                onChange={e => setSearchText(e.target.value)}
                className="pl-9 pr-4 py-2 bg-zinc-50 border border-zinc-200 rounded-xl text-sm w-full focus:ring-2 focus:ring-emerald-500 outline-none transition-all"
              />
            </div>

            <div className="flex items-center gap-2 w-full sm:w-auto">
              <button
                onClick={startAdd}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Plus className="w-4 h-4" />
                Thêm mới
              </button>
              <button
                disabled={!selectedPos}
                onClick={startEdit}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-white hover:bg-zinc-50 border text-zinc-700 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Edit className="w-4 h-4" />
                Sửa
              </button>
              <button
                disabled={!selectedPos}
                onClick={handleDelete}
                className="flex-1 sm:flex-initial flex items-center justify-center gap-2 bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 disabled:opacity-50 px-4 py-2 rounded-xl font-bold text-sm transition-all"
              >
                <Trash2 className="w-4 h-4" />
                Xóa
              </button>
            </div>
          </div>

          <div className="border border-zinc-100 rounded-xl overflow-hidden max-h-[500px] overflow-y-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-zinc-50/70 border-b border-zinc-100 text-xs font-black tracking-wider text-zinc-400 uppercase">
                  <th className="px-6 py-4">Mã</th>
                  <th className="px-6 py-4">Tên chức vụ</th>
                  <th className="px-6 py-4">Phòng ban</th>
                  <th className="px-6 py-4">Mô tả</th>
                  <th className="px-6 py-4">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-50 text-sm">
                {filteredPositions.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-zinc-400">
                      Không tìm thấy chức vụ nào khớp điều kiện.
                    </td>
                  </tr>
                ) : (
                  filteredPositions.map(p => {
                    const dept = departments.find(d => d.id === p.departmentId);
                    return (
                      <tr
                        key={p.id}
                        onClick={() => setSelectedPos(p)}
                        className={`cursor-pointer transition-colors ${
                          selectedPos?.id === p.id ? 'bg-blue-50/50 border-l-4 border-blue-600' : 'hover:bg-zinc-50/50'
                        }`}
                      >
                        <td className="px-6 py-4 font-mono font-bold text-zinc-900">{p.positionCode}</td>
                        <td className="px-6 py-4 font-bold text-zinc-800">{p.positionName}</td>
                        <td className="px-6 py-4 font-semibold text-emerald-800">{dept?.departmentName || 'Không rõ'}</td>
                        <td className="px-6 py-4 text-zinc-500 max-w-[200px] truncate">{p.description || 'N/A'}</td>
                        <td className="px-6 py-4">
                          <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${
                            p.isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-100' : 'bg-zinc-100 text-zinc-600'
                          }`}>
                            {p.isActive ? 'Hoạt động' : 'Tạm dừng'}
                          </span>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Edit Panel form */}
        {isEditing && (
          <div className="xl:col-span-4 bg-[#f9f9f9] border border-zinc-200 rounded-xl p-6 shadow-sm flex flex-col gap-6 animate-slide-left">
            <div className="flex justify-between items-center pb-2 border-b border-zinc-100">
              <h2 className="text-lg font-bold text-zinc-800">Thông tin chức vụ</h2>
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
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Mã chức vụ (*)</label>
                <input
                  type="text"
                  placeholder="Nhập mã chức vụ..."
                  value={editingPos.positionCode || ''}
                  onChange={e => setEditingPos(prev => ({ ...prev, positionCode: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Tên chức vụ (*)</label>
                <input
                  type="text"
                  placeholder="Nhập tên chức vụ..."
                  value={editingPos.positionName || ''}
                  onChange={e => setEditingPos(prev => ({ ...prev, positionName: e.target.value }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Phòng ban (*)</label>
                <select
                  value={editingPos.departmentId || ''}
                  onChange={e => setEditingPos(prev => ({ ...prev, departmentId: Number(e.target.value) }))}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all"
                >
                  {activeDepts.map(d => (
                    <option key={d.id} value={d.id}>{d.departmentName}</option>
                  ))}
                </select>
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-zinc-600 uppercase tracking-wider">Mô tả</label>
                <textarea
                  placeholder="Nhập ghi chú chức vụ..."
                  value={editingPos.description || ''}
                  onChange={e => setEditingPos(prev => ({ ...prev, description: e.target.value }))}
                  rows={3}
                  className="px-4 py-2.5 bg-white border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none transition-all resize-none"
                />
              </div>

              <div className="flex items-center gap-2 py-2">
                <input
                  type="checkbox"
                  id="chkActive"
                  checked={!!editingPos.isActive}
                  onChange={e => setEditingPos(prev => ({ ...prev, isActive: e.target.checked }))}
                  className="w-4 h-4 text-emerald-600 border-zinc-300 rounded focus:ring-emerald-500"
                />
                <label htmlFor="chkActive" className="text-sm font-bold text-zinc-700 cursor-pointer">Đang hoạt động</label>
              </div>
            </div>

            <div className="flex flex-col gap-2 mt-2 pb-2">
              <button
                onClick={handleSave}
                className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-xl font-bold text-sm transition-all shadow-sm"
              >
                <Save className="w-4 h-4" />
                Lưu lại
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
