using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;

namespace MunicipalPropertyAPI.Controllers
{
    public class ContractsController : BaseApiController
    {
        public ContractsController(AppDbContext context) : base(context)
        {
        }

        // GET: api/contracts
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker", "tenant" });
            if (user == null) return Unauthorized();

            var query = _context.Contracts
                .Include(c => c.PropertyObject)
                .Include(c => c.Tenant)
                .AsQueryable();

            if (user.Role == "tenant")
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == user.Id);
                if (tenant != null)
                {
                    query = query.Where(c => c.TenantId == tenant.Id);
                }
                else
                {
                    return Ok(new List<ContractDto>());
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.ContractNumber.Contains(search) ||
                                         (c.PropertyObject != null && c.PropertyObject.Address.Contains(search)) ||
                                         (c.Tenant != null && c.Tenant.ShortName.Contains(search)));
            }

            var contracts = query.OrderByDescending(c => c.StartDate)
                .Select(c => new ContractDto
                {
                    Id = c.Id,
                    Number = c.ContractNumber,
                    ObjectId = c.ObjectId,
                    ObjectAddress = c.PropertyObject != null ? c.PropertyObject.Address : "Не указан",
                    TenantId = c.TenantId,
                    TenantName = c.Tenant != null ? c.Tenant.ShortName : "Не указан",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MonthlyRate = c.MonthlyRate,
                    PaymentDay = c.PaymentDay,
                    Status = c.ContractStatus ?? "неизвестен",
                    Notes = c.Notes
                })
                .ToList();

            return Ok(contracts);
        }

        // GET: api/contracts/active
        [HttpGet("active")]
        public IActionResult GetActive()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker", "tenant" });
            if (user == null) return Unauthorized();

            var query = _context.Contracts
                .Include(c => c.PropertyObject)
                .Include(c => c.Tenant)
                .Where(c => c.ContractStatus == "действует" && c.EndDate >= DateTime.Now)
                .AsQueryable();

            if (user.Role == "tenant")
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == user.Id);
                if (tenant != null)
                {
                    query = query.Where(c => c.TenantId == tenant.Id);
                }
                else
                {
                    return Ok(new List<ContractDto>());
                }
            }

            var contracts = query.Select(c => new ContractDto
            {
                Id = c.Id,
                Number = c.ContractNumber,
                ObjectId = c.ObjectId,
                ObjectAddress = c.PropertyObject != null ? c.PropertyObject.Address : "Не указан",
                TenantId = c.TenantId,
                TenantName = c.Tenant != null ? c.Tenant.ShortName : "Не указан",
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRate = c.MonthlyRate,
                PaymentDay = c.PaymentDay,
                Status = c.ContractStatus,
                Notes = c.Notes
            })
            .ToList();

            return Ok(contracts);
        }

        // GET: api/contracts/expiring?days=30
        [HttpGet("expiring")]
        public IActionResult GetExpiring([FromQuery] int days = 30)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker", "tenant" });
            if (user == null) return Unauthorized();

            var expirationDate = DateTime.Now.AddDays(days);

            var query = _context.Contracts
                .Include(c => c.PropertyObject)
                .Include(c => c.Tenant)
                .Where(c => c.ContractStatus == "действует"
                            && c.EndDate <= expirationDate
                            && c.EndDate >= DateTime.Now)
                .AsQueryable();

            if (user.Role == "tenant")
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == user.Id);
                if (tenant != null)
                {
                    query = query.Where(c => c.TenantId == tenant.Id);
                }
                else
                {
                    return Ok(new List<ContractDto>());
                }
            }

            var contracts = query.Select(c => new ContractDto
            {
                Id = c.Id,
                Number = c.ContractNumber,
                ObjectId = c.ObjectId,
                ObjectAddress = c.PropertyObject != null ? c.PropertyObject.Address : "Не указан",
                TenantId = c.TenantId,
                TenantName = c.Tenant != null ? c.Tenant.ShortName : "Не указан",
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRate = c.MonthlyRate,
                PaymentDay = c.PaymentDay,
                Status = c.ContractStatus,
                Notes = c.Notes
            })
            .ToList();

            return Ok(contracts);
        }

        // GET: api/contracts/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker", "tenant" });
            if (user == null) return Unauthorized();

            var contract = _context.Contracts
                .Include(c => c.PropertyObject)
                .Include(c => c.Tenant)
                .Include(c => c.ResponsibleEmployee)
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.Id == id);

            if (contract == null) return NotFound();

            if (user.Role == "tenant")
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == user.Id);
                if (tenant == null || contract.TenantId != tenant.Id)
                {
                    return Unauthorized("Доступ запрещён.");
                }
            }

            var result = new
            {
                contract.Id,
                contract.ContractNumber,
                PropertyObject = contract.PropertyObject == null ? null : new { contract.PropertyObject.Id, contract.PropertyObject.Address, contract.PropertyObject.CadastralNumber },
                Tenant = contract.Tenant == null ? null : new { contract.Tenant.Id, contract.Tenant.ShortName, contract.Tenant.Inn },
                ResponsibleEmployee = contract.ResponsibleEmployee == null ? null : new
                {
                    contract.ResponsibleEmployee.Id,
                    FullName = $"{contract.ResponsibleEmployee.LastName} {contract.ResponsibleEmployee.FirstName}"
                },
                contract.StartDate,
                contract.EndDate,
                contract.MonthlyRate,
                contract.PaymentDay,
                contract.ContractStatus,
                contract.Notes,
                Payments = contract.Payments.Select(p => new
                {
                    p.Id,
                    p.PaymentDate,
                    p.Amount,
                    p.PaymentType,
                    p.IsPenalty
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/contracts
        [HttpPost]
        public IActionResult Create([FromBody] CreateContractRequest request)
        {
            try
            {
                var user = AuthorizeAndGetUser(new[] { "admin", "property_worker", "accountant" });
                if (user == null) return Unauthorized();

                int newId = 1;
                if (_context.Contracts.Any())
                {
                    newId = _context.Contracts.Max(c => c.Id) + 1;
                }

                var conflict = _context.Contracts
                    .Any(c => c.ObjectId == request.ObjectId
                               && c.ContractStatus == "действует"
                               && c.StartDate <= request.EndDate
                               && c.EndDate >= request.StartDate);

                if (conflict)
                    return BadRequest("На это помещение уже есть действующий договор на указанный период.");

                if (!_context.PropertyObjects.Any(o => o.Id == request.ObjectId))
                    return BadRequest("Объект не найден.");
                if (!_context.Tenants.Any(t => t.Id == request.TenantId))
                    return BadRequest("Арендатор не найден.");
                if (!_context.Employees.Any(e => e.Id == request.ResponsibleEmployeeId))
                    return BadRequest("Сотрудник не найден.");

                var contract = new Contract
                {
                    Id = newId,
                    ContractNumber = request.Number ?? $"Д-{DateTime.Now:yyyyMMdd-HHmmss}",
                    ObjectId = request.ObjectId,
                    TenantId = request.TenantId,
                    ResponsibleEmployeeId = request.ResponsibleEmployeeId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MonthlyRate = request.MonthlyRate,
                    PaymentDay = request.PaymentDay,
                    ContractStatus = "действует",
                    Notes = request.Notes
                };

                _context.Contracts.Add(contract);

                var obj = _context.PropertyObjects.Find(request.ObjectId);
                if (obj != null) obj.IsRentedNow = true;

                _context.SaveChanges();

                return CreatedAtAction(nameof(GetById), new { id = contract.Id }, new { contract.Id, contract.ContractNumber });
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

        // ===== ДОБАВЛЕНО: ОБНОВЛЕНИЕ ДОГОВОРА =====
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateContractRequest request)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker", "accountant" });
            if (user == null) return Unauthorized();

            var contract = _context.Contracts.Find(id);
            if (contract == null) return NotFound();

            if (request.MonthlyRate.HasValue)
                contract.MonthlyRate = request.MonthlyRate;

            if (request.PaymentDay.HasValue)
                contract.PaymentDay = request.PaymentDay;

            if (request.EndDate.HasValue)
                contract.EndDate = request.EndDate.Value;

            if (!string.IsNullOrEmpty(request.Notes))
                contract.Notes = request.Notes;

            _context.SaveChanges();

            return Ok(new { message = "Договор обновлён", id = contract.Id });
        }

        // PUT: api/contracts/{id}/terminate
        [HttpPut("{id}/terminate")]
        public IActionResult Terminate(int id, [FromBody] string reason)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
            if (user == null) return Unauthorized();

            var contract = _context.Contracts.Find(id);
            if (contract == null) return NotFound();

            if (contract.ContractStatus == "завершен")
                return BadRequest("Договор уже расторгнут.");

            contract.ContractStatus = "завершен";
            contract.Notes = $"{contract.Notes} Расторгнут: {reason ?? "Причина не указана"}";

            var obj = _context.PropertyObjects.Find(contract.ObjectId);
            if (obj != null) obj.IsRentedNow = false;

            _context.SaveChanges();

            return Ok(new { message = "Договор расторгнут", id = id });
        }

        // DELETE: api/contracts/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin" });
            if (user == null) return Unauthorized();

            var contract = _context.Contracts
                .Include(c => c.PropertyObject)
                .FirstOrDefault(c => c.Id == id);

            if (contract == null) return NotFound();

            if (_context.Payments.Any(p => p.ContractId == id))
                return BadRequest("Нельзя удалить договор, по которому есть платежи.");

            if (contract.PropertyObject != null)
                contract.PropertyObject.IsRentedNow = false;

            _context.Contracts.Remove(contract);
            _context.SaveChanges();

            return NoContent();
        }
    }
}