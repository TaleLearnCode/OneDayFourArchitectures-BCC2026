using Microsoft.EntityFrameworkCore;
using TheCircuit.Penalties.Models;

namespace TheCircuit.Penalties.Data;

internal class PenaltiesDbContext : DbContext
{
	public PenaltiesDbContext(DbContextOptions<PenaltiesDbContext> options)
			: base(options)
	{
	}

	public DbSet<Penalty> Penalties => Set<Penalty>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Penalty>(entity =>
		{
			entity.HasKey(p => p.Id);
			entity.Property(p => p.EventId).IsRequired();
			entity.Property(p => p.RacerId).IsRequired();
			entity.Property(p => p.Reason).HasConversion<int>();
			entity.Property(p => p.Status).HasConversion<int>();
			entity.Property(p => p.OfficialNotes).IsRequired().HasMaxLength(500);
		});
	}
}