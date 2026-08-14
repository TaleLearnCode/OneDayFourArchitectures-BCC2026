namespace CiphersGrid.RaceService.Clients;

public class AlertServiceClient(HttpClient httpClient)
{
    public async Task<bool> CreateAlertAsync(CreateAlertRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/alerts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<AlertResponse>?> GetAlertsForRaceAsync(Guid raceId)
    {
        var response = await httpClient.GetAsync($"/api/alerts?raceId={raceId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IEnumerable<AlertResponse>>();
    }
}

public record CreateAlertRequest(Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message);
public record AlertResponse(Guid Id, Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message, DateTime IssuedAt, bool IsAcknowledged);
