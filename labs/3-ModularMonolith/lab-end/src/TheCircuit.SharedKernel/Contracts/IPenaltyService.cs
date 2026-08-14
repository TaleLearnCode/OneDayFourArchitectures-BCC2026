using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.Contracts;

public interface IPenaltyService
{
    Task<PenaltyDto?> GetPenaltyByIdAsync(PenaltyId penaltyId);
    Task<IEnumerable<PenaltyDto>> GetPenaltiesByEventAsync(EventId eventId);
    Task<PenaltyDto> IssuePenaltyAsync(PenaltyDto penaltyDto);
}
