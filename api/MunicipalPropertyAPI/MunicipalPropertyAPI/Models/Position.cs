namespace MunicipalPropertyAPI.Models
{
    

    public class Position
    {
        public int Id { get; set; }
        public string PositionName { get; set; } = null!;
        public string? SalaryGrade { get; set; }
        public int? AccessLevel { get; set; }
        public decimal? BaseSalary { get; set; }
    }

    
    
}