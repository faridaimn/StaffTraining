using System.Net.Http.Json;
using Testing.Components.Model;

namespace Testing.Components.Data;

public class EsiarService
{
    private readonly HttpClient _httpClient;

    public EsiarService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<GetParameterResponse?> GetParameterAsync(GetParameterRequest request)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        // REST URL untuk GetParameter
        string url = "http://100.100.100.68:8082/EsiarService.svc/rest/GetParameter";

        var response = await _httpClient.PostAsJsonAsync(url, request, options);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GetParameterResponse>(options); 
    }


    public async Task<EsiarResponse?> GetTopRecordsAsync(EsiarRequest request)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Disable camelCase serialization
            PropertyNameCaseInsensitive = true
        };

        var response = await _httpClient.PostAsJsonAsync("http://100.100.100.68:8082/EsiarService.svc/rest/RetrieveTop10Records", request, options);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EsiarResponse>(options);
    }

    public async Task<bool> CheckPdfExistsAsync(string pdfUrl)
    {
        try
        {
            // Gunakan HEAD request untuk check tanpa download full content
            var request = new HttpRequestMessage(HttpMethod.Head, pdfUrl);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
