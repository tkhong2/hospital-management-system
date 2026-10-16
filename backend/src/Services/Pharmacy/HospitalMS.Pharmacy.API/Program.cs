using HospitalMS.BuildingBlocks.Middleware;
using HospitalMS.Pharmacy.Application;
using HospitalMS.Pharmacy.Infrastructure;
using HospitalMS.Pharmacy.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure layers
builder.Services.AddPharmacyApplication();
builder.Services.AddPharmacyInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5100")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HospitalMS - Pharmacy Service API",
        Version = "v1",
        Description = "Microservice Quản lý Kho Dược & Vật tư y tế (FEFO, Lô - Hạn dùng, Nhập - Xuất - Chuyển, Thẻ kho, Cấp phát đơn thuốc, Cảnh báo tồn kho)."
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
    await PharmacyDbInitializer.SeedAsync(dbContext);
}

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pharmacy API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
