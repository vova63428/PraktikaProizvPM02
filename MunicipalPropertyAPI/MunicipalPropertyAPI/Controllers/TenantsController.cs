using Microsoft.AspNetCore.Mvc;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;

namespace MunicipalPropertyAPI.Controllers
{
    public class TenantsController : BaseApiController
    {
        public TenantsController(AppDbContext context) : base(context)
        {
        }

        // GET: api/tenants
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var query = _context.Tenants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.ShortName.Contains(search) ||
                                         t.Inn.Contains(search) ||
                                         (t.FullName != null && t.FullName.Contains(search)));
            }

            var tenants = query.OrderBy(t => t.ShortName)
                .Select(t => new TenantDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Inn = t.Inn,
                    Ogrn = t.Ogrn,
                    ShortName = t.ShortName,
                    FullName = t.FullName,
                    Phone = t.Phone,
                    Email = t.Email,
                    LegalAddress = t.LegalAddress,
                    RegistrationDate = t.RegistrationDate
                })
                .ToList();

            return Ok(tenants);
        }

        // GET: api/tenants/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var tenant = _context.Tenants.Find(id);
            if (tenant == null) return NotFound();

            return Ok(new TenantDto
            {
                Id = tenant.Id,
                Type = tenant.Type,
                Inn = tenant.Inn,
                Ogrn = tenant.Ogrn,
                ShortName = tenant.ShortName,
                FullName = tenant.FullName,
                Phone = tenant.Phone,
                Email = tenant.Email,
                LegalAddress = tenant.LegalAddress,
                RegistrationDate = tenant.RegistrationDate
            });
        }

        // GET: api/tenants/byinn/{inn}
        [HttpGet("byinn/{inn}")]
        public IActionResult GetByInn(string inn)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var tenant = _context.Tenants.FirstOrDefault(t => t.Inn == inn);
            if (tenant == null) return NotFound();

            return Ok(new TenantDto
            {
                Id = tenant.Id,
                Type = tenant.Type,
                Inn = tenant.Inn,
                Ogrn = tenant.Ogrn,
                ShortName = tenant.ShortName,
                FullName = tenant.FullName,
                Phone = tenant.Phone,
                Email = tenant.Email,
                LegalAddress = tenant.LegalAddress,
                RegistrationDate = tenant.RegistrationDate
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateTenantRequest request)
        {
            try
            {
                var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
                if (user == null) return Unauthorized();

                if (_context.Tenants.Any(t => t.Inn == request.Inn))
                    return BadRequest("Арендатор с таким ИНН уже существует.");

                // ===== РУЧНОЙ ID =====
                int newId = 1;
                if (_context.Tenants.Any())
                {
                    newId = _context.Tenants.Max(t => t.Id) + 1;
                }

                var tenant = new Tenant
                {
                    Id = newId,
                    Type = request.Type ?? "Юридическое лицо",
                    Inn = request.Inn,
                    Ogrn = request.Ogrn ?? "",
                    ShortName = request.ShortName,
                    FullName = string.IsNullOrEmpty(request.FullName) ? request.ShortName : request.FullName,
                    Phone = request.Phone ?? "",
                    Email = request.Email ?? "",
                    LegalAddress = request.LegalAddress ?? "",
                    RegistrationDate = request.RegistrationDate ?? DateTime.Now
                };

                _context.Tenants.Add(tenant);
                _context.SaveChanges();

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ОШИБКА ===");
                Console.WriteLine($"Сообщение: {ex.Message}");
                Console.WriteLine($"Внутренняя: {ex.InnerException?.Message}");
                Console.WriteLine($"Стек: {ex.StackTrace}");

                return BadRequest($"Ошибка: {ex.Message}. Внутренняя: {ex.InnerException?.Message}");
            }
        }
    }
}