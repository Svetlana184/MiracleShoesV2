using MiracleShoesDesktop.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiracleShoesDesktop.Services
{
    public static class StockExtensions
    {
        public static void Add(this AvailableSize a, int qty)
        {
            a.Amount += qty;
            a.IsAvailable = a.Amount > 0;
        }
        public static void Take(this AvailableSize a, int qty)
        {
            a.Amount -= qty;
            if (a.Amount < 0) a.Amount = 0;
            a.IsAvailable = a.Amount > 0;
        }
    }
}
