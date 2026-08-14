using Microsoft.EntityFrameworkCore;
using TorettoMotors.BLL.Services.Implementations;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Repositories.Implementations;
using TorettoMotors.DAL.Repositories.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---- Data Access -------------------------------------------------------
builder.Services.AddDbContext<TorettoDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=toretto.db");
    }
    else
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The 'DefaultConnection' connection string is required when running outside Development.");
        }

        options.UseSqlServer(connectionString);
    }
});

// ---- Repositories ------------------------------------------------------
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IMaintenancePlanRepository, MaintenancePlanRepository>();

// ---- Services ----------------------------------------------------------
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IMaintenancePlanService, MaintenancePlanService>();

// ---- Controllers & Swagger ---------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Toretto Motors API",
        Version = "v1",
        Description = "Vehicle maintenance management — Module 2: N-Tier demo"
    });
});

WebApplication app = builder.Build();

// ---- Migrations & Seed -------------------------------------------------
using (IServiceScope scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TorettoDbContext>();
    await context.Database.MigrateAsync();
}

// ---- Middleware --------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Toretto Motors API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
