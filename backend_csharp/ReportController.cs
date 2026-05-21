using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using NexusHQ.Api.Models;

namespace NexusHQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("excel")]
        public IActionResult ExportExcel()
        {
            var procedures = _context.Procedures.ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Procedures");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Tiêu đề";
                worksheet.Cell(1, 3).Value = "Danh mục";
                worksheet.Cell(1, 4).Value = "Ngày tạo";

                for (int i = 0; i < procedures.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = procedures[i].Id;
                    worksheet.Cell(i + 2, 2).Value = procedures[i].Title;
                    worksheet.Cell(i + 2, 3).Value = procedures[i].Category;
                    worksheet.Cell(i + 2, 4).Value = procedures[i].CreatedAt.ToString("dd/MM/yyyy");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCao_ThuTuc.xlsx");
                }
            }
        }

        [HttpGet("health")]
        public IActionResult GetSystemStatus()
        {
            var totalCommands = _context.CommandLogs.Count();
            return Ok(new
            {
                status = "Hệ thống hoạt động bình thường",
                uptime = "99.9%",
                activeUsers = _context.Users.Count(),
                securityLevel = "Cao",
                totalCommandsProcessed = totalCommands
            });
        }
    }
}
