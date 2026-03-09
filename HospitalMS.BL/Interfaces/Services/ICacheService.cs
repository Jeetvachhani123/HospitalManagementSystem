using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalMS.BL.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null);

    Task RemoveAsync(string key);
}