using JobApplicationTracker.Api.Data;
using JobApplicationTracker.Api.DTOs;
using JobApplicationTracker.Api.Models;
using JobApplicationTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobApplicationTracker.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User already exists");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = PasswordService.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        //Test CODE
/*
        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            var password = "Password123";
            var hash = PasswordService.HashPassword(password);

            var user = new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                PasswordHash = hash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { password, hash });
        }
*/
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required");

            var email = dto.Email.Trim();
            var password = dto.Password.Trim();

            var hash = PasswordService.HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (user.PasswordHash != hash)
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        //Test Code
        /*
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var password = "Password123";
            var hash = PasswordService.HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == "test@test.com");

            return Ok(new
            {
                InputHash = hash,
                DbHash = user!.PasswordHash,
                Match = hash == user.PasswordHash
            });
        }*/

        [Authorize]
        [HttpGet("secure-test")]
        public IActionResult SecureTest()
        {
            return Ok("JWT authentication works!");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
