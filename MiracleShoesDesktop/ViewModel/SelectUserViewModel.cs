using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MiracleShoesDesktop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class SelectUserViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<User> _users = new();
        [ObservableProperty] private User? _selectedUser;

        public int? SelectedUserId { get; private set; }

        public event Action<bool>? RequestClose;

        public SelectUserViewModel()
        {
            using var ctx = new PostgresContext();
            Users = new ObservableCollection<User>(
                ctx.Users.AsNoTracking().OrderBy(u => u.Surname).ToList());
        }

        [RelayCommand]
        private void Ok()
        {
            if (SelectedUser == null) return;
            SelectedUserId = SelectedUser.IdUser;
            RequestClose?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
