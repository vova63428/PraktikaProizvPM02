namespace MunicipalPropertyAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = null!; // admin, property_worker, tenant, accountant
        public bool IsActive { get; set; } = false; // false = не заблокирован
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedAt { get; set; }

        public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}