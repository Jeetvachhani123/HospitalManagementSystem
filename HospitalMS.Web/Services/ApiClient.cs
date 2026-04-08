using HospitalMS.BL.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HospitalMS.Web.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{endpoint}", cancellationToken);
            
            return await HandleResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
           
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PatchAsync<TResponse>(string endpoint, HttpContent content, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request, cancellationToken);
           
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PATCH request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrEmpty(content))
                return default;
            
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(content, jsonOptions);
        }
        _logger.LogWarning("API request failed with status {StatusCode}: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"API request failed with status {response.StatusCode}");
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor?.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}