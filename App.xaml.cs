using System.Windows;

namespace RouterMeter;

public partial class App : Application
{
    // 現状は特別な起動処理は不要（MainWindow.xaml.cs 側で初期化する）。
    // 将来タスクトレイ対応する際は、ここで NotifyIcon の生成/破棄を行う想定。
}
