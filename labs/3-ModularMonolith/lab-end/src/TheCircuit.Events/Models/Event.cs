using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Events.Models;

internal class Event
{
    public int Id { get; set; }
    public required string EventName { get; set; }
    public DateTime ScheduledDate { get; set; }
    public required string VenueId { get; set; }
    public EventStatus Status { get; set; }
}
