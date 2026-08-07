using System.IO;
using System.Text.Json;
using RouterMeter.Models;

namespace RouterMeter.Services;

public record TodayUsage(double Spend, int RequestCountApprox);

/// <summary>
/// OpenRouter APIが返すのは「キーの累計usage」のみ。
/// このクラスはローカル日付が変わるたびに基準値(baseline)を取り直し、
/// 差分から「本日の消費額」を疑似的に算出して daily_state.json に永続化する。
/// </summary>
public class DailyUsageTracker
{
    private const double Epsilon = 0.0001;
    private readonly string _statePath;

    public DailyUsageTracker(string? statePath = null)
    {
        _statePath = statePath ?? Path.Combine(AppContext.BaseDirectory, "daily_state.json");
    }

    public TodayUsage UpdateAndGetTodayUsage(double currentTotalUsage)
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var state = LoadState();

        if (state.Date != today)
        {
            // 日付が変わった（または初回起動）＝ここを新しい基準点にする
            state = new DailyUsageState
            {
                Date = today,
                BaselineUsage = currentTotalUsage,
                LastSeenUsage = currentTotalUsage,
                RequestCountApprox = 0
            };
        }
        else if (currentTotalUsage > state.LastSeenUsage + Epsilon)
        {
            // 前回ポーリング以降にusageが増えていた＝何らかのリクエストが発生したとみなす
            state.RequestCountApprox++;
            state.LastSeenUsage = currentTotalUsage;
        }

        SaveState(state);

        var spend = Math.Max(0, currentTotalUsage - state.BaselineUsage);
        return new TodayUsage(spend, state.RequestCountApprox);
    }

    private DailyUsageState LoadState()
    {
        if (!File.Exists(_statePath))
        {
            return new DailyUsageState();
        }

        try
        {
            var json = File.ReadAllText(_statePath);
            return JsonSerializer.Deserialize<DailyUsageState>(json) ?? new DailyUsageState();
        }
        catch
        {
            // 壊れたstateファイルは無視して作り直す
            return new DailyUsageState();
        }
    }

    private void SaveState(DailyUsageState state)
    {
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_statePath, json);
    }
}
