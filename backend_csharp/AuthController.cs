using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NexusHQ.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusHQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtSecret = "quan_doi_nhan_dan_viet_nam_anh_hung_123456";

        public AuthortController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("sync")]
        public IActionResult SyncFirebaseUser([FromBody] FirebaseUserRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Email);

            if (user == null)
            {
                user = new User
                {
                    Username = request.Email,
                    FullName = request.DisplayName,
                    PasswordHash = "FIREBASE_AUTH", // Không cần pass vì login qua Google
                    Role = "User"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.Username, user.Role, user.FullName } });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class FirebaseUserRequest
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
