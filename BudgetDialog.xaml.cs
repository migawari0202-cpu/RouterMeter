using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace RouterMeter;

public partial class BudgetDialog : Window
{
    public double Budget { get; private set; }

    public BudgetDialog(double currentBudget)
    {
        InitializeComponent();
        BudgetTextBox.Text = currentBudget.ToString("0.00", CultureInfo.CurrentCulture);
        Loaded += (_, _) =>
        {
            BudgetTextBox.Focus();
            BudgetTextBox.SelectAll();
        };
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(BudgetTextBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var budget)
            || !double.IsFinite(budget) || budget <= 0)
        {
            MessageBox.Show(this, "0より大きい数値を入力してください。", "入力エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            BudgetTextBox.SelectAll();
            BudgetTextBox.Focus();
            return;
        }

        Budget = budget;
        DialogResult = true;
    }

    private void BudgetTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SaveButton_Click(sender, e);
            e.Handled = true;
        }
    }
}
