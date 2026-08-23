using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;

namespace MunicipalPropertyAPI.Controllers
{
    public class PaymentsController : BaseApiController
    {
        public PaymentsController(AppDbContext context) : base(context)
        {
        }

        // GET: api/payments/contract/{contractId}
        [HttpGet("contract/{contractId}")]
        public IActionResult GetByContract(int contractId)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker", "tenant" });
            if (user == null) return Unauthorized();

            // ===== ИСПРАВЛЕНО: фильтр по UserId =====
            if (user.Role == "tenant")
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == user.Id);
                if (tenant == null) return Unauthorized("Доступ запрещён.");

                var contract = _context.Contracts.FirstOrDefault(c => c.Id == contractId);
                if (contract == null || contract.TenantId != tenant.Id)
                {
                    return Unauthorized("Доступ запрещён.");
                }
            }

            var payments = _context.Payments
                .Include(p => p.EmployeeWhoAccepted)
                .Where(p => p.ContractId == contractId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    ContractId = p.ContractId,
                    PaymentDate = p.PaymentDate,
                    PeriodMonth = p.PeriodMonth,
                    Amount = p.Amount ?? 0,
                    PaymentType = p.PaymentType ?? "безналичный",
                    IsPenalty = p.IsPenalty,
                    ReceiptNumber = p.ReceiptNumber,
                    EmployeeName = p.EmployeeWhoAccepted != null
                        ? $"{p.EmployeeWhoAccepted.LastName} {p.EmployeeWhoAccepted.FirstName}"
                        : "Не указан"
                })
                .ToList();

            return Ok(payments);
        }

        // GET: api/payments/debtors
        [HttpGet("debtors")]
        public IActionResult GetDebtors()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant" });
            if (user == null) return Unauthorized();

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var debtors = _context.Tenants
                .Where(t => _context.Contracts.Any(c => c.TenantId == t.Id && c.ContractStatus == "действует"))
                .Select(t => new DebtorDto
                {
                    TenantId = t.Id,
                    TenantName = t.ShortName ?? "Не указано",
                    Inn = t.Inn ?? "Не указан",
                    TotalDebt = _context.Contracts
                        .Where(c => c.TenantId == t.Id && c.ContractStatus == "действует")
                        .Sum(c => c.MonthlyRate ?? 0) -
                        _context.Payments
                        .Where(p => p.Contract.TenantId == t.Id
                                    && p.PeriodMonth >= threeMonthsAgo)
                        .Sum(p => p.Amount ?? 0)
                })
                .Where(d => d.TotalDebt > 0)
                .OrderByDescending(d => d.TotalDebt)
                .ToList();

            return Ok(debtors);
        }

        // GET: api/payments/summary
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = currentMonth.AddMonths(1);

            var summary = new PaymentSummaryDto
            {
                TotalPaymentsCurrentMonth = _context.Payments
                    .Where(p => p.PaymentDate >= currentMonth && p.PaymentDate < nextMonth)
                    .Sum(p => p.Amount ?? 0),
                TotalPaymentsLastMonth = _context.Payments
                    .Where(p => p.PaymentDate >= currentMonth.AddMonths(-1) && p.PaymentDate < currentMonth)
                    .Sum(p => p.Amount ?? 0),
                TotalPenalties = _context.Payments
                    .Where(p => p.IsPenalty == true)
                    .Sum(p => p.Amount ?? 0),
                ActiveContractsCount = _context.Contracts
                    .Count(c => c.ContractStatus == "действует")
            };

            return Ok(summary);
        }

        // POST: api/payments
        [HttpPost]
        public IActionResult Create([FromBody] CreatePaymentRequest request)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var contract = _context.Contracts.Find(request.ContractId);
            if (contract == null)
                return BadRequest("Договор не найден.");

            if (contract.ContractStatus != "действует")
                return BadRequest("Договор не активен.");

            if (!_context.Employees.Any(e => e.Id == request.EmployeeWhoAcceptedId))
                return BadRequest("Сотрудник не найден.");

            var payment = new Payment
            {
                ContractId = request.ContractId,
                PaymentDate = request.PaymentDate,
                PeriodMonth = request.PeriodMonth,
                Amount = request.Amount,
                PaymentType = request.PaymentType ?? "безналичный",
                IsPenalty = request.IsPenalty ?? false,
                EmployeeWhoAcceptedId = request.EmployeeWhoAcceptedId,
                ReceiptNumber = request.ReceiptNumber,
                UserWhoAcceptedId = request.UserWhoAcceptedId
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetByContract), new { contractId = payment.ContractId }, new { payment.Id });
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant" });
            if (user == null) return Unauthorized();

            var payment = _context.Payments.Find(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            _context.SaveChanges();

            return NoContent();
        }
    }
}