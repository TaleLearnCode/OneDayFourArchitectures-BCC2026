using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.AlertService.Tests.xUnit;

/// <summary>
/// Test fixture for in-memory Alert database.
/// Provides a clean database context for each test.
/// </summary>
public class TestAlertDbContext : AlertDbContext
{
    public TestAlertDbContext() : base(GetContextOptions())
    {
        // Initialize in-memory database for testing
        Database.EnsureCreated();
    }

    private static DbContextOptions<AlertDbContext> GetContextOptions()
    {
        // TODO: Configure in-memory SQLite database
        // Use a unique database name for test isolation
        //return new DbContextOptionsBuilder<AlertDbContext>()
        //    .UseSqlite("DataSource=:memory:")
        //    .Options;

        throw new NotImplementedException("Configure test database context");
    }
}
