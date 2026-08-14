using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Events.Data;
using TheCircuit.Events.Repositories;
using TheCircuit.Events.Services;
using TheCircuit.SharedKernel.Contracts;

namespace TheCircuit.Events;

public static class EventsModule
{
    public static IServiceCollection AddEventsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=circuit.db"));

        services.AddScoped<EventRepository>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }

    public static async Task InitializeEventsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        await context.Database.MigrateAsync();
        await SeedEventsAsync(context);
    }

    private static async Task SeedEventsAsync(EventsDbContext context)
    {
        if (await context.Events.AnyAsync())
            return;

        var events = new[]
        {
            new Models.Event
            {
                EventName = "Spring Grand Prix 2026",
                ScheduledDate = new DateTime(2026, 5, 15, 14, 0, 0),
                VenueId = "track-001",
                Status = SharedKernel.Enums.EventStatus.Scheduled
            },
            new Models.Event
            {
                EventName = "Summer Circuit Challenge",
                ScheduledDate = new DateTime(2026, 7, 22, 10, 0, 0),
                VenueId = "track-002",
                Status = SharedKernel.Enums.EventStatus.Scheduled
            },
            new Models.Event
            {
                EventName = "Fall Classic",
                ScheduledDate = new DateTime(2026, 9, 18, 15, 30, 0),
                VenueId = "track-001",
                Status = SharedKernel.Enums.EventStatus.Scheduled
            }
        };

        context.Events.AddRange(events);
        await context.SaveChangesAsync();
    }
}
