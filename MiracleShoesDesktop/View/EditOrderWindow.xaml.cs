using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
    /// Логика взаимодействия для EditOrderWindow.xaml
    /// </summary>
    public partial class EditOrderWindow : Window
    {
        public EditOrderWindow(Order order)
        {
            InitializeComponent();

            var vm = new EditOrderViewModel(order);
            vm.RequestClose += ok => DialogResult = ok;
            DataContext = vm;
        }
    }
}
