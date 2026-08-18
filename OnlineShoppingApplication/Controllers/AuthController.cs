using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShoppingApplication.Data;
using OnlineShoppingApplication.Models;
using OnlineShoppingApplication.Services;

namespace OnlineShoppingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(
            ApplicationDbContext context,
            ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ==========================
        // Register User
        // ==========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new
                {
                    Message = "Email already exists."
                });
            }

            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email
            };

            // Hash Password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Save User
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User registered successfully."
            });
        }

        // ==========================
        // Login User
        // ==========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            // Verify Password
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            // Generate JWT Token
            var token = _tokenService.CreateToken(
                user.Id.ToString(),
                user.Name,
                user.Email);

            return Ok(new
            {
                Message = "Login successful.",
                Token = token,
                User = new
                {
                    user.Id,
                    user.Name,
                    user.Email
                }
            });
        }
    }
}