using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Services;
using TheCircuit.SharedKernel.Contracts;

namespace TheCircuit.Penalties;

public static class PenaltiesModule
{
	public static IServiceCollection AddPenaltiesModule(
			this IServiceCollection services,
			IConfiguration configuration)
	{
		// Register DbContext
		services.AddDbContext<PenaltiesDbContext>(options =>
				options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=Penalties.db")
		);

		// Register service
		services.AddScoped<IPenaltyService, PenaltyService>();

		return services;
	}

	public static async Task InitializePenaltiesAsync(IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<PenaltiesDbContext>();
		await context.Database.MigrateAsync();
	}
}