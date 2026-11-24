using MiracleShoesDesktop.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiracleShoesDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для SelectSizeWindow.xaml
    /// </summary>
    public partial class SelectSizeWindow : Window
    {
        public string ProductName { get; }
        public int? SelectedSizeId { get; private set; }
        public int SelectedAmount { get; private set; }

        private readonly List<SizeOption> _sizes;

        public SelectSizeWindow(string productName, List<SizeOption> sizes)
        {
            InitializeComponent();
            ProductName = productName;
            _sizes = sizes;

            SizeCombo.ItemsSource = _sizes;
            if (_sizes.Count > 0)
                SizeCombo.SelectedIndex = 0;

            DataContext = this;
        }

        public SizeOption? CurrentSize => SizeCombo.SelectedItem as SizeOption;

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAvailable();
            ClampAmount();
        }

        private void UpdateAvailable()
        {
            AvailableText.Text = CurrentSize != null
                ? $"Доступно: {CurrentSize.Amount}"
                : string.Empty;
        }

        private void ClampAmount()
        {
            if (!int.TryParse(AmountBox.Text, out var n) || n < 1)
                n = 1;
            if (CurrentSize != null && n > CurrentSize.Amount)
                n = CurrentSize.Amount;
            AmountBox.Text = n.ToString();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AmountBox.Text, out var n) && n > 1)
                AmountBox.Text = (n - 1).ToString();
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AmountBox.Text, out var n)) n = 1;
            if (CurrentSize == null || n < CurrentSize.Amount)
                AmountBox.Text = (n + 1).ToString();
        }

        private void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSize == null)
            {
                MessageBox.Show("Выберите размер.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AmountBox.Text, out var n) || n < 1)
            {
                MessageBox.Show("Укажите количество (целое число > 0).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (n > CurrentSize.Amount)
            {
                MessageBox.Show($"Доступно только {CurrentSize.Amount} шт.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedSizeId = CurrentSize.IdSize;
            SelectedAmount = n;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
