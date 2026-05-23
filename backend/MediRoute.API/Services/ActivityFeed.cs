using MediRoute.API.DTOs;
using MediRoute.API.Models;

namespace MediRoute.API.Services;

/// <summary>In-memory ring buffer of recent admin activities per hospital (last 10).</summary>
public interface IActivityFeed
{
    void Record(int hospitalId, string resourceType, string description, string? updatedBy);
    IReadOnlyList<RecentActivityDto> GetRecent(int hospitalId, int count = 10);
}

public class ActivityFeed : IActivityFeed
{
    private readonly Dictionary<int, LinkedList<RecentActivityDto>> _store = new();
    private readonly object _lock = new();

    public void Record(int hospitalId, string resourceType, string description, string? updatedBy)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(hospitalId, out var list))
            {
                list = new LinkedList<RecentActivityDto>();
                _store[hospitalId] = list;
            }
            list.AddFirst(new RecentActivityDto(resourceType, description, DateTime.UtcNow, updatedBy));
            while (list.Count > 50) list.RemoveLast();
        }
    }

    public IReadOnlyList<RecentActivityDto> GetRecent(int hospitalId, int count = 10)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(hospitalId, out var list)) return Array.Empty<RecentActivityDto>();
            return list.Take(count).ToList();
        }
    }
}
