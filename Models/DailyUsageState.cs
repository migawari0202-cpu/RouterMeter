namespace RouterMeter.Models;

/// <summary>
/// daily_state.json に保存する内容。
/// OpenRouterのAPIは「累計usage」しか返さず、当日分だけの値は提供されないため、
/// 日付が変わった時点のusageをベースラインとして保持し、差分で「本日の消費」を算出する。
/// </summary>
public class DailyUsageState
{
    /// <summary>基準日（ローカル日付, yyyy-MM-dd）。</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>その日の開始時点での累計usage。</summary>
    public double BaselineUsage { get; set; }

    /// <summary>直近ポーリングで観測した累計usage（増分検知用）。</summary>
    public double LastSeenUsage { get; set; }

    /// <summary>
    /// 本日のリクエスト回数の近似値。
    /// APIはリクエスト件数を返さないため、「ポーリング間でusageが増えた回数」を代用値として数える。
    /// 短時間に複数リクエストが発生した場合は実際より少なく数えられる点に注意。
    /// </summary>
    public int RequestCountApprox { get; set; }
}
