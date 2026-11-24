using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiracleShoesDesktop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MiracleShoesDesktop.ViewModel
{
    public partial class SelectSizeViewModel : ObservableObject
    {
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private ObservableCollection<SizeOption> _sizes = new();
        [ObservableProperty] private SizeOption? _selectedSize;
        [ObservableProperty] private int _amount = 1;
        [ObservableProperty] private string _availableText = string.Empty;

        public int? SelectedSizeId { get; private set; }
        public int SelectedAmount { get; private set; }

        public event Action<bool>? RequestClose;

        public SelectSizeViewModel(string productName, System.Collections.Generic.List<SizeOption> sizes)
        {
            ProductName = productName;
            Sizes = new ObservableCollection<SizeOption>(sizes);
            SelectedSize = Sizes.FirstOrDefault();
        }

        partial void OnSelectedSizeChanged(SizeOption? value)
        {
            UpdateAvailable();
            ClampAmount();
        }

        partial void OnAmountChanged(int value) => ClampAmount(silent: true);

        private void UpdateAvailable()
        {
            AvailableText = SelectedSize != null
                ? $"Доступно: {SelectedSize.Amount}"
                : string.Empty;
        }

        private void ClampAmount(bool silent = false)
        {
            if (SelectedSize == null) return;
            if (Amount < 1) { Amount = 1; return; }
            if (Amount > SelectedSize.Amount) Amount = SelectedSize.Amount;
        }

        [RelayCommand] private void Minus() { if (Amount > 1) Amount--; }

        [RelayCommand]
        private void Plus()
        {
            if (SelectedSize != null && Amount < SelectedSize.Amount) Amount++;
        }

        [RelayCommand]
        private void Ok()
        {
            if (SelectedSize == null) return;
            if (Amount < 1 || Amount > SelectedSize.Amount) return;

            SelectedSizeId = SelectedSize.IdSize;
            SelectedAmount = Amount;
            RequestClose?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
