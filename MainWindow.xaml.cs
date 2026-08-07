using System.Windows;
using System.Windows.Input;
using RouterMeter.Models;
using RouterMeter.Services;
using RouterMeter.ViewModels;

namespace RouterMeter;

public partial class MainWindow : Window
{
    private OpenRouterApiService? _apiService;
    private ConfigService? _configService;
    private AppConfig? _config;
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += (_, _) => _apiService?.Dispose();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PositionBottomRight();

        try
        {
            _configService = new ConfigService();
            var config = _configService.Load();
            _config = config;
            _apiService = new OpenRouterApiService(config.ApiKey);
            var tracker = new DailyUsageTracker();

            _viewModel = new MainViewModel(_apiService, tracker, config);
            DataContext = _viewModel;

            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "RouterMeter - 設定エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
        }
    }

    private void PositionBottomRight()
    {
        const double margin = 16;
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - margin;
        Top = workArea.Bottom - Height - margin;
    }

    // タイトルバーが無いため、ウィンドウ本体のドラッグで移動できるようにする
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        AlwaysOnTopMenuItem.IsChecked = Topmost;
    }

    private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Topmost = AlwaysOnTopMenuItem.IsChecked;
    }

    private void ChangeBudgetMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_config is null || _configService is null || _viewModel is null) return;

        var dialog = new BudgetDialog(_config.DailyBudget) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        try
        {
            _config.DailyBudget = dialog.Budget;
            _configService.Save(_config);
            _viewModel.UpdateBudget(dialog.Budget);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"予算の保存に失敗しました。\n{ex.Message}", "保存エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();
}
