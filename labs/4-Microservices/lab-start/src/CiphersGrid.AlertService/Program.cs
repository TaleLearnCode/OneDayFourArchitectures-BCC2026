// TODO: Set up Program.cs with:
// - DbContext registration for AlertDbContext
// - Repository registration
// - Service registration
// - Swagger configuration
// - Controller mapping
// - Health check endpoint

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

app.MapGet("/health", () => Results.Ok("Alert Service is up (stub)")).WithName("Health");
app.Run();

