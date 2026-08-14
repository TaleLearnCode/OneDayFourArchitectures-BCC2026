using CiphersGrid.RaceService.Data;
using CiphersGrid.RaceService.Repositories;
using CiphersGrid.RaceService.Services;
using CiphersGrid.RaceService.Clients;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("RaceDb")));

builder.Services.AddScoped<RaceRepository>();
builder.Services.AddScoped<RaceEntryRepository>();
builder.Services.AddScoped<RaceService>();

builder.Services.AddHttpClient<AlertServiceClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AlertService"] ?? "http://localhost:5300"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Race Service API",
        Version = "v1",
        Description = "Module 4: Microservices - Race management service"
    });
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RaceDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Race Service v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
    .WithName("Health")
    .Produces(200);

app.Run();
