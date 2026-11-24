using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class EditOrderViewModel : ObservableObject
    {
        private readonly PostgresContext _context = new();
        private readonly int _orderId;

        [ObservableProperty] private DateTime? _orderDate;
        [ObservableProperty] private User? _selectedClient;
        [ObservableProperty] private ObservableCollection<User> _clients = new();
        [ObservableProperty] private ObservableCollection<OrderItem> _items = new();

        public event Action<bool>? RequestClose;

        public EditOrderViewModel(Order order)
        {
            _orderId = order.IdOrder;

            var entity = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.IdProductNavigation)
                .Include(o => o.OrderItems).ThenInclude(i => i.IdSizeNavigation)
                .First(o => o.IdOrder == order.IdOrder);

            OrderDate = entity.DateOrder.ToDateTime(TimeOnly.MinValue);
            Clients = new ObservableCollection<User>(
                _context.Users.AsNoTracking().OrderBy(u => u.Surname).ToList());
            SelectedClient = Clients.FirstOrDefault(c => c.IdUser == entity.IdUser);
            Items = new ObservableCollection<OrderItem>(entity.OrderItems);
        }

        [RelayCommand]
        private void RemoveItem(OrderItem? item)
        {
            if (item == null) return;

            _context.AvailableSizes
                .FirstOrDefault(a => a.IdProduct == item.IdProduct && a.IdSize == item.IdSize)
                ?.Add(item.Amount);

            Items.Remove(item);
        }

        [RelayCommand]
        private void Save()
        {
            if (OrderDate == null)
            {
                MessageBox.Show("Укажите дату заказа.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedClient == null)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entity = _context.Orders
                .Include(o => o.OrderItems)
                .First(o => o.IdOrder == _orderId);

            entity.DateOrder = DateOnly.FromDateTime(OrderDate.Value);
            entity.IdUser = SelectedClient.IdUser;

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
                        if (diff > 0) avail.Take(diff);
                        else avail.Add(-diff);
                    }
                }

                dbItem.Amount = uiItem.Amount;
                dbItem.CostForItem = uiItem.CostForItem;
            }

            _context.SaveChanges();
            RequestClose?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
