using System;
using System.Collections.Generic;
using System.Text;

namespace MiracleShoesDesktop.Model
{
    public class ProductListItem
    {
        public int IdProduct { get; set; }
        public string NameProduct { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
        public int Amount { get; set; }
        public string Category { get; set; } = null!;
        public string? Subcategory { get; set; }
        public string? CompanyName { get; set; }
        public string? ExternalUrl { get; set; }
        public List<SizeOption> Sizes { get; set; } = new();
    }
}
