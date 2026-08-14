using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Events;
using TheCircuit.Participants;
using TheCircuit.Results;
using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Tests;

public class StarterModuleSmokeTests
{
    [Fact]
    public async Task ExistingModules_ShouldRegisterSeedAndUpdateResults()
    {
        string databasePath = Path.Combine(
            AppContext.BaseDirectory,
            $"the-circuit-starter-{Guid.NewGuid():N}.db");
        ServiceProvider? provider = null;

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath};Pooling=False"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddEventsModule(configuration);
            services.AddParticipantsModule(configuration);
            services.AddResultsModule(configuration);

            provider = services.BuildServiceProvider();

            await EventsModule.InitializeEventsAsync(provider);
            await ParticipantsModule.InitializeParticipantsAsync(provider);
            await ResultsModule.InitializeResultsAsync(provider);

            using IServiceScope scope = provider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var participantService = scope.ServiceProvider.GetRequiredService<IParticipantService>();
            var resultsService = scope.ServiceProvider.GetRequiredService<IResultsService>();

            (await eventService.GetAllEventsAsync()).Should().NotBeEmpty();
            (await participantService.GetAllRacersAsync()).Should().NotBeEmpty();

            RaceResultDto? resultBeforePenalty = await resultsService.GetResultByIdAsync(new ResultId(1));
            resultBeforePenalty.Should().NotBeNull();
            resultBeforePenalty!.Status.Should().Be(RaceResultStatus.Completed);

            await resultsService.ApplyPenaltyAsync(new EventId(1), new RacerId(1), 5);

            RaceResultDto? resultAfterPenalty = await resultsService.GetResultByIdAsync(new ResultId(1));
            resultAfterPenalty.Should().NotBeNull();
            resultAfterPenalty!.AdjustedTimeMs.Should().Be(resultBeforePenalty.AdjustedTimeMs + 5_000);
        }
        finally
        {
            if (provider is not null)
            {
                await provider.DisposeAsync();
            }

            DeleteIfExists(databasePath);
            DeleteIfExists($"{databasePath}-shm");
            DeleteIfExists($"{databasePath}-wal");
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
