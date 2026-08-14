using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.Contracts;

public interface IResultsService
{
    Task<RaceResultDto?> GetResultByIdAsync(ResultId resultId);
    Task<IEnumerable<RaceResultDto>> GetResultsByEventAsync(EventId eventId);
    Task<RaceResultDto> RecordResultAsync(RaceResultDto resultDto);
    Task ApplyPenaltyAsync(EventId eventId, RacerId racerId, int penaltySeconds);
}
