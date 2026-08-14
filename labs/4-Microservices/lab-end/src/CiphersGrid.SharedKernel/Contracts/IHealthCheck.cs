namespace CiphersGrid.SharedKernel.Contracts;

public interface IHealthCheck
{
    Task<bool> IsHealthyAsync();
}
