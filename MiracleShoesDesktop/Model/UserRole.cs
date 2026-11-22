using System;
using System.Collections.Generic;

namespace MiracleShoesDesktop.Model;

public partial class UserRole
{
    public int IdUserRole { get; set; }

    public string NameUserRole { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
