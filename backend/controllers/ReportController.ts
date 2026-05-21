import { Request, Response } from 'express';
import db from '../database/db.js';
import ExcelJS from 'exceljs';
import { Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell } from 'docx';
import pptxgen from 'pptxgenjs';
import { jsPDF } from 'jspdf';
import 'jspdf-autotable';

export const ReportController = {
  exportExcel: async (req: Request, res: Response) => {
    const procedures = db.prepare('SELECT * FROM procedures').all() as any[];
    
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Procedures');
    
    worksheet.columns = [
      { header: 'ID', key: 'id', width: 10 },
      { header: 'Tiêu đề', key: 'title', width: 30 },
      { header: 'Mô tả', key: 'description', width: 50 },
      { header: 'Danh mục', key: 'category', width: 20 },
      { header: 'Ngày tạo', key: 'created_at', width: 25 },
    ];
    
    procedures.forEach(p => worksheet.addRow(p));
    
    res.setHeader('Content-Type', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
    res.setHeader('Content-Disposition', 'attachment; filename=bao_cao_thu_tuc.xlsx');
    
    await workbook.xlsx.write(res);
    res.end();
  },

  exportWord: async (req: Request, res: Response) => {
    const procedures = db.prepare('SELECT * FROM procedures').all() as any[];
    
    const doc = new Document({
      sections: [{
        properties: {},
        children: [
          new Paragraph({
            children: [new TextRun({ text: "BÁO CÁO DANH SÁCH THỦ TỤC", bold: true, size: 32 })],
          }),
          new Table({
            rows: [
              new TableRow({
                children: [
                  new TableCell({ children: [new Paragraph("ID")] }),
                  new TableCell({ children: [new Paragraph("Tiêu đề")] }),
                  new TableCell({ children: [new Paragraph("Danh mục")] }),
                ],
              }),
              ...procedures.map(p => new TableRow({
                children: [
                  new TableCell({ children: [new Paragraph(p.id.toString())] }),
                  new TableCell({ children: [new Paragraph(p.title)] }),
                  new TableCell({ children: [new Paragraph(p.category || "")] }),
                ],
              })),
            ],
          }),
        ],
      }],
    });

    const buffer = await Packer.toBuffer(doc);
    res.setHeader('Content-Type', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document');
    res.setHeader('Content-Disposition', 'attachment; filename=bao_cao.docx');
    res.send(buffer);
  },

  exportPPT: async (req: Request, res: Response) => {
    const procedures = db.prepare('SELECT * FROM procedures').all() as any[];
    const pres = new pptxgen();
    
    let slide = pres.addSlide();
    slide.addText("BÁO CÁO THỦ TỤC HỆ THỐNG", { x: 1, y: 1, fontSize: 24, color: "363636" });
    
    procedures.forEach((p, index) => {
      if (index > 0 && index % 5 === 0) slide = pres.addSlide();
      slide.addText(`${p.id}. ${p.title} (${p.category})`, { x: 1, y: 2 + (index % 5) * 0.8, fontSize: 14 });
    });

    const buffer = await pres.write({ outputType: "nodebuffer" });
    res.setHeader('Content-Type', 'application/vnd.openxmlformats-officedocument.presentationml.presentation');
    res.setHeader('Content-Disposition', 'attachment; filename=thuyet_minh.pptx');
    res.send(buffer);
  },

  exportPDF: async (req: Request, res: Response) => {
    const procedures = db.prepare('SELECT * FROM procedures').all() as any[];
    const doc = new jsPDF();
    
    doc.text("DANH SACH THU TUC", 10, 10);
    
    (doc as any).autoTable({
      head: [['ID', 'Tieu de', 'Danh muc']],
      body: procedures.map(p => [p.id, p.title, p.category]),
    });

    const buffer = doc.output('arraybuffer');
    res.setHeader('Content-Type', 'application/pdf');
    res.setHeader('Content-Disposition', 'attachment; filename=bao_cao.pdf');
    res.send(Buffer.from(buffer));
  }
};
