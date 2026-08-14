using TheCircuit.Participants.Models;
using TheCircuit.Participants.Repositories;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Participants.Services;

internal class ParticipantService : IParticipantService
{
    private readonly RacerRepository _repository;

    public ParticipantService(RacerRepository repository)
    {
        _repository = repository;
    }

    public async Task<ParticipantDto?> GetRacerByIdAsync(RacerId racerId)
    {
        var racer = await _repository.GetByIdAsync(racerId);
        return racer is null ? null : MapToDto(racer);
    }

    public async Task<IEnumerable<ParticipantDto>> GetAllRacersAsync()
    {
        var racers = _repository.GetAll().ToList();
        return racers.Select(MapToDto);
    }

    public async Task<ParticipantDto> CreateRacerAsync(ParticipantDto racerDto)
    {
        var racer = new Racer
        {
            FullName = racerDto.FullName,
            LicenseNumber = racerDto.LicenseNumber,
            TeamName = racerDto.TeamName,
            IsActive = racerDto.IsActive
        };

        await _repository.AddAsync(racer);
        return MapToDto(racer);
    }

    private static ParticipantDto MapToDto(Racer racer)
    {
        return new ParticipantDto(
            new RacerId(racer.Id),
            racer.FullName,
            racer.LicenseNumber,
            racer.TeamName,
            racer.IsActive
        );
    }
}
