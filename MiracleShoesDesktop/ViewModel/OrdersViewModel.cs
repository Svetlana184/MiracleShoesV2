using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.Services;
using MiracleShoesDesktop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly PostgresContext _context;

        [ObservableProperty] private ObservableCollection<Order> _orders = new();
        [ObservableProperty] private Order? _selectedOrder;
        [ObservableProperty] private ObservableCollection<OrderItem> _selectedItems = new();

        public bool CanDelete => Session.IsManager || Session.IsAdmin;
        public bool CanEdit => Session.IsAdmin;
        public bool CanOrder => Session.IsAuthorized;

        public OrdersViewModel()
        {
            _context = new PostgresContext();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.IdUserNavigation)
                .Include(o => o.OrderItems).ThenInclude(i => i.IdProductNavigation)
                .Include(o => o.OrderItems).ThenInclude(i => i.IdSizeNavigation)
                .AsQueryable();

            if (!Session.IsManager && !Session.IsAdmin && Session.CurrentUser != null)
                query = query.Where(o => o.IdUser == Session.CurrentUser.IdUser);

            Orders = new ObservableCollection<Order>(
                query.OrderByDescending(o => o.DateOrder)
                     .ThenByDescending(o => o.IdOrder)
                     .ToList());

            if (SelectedOrder != null)
                SelectedOrder = Orders.FirstOrDefault(o => o.IdOrder == SelectedOrder.IdOrder);
        }

        partial void OnSelectedOrderChanged(Order? value)
        {
            SelectedItems = value?.OrderItems != null
                ? new ObservableCollection<OrderItem>(value.OrderItems)
                : new ObservableCollection<OrderItem>();
        }

        private bool IsOwnOrder(int orderId) =>
            Session.IsManager || Session.IsAdmin ||
            _context.Orders.Any(o => o.IdOrder == orderId && o.IdUser == Session.CurrentUser!.IdUser);

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void DeleteOrder(Order? order)
        {
            if (order == null) return;

            if (MessageBox.Show($"Удалить заказ №{order.IdOrder}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var ctx = new PostgresContext();

            // вернуть остатки на склад
            var items = ctx.OrderItems.Where(i => i.IdOrder == order.IdOrder).ToList();
            foreach (var i in items)
            {
                if (i.IdSize.HasValue)
                {
                    var avail = ctx.AvailableSizes
                        .FirstOrDefault(a => a.IdProduct == i.IdProduct && a.IdSize == i.IdSize);
                    if (avail != null)
                    {
                        avail.Amount += i.Amount;
                        avail.IsAvailable = true;
                    }
                }
            }

            var entity = ctx.Orders.FirstOrDefault(o => o.IdOrder == order.IdOrder);
            if (entity != null)
            {
                ctx.Orders.Remove(entity);
                ctx.SaveChanges();
            }

            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private void EditOrder(Order? order)
        {
            if (order == null) return;

            var win = new EditOrderWindow(order);
            if (win.ShowDialog() == true)
                LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void RemoveItem(OrderItem? item)
        {
            if (item == null || !IsOwnOrder(item.IdOrder)) return;

            using var ctx = new PostgresContext();
            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null) return;

            if (entity.IdSize.HasValue)
            {
                var avail = ctx.AvailableSizes
                    .FirstOrDefault(a => a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize);
                if (avail != null)
                {
                    avail.Amount += entity.Amount;
                    avail.IsAvailable = true;
                }
            }

            var orderId = entity.IdOrder;
            ctx.OrderItems.Remove(entity);

            var left = ctx.OrderItems.Count(i => i.IdOrder == orderId) - 1;
            if (left == 0)
            {
                var order = ctx.Orders.FirstOrDefault(o => o.IdOrder == orderId);
                if (order != null) ctx.Orders.Remove(order);
            }

            ctx.SaveChanges();
            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void IncreaseItem(OrderItem? item)
        {
            if (item == null || !IsOwnOrder(item.IdOrder)) return;

            using var ctx = new PostgresContext();
            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null) return;

            var avail = entity.IdSize.HasValue
                ? ctx.AvailableSizes.FirstOrDefault(a =>
                    a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize)
                : null;

            if (avail == null || avail.Amount <= 0)
            {
                MessageBox.Show("Нет в наличии.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            entity.Amount += 1;
            avail.Amount -= 1;
            if (avail.Amount <= 0) avail.IsAvailable = false;

            ctx.SaveChanges();
            LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void DecreaseItem(OrderItem? item)
        {
            if (item == null || !IsOwnOrder(item.IdOrder)) return;

            if (item.Amount <= 1)
            {
                RemoveItem(item);
                return;
            }

            using var ctx = new PostgresContext();
            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null) return;

            entity.Amount -= 1;

            if (entity.IdSize.HasValue)
            {
                var avail = ctx.AvailableSizes
                    .FirstOrDefault(a => a.IdProduct == entity.IdProduct && a.IdSize == entity.IdSize);
                if (avail != null)
                {
                    avail.Amount += 1;
                    avail.IsAvailable = true;
                }
            }

            ctx.SaveChanges();
            LoadOrders();
        }
    }
}
