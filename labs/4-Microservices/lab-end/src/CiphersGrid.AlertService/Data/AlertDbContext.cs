using CiphersGrid.AlertService.Models;
using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.AlertService.Data;

public class AlertDbContext(DbContextOptions<AlertDbContext> options) : DbContext(options)
{
	public DbSet<Alert> Alerts { get; set; }
}