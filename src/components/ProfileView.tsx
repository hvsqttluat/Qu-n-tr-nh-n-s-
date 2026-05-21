import React, { useState } from 'react';
import { User } from '../types';
import { UserCheck, Shield, Key, Mail, RefreshCw, Layers, CheckCircle2 } from 'lucide-react';
import { initialUsers } from '../data';

interface ProfileViewProps {
  currentUser: User;
  onUpdateCurrentUser: (user: User) => void;
  addLog: (action: string, table: string, desc: string) => void;
  onInstantLogin: (username: string) => void;
}

export function ProfileView({
  currentUser,
  onUpdateCurrentUser,
  addLog,
  onInstantLogin
}: ProfileViewProps) {
  const [fullName, setFullName] = useState(currentUser.fullName);
  const [email, setEmail] = useState(currentUser.email);
  const [username, setUsername] = useState(currentUser.username);
  const [password, setPassword] = useState('123'); // Preset demo password
  const [successMsg, setSuccessMsg] = useState('');

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    const updatedUser: User = {
      ...currentUser,
      fullName,
      email,
      username
    };
    onUpdateCurrentUser(updatedUser);
    addLog('Cài đặt tài khoản', 'Users', `Cập nhật thông tin tài khoản cho username: ${username}`);
    
    setSuccessMsg('Cập nhật tài khoản chính thức thành công! Thông tin mới được lưu trữ định dạng.');
    setTimeout(() => setSuccessMsg(''), 4000);
  };

  // Roles permission overview helper
  const getPermissionDescription = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'Quyền tối cao: Có toàn quyền quản lý nhân phẩm, phòng ban, phân bố lương cơ bản, kiểm tra lịch trình và theo dõi toàn vẹn hệ thống Audit Log.';
      case 'Giám đốc':
        return 'Quyền Giám đốc (CEO): Toàn quyền kiểm soát kinh doanh, chốt/duyệt bảng lương tự động, phê trực duyệt đơn phép nghỉ tự động, xuất mọi hợp đồng lao động.';
      case 'Thư ký':
        return 'Quyền Thư ký: Tiếp nhận hồ sơ nhân sự, rà soát phòng ban chức vụ, điều hướng chấm công và đơn xin nghỉ phép định kỳ trong hệ thống.';
      case 'Kế toán':
        return 'Quyền Kế toán: Tiếp cận chuyên biệt mô-đun tiền lương, lập biểu thuế, thưởng phạt, chấm công và khoá sổ lương nhân phẩm hàng tháng.';
      case 'HR':
        return 'Quyền Trưởng phòng Nhân sự: Độc lập tuyển dụng nhân viên, kiến tạo hợp đồng làm việc mới, phê duyệt cơ động các kỳ nghỉ phép hành chính.';
      case 'Manager':
        return 'Quyền Quản lý Phòng Ban: Giám sát toàn thể nhân sự trực thuộc, ký ghi nhận chấm công hàng ngày cho đơn vị cơ sở.';
      case 'Employee':
        return 'Quyền Nhân viên: Quản lý hồ sơ cá nhân tự thục, theo dõi phiếu lương hàng tháng cá nhân và thực thi nộp đơn phép trực tuyến.';
      default:
        return 'Quyền hạn cơ bản theo quy chế cơ yếu.';
    }
  };

  return (
    <div className="space-y-6 animate-fade-in font-sans">
      <div className="flex flex-col">
        <h1 className="text-3xl font-extrabold tracking-tight text-zinc-900">Cài đặt Tài khoản</h1>
        <p className="text-sm text-zinc-500 mt-1">Cấu hình hồ sơ đăng nhập và phân quyền bảo mật hành chính</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Profile Card & Switch Account simulation */}
        <div className="lg:col-span-4 space-y-6">
          
          {/* User badge */}
          <div className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm text-center relative overflow-hidden">
            <div className="absolute top-0 left-0 right-0 h-2 bg-gradient-to-r from-blue-600 to-indigo-600" />
            <div className="w-20 h-20 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 font-black text-2xl mx-auto mt-4 mb-4 border border-blue-200">
              {currentUser.fullName.charAt(0)}
            </div>
            <h2 className="text-xl font-bold text-zinc-900 uppercase tracking-tight">{currentUser.fullName}</h2>
            <p className="text-xs text-zinc-400 font-bold mt-1">@{currentUser.username}</p>
            
            <div className="mt-4 inline-block px-3 py-1 bg-emerald-50 text-emerald-700 border border-emerald-100 rounded-full font-black text-xs uppercase tracking-wider">
              {currentUser.role}
            </div>

            <div className="mt-6 pt-6 border-t border-zinc-100 text-left text-xs space-y-2">
              <div className="flex justify-between">
                <span className="text-zinc-400 font-bold">Email:</span>
                <span className="text-zinc-700 font-semibold truncate max-w-[170px]">{currentUser.email}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-zinc-400 font-bold">Bắt đầu ngày:</span>
                <span className="text-zinc-700 font-mono font-semibold">21/05/2026</span>
              </div>
              <div className="flex justify-between">
                <span className="text-zinc-400 font-bold">Trạng thái:</span>
                <span className="text-emerald-700 font-bold">Hoạt động (Active)</span>
              </div>
            </div>
          </div>

          {/* Quick simulation accounts (CRITICAL ASSIGNMENT REQUIREMENT) */}
          <div className="bg-blue-50/70 border-2 border-blue-200 rounded-xl p-6 shadow-sm">
            <div className="flex items-center gap-2 mb-3">
              <RefreshCw className="w-5 h-5 text-blue-800 animate-spin-slow" />
              <h3 className="font-extrabold text-sm text-blue-800 uppercase tracking-wider">Lập bảng Giả lập Đăng nhập nhanh</h3>
            </div>
            <p className="text-xs text-zinc-650 mb-4 leading-relaxed">
              Bạn có thể **click chuyển đổi trực tiếp** các tài khoản dưới đây để giả lập phân bổ giao diện chuẩn MVVM theo phân quyền:
            </p>

            <div className="space-y-2.5">
              {initialUsers.map((testUser) => (
                <button
                  key={testUser.id}
                  onClick={() => onInstantLogin(testUser.username)}
                  className={`w-full flex items-center justify-between p-3 rounded-lg text-xs font-bold transition-all border ${
                    currentUser.username === testUser.username
                      ? 'bg-blue-600 text-white border-transparent'
                      : 'bg-white hover:bg-zinc-50 text-zinc-800 border-zinc-200 hover:border-zinc-400'
                  }`}
                >
                  <div className="flex flex-col text-left">
                    <span>{testUser.fullName}</span>
                    <span className={`text-[10px] ${currentUser.username === testUser.username ? 'text-blue-300' : 'text-zinc-400'}`}>
                      @{testUser.username}
                    </span>
                  </div>
                  <span className={`px-2 py-0.5 rounded text-[9px] font-black uppercase ${
                    currentUser.username === testUser.username
                      ? 'bg-white/15 text-white'
                      : 'bg-zinc-100 text-zinc-600'
                  }`}>
                    {testUser.role}
                  </span>
                </button>
              ))}
            </div>
          </div>

        </div>

        {/* Edit Credential fields & Permission Breakdown */}
        <div className="lg:col-span-8 space-y-6">
          
          <form onSubmit={handleSave} className="bg-white border border-zinc-200 rounded-xl p-6 shadow-sm space-y-6">
            <h3 className="text-lg font-extrabold text-zinc-800 pb-2 border-b border-zinc-100">Cập nhật mật thư tài khoản</h3>

            {successMsg && (
              <div className="bg-emerald-50 text-emerald-850 border border-emerald-200 px-4 py-3 rounded-xl text-xs font-bold flex items-center gap-2 animate-bounce">
                <CheckCircle2 className="w-4 h-4 text-emerald-600 flex-shrink-0" />
                <span>{successMsg}</span>
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-wider">Họ và Tên</label>
                <input
                  type="text"
                  value={fullName}
                  onChange={e => setFullName(e.target.value)}
                  className="px-4 py-2.5 bg-zinc-50 border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all font-semibold"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-wider">Địa chỉ Email</label>
                <input
                  type="email"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  className="px-4 py-2.5 bg-zinc-50 border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all font-semibold"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-wider">Tên đăng nhập (Username)</label>
                <input
                  type="text"
                  value={username}
                  onChange={e => setUsername(e.target.value)}
                  className="px-4 py-2.5 bg-zinc-50 border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all font-mono font-bold"
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-black text-zinc-500 uppercase tracking-wider">Đổi mật khẩu (*)</label>
                <input
                  type="password"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  placeholder="Mật khẩu mã hoá..."
                  className="px-4 py-2.5 bg-zinc-50 border border-zinc-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                />
              </div>
            </div>

            <button
              type="submit"
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-xl font-bold text-sm transition-all shadow-md active:scale-95 outline-none flex items-center gap-2"
            >
              <Key className="w-4 h-4 text-white" />
              Áp dụng thay đổi
            </button>
          </form>

          {/* Role matrix explanation card */}
          <div className="bg-white border border-rose-100 rounded-xl p-6 shadow-sm space-y-4">
            <div className="flex items-center gap-2 pb-2 border-b border-zinc-100 text-rose-800">
              <Shield className="w-5 h-5 text-rose-700" />
              <h3 className="font-extrabold text-sm uppercase tracking-wider">Cấu trúc Phân Quyền Vai Trò Đăng Nhập</h3>
            </div>
            
            <div className="border border-zinc-100 rounded-lg overflow-hidden text-xs">
              <div className="bg-zinc-50/70 p-4 border-b">
                <span className="font-bold text-zinc-700 uppercase tracking-wider block">Phân quyền của bạn: {currentUser.role}</span>
                <p className="text-zinc-500 mt-1 leading-relaxed font-semibold">
                  {getPermissionDescription(currentUser.role)}
                </p>
              </div>

              <div className="p-4 space-y-3 font-semibold text-zinc-600">
                <p className="font-bold text-zinc-750 uppercase text-[10px] tracking-wider text-zinc-550 border-b pb-1.5">Tổng hợp Ma trận Vai trò:</p>
                
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
                  <div className="p-2.5 bg-zinc-50 border rounded-lg">
                    <span className="font-black text-zinc-800 block text-[11px] mb-1">👑 BAN GIÁM ĐỐC / ADMIN</span>
                    <p className="text-[10px] text-zinc-400 font-semibold">Toàn bộ 11 mô-đun, chốt lương, duyệt phép chỉ huy, phê duyệt nhân phẩm.</p>
                  </div>
                  <div className="p-2.5 bg-zinc-50 border rounded-lg">
                    <span className="font-black text-[#5821c4] block text-[11px] mb-1">✍️ THƯ KÝ ĐƠN VỊ</span>
                    <p className="text-[10px] text-zinc-400 font-semibold">Điều phối nhân viên, phòng ban, chức vụ, quản lý đơn phép hành chính.</p>
                  </div>
                  <div className="p-2.5 bg-zinc-50 border rounded-lg">
                    <span className="font-black text-emerald-800 block text-[11px] mb-1">💸 KẾ TOÁN QUÂN SỐ</span>
                    <p className="text-[10px] text-zinc-400 font-semibold">Tự động cấu toán thu chi, quyết định thưởng phạt, khoá sổ tiền lương.</p>
                  </div>
                  <div className="p-2.5 bg-zinc-50 border rounded-lg">
                    <span className="font-black text-blue-800 block text-[11px] mb-1">👤 NHÂN VIÊN THÂN HỮU</span>
                    <p className="text-[10px] text-zinc-400 font-semibold">Tự bảo dưỡng hồ sơ, xin nghỉ phép điện tử, nhận tiền lương trực tuyến.</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}
