using CiphersGrid.TelemetryService.Data;
using CiphersGrid.TelemetryService.Repositories;
using CiphersGrid.TelemetryService.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TelemetryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TelemetryDb")));

builder.Services.AddScoped<LapRecordRepository>();
builder.Services.AddScoped<DriverPositionRepository>();
builder.Services.AddScoped<TelemetryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Telemetry Service API",
        Version = "v1",
        Description = "Module 4: Microservices - Lap timing and position tracking"
    });
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Telemetry Service v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
    .WithName("Health")
    .Produces(200);

app.Run();
