using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Participants.Data;
using TheCircuit.Participants.Models;
using TheCircuit.Participants.Repositories;
using TheCircuit.Participants.Services;
using TheCircuit.SharedKernel.Contracts;

namespace TheCircuit.Participants;

public static class ParticipantsModule
{
    public static IServiceCollection AddParticipantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParticipantsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=circuit.db"));

        services.AddScoped<RacerRepository>();
        services.AddScoped<IParticipantService, ParticipantService>();

        return services;
    }

    public static async Task InitializeParticipantsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParticipantsDbContext>();
        await context.Database.MigrateAsync();
        await SeedRacersAsync(context);
    }

    private static async Task SeedRacersAsync(ParticipantsDbContext context)
    {
        if (await context.Racers.AnyAsync())
            return;

        var racers = new[]
        {
            new Racer
            {
                FullName = "Dominic Toretto",
                LicenseNumber = "DT-001",
                TeamName = "Team Toretto",
                IsActive = true
            },
            new Racer
            {
                FullName = "Letty Ortiz",
                LicenseNumber = "LO-002",
                TeamName = "Team Toretto",
                IsActive = true
            },
            new Racer
            {
                FullName = "Brian O'Conner",
                LicenseNumber = "BO-003",
                TeamName = "Team Toretto",
                IsActive = true
            },
            new Racer
            {
                FullName = "Mia Toretto",
                LicenseNumber = "MT-004",
                TeamName = "Team Toretto",
                IsActive = true
            },
            new Racer
            {
                FullName = "Tej Parker",
                LicenseNumber = "TP-005",
                TeamName = "Team Toretto",
                IsActive = true
            },
            new Racer
            {
                FullName = "Roman Pearce",
                LicenseNumber = "RP-006",
                TeamName = "Team Rivals",
                IsActive = true
            },
            new Racer
            {
                FullName = "Han Lue",
                LicenseNumber = "HL-007",
                TeamName = "Team Rivals",
                IsActive = true
            },
            new Racer
            {
                FullName = "Gisele Yashar",
                LicenseNumber = "GY-008",
                TeamName = "Team Independent",
                IsActive = true
            }
        };

        context.Racers.AddRange(racers);
        await context.SaveChangesAsync();
    }
}
