using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiracleShoesDesktop.Model;

public partial class Order
{
    public int IdOrder { get; set; }

    public int? IdUser { get; set; }

    public DateOnly DateOrder { get; set; }

    public decimal? TotalCost { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [NotMapped]
    public decimal TotalCostComputed => OrderItems?.Sum(i => i.Cost ?? 0) ?? 0;
}
