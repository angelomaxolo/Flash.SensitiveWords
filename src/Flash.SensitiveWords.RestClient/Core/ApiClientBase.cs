using System.Net.Http.Json;

namespace Flash.SensitiveWords.RestClient.Core;

public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;

    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await HttpClient.GetAsync(url);
        return await HandleResponse<T>(response);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request)
    {
        var response = await HttpClient.PostAsJsonAsync(url, request);
        return await HandleResponse<TResponse>(response);
    }

    protected async Task<T?> DeleteAsync<T>(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        return await HandleResponse<T>(response);
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await HttpClient.PutAsJsonAsync(url, request);

        return await HandleResponse<TResponse>(response);
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch
        {
            return default;
        }
    }
}