import { Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, WidthType, AlignmentType, BorderStyle } from 'docx';
import ExcelJS from 'exceljs';
import pptxgen from 'pptxgenjs';

/**
 * UTILS FOR NATIVE CLIENT-SIDE EXPORTS
 * Uses docx, exceljs, and pptxgenjs to build high-quality reports that represent direct live data.
 */

// Helper: safe string converter
const cleanString = (val: any) => {
  if (val === null || val === undefined) return '';
  return String(val);
};

/**
 * EXPORT TO EXCEL (.xlsx)
 * Generates structured spreadsheets with standard header formatting, bold styles, auto columns.
 */
export async function exportToExcel(
  title: string,
  headers: { header: string; key: string; width: number }[],
  data: any[],
  fileName: string
) {
  try {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Báo cáo');

    // Title Row
    const titleRow = worksheet.addRow([title]);
    titleRow.font = { name: 'Arial', size: 16, bold: true, color: { argb: '1B4D22' } };
    worksheet.mergeCells(`A1:${String.fromCharCode(65 + headers.length - 1)}1`);
    worksheet.addRow([]); // Blank spacer

    // Meta Info Row
    const metaRow = worksheet.addRow([`Ngày xuất: ${new Date().toLocaleDateString('vi-VN')} - Hệ thống quản lý HRM CNPM`]);
    metaRow.font = { name: 'Arial', size: 10, italic: true, color: { argb: '555555' } };
    worksheet.mergeCells(`A3:${String.fromCharCode(65 + headers.length - 1)}3`);
    worksheet.addRow([]); // Blank spacer

    // Header Row
    const headerRow = worksheet.addRow(headers.map(h => h.header));
    headerRow.font = { name: 'Arial', size: 11, bold: true, color: { argb: 'FFFFFF' } };
    headerRow.eachCell((cell) => {
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: '2D3A2D' } // Forest Green/Charcoal brand background
      };
      cell.alignment = { vertical: 'middle', horizontal: 'center' };
      cell.border = {
        top: { style: 'thin', color: { argb: 'CCCCCC' } },
        bottom: { style: 'medium', color: { argb: '111111' } },
        left: { style: 'thin', color: { argb: 'CCCCCC' } },
        right: { style: 'thin', color: { argb: 'CCCCCC' } }
      };
    });
    headerRow.height = 25;

    // Add Data Rows
    data.forEach(item => {
      const rowValues = headers.map(h => cleanString(item[h.key]));
      const row = worksheet.addRow(rowValues);
      row.font = { name: 'Arial', size: 10 };
      row.eachCell((cell) => {
        cell.border = {
          top: { style: 'thin', color: { argb: 'E5E7EB' } },
          bottom: { style: 'thin', color: { argb: 'E5E7EB' } },
          left: { style: 'thin', color: { argb: 'E5E7EB' } },
          right: { style: 'thin', color: { argb: 'E5E7EB' } }
        };
        cell.alignment = { vertical: 'middle', wrapText: true };
      });
      row.height = 22;
    });

    // Auto-fit Columns width
    headers.forEach((h, index) => {
      const col = worksheet.getColumn(index + 1);
      col.width = h.width || 15;
    });

    // Write to Buffer and save
    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName.endsWith('.xlsx') ? fileName : `${fileName}.xlsx`;
    link.click();
  } catch (err) {
    console.error('Lỗi xuất Excel:', err);
    alert('Không thể xuất tệp Excel. Chi tiết lỗi đã được ghi vào hệ thống.');
  }
}

/**
 * EXPORT TO WORD (.docx)
 * Generates beautiful, styled legal or report documents using Microsoft Word structures.
 */
