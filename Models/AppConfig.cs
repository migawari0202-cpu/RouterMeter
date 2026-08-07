namespace RouterMeter.Models;

/// <summary>
/// config.json の内容をそのまま表すモデル。
/// </summary>
public class AppConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public double DailyBudget { get; set; } = 20.0;
    public int RefreshSeconds { get; set; } = 5;
}
