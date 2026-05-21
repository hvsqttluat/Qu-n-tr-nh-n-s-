using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusHQ.Models;
using NexusHQ.Data;

namespace NexusHQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProceduresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProceduresController(ApplicationDbContext context) { _context = context; }

        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Procedures.OrderByDescending(p => p.CreatedAt).ToList());

        [HttpPost]
        [Authorize(Roles = "Admin,Commander")]
        public IActionResult Create([FromBody] Procedure procedure)
        {
            procedure.CreatedAt = DateTime.Now;
            _context.Procedures.Add(procedure);
            _context.SaveChanges();
            return Ok(procedure);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var p = _context.Procedures.Find(id);
            if (p == null) return NotFound();
            _context.Procedures.Remove(p);
            _context.SaveChanges();
            return Ok(new { message = "Success" });
        }
    }
}