export async function exportToWord(
  title: string,
  headers: string[],
  rows: string[][],
  fileName: string,
  summaryText: string = ''
) {
  try {
    const tableHeaderCells = headers.map(header => (
      new TableCell({
        children: [
          new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({
                text: header,
                bold: true,
                color: 'FFFFFF',
                font: 'Arial',
                size: 20 // 10pt
              })
            ]
          })
        ],
        shading: { fill: '2D3A2D' } // Forest Green Brand Header
      })
    ));

    const tableRows = [
      new TableRow({ children: tableHeaderCells })
    ];

    rows.forEach(rowData => {
      const cells = rowData.map(text => (
        new TableCell({
          children: [
            new Paragraph({
              alignment: AlignmentType.LEFT,
              children: [
                new TextRun({
                  text: text,
                  font: 'Arial',
                  size: 18 // 9pt
                })
              ]
            })
          ],
          margins: { top: 100, bottom: 100, left: 150, right: 150 }
        })
      ));
      tableRows.push(new TableRow({ children: cells }));
    });

    const doc = new Document({
      sections: [{
        properties: {},
        children: [
          // National Title
          new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({ text: 'CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', bold: true, font: 'Arial', size: 24 }),
            ]
          }),
          new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({ text: 'Độc lập - Tự do - Hạnh phúc', bold: true, italics: true, font: 'Arial', size: 22 }),
            ]
          }),
          new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({ text: '-----------------------' }),
            ]
          }),
          new Paragraph({ text: '', spacing: { after: 300 } }),

          // Report Title
          new Paragraph({
            alignment: AlignmentType.CENTER,
            spacing: { after: 200 },
            children: [
              new TextRun({ text: title.toUpperCase(), bold: true, color: '1B4D22', font: 'Arial', size: 32 })
            ]
          }),

          // Date & Metadata info
          new Paragraph({
            alignment: AlignmentType.RIGHT,
            spacing: { after: 300 },
            children: [
              new TextRun({ text: `Ngày xuất báo cáo: ${new Date().toLocaleDateString('vi-VN')} \n`, italics: true, font: 'Arial', size: 18 }),
              new TextRun({ text: 'Người lập biểu: Hệ thống phần mềm quản trị HRM CNPM', font: 'Arial', size: 18, italics: true })
            ]
          }),

          // Optional summary text card
          ...(summaryText ? [
            new Paragraph({
              spacing: { after: 200 },
              children: [
                new TextRun({ text: 'Tóm tắt dữ liệu thống kê: ', bold: true, font: 'Arial', size: 20 }),
                new TextRun({ text: summaryText, italics: true, font: 'Arial', size: 18 })
              ]
            })
          ] : []),

          // Data Table
          new Table({
            width: { size: 100, type: WidthType.PERCENTAGE },
            rows: tableRows
          }),

          new Paragraph({ text: '', spacing: { after: 400 } }),

          // Signatures Section
          new Paragraph({
            alignment: AlignmentType.RIGHT,
            children: [
              new TextRun({ text: 'Người Phê Duyệt Hệ Thống \n', bold: true, font: 'Arial', size: 20 }),
              new TextRun({ text: '(Ký, đóng dấu điện tử)', italics: true, font: 'Arial', size: 16 }),
            ]
          })
        ]
      }]
    });

    const blob = await Packer.toBlob(doc);
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName.endsWith('.docx') ? fileName : `${fileName}.docx`;
    link.click();
  } catch (err) {
    console.error('Lỗi xuất Word:', err);
    alert('Không thể kết xuất hợp đồng hoặc báo cáo ra dạng Word. Vui lòng kiểm tra lại cấu trúc.');
  }
}

/**
 * EXPORT TO POWERPOINT (.pptx)
 * Generates interactive slide decks summarizing staff, company statistics or departments structure.
 */
export async function exportToPPTX(
  presentationTitle: string,
  slides: { title: string; subtitle?: string; content: string[] }[],
  fileName: string
) {
  try {
    const pres = new pptxgen();
    pres.layout = 'LAYOUT_16x9';

    // Slide 1: Welcome Intro Slide
    const introSlide = pres.addSlide();
    
    // Add nice background accent block
    introSlide.addShape(pres.ShapeType.rect, {
      x: 0, y: 0, w: '100%', h: '100%',
      fill: { color: '2D3A2D' } // Charcoal green branding color
    });

    introSlide.addText(presentationTitle, {
      x: 0.5, y: 2.0, w: 9, h: 1.5,
      fontSize: 32, bold: true, color: 'C5A059',
      align: pres.AlignH.left,
      valign: pres.AlignV.middle,
      fontFace: 'Arial'
    });

    introSlide.addText('HỆ THỐNG QUẢN TRỊ NHÂN SỰ TOÀN DIỆN - HRM CNPM\nXuất dữ liệu tự động từ bảng trực tuyến.', {
      x: 0.5, y: 3.5, w: 9, h: 1.2,
      fontSize: 14, color: 'FFFFFF',
      italic: true,
      fontFace: 'Arial'
    });

    // Additional database slides
    slides.forEach(slideData => {
      const slide = pres.addSlide();

      // Top Header bar
      slide.addShape(pres.ShapeType.rect, {
        x: 0, y: 0, w: '100%', h: 1.0,
        fill: { color: 'F4F6F4' }
      });
      slide.addShape(pres.ShapeType.rect, {
        x: 0, y: 1.0, w: '100%', h: 0.03,
        fill: { color: 'C5A059' }
      });

      slide.addText(slideData.title.toUpperCase(), {
        x: 0.5, y: 0.2, w: 9, h: 0.6,
        fontSize: 20, bold: true, color: '2D3A2D',
        fontFace: 'Arial'
      });

      if (slideData.subtitle) {
        slide.addText(slideData.subtitle, {
          x: 0.5, y: 1.2, w: 9, h: 0.4,
          fontSize: 12, italic: true, color: '666666',
          fontFace: 'Arial'
        });
      }

      // Add Content box as structured bullet points
      const bulletPoints = slideData.content.map(bullet => ({
        text: bullet,
        options: { bullet: true, fontSize: 13, face: 'Arial', color: '333333', lineSpacing: 24 }
      }));

      slide.addText(bulletPoints, {
        x: 0.8, y: 1.8, w: 8.4, h: 4.8,
        valign: pres.AlignV.top
      });
    });

    // Write file
    pres.writeFile({ fileName: fileName.endsWith('.pptx') ? fileName : `${fileName}.pptx` });
  } catch (err) {
    console.error('Lỗi xuất PowerPoint:', err);
    alert('Không thể tạo file báo cáo thuyệt minh PowerPoint. Lỗi thư viện.');
  }
}

