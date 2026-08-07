using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RouterMeter.Controls;

/// <summary>
/// シンプルな円グラフ（リング型）を描画するコントロール。
/// 色分けや円弧の座標計算はこのコントロール固有の描画ロジックであり、
/// アプリケーションの業務ロジック（ViewModel側）とは切り離している。
/// </summary>
public partial class PieChartControl : UserControl
{
    public static readonly DependencyProperty PercentageProperty =
        DependencyProperty.Register(
            nameof(Percentage),
            typeof(double),
            typeof(PieChartControl),
            new PropertyMetadata(0.0, OnPercentageChanged));

    public double Percentage
    {
        get => (double)GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }

    private const double Size = 90;
    private const double StrokeThickness = 10;
    private const double Radius = (Size - StrokeThickness) / 2;
    private static readonly Point Center = new(Size / 2, Size / 2);

    public PieChartControl()
    {
        InitializeComponent();
        Loaded += (_, _) => Draw();
    }

    private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((PieChartControl)d).Draw();

    private void Draw()
    {
        if (ChartCanvas == null) return;
        ChartCanvas.Children.Clear();

        // 背景トラック（未消費部分）
        ChartCanvas.Children.Add(new Ellipse
        {
            Width = Size - StrokeThickness,
            Height = Size - StrokeThickness,
            Stroke = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A)),
            StrokeThickness = StrokeThickness,
            Margin = new Thickness(StrokeThickness / 2)
        });

        var clamped = Math.Max(0, Math.Min(Percentage, 100));
        var brush = GetBrushFor(Percentage);

        if (clamped >= 99.95)
        {
            // ほぼ/完全に100%の場合は円弧の退化(始点=終点)を避けてフル円を描く
            ChartCanvas.Children.Add(new Ellipse
            {
                Width = Size - StrokeThickness,
                Height = Size - StrokeThickness,
                Stroke = brush,
                StrokeThickness = StrokeThickness,
                Margin = new Thickness(StrokeThickness / 2)
            });
        }
        else if (clamped > 0)
        {
            var path = new Path { Stroke = brush, StrokeThickness = StrokeThickness, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            path.Data = BuildArcGeometry(clamped);
            ChartCanvas.Children.Add(path);
        }

        PercentageLabel.Text = $"{Percentage:0}%";
        PercentageLabel.Foreground = brush;
    }

    private static Geometry BuildArcGeometry(double percentage)
    {
        const double startAngleDeg = -90; // 12時の位置から開始
        var sweepAngleDeg = percentage / 100.0 * 360.0;

        var startPoint = PointOnCircle(startAngleDeg);
        var endPoint = PointOnCircle(startAngleDeg + sweepAngleDeg);
        var isLargeArc = sweepAngleDeg > 180;

        var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
        figure.Segments.Add(new ArcSegment(
            endPoint,
            new Size(Radius, Radius),
            0,
            isLargeArc,
            SweepDirection.Clockwise,
            true));

        return new PathGeometry(new[] { figure });
    }

    private static Point PointOnCircle(double angleDegrees)
    {
        var angleRad = angleDegrees * Math.PI / 180.0;
        return new Point(
            Center.X + Radius * Math.Cos(angleRad),
            Center.Y + Radius * Math.Sin(angleRad));
    }

    private static SolidColorBrush GetBrushFor(double percentage) => percentage switch
    {
        >= 90 => new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)), // 赤
        >= 70 => new SolidColorBrush(Color.FromRgb(0xFF, 0xA7, 0x26)), // 橙
        _ => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))      // 緑
    };
}
