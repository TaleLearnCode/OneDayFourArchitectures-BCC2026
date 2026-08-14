using CiphersGrid.CrewService.Data;
using CiphersGrid.CrewService.Repositories;
using CiphersGrid.CrewService.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CrewDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CrewDb")));

builder.Services.AddScoped<DriverRepository>();
builder.Services.AddScoped<CrewService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Crew Service API",
        Version = "v1",
        Description = "Module 4: Microservices - Crew management service"
    });
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CrewDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Crew Service v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
    .WithName("Health")
    .Produces(200);

app.Run();
