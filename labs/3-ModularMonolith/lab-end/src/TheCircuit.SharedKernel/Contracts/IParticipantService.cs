using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.Contracts;

public interface IParticipantService
{
    Task<ParticipantDto?> GetRacerByIdAsync(RacerId racerId);
    Task<IEnumerable<ParticipantDto>> GetAllRacersAsync();
    Task<ParticipantDto> CreateRacerAsync(ParticipantDto racerDto);
}
