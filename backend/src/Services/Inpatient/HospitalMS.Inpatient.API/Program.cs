using Serilog;
using HospitalMS.BuildingBlocks.Middleware;
using HospitalMS.Inpatient.Application;
using HospitalMS.Inpatient.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure layers
builder.Services.AddInpatientApplication();
builder.Services.AddInpatientInfrastructure(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HospitalMS - Inpatient API", Version = "v1" });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inpatient API v1"));

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HospitalMS.Inpatient.Infrastructure.Persistence.InpatientDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await HospitalMS.Inpatient.Infrastructure.Persistence.InpatientDbInitializer.SeedAsync(db, logger);
}

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
