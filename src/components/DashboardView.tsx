import React, { useState, useMemo } from 'react';
import { Department, Position, Employee, LeaveRequest, Contract } from '../types';
import { 
  Users, 
  ClipboardList, 
  FileWarning, 
  Landmark, 
  AlertTriangle, 
  CheckCircle, 
  Info, 
  TrendingUp, 
  DollarSign, 
  Briefcase, 
  Search,
  Filter,
  BarChart2,
  PieChart as PieIcon,
  Activity
} from 'lucide-react';
import { 
  ResponsiveContainer, 
  PieChart, 
  Pie, 
  Cell, 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  Legend, 
  AreaChart, 
  Area 
} from 'recharts';
import { motion } from 'motion/react';

interface DashboardProps {
  employees: Employee[];
  departments: Department[];
  positions: Position[];
  leaveRequests: LeaveRequest[];
  contracts: Contract[];
}

const COLORS = [
  '#3b82f6', // Bright Blue
  '#6366f1', // Indigo
  '#14b8a6', // Teal
  '#06b6d4', // Cyan
  '#ec4899', // Pink
  '#f97316', // Orange
  '#8b5cf6', // Violet
  '#22c55e', // Green
];

export function DashboardView({ employees, departments, positions, leaveRequests, contracts }: DashboardProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [chartFocus, setChartFocus] = useState<string | null>(null);

  // Compute metrics
  const activeEmployees = useMemo(() => employees.filter(e => !e.isDeleted && e.workStatus !== 'Đã nghỉ'), [employees]);
  const pendingLeaves = useMemo(() => leaveRequests.filter(l => l.status === 'Chờ duyệt'), [leaveRequests]);
  const expiringContracts = useMemo(() => contracts.filter(c => c.status === 'Sắp hết hạn'), [contracts]);
  const totalDepts = useMemo(() => departments.filter(d => !d.isDeleted && d.isActive), [departments]);

  // Compute stats per department
  const deptStats = useMemo(() => {
    return totalDepts.map(d => {
      const deptEmps = activeEmployees.filter(e => e.departmentId === d.id);
      const totalBaseSalary = deptEmps.reduce((acc, curr) => acc + curr.baseSalary, 0);
      const avgSalary = deptEmps.length > 0 ? Math.round(totalBaseSalary / deptEmps.length) : 0;
      
      return {
        id: d.id,
        code: d.departmentCode,
        name: d.departmentName,
        count: deptEmps.length,
        totalPayroll: totalBaseSalary,
        avgSalary: avgSalary,
        percentage: activeEmployees.length > 0 ? ((deptEmps.length / activeEmployees.length) * 100).toFixed(1) : '0.0'
      };
    }).sort((a, b) => b.count - a.count);
  }, [totalDepts, activeEmployees]);

  // Filtered stats for Search in Data Grid
  const filteredDeptStats = useMemo(() => {
    return deptStats.filter(d => 
      d.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
      d.code.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [deptStats, searchTerm]);

  // Pie chart data
  const pieData = useMemo(() => {
    return deptStats
      .filter(item => item.count > 0)
      .map(item => ({
        name: item.name,
        value: item.count
      }));
  }, [deptStats]);

  // Bar chart data (Avg Base Salary by department in Million VND)
  const barData = useMemo(() => {
    return deptStats
      .filter(item => item.avgSalary > 0)
      .map(item => ({
        name: item.code,
        fullName: item.name,
        'Lương TB (Triệu)': Math.round(item.avgSalary / 1000000 * 10) / 10
      }));
  }, [deptStats]);

  // Area chart data: Leave requests status trend by Leave type
  const leaveData = useMemo(() => {
    const types = Array.from(new Set(leaveRequests.map(r => r.leaveType)));
    return types.map(t => {
      const total = leaveRequests.filter(r => r.leaveType === t).length;
      const approved = leaveRequests.filter(r => r.leaveType === t && r.status === 'Đã duyệt').length;
      const pending = leaveRequests.filter(r => r.leaveType === t && r.status === 'Chờ duyệt').length;
      return {
        name: t || 'Khác',
        'Tổng số đơn': total,
        'Đơn đã duyệt': approved,
        'Đơn chờ duyệt': pending
      };
    });
  }, [leaveRequests]);

  // Gender demographics
  const genderStats = useMemo(() => {
    const male = activeEmployees.filter(e => e.gender === 'Nam').length;
    const female = activeEmployees.filter(e => e.gender === 'Nữ').length;
    const other = activeEmployees.length - male - female;
    return [
      { name: 'Nam', value: male, color: '#3b82f6' },
      { name: 'Nữ', value: female, color: '#ec4899' },
      { name: 'Khác', value: other, color: '#f59e0b' }
    ].filter(g => g.value > 0);
  }, [activeEmployees]);

  // Currency Formatter Helpers
  const formatVND = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(val);
  };

  // Warning Alerts List
  const alerts = useMemo(() => {
    return [
      ...expiringContracts.map(c => {
        const emp = employees.find(e => e.id === c.employeeId);
        return {
          title: 'Hợp đồng sắp hết hạn',
          description: `Hợp đồng lao động ${c.contractCode} của nhân sự ${emp?.fullName || ''} sắp hết hiệu lực.`,
          type: 'Warning',
          color: 'amber'
        };
      }),
      ...pendingLeaves.map(l => {
        const emp = employees.find(e => e.id === l.employeeId);
        return {
          title: 'Đơn xin nghỉ phép cần duyệt',
          description: `Nhân viên ${emp?.fullName || ''} xin nghỉ ${l.totalDays} ngày (${l.leaveType}) lý do: "${l.reason}"`,
          type: 'Info',
          color: 'sky'
        };
      }),
      ...employees.filter(e => e.workStatus === 'Thử việc').map(e => ({
        title: 'Nhân sự đang thử việc',
        description: `Thử việc: ${e.fullName} (${e.employeeCode}) đang trong quá trình đánh giá hiệu suất.`,
        type: 'Info',
        color: 'emerald'
      }))
    ];
  }, [expiringContracts, pendingLeaves, employees]);

  return (
    <div className="space-y-8 animate-fade-in font-sans pb-12">
      {/* Dynamic Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 bg-white p-6 border border-slate-200/80 rounded-2xl shadow-sm">
        <div>
          <h1 className="text-3xl font-black tracking-tight text-slate-900 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
            Dashboard Tổng Quan
          </h1>
          <p className="text-sm text-slate-500 mt-1 font-medium">
            Hệ thống phân tích, giám sát thực lực & quản trị hiệu quả nhân sự doanh nghiệp NexusHQ
          </p>
        </div>
        <div className="flex items-center gap-2 text-xs font-bold text-slate-500 bg-slate-50 px-3 py-1.5 rounded-lg border border-slate-200">
          <Activity className="w-3.5 h-3.5 text-blue-500 animate-pulse" />
          <span>Dữ liệu thời gian thực (Real-time synced)</span>
        </div>
      </div>

      {/* Grid widgets */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div 
          whileHover={{ y: -3, scale: 1.01 }}
          className="bg-white border border-slate-200 shadow-sm hover:shadow-md rounded-2xl p-6 transition-all flex items-center gap-5 relative overflow-hidden"
        >
          <div className="absolute right-0 top-0 w-24 h-24 bg-blue-50 rounded-full translate-x-8 -translate-y-8 opacity-40" />
          <div className="w-12 h-12 rounded-xl bg-blue-100 flex items-center justify-center text-blue-600 relative z-10 shadow-inner">
            <Users className="w-6 h-6" />
          </div>
          <div className="relative z-10">
            <p className="text-xs font-bold uppercase tracking-wider text-slate-400">Tổng nhân viên</p>
            <p className="text-3xl font-black text-slate-800 leading-none mt-1">{activeEmployees.length}</p>
            <span className="text-[10px] text-green-600 font-extrabold flex items-center gap-0.5 mt-1">
              <TrendingUp className="w-3 h-3" /> Đang hoạt động
            </span>
          </div>
        </motion.div>

        <motion.div 
          whileHover={{ y: -3, scale: 1.01 }}
          className="bg-white border border-slate-200 shadow-sm hover:shadow-md rounded-2xl p-6 transition-all flex items-center gap-5 relative overflow-hidden"
        >
          <div className="absolute right-0 top-0 w-24 h-24 bg-rose-50 rounded-full translate-x-8 -translate-y-8 opacity-40" />
          <div className="w-12 h-12 rounded-xl bg-rose-100 flex items-center justify-center text-rose-600 relative z-10 shadow-inner">
            <ClipboardList className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-bold uppercase tracking-wider text-slate-400">Đơn chờ duyệt</p>
            <p className="text-3xl font-black text-rose-600 leading-none mt-1">{pendingLeaves.length}</p>
            <span className="text-[10px] text-rose-500 font-extrabold block mt-1">Cần phê duyệt sớm</span>
          </div>
        </motion.div>

        <motion.div 
          whileHover={{ y: -3, scale: 1.01 }}
          className="bg-white border border-slate-200 shadow-sm hover:shadow-md rounded-2xl p-6 transition-all flex items-center gap-5 relative overflow-hidden"
        >
          <div className="absolute right-0 top-0 w-24 h-24 bg-amber-50 rounded-full translate-x-8 -translate-y-8 opacity-40" />
          <div className="w-12 h-12 rounded-xl bg-amber-100 flex items-center justify-center text-amber-600 relative z-10 shadow-inner">
            <FileWarning className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-bold uppercase tracking-wider text-slate-400">HĐ sắp hết hạn</p>
            <p className="text-3xl font-black text-amber-600 leading-none mt-1">{expiringContracts.length}</p>
            <span className="text-[10px] text-amber-500 font-extrabold block mt-1">Yêu cầu gia hạn</span>
          </div>
        </motion.div>

        <motion.div 
          whileHover={{ y: -3, scale: 1.01 }}
          className="bg-white border border-slate-200 shadow-sm hover:shadow-md rounded-2xl p-6 transition-all flex items-center gap-5 relative overflow-hidden"
        >
          <div className="absolute right-0 top-0 w-24 h-24 bg-indigo-50 rounded-full translate-x-8 -translate-y-8 opacity-40" />
          <div className="w-12 h-12 rounded-xl bg-indigo-100 flex items-center justify-center text-indigo-600 relative z-10 shadow-inner">
            <Landmark className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs font-bold uppercase tracking-wider text-slate-400">Phòng ban hoạt động</p>
            <p className="text-3xl font-black text-indigo-600 leading-none mt-1">{totalDepts.length}</p>
            <span className="text-[10px] text-indigo-500 font-extrabold block mt-1">Đầy đủ chức năng</span>
          </div>
        </motion.div>
      </div>

      {/* Main Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Donut Chart: Headcount by Department */}
        <div id="dept-distribution-card" className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between mb-4 pb-2 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-blue-50 rounded-lg text-blue-600">
                <PieIcon className="w-4 h-4" />
              </div>
              <h2 className="text-lg font-extrabold text-slate-800">Cơ cấu nhân lực phòng ban</h2>
            </div>
            <span className="text-xs font-bold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-lg border border-blue-100">Biểu đồ tỷ phần</span>
          </div>

          <div className="h-72 w-full flex items-center justify-center">
            {pieData.length === 0 ? (
              <div className="text-center text-slate-400 text-sm">Chưa có dữ liệu phòng ban cụ thể</div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={pieData}
                    cx="50%"
                    cy="50%"
                    innerRadius={70}
                    outerRadius={100}
                    paddingAngle={4}
                    dataKey="value"
                    onMouseEnter={(_, index) => setChartFocus(pieData[index].name)}
                    onMouseLeave={() => setChartFocus(null)}
                  >
                    {pieData.map((entry, index) => (
                      <Cell 
                        key={`cell-${index}`} 
                        fill={COLORS[index % COLORS.length]} 
                        opacity={chartFocus === null || chartFocus === entry.name ? 1 : 0.4}
                        className="transition-all duration-300 outline-none"
                      />
                    ))}
                  </Pie>
                  <Tooltip 
                    contentStyle={{ borderRadius: '12px', borderColor: '#e2e8f0', boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }} 
                    formatter={(value) => [`${value} nhân sự`, 'Số lượng']}
                  />
                  <Legend 
                    verticalAlign="bottom" 
                    height={36} 
                    iconType="circle"
                    iconSize={8}
                    wrapperStyle={{ fontSize: '11px', fontWeight: 'bold', color: '#64748b' }}
                  />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Bar Chart: Average department salaries */}
        <div id="salary-chart-card" className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between mb-4 pb-2 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-indigo-50 rounded-lg text-indigo-600">
                <BarChart2 className="w-4 h-4" />
              </div>
              <h2 className="text-lg font-extrabold text-slate-800">Quỹ lương trung bình các ban</h2>
            </div>
            <span className="text-xs font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded-lg border border-indigo-100">Phân khúc triệu VND</span>
          </div>

          <div className="h-72 w-full">
            {barData.length === 0 ? (
              <div className="text-center text-slate-400 text-sm">Chưa có dữ liệu tính lương</div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={barData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis 
                    dataKey="name" 
                    tickLine={false} 
                    axisLine={false} 
                    tick={{ fontSize: 10, fontWeight: 'bold', fill: '#64748b' }} 
                  />
                  <YAxis 
                    tickLine={false} 
                    axisLine={false} 
                    tick={{ fontSize: 10, fill: '#64748b' }}
                    unit=" tr" 
                  />
                  <Tooltip 
                    contentStyle={{ borderRadius: '12px', borderColor: '#e2e8f0', boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }} 
                    formatter={(value, name, props) => [`${value} triệu VND`, `Trung bình: ${props.payload.fullName}`]}
                  />
                  <Bar 
                    dataKey="Lương TB (Triệu)" 
                    fill="#4f46e5" 
                    radius={[6, 6, 0, 0]}
                    maxBarSize={45}
                  >
                    {barData.map((entry, index) => (
                      <Cell 
                        key={`cell-${index}`} 
                        fill={COLORS[(index + 1) % COLORS.length]} 
                        className="transition-colors duration-300"
                      />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      </div>

      {/* Demographics Area Wave chart */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Area Chart: Leave trend and types */}
        <div className="lg:col-span-2 bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between mb-4 pb-2 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-teal-50 rounded-lg text-teal-600">
                <Activity className="w-4 h-4" />
              </div>
              <h2 className="text-lg font-extrabold text-slate-800">Thống kê lý do nghỉ phép & tần suất</h2>
            </div>
            <span className="text-xs font-bold text-teal-600 bg-teal-50 px-2 py-0.5 rounded-lg border border-teal-100">Nghỉ phép thường niên</span>
          </div>

          <div className="h-64 w-full">
            {leaveData.length === 0 ? (
              <div className="text-center text-slate-400 text-sm py-20">Không phát hiện đơn nghỉ nào</div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={leaveData} margin={{ top: 10, right: 10, left: -25, bottom: 0 }}>
                  <defs>
                    <linearGradient id="colorTotal" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.2}/>
                      <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                    </linearGradient>
                    <linearGradient id="colorApprove" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.2}/>
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis 
                    dataKey="name" 
                    tickLine={false} 
                    axisLine={false} 
                    tick={{ fontSize: 10, fontWeight: 'medium', fill: '#64748b' }} 
                  />
                  <YAxis 
                    tickLine={false} 
                    axisLine={false} 
                    tick={{ fontSize: 10, fill: '#64748b' }}
                    allowDecimals={false} 
                  />
                  <Tooltip contentStyle={{ borderRadius: '12px', borderColor: '#e2e8f0' }} />
                  <Area type="monotone" dataKey="Tổng số đơn" stroke="#3b82f6" fillOpacity={1} fill="url(#colorTotal)" strokeWidth={2} />
                  <Area type="monotone" dataKey="Đơn đã duyệt" stroke="#10b981" fillOpacity={1} fill="url(#colorApprove)" strokeWidth={1.5} />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Gender Demographics Circle distribution */}
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col justify-between">
          <div className="flex items-center justify-between mb-4 pb-2 border-b border-slate-100">
            <h2 className="text-lg font-extrabold text-slate-800">Cơ cấu giới tính</h2>
            <span className="text-xs font-bold text-slate-400 uppercase tracking-widest text-[10px]">Demographic</span>
          </div>
          
          <div className="flex-1 flex flex-col justify-center">
            <div className="space-y-4">
              {genderStats.map((item, index) => {
                const total = genderStats.reduce((sum, g) => sum + g.value, 0);
                const perc = total > 0 ? ((item.value / total) * 100).toFixed(0) : '0';
                return (
                  <div key={index} className="space-y-1.5">
                    <div className="flex justify-between items-center text-xs font-bold">
                      <span className="text-slate-600 flex items-center gap-1.5">
                        <span className="w-2.5 h-2.5 rounded-full inline-block" style={{ backgroundColor: item.color }} />
                        {item.name}
                      </span>
                      <span className="text-slate-800 font-black">{item.value} ({perc}%)</span>
                    </div>
                    <div className="w-full bg-slate-100 h-2.5 rounded-full overflow-hidden">
                      <div 
                        className="h-full rounded-full transition-all duration-500" 
                        style={{ width: `${perc}%`, backgroundColor: item.color }} 
                      />
                    </div>
                  </div>
                );
              })}
            </div>
            
            <div className="mt-6 pt-4 border-t border-slate-100 text-center text-[10px] text-slate-400 font-extrabold uppercase tracking-widest">
              Năng lực bình đẳng giới doanh nghiệp
            </div>
          </div>
        </div>
      </div>

      {/* Structured Department analytical data Grid with search */}
      <div id="departmental-analytical-table" className="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100 bg-slate-50/50 flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div>
            <h2 className="text-lg font-black text-slate-900">
              Phân tích hiệu ích Phân ban tổ chức
            </h2>
            <p className="text-xs text-slate-500 font-medium mt-0.5">Bảng hiệu suất cơ cấu nhân sự, quỹ lương và tỷ lệ phân bổ trên toàn hệ thống NexusHQ</p>
          </div>
          
          {/* Dashboard search input */}
          <div className="relative w-full md:w-72">
            <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Bộ lọc phân ban..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
              className="pl-9 pr-4 py-2 text-xs w-full bg-white border border-slate-250/80 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none text-slate-700 font-semibold"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50 text-slate-500 uppercase tracking-wider text-[10px] font-black border-b border-slate-200">
                <th className="px-6 py-3.5">Mã ban</th>
                <th className="px-6 py-3.5">Tên phòng ban tổ chức</th>
                <th className="px-6 py-3.5 text-center">Tổng nhân sự</th>
                <th className="px-6 py-3.5 text-center">Tỉ lệ cơ cấu</th>
                <th className="px-6 py-3.5 text-right">Lương trung bình</th>
                <th className="px-6 py-3.5 text-right">Quỹ lương tháng</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-sm font-medium text-slate-700">
              {filteredDeptStats.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-10 text-center text-slate-400 font-bold">
                    Không tìm thấy ban ngành nào phù hợp
                  </td>
                </tr>
              ) : (
                filteredDeptStats.map((item, idx) => (
                  <tr key={item.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="px-6 py-4 font-mono font-black text-blue-600 text-xs">{item.code}</td>
                    <td className="px-6 py-4 font-extrabold text-slate-900">{item.name}</td>
                    <td className="px-6 py-4 text-center font-bold text-slate-800">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-black bg-blue-50 text-blue-700 border border-blue-100">
                        {item.count} nhân sự
                      </span>
                    </td>
                    <td className="px-6 py-4 text-center">
                      <div className="flex items-center justify-center gap-2">
                        <div className="w-16 bg-slate-100 h-2 rounded-full overflow-hidden hidden sm:block">
                          <div 
                            className="bg-gradient-to-r from-blue-500 to-indigo-500 h-full rounded-full" 
                            style={{ width: `${item.percentage}%` }} 
                          />
                        </div>
                        <span className="font-mono text-xs font-black text-slate-600">{item.percentage}%</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-right font-bold text-slate-900">
                      {item.avgSalary > 0 ? formatVND(item.avgSalary) : 'N/A'}
                    </td>
                    <td className="px-6 py-4 text-right font-black text-indigo-600">
                      {item.totalPayroll > 0 ? formatVND(item.totalPayroll) : formatVND(0)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Warnings & Alerts */}
      <div id="alerts-and-warnings" className="bg-white border rounded-2xl overflow-hidden shadow-sm">
        <div className="px-6 py-4 border-b border-slate-100 bg-slate-50/70 flex justify-between items-center">
          <h2 className="text-md font-extrabold text-slate-800 flex items-center gap-2">
            <AlertTriangle className="w-5 h-5 text-amber-500 animate-bounce" />
            Cảnh báo cần phê duyệt &amp; Giám sát thời hạn
          </h2>
          <span className="text-xs font-black bg-amber-50 text-amber-700 px-3 py-1 rounded-full border border-amber-200">
            {alerts.length} yêu cầu tồn đọng
          </span>
        </div>

        <div className="divide-y divide-slate-100 max-h-[350px] overflow-y-auto">
          {alerts.length === 0 ? (
            <div className="p-8 text-center text-slate-400">
              <CheckCircle className="w-10 h-10 text-emerald-500 mx-auto mb-2 opacity-50" />
              Đã xử lý tất cả nhiệm vụ, không có cảnh báo tồn đọng!
            </div>
          ) : (
            alerts.map((al, index) => (
              <div key={index} className="px-6 py-4 flex gap-4 items-start hover:bg-slate-50 transition-colors">
                <div className={`mt-0.5 p-1.5 rounded-lg ${
                  al.color === 'amber' ? 'bg-amber-100 text-amber-700' :
                  al.color === 'sky' ? 'bg-blue-100 text-blue-700' :
                  'bg-emerald-100 text-emerald-700'
                }`}>
                  <Info className="w-4 h-4" />
                </div>
                <div>
                  <h3 className="font-extrabold text-slate-800 leading-tight text-sm">{al.title}</h3>
                  <p className="text-xs text-slate-650 mt-1 font-medium">{al.description}</p>
                  <span className="text-[9px] font-black tracking-wider uppercase text-slate-400 mt-2 block">
                    ĐỘ ƯU TIÊN: {al.type === 'Warning' ? 'HỎA TỐC' : 'THÔNG THƯỜNG'}
                  </span>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
