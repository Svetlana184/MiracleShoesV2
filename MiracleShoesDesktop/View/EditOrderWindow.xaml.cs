using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
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
        private readonly Order _order;
        private readonly PostgresContext _context = new();

        public DateTime? OrderDate { get; set; }
        public ObservableCollection<OrderItem> Items { get; }

        public EditOrderWindow(Order order)
        {
            InitializeComponent();

            _order = order;
            OrderDate = order.DateOrder.ToDateTime(TimeOnly.MinValue);

            // перечитываем заказ с навигациями из нового контекста
            var entity = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.IdProductNavigation)
                .Include(o => o.OrderItems).ThenInclude(i => i.IdSizeNavigation)
                .First(o => o.IdOrder == order.IdOrder);

            Items = new ObservableCollection<OrderItem>(entity.OrderItems);

            ClientCombo.ItemsSource = _context.Users.OrderBy(u => u.Surname).ToList();
            ClientCombo.SelectedValuePath = "IdUser";
            ClientCombo.SelectedValue = entity.IdUser;

            DataContext = this;
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: OrderItem item })
            {
                // вернуть остаток сразу
                if (item.IdSize.HasValue)
                {
                    var avail = _context.AvailableSizes
                        .FirstOrDefault(a => a.IdProduct == item.IdProduct && a.IdSize == item.IdSize);
                    if (avail != null)
                    {
                        avail.Amount += item.Amount;
                        avail.IsAvailable = true;
                    }
                }
                Items.Remove(item);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (OrderDate == null)
            {
                MessageBox.Show("Укажите дату заказа.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientCombo.SelectedValue is not int clientId)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entity = _context.Orders
                .Include(o => o.OrderItems)
                .First(o => o.IdOrder == _order.IdOrder);

            entity.DateOrder = DateOnly.FromDateTime(OrderDate.Value);
            entity.IdUser = clientId;

            // синхронизация позиций: удалённые уже вернули остаток в RemoveItem_Click;
            // обновляем amount/cost_for_item для оставшихся
            foreach (var uiItem in Items)
            {
                var dbItem = entity.OrderItems.FirstOrDefault(i => i.IdOrderItem == uiItem.IdOrderItem);
                if (dbItem == null) continue;

                if (dbItem.Amount != uiItem.Amount && dbItem.IdSize.HasValue)
                {
                    var diff = uiItem.Amount - dbItem.Amount;
                    var avail = _context.AvailableSizes
                        .FirstOrDefault(a => a.IdProduct == dbItem.IdProduct && a.IdSize == dbItem.IdSize);
                    if (avail != null)
                    {
                        avail.Amount -= diff;
                        if (avail.Amount < 0) avail.Amount = 0;
                        avail.IsAvailable = avail.Amount > 0;
                    }
                }

                dbItem.Amount = uiItem.Amount;
                dbItem.CostForItem = uiItem.CostForItem;
            }

            _context.SaveChanges();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
