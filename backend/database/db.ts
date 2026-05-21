import Database from 'better-sqlite3';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Đường dẫn file database
const dbPath = path.resolve(__dirname, '../../nexus_hq.db');

const db = new Database(dbPath, { verbose: console.log });

// Khởi tạo các bảng cần thiết
export function initDatabase() {
  // Bảng lưu trữ lịch sử lệnh
  db.exec(`
    CREATE TABLE IF NOT EXISTS command_logs (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      execution_id TEXT NOT NULL,
      command TEXT NOT NULL,
      parameters TEXT,
      status TEXT,
      timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
    )
  `);

  // Bảng lưu trữ thông tin quân số/nhân sự
  db.exec(`
    CREATE TABLE IF NOT EXISTS personnel (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT NOT NULL,
      rank TEXT NOT NULL,
      unit TEXT,
      status TEXT DEFAULT 'Active'
    )
  `);

  // Bảng người dùng và phân quyền
  db.exec(`
    CREATE TABLE IF NOT EXISTS users (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      username TEXT UNIQUE NOT NULL,
      password TEXT NOT NULL,
      role TEXT DEFAULT 'User', -- Admin, Commander, User
      full_name TEXT
    )
  `);

  // Bảng quản lý thủ tục/quy trình
  db.exec(`
    CREATE TABLE IF NOT EXISTS procedures (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      title TEXT NOT NULL,
      description TEXT,
      category TEXT,
      created_by INTEGER,
      created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY(created_by) REFERENCES users(id)
    )
  `);

  console.log('[DATABASE]: SQLite initialized at', dbPath);
}

export default db;
