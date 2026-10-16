using Serilog;
using HospitalMS.BuildingBlocks.Middleware;
using HospitalMS.Outpatient.Application;
using HospitalMS.Outpatient.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure layers
builder.Services.AddOutpatientApplication();
builder.Services.AddOutpatientInfrastructure(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HospitalMS - Outpatient API", Version = "v1" });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Outpatient API v1"));

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HospitalMS.Outpatient.Infrastructure.Persistence.OutpatientDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await HospitalMS.Outpatient.Infrastructure.Persistence.OutpatientDbInitializer.SeedAsync(db, logger);
}

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
