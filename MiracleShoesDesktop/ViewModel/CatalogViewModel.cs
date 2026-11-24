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

namespace MiracleShoesDesktop.ViewModel
{
    public partial class CatalogViewModel : ObservableObject
    {
        private readonly PostgresContext _context;

        [ObservableProperty] private ObservableCollection<ProductListItem> _products = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private string _selectedSort = string.Empty;

        public bool CanUseFilters => Session.IsAuthorized;
        public bool CanOrder => Session.IsAuthorized;

        public string[] SortOptions { get; } =
            { "По умолчанию", "Цена (возр.)", "Цена (убыв.)", "Название" };

        public CatalogViewModel()
        {
            _context = new PostgresContext();
            SelectedSort = SortOptions[0];   
        }

        partial void OnSearchTextChanged(string value) => LoadProducts();

        partial void OnSelectedSortChanged(string value) => LoadProducts();

        private void LoadProducts()
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (CanUseFilters && !string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLower();
                query = query.Where(p => p.NameProduct.ToLower().Contains(s));
            }

            if (CanUseFilters)
            {
                query = SelectedSort switch
                {
                    "Цена (возр.)" => query.OrderBy(p => p.Cost),
                    "Цена (убыв.)" => query.OrderByDescending(p => p.Cost),
                    "Название" => query.OrderBy(p => p.NameProduct),
                    _ => query
                };
            }

            var list = query.Select(p => new ProductListItem
            {
                IdProduct = p.IdProduct,
                NameProduct = p.NameProduct,
                Description = p.Description,
                Cost = p.Cost,
                Amount = p.AvailableSizes.Sum(a => (int?)a.Amount) ?? 0,
                Category = p.Category,
                Subcategory = p.Subcategory,
                CompanyName = p.IdCompanyNavigation == null ? null : p.IdCompanyNavigation.NameCompany,
                ExternalUrl = p.Url,
                Sizes = p.AvailableSizes
                    .Where(a => a.IsAvailable && a.Amount > 0)
                    .Select(a => new SizeOption
                    {
                        IdSize = a.IdSize,
                        ShoeSize = a.IdSizeNavigation.ShoeSize,
                        Amount = a.Amount
                    })
                    .ToList()
            }).ToList();

            Products = new ObservableCollection<ProductListItem>(list);
        }

        [RelayCommand(CanExecute = nameof(CanOrder))]
        private void AddToOrder(ProductListItem? item)
        {
            if (item == null) return;

            if (item.Sizes.Count == 0)
            {
                MessageBox.Show("Нет доступных размеров для заказа.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sizeDialog = new SelectSizeWindow(item.NameProduct, item.Sizes);
            if (sizeDialog.ShowDialog() != true || sizeDialog.SelectedSizeId == null)
                return;

            var sizeId = sizeDialog.SelectedSizeId.Value;
            var qty = sizeDialog.SelectedAmount;

            int? clientId = Session.CurrentUser!.IdUser;
            if (Session.IsManager || Session.IsAdmin)
            {
                var picker = new SelectUserWindow();
                if (picker.ShowDialog() != true || picker.SelectedUserId == null)
                    return;
                clientId = picker.SelectedUserId;
            }

            var avail = _context.AvailableSizes
                .FirstOrDefault(a => a.IdProduct == item.IdProduct && a.IdSize == sizeId);

            if (avail == null || avail.Amount < qty)
            {
                MessageBox.Show(avail == null ? "Нет в наличии." : $"Доступно только {avail.Amount} шт.",
                    "Заказ", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var order = _context.Orders
                .FirstOrDefault(o => o.IdUser == clientId && o.DateOrder == today);

            if (order == null)
            {
                order = new Order { IdUser = clientId, DateOrder = today };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            var existing = _context.OrderItems
                .FirstOrDefault(i => i.IdOrder == order.IdOrder
                                     && i.IdProduct == item.IdProduct
                                     && i.IdSize == sizeId);

            if (existing != null)
            {
                existing.Amount += qty;
                existing.CostForItem = item.Cost;
            }
            else
            {
                _context.OrderItems.Add(new OrderItem
                {
                    IdOrder = order.IdOrder,
                    IdProduct = item.IdProduct,
                    IdSize = sizeId,
                    Amount = qty,
                    CostForItem = item.Cost
                });
            }

            avail.Amount -= qty;
            if (avail.Amount <= 0) avail.IsAvailable = false;

            _context.SaveChanges();
            LoadProducts();

            MessageBox.Show($"Добавлено {qty} шт. (размер {sizeDialog.CurrentSize?.ShoeSize}).", "Заказ",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

