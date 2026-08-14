using TheCircuit.Events;
using TheCircuit.Participants;
using TheCircuit.Results;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---- Module Registration -----------------------------------------------
builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddParticipantsModule(builder.Configuration);
builder.Services.AddResultsModule(builder.Configuration);

// ---- Controllers & Swagger -------------------------------------------------
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
        Title = "The Circuit Race Event API",
        Version = "v1",
        Description = "Module 3: Modular Monolith demo — Race event management system"
    });
});

WebApplication app = builder.Build();

// ---- Database Initialization -----------------------------------------------
using (IServiceScope scope = app.Services.CreateScope())
{
    // Initialize all modules
    await TheCircuit.Events.EventsModule.InitializeEventsAsync(app.Services);
    await TheCircuit.Participants.ParticipantsModule.InitializeParticipantsAsync(app.Services);
    await TheCircuit.Results.ResultsModule.InitializeResultsAsync(app.Services);
}

// ---- Middleware --------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "The Circuit v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
    .WithName("Health")
    .Produces(200);

app.Run();
