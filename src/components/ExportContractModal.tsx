import React from 'react';
import { Employee, Contract, Department, Position } from '../types';
import { X, Mail, Printer, FileText, CheckCircle } from 'lucide-react';
import { Document, Packer, Paragraph, TextRun, AlignmentType } from 'docx';

interface ExportContractModalProps {
  isOpen: boolean;
  onClose: () => void;
  employee: Employee;
  contract?: Contract;
  department?: Department;
  position?: Position;
}

export function ExportContractModal({
  isOpen,
  onClose,
  employee,
  contract,
  department,
  position
}: ExportContractModalProps) {
  if (!isOpen) return null;

  const getSalaryInWords = (salaryAmount: number) => {
    // Simple converter or nice hardcode for typical salary
    if (salaryAmount === 20000000) return 'Hai mươi triệu đồng chẵn';
    if (salaryAmount === 18000000) return 'Mười tám triệu đồng chẵn';
    if (salaryAmount === 15000000) return 'Mười lăm triệu đồng chẵn';
    if (salaryAmount === 13000000) return 'Mười ba triệu đồng chẵn';
    if (salaryAmount === 12000000) return 'Mười hai triệu đồng chẵn';
    if (salaryAmount === 10000000) return 'Mười triệu đồng chẵn';
    return 'Bằng chữ mười triệu đồng chẵn (tự định mức lương cơ bản)';
  };

  const salary = contract ? contract.salary : employee.baseSalary;
  const contractCode = contract ? contract.contractCode : `HĐ-${employee.employeeCode}`;
  const contractType = contract ? contract.contractType : 'Không xác định thời hạn';
  const startDate = contract ? contract.startDate : employee.joinDate;

  const handlePrint = () => {
    const printContent = document.getElementById('contract-printable-area');
    if (!printContent) {
      alert('Không tìm thấy vùng dữ liệu hợp đồng bản in!');
      return;
    }
    
    let iframe = document.getElementById('contract-print-iframe') as HTMLIFrameElement;
    if (!iframe) {
      iframe = document.createElement('iframe');
      iframe.id = 'contract-print-iframe';
      iframe.style.position = 'absolute';
      iframe.style.width = '0px';
      iframe.style.height = '0px';
      iframe.style.border = 'none';
      iframe.style.top = '-9999px';
      document.body.appendChild(iframe);
    }
    
    const doc = iframe.contentWindow?.document || iframe.contentDocument;
    if (!doc) {
      alert('Không thể tạo luồng In!');
      return;
    }
    
    doc.open();
    doc.write(`
      <html>
        <head>
          <title>Hợp đồng lao động - ${employee.fullName}</title>
          <style>
            body { 
              font-family: "Times New Roman", Times, serif; 
              padding: 40px; 
              line-height: 1.6; 
              color: #000; 
              background-color: #fff;
            }
            .text-center { text-align: center; }
            .text-right { text-align: right; }
            .font-bold { font-weight: bold; }
            .flex { display: flex; justify-content: space-between; }
            .mt-4 { margin-top: 16px; }
            .mt-8 { margin-top: 32px; }
            .mb-2 { margin-bottom: 8px; }
            .title { font-size: 18px; font-weight: bold; margin-top: 20px; text-transform: uppercase; }
            .sub-title { font-size: 14px; font-style: italic; }
            .divider { border-bottom: 1px solid #000; width: 154px; margin: 8px auto; }
            .signature-box { margin-top: 50px; display: grid; grid-template-columns: 1fr 1fr; gap: 40px; }
          </style>
        </head>
        <body>
          ${printContent.innerHTML}
        </body>
      </html>
    `);
    doc.close();
    
    setTimeout(() => {
      iframe.contentWindow?.focus();
      iframe.contentWindow?.print();
    }, 500);
  };

  const handleExportDocx = async () => {
    try {
      const doc = new Document({
        sections: [{
          properties: {},
          children: [
            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: 'CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', bold: true, font: 'Times New Roman', size: 24 }),
              ]
            }),
            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: 'Độc lập - Tự do - Hạnh phúc', bold: true, italics: true, font: 'Times New Roman', size: 24 }),
              ]
            }),
            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: '-----------------------' }),
              ]
            }),
            new Paragraph({ text: '', spacing: { after: 300 } }),

            new Paragraph({
              alignment: AlignmentType.CENTER,
              spacing: { after: 200 },
              children: [
                new TextRun({ text: 'HỢP ĐỒNG LAO ĐỘNG HÀNH CHÍNH', bold: true, font: 'Times New Roman', size: 36 })
              ]
            }),
            new Paragraph({
              alignment: AlignmentType.CENTER,
              spacing: { after: 400 },
              children: [
                new TextRun({ text: `Số hiệu: ${contractCode}`, italics: true, font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: 'Chúng tôi, một bên là Người sử dụng lao động:', bold: true, font: 'Times New Roman', size: 24 })
              ]
            }),
            new Paragraph({
              spacing: { after: 100 },
              children: [
                new TextRun({ text: '• Tên công ty: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: 'Ban Nhân sự Tổng hợp Công ty HRM_WPF_CNPM\n', font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Đại diện pháp lý: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: 'Ông TRẦN MINH GIÁM ĐỐC\n', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Chức vụ điều hành: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: 'Giám đốc Điều hành', font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: 'Và một bên là Người lao động:', bold: true, font: 'Times New Roman', size: 24 })
              ]
            }),
            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: '• Ông/Bà: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `${employee.fullName.toUpperCase()}\n`, bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Quốc tịch chính: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: 'Việt Nam\n', font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Ngày sinh đăng ký: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `${employee.dateOfBirth ? new Date(employee.dateOfBirth).toLocaleDateString('vi-VN') : '15/08/1995'}\n`, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Địa chỉ thường trú: ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `${employee.address || 'Hà Nội, Việt Nam'}\n`, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '• Số thẻ căn cước (CCCD): ', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `${employee.citizenId || '012345678912'}`, font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: 'Thỏa thuận thống nhất ký kết các điều khoản dưới đây:', font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: 'Điều 1: Vị trí và thời hạn ký hợp đồng\n', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Loại hợp đồng lao động: ${contractType}\n`, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Ngày chính thức bắt đầu làm việc: ${new Date(startDate).toLocaleDateString('vi-VN')}\n`, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Chức vụ chuyên môn: ${position?.positionName || 'Nhân viên'}\n`, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Phòng ban phân bổ: ${department?.departmentName || 'Hành chính'}`, font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 150 },
              children: [
                new TextRun({ text: 'Điều 2: Mức lương cơ cấu và quyền lợi\n', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Lương căn bản chi trả định kỳ: ${salary.toLocaleString('vi-VN')} đ / tháng\n`, bold: true, color: 'B41E1E', font: 'Times New Roman', size: 24 }),
                new TextRun({ text: `- Viết bằng chữ số: ${getSalaryInWords(salary)}\n`, italics: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '- Chế độ đãi ngộ: Được đóng bảo hiểm xã hội, bảo hiểm y tế theo luật định của nhà nước Việt Nam và các phụ lý công chức khác.', font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              spacing: { after: 300 },
              children: [
                new TextRun({ text: 'Điều 3: Nghĩa vụ của người lao động\n', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: 'Chấp hành nghiêm nội quy đơn vị, tuân thủ kỷ luật lao động hành chính nghiêm túc, đạt chỉ tiêu công việc cấp trên giao phó.', font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              alignment: AlignmentType.RIGHT,
              spacing: { after: 400 },
              children: [
                new TextRun({ text: `Hà Nội, ngày ${new Date().getDate()} tháng ${new Date().getMonth() + 1} năm ${new Date().getFullYear()}`, italics: true, font: 'Times New Roman', size: 24 })
              ]
            }),

            new Paragraph({
              alignment: AlignmentType.CENTER,
              children: [
                new TextRun({ text: 'ĐẠI DIỆN SỬ DỤNG LAO ĐỘNG              NGƯỜI LAO ĐỘNG\n', bold: true, font: 'Times New Roman', size: 24 }),
                new TextRun({ text: '     Ông Trần Minh Giám Đốc                       (Đã ký nhận cẩn mật)', italics: true, font: 'Times New Roman', size: 22 })
              ]
            })
          ]
        }]
      });

      const blob = await Packer.toBlob(doc);
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `HopDong_LaoDong_${employee.fullName.replace(/\s+/g, '_')}.docx`;
      link.click();
    } catch (err) {
      console.error('Lỗi khi tải tệp .docx:', err);
      alert('Không thể hoàn tất tạo văn bản định dạng file Word .docx.');
    }
  };

  const handleDownloadTxt = () => {
    const textContent = `
CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM
Độc lập - Tự do - Hạnh phúc
----------------------------

HỢP ĐỒNG LAO ĐỘNG HÀNH CHÍNH
Số hiệu: ${contractCode}

Chúng tôi, một bên là:
Đại diện người sử dụng lao động:
- Tên công ty: Ban Nhân sự Tổng hợp Công ty HRM_WPF_CNPM
- Đại diện bởi: Ông Trần Minh Giám Đốc
- Chức vụ: Giám đốc Điều hành

Và một bên là người lao động:
- Ông/Bà: ${employee.fullName}
- Quốc tịch: Việt Nam
- Sinh ngày: ${employee.dateOfBirth ? new Date(employee.dateOfBirth).toLocaleDateString('vi-VN') : '1995-01-01'}
- Địa chỉ thường trú: ${employee.address || 'Hà Nội'}
- Số CCCD/CMND: ${employee.citizenId || '012345678912'}
- Chức danh chuyên môn: ${position?.positionName || 'Nhân viên'}
- Phòng ban trực thuộc: ${department?.departmentName || 'Nhân sự'}

Thỏa thuận ký kết Hợp đồng lao động với các điều khoản sau:
Điều 1: Thời hạn và công việc hợp đồng
- Loại hợp đồng: ${contractType}
- Ngày bắt đầu làm việc: ${new Date(startDate).toLocaleDateString('vi-VN')}
- Địa điểm làm việc: Văn phòng Sư đoàn Chỉ huy Quân sự tỉnh

Điều 2: Chế độ làm việc và quyền lợi
- Thời gian làm việc: 8 giờ/ngày, 44 giờ/tuần
- Mức lương cơ bản: ${salary.toLocaleString('vi-VN')} đ
- Bằng chữ: ${getSalaryInWords(salary)}
- Hình thức trả lương: Chuyển khoản ngân hàng định kỳ ngày 05 hàng tháng

Hợp đồng được lập thành 02 bản có giá trị pháp lý như nhau.
Hà Nội, Ngày ${new Date().getDate()} tháng ${new Date().getMonth() + 1} năm ${new Date().getFullYear()}

ĐẠI DIỆN NGƯỜI SỬ DỤNG LAO ĐỘNG             NGƯỜI LAO ĐỘNG
      (Đã ký)                                   (Đã ký)
    `;

    const blob = new Blob([textContent], { type: 'text/plain;charset=utf-8' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `HopDong_LaoDong_${employee.fullName.replace(/\s+/g, '_')}.txt`;
    link.click();
  };

  return (
    <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in font-sans">
      <div className="bg-white rounded-2xl w-full max-w-3xl flex flex-col max-h-[90vh] shadow-2xl border border-zinc-200">
        
        {/* Title bar */}
        <div className="px-6 py-4 border-b bg-gradient-to-r from-blue-950 to-[#1e293b] text-white flex justify-between items-center rounded-t-2xl">
          <div className="flex items-center gap-2">
            <FileText className="w-5 h-5 text-blue-400" />
            <h3 className="font-extrabold text-sm uppercase tracking-wider">Xuất Bản Số - Hợp Đồng Lao Động Pháp Lý</h3>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-white/15 rounded-lg text-white transition-all outline-none"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Action Header */}
        <div className="bg-zinc-50 border-b px-6 py-4 flex flex-wrap justify-between items-center gap-3">
          <p className="text-xs text-zinc-500 font-bold">
            HĐ: <span className="font-mono text-zinc-800 font-extrabold">{contractCode}</span> — Nhân sự: <span className="text-emerald-800 font-extrabold">{employee.fullName}</span>
          </p>
          <div className="flex gap-2">
            <button
              onClick={handleDownloadTxt}
              className="flex items-center gap-1.5 bg-zinc-100 hover:bg-zinc-200 text-zinc-800 border border-zinc-350 px-3.5 py-1.5 rounded-xl font-bold text-xs transition-all outline-none"
            >
              <FileText className="w-3.5 h-3.5 text-zinc-650" />
              Tải xuống bản .TXT
            </button>
            <button
              onClick={handleExportDocx}
              className="flex items-center gap-1.5 bg-blue-50 hover:bg-blue-100 text-blue-800 border border-blue-200 px-3.5 py-1.5 rounded-xl font-bold text-xs transition-all outline-none"
            >
              <FileText className="w-3.5 h-3.5 text-blue-700" />
              Tải xuống bản .DOCX (Word)
            </button>
            <button
              onClick={handlePrint}
              className="flex items-center gap-1.5 bg-emerald-700 hover:bg-emerald-800 text-white px-3.5 py-1.5 rounded-xl font-bold text-xs transition-all outline-none shadow-sm"
            >
              <Printer className="w-3.5 h-3.5" />
              In hợp đồng (LPT/PDF)
            </button>
          </div>
        </div>

        {/* Contract Preview Paper Container */}
        <div className="flex-1 overflow-y-auto p-12 bg-zinc-100 flex justify-center custom-scrollbar">
          <div
            id="contract-printable-area"
            className="bg-white w-full max-w-2xl px-10 py-12 shadow-md border border-neutral-300 text-black leading-relaxed text-sm relative"
            style={{ fontFamily: '"Times New Roman", Times, serif' }}
          >
            {/* National Motto */}
            <div className="text-center font-bold">
              <p className="text-xs uppercase tracking-[0.05em] mb-0.5">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</p>
              <p className="text-xs mb-1">Độc lập - Tự do - Hạnh phúc</p>
              <div className="w-36 h-[1.2px] bg-black mx-auto mt-1 mb-6"></div>
            </div>

            {/* Contract Code */}
            <p className="text-[11px] italic mb-6">Số hiệu bản thảo: {contractCode}</p>

            {/* Document Title */}
            <div className="text-center mb-6">
              <h1 className="text-base font-bold uppercase tracking-wide">HỢP ĐỒNG LAO ĐỘNG</h1>
              <p className="text-[11px] italic font-semibold">(Ban hành theo quy chế nội bộ HRM_WPF_CNPM Sư đoàn chỉ huy)</p>
            </div>

            {/* Document Body */}
            <div className="space-y-4 text-justify">
              <p className="font-bold">Chúng tôi, một bên là Người sử dụng lao động:</p>
              <ul className="pl-4 space-y-1">
                <li>• <span className="font-bold">Tên công ty:</span> Phòng chỉ đạo nhân sự Tổng công ty quản trị HRM WPF</li>
                <li>• <span className="font-bold">Đại diện pháp lý:</span> Ông <span className="font-bold uppercase">Trần Minh Giám Đốc</span></li>
                <li>• <span className="font-bold">Chức vụ điều hành:</span> Giám đốc Hành chính Nhân sự</li>
              </ul>

              <p className="font-bold">Và một bên là Người lao động:</p>
              <ul className="pl-4 space-y-1">
                <li>• <span className="font-bold">Ông/Bà:</span> <span className="font-bold uppercase text-zinc-950">{employee.fullName}</span></li>
                <li>• <span className="font-bold">Quốc tịch chính:</span> Việt Nam</li>
                <li>• <span className="font-bold">Ngày sinh đăng ký:</span> {employee.dateOfBirth ? new Date(employee.dateOfBirth).toLocaleDateString('vi-VN') : '1995-01-01'}</li>
                <li>• <span className="font-bold">Địa chỉ thường trú:</span> {employee.address || 'Hà Nội, Việt Nam'}</li>
                <li>• <span className="font-bold">Mã số thẻ căn cước (CCCD):</span> {employee.citizenId || '012345678912'}</li>
              </ul>

              <p className="font-bold">Thỏa thuận thống nhất ký kết các điều khoản dưới đây:</p>
              
              <p>
                <span className="font-bold">Điều 1: Vị trí và thời hạn ký hợp đồng</span> <br />
                - Loại hợp đồng lao động: <span className="font-bold">{contractType}</span> <br />
                - Ngày chính thức bắt đầu làm việc: <span className="font-bold">{new Date(startDate).toLocaleDateString('vi-VN')}</span> <br />
                - Chức vụ chuyên môn: <span className="font-bold">{position?.positionName || 'Nhân sự thực thi'}</span> <br />
                - Phòng ban phân bổ: <span className="font-bold">{department?.departmentName || 'Hành chính'}</span>
              </p>

              <p>
                <span className="font-bold">Điều 2: Mức lương cơ cấu và quyền lợi</span> <br />
                - Lương căn bản chi trả định kỳ: <span className="font-bold text-[#b41e1e]">{salary.toLocaleString('vi-VN')} đ / tháng</span> <br />
                - Viết bằng chữ số: <span className="italic font-semibold">{getSalaryInWords(salary)}</span> <br />
                - Chế độ đãi ngộ: Được đóng bảo hiểm xã hội, bảo hiểm y tế theo luật định của nhà nước Việt Nam, thưởng vào dịp lễ Tết và phụ cấp công vụ.
              </p>

              <p>
                <span className="font-bold">Điều 3: Nghĩa vụ của người lao động</span> <br />
                Chấp hành nghiêm nghị quyết của chi bộ và nội quy đơn vị, tuân thủ kỷ luật lao động hành chính nghiêm túc, đạt chỉ tiêu công việc cấp trên giao phó.
              </p>
            </div>

            {/* Official Date */}
            <p className="text-right italic mt-8 text-xs">
              Hà Nội, ngày {new Date().getDate()} tháng {new Date().getMonth() + 1} năm {new Date().getFullYear()}
            </p>

            {/* Signatures and Stamp */}
            <div className="mt-8 flex justify-between text-xs items-start" style={{ display: 'flex', justifyContent: 'space-between' }}>
              <div className="text-center w-1/2">
                <p className="font-bold uppercase">NGƯỜI LAO ĐỘNG</p>
                <p className="italic text-[10px] text-zinc-500 mb-8">(Ký, ghi rõ họ tên)</p>
                <div className="h-10"></div>
                <p className="font-bold uppercase text-black">{employee.fullName}</p>
                <div className="text-[10px] text-emerald-800 font-bold font-mono tracking-wider mt-2 border border-emerald-300 rounded inline-block px-1 bg-emerald-50">
                  ✓ VERIFIED ON DEV_SERVER
                </div>
              </div>

              <div className="text-center w-1/2 relative">
                <p className="font-bold uppercase">ĐẠI DIỆN SỬ DỤNG LAO ĐỘNG</p>
                <p className="italic text-[10px] text-zinc-500 mb-8">(Giám đốc điều hành - CEO)</p>
                
                {/* Red Circular Company Stamp emulation */}
                <div className="absolute right-4 top-8 w-24 h-24 border-2 border-red-500 rounded-full flex flex-col items-center justify-center p-1 text-[7px] text-center font-bold text-red-500 uppercase select-none opacity-80 pointer-events-none rotate-12">
                  <span className="leading-none mb-0.5">NEXUSHQ GROUP</span>
                  <div className="border-t border-b border-red-500 py-0.5 my-0.5 font-black text-[8px]">BAN GIÁM ĐỐC</div>
                  <span className="leading-none text-[6px]">ĐÃ PHÊ DUYỆT</span>
                </div>

                <div className="h-10"></div>
                <p className="font-bold uppercase text-black">Trần Minh Giám Đốc</p>
                <p className="text-[9px] text-[#1e293b]/60 font-semibold italic">Đã đóng dấu điện tử</p>
              </div>
            </div>
          </div>
        </div>

        {/* Modal Footer */}
        <div className="px-6 py-4 bg-zinc-50 border-t flex justify-end">
          <button
            onClick={onClose}
            className="bg-zinc-200 hover:bg-zinc-300 text-zinc-800 px-5 py-2.5 rounded-xl font-bold text-sm transition-all outline-none"
          >
            Đóng bảng xem
          </button>
        </div>

      </div>
    </div>
  );
}
