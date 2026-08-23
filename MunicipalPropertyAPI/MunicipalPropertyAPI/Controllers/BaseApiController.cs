using Microsoft.AspNetCore.Mvc;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Models;
using System.Linq;

namespace MunicipalPropertyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly AppDbContext _context;

        protected BaseApiController(AppDbContext context)
        {
            _context = context;
        }

        protected User? AuthorizeAndGetUser(string[] allowedRoles)
        {
            var login = Request.Headers["X-Login"].FirstOrDefault();
            var password = Request.Headers["X-Password"].FirstOrDefault();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                HttpContext.Response.StatusCode = 401;
                return null;
            }

            // Ищем пользователя по логину и паролю (без хеширования)
            var user = _context.Users.FirstOrDefault(u =>
                u.Login == login &&
                u.PasswordHash == password &&  // ← сравниваем как есть
                u.IsActive == false);  // false = не заблокирован

            if (user == null || !allowedRoles.Contains(user.Role))
            {
                HttpContext.Response.StatusCode = 401;
                return null;
            }
            return user;
        }
    }
}