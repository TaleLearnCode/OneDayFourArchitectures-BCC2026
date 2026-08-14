// Dom's Garage — Module 1: Monolith
// ============================================================
// Single deployable unit. Flat folder structure. Shared DbContext.
// No network hops. No layer enforcement. Everything in one project.
//
// WALKTHROUGH STOP — Anti-Pattern 3: All-or-Nothing Deployment
// To ship a one-line fix in CarService, you deploy every line of code here.
// At small scale: minor inconvenience. At scale: a typo ships with everything.
// ============================================================

using DomsGarage.Data;
using DomsGarage.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---- Data Access -------------------------------------------------------
// One DbContext. Registered once. Shared by every service in the application.
// WALKTHROUGH STOP: the simplicity of Program.cs is a feature. Read this in 30 seconds.
builder.Services.AddDbContext<GarageDbContext>(options =>
		options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
				?? "Data Source=garage.db"));

// ---- Services ----------------------------------------------------------
// No interfaces. No modules. Direct injection into controllers.
// Anti-pattern note: at 40+ services, this list becomes unwieldy.
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<MechanicService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<PartService>();

// LAB Step 3: Register ServiceRecordService here
// builder.Services.AddScoped<ServiceRecordService>();

// ---- Controllers & Swagger ---------------------------------------------
// Swagger is always enabled — not just in Development.
// Participants hit /swagger on first run without any env config overhead.
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
		Title = "Dom's Garage API",
		Version = "v1",
		Description = "Auto repair shop management — Module 1: Monolith demo"
	});
});

WebApplication app = builder.Build();

// ---- Migrations & Seed -------------------------------------------------
// Applies pending migrations on startup and seeds F&F demo data.
// Participants will run their own migration in the lab: dotnet ef migrations add AddServiceRecord
using (IServiceScope scope = app.Services.CreateScope())
{
	GarageDbContext db = scope.ServiceProvider.GetRequiredService<GarageDbContext>();
	await GarageSeeder.SeedAsync(db);
}

// ---- Middleware --------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dom''s Garage v1");
	options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
