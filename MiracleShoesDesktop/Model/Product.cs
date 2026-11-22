using System;
using System.Collections.Generic;

namespace MiracleShoesDesktop.Model;

public partial class Product
{
    public int IdProduct { get; set; }

    public string Category { get; set; } = null!;

    public string? Subcategory { get; set; }

    public int? IdCompany { get; set; }

    public string NameProduct { get; set; } = null!;

    public string? Url { get; set; }

    public string Composition { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Cost { get; set; }

    public virtual ICollection<AvailableSize> AvailableSizes { get; set; } = new List<AvailableSize>();

    public virtual Company? IdCompanyNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
