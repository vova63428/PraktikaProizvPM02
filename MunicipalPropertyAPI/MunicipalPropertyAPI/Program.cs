using Microsoft.EntityFrameworkCore;
using MunicipalPropertyAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// === Подключение к БД ===
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === Контроллеры ===
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Municipal Property API",
        Version = "v1",
        Description = "API для учета муниципального имущества и аренды помещений"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Municipal Property API v1");
        c.RoutePrefix = "swagger";  // ← ИСПРАВЛЕНО - теперь Swagger будет на корневом пути
    });
}

// ЗАКОММЕНТИРУЙТЕ ИЛИ УДАЛИТЕ ЭТУ СТРОКУ:
// app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("==========================================");
Console.WriteLine(" Municipal Property API v1.0");
Console.WriteLine("==========================================");
Console.WriteLine($" Swagger: http://localhost:5236");
Console.WriteLine("==========================================");

app.Run();
public partial class Program { }