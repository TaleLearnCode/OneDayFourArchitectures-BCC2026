using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Models;
using TheCircuit.Penalties.Repositories;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Penalties.Services;

internal class PenaltyService : IPenaltyService
{
	private readonly PenaltyRepository _repository;
	private readonly IResultsService _resultsService;

	public PenaltyService(PenaltiesDbContext context, IResultsService resultsService)
	{
		_repository = new PenaltyRepository(context);
		_resultsService = resultsService;
	}

	public async Task<PenaltyDto?> GetPenaltyByIdAsync(PenaltyId id)
	{
		var penalty = await _repository.GetByIdAsync(id);
		return penalty is null ? null : MapToDto(penalty);
	}

	public async Task<IEnumerable<PenaltyDto>> GetPenaltiesByEventAsync(EventId eventId)
	{
		var penalties = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
			.ToListAsync(_repository.GetByEventId(eventId));
		return penalties.Select(MapToDto);
	}

	public async Task<PenaltyDto> IssuePenaltyAsync(PenaltyDto penaltyDto)
	{
		var penalty = new Penalty
		{
			EventId = penaltyDto.EventId.Value,
			RacerId = penaltyDto.RacerId.Value,
			Reason = penaltyDto.Reason,
			PenaltySeconds = penaltyDto.PenaltySeconds,
			IssuedAt = penaltyDto.IssuedAt,
			OfficialNotes = penaltyDto.OfficialNotes,
			Status = PenaltyStatus.Issued
		};

		var saved = await _repository.AddAsync(penalty);

		// Apply penalty to race results
		await _resultsService.ApplyPenaltyAsync(
				penaltyDto.EventId,
				penaltyDto.RacerId,
				penaltyDto.PenaltySeconds
		);

		// Mark as applied
		saved.Status = PenaltyStatus.Applied;
		await _repository.UpdateAsync(saved);

		return MapToDto(saved);
	}

	private PenaltyDto MapToDto(Penalty penalty)
	{
		return new PenaltyDto(
				new PenaltyId(penalty.Id),
				new EventId(penalty.EventId),
				new RacerId(penalty.RacerId),
				penalty.Reason,
				penalty.PenaltySeconds,
				penalty.IssuedAt,
				penalty.OfficialNotes,
				penalty.Status
		);
	}
}