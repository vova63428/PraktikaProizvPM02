using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MunicipalPropertyAPI.Models
{
    public class PropertyObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_object")]
        public int Id { get; set; }

        public string Address { get; set; } = null!;
        public string? CadastralNumber { get; set; }
        public string Type { get; set; } = null!;
        public decimal? TotalArea { get; set; }
        public string? Purpose { get; set; }
        public string? Condition { get; set; }
        public bool IsRentedNow { get; set; }
    }
}