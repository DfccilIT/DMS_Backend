using System;
using System.Threading.Tasks;

namespace ModuleManagementBackend.BAL.IServices.ICacheServices
{
    public interface ICacheService
    {
       
        Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> getItem,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null);

        
        Task<T?> GetAsync<T>(string key);

       
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null);

        
        Task RemoveAsync(string key);

       
        Task RemoveByPatternAsync(string pattern);
    }
}
