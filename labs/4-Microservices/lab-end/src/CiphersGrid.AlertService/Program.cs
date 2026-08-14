using CiphersGrid.AlertService.Data;
using CiphersGrid.AlertService.Repositories;
using CiphersGrid.AlertService.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlertDbContext>(options =>
		options.UseSqlite(builder.Configuration.GetConnectionString("AlertDb")));

builder.Services.AddScoped<AlertRepository>();
builder.Services.AddScoped<AlertService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new()
	{
		Title = "Alert Service API",
		Version = "v1",
		Description = "Module 4: Microservices - Crew intelligence alerts"
	});
});

WebApplication app = builder.Build();

// Apply migrations at startup
using (IServiceScope scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
	await dbContext.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "Alert Service v1");
	options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
		.WithName("Health")
		.Produces(200);

app.Run();