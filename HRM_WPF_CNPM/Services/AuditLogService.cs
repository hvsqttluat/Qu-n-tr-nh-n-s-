using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRM_WPF_CNPM.Data;
using HRM_WPF_CNPM.Models;
using HRM_WPF_CNPM.Helpers;

namespace HRM_WPF_CNPM.Services
{
    public class AuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        // Method to log system actions asynchronously safely without breaking flow
        public async Task LogAsync(string action, string tableName, int? recordId, string description)
        {
            try
            {
                var currentUser = UserSession.CurrentUser;
                int? userId = currentUser?.Id;

                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    TableName = tableName,
                    RecordId = recordId,
                    Description = description,
                    CreatedAt = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Silent catch or Console debug to ensure logging exceptions never crash the major business workflows
                Console.WriteLine($"[AuditLog Error]: {ex.Message}");
            }
        }

        // Query all logs with User included
        public async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            try
            {
                return await _context.AuditLogs
                    .Include(l => l.User)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLog Error]: {ex.Message}");
                return new List<AuditLog>();
            }
        }
    }
}
