using TheCircuit.Results.Models;
using TheCircuit.Results.Repositories;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Results.Services;

internal class ResultsService : IResultsService
{
    private readonly ResultRepository _repository;
    private readonly IParticipantService _participantService;
    private readonly IEventService _eventService;

    public ResultsService(
        ResultRepository repository,
        IParticipantService participantService,
        IEventService eventService)
    {
        _repository = repository;
        _participantService = participantService;
        _eventService = eventService;
    }

    public async Task<RaceResultDto?> GetResultByIdAsync(ResultId resultId)
    {
        var result = await _repository.GetByIdAsync(resultId);
        return result is null ? null : MapToDto(result);
    }

    public async Task<IEnumerable<RaceResultDto>> GetResultsByEventAsync(EventId eventId)
    {
        var results = _repository.GetByEventId(eventId).OrderBy(r => r.FinishPosition).ToList();
        return results.Select(MapToDto);
    }

    public async Task<RaceResultDto> RecordResultAsync(RaceResultDto resultDto)
    {
        var result = new RaceResult
        {
            EventId = resultDto.EventId.Value,
            RacerId = resultDto.RacerId.Value,
            FinishPosition = resultDto.FinishPosition,
            LapTimeMs = resultDto.LapTimeMs,
            AdjustedTimeMs = resultDto.AdjustedTimeMs,
            Points = resultDto.Points,
            Status = resultDto.Status
        };

        await _repository.AddAsync(result);
        return MapToDto(result);
    }

    public async Task ApplyPenaltyAsync(EventId eventId, RacerId racerId, int penaltySeconds)
    {
        var results = _repository.GetByEventId(eventId)
            .Where(r => r.RacerId == racerId.Value)
            .ToList();

        foreach (var result in results)
        {
            result.AdjustedTimeMs += (penaltySeconds * 1000);
            await _repository.UpdateAsync(result);
        }
    }

    private static RaceResultDto MapToDto(RaceResult result)
    {
        return new RaceResultDto(
            new ResultId(result.Id),
            new EventId(result.EventId),
            new RacerId(result.RacerId),
            result.FinishPosition,
            result.LapTimeMs,
            result.AdjustedTimeMs,
            result.Points,
            result.Status
        );
    }
}
