using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
using MiracleShoesDesktop.Services;
using MiracleShoesDesktop.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly PostgresContext _context;
        private readonly Window _loginWindow;

        [ObservableProperty] private string? _selectedLogin;

        public LoginViewModel(Window loginWindow)
        {
            _loginWindow = loginWindow;
            _context = new PostgresContext();
        }

        [RelayCommand]
        private void Login()
        {
            if (string.IsNullOrWhiteSpace(SelectedLogin))
            {
                MessageBox.Show("Введите логин.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Login == SelectedLogin);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден в системе.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Session.CurrentUser = user;
            new MainWindow().Show();
            _loginWindow.Close();
        }
    }
}
