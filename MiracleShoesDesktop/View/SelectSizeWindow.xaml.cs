using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.ViewModel;
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
        private void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        => e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");

        public int? SelectedSizeId { get; private set; }
        public int SelectedAmount { get; private set; }
        public SizeOption? CurrentSize { get; private set; }

        public SelectSizeWindow(string productName, List<SizeOption> sizes)
        {
            InitializeComponent();

            var vm = new SelectSizeViewModel(productName, sizes);
            vm.RequestClose += ok =>
            {
                SelectedSizeId = vm.SelectedSizeId;
                SelectedAmount = vm.SelectedAmount;
                CurrentSize = vm.SelectedSize;
                DialogResult = ok;
            };
            DataContext = vm;
        }
    }
}
