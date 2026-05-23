using MediRoute.API.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace MediRoute.API.Hubs;

/// <summary>
/// Real-time resource hub. Clients (patients) join a group per hospital they're viewing.
/// Admin actions broadcast `AvailabilityUpdated` to that group.
/// </summary>
public class ResourceHub : Hub
{
    private static string GroupName(string hospitalId) => $"hospital-{hospitalId}";

    /// <summary>Join the SignalR group for a specific hospital to receive live updates.</summary>
    public Task JoinHospitalGroup(string hospitalId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(hospitalId));

    /// <summary>Leave the SignalR group for a specific hospital.</summary>
    public Task LeaveHospitalGroup(string hospitalId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(hospitalId));

    /// <summary>
    /// Helper invoked from server-side services to broadcast updates.
    /// (Not normally called by clients but kept here to match the spec.)
    /// </summary>
    public Task BroadcastAvailabilityUpdate(string hospitalId, AvailabilityDto availability) =>
        Clients.Group(GroupName(hospitalId))
            .SendAsync("AvailabilityUpdated", new
            {
                HospitalId = hospitalId,
                Availability = availability,
                Timestamp = DateTime.UtcNow
            });
}

/// <summary>Strongly-typed broadcaster used by services to push updates without depending on Hub directly.</summary>
public interface IResourceBroadcaster
{
    Task BroadcastAsync(int hospitalId, string resourceType, AvailabilityDto availability, CancellationToken ct = default);
}

public class ResourceBroadcaster : IResourceBroadcaster
{
    private readonly IHubContext<ResourceHub> _hub;
    public ResourceBroadcaster(IHubContext<ResourceHub> hub) { _hub = hub; }

    public Task BroadcastAsync(int hospitalId, string resourceType, AvailabilityDto availability, CancellationToken ct = default) =>
        _hub.Clients.Group($"hospital-{hospitalId}").SendAsync("AvailabilityUpdated", new
        {
            HospitalId = hospitalId,
            UpdatedResourceType = resourceType,
            Availability = availability,
            Timestamp = DateTime.UtcNow
        }, ct);
}
