using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ModuleManagementBackend.BAL.IServices.ICacheServices;

namespace ModuleManagementBackend.BAL.Services.CacheServices
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _lock = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> getItem,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
            
            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                
                _logger.LogInformation("Cache HIT for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache MISS for key: {Key}", key);
            var item = await getItem();

            await SetAsync(key, item, slidingExpiration, absoluteExpiration);
            return item;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                _logger.LogInformation("Cache HIT for key: {Key}", key);
                return Task.FromResult((T?)value);
            }

            _logger.LogWarning("Cache MISS for key: {Key}", key);
            return Task.FromResult(default(T));
        }

        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(30)
            };

            if (absoluteExpiration.HasValue)
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration;

            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, valueObj, reason, state) =>
            {
                lock (_lock)
                {
                    var keyString = evictedKey?.ToString();
                    if (!string.IsNullOrEmpty(keyString))
                    {
                        _cacheKeys.Remove(keyString);
                        _logger.LogWarning("Cache EVICTED. Key: {Key}, Reason: {Reason}", keyString, reason);
                    }
                }
            });

            _memoryCache.Set(key, value, cacheEntryOptions);
            _logger.LogInformation("Cache SET for key: {Key}, Expiry: {Expiry}", key, cacheEntryOptions.AbsoluteExpirationRelativeToNow);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }
            _logger.LogInformation("Cache REMOVED manually. Key: {Key}", key);
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
                    _logger.LogInformation("Cache REMOVED by pattern. Key: {Key}, Pattern: {Pattern}", key, pattern);
                }
            }
            return Task.CompletedTask;
        }
    }
}
