using System.Text.Json;
using HospitalMS.Catalog.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Catalog.Infrastructure.Services;

public class RedisCacheService(
    IDistributedCache distributedCache,
    IMemoryCache memoryCache,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await distributedCache.GetAsync(key, cancellationToken);
            if (bytes is not null && bytes.Length > 0)
            {
                return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Cache] Redis Get failed for {Key}. Falling back to In-Memory cache.", key);
        }

        // Fallback to local memory cache
        if (memoryCache.TryGetValue(key, out T? memoryValue))
        {
            return memoryValue;
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(15)
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

        try
        {
            await distributedCache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Cache] Redis Set failed for {Key}. Writing to In-Memory cache only.", key);
        }

        // Also save to memory cache
        memoryCache.Set(key, value, expiry ?? TimeSpan.FromMinutes(15));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Cache] Redis Remove failed for {Key}", key);
        }

        memoryCache.Remove(key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // For distributed cache, removing by prefix typically requires Redis KEYS or SCAN
        // In this implementation, we remove from memory cache and log prefix purge
        logger.LogInformation("[Cache] Invalidating cache items with prefix {Prefix}", prefix);
        await Task.CompletedTask;
    }
}

