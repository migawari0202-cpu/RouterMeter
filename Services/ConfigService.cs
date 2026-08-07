using System.IO;
using System.Text.Json;
using RouterMeter.Models;

namespace RouterMeter.Services;

/// <summary>
/// config.json の読み込みを担当。存在しない場合は雛形を自動生成する。
/// 設定ファイルの読み書きを担当する。
/// </summary>
public class ConfigService
{
    private readonly string _configPath;

    public ConfigService(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(AppContext.BaseDirectory, "config.json");
    }

    /// <summary>
    /// 設定を読み込む。config.json が存在しない場合は雛形を書き出してから例外を投げる
    /// （APIキー未設定のまま起動させないため）。
    /// </summary>
    public AppConfig Load()
    {
        if (!File.Exists(_configPath))
        {
            var template = new AppConfig
            {
                ApiKey = "sk-or-xxxxxxxxxxxxxxxxxxxxxxxx",
                DailyBudget = 20.0,
                RefreshSeconds = 5
            };
            File.WriteAllText(_configPath, JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true }));

            throw new InvalidOperationException(
                $"config.json が見つからなかったため雛形を作成しました。\n{_configPath}\nAPIキーを設定して再起動してください。");
        }

        var json = File.ReadAllText(_configPath);
        var config = JsonSerializer.Deserialize<AppConfig>(json)
                     ?? throw new InvalidOperationException("config.json の形式が不正です。");

        if (string.IsNullOrWhiteSpace(config.ApiKey) || config.ApiKey.StartsWith("sk-or-xxxx"))
        {
            throw new InvalidOperationException("config.json に有効な ApiKey を設定してください。");
        }

        if (config.RefreshSeconds < 5)
        {
            // API負荷軽減のため下限を設ける
            config.RefreshSeconds = 5;
        }

        return config;
    }

    public void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }
}
