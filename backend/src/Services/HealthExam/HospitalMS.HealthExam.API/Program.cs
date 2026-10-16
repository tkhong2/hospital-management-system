using HospitalMS.BuildingBlocks.Middleware;
using HospitalMS.HealthExam.Application;
using HospitalMS.HealthExam.Infrastructure;
using HospitalMS.HealthExam.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure layers
builder.Services.AddHealthExamApplication();
builder.Services.AddHealthExamInfrastructure(builder.Configuration);

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
        Title = "HospitalMS - HealthExam Service API",
        Version = "v1",
        Description = "Microservice Quản lý Khám Sức Khỏe (Hợp đồng, Gói khám, Tiếp đón, Đa trạm chuyên khoa, Cận lâm sàng, Kết luận & Giấy KSK)."
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HealthExamDbContext>();
    await HealthExamDbInitializer.SeedAsync(dbContext);
}

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthExam API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
