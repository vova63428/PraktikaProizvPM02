using Microsoft.AspNetCore.Mvc;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;
using System;
using System.Linq;

namespace MunicipalPropertyAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/auth/login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Логин и пароль обязательны.");

            // Ищем пользователя по логину и паролю (без хеширования)
            var user = _context.Users.FirstOrDefault(u =>
                u.Login == request.Login &&
                u.PasswordHash == request.Password &&  // ← сравниваем как есть
                u.IsActive == false);  // false = не заблокирован

            if (user == null)
                return Unauthorized("Неверный логин или пароль.");

            user.LastLogin = DateTime.Now;
            _context.SaveChanges();

            return Ok(new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Role = user.Role
            });
        }

        [HttpPost("/api/auth/register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Логин, пароль и ФИО обязательны.");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("Пароли не совпадают.");

            if (_context.Users.Any(u => u.Login == request.Login))
                return BadRequest("Пользователь с таким логином уже существует.");

            var newUser = new User
            {
                Login = request.Login,
                PasswordHash = request.Password,  // ← сохраняем пароль как есть (без хеширования)
                FullName = request.FullName,
                Role = "tenant",
                Email = request.Email ?? "",
                Phone = request.Phone ?? "",
                IsActive = false, // false = не заблокирован (is_blocked = 0)
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { newUser.Id, newUser.Login, newUser.FullName, newUser.Role });
        }
    }

    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}