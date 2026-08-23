using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MunicipalPropertyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : BaseApiController
    {
        public ReportsController(AppDbContext context) : base(context)
        {
            // ===== ВАЖНО! Устанавливаем лицензию EPPlus =====
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: api/reports/contracts
        [HttpGet("contracts")]
        public async Task<IActionResult> ExportContracts([FromQuery] string? search = null)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var query = _context.Contracts
                .Include(c => c.PropertyObject)
                .Include(c => c.Tenant)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.ContractNumber.Contains(search) ||
                                         (c.PropertyObject != null && c.PropertyObject.Address.Contains(search)) ||
                                         (c.Tenant != null && c.Tenant.ShortName.Contains(search)));
            }

            var contracts = await query
                .OrderByDescending(c => c.StartDate)
                .Select(c => new
                {
                    c.Id,
                    c.ContractNumber,
                    ObjectAddress = c.PropertyObject != null ? c.PropertyObject.Address : "Не указан",
                    TenantName = c.Tenant != null ? c.Tenant.ShortName : "Не указан",
                    c.StartDate,
                    c.EndDate,
                    c.MonthlyRate,
                    c.PaymentDay,
                    c.ContractStatus
                })
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Договоры");

                // Заголовки
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Номер договора";
                worksheet.Cells[1, 3].Value = "Объект";
                worksheet.Cells[1, 4].Value = "Арендатор";
                worksheet.Cells[1, 5].Value = "Дата начала";
                worksheet.Cells[1, 6].Value = "Дата окончания";
                worksheet.Cells[1, 7].Value = "Ставка";
                worksheet.Cells[1, 8].Value = "День оплаты";
                worksheet.Cells[1, 9].Value = "Статус";

                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                for (int i = 0; i < contracts.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = contracts[i].Id;
                    worksheet.Cells[row, 2].Value = contracts[i].ContractNumber;
                    worksheet.Cells[row, 3].Value = contracts[i].ObjectAddress;
                    worksheet.Cells[row, 4].Value = contracts[i].TenantName;
                    worksheet.Cells[row, 5].Value = contracts[i].StartDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 6].Value = contracts[i].EndDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 7].Value = contracts[i].MonthlyRate;
                    worksheet.Cells[row, 8].Value = contracts[i].PaymentDay;
                    worksheet.Cells[row, 9].Value = contracts[i].ContractStatus;
                }

                worksheet.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Договоры_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
        }

        // GET: api/reports/objects
        [HttpGet("objects")]
        public async Task<IActionResult> ExportObjects()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
            if (user == null) return Unauthorized();

            var objects = await _context.PropertyObjects
                .OrderBy(o => o.Address)
                .Select(o => new
                {
                    o.Id,
                    o.Address,
                    o.CadastralNumber,
                    o.Type,
                    o.TotalArea,
                    o.Purpose,
                    o.Condition,
                    o.IsRentedNow
                })
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Объекты");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Адрес";
                worksheet.Cells[1, 3].Value = "Кадастровый номер";
                worksheet.Cells[1, 4].Value = "Тип";
                worksheet.Cells[1, 5].Value = "Площадь";
                worksheet.Cells[1, 6].Value = "Назначение";
                worksheet.Cells[1, 7].Value = "Состояние";
                worksheet.Cells[1, 8].Value = "Арендуется";

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                for (int i = 0; i < objects.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = objects[i].Id;
                    worksheet.Cells[row, 2].Value = objects[i].Address;
                    worksheet.Cells[row, 3].Value = objects[i].CadastralNumber;
                    worksheet.Cells[row, 4].Value = objects[i].Type;
                    worksheet.Cells[row, 5].Value = objects[i].TotalArea;
                    worksheet.Cells[row, 6].Value = objects[i].Purpose;
                    worksheet.Cells[row, 7].Value = objects[i].Condition;
                    worksheet.Cells[row, 8].Value = objects[i].IsRentedNow ? "Да" : "Нет";
                }

                worksheet.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Объекты_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
        }

        // GET: api/reports/tenants
        [HttpGet("tenants")]
        public async Task<IActionResult> ExportTenants()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
            if (user == null) return Unauthorized();

            var tenants = await _context.Tenants
                .OrderBy(t => t.ShortName)
                .Select(t => new
                {
                    t.Id,
                    t.Type,
                    t.Inn,
                    t.Ogrn,
                    t.ShortName,
                    t.FullName,
                    t.Phone,
                    t.Email,
                    t.LegalAddress,
                    t.RegistrationDate
                })
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Арендаторы");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Тип";
                worksheet.Cells[1, 3].Value = "ИНН";
                worksheet.Cells[1, 4].Value = "ОГРН";
                worksheet.Cells[1, 5].Value = "Краткое название";
                worksheet.Cells[1, 6].Value = "Полное название";
                worksheet.Cells[1, 7].Value = "Телефон";
                worksheet.Cells[1, 8].Value = "Email";
                worksheet.Cells[1, 9].Value = "Юридический адрес";
                worksheet.Cells[1, 10].Value = "Дата регистрации";

                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                for (int i = 0; i < tenants.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = tenants[i].Id;
                    worksheet.Cells[row, 2].Value = tenants[i].Type;
                    worksheet.Cells[row, 3].Value = tenants[i].Inn;
                    worksheet.Cells[row, 4].Value = tenants[i].Ogrn;
                    worksheet.Cells[row, 5].Value = tenants[i].ShortName;
                    worksheet.Cells[row, 6].Value = tenants[i].FullName;
                    worksheet.Cells[row, 7].Value = tenants[i].Phone;
                    worksheet.Cells[row, 8].Value = tenants[i].Email;
                    worksheet.Cells[row, 9].Value = tenants[i].LegalAddress;
                    worksheet.Cells[row, 10].Value = tenants[i].RegistrationDate?.ToString("dd.MM.yyyy");
                }

                worksheet.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Арендаторы_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
        }

        // GET: api/reports/debtors
        [HttpGet("debtors")]
        public async Task<IActionResult> ExportDebtors()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant" });
            if (user == null) return Unauthorized();

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var debtors = await _context.Tenants
                .Where(t => _context.Contracts.Any(c => c.TenantId == t.Id && c.ContractStatus == "действует"))
                .Select(t => new
                {
                    t.Id,
                    t.ShortName,
                    t.Inn,
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
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Должники");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Арендатор";
                worksheet.Cells[1, 3].Value = "ИНН";
                worksheet.Cells[1, 4].Value = "Задолженность";

                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                for (int i = 0; i < debtors.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = debtors[i].Id;
                    worksheet.Cells[row, 2].Value = debtors[i].ShortName;
                    worksheet.Cells[row, 3].Value = debtors[i].Inn;
                    worksheet.Cells[row, 4].Value = debtors[i].TotalDebt;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                }

                worksheet.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Должники_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
        }
    }
}