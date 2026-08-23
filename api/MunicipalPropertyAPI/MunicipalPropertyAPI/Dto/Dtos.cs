namespace MunicipalPropertyAPI.Dto
{
    // ===== АУТЕНТИФИКАЦИЯ =====
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // ===== ДОГОВОРЫ =====
    public class ContractDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int ObjectId { get; set; }
        public string ObjectAddress { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public int? PaymentDay { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateContractRequest
    {
        public string Number { get; set; } = string.Empty;
        public int ObjectId { get; set; }
        public int TenantId { get; set; }
        public int ResponsibleEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public int? PaymentDay { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateContractRequest
    {
        public decimal? MonthlyRate { get; set; }
        public int? PaymentDay { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PeriodMonth { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public bool? IsPenalty { get; set; }  // ← ИЗМЕНЕНО: bool → bool?
        public string? ReceiptNumber { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
    }

    public class CreatePaymentRequest
    {
        public int ContractId { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PeriodMonth { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = "безналичный";
        public bool? IsPenalty { get; set; }  // ← ИЗМЕНЕНО: bool? → bool?
        public int EmployeeWhoAcceptedId { get; set; }
        public string? ReceiptNumber { get; set; }
        public int? UserWhoAcceptedId { get; set; }
    }

    // ===== ОБЪЕКТЫ =====
    public class ObjectDto
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? CadastralNumber { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal? TotalArea { get; set; }
        public string? Purpose { get; set; }
        public string? Condition { get; set; }
        public bool IsRentedNow { get; set; }
    }

    public class CreateObjectRequest
    {
        public string Address { get; set; } = string.Empty;
        public string? CadastralNumber { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal? TotalArea { get; set; }
        public string? Purpose { get; set; }
        public string? Condition { get; set; }
        public bool IsRentedNow { get; set; } // ← ДОБАВИТЬ!
    }

    // ===== АРЕНДАТОРЫ =====
    public class TenantDto
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string Inn { get; set; } = string.Empty;
        public string? Ogrn { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LegalAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }

    public class CreateTenantRequest
    {
        public string? Type { get; set; }
        public string? Inn { get; set; }
        public string? Ogrn { get; set; }
        public string? ShortName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LegalAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }


    // ===== ДОЛЖНИКИ =====
    public class DebtorDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Inn { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
    }

    // ===== ОТЧЕТЫ =====
    public class PaymentSummaryDto
    {
        public decimal TotalPaymentsCurrentMonth { get; set; }
        public decimal TotalPaymentsLastMonth { get; set; }
        public decimal TotalPenalties { get; set; }
        public int ActiveContractsCount { get; set; }
    }
   
    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}