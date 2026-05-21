import { Request, Response } from 'express';
import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import db from '../database/db.js';

const JWT_SECRET = process.env.JWT_SECRET || 'nexus-super-secret-key';

export const AuthController = {
  register: async (req: Request, res: Response) => {
    const { username, password, role, fullName } = req.body;
    
    try {
      const hashedPassword = await bcrypt.hash(password, 10);
      const stmt = db.prepare('INSERT INTO users (username, password, role, full_name) VALUES (?, ?, ?, ?)');
      const result = stmt.run(username, hashedPassword, role || 'User', fullName);
      
      res.status(201).json({ message: 'Đăng ký thành công', userId: result.lastInsertRowid });
    } catch (error) {
      res.status(400).json({ message: 'Tên đăng nhập đã tồn tại hoặc dữ liệu không hợp lệ.' });
    }
  },

  login: async (req: Request, res: Response) => {
    const { username, password } = req.body;
    
    const user = db.prepare('SELECT * FROM users WHERE username = ?').get(username) as any;
    
    if (!user || !(await bcrypt.compare(password, user.password))) {
      return res.status(401).json({ message: 'Sai tên đăng nhập hoặc mật khẩu.' });
    }
    
    const token = jwt.sign(
      { id: user.id, username: user.username, role: user.role },
      JWT_SECRET,
      { expiresIn: '24h' }
    );
    
    res.json({
      token,
      user: {
        id: user.id,
        username: user.username,
        role: user.role,
        fullName: user.full_name
      }
    });
  },

  syncFirebaseUser: async (req: Request, res: Response) => {
    const { uid, email, displayName, photoURL } = req.body;
    
    try {
      // Kiểm tra xem user đã tồn tại trong SQLite chưa
      let user = db.prepare('SELECT * FROM users WHERE username = ?').get(email) as any;
      
      if (!user) {
        // Nếu chưa, tạo mới với role mặc định là User
        const stmt = db.prepare('INSERT INTO users (username, password, role, full_name) VALUES (?, ?, ?, ?)');
        // Password ngẫu nhiên vì login qua Firebase
        const randomPass = await bcrypt.hash(Math.random().toString(36), 10);
        const result = stmt.run(email, randomPass, 'User', displayName);
        user = { id: result.lastInsertRowid, username: email, role: 'User', full_name: displayName };
      }
      
      const token = jwt.sign(
        { id: user.id, username: user.username, role: user.role },
        JWT_SECRET,
        { expiresIn: '24h' }
      );
      
      res.json({
        token,
        user: {
          id: user.id,
          username: user.username,
          role: user.role,
          fullName: user.full_name
        }
      });
    } catch (error) {
      console.error('[AUTH SYNC ERROR]:', error);
      res.status(500).json({ message: 'Lỗi đồng bộ tài khoản.' });
    }
  }
};
