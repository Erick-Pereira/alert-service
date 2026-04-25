using Simcag.AlertService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Cache;

public class RedisAlertDeduplicationCache : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisAlertDeduplicationCache> _logger;

    public RedisAlertDeduplicationCache(
        IDistributedCache cache,
        ILogger<RedisAlertDeduplicationCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsDuplicateAlertAsync(
        string entityType,
        string entityId,
        TimeSpan cooldown,
        CancellationToken ct)
    {
        var key = GenerateKey(entityType, entityId);

        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            var isDuplicate = !string.IsNullOrEmpty(cached);

            if (isDuplicate)
            {
                _logger.LogDebug(
                    "Duplicate alert detected for {EntityType}:{EntityId}",
                    entityType,
                    entityId);
            }

            return isDuplicate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Redis unavailable, skipping deduplication check for " +
                "{EntityType}:{EntityId}",
                entityType,
                entityId);
            return false; // Fail open: allow alert if Redis down
        }
    }

    public async Task MarkAlertAsSentAsync(
        string entityType,
        string entityId,
        TimeSpan ttl,
        CancellationToken ct)
    {
        var key = GenerateKey(entityType, entityId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            SlidingExpiration = TimeSpan.FromHours(12)
        };

        try
        {
            await _cache.SetStringAsync(
                key,
                "sent",
                options,
                ct);

            _logger.LogDebug(
                "Marked alert as sent for {EntityType}:{EntityId} " +
                "(TTL: {TTL})",
                entityType,
                entityId,
                ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Redis unavailable, skipping deduplication mark for " +
                "{EntityType}:{EntityId}",
                entityType,
                entityId);
        }
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken ct) where T : class
    {
        try
        {
            var cached = await _cache.GetStringAsync(key, ct);
            if (string.IsNullOrEmpty(cached)) return null;

            return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get cached value for key {Key}",
                key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken ct) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetStringAsync(key, serialized, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to set cached value for key {Key}",
                key);
        }
    }

    private static string GenerateKey(string entityType, string entityId) =>
        $"alert:dedup:{entityType.ToLower()}:{entityId.ToLower()}";
}
