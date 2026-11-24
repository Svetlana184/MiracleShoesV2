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

        private static AvailableSize? GetAvailable(PostgresContext ctx, int productId, int? sizeId)
            => sizeId.HasValue
                ? ctx.AvailableSizes.FirstOrDefault(a => a.IdProduct == productId && a.IdSize == sizeId)
                : null;

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void DeleteOrder(Order? order)
        {
            if (order == null) return;

            if (MessageBox.Show($"Удалить заказ №{order.IdOrder}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var ctx = new PostgresContext();

            foreach (var i in ctx.OrderItems.Where(i => i.IdOrder == order.IdOrder).ToList())
                GetAvailable(ctx, i.IdProduct, i.IdSize)?.Add(i.Amount);

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
            if (new EditOrderWindow(order).ShowDialog() == true)
                LoadOrders();
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void RemoveItem(OrderItem? item)
        {
            if (item == null || !IsOwnOrder(item.IdOrder)) return;

            using var ctx = new PostgresContext();
            var entity = ctx.OrderItems.FirstOrDefault(i => i.IdOrderItem == item.IdOrderItem);
            if (entity == null) return;

            GetAvailable(ctx, entity.IdProduct, entity.IdSize)?.Add(entity.Amount);

            var orderId = entity.IdOrder;
            ctx.OrderItems.Remove(entity);

            if (ctx.OrderItems.Count(i => i.IdOrder == orderId) - 1 == 0)
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

            var avail = GetAvailable(ctx, entity.IdProduct, entity.IdSize);
            if (avail == null || avail.Amount <= 0)
            {
                MessageBox.Show("Нет в наличии.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            entity.Amount += 1;
            avail.Take(1);

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
            GetAvailable(ctx, entity.IdProduct, entity.IdSize)?.Add(1);

            ctx.SaveChanges();
            LoadOrders();
        }
    }
}
