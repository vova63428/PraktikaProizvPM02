using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MunicipalPropertyAPI.Models
{
    public class Contract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int Id { get; set; }

        public string ContractNumber { get; set; } = null!;
        public int ObjectId { get; set; }
        public int TenantId { get; set; }
        public int ResponsibleEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public int? PaymentDay { get; set; }
        public string ContractStatus { get; set; } = "действует";
        public string? Notes { get; set; }

        public virtual PropertyObject? PropertyObject { get; set; }
        public virtual Tenant? Tenant { get; set; }
        public virtual Employee? ResponsibleEmployee { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}