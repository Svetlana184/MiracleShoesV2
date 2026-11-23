using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiracleShoesDesktop.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using MiracleShoesDesktop.View;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Window _window;

        [ObservableProperty] private object? _currentView;
        [ObservableProperty] private string _userInfo = string.Empty;

        public bool ShowOrdersBool => Session.IsAuthorized;

        [ObservableProperty] private string authorizeButton = Session.IsAuthorized ? "Выйти" : "Войти";

        public MainViewModel(Window window)
        {
            _window = window;

            if (Session.IsAuthorized)
            {
                var u = Session.CurrentUser!;
                var role = u.IdUserRoleNavigation?.NameUserRole ?? "?";
                UserInfo = $"Вы вошли как: {u.FullName} ({role})";
            }
            else
            {
                UserInfo = "Гостевой режим";
            }

            CurrentView = CreateCatalog();
        }

        private static CatalogView CreateCatalog() =>
            new() { DataContext = new CatalogViewModel() };

        private static OrdersView CreateOrders() =>
            new() { DataContext = new OrdersViewModel() };

        [RelayCommand] private void ShowCatalog() => CurrentView = CreateCatalog();

        [RelayCommand]
        private void ShowOrders()
        {
            if (Session.IsAuthorized) CurrentView = CreateOrders();
        }

        [RelayCommand]
        private void Logout()
        {
            Session.CurrentUser = null;
            new LoginWindow().Show();
            _window.Close();
        }
    }
}
