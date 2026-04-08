using HospitalMS.BL.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalMS.BL.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan DoctorListExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan SpecializationsExpiration = TimeSpan.FromHours(1);
    private static readonly TimeSpan MedicalHistoryExpiration = TimeSpan.FromMinutes(15);
    private const string DoctorListKey = "doctors:list";
    private const string DoctorByIdKeyPrefix = "doctors:id:";
    private const string SpecializationsKey = "doctors:specializations";
    private const string MedicalHistoryKeyPrefix = "patients:medical-history:";
    public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    #region Generic Cache Operations
    // get cached value
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            if (cachedData == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached data for key: {Key}", key);
            return default;
        }
    }

    // set cached value
    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (absoluteExpireTime.HasValue)
                options.AbsoluteExpirationRelativeToNow = absoluteExpireTime;
            else
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
            if (slidingExpireTime.HasValue)
                options.SlidingExpiration = slidingExpireTime;
            var jsonData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached data for key: {Key}", key);
        }
    }

    // remove cached key
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached data for key: {Key}", key);
        }
    }

    // get or create cache
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cachedData = await GetAsync<T>(key);
        if (cachedData != null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedData;
        }
        _logger.LogDebug("Cache miss for key: {Key}", key);
        var data = await factory();
        await SetAsync(key, data, expiration);
        return data;
    }
    #endregion
    #region Doctor Caching
    // get doctor list cache
    public Task<T?> GetDoctorListAsync<T>()
    {
        return GetAsync<T>(DoctorListKey);
    }

    // set doctor list cache
    public Task SetDoctorListAsync<T>(T doctors)
    {
        return SetAsync(DoctorListKey, doctors, DoctorListExpiration);
    }

    // invalidate doctor list
    public Task InvalidateDoctorListAsync()
    {
        return RemoveAsync(DoctorListKey);
    }

    // get doctor by id cache
    public Task<T?> GetDoctorByIdAsync<T>(int doctorId)
    {
        return GetAsync<T>($"{DoctorByIdKeyPrefix}{doctorId}");
    }

    // set doctor by id cache
    public Task SetDoctorByIdAsync<T>(int doctorId, T doctor)
    {
        return SetAsync($"{DoctorByIdKeyPrefix}{doctorId}", doctor, DoctorListExpiration);
    }

    // invalidate doctor by id
    public Task InvalidateDoctorByIdAsync(int doctorId)
    {
        return RemoveAsync($"{DoctorByIdKeyPrefix}{doctorId}");
    }
    #endregion
    #region Specializations Caching
    // get specializations cache
    public Task<T?> GetSpecializationsAsync<T>()
    {
        return GetAsync<T>(SpecializationsKey);
    }

    // set specializations cache
    public Task SetSpecializationsAsync<T>(T specializations)
    {
        return SetAsync(SpecializationsKey, specializations, SpecializationsExpiration);
    }

    // invalidate specializations
    public Task InvalidateSpecializationsAsync()
    {
        return RemoveAsync(SpecializationsKey);
    }
    #endregion
    #region Medical History Caching
    // get medical history cache
    public Task<T?> GetMedicalHistoryAsync<T>(int patientId)
    {
        return GetAsync<T>($"{MedicalHistoryKeyPrefix}{patientId}");
    }

    // set medical history cache
    public Task SetMedicalHistoryAsync<T>(int patientId, T medicalHistory)
    {
        return SetAsync($"{MedicalHistoryKeyPrefix}{patientId}", medicalHistory, MedicalHistoryExpiration);
    }

    // invalidate medical history
    public Task InvalidateMedicalHistoryAsync(int patientId)
    {
        return RemoveAsync($"{MedicalHistoryKeyPrefix}{patientId}");
    }
    #endregion
    #region Bulk Operations
    // invalidate all doctor caches
    public async Task InvalidateAllDoctorCachesAsync()
    {
        await InvalidateDoctorListAsync();
        await InvalidateSpecializationsAsync();
        _logger.LogInformation("Invalidated all doctor-related caches");
    }
    #endregion
}