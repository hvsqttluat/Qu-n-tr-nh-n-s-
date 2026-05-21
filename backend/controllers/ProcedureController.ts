import { Response } from 'express';
import { AuthRequest } from '../middleware/AuthMiddleware.js';
import db from '../database/db.js';

export const ProcedureController = {
  getAll: (req: AuthRequest, res: Response) => {
    const procedures = db.prepare('SELECT p.*, u.full_name as author FROM procedures p LEFT JOIN users u ON p.created_by = u.id').all();
    res.json(procedures);
  },

  create: (req: AuthRequest, res: Response) => {
    const { title, description, category } = req.body;
    const userId = req.user?.id;
    
    const stmt = db.prepare('INSERT INTO procedures (title, description, category, created_by) VALUES (?, ?, ?, ?)');
    const result = stmt.run(title, description, category, userId);
    
    res.status(201).json({ message: 'Tạo thủ tục thành công', id: result.lastInsertRowid });
  },

  update: (req: AuthRequest, res: Response) => {
    const { id } = req.params;
    const { title, description, category } = req.body;
    
    const stmt = db.prepare('UPDATE procedures SET title = ?, description = ?, category = ? WHERE id = ?');
    const result = stmt.run(title, description, category, id);
    
    if (result.changes === 0) return res.status(404).json({ message: 'Không tìm thấy thủ tục' });
    res.json({ message: 'Cập nhật thành công' });
  },

  delete: (req: AuthRequest, res: Response) => {
    const { id } = req.params;
    const stmt = db.prepare('DELETE FROM procedures WHERE id = ?');
    const result = stmt.run(id);
    
    if (result.changes === 0) return res.status(404).json({ message: 'Không tìm thấy thủ tục' });
    res.json({ message: 'Xóa thành công' });
  }
};
