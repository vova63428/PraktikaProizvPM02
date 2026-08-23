using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MunicipalPropertyAPI;
using MunicipalPropertyAPI.Data;
using MunicipalPropertyAPI.Dto;
using MunicipalPropertyAPI.Models;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace MunicipalPropertyAPI.IntegrationTests
{
    [TestFixture]
    public class ApiIntegrationTests
    {
        private WebApplicationFactory<Program> _factory = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        string dbName = "IntegrationTests_" + Guid.NewGuid();
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseInMemoryDatabase(dbName));
                    });
                });

            // ===== ДОБАВЛЯЕМ ПОЛЬЗОВАТЕЛЕЙ В БАЗУ =====
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Пользователь admin1
                var admin1 = new User
                {
                    Id = 1,
                    Login = "admin1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Администратор",
                    Role = "admin1",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(admin1);
                
                // Пользователь tenant_522
                var tenant = new User
                {
                    Id = 2,
                    Login = "tenant_522",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    FullName = "Арендатор 522",
                    Role = "tenant",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(tenant);

                context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _factory?.Dispose();
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД — СОЗДАЁТ НОВЫЙ КЛИЕНТ
        // ============================================================
        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Remove("X-Login");
            client.DefaultRequestHeaders.Remove("X-Password");
            return client;
        }

        private HttpClient CreateClientWithAuth(string login, string password)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Remove("X-Login");
            client.DefaultRequestHeaders.Remove("X-Password");
            client.DefaultRequestHeaders.Add("X-Login", login);
            client.DefaultRequestHeaders.Add("X-Password", password);
            return client;
        }

        // ============================================================
        // ТЕСТ 1: Без авторизации — доступ к договорам запрещён
        // ============================================================
        [Test]
        public async Task Unauthorized_Access_Contracts_Denied()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/api/contracts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 2: Без авторизации — доступ к объектам запрещён
        // ============================================================
        [Test]
        public async Task Unauthorized_Access_Objects_Denied()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/api/propertyobjects");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 3: Без авторизации — доступ к арендаторам запрещён
        // ============================================================
        [Test]
        public async Task Unauthorized_Access_Tenants_Denied()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/api/tenants");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 4: Без авторизации — доступ к платежам запрещён
        // ============================================================
        [Test]
        public async Task Unauthorized_Access_Payments_Denied()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/api/payments/summary");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 5: Без авторизации — доступ к должникам запрещён
        // ============================================================
        [Test]
        public async Task Unauthorized_Access_Debtors_Denied()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/api/payments/debtors");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 6: Арендатор НЕ может видеть объекты
        // ============================================================
        [Test]
        public async Task Tenant_CannotAccessObjects()
        {
            using var client = CreateClientWithAuth("tenant_52", "password");
            var response = await client.GetAsync("/api/propertyobjects");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // ============================================================
        // ТЕСТ 7: Арендатор НЕ может видеть арендаторов
        // ============================================================
        [Test]
        public async Task Tenant_CannotAccessTenants()
        {
            using var client = CreateClientWithAuth("tenant_52", "password");
            var response = await client.GetAsync("/api/tenants");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}