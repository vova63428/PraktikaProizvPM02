using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;

namespace MunicipalPropertyAPI.Controllers
{
    public class PropertyObjectsController : BaseApiController
    {
        public PropertyObjectsController(AppDbContext context) : base(context)
        {
        }

        // GET: api/propertyobjects
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            // Арендатор не видит объекты
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var query = _context.PropertyObjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.Address.Contains(search) ||
                                         (o.CadastralNumber != null && o.CadastralNumber.Contains(search)));
            }

            var objects = query.OrderBy(o => o.Address)
                .Select(o => new ObjectDto
                {
                    Id = o.Id,
                    Address = o.Address,
                    CadastralNumber = o.CadastralNumber,
                    Type = o.Type,
                    TotalArea = o.TotalArea,
                    Purpose = o.Purpose,
                    Condition = o.Condition,
                    IsRentedNow = o.IsRentedNow
                })
                .ToList();

            return Ok(objects);
        }

        // GET: api/propertyobjects/available
        [HttpGet("available")]
        public IActionResult GetAvailable()
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var objects = _context.PropertyObjects
                .Where(o => !o.IsRentedNow)
                .Select(o => new ObjectDto
                {
                    Id = o.Id,
                    Address = o.Address,
                    CadastralNumber = o.CadastralNumber,
                    Type = o.Type,
                    TotalArea = o.TotalArea,
                    Purpose = o.Purpose,
                    Condition = o.Condition,
                    IsRentedNow = o.IsRentedNow
                })
                .ToList();

            return Ok(objects);
        }

        // GET: api/propertyobjects/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "accountant", "property_worker" });
            if (user == null) return Unauthorized();

            var obj = _context.PropertyObjects.Find(id);
            if (obj == null) return NotFound();

            return Ok(new ObjectDto
            {
                Id = obj.Id,
                Address = obj.Address,
                CadastralNumber = obj.CadastralNumber,
                Type = obj.Type,
                TotalArea = obj.TotalArea,
                Purpose = obj.Purpose,
                Condition = obj.Condition,
                IsRentedNow = obj.IsRentedNow
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateObjectRequest request)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
            if (user == null) return Unauthorized();

            if (!string.IsNullOrEmpty(request.CadastralNumber))
            {
                if (_context.PropertyObjects.Any(o => o.CadastralNumber == request.CadastralNumber))
                    return BadRequest("Объект с таким кадастровым номером уже существует.");
            }

            // Получаем максимальный ID + 1
            int newId = 1;
            if (_context.PropertyObjects.Any())
            {
                newId = _context.PropertyObjects.Max(o => o.Id) + 1;
            }

            var obj = new PropertyObject
            {
                Id = newId, // ← ВРУЧНУЮ ПЕРЕДАЁМ ID
                Address = request.Address,
                CadastralNumber = request.CadastralNumber,
                Type = request.Type,
                TotalArea = request.TotalArea,
                Purpose = request.Purpose,
                Condition = request.Condition,
                IsRentedNow = request.IsRentedNow
            };

            _context.PropertyObjects.Add(obj);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = obj.Id }, new { obj.Id, obj.Address });
        }

        // PUT: api/propertyobjects/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateObjectRequest request)
        {
            var user = AuthorizeAndGetUser(new[] { "admin", "property_worker" });
            if (user == null) return Unauthorized();

            var obj = _context.PropertyObjects.Find(id);
            if (obj == null) return NotFound();

            // Проверка уникальности кадастрового номера (если изменился)
            if (!string.IsNullOrEmpty(request.CadastralNumber) &&
                request.CadastralNumber != obj.CadastralNumber)
            {
                if (_context.PropertyObjects.Any(o => o.CadastralNumber == request.CadastralNumber && o.Id != id))
                    return BadRequest("Объект с таким кадастровым номером уже существует.");
                obj.CadastralNumber = request.CadastralNumber;
            }

            if (!string.IsNullOrEmpty(request.Address))
                obj.Address = request.Address;

            if (!string.IsNullOrEmpty(request.Type))
                obj.Type = request.Type;

            if (request.TotalArea.HasValue)
                obj.TotalArea = request.TotalArea;

            if (!string.IsNullOrEmpty(request.Purpose))
                obj.Purpose = request.Purpose;

            if (!string.IsNullOrEmpty(request.Condition))
                obj.Condition = request.Condition;

            _context.SaveChanges();

            return Ok(new { message = "Объект обновлен", id = obj.Id });
        }

        // DELETE: api/propertyobjects/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = AuthorizeAndGetUser(new[] { "admin" });
            if (user == null) return Unauthorized();

            var obj = _context.PropertyObjects.Find(id);
            if (obj == null) return NotFound();

            // Проверка наличия активных договоров
            if (_context.Contracts.Any(c => c.ObjectId == id && c.ContractStatus == "действует"))
                return BadRequest("Нельзя удалить объект, на который есть активный договор.");

            _context.PropertyObjects.Remove(obj);
            _context.SaveChanges();

            return NoContent();
        }
    }

    // Дополнительный DTO для обновления объекта
    public class UpdateObjectRequest
    {
        public string? Address { get; set; }
        public string? CadastralNumber { get; set; }
        public string? Type { get; set; }
        public decimal? TotalArea { get; set; }
        public string? Purpose { get; set; }
        public string? Condition { get; set; }
    }
}