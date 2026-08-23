using MunicipalPropertyAPI.Models;

public class Payment
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime PeriodMonth { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentType { get; set; }
    public bool IsPenalty { get; set; }  // ← ИЗМЕНЕНО: bool? → bool
    public int EmployeeWhoAcceptedId { get; set; }
    public string? ReceiptNumber { get; set; }
    public int? UserWhoAcceptedId { get; set; }

    public virtual Contract Contract { get; set; } = null!;
    public virtual Employee EmployeeWhoAccepted { get; set; } = null!;
}