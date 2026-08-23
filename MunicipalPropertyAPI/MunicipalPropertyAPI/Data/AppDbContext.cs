using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Models;

namespace MunicipalPropertyAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<PropertyObject> PropertyObjects { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext() : base()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_user");
                entity.Property(e => e.Login).HasColumnName("login").IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.PasswordHash).HasColumnName("password").IsRequired().HasMaxLength(128);
                entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Role).HasColumnName("role").IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsActive).HasColumnName("is_blocked").HasDefaultValue(0);
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            });

            // === Contract ===
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("contracts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_contract");
                entity.Property(e => e.ContractNumber).HasColumnName("contract_number").IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.Property(e => e.ObjectId).HasColumnName("id_object");
                entity.Property(e => e.TenantId).HasColumnName("id_tenant");
                entity.Property(e => e.ResponsibleEmployeeId).HasColumnName("id_responsible_employee");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.MonthlyRate).HasColumnName("monthly_rate").HasPrecision(10, 2);
                entity.Property(e => e.PaymentDay).HasColumnName("payment_day");
                entity.Property(e => e.ContractStatus).HasColumnName("contract_status").HasMaxLength(30).HasDefaultValue("действует");
                entity.Property(e => e.Notes).HasColumnName("notes");

                entity.HasOne(e => e.PropertyObject)
                    .WithMany()
                    .HasForeignKey(e => e.ObjectId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Contracts)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ResponsibleEmployee)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.ResponsibleEmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // === PropertyObject (ИСПРАВЛЕНО!) ===
            modelBuilder.Entity<PropertyObject>(entity =>
            {
                entity.ToTable("objects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_object").ValueGeneratedOnAdd();
                entity.Property(e => e.Address).HasColumnName("address").IsRequired().HasMaxLength(200);
                entity.Property(e => e.CadastralNumber).HasColumnName("cadastral_number").HasMaxLength(50);
                entity.HasIndex(e => e.CadastralNumber).IsUnique();
                entity.Property(e => e.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalArea).HasColumnName("total_area").HasPrecision(10, 2);
                entity.Property(e => e.Purpose).HasColumnName("purpose").HasMaxLength(100);
                entity.Property(e => e.Condition).HasColumnName("condition").HasMaxLength(50);
                entity.Property(e => e.IsRentedNow).HasColumnName("is_rented_now").HasDefaultValue(false);
            });

            // === Tenant ===
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("tenants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_tenant").ValueGeneratedOnAdd();
                entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(20);
                entity.Property(e => e.Inn).HasColumnName("inn").IsRequired().HasMaxLength(12);
                entity.HasIndex(e => e.Inn).IsUnique();
                entity.Property(e => e.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
                entity.Property(e => e.ShortName).HasColumnName("short_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.LegalAddress).HasColumnName("legal_address").HasMaxLength(200);
                entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
                entity.Property(e => e.UserId).HasColumnName("id_user");  // ← ДОБАВЛЕНО!
            });

            // === Employee ===
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_employee");
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
                entity.Property(e => e.PositionId).HasColumnName("id_position");
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.HireDate).HasColumnName("hire_date");
                entity.Property(e => e.Login).HasColumnName("login").IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.PasswordHash).HasColumnName("password").IsRequired().HasMaxLength(128);
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(30);
                entity.Property(e => e.DepartmentId).HasColumnName("department");

                entity.HasOne(e => e.Position)
                    .WithMany()
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // === Payment ===
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_payment");
                entity.Property(e => e.ContractId).HasColumnName("id_contract");
                entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
                entity.Property(e => e.PeriodMonth).HasColumnName("period_month");
                entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(10, 2);
                entity.Property(e => e.PaymentType).HasColumnName("payment_type").HasMaxLength(30).HasDefaultValue("безналичный");
                entity.Property(e => e.IsPenalty).HasColumnName("is_penalty").HasDefaultValue(false);
                entity.Property(e => e.EmployeeWhoAcceptedId).HasColumnName("id_employee_who_accepted");
                entity.Property(e => e.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(50);
                entity.Property(e => e.UserWhoAcceptedId).HasColumnName("id_user_who_accepted");

                entity.HasOne(e => e.Contract)
                    .WithMany(c => c.Payments)
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.EmployeeWhoAccepted)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.EmployeeWhoAcceptedId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // === Department ===
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("departments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_department").ValueGeneratedOnAdd();
                entity.Property(e => e.DepartmentName).HasColumnName("department_name").IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.DepartmentName).IsUnique();
            });

            // === Position ===
            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("positions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_position");
                entity.Property(e => e.PositionName).HasColumnName("position_name").IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.PositionName).IsUnique();
                entity.Property(e => e.SalaryGrade).HasColumnName("salary_grade").HasMaxLength(50);
                entity.Property(e => e.AccessLevel).HasColumnName("access_level").HasDefaultValue(1);
                entity.Property(e => e.BaseSalary).HasColumnName("base_salary").HasPrecision(10, 2);
            });

            // === AuditLog ===
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_log");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_log").ValueGeneratedOnAdd();
                entity.Property(e => e.EmployeeId).HasColumnName("id_employee");
                entity.Property(e => e.ActionType).HasColumnName("action_type").IsRequired().HasMaxLength(30);
                entity.Property(e => e.TableName).HasColumnName("table_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.RecordId).HasColumnName("record_id");
                entity.Property(e => e.ActionDateTime).HasColumnName("action_datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.OldValue).HasColumnName("old_value");
                entity.Property(e => e.NewValue).HasColumnName("new_value");
                entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
                entity.Property(e => e.UserId).HasColumnName("id_user");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}