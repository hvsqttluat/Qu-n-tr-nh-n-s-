import React, { useState } from 'react';
import { csharpFiles, CSharpFile } from '../csharpFiles';
import { 
  File, 
  Folder, 
  Terminal, 
  Cpu, 
  Layers, 
  Monitor, 
  Play, 
  CheckCircle2, 
  Eye, 
  ArrowRight,
  Database,
  Code,
  BookOpen,
  Copy,
  Info,
  ExternalLink,
  ShieldAlert
} from 'lucide-react';

export function WpfExplorerView() {
  const [activeSubTab, setActiveSubTab] = useState<'code' | 'architecture' | 'simulator' | 'build'>('simulator');
  const [selectedFile, setSelectedFile] = useState<CSharpFile>(csharpFiles[0]);
  const [searchTerm, setSearchTerm] = useState('');
  const [copied, setCopied] = useState(false);
  const [searchCodeResult, setSearchCodeResult] = useState<CSharpFile[]>(csharpFiles);

  // Terminal Simulator states
  const [terminalLogs, setTerminalLogs] = useState<string[]>([
    "=== MSBuild & dotnet CLI Terminal Environment ===",
    "Windows/.NET build toolchain ready.",
    "Type 'dotnet build' or 'dotnet run' to build the WPF desktop app.",
  ]);
  const [terminalInput, setTerminalInput] = useState('');
  const [isBuilding, setIsBuilding] = useState(false);

  // Simulated Desktop WPF app states
  const [simulatedScreen, setSimulatedScreen] = useState<'login' | 'dashboard' | 'employees' | 'departments'>('login');
  const [simUsername, setSimUsername] = useState('manager');
  const [simPassword, setSimPassword] = useState('123');
  const [simError, setSimError] = useState('');
  const [simIsBusy, setSimIsBusy] = useState(false);
  const [activeWpfControl, setActiveWpfControl] = useState<string | null>(null);

  // Filter csharp files based on search
  const handleSearchChange = (val: string) => {
    setSearchTerm(val);
    if (!val) {
      setSearchCodeResult(csharpFiles);
    } else {
      const filtered = csharpFiles.filter(f => 
        f.name.toLowerCase().includes(val.toLowerCase()) || 
        f.category.toLowerCase().includes(val.toLowerCase()) ||
        f.content.toLowerCase().includes(val.toLowerCase())
      );
      setSearchCodeResult(filtered);
    }
  };

  const handleCopyCode = () => {
    navigator.clipboard.writeText(selectedFile.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const runTerminalCommand = (cmdStr: string) => {
    const rawCmd = cmdStr.trim();
    if (!rawCmd) return;

    const newLogs = [...terminalLogs, `C:\\Projects\\HRM_WPF_CNPM> ${rawCmd}`];
    
    if (rawCmd.toLowerCase() === 'dotnet build' || rawCmd.toLowerCase() === 'dotnet build hrm_wpf_cnpm.csproj') {
      setIsBuilding(true);
      newLogs.push("Microsoft (R) Build Engine version 17.6.3 for .NET");
      newLogs.push("Copyright (C) Microsoft Corporation. All rights reserved.");
      newLogs.push("");
      newLogs.push("  Restoring packages for C:\\Projects\\HRM_WPF_CNPM\\HRM_WPF_CNPM.csproj...");
      
      setTimeout(() => {
        setTerminalLogs(prev => [
          ...prev,
          "  Mã hóa phục dựng gói thành công. Đang phân tích cú pháp XAML...",
          "  Biên dịch Views/LoginWindow.xaml -> BHO (Binary Hot Object)...",
          "  Biên dịch Views/MainWindow.xaml -> BHO...",
          "  Biên dịch thành công mã nguồn C# (.NET 8.0 SDK)...",
          "  Cơ sở dữ liệu Entity Framework Core: Kết nối SQLite database -> OK.",
          "      => HRM_WPF_CNPM.dll -> C:\\Projects\\HRM_WPF_CNPM\\bin\\Debug\\net8.0-windows\\HRM_WPF_CNPM.dll",
          "      => HRM_WPF_CNPM.exe -> C:\\Projects\\HRM_WPF_CNPM\\bin\\Debug\\net8.0-windows\\HRM_WPF_CNPM.exe",
          "  SUCCESS: Biên dịch hoàn tất thành công! 0 Cảnh báo, 0 Lỗi.",
          "  Thời gian hoàn thành: 1.42s"
        ]);
        setIsBuilding(false);
      }, 1500);

    } else if (rawCmd.toLowerCase() === 'dotnet run' || rawCmd.toLowerCase() === 'dotnet run --project hrm_wpf_cnpm.csproj') {
      setIsBuilding(true);
      newLogs.push("Đang chạy ứng dụng WPF trên màn hình Desktop Windows 11...");
      setTimeout(() => {
        setTerminalLogs(prev => [
          ...prev,
          "  Khởi chạy thành công ứng dụng: HRM_WPF_CNPM.exe (PID: 4325)",
          "  Đang kết nối Server qua Chuỗi kết nối default...",
          "  Cáp truyền tải dữ liệu: WPF <--> AppDbContext <--> SQLite",
          "  [WPF MONITOR]: Hệ thống WPF đã khởi động màn hình chính."
        ]);
        setSimulatedScreen('login');
        setActiveSubTab('simulator');
        setIsBuilding(false);
      }, 1200);

    } else if (rawCmd.toLowerCase() === 'dotnet ef migrations list') {
      newLogs.push("Danh sách các bản Migrations của dự án WPF:");
      newLogs.push("  - 20260515120000_InitialCreate (Áp dụng: Có)");
      newLogs.push("  - 20260520093000_AddAttendanceTrigger (Áp dụng: Có)");

    } else if (rawCmd.toLowerCase() === 'clear' || rawCmd.toLowerCase() === 'cls') {
      setTerminalLogs([
        "=== MSBuild & dotnet CLI Terminal Environment ===",
        "Windows/.NET build toolchain ready."
      ]);
      setTerminalInput('');
      return;
    } else {
      newLogs.push(`Lệnh '${rawCmd}' không tìm thấy hoặc chưa được hỗ trợ trong phiên giả lập.`);
      newLogs.push("Hãy thử các lệnh sau:");
      newLogs.push("  - `dotnet build` : Biên dịch ứng dụng WPF và XAML");
      newLogs.push("  - `dotnet run` : Khởi chạy mô-đun WPF Simulator desktop");
      newLogs.push("  - `dotnet ef migrations list` : Liệt kê di trú cơ sở dữ liệu");
      newLogs.push("  - `clear` : Xóa màn hình.");
    }

    setTerminalLogs(newLogs);
    setTerminalInput('');
  };

  const handleSimulatedLogin = (e: React.FormEvent) => {
    e.preventDefault();
    setSimIsBusy(true);
    setSimError('');

    setTimeout(() => {
      // Allow any role logic mimicking WPF AuthService.cs and LoginViewModel.cs
      const matched = simUsername.toLowerCase() === 'admin' || 
                      simUsername.toLowerCase() === 'hr' || 
                      simUsername.toLowerCase() === 'manager' || 
                      simUsername.toLowerCase() === 'employee' ||
                      simUsername.toLowerCase() === 'ke_toan';

      if (matched && simPassword === '123') {
        setSimulatedScreen('dashboard');
      } else {
        setSimError('Tên đăng nhập hoặc mật khẩu sai!');
      }
      setSimIsBusy(false);
    }, 800);
  };

  const categories = Array.from(new Set(csharpFiles.map(f => f.category)));

  return (
    <div className="space-y-6 animate-fade-in font-sans">
      
      {/* Visual Header */}
      <div className="bg-gradient-to-r from-blue-900 via-indigo-900 to-slate-900 text-white p-8 rounded-3xl shadow-xl relative overflow-hidden border-b-4 border-blue-500">
        <div className="absolute inset-0 opacity-[0.05] pointer-events-none" style={{ backgroundImage: 'radial-gradient(#ffffff 1px, transparent 1px)', backgroundSize: '16px 16px' }} />
        
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6 relative z-10">
          <div className="space-y-2">
            <div className="inline-flex items-center gap-2 bg-blue-500/10 text-blue-300 border border-blue-500/25 px-3 py-1 rounded-full text-xs font-black uppercase tracking-widest">
              <Cpu className="w-3.5 h-3.5" />
              Windows Desktop WPF Application Built with .NET Core
            </div>
            <h1 className="text-3xl md:text-4xl font-black uppercase tracking-tight">C# WPF Software Architect Studio</h1>
            <p className="text-slate-300 text-sm max-w-2xl leading-relaxed font-semibold">
              Hệ thống này tích hợp song song ứng dụng **WPF (Windows Presentation Foundation)** viết bằng C# và XAML dùng kiến trúc MVVM, liên kết Entity Framework Core với giao diện quản trị Web mô phỏng.
            </p>
          </div>
          
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => {
                setActiveSubTab('simulator');
                setSimulatedScreen('login');
              }}
              className={`px-5 py-3 rounded-xl text-sm font-bold flex items-center gap-2 transition-all ${
                activeSubTab === 'simulator' 
                  ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' 
                  : 'bg-white/10 hover:bg-white/15 text-white'
              }`}
            >
              <Monitor className="w-4 h-4" />
              Chạy WPF Mobile/Desktop Simulator
            </button>
            <button
              onClick={() => setActiveSubTab('code')}
              className={`px-5 py-3 rounded-xl text-sm font-bold flex items-center gap-2 transition-all ${
                activeSubTab === 'code' 
                  ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' 
                  : 'bg-white/10 hover:bg-white/15 text-white'
              }`}
            >
              <Code className="w-4 h-4" />
              Xem Code C# &amp; XAML
            </button>
          </div>
        </div>

        {/* Tab Selection Navigation */}
        <div className="flex gap-4 mt-8 border-t border-white/10 pt-4 text-xs font-bold uppercase tracking-wider">
          <button 
            onClick={() => setActiveSubTab('simulator')} 
            className={`pb-2 border-b-2 transition-all flex items-center gap-1.5 ${activeSubTab === 'simulator' ? 'border-blue-400 text-blue-300' : 'border-transparent text-slate-400 hover:text-white'}`}
          >
            <Monitor className="w-3.5 h-3.5" /> Simulating Active WPF Client
          </button>
          <button 
            onClick={() => setActiveSubTab('code')} 
            className={`pb-2 border-b-2 transition-all flex items-center gap-1.5 ${activeSubTab === 'code' ? 'border-blue-400 text-blue-300' : 'border-transparent text-slate-400 hover:text-white'}`}
          >
            <Code className="w-3.5 h-3.5" /> Source Code Files Explorer
          </button>
          <button 
            onClick={() => setActiveSubTab('architecture')} 
            className={`pb-2 border-b-2 transition-all flex items-center gap-1.5 ${activeSubTab === 'architecture' ? 'border-blue-400 text-blue-300' : 'border-transparent text-slate-400 hover:text-white'}`}
          >
            <Layers className="w-3.5 h-3.5" /> WPF MVVM Databinding Rules
          </button>
          <button 
            onClick={() => setActiveSubTab('build')} 
            className={`pb-2 border-b-2 transition-all flex items-center gap-1.5 ${activeSubTab === 'build' ? 'border-blue-400 text-blue-300' : 'border-transparent text-slate-400 hover:text-white'}`}
          >
            <Terminal className="w-3.5 h-3.5" /> MSBuild compiler console
          </button>
        </div>
      </div>

      {/* RENDER ACTIVE TAB */}

      {/* 1. WPF PREVIEW SIMULATOR */}
      {activeSubTab === 'simulator' && (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          
          {/* Virtual Windows OS Shell */}
          <div className="lg:col-span-8 bg-slate-900 rounded-3xl border-2 border-slate-700 overflow-hidden shadow-2xl flex flex-col min-h-[550px]">
            {/* Win11 Titlebar */}
            <div className="bg-slate-800 px-4 py-3 flex items-center justify-between border-b border-slate-700 select-none">
              <div className="flex items-center gap-2 text-xs font-semibold text-slate-300">
                <Monitor className="w-4 h-4 text-blue-400 animate-pulse" />
                <span>HRM_WPF_CNPM.exe — ModernWpf .NET Client</span>
              </div>
              <div className="flex gap-2.5">
                <div className="w-3.5 h-3.5 rounded-full bg-slate-600 hover:bg-slate-500 cursor-pointer" />
                <div className="w-3.5 h-3.5 rounded-full bg-slate-600 hover:bg-slate-500 cursor-pointer" />
                <button 
                  onClick={() => setSimulatedScreen('login')}
                  className="w-3.5 h-3.5 rounded-full bg-red-500 hover:bg-red-400 cursor-pointer" 
                  title="Thoát ứng dụng WPF"
                />
              </div>
            </div>

            {/* Sim Body */}
            <div className="flex-1 bg-slate-100 p-6 flex items-center justify-center relative">
              
              {/* Login Window Emulation */}
              {simulatedScreen === 'login' && (
                <div className="w-full max-w-sm bg-white border border-slate-350 rounded-2xl shadow-xl overflow-hidden animate-fade-in text-zinc-900">
                  {/* WPF Window inside Window bar */}
                  <div className="bg-zinc-100 border-b px-4 py-2 flex justify-between items-center text-xs font-bold text-zinc-500">
                    <span>Đăng nhập hệ thống HRM Desktop</span>
                    <span className="text-[10px] bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded uppercase">XAML view</span>
                  </div>
                  <div className="p-6 space-y-5">
                    <div className="text-center space-y-1">
                      <div className="w-12 h-12 bg-blue-600 rounded-xl flex items-center justify-center mx-auto text-white font-bold text-xl shadow-md">
                        H
                      </div>
                      <h3 className="text-lg font-black tracking-tight text-zinc-800 uppercase">HRM_WPF_CNPM App</h3>
                      <p className="text-xs text-zinc-500 font-bold">WPF Client v1.0.0 (Fluent Design)</p>
                    </div>

                    <form onSubmit={handleSimulatedLogin} className="space-y-4">
                      <div className="flex flex-col gap-1.5">
                        <label className="text-[11px] font-black text-zinc-550 uppercase tracking-wider">Tài khoản</label>
                        <input
                          type="text"
                          value={simUsername}
                          onChange={e => setSimUsername(e.target.value)}
                          onFocus={() => setActiveWpfControl("LoginViewModel -> string Username (Two-Way Binding)")}
                          className="px-4 py-2 bg-zinc-50 border border-zinc-200 rounded-lg text-sm font-semibold outline-none focus:ring-1 focus:ring-blue-600 focus:bg-white"
                          placeholder="admin, hr, manager, employee"
                        />
                      </div>

                      <div className="flex flex-col gap-1.5">
                        <label className="text-[11px] font-black text-zinc-550 uppercase tracking-wider">Mật khẩu</label>
                        <input
                          type="password"
                          value={simPassword}
                          onChange={e => setSimPassword(e.target.value)}
                          onFocus={() => setActiveWpfControl("CommandParameter passed to LoginCommand (Secure PasswordBox)")}
                          className="px-4 py-2 bg-zinc-50 border border-zinc-200 rounded-lg text-sm font-semibold outline-none focus:ring-1 focus:ring-blue-600 focus:bg-white"
                          placeholder="mặc định: 123"
                        />
                      </div>

                      {simError && (
                        <p className="text-xs text-red-650 font-black bg-red-50 p-2 border border-red-100 rounded-lg">
                          ⚠️ {simError}
                        </p>
                      )}

                      <button
                        type="submit"
                        disabled={simIsBusy}
                        onClick={() => setActiveWpfControl("RelayCommand: LoginCommand.Execute() => AuthService.Authenticate()")}
                        className="w-full py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-black rounded-lg text-xs tracking-wider uppercase transition-all shadow-md active:scale-[0.98] flex items-center justify-center gap-2"
                      >
                        {simIsBusy ? (
                          <>Đang kiểm tra kết nối SQL...</>
                        ) : (
                          <>
                            <Play className="w-3.5 h-3.5 fill-current text-white" />
                            Đăng nhập (Command: LoginCommand)
                          </>
                        )}
                      </button>
                    </form>
                    <div className="bg-zinc-50 p-2 text-[10px] text-zinc-500 font-bold text-center border rounded border-dashed">
                      💡 Mẹo: Nhập Username **admin**, **hr**, **manager** hoặc **employee** với mật khẩu **123** để kiểm tra phân quyền WPF.
                    </div>
                  </div>
                </div>
              )}

              {/* Main Desktop Dashboard Emulation */}
              {simulatedScreen !== 'login' && (
                <div className="w-full bg-white border border-slate-350 rounded-2xl shadow-xl overflow-hidden animate-fade-in flex flex-col text-zinc-900 min-h-[460px]">
                  {/* Header layout mimicking ModernWpf title and layout */}
                  <div className="bg-slate-800 px-4 py-2.5 flex justify-between items-center text-xs font-bold text-white select-none">
                    <span className="flex items-center gap-2">
                      <div className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                      WPF Desktop Main Window Client — Xin chào, {simUsername.toUpperCase()}!
                    </span>
                    <button 
                      onClick={() => setSimulatedScreen('login')}
                      className="bg-white/15 hover:bg-white/25 text-white text-[10px] px-2 py-0.5 rounded font-black border border-white/10"
                    >
                      LOGOUT
                    </button>
                  </div>

                  <div className="flex flex-1 min-h-[380px]">
                    {/* Simulated Left Navigation Menu inside WPF */}
                    <aside className="w-36 bg-zinc-50 border-r text-[11px] font-bold text-zinc-600 p-2 flex flex-col gap-1.5">
                      <div className="text-[10px] font-black uppercase text-zinc-400 mb-1 px-1.5">MENU WPF</div>
                      
                      <button 
                        onClick={() => {
                          setSimulatedScreen('dashboard');
                          setActiveWpfControl("MainViewModel -> SetProperty(ref _currentView, new DashboardViewModel())");
                        }}
                        className={`w-full text-left px-3 py-2 rounded-lg transition-all ${
                          simulatedScreen === 'dashboard' ? 'bg-blue-100 text-blue-800' : 'hover:bg-zinc-200/50'
                        }`}
                      >
                        📊 Dashboard
                      </button>

                      <button 
                        onClick={() => {
                          setSimulatedScreen('employees');
                          setActiveWpfControl("MainViewModel -> ShowEmployeeMenu binding triggers view update (EmployeeViewModel)");
                        }}
                        className={`w-full text-left px-3 py-2 rounded-lg transition-all ${
                          simulatedScreen === 'employees' ? 'bg-blue-100 text-blue-800' : 'hover:bg-zinc-200/50'
                        }`}
                      >
                        👥 Nhân viên
                      </button>

                      <button 
                        onClick={() => {
                          setSimulatedScreen('departments');
                          setActiveWpfControl("MainViewModel -> DepartmentViewModel (EF Core loaded)");
                        }}
                        className={`w-full text-left px-3 py-2 rounded-lg transition-all ${
                          simulatedScreen === 'departments' ? 'bg-blue-100 text-blue-800' : 'hover:bg-zinc-200/50'
                        }`}
                      >
                        🏢 Phòng ban
                      </button>
                    </aside>

                    {/* Sim Workspace Content Pane */}
                    <div className="flex-1 p-5 overflow-y-auto custom-scrollbar">
                      
                      {/* WPF simulated dashboard tab */}
                      {simulatedScreen === 'dashboard' && (
                        <div className="space-y-4 animate-fade-in text-xs font-bold">
                          <h4 className="text-zinc-800 text-sm font-black uppercase tracking-tight border-b pb-1.5">WPF: DashboardView.xaml</h4>
                          <div className="grid grid-cols-2 gap-3">
                            <div className="p-3 bg-blue-50/50 border border-blue-100 rounded-xl space-y-1">
                              <span className="text-zinc-400 font-extrabold uppercase text-[9px] tracking-wider">Tổng nhân sự</span>
                              <div className="text-xl font-black text-blue-800">12 Nhân viên</div>
                              <span className="text-[9px] text-zinc-500 block">Nguồn: SQLite db</span>
                            </div>
                            <div className="p-3 bg-emerald-50/50 border border-emerald-100 rounded-xl space-y-1">
                              <span className="text-zinc-400 font-extrabold uppercase text-[9px] tracking-wider">Số phòng ban</span>
                              <div className="text-xl font-black text-emerald-800">4 Phòng ban</div>
                              <span className="text-[9px] text-zinc-500 block">QL trực tiếp</span>
                            </div>
                          </div>
                          
                          <div className="p-3 bg-amber-50 border border-amber-200 rounded-xl">
                            <div className="text-amber-800 font-black mb-1">⚡ MVVM Data Stream Status:</div>
                            <p className="text-[10px] font-semibold text-zinc-600 leading-relaxed">
                              Khi thực hiện chỉnh sửa, thêm hoặc xóa bên tab ngoài web, trạng thái cơ sở dữ liệu SQLite giả lập đồng bộ tự động và nạp lại vào WPF ViewModel tương ứng.
                            </p>
                          </div>
                        </div>
                      )}

                      {/* WPF simulated employees list */}
                      {simulatedScreen === 'employees' && (
                        <div className="space-y-3 animate-fade-in text-xs">
                          <div className="flex justify-between items-center border-b pb-2">
                            <h4 className="text-zinc-800 text-sm font-black uppercase tracking-tight">WPF: EmployeeView.xaml</h4>
                            <span className="bg-blue-600 text-white min-w-4 px-2.5 py-0.5 rounded text-[8px] font-black uppercase">Admin / Manager role</span>
                          </div>
                          
                          {/* Search mockup inside WPF */}
                          <div className="flex gap-2">
                            <input
                              type="text"
                              disabled
                              placeholder="SearchText bound property..."
                              className="px-2 py-1 bg-zinc-50 border rounded text-[11px] font-bold flex-1"
                            />
                            <button className="bg-blue-600 text-white font-bold px-3 py-1 rounded text-[10px] uppercase">Duyệt</button>
                          </div>

                          {/* DataGrid Simulated */}
                          <div className="border rounded-lg overflow-hidden bg-white text-[10px]">
                            <div className="bg-zinc-100 p-2 font-black grid grid-cols-4 border-b text-zinc-500">
                              <span>MÃ</span>
                              <span>HỌ TÊN</span>
                              <span>CHỨC VỤ</span>
                              <span>MỨC LƯƠNG</span>
                            </div>
                            <div className="divide-y text-zinc-600 font-bold">
                              <div className="p-2 grid grid-cols-4 hover:bg-slate-50 cursor-pointer" onClick={() => setActiveWpfControl("DataGrid.SelectedItem bound to SelectedEmployee property")}>
                                <span>NV001</span>
                                <span>Nguyễn Văn Admin</span>
                                <span>Trưởng phòng</span>
                                <span>20,000,000đ</span>
                              </div>
                              <div className="p-2 grid grid-cols-4 hover:bg-slate-50 cursor-pointer" onClick={() => setActiveWpfControl("DataGrid.SelectedItem bound to SelectedEmployee property")}>
                                <span>NV002</span>
                                <span>Trần Thị Nhân Sự</span>
                                <span>Kế toán viên</span>
                                <span>15,000,000đ</span>
                              </div>
                              <div className="p-2 grid grid-cols-4 hover:bg-slate-50 cursor-pointer" onClick={() => setActiveWpfControl("DataGrid.SelectedItem bound to SelectedEmployee property")}>
                                <span>NV003</span>
                                <span>Lê Văn Quản Lý</span>
                                <span>Lập trình viên</span>
                                <span>18,000,000đ</span>
                              </div>
                            </div>
                          </div>
                        </div>
                      )}

                      {/* WPF simulated departments */}
                      {simulatedScreen === 'departments' && (
                        <div className="space-y-3 animate-fade-in text-xs">
                          <h4 className="text-zinc-800 text-sm font-black uppercase tracking-tight border-b pb-1.5">WPF: DepartmentView.xaml</h4>
                          <div className="grid grid-cols-2 gap-2 text-[10px] font-bold">
                            <div className="p-2.5 bg-zinc-150 border rounded-lg">
                              <div className="font-extrabold text-blue-800">ID 1: Phòng Nhân sự</div>
                              <span className="text-[9px] text-zinc-400 font-mono">CODE: NS_CNPM</span>
                            </div>
                            <div className="p-2.5 bg-zinc-150 border rounded-lg">
                              <div className="font-extrabold text-blue-800">ID 2: Phòng Kinh doanh</div>
                              <span className="text-[9px] text-zinc-400 font-mono">CODE: KD_GLOBAL</span>
                            </div>
                            <div className="p-2.5 bg-zinc-150 border rounded-lg">
                              <div className="font-extrabold text-blue-800">ID 3: Phòng Kỹ thuật IT</div>
                              <span className="text-[9px] text-zinc-400 font-mono">CODE: SYS_ADMIN</span>
                            </div>
                            <div className="p-2.5 bg-zinc-150 border rounded-lg">
                              <div className="font-extrabold text-blue-800">ID 4: Phòng Kế toán</div>
                              <span className="text-[9px] text-zinc-400 font-mono">CODE: KT_MONEY</span>
                            </div>
                          </div>
                        </div>
                      )}

                    </div>
                  </div>
                </div>
              )}

            </div>
          </div>

          {/* Binding telemetry dashboard */}
          <div className="lg:col-span-4 space-y-6">
            <div className="bg-white border-2 border-slate-200 rounded-3xl p-6 shadow-md relative overflow-hidden">
              <div className="absolute top-0 right-0 p-3 text-blue-100">
                <Cpu className="w-16 h-16 opacity-[0.03]" />
              </div>
              <h3 className="text-lg font-black text-slate-800 border-b-2 border-blue-50/50 pb-3 flex items-center gap-2">
                <Cpu className="w-5 h-5 text-blue-600" />
                Bindings Inspector
              </h3>
              
              <div className="mt-4 text-xs space-y-4">
                <p className="font-semibold text-zinc-500 leading-relaxed">
                  Bấm vào bất cứ nút hay trường nhập liệu nào ở mô phỏng WPF bên trái để xem luồng dữ liệu MVVM và Entity Framework được kích hoạt ngay dưới đây:
                </p>

                <div className="p-4 bg-[#0f172a] rounded-xl text-white font-mono space-y-2.5">
                  <div className="text-[10px] text-blue-400 border-b border-white/5 pb-1 font-bold">
                    [WPF TELEMETRY STREAM] — TRẠNG THÁI ACTIVE
                  </div>
                  <div>
                    <span className="text-zinc-500">Interactive Object:</span>
                    <p className="text-emerald-400 font-bold block overflow-x-auto whitespace-pre custom-scrollbar">
                      {activeWpfControl || "Waiting for WPF interaction..."}
                    </p>
                  </div>
                  <div>
                    <span className="text-zinc-500">MVVM Binding:</span>
                    <span className="text-amber-400 block font-bold">
                      {activeWpfControl ? "Two-Way / ICommand Trigger" : "N/A"}
                    </span>
                  </div>
                  <div>
                    <span className="text-zinc-500">DBMS Transaction:</span>
                    <span className="text-cyan-400 block font-bold">
                      {simulatedScreen === 'login' ? "None (Awaiting Login)" : "Entity Framework SQLite Context (Active)"}
                    </span>
                  </div>
                </div>

                <div className="bg-blue-50/80 border border-blue-100 rounded-2xl p-4 text-xs font-bold text-blue-800 space-y-2">
                  <div className="flex items-center gap-1.5 text-blue-900 uppercase">
                    <Info className="w-4 h-4 flex-shrink-0" />
                    <span>Hệ WPF hoạt động thế nào?</span>
                  </div>
                  <p className="text-[11px] leading-relaxed text-zinc-600 font-semibold">
                    C# WPF giao tiếp với **SQL Server hoặc SQLite** cục bộ thông qua **Entity Framework Core (C#)** để xử lý nghiệp vụ, sử dụng **INotifyPropertyChanged** để tự động đồng màu và làm tươi View XAML mà không cần tải lại từng trang một.
                  </p>
                </div>
              </div>
            </div>
          </div>

        </div>
      )}

      {/* 2. WPF CODE FILES EXPLORER */}
      {activeSubTab === 'code' && (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
          
          {/* File Tree Selector Sidebar */}
          <div className="lg:col-span-4 bg-white border border-slate-200 rounded-3xl p-5 shadow-sm space-y-5 max-h-[600px] overflow-y-auto custom-scrollbar">
            
            {/* Folder search wrapper */}
            <div className="relative">
              <input
                type="text"
                placeholder="Tìm tên file, thư mục, code..."
                value={searchTerm}
                onChange={e => handleSearchChange(e.target.value)}
                className="w-full text-xs font-bold pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl outline-none focus:ring-2 focus:ring-blue-600 transition-all font-semibold"
              />
              <span className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400">
                <Eye className="w-4 h-4" />
              </span>
            </div>

            {categories.map(category => {
              const filesInCat = searchCodeResult.filter(f => f.category === category);
              if (filesInCat.length === 0) return null;

              return (
                <div key={category} className="space-y-1.5">
                  <div className="flex items-center gap-1.5 text-xs text-slate-400 font-black uppercase tracking-wider">
                    <Folder className="w-3.5 h-3.5 fill-current text-slate-400" />
                    <span>{category}</span>
                  </div>
                  <div className="space-y-1 pl-4">
                    {filesInCat.map(file => {
                      const isSelected = selectedFile.path === file.path;
                      return (
                        <button
                          key={file.path}
                          onClick={() => setSelectedFile(file)}
                          className={`w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-bold text-left transition-all ${
                            isSelected 
                              ? 'bg-blue-50 text-blue-700 border-l-4 border-blue-600' 
                              : 'hover:bg-slate-50 text-slate-700'
                          }`}
                        >
                          <span className="truncate">{file.name}</span>
                          <span className="text-[10px] font-mono opacity-50 uppercase">{file.language}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>

          {/* Interactive Code Viewer Area */}
          <div className="lg:col-span-8 bg-[#0f172a] rounded-3xl overflow-hidden border-2 border-slate-800 shadow-2xl flex flex-col">
            
            {/* Header toolbar */}
            <div className="bg-slate-900 border-b border-slate-800 px-6 py-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div className="flex items-center gap-3">
                <File className="w-5 h-5 text-blue-400" />
                <div>
                  <h4 className="text-white text-md font-extrabold font-mono">{selectedFile.name}</h4>
                  <p className="text-slate-400 text-xs font-mono">HRM_WPF_CNPM / {selectedFile.path}</p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={handleCopyCode}
                  className="bg-slate-800 hover:bg-slate-750 text-slate-300 px-4 py-2 border border-slate-700 rounded-xl text-xs font-bold transition-all flex items-center gap-2"
                >
                  <Copy className="w-4 h-4 text-slate-300" />
                  {copied ? 'Đã sao chép!' : 'Sao chép raw'}
                </button>
              </div>
            </div>

            {/* View description of file role */}
            <div className="bg-blue-950/40 px-6 py-4 border-b border-blue-900/30 text-xs font-bold text-blue-200 leading-relaxed flex gap-3">
              <BookOpen className="w-6 h-6 text-blue-400 flex-shrink-0" />
              <div>
                <span className="uppercase text-[9px] text-blue-400 font-extrabold block mb-0.5 tracking-wider">Mô tả kiến trúc:</span>
                <p className="text-slate-200 font-semibold">{selectedFile.explanation}</p>
              </div>
            </div>

            {/* Scrollable Highlight Code block style */}
            <div className="p-6 bg-[#090d16] font-mono text-xs leading-relaxed overflow-x-auto text-slate-350 max-h-[480px] custom-scrollbar">
              <pre className="grid grid-cols-1">
                <code>
                  {selectedFile.content.split('\n').map((line, idx) => (
                    <div key={idx} className="table-row hover:bg-slate-900/40">
                      <span className="table-cell select-none pr-4 text-right text-slate-600 font-bold font-mono text-[10px] w-8">
                        {idx + 1}
                      </span>
                      <span className="table-cell whitespace-pre font-mono font-bold">{line}</span>
                    </div>
                  ))}
                </code>
              </pre>
            </div>
          </div>

        </div>
      )}

      {/* 3. WPF MVVM FLOW DESIGN */}
      {activeSubTab === 'architecture' && (
        <div className="bg-white border border-slate-200 rounded-3xl p-8 shadow-sm space-y-8 animate-fade-in font-sans">
          <div className="space-y-2 border-b-2 border-slate-50 pb-4">
            <h2 className="text-2xl font-black text-slate-900 uppercase tracking-tight">Cơ chế Binding MVVM trong WPF</h2>
            <p className="text-slate-500 text-sm font-semibold max-w-3xl leading-relaxed">
              Mô hình MVVM (Model - View - ViewModel) tách biệt rạch ròi giao diện (XAML), nghiệp vụ (ViewModel) và cơ sở thực thể dữ liệu (Model). Giúp WPF đạt độ mượt tuyệt hảo và loại trừ viết mã rườm rà ở file Code-Behind `.xaml.cs`.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 relative">
            
            {/* Box 1: VIEW (XAML) */}
            <div className="bg-pink-50/50 border-2 border-pink-200 p-6 rounded-2xl relative space-y-3 shadow-inner">
              <span className="absolute -top-3 left-4 bg-pink-600 text-white text-[9px] font-black px-2.5 py-0.5 rounded-full uppercase tracking-wider">
                1. View (XAML)
              </span>
              <div className="flex items-center gap-2 pt-2 text-[#b0225d]">
                <Monitor className="w-5 h-5" />
                <h4 className="font-extrabold text-md uppercase">Màn hình WPF</h4>
              </div>
              <p className="text-xs text-zinc-600 leading-relaxed font-semibold">
                Được viết hoàn toàn bằng định dạng XAML (`LoginWindow.xaml`, `MainWindow.xaml`). Chứa các control ràng buộc dữ liệu:
              </p>
              <div className="p-3 bg-white border border-pink-100 rounded-xl space-y-1.5 text-[11px] font-mono font-bold text-pink-700">
                <div>&lt;TextBox Text=&quot;&#123;Binding Username&#125;&quot;/&gt;</div>
                <div>&lt;Button Command=&quot;&#123;Binding LoginCommand&#125;&quot;/&gt;</div>
              </div>
              <p className="text-[10px] text-zinc-400">
                Ràng buộc chặt với ViewModel qua thuộc tính DataContext của Window.
              </p>
            </div>

            {/* Box 2: VIEWMODEL (C#) */}
            <div className="bg-purple-50/50 border-2 border-purple-200 p-6 rounded-2xl relative space-y-3 shadow-inner">
              <span className="absolute -top-3 left-4 bg-purple-600 text-white text-[9px] font-black px-2.5 py-0.5 rounded-full uppercase tracking-wider">
                2. ViewModel (C#)
              </span>
              <div className="flex items-center gap-2 pt-2 text-[#631fb2]">
                <Cpu className="w-5 h-5 text-purple-600" />
                <h4 className="font-extrabold text-md uppercase">Nghiệp vụ luồng</h4>
              </div>
              <p className="text-xs text-zinc-600 leading-relaxed font-semibold">
                Lớp xử lý logic, triển khai thuộc tính kiểu `BaseViewModel` và lệnh điều hướng kiểu `RelayCommand` để hứng hành động:
              </p>
              <div className="p-3 bg-white border border-purple-150 rounded-xl space-y-1 text-[11px] font-mono font-bold text-purple-700 leading-relaxed">
                <div>public string Username &#123; get; set; &#125;</div>
                <div>public ICommand LoginCommand &#123; get; &#125;</div>
              </div>
              <p className="text-[10px] text-zinc-400">
                Thúc đẩy sự kiện `OnPropertyChanged` để phản hồi ngược dữ liệu lên XAML cực kỳ êm ái.
              </p>
            </div>

            {/* Box 3: MODEL (Database) */}
            <div className="bg-blue-50/50 border-2 border-blue-200 p-6 rounded-2xl relative space-y-3 shadow-inner">
              <span className="absolute -top-3 left-4 bg-blue-600 text-white text-[9px] font-black px-2.5 py-0.5 rounded-full uppercase tracking-wider">
                3. Model (DBMS)
              </span>
              <div className="flex items-center gap-2 pt-2 text-[#1f4db2]">
                <Database className="w-5 h-5 text-blue-600" />
                <h4 className="font-extrabold text-md uppercase">Cơ sở dữ liệu</h4>
              </div>
              <p className="text-xs text-zinc-600 leading-relaxed font-semibold">
                Lớp thực thể đại diện cho cấu trúc bảng cơ sở dữ liệu (`Employee.cs`, `User.cs`).
              </p>
              <div className="p-3 bg-white border border-blue-150 rounded-xl space-y-1.5 text-[11px] font-mono font-bold text-blue-700">
                <div>public class Employee &#123;&#125;</div>
                <div>DbSet&lt;Employee&gt; Employees</div>
              </div>
              <p className="text-[10px] text-zinc-400">
                Sử dụng bộ kịch khung Entity Framework Core kết nối bảo toàn SQL Server, thực thi di trú và đóng gói bảng ghi.
              </p>
            </div>

          </div>

          {/* Binding explanation summary */}
          <div className="bg-amber-50 border border-amber-200 p-6 rounded-2xl flex items-start gap-4">
            <Info className="w-8 h-8 text-amber-600 flex-shrink-0 mt-0.5" />
            <div className="space-y-1 text-xs">
              <h4 className="font-black text-amber-900 uppercase">Quy chuẩn MVVM Databinding &amp; ICommand - Sức mạnh của .NET WPF:</h4>
              <p className="font-semibold text-zinc-600 leading-relaxed">
                Khi người dùng tương tác với Control trên **WPF View**, `RelayCommand` sẽ bắt các lệnh click chuột để kích hoạt các Hàm điều khiển trong **ViewModel**. ViewModel sau đó sử dụng các **Services** có chứa **EntityFramework (EF)** kết nối vào lớp **Model** truy vấn database. Dữ liệu quay trở lại được ViewModel cập nhật vào các thuộc tính kích hoạt `OnPropertyChanged` để tự động dội luồng hiển thị giao dịch ngược lại lên View mà không cần viết mã gán tĩnh.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* 4. DOTNET BUILD TOOLCHAIN PREVIEW */}
      {activeSubTab === 'build' && (
        <div className="bg-white border border-slate-200 rounded-3xl p-6 shadow-sm space-y-6 animate-fade-in font-sans">
          
          <div className="space-y-1">
            <h2 className="text-lg font-black text-slate-800 uppercase tracking-tight">dotnet Core Build &amp; Compile Console</h2>
            <p className="text-xs text-zinc-500 font-bold">Mô phỏng trình phát triển, build code, sinh DLL và chạy database migrations cho WPF</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <button
              onClick={() => runTerminalCommand('dotnet build')}
              disabled={isBuilding}
              className="bg-blue-600 hover:bg-blue-700 disabled:bg-slate-300 text-white p-3 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-2 shadow"
            >
              <Cpu className="w-4 h-4 text-white" />
              dotnet build (Biên dịch WPF)
            </button>
            <button
              onClick={() => runTerminalCommand('dotnet run')}
              disabled={isBuilding}
              className="bg-emerald-600 hover:bg-emerald-700 disabled:bg-slate-300 text-white p-3 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-2 shadow"
            >
              <Play className="w-4 h-4 text-white" />
              dotnet run (Chạy Simulator)
            </button>
            <button
              onClick={() => runTerminalCommand('dotnet ef migrations list')}
              disabled={isBuilding}
              className="bg-indigo-600 hover:bg-indigo-700 disabled:bg-slate-300 text-white p-3 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-2 shadow"
            >
              <Database className="w-4 h-4 text-white" />
              ef migrations list (EF Core)
            </button>
            <button
              onClick={() => runTerminalCommand('clear')}
              className="bg-zinc-700 hover:bg-zinc-800 text-white p-3 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-2 shadow"
            >
              <Terminal className="w-4 h-4 text-white" />
              Xóa lịch sử Console
            </button>
          </div>

          {/* Terminal Block */}
          <div className="bg-[#0b0f19] text-emerald-450 p-5 rounded-2xl border-2 border-slate-850 font-mono text-[11px] leading-relaxed shadow-inner min-h-[300px] flex flex-col justify-between">
            <div className="space-y-2 overflow-y-auto max-h-[350px] custom-scrollbar pb-4 flex-1">
              {terminalLogs.map((log, idx) => (
                <div key={idx} className="whitespace-pre-wrap font-mono font-semibold">
                  {log}
                </div>
              ))}
              {isBuilding && (
                <div className="text-blue-400 font-mono animate-pulse">
                  ⚙️ Đang xử lý biên dịch thư viện, tệp tin và cơ sở dữ liệu .NET Core...
                </div>
              )}
            </div>

            {/* Input Line */}
            <form 
              onSubmit={(e) => {
                e.preventDefault();
                runTerminalCommand(terminalInput);
              }}
              className="border-t border-slate-800 pt-3 flex items-center gap-2"
            >
              <span className="text-blue-400 font-bold font-mono">C:\Projects\HRM_WPF_CNPM&gt;</span>
              <input
                type="text"
                value={terminalInput}
                onChange={e => setTerminalInput(e.target.value)}
                placeholder="Nhập lệnh (ví dụ: dotnet build, dotnet run, clear)..."
                className="bg-transparent border-none text-emerald-400 font-mono text-xs focus:ring-0 focus:outline-none flex-1 font-semibold"
              />
            </form>
          </div>

        </div>
      )}

    </div>
  );
}
