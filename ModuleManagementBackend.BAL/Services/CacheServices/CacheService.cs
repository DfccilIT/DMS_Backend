using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ModuleManagementBackend.BAL.IServices.ICacheServices;

namespace ModuleManagementBackend.BAL.Services.CacheServices
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new HashSet<string>();
        private readonly object _lock = new object();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
        {
            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache miss for key: {Key}", key);
            var item = await getItem();

            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (slidingExpiration.HasValue)
                cacheEntryOptions.SlidingExpiration = slidingExpiration;
            else
                cacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(30); // Default

            if (absoluteExpiration.HasValue)
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration;

           
            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, value, reason, state) =>
            {
                lock (_lock)
                {
                    var keyString = evictedKey?.ToString();
                    if (!string.IsNullOrEmpty(keyString))
                    {
                        _cacheKeys.Remove(keyString);
                    }
                }
            });


            _memoryCache.Set(key, item, cacheEntryOptions);
            return item;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            lock (_lock)
            {
                var keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }
            }
            return Task.CompletedTask;
        }
    }

}
