namespace MunicipalPropertyAPI.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string Inn { get; set; } = null!;
        public string? Ogrn { get; set; }
        public string ShortName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LegalAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? UserId { get; set; }  // ← ИСПРАВЛЕНО: UserId (с I)

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}