import React from 'react';
import { Notification } from '../types';
import { RefreshCw, Check, AlertCircle, Bell, Info, CheckSquare } from 'lucide-react';

interface NotificationViewProps {
  notifications: Notification[];
  setNotifications: React.Dispatch<React.SetStateAction<Notification[]>>;
  addLog: (action: string, table: string, desc: string) => void;
}

export function NotificationView({ notifications, setNotifications, addLog }: NotificationViewProps) {
  const handleMarkAsRead = (id: number) => {
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    addLog('Đọc thông báo', 'Notifications', `Đã đánh dấu ảo đọc thông báo mã ID ${id}`);
  };

  const handleMarkAllAsRead = () => {
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    addLog('Đọc tất cả thông báo', 'Notifications', 'Đã đánh dấu đọc toàn bộ thông báo');
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex justify-between items-center">
        <div className="flex flex-col">
          <h1 className="text-3xl font-bold tracking-tight text-zinc-900">Thông báo của tôi</h1>
          <p className="text-sm text-zinc-500 mt-1">Thông báo và chỉ thị từ Bộ Chỉ huy HRM_WPF_CNPM</p>
        </div>

        <button
          onClick={handleMarkAllAsRead}
          className="flex items-center gap-2 text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 px-3.5 py-2 rounded-xl transition-all"
        >
          <CheckSquare className="w-4 h-4" />
          Đánh dấu tất cả đã đọc
        </button>
      </div>

      <div className="bg-white border rounded-xl overflow-hidden shadow-sm">
        <div className="divide-y divide-zinc-100">
          {notifications.length === 0 ? (
            <div className="p-12 text-center text-zinc-400">
              <Bell className="w-12 h-12 text-zinc-300 mx-auto mb-3 animate-bounce" />
              Chưa có thông báo nào gửi đến đồng chí.
            </div>
          ) : (
            notifications.map(n => (
              <div
                key={n.id}
                className={`p-6 flex gap-4 items-start hover:bg-zinc-50/50 transition-colors ${
                  !n.isRead ? 'bg-[#2d3a2d]/5 border-l-4 border-[#2d3a2d]' : ''
                }`}
              >
                <div className={`p-2 rounded-xl ${
                  n.type === 'Success' ? 'bg-emerald-100 text-emerald-700' :
                  n.type === 'Warning' ? 'bg-amber-100 text-amber-700' :
                  'bg-blue-100 text-blue-700'
                }`}>
                  {n.type === 'Success' ? <Check className="w-5 h-5" /> : 
                   n.type === 'Warning' ? <AlertCircle className="w-5 h-5" /> : 
                   <Info className="w-5 h-5" />}
                </div>

                <div className="flex-1">
                  <div className="flex flex-wrap justify-between items-start gap-2">
                    <h3 className={`text-base font-bold ${!n.isRead ? 'text-zinc-950 font-black' : 'text-zinc-700'}`}>
                      {n.title}
                    </h3>
                    <span className="text-xs text-zinc-400 font-semibold">
                      {new Date(n.createdAt).toLocaleDateString('vi-VN')} {new Date(n.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>
                  <p className="text-sm text-zinc-600 mt-1.5 leading-relaxed">{n.message}</p>
                </div>

                {!n.isRead && (
                  <button
                    onClick={() => handleMarkAsRead(n.id)}
                    className="p-1.5 hover:bg-zinc-100 rounded-lg text-emerald-600 font-bold text-xs flex items-center gap-1 border border-zinc-200"
                    title="Đánh dấu đã đọc"
                  >
                    <Check className="w-4 h-4" />
                    Đọc
                  </button>
                )}
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
