namespace MunicipalPropertyAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public int? PositionId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
        public string Login { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public string? Role { get; set; }
        public int DepartmentId { get; set; }

        public virtual Position? Position { get; set; }
        public virtual Department Department { get; set; } = null!;
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}