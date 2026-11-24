using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.ViewModel;
using System;
using System.Collections.Generic;
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
    public partial class SelectUserWindow : Window
    {
        public int? SelectedUserId { get; private set; }

        public SelectUserWindow()
        {
            InitializeComponent();

            var vm = new SelectUserViewModel();
            vm.RequestClose += ok =>
            {
                SelectedUserId = vm.SelectedUserId;
                DialogResult = ok;
            };
            DataContext = vm;
        }
    }
}
