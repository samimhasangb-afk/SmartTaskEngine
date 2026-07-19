using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Auth;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(ApplicationDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
            if (exists) return null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = UserRole.Member
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) return null;

            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(User user)
        {
            var (token, expiresAt) = _jwtService.GenerateToken(user);
            return new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}
