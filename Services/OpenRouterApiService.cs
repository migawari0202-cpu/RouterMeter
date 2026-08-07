using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RouterMeter.Models;

namespace RouterMeter.Services;

/// <summary>
/// 通常のAPIキー（sk-or-...）で利用できる GET /api/v1/auth/key のみを使用する。
/// この1本で label / usage(累計消費額, USD) / limit / is_free_tier が取得できる。
///
/// 注意: /api/v1/credits と /api/v1/activity は Provisioning key（管理キー）専用のため使用しない。
/// また /api/v1/activity は「完了したUTC日」しか返さず当日分は含まれないため、
/// そもそも「本日の消費」を得る用途には使えない。
/// </summary>
public class OpenRouterApiService : IDisposable
{
    private readonly HttpClient _httpClient;

    public OpenRouterApiService(string apiKey)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<OpenRouterKeyInfo> GetKeyInfoAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("auth/key", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenRouterKeyResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("OpenRouter APIのレスポンスが空です。");

        return body.Data;
    }

    public void Dispose() => _httpClient.Dispose();
}