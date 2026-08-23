namespace MunicipalPropertyAPI.Models
{
    

    public class AuditLog
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string ActionType { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public int RecordId { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public int? UserId { get; set; }

        public virtual User? User { get; set; }
    }
}