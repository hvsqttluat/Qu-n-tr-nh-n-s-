import { Request, Response } from 'express';
import db from '../database/db.js';

export const CommandController = {
  execute: (req: Request, res: Response) => {
    const { command, parameters } = req.body;
    
    const executionId = `OP-${Math.random().toString(36).substring(7).toUpperCase()}`;
    
    // Lưu vào SQLite
    try {
      const stmt = db.prepare('INSERT INTO command_logs (execution_id, command, parameters, status) VALUES (?, ?, ?, ?)');
      stmt.run(executionId, command, JSON.stringify(parameters), 'Acknowledged');
    } catch (error) {
      console.error('[DB ERROR]: Failed to log command', error);
    }
    
    res.json({
      status: "Success",
      executionId,
      data: {
        command,
        parameters,
        timestamp: new Date().toISOString(),
        node: "Nexus-Alpha-1"
      },
      message: `Hệ thống đã phê duyệt lệnh ${command}. Đang triển khai...`
    });
  },

  getStatus: (req: Request, res: Response) => {
    // Lấy số lượng lệnh đã thực hiện từ DB
    const row = db.prepare('SELECT COUNT(*) as count FROM command_logs').get() as { count: number };
    
    res.json({
      cpu: "12%",
      memory: "2.4GB / 8GB",
      uptime: process.uptime(),
      activeUsers: 1,
      securityLevel: "DEFCON 4",
      totalCommandsProcessed: row.count
    });
  }
};
