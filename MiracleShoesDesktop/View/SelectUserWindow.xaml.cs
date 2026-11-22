using MiracleShoesDesktop.Model;
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
    /// <summary>
    /// Логика взаимодействия для SelectUserWindow.xaml
    /// </summary>
    public partial class SelectUserWindow : Window
    {
        public int? SelectedUserId { get; private set; }

        public SelectUserWindow()
        {
            InitializeComponent();
            using var ctx = new PostgresContext();
            UserCombo.ItemsSource = ctx.Users.OrderBy(u => u.Surname).ToList();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (UserCombo.SelectedItem is User u)
            {
                SelectedUserId = u.IdUser;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
