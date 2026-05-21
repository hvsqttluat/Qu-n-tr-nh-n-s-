using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusHQ.Api.Models;

namespace NexusHQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu phải có Token mới được truy cập
    public class ProceduresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProceduresController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var procedures = _context.Procedures.OrderByDescending(p => p.CreatedAt).ToList();
            return Ok(procedures);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Commander")] // Chỉ Admin hoặc Chỉ huy mới được thêm
        public IActionResult Create([FromBody] Procedure procedure)
        {
            procedure.CreatedAt = DateTime.Now;
            procedure.Author = User.Identity?.Name ?? "Hệ thống";
            _context.Procedures.Add(procedure);
            _context.SaveChanges();
            return Ok(procedure);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Procedure updatedProcedure)
        {
            var procedure = _context.Procedures.Find(id);
            if (procedure == null) return NotFound();

            procedure.Title = updatedProcedure.Title;
            procedure.Description = updatedProcedure.Description;
            procedure.Category = updatedProcedure.Category;

            _context.SaveChanges();
            return Ok(procedure);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa
        public IActionResult Delete(int id)
        {
            var procedure = _context.Procedures.Find(id);
            if (procedure == null) return NotFound();

            _context.Procedures.Remove(procedure);
            _context.SaveChanges();
            return Ok(new { message = "Đã xóa thủ tục thành công." });
        }
    }
}
