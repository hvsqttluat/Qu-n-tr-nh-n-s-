import express from "express";
import { createServer as createViteServer } from "vite";
import path from "path";
import { fileURLToPath } from "url";
import { CommandController } from "./backend/controllers/CommandController.js";
import { AuthController } from "./backend/controllers/AuthController.js";
import { ProcedureController } from "./backend/controllers/ProcedureController.js";
import { ReportController } from "./backend/controllers/ReportController.js";
import { initDatabase } from "./backend/database/db.js";
import { authMiddleware, authorize } from "./backend/middleware/AuthMiddleware.js";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function startServer() {
  initDatabase();

  const app = express();
  const PORT = 3000;

  app.use(express.json());

  // --- PUBLIC ROUTES ---
  app.post("/api/auth/register", AuthController.register);
  app.post("/api/auth/login", AuthController.login);
  app.post("/api/auth/sync", AuthController.syncFirebaseUser);
  app.get("/api/health", CommandController.getStatus);

  // --- PROTECTED ROUTES (Auth Required) ---
  app.use("/api/protected", authMiddleware);
  
  // Personnel & Commands
  app.post("/api/command", authMiddleware, CommandController.execute);

  // Procedures CRUD
  app.get("/api/procedures", authMiddleware, ProcedureController.getAll);
  app.post("/api/procedures", authMiddleware, authorize(['Admin', 'Commander']), ProcedureController.create);
  app.put("/api/procedures/:id", authMiddleware, authorize(['Admin', 'Commander']), ProcedureController.update);
  app.delete("/api/procedures/:id", authMiddleware, authorize(['Admin']), ProcedureController.delete);

  // Reports Export
  app.get("/api/reports/excel", authMiddleware, ReportController.exportExcel);
  app.get("/api/reports/word", authMiddleware, ReportController.exportWord);
  app.get("/api/reports/ppt", authMiddleware, ReportController.exportPPT);
  app.get("/api/reports/pdf", authMiddleware, ReportController.exportPDF);

  // --- VITE MIDDLEWARE SETUP ---
  
  if (process.env.NODE_ENV !== "production") {
    const vite = await createViteServer({
      root: path.resolve(__dirname, '.'),
      server: { 
        middlewareMode: true,
      },
      appType: "spa",
    });
    app.use(vite.middlewares);
  } else {
    // In production, serve the built files from the root dist folder
    const distPath = path.join(process.cwd(), 'dist');
    app.use(express.static(distPath));
    app.get('*', (req, res) => {
      res.sendFile(path.join(distPath, 'index.html'));
    });
  }

  app.listen(PORT, "0.0.0.0", () => {
    console.log(`[SYSTEM]: Nexus AI Server running on http://localhost:${PORT}`);
  });
}

startServer().catch((err) => {
  console.error("[CRITICAL ERROR]: Failed to start server", err);
  process.exit(1);
});