/**
 * CLIENT PRINT DIALOG HANDLER
 * Seamlessly opens printable window rendering beautiful printable containers.
 * Bypasses popup block restrictions by using an invisible temporary iframe.
 */
export function triggerPrintSelection(elementId: string, documentTitle: string) {
  const printElement = document.getElementById(elementId);
  if (!printElement) {
    alert('Đối tượng cần in không tồn tại trên giao diện hiển thị!');
    return;
  }

  // Create an iframe to print
  let iframe = document.getElementById('print-iframe') as HTMLIFrameElement;
  if (!iframe) {
    iframe = document.createElement('iframe');
    iframe.id = 'print-iframe';
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
        <title>${documentTitle}</title>
        <style>
          @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');
          body { 
            font-family: 'Inter', "Times New Roman", sans-serif; 
            padding: 30px; 
            color: #111;
            line-height: 1.5;
            background-color: #fff;
          }
          .header-tab {
            border-bottom: 2px solid #2d3a2d;
            padding-bottom: 12px;
            margin-bottom: 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
          }
          .title {
            font-size: 20px;
            font-weight: 800;
            color: #2d3a2d;
            text-transform: uppercase;
          }
          .meta-info {
            font-size: 11px;
            color: #555;
            text-align: right;
          }
          table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11px;
            margin-top: 15px;
          }
          th {
            background-color: #f3f4f3;
            color: #2d3a2d;
            font-weight: bold;
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
          }
          td {
            border: 1px solid #ddd;
            padding: 8px;
          }
          tr:nth-child(even) {
            background-color: #fcfcfc;
          }
          .badge {
            display: inline-block;
            padding: 2px 6px;
            font-size: 10px;
            font-weight: bold;
            border-radius: 99px;
            background-color: #e5e7eb;
            color: #374151;
            border: 1px solid #d1d5db;
          }
          @media print {
            body { padding: 10px; }
            button { display: none; }
          }
        </style>
      </head>
      <body>
        <div class="header-tab">
          <div>
            <div class="title">${documentTitle}</div>
            <div style="font-size: 11px; margin-top: 4px; color: #666;">Hồ Sơ Điện Tử HRM WPF CNPM Sư Đoàn Bộ Chỉ Huy</div>
          </div>
          <div class="meta-info">
            <strong>Ngày lập:</strong> ${new Date().toLocaleDateString('vi-VN')}<br/>
            <strong>Người trích xuất:</strong> Hệ thống tự động
          </div>
        </div>
        <div>
          ${printElement.innerHTML}
        </div>
        <div style="margin-top: 40px; display: flex; justify-content: space-between; font-size: 11px;">
          <div>
            <em>* Báo cáo định hạn, kiểm tra thông tin kỹ lưỡng trước khi lưu trữ.</em>
          </div>
          <div style="text-align: center; width: 220px;">
            <strong>ỦY BIÊN PHÊ DUYỆT</strong><br/>
            <span style="font-size: 10px; color:#555;">(Ký, đóng dấu số điện tử)</span>
            <div style="height: 50px;"></div>
            <strong>Trần Minh Giám Đốc</strong>
          </div>
        </div>
      </body>
    </html>
  `);
  doc.close();

  // Wait for loading to trigger print
  setTimeout(() => {
    iframe.contentWindow?.focus();
    iframe.contentWindow?.print();
  }, 500);
}
