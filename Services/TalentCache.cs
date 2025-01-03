using System.Text.Json;
using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services;

public class TalentCache
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2);
    private readonly string _cacheFilePath;

    public TalentCache(string cacheFilePath)
    {
        _cacheFilePath = cacheFilePath;
    }

    public bool TryGetCachedTalents(out List<Talent> talents)
    {
        talents = null;

        if (!File.Exists(_cacheFilePath))
        {
            return false;
        }

        var cacheContent = File.ReadAllText(_cacheFilePath);
        var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(cacheContent);

        if (cacheEntry == null || DateTime.UtcNow - cacheEntry.Timestamp > _cacheDuration)
        {
            return false;
        }

        talents = cacheEntry.Talents;
        return true;
    }

    public void SaveTalentsToCache(List<Talent> talents)
    {
        var cacheEntry = new CacheEntry
        {
            Talents = talents,
            Timestamp = DateTime.UtcNow
        };

        var cacheContent = JsonSerializer.Serialize(cacheEntry);
        File.WriteAllText(_cacheFilePath, cacheContent);
    }

    private class CacheEntry
    {
        public List<Talent> Talents { get; set; }
        public DateTime Timestamp { get; set; }
    }
}