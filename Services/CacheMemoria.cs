
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Runtime.Remoting;
using System.Text.Json;

namespace Services;

public class CacheMemoria {
    public IDistributedCache Cache { get; set; }

    public CacheMemoria(IDistributedCache cache)
    {
        Cache = cache;
    }

    public async Task<T?> Get<T>(string key) where T : class
    {
        var value = await Cache.GetStringAsync(key);
        if (value == null)
        {
            return null;
        }
        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task<bool> Exist(string key) {
        var obj = await Cache.GetAsync(key);

        return obj!= null;
    }

    public async Task Set<T>(string key, T value, TimeSpan? timeToLive = null) where T : class
    {
        var options = new DistributedCacheEntryOptions();
        
        options.AbsoluteExpirationRelativeToNow = timeToLive ?? TimeSpan.FromSeconds(60);
        await Cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }

    public async Task Remove(string key)
    {
        await Cache.RemoveAsync(key);
    }
}