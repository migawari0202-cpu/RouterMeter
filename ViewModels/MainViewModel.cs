using System.Windows.Threading;
using RouterMeter.Models;
using RouterMeter.Services;

namespace RouterMeter.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly OpenRouterApiService _apiService;
    private readonly DailyUsageTracker _tracker;
    private readonly AppConfig _config;
    private readonly DispatcherTimer _timer;

    public MainViewModel(OpenRouterApiService apiService, DailyUsageTracker tracker, AppConfig config)
    {
        _apiService = apiService;
        _tracker = tracker;
        _config = config;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(config.RefreshSeconds)
        };
        _timer.Tick += async (_, _) => await RefreshAsync();
    }

    private double _percentage;
    /// <summary>円グラフ用の割合（0〜100。予算超過時は100超になることもある）。</summary>
    public double Percentage
    {
        get => _percentage;
        private set => SetProperty(ref _percentage, value);
    }

    private string _percentageText = "--%";
    public string PercentageText
    {
        get => _percentageText;
        private set => SetProperty(ref _percentageText, value);
    }

    private double _todaySpend;

    private string _spendText = "$0.00 / $0.00";
    public string SpendText
    {
        get => _spendText;
        private set => SetProperty(ref _spendText, value);
    }

    private string _requestsText = "Today Requests: --";
    public string RequestsText
    {
        get => _requestsText;
        private set => SetProperty(ref _requestsText, value);
    }

    private string _lastUpdateText = "Last Update: --:--:--";
    public string LastUpdateText
    {
        get => _lastUpdateText;
        private set => SetProperty(ref _lastUpdateText, value);
    }

    private ApiConnectionStatus _status = ApiConnectionStatus.Connecting;
    public ApiConnectionStatus Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        _timer.Start();
    }

    public void UpdateBudget(double budget)
    {
        _config.DailyBudget = budget;
        var rawPercentage = budget > 0 ? _todaySpend / budget * 100.0 : 0;
        Percentage = rawPercentage;
        PercentageText = $"{rawPercentage:0}%";
        SpendText = $"${_todaySpend:0.00} / ${budget:0.00}";
    }

    private async Task RefreshAsync()
    {
        try
        {
            var keyInfo = await _apiService.GetKeyInfoAsync();
            var today = _tracker.UpdateAndGetTodayUsage(keyInfo.Usage);

            _todaySpend = today.Spend;
            var rawPercentage = _config.DailyBudget > 0
                ? _todaySpend / _config.DailyBudget * 100.0
                : 0;

            Percentage = rawPercentage;
            PercentageText = $"{rawPercentage:0}%";
            SpendText = $"${_todaySpend:0.00} / ${_config.DailyBudget:0.00}";
            RequestsText = $"Today Requests: {today.RequestCountApprox}";
            LastUpdateText = $"Last Update: {DateTime.Now:HH:mm:ss}";
            Status = ApiConnectionStatus.Online;
        }
        catch
        {
            // 取得失敗時は最終取得値を保持したまま Offline 表示のみ切り替える。
            // 再接続は次回のタイマーTickで自動的に試行される。
            Status = ApiConnectionStatus.Offline;
        }
    }
}
