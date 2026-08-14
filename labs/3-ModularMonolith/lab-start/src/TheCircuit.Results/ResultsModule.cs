using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Results.Data;
using TheCircuit.Results.Models;
using TheCircuit.Results.Repositories;
using TheCircuit.Results.Services;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Results;

public static class ResultsModule
{
    public static IServiceCollection AddResultsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ResultsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=circuit.db"));

        services.AddScoped<ResultRepository>();
        services.AddScoped<IResultsService, ResultsService>();

        return services;
    }

    public static async Task InitializeResultsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ResultsDbContext>();
        await context.Database.MigrateAsync();
        await SeedResultsAsync(context);
    }

    private static async Task SeedResultsAsync(ResultsDbContext context)
    {
        if (await context.RaceResults.AnyAsync())
            return;

        var results = new[]
        {
            new RaceResult
            {
                EventId = 1,
                RacerId = 1,
                FinishPosition = 1,
                LapTimeMs = 125000,
                AdjustedTimeMs = 125000,
                Points = 25,
                Status = RaceResultStatus.Completed
            },
            new RaceResult
            {
                EventId = 1,
                RacerId = 2,
                FinishPosition = 2,
                LapTimeMs = 126500,
                AdjustedTimeMs = 126500,
                Points = 18,
                Status = RaceResultStatus.Completed
            },
            new RaceResult
            {
                EventId = 1,
                RacerId = 3,
                FinishPosition = 3,
                LapTimeMs = 127200,
                AdjustedTimeMs = 127200,
                Points = 15,
                Status = RaceResultStatus.Completed
            },
            new RaceResult
            {
                EventId = 1,
                RacerId = 4,
                FinishPosition = 4,
                LapTimeMs = 128100,
                AdjustedTimeMs = 128100,
                Points = 12,
                Status = RaceResultStatus.Completed
            },
            new RaceResult
            {
                EventId = 1,
                RacerId = 5,
                FinishPosition = 5,
                LapTimeMs = 129300,
                AdjustedTimeMs = 129300,
                Points = 10,
                Status = RaceResultStatus.Completed
            }
        };

        context.RaceResults.AddRange(results);
        await context.SaveChangesAsync();
    }
}
