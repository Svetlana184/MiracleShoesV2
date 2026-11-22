using System;
using System.Collections.Generic;

namespace MiracleShoesDesktop.Model;

public partial class Company
{
    public int IdCompany { get; set; }

    public string NameCompany { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
