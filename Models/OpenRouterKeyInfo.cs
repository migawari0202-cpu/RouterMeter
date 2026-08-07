using System.Text.Json.Serialization;

namespace RouterMeter.Models;

/// <summary>
/// GET https://openrouter.ai/api/v1/auth/key のレスポンス全体。
/// 通常のAPIキー（sk-or-...）でそのまま利用可能。
/// ※ /api/v1/credits や /api/v1/activity は Provisioning key（管理キー）専用のため、
///    このアプリでは使用しない。
/// </summary>
public class OpenRouterKeyResponse
{
    [JsonPropertyName("data")]
    public OpenRouterKeyInfo Data { get; set; } = new();
}

public class OpenRouterKeyInfo
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>このキーで累計消費した金額（USD）。OpenRouter側で計算済みの値。</summary>
    [JsonPropertyName("usage")]
    public double Usage { get; set; }

    /// <summary>キーに設定された上限額（USD）。未設定の場合はnull。</summary>
    [JsonPropertyName("limit")]
    public double? Limit { get; set; }

    [JsonPropertyName("is_free_tier")]
    public bool IsFreeTier { get; set; }
}
